<#
.SYNOPSIS
    Deploys Logic App Standard workflows from local JSON files to an Azure Logic App.

.DESCRIPTION
    This script reads *_Workflow.json files from a specified directory and creates/updates
    workflows in an Azure Logic App Standard resource using the Azure REST API (via az CLI).
    It also optionally deploys a parameters.json file for workflow parameters.

    The script performs pre-deployment validation of all files before making any API calls,
    ensuring no partial deployments occur if any file is invalid.

.PARAMETER SubscriptionId
    The Azure subscription ID where the Logic App resides.

.PARAMETER ResourceGroup
    The Azure resource group containing the Logic App.

.PARAMETER LogicAppName
    The name of the Azure Logic App Standard resource.

.PARAMETER WorkflowsPath
    Path to the directory containing *_Workflow.json files. Defaults to the current directory ('.').

.PARAMETER ParametersFile
    (Optional) Path to a parameters JSON file to deploy as the app-level parameters.json.
    If not specified, the script will look for *_Parameters.json in the WorkflowsPath.

.EXAMPLE
    .\Deploy-LogicAppWorkflows.ps1 -SubscriptionId "xxxx" -ResourceGroup "myRG" -LogicAppName "myLogicApp"

.EXAMPLE
    .\Deploy-LogicAppWorkflows.ps1 -SubscriptionId "xxxx" -ResourceGroup "myRG" -LogicAppName "myLogicApp" -WorkflowsPath ".\ProvisioningTesting"

.EXAMPLE
    .\Deploy-LogicAppWorkflows.ps1 -SubscriptionId "xxxx" -ResourceGroup "myRG" -LogicAppName "myLogicApp" -SkipParameters
    Deploys only workflow files without updating the parameters.json.

.NOTES
    Prerequisites:
    - Azure CLI (az) must be installed and logged in (run 'az login' first)
    - The Logic App Standard resource must already exist in Azure
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, HelpMessage = "Azure Subscription ID")]
    [string]$SubscriptionId,

    [Parameter(Mandatory = $true, HelpMessage = "Azure Resource Group name")]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $true, HelpMessage = "Logic App Standard resource name")]
    [string]$LogicAppName,

    [Parameter(Mandatory = $false, HelpMessage = "Path to directory containing *_Workflow.json files")]
    [string]$WorkflowsPath = ".",

    [Parameter(Mandatory = $false, HelpMessage = "Path to parameters JSON file")]
    [string]$ParametersFile = "",

    [Parameter(Mandatory = $false, HelpMessage = "Skip parameters deployment and update only workflows")]
    [switch]$SkipParameters
)

# ============================================================================
# Configuration
# ============================================================================
$ErrorActionPreference = "Stop"
$script:ManagementApiBase = "https://management.azure.com"
$script:ApiVersion = "2018-11-01"

# ============================================================================
# Helper Functions
# ============================================================================

function Write-LogMessage {
    param(
        [string]$Message,
        [ValidateSet("Info", "Success", "Warning", "Error")]
        [string]$Level = "Info"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    switch ($Level) {
        "Info"    { Write-Host "[$timestamp] INFO    : $Message" -ForegroundColor Cyan }
        "Success" { Write-Host "[$timestamp] SUCCESS : $Message" -ForegroundColor Green }
        "Warning" { Write-Host "[$timestamp] WARNING : $Message" -ForegroundColor Yellow }
        "Error"   { Write-Host "[$timestamp] ERROR   : $Message" -ForegroundColor Red }
    }
}

function Test-AzCliLogin {
    <#
    .SYNOPSIS
        Verifies that the Azure CLI is installed and the user is logged in.
    #>
    try {
        $account = az account show 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-LogMessage "Azure CLI is not logged in. Please run 'az login' first." -Level Error
            return $false
        }
        $accountObj = $account | ConvertFrom-Json
        Write-LogMessage "Logged in as: $($accountObj.user.name) (Tenant: $($accountObj.tenantId))" -Level Info
        return $true
    }
    catch {
        Write-LogMessage "Azure CLI (az) is not installed or not accessible. Please install it from https://aka.ms/installazurecli" -Level Error
        return $false
    }
}

