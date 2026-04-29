<#
.SYNOPSIS
    Validates a Logic App **Standard** SCIM-onboarding orchestrator run and emits a
    detailed JSON report (Phase 7 of the SCIM gallery onboarding flow).

.DESCRIPTION
    Standard-flavored equivalent of the Consumption-only ValidateLogicAppRun.ps1
    shipped in https://github.com/AzureAD/SCIMReferenceCode/blob/master/Microsoft.SCIM.LogicAppValidationTemplate.

    Logic App Standard hosts every workflow under a single hostruntime endpoint, so
    this script:
      1. Pulls the Orchestrator run header and Final_TestResults summary.
      2. Discovers each child workflow's run id by reading the outputs of every
         Call_*_Workflow action on the Orchestrator.
      3. Pages all run actions per workflow.
      4. Bulk-fetches every action's inputsLink/outputsLink content (parallel).
      5. For Until / Foreach actions that have no direct links it pulls the LAST
         repetition and inlines that iteration's inputs/outputs (matches the
         Consumption reference script behaviour).
      6. Walks the local workflow.json definitions to build a nested
         allActionsDetailed tree (Scope / If.else / Switch.cases.<n> / default
         children are kept), placing every action's status, code, startTime,
         endTime, error, inputs and outputs under a `_details` key beside its
         children.
      7. Writes validation-result-<RunId>.json next to the script.

    Authenticates with Azure CLI. Caller needs:
      * Reader on the Logic App resource group.
      * Logic App Standard Operator (or equivalent) on the Logic App site.

.PARAMETER SubscriptionId
    Azure subscription id holding the Logic App.

.PARAMETER ResourceGroup
    Resource group of the Logic App Standard site.

.PARAMETER LogicAppName
    Name of the Logic App Standard site (the App Service name).

.PARAMETER RunId
    The Orchestrator workflow run id to validate (just the id, not the full path).

.PARAMETER DefinitionsDir
    Directory containing the workflow.json definitions
    (Orchestrator_Workflow.json, Initialization_Workflow.json,
    UserTests_Workflow.json, GroupTests_Workflow.json,
    SCIMTests_Workflow.json). Defaults to the script directory.

.PARAMETER ParametersFile
    Optional path to Orchestrator_Parameters.json — used to embed a redacted
    snapshot of the run parameters. Defaults to
    "<DefinitionsDir>/Orchestrator_Parameters.json" when present.

.PARAMETER OutDir
    Where to write the report. Defaults to the script directory.

.PARAMETER ApiVersion
    hostruntime management API version. Default 2022-03-01 (the only version
    confirmed working against current Logic App Standard regions).

.PARAMETER ThrottleLimit
    Parallel fan-out for SAS link fetches. Default 20.

.PARAMETER SkipActionDetails
    Skip the inputs/outputs link fetches. Faster run; the tree still emits
    `_details` (status / times / error) but inputs and outputs are null.

.EXAMPLE
    .\ValidateLogicAppRun-Standard.ps1 `
        -SubscriptionId  '00000000-0000-0000-0000-000000000000' `
        -ResourceGroup   'StandardLogicAppRG' `
        -LogicAppName    'ISVOnboardingTests' `
        -RunId           '08584241990693220123474542218CU00'