function Set-AzSubscription {
    <#
    .SYNOPSIS
        Sets the active Azure subscription.
    #>
    param([string]$SubId)
    
    Write-LogMessage "Setting subscription to: $SubId" -Level Info
    $result = az account set --subscription $SubId 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-LogMessage "Failed to set subscription: $result" -Level Error
        return $false
    }
    Write-LogMessage "Subscription set successfully." -Level Success
    return $true
}

function Test-LogicAppExists {
    <#
    .SYNOPSIS
        Verifies that the Logic App Standard resource exists.
    #>
    param(
        [string]$SubId,
        [string]$RG,
        [string]$AppName
    )

    $uri = "$script:ManagementApiBase/subscriptions/$SubId/resourceGroups/$RG/providers/Microsoft.Web/sites/${AppName}?api-version=2022-03-01"
    
    try {
        $result = az rest --method GET --uri $uri 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-LogMessage "Logic App '$AppName' not found in resource group '$RG'. Please ensure it exists." -Level Error
            return $false
        }
        $appObj = $result | ConvertFrom-Json
        Write-LogMessage "Found Logic App: $($appObj.name) (Location: $($appObj.location))" -Level Success
        return $true
    }
    catch {
        Write-LogMessage "Error checking Logic App existence: $_" -Level Error
        return $false
    }
}

function Get-WorkflowFiles {
    <#
    .SYNOPSIS
        Discovers all *_Workflow.json files in the specified directory.
    #>
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        Write-LogMessage "Workflows path not found: $Path" -Level Error
        return @()
    }

    $files = Get-ChildItem -Path $Path -Filter "*_Workflow.json" -File
    if ($files.Count -eq 0) {
        Write-LogMessage "No *_Workflow.json files found in: $Path" -Level Warning
    }
    else {
        Write-LogMessage "Found $($files.Count) workflow file(s) in: $Path" -Level Info
    }

    return $files
}

function Get-ParametersFiles {
    <#
    .SYNOPSIS
        Discovers *_Parameters.json files in the specified directory.
    #>
    param([string]$Path)

    $files = Get-ChildItem -Path $Path -Filter "*_Parameters.json" -File -ErrorAction SilentlyContinue
    return $files
}

function Get-WorkflowNameFromFile {
    <#
    .SYNOPSIS
        Extracts the workflow name from a filename (e.g., Orchestrator_Workflow.json -> Orchestrator_Workflow).
    #>
    param([string]$FileName)
    return [System.IO.Path]::GetFileNameWithoutExtension($FileName)
}

function Test-WorkflowFile {
    <#
    .SYNOPSIS
        Validates a single workflow JSON file before deployment.
        Checks that the file is valid JSON and contains a 'definition' property.
    #>
    param(
        [string]$FilePath,
        [string]$FileName
    )

    $errors = @()

    try {
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        $obj = $content | ConvertFrom-Json

        if (-not $obj.definition) {
            $errors += "Missing 'definition' property"
        }
    }
    catch {
        $errors += "Invalid JSON: $_"
    }

    return $errors
}

function Test-ParametersFile {
    <#
    .SYNOPSIS
        Validates a parameters JSON file before deployment.
        Checks that the file is valid JSON and is not empty.
    #>
    param(
        [string]$FilePath,
        [string]$FileName
    )

    $errors = @()

    try {
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        $obj = $content | ConvertFrom-Json

        if ($null -eq $obj) {
            $errors += "Parameters file is empty or null"
        }
    }
    catch {
        $errors += "Invalid JSON: $_"
    }

    return $errors
}