.EXAMPLE
    .\ValidateLogicAppRun-Standard.ps1 -SubscriptionId $sub -ResourceGroup $rg `
        -LogicAppName $app -RunId $rid -SkipActionDetails

.NOTES
    Requires PowerShell 7+ (uses ForEach-Object -Parallel). Use
    -SkipActionDetails on PowerShell 5.1, but the parallel block will still need
    PS7. Either upgrade to PS7 or run inside `pwsh`.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$SubscriptionId,
    [Parameter(Mandatory=$true)][string]$ResourceGroup,
    [Parameter(Mandatory=$true)][string]$LogicAppName,
    [Parameter(Mandatory=$true)][string]$RunId,
    [string]$DefinitionsDir,
    [string]$ParametersFile,
    [string]$OutDir,
    [string]$ApiVersion = "2022-03-01",
    [int]$ThrottleLimit = 20,
    [switch]$SkipActionDetails
)

$ErrorActionPreference = "Stop"

if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host "ERROR: This script requires PowerShell 7+. Detected $($PSVersionTable.PSVersion). Re-run inside 'pwsh'." -ForegroundColor Red
    exit 1
}

if (-not $DefinitionsDir) { $DefinitionsDir = $PSScriptRoot }
if (-not $OutDir)         { $OutDir         = $PSScriptRoot }
if (-not $ParametersFile) {
    $candidate = Join-Path $DefinitionsDir "Orchestrator_Parameters.json"
    if (Test-Path $candidate) { $ParametersFile = $candidate }
}

$Base = "https://management.azure.com/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Web/sites/$LogicAppName/hostruntime/runtime/webhooks/workflow/api/management"

Write-Host "Authenticating via Azure CLI..." -ForegroundColor Cyan
$token = (az account get-access-token --resource https://management.azure.com --query accessToken -o tsv)
if (-not $token) { throw "az account get-access-token failed. Run 'az login' and select the right tenant." }
$Headers = @{ Authorization = "Bearer $token" }

function Invoke-Mgmt([string]$Path) {
    Invoke-RestMethod -Uri "$Base$Path`?api-version=$ApiVersion" -Headers $Headers -ErrorAction Stop
}
function Invoke-Link([string]$Uri) {
    try { Invoke-RestMethod -Uri $Uri -ErrorAction Stop -TimeoutSec 30 } catch { $null }
}
function Get-AllActions([string]$Workflow, [string]$Rid) {
    $all = @()
    $next = "$Base/workflows/$Workflow/runs/$Rid/actions?api-version=$ApiVersion"
    while ($next) {
        $page = Invoke-RestMethod -Uri $next -Headers $Headers -ErrorAction Stop
        if ($page.value) { $all += $page.value }
        $next = $page.nextLink
    }
    return $all
}

# ---------- 1. Orchestrator run header ----------
Write-Host "[1/9] Orchestrator run header" -ForegroundColor Cyan
$run = Invoke-Mgmt "/workflows/Orchestrator_Workflow/runs/$RunId"
$runStatus = $run.properties.status
$startTime = $run.properties.startTime
$endTime   = $run.properties.endTime
$dur = if ($endTime) { New-TimeSpan -Start $startTime -End $endTime } else { $null }
$durStr = if ($dur) { "{0}h {1}m {2}s" -f $dur.Hours, $dur.Minutes, $dur.Seconds } else { "N/A" }
Write-Host "    status=$runStatus duration=$durStr"

# ---------- 2. Load workflow definitions from disk ----------
Write-Host "[2/9] Load workflow definitions from $DefinitionsDir" -ForegroundColor Cyan
$wfDefs = [ordered]@{}
$wfFiles = Get-ChildItem -Path $DefinitionsDir -Filter "*_Workflow.json" -File -ErrorAction Stop
if (-not $wfFiles -or $wfFiles.Count -eq 0) {
    throw "No *_Workflow.json files found in $DefinitionsDir."
}
foreach ($f in $wfFiles) {
    $name = $f.BaseName
    try {
        $j = Get-Content $f.FullName -Raw | ConvertFrom-Json
        $def = if ($j.definition) { $j.definition } else { $j }
        $wfDefs[$name] = $def
        Write-Host ("    {0,-30} {1} root actions" -f $name, @($def.actions.PSObject.Properties.Name).Count)
    } catch {
        Write-Host "    !! $name parse failed: $_" -ForegroundColor Yellow
    }
}
if (-not $wfDefs.Contains('Orchestrator_Workflow')) {
    throw "Orchestrator_Workflow.json missing from $DefinitionsDir."
}

# ---------- 3. Final_TestResults body ----------
Write-Host "[3/9] Read Final_TestResults" -ForegroundColor Cyan
$ftr = $null; $testResults = @(); $childLinks = $null; $overallResult = $null
try {
    $ftrAction = Invoke-Mgmt "/workflows/Orchestrator_Workflow/runs/$RunId/actions/Final_TestResults"
    $ftr = Invoke-Link $ftrAction.properties.outputsLink.uri
    if ($ftr) {
        $testResults  = @($ftr.testResults)
        $childLinks   = $ftr.childWorkflowRunLinks
        $overallResult = $ftr.overallResult
        Write-Host "    overallResult=$overallResult tests=$($testResults.Count)"
    } else {
        Write-Host "    Final_TestResults outputs not available" -ForegroundColor Yellow
    }
} catch {
    Write-Host "    Final_TestResults action not found ($_)" -ForegroundColor Yellow
}