function Deploy-VfsFile {
    <#
    .SYNOPSIS
        Uploads a file to the Logic App file system via ARM-proxied Kudu VFS API.
        Uses Invoke-RestMethod with a bearer token to send raw file content without
        any re-serialization (avoids az rest JSON body parsing that can corrupt
        Logic Apps expression strings like @parameters(), @triggerBody(), etc.).
    #>
    param(
        [string]$SubId,
        [string]$RG,
        [string]$AppName,
        [string]$RemotePath,
        [string]$LocalFilePath
    )

    # ARM-proxied VFS endpoint — routes through management.azure.com instead of SCM directly
    $vfsUri = "$script:ManagementApiBase/subscriptions/$SubId/resourceGroups/$RG/providers/Microsoft.Web/sites/$AppName/extensions/api/vfs/site/wwwroot/${RemotePath}?api-version=2022-03-01"

    Write-LogMessage "  Uploading to ARM VFS: $RemotePath" -Level Info

    try {
        # Get access token from Azure CLI (reuses existing az login session)
        $token = az account get-access-token --query accessToken -o tsv 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to get access token: $($token | Out-String)"
        }

        # Read the raw file content — preserves exact bytes including Logic Apps expressions
        $bodyContent = Get-Content -Path $LocalFilePath -Raw -Encoding UTF8

        # Use Invoke-RestMethod to send raw content without JSON re-serialization
        $headers = @{
            "Authorization" = "Bearer $token"
            "Content-Type"  = "application/json"
            "If-Match"      = "*"
        }

        $result = Invoke-RestMethod -Method PUT -Uri $vfsUri -Headers $headers -Body $bodyContent -ContentType "application/json"

        return $true
    }
    catch {
        $errorMsg = $_.Exception.Message
        throw "VFS upload failed: $errorMsg"
    }
}

function Deploy-Workflow {
    <#
    .SYNOPSIS
        Deploys a single workflow to the Logic App via ARM-proxied Kudu VFS API.
        Uploads the workflow JSON as wwwroot/{WorkflowName}/workflow.json.
    #>
    param(
        [string]$SubId,
        [string]$RG,
        [string]$AppName,
        [string]$WorkflowName,
        [string]$WorkflowJsonPath
    )

    Write-LogMessage "Deploying workflow: $WorkflowName" -Level Info
    Write-LogMessage "  Source file: $WorkflowJsonPath" -Level Info

    # Read and validate the workflow JSON
    try {
        $workflowContent = Get-Content -Path $WorkflowJsonPath -Raw -Encoding UTF8
        $workflowObj = $workflowContent | ConvertFrom-Json

        # Validate required structure
        if (-not $workflowObj.definition) {
            Write-LogMessage "  Workflow file is missing 'definition' property: $WorkflowJsonPath" -Level Error
            return @{ WorkflowName = $WorkflowName; Status = "FAILED"; Error = "Missing 'definition' property" }
        }
    }
    catch {
        Write-LogMessage "  Failed to read/parse workflow file: $_" -Level Error
        return @{ WorkflowName = $WorkflowName; Status = "FAILED"; Error = "JSON parse error: $_" }
    }

    try {
        # Upload workflow.json to {WorkflowName}/workflow.json via ARM VFS
        Deploy-VfsFile -SubId $SubId -RG $RG -AppName $AppName `
            -RemotePath "${WorkflowName}/workflow.json" `
            -LocalFilePath $WorkflowJsonPath

        Write-LogMessage "  Workflow '$WorkflowName' deployed successfully." -Level Success
        return @{ WorkflowName = $WorkflowName; Status = "SUCCESS"; Error = "" }
    }
    catch {
        Write-LogMessage "  Error deploying workflow '$WorkflowName': $_" -Level Error
        return @{ WorkflowName = $WorkflowName; Status = "FAILED"; Error = "$_" }
    }
}

function Deploy-Parameters {
    <#
    .SYNOPSIS
        Deploys the parameters.json file to the Logic App root via ARM-proxied Kudu VFS API.
    #>
    param(
        [string]$SubId,
        [string]$RG,
        [string]$AppName,
        [string]$ParametersFilePath
    )

    Write-LogMessage "Deploying parameters file: $ParametersFilePath" -Level Info

    try {
        $paramsContent = Get-Content -Path $ParametersFilePath -Raw -Encoding UTF8
        $paramsObj = $paramsContent | ConvertFrom-Json

        # Validate it's a valid JSON object
        if ($null -eq $paramsObj) {
            Write-LogMessage "  Parameters file is empty or invalid." -Level Error
            return $false
        }
    }
    catch {
        Write-LogMessage "  Failed to read/parse parameters file: $_" -Level Error
        return $false
    }

    try {
        # Upload parameters.json to root via ARM VFS
        Deploy-VfsFile -SubId $SubId -RG $RG -AppName $AppName `
            -RemotePath "parameters.json" `
            -LocalFilePath $ParametersFilePath

        Write-LogMessage "  Parameters file deployed successfully." -Level Success
        return $true
    }
    catch {
        Write-LogMessage "  Failed to deploy parameters file: $_" -Level Error
        return $false
    }
}