# ---------- 4. Discover child workflows from the Orchestrator definition ----------
Write-Host "[4/9] Discover child workflow run ids" -ForegroundColor Cyan
$childRuns = [ordered]@{ Orchestrator_Workflow = $RunId }
$callMap = @{}
function Find-WorkflowCalls {
    param($Actions, [hashtable]$Map)
    if (-not $Actions) { return }
    foreach ($p in $Actions.PSObject.Properties) {
        $a = $p.Value
        if ($a.type -eq 'Workflow' -and $a.inputs.host.workflow.id) {
            $wfRefName = Split-Path $a.inputs.host.workflow.id -Leaf
            $Map[$wfRefName] = $p.Name
        }
        if ($a.actions)        { Find-WorkflowCalls -Actions $a.actions        -Map $Map }
        if ($a.else.actions)   { Find-WorkflowCalls -Actions $a.else.actions   -Map $Map }
        if ($a.default.actions){ Find-WorkflowCalls -Actions $a.default.actions -Map $Map }
        if ($a.cases) {
            foreach ($c in $a.cases.PSObject.Properties) {
                if ($c.Value.actions) { Find-WorkflowCalls -Actions $c.Value.actions -Map $Map }
            }
        }
    }
}
Find-WorkflowCalls -Actions $wfDefs['Orchestrator_Workflow'].actions -Map $callMap

$orchActionsList = Get-AllActions 'Orchestrator_Workflow' $RunId
foreach ($wfName in $callMap.Keys) {
    $callName = $callMap[$wfName]
    if (-not ($orchActionsList | Where-Object name -eq $callName)) { continue }
    try {
        $det = Invoke-Mgmt "/workflows/Orchestrator_Workflow/runs/$RunId/actions/$callName"
        $out = Invoke-Link $det.properties.outputsLink.uri
        $crid = $out.headers.'x-ms-workflow-run-id'
        if ($crid) { $childRuns[$wfName] = $crid; Write-Host "    $wfName -> $crid" }
        else       { Write-Host "    $wfName : no run id in outputs (status=$($det.properties.status))" -ForegroundColor Yellow }
    } catch {
        Write-Host "    $wfName : lookup failed ($_)" -ForegroundColor Yellow
    }
}

# ---------- 5. Page all run actions per workflow ----------
Write-Host "[5/9] Fetch run actions per workflow" -ForegroundColor Cyan
$wfActions = [ordered]@{}
foreach ($wf in $childRuns.Keys) {
    try {
        $wfActions[$wf] = Get-AllActions $wf $childRuns[$wf]
        Write-Host ("    {0,-30} {1} actions" -f $wf, $wfActions[$wf].Count)
    } catch {
        Write-Host "    !! $wf actions fetch failed: $_" -ForegroundColor Yellow
        $wfActions[$wf] = @()
    }
}

# ---------- 6. Fetch inputsLink/outputsLink content ----------
$contentMap = @{}
$failedFetches = 0
if (-not $SkipActionDetails) {
    Write-Host "[6/9] Fetch inputs/outputs (parallel, throttle=$ThrottleLimit)" -ForegroundColor Cyan
    $jobItems = New-Object System.Collections.ArrayList
    foreach ($wf in $wfActions.Keys) {
        foreach ($a in $wfActions[$wf]) {
            if ($a.properties.inputsLink)  { [void]$jobItems.Add(@{ wf=$wf; name=$a.name; kind='inputs';  uri=$a.properties.inputsLink.uri }) }
            if ($a.properties.outputsLink) { [void]$jobItems.Add(@{ wf=$wf; name=$a.name; kind='outputs'; uri=$a.properties.outputsLink.uri }) }
        }
    }
    Write-Host "    $($jobItems.Count) link fetches queued"

    $results = $jobItems | ForEach-Object -Parallel {
        $item = $_
        try {
            $r = Invoke-RestMethod -Uri $item.uri -ErrorAction Stop -TimeoutSec 30
            return @{ wf=$item.wf; name=$item.name; kind=$item.kind; data=$r; ok=$true }
        } catch {
            return @{ wf=$item.wf; name=$item.name; kind=$item.kind; data=$null; ok=$false; err=$_.Exception.Message }
        }
    } -ThrottleLimit $ThrottleLimit

    foreach ($r in $results) {
        if (-not $contentMap.ContainsKey($r.wf)) { $contentMap[$r.wf] = @{} }
        if (-not $contentMap[$r.wf].ContainsKey($r.name)) { $contentMap[$r.wf][$r.name] = @{} }
        $contentMap[$r.wf][$r.name][$r.kind] = $r.data
        if (-not $r.ok) { $failedFetches++ }
    }
    Write-Host "    fetched (failed: $failedFetches)"

    # ---------- 6b. Repetitions fallback for Until / Foreach ----------
    Write-Host "[6b/9] Repetitions fallback (Until / Foreach final iteration)" -ForegroundColor Cyan
    $repItems = New-Object System.Collections.ArrayList
    foreach ($wf in $wfActions.Keys) {
        foreach ($a in $wfActions[$wf]) {
            if ($a.properties.inputsLink -or $a.properties.outputsLink) { continue }
            if ($a.properties.status -notin @('Succeeded','Failed')) { continue }
            if (-not $a.properties.repetitionCount -or $a.properties.repetitionCount -le 0) { continue }
            [void]$repItems.Add(@{ wf=$wf; name=$a.name; rid=$childRuns[$wf] })
        }
    }
    Write-Host "    $($repItems.Count) actions need repetitions lookup"

    if ($repItems.Count -gt 0) {
        $repResults = $repItems | ForEach-Object -Parallel {
            $item = $_
            $base = $using:Base; $api = $using:ApiVersion; $h = $using:Headers
            $out = @{ wf=$item.wf; name=$item.name; inputs=$null; outputs=$null; ok=$false }
            try {
                $list = Invoke-RestMethod -Uri "$base/workflows/$($item.wf)/runs/$($item.rid)/actions/$($item.name)/repetitions?api-version=$api" -Headers $h -TimeoutSec 30
                if ($list.value -and $list.value.Count -gt 0) {
                    $last = $list.value | Sort-Object { $_.properties.startTime } | Select-Object -Last 1
                    $detail = Invoke-RestMethod -Uri "$base/workflows/$($item.wf)/runs/$($item.rid)/actions/$($item.name)/repetitions/$($last.name)?api-version=$api" -Headers $h -TimeoutSec 30
                    if ($detail.properties.inputsLink.uri) {
                        try { $out.inputs = Invoke-RestMethod -Uri $detail.properties.inputsLink.uri -TimeoutSec 30 } catch {}
                    }
                    if ($detail.properties.outputsLink.uri) {
                        try { $out.outputs = Invoke-RestMethod -Uri $detail.properties.outputsLink.uri -TimeoutSec 30 } catch {}
                    }
                    $out.ok = $true
                }
            } catch {}
            return $out
        } -ThrottleLimit ([Math]::Min($ThrottleLimit, 10))

        $repFilled = 0
        foreach ($r in $repResults) {
            if (-not $r.ok) { continue }
            if (-not $contentMap.ContainsKey($r.wf)) { $contentMap[$r.wf] = @{} }
            if (-not $contentMap[$r.wf].ContainsKey($r.name)) { $contentMap[$r.wf][$r.name] = @{} }
            if ($r.inputs  -ne $null) { $contentMap[$r.wf][$r.name]['inputs']  = $r.inputs }
            if ($r.outputs -ne $null) { $contentMap[$r.wf][$r.name]['outputs'] = $r.outputs }
            if ($r.inputs -ne $null -or $r.outputs -ne $null) { $repFilled++ }
        }
        Write-Host "    filled $repFilled actions from last repetition"
    }
} else {
    Write-Host "[6/9] SkipActionDetails set — inputs/outputs will be null" -ForegroundColor Yellow
}