# ============================================================================
# Main Execution
# ============================================================================

Write-Host ""
Write-Host "============================================================" -ForegroundColor White
Write-Host "  Azure Logic App Standard - Workflow Deployment Script" -ForegroundColor White
Write-Host "============================================================" -ForegroundColor White
Write-Host ""

# Resolve the workflows path to an absolute path
$WorkflowsPath = Resolve-Path -Path $WorkflowsPath -ErrorAction SilentlyContinue
if (-not $WorkflowsPath) {
    Write-LogMessage "Workflows path does not exist: $WorkflowsPath" -Level Error
    exit 1
}

Write-LogMessage "Configuration:" -Level Info
Write-Host "  Subscription ID : $SubscriptionId"
Write-Host "  Resource Group  : $ResourceGroup"
Write-Host "  Logic App Name  : $LogicAppName"
Write-Host "  Workflows Path  : $WorkflowsPath"
Write-Host "  Parameters File : $(if ($SkipParameters) { '(skipped)' } elseif ($ParametersFile) { $ParametersFile } else { '(auto-detect)' })"
Write-Host "  Skip Parameters : $SkipParameters"
Write-Host ""

# --- Step 1: Validate Azure CLI login ---
Write-LogMessage "Step 1: Validating Azure CLI login..." -Level Info
if (-not (Test-AzCliLogin)) {
    exit 1
}
Write-Host ""

# --- Step 2: Set subscription ---
Write-LogMessage "Step 2: Setting Azure subscription..." -Level Info
if (-not (Set-AzSubscription -SubId $SubscriptionId)) {
    exit 1
}
Write-Host ""

# --- Step 3: Verify Logic App exists ---
Write-LogMessage "Step 3: Verifying Logic App exists..." -Level Info
if (-not (Test-LogicAppExists -SubId $SubscriptionId -RG $ResourceGroup -AppName $LogicAppName)) {
    exit 1
}
Write-Host ""

# --- Step 4: Discover workflow files ---
Write-LogMessage "Step 4: Discovering workflow files..." -Level Info
$workflowFiles = Get-WorkflowFiles -Path $WorkflowsPath
if ($workflowFiles.Count -eq 0) {
    Write-LogMessage "No workflow files found. Nothing to deploy." -Level Warning
    exit 0
}

Write-Host ""
Write-LogMessage "Workflows to deploy:" -Level Info
foreach ($wf in $workflowFiles) {
    $wfName = Get-WorkflowNameFromFile -FileName $wf.Name
    Write-Host "  - $wfName ($($wf.Name))"
}
Write-Host ""

# --- Step 5: Pre-deployment validation of all files ---
Write-LogMessage "Step 5: Validating all workflow and parameter files before deployment..." -Level Info
$validationFailed = $false

foreach ($wfFile in $workflowFiles) {
    $wfName = Get-WorkflowNameFromFile -FileName $wfFile.Name
    $errors = Test-WorkflowFile -FilePath $wfFile.FullName -FileName $wfFile.Name
    if ($errors.Count -gt 0) {
        $validationFailed = $true
        foreach ($err in $errors) {
            Write-LogMessage "  INVALID: $($wfFile.Name) - $err" -Level Error
        }
    }
    else {
        Write-LogMessage "  VALID: $($wfFile.Name)" -Level Success
    }
}

# Also validate parameters file(s) if applicable (skip when -SkipParameters is set)
if ($SkipParameters) {
    Write-LogMessage "  Skipping parameters file validation (SkipParameters flag set)." -Level Info
}
elseif ($ParametersFile -and (Test-Path $ParametersFile)) {
    $pErrors = Test-ParametersFile -FilePath $ParametersFile -FileName (Split-Path $ParametersFile -Leaf)
    if ($pErrors.Count -gt 0) {
        $validationFailed = $true
        foreach ($err in $pErrors) {
            Write-LogMessage "  INVALID: $(Split-Path $ParametersFile -Leaf) - $err" -Level Error
        }
    }
    else {
        Write-LogMessage "  VALID: $(Split-Path $ParametersFile -Leaf)" -Level Success
    }
}
else {
    $autoParamFiles = Get-ParametersFiles -Path $WorkflowsPath
    foreach ($pf in $autoParamFiles) {
        $pErrors = Test-ParametersFile -FilePath $pf.FullName -FileName $pf.Name
        if ($pErrors.Count -gt 0) {
            $validationFailed = $true
            foreach ($err in $pErrors) {
                Write-LogMessage "  INVALID: $($pf.Name) - $err" -Level Error
            }
        }
        else {
            Write-LogMessage "  VALID: $($pf.Name)" -Level Success
        }
    }
}