# ---------- 7. Build nested allActionsDetailed tree ----------
Write-Host "[7/9] Build nested action tree" -ForegroundColor Cyan
function New-ActionDetails($wf, $name, $runAction) {
    $det = [ordered]@{
        status     = if ($runAction) { $runAction.properties.status } else { "NotExecuted" }
        code       = if ($runAction) { $runAction.properties.code } else { $null }
        startTime  = if ($runAction) { $runAction.properties.startTime } else { $null }
        endTime    = if ($runAction) { $runAction.properties.endTime }   else { $null }
        error      = if ($runAction) { $runAction.properties.error }     else { $null }
        inputs     = $null
        outputs    = $null
    }
    if ($contentMap.ContainsKey($wf) -and $contentMap[$wf].ContainsKey($name)) {
        if ($contentMap[$wf][$name].ContainsKey('inputs'))  { $det.inputs  = $contentMap[$wf][$name].inputs }
        if ($contentMap[$wf][$name].ContainsKey('outputs')) { $det.outputs = $contentMap[$wf][$name].outputs }
    }
    return $det
}
function Build-Tree($wf, $defActions, $runIndex) {
    if (-not $defActions) { return $null }
    $tree = [ordered]@{}
    foreach ($prop in $defActions.PSObject.Properties) {
        $aname = $prop.Name
        $adef  = $prop.Value
        $node  = [ordered]@{}
        $node['_details'] = New-ActionDetails $wf $aname $runIndex[$aname]
        if ($adef.actions) {
            $sub = Build-Tree $wf $adef.actions $runIndex
            if ($sub) { foreach ($k in $sub.Keys) { $node[$k] = $sub[$k] } }
        }
        if ($adef.else -and $adef.else.actions) {
            $sub = Build-Tree $wf $adef.else.actions $runIndex
            if ($sub) { foreach ($k in $sub.Keys) { $node["__else_$k"] = $sub[$k] } }
        }
        if ($adef.default -and $adef.default.actions) {
            $sub = Build-Tree $wf $adef.default.actions $runIndex
            if ($sub) { foreach ($k in $sub.Keys) { $node["__default_$k"] = $sub[$k] } }
        }
        if ($adef.cases) {
            foreach ($cp in $adef.cases.PSObject.Properties) {
                if ($cp.Value.actions) {
                    $sub = Build-Tree $wf $cp.Value.actions $runIndex
                    if ($sub) { foreach ($k in $sub.Keys) { $node["__case_$($cp.Name)_$k"] = $sub[$k] } }
                }
            }
        }
        $tree[$aname] = $node
    }
    return $tree
}
$allDetailed = [ordered]@{}
foreach ($wf in $wfDefs.Keys) {
    if (-not $wfActions.Contains($wf)) { continue }
    $idx = @{}
    foreach ($a in $wfActions[$wf]) { $idx[$a.name] = $a }
    $tree = Build-Tree $wf $wfDefs[$wf].actions $idx
    $allDetailed[$wf] = $tree
    $rootCnt = if ($tree) { $tree.Keys.Count } else { 0 }
    Write-Host ("    {0,-30} {1} root nodes" -f $wf, $rootCnt)
}

# ---------- 8. Aggregates ----------
Write-Host "[8/9] Compute aggregates" -ForegroundColor Cyan
$allFlat = @()
foreach ($wf in $wfActions.Keys) { $allFlat += $wfActions[$wf] }
$summary = [ordered]@{
    total     = $allFlat.Count
    succeeded = ($allFlat | Where-Object { $_.properties.status -eq "Succeeded" }).Count
    failed    = ($allFlat | Where-Object { $_.properties.status -eq "Failed" }).Count
    skipped   = ($allFlat | Where-Object { $_.properties.status -eq "Skipped" }).Count
    other     = ($allFlat | Where-Object { $_.properties.status -notin @("Succeeded","Failed","Skipped") }).Count
}
$failedActions = New-Object System.Collections.ArrayList
foreach ($wf in $wfActions.Keys) {
    foreach ($a in $wfActions[$wf]) {
        if ($a.properties.status -ne "Failed") { continue }
        $msg  = $a.properties.error.message
        $code = $a.properties.error.code
        if (-not $msg -and $contentMap.ContainsKey($wf) -and $contentMap[$wf].ContainsKey($a.name)) {
            $body = $contentMap[$wf][$a.name].outputs
            if ($body.body.error.message) { $msg = $body.body.error.message }
            elseif ($body.body.message)   { $msg = $body.body.message }
            elseif ($body.error.message)  { $msg = $body.error.message }
        }
        [void]$failedActions.Add([PSCustomObject]@{
            name=$a.name; workflow=$wf; runId=$childRuns[$wf]; status="Failed"
            errorCode=$code; errorMessage=$msg
            startTime=$a.properties.startTime; endTime=$a.properties.endTime
        })
    }
}
$testSummary = [ordered]@{
    total   = $testResults.Count
    success = ($testResults | Where-Object { $_.testResult -eq "success" }).Count
    skipped = ($testResults | Where-Object { $_.testResult -eq "SKIPPED" }).Count
    failed  = ($testResults | Where-Object { $_.testResult -ne "success" -and $_.testResult -ne "SKIPPED" }).Count
}

$tplRoot         = $wfDefs['Orchestrator_Workflow'].actions
$tplActionNames  = if ($tplRoot) { @($tplRoot.PSObject.Properties.Name) } else { @() }
$reqStages = foreach ($n in $tplActionNames) {
    $exec = $orchActionsList | Where-Object name -eq $n | Select-Object -First 1
    [PSCustomObject]@{
        stage    = ($n -replace '_',' ')
        action   = $n
        executed = [bool]$exec
        status   = if ($exec) { $exec.properties.status } else { "NotExecuted" }
    }
}
$missing = @($tplActionNames | Where-Object { $n=$_; -not ($orchActionsList | Where-Object name -eq $n) })

# ---------- 9. Parameters (redacted) + write report ----------
Write-Host "[9/9] Parameters + emit report" -ForegroundColor Cyan
$params = $null
if ($ParametersFile -and (Test-Path $ParametersFile)) {
    $raw = Get-Content $ParametersFile -Raw | ConvertFrom-Json
    $r = [ordered]@{}
    foreach ($p in $raw.PSObject.Properties) {
        $v = $p.Value.value
        if ($p.Name -imatch 'token|secret|credential|password|key' -and $v) { $v = "***" }
        elseif ($p.Name -ieq 'defaultUserProperties' -and $v -is [System.Collections.IEnumerable]) {
            $redacted = @()
            foreach ($u in $v) {
                $j = ($u | ConvertTo-Json -Depth 20 -Compress) -replace '"password"\s*:\s*"[^"]*"','"password":"***"'
                $redacted += ($j | ConvertFrom-Json)
            }
            $v = $redacted
        }
        $r[$p.Name] = $v
    }
    $params = $r
}

$noFailedActions = $summary.failed -eq 0
$noFailedTests   = $testSummary.failed -eq 0
$tplValid        = $tplActionNames.Count -gt 0
$reqExec         = -not ($reqStages | Where-Object { -not $_.executed })
$allTplExec      = $missing.Count -eq 0
$allChecks       = $noFailedActions -and $noFailedTests -and $tplValid -and $reqExec -and $allTplExec
$result          = if ($allChecks -and $runStatus -eq "Succeeded") { "PASSED" } else { "FAILED" }

$report = [ordered]@{
    validationResult       = $result
    runStatus              = $runStatus
    overallResultFromTests = $overallResult
    validationChecks       = [ordered]@{
        noFailedActions            = $noFailedActions
        noFailedTests              = $noFailedTests
        templateStructureValid     = $tplValid
        requiredStagesExecuted     = $reqExec
        allTemplateActionsExecuted = $allTplExec
    }
    timestamp        = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    runId            = $RunId
    logicAppName     = $LogicAppName
    resourceGroup    = $ResourceGroup
    subscriptionId   = $SubscriptionId
    startTime        = $startTime
    endTime          = $endTime
    duration         = $durStr
    parameters       = $params
    actionSummary    = $summary
    testSummary      = $testSummary
    testResults      = $testResults
    failedActions    = $failedActions
    templateValidation = [ordered]@{
        valid          = $allTplExec -and $reqExec
        requiredStages = $reqStages
        errors         = @()
    }
    actionComparison = [ordered]@{
        valid               = $allTplExec
        missingFromRunCount = $missing.Count
        missingActions      = if ($missing.Count -gt 0) { $missing } else { $null }
    }
    childWorkflowRuns      = $childRuns
    childWorkflowRunLinks  = $childLinks
    allActionsDetailed     = $allDetailed
}

$outFile = Join-Path $OutDir "validation-result-$RunId.json"
$report | ConvertTo-Json -Depth 100 | Out-File -FilePath $outFile -Encoding utf8

Write-Host ""
Write-Host "============================================" -ForegroundColor White
Write-Host "Report: $outFile" -ForegroundColor Green
Write-Host ("Size:   {0} KB" -f [math]::Round((Get-Item $outFile).Length/1KB,1))
Write-Host ("Validation: {0}" -f $result) -ForegroundColor $(if ($result -eq "PASSED") { "Green" } else { "Red" })
Write-Host ("Run status: {0} | Tests: {1}/{2} success, {3} failed, {4} skipped" -f $runStatus,$testSummary.success,$testSummary.total,$testSummary.failed,$testSummary.skipped)
Write-Host ("Actions: {0} total, {1} ok, {2} failed, {3} skipped" -f $summary.total,$summary.succeeded,$summary.failed,$summary.skipped)
Write-Host ("Workflows in tree: {0}" -f $allDetailed.Keys.Count)
Write-Host "============================================" -ForegroundColor White

exit $(if ($result -eq "PASSED") { 0 } else { 1 })