if ($validationFailed) {
    Write-Host ""
    Write-LogMessage "Pre-deployment validation FAILED. Fix the errors above before deploying." -Level Error
    Write-LogMessage "No workflows were deployed." -Level Warning
    exit 1
}

Write-LogMessage "All files passed pre-deployment validation." -Level Success
Write-Host ""

# --- Step 6: Deploy parameters file (if applicable) ---
$paramsDeployed = $false
if ($SkipParameters) {
    Write-LogMessage "Step 6: Skipping parameters deployment (SkipParameters flag set)." -Level Info
}
elseif ($ParametersFile -and (Test-Path $ParametersFile)) {
    Write-LogMessage "Step 6: Deploying parameters file..." -Level Info
    $paramsDeployed = Deploy-Parameters -SubId $SubscriptionId -RG $ResourceGroup -AppName $LogicAppName -ParametersFilePath $ParametersFile
}
else {
    # Auto-detect parameters files
    $paramFiles = Get-ParametersFiles -Path $WorkflowsPath
    if ($paramFiles.Count -gt 0) {
        Write-LogMessage "Step 6: Auto-detected $($paramFiles.Count) parameters file(s)..." -Level Info
        foreach ($pf in $paramFiles) {
            Write-LogMessage "  Found: $($pf.Name)" -Level Info
            $paramsDeployed = Deploy-Parameters -SubId $SubscriptionId -RG $ResourceGroup -AppName $LogicAppName -ParametersFilePath $pf.FullName
        }
    }
    else {
        Write-LogMessage "Step 6: No parameters file found. Skipping parameters deployment." -Level Info
    }
}
Write-Host ""

# --- Step 7: Deploy workflows ---
Write-LogMessage "Step 7: Deploying workflows..." -Level Info
Write-Host ""

$results = [System.Collections.ArrayList]::new()
foreach ($wfFile in $workflowFiles) {
    $workflowName = Get-WorkflowNameFromFile -FileName $wfFile.Name

    $result = Deploy-Workflow `
        -SubId $SubscriptionId `
        -RG $ResourceGroup `
        -AppName $LogicAppName `
        -WorkflowName $workflowName `
        -WorkflowJsonPath $wfFile.FullName

    [void]$results.Add($result)
    Write-Host ""
}

# --- Step 8: Summary ---
Write-Host ""
Write-Host "============================================================" -ForegroundColor White
Write-Host "  Deployment Summary" -ForegroundColor White
Write-Host "============================================================" -ForegroundColor White
Write-Host ""

# Display results table
$successCount = @($results | Where-Object { $_.Status -eq "SUCCESS" }).Count
$failedCount = @($results | Where-Object { $_.Status -eq "FAILED" }).Count
$totalCount = $successCount + $failedCount

Write-Host ("{0,-35} {1,-12} {2}" -f "Workflow Name", "Status", "Error") -ForegroundColor White
Write-Host ("{0,-35} {1,-12} {2}" -f ("-" * 35), ("-" * 12), ("-" * 40)) -ForegroundColor Gray

foreach ($r in $results) {
    $color = switch ($r.Status) {
        "SUCCESS" { "Green" }
        "FAILED"  { "Red" }
        default   { "White" }
    }
    $errorDisplay = if ($r.Error) { $r.Error.Substring(0, [Math]::Min(60, $r.Error.Length)) } else { "" }
    Write-Host ("{0,-35} {1,-12} {2}" -f $r.WorkflowName, $r.Status, $errorDisplay) -ForegroundColor $color
}

Write-Host ""
Write-Host "  Total: $totalCount | Success: $successCount | Failed: $failedCount" -ForegroundColor White
Write-Host ""

if ($failedCount -gt 0) {
    Write-LogMessage "Some workflows failed to deploy. Review the errors above." -Level Warning
    exit 1
}
else {
    Write-LogMessage "All workflows deployed successfully!" -Level Success
}

Write-Host ""