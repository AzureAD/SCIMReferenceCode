# Run locally with pwsh -NoProfile -File ./version-deployment.test.ps1.
# Load only function definitions: never execute the deployment script's main/cloud path.
$ErrorActionPreference = 'Stop'
$bundle = Split-Path $PSScriptRoot -Parent
$scriptPath = Join-Path $bundle 'Deploy-LogicAppWorkflows.ps1'
$tokens = $null
$parseErrors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count) { throw ($parseErrors | Out-String) }
foreach ($name in @('Get-VersionedParametersContent', 'Deploy-VfsFile', 'Deploy-Parameters')) {
    $function = $ast.Find({ param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name }, $true)
    if (-not $function) { throw "Missing function: $name" }
    Invoke-Expression $function.Extent.Text
}
function Write-LogMessage { param($Message, $Level) }
function Assert-True { param($Value, $Message) if (-not $Value) { throw $Message } }
function Assert-Throws {
    param([scriptblock]$Action)
    $failed = $false
    try { & $Action | Out-Null } catch { $failed = $true }
    Assert-True $failed 'Expected version validation to fail.'
}

$temp = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString())
$null = New-Item -ItemType Directory -Path $temp
try {
    $file = Join-Path $temp 'parameters.json'
    $fixture = '{"LogicAppVersion":{"type":"String","value":""},"fixture":{"type":"Object","value":{"nested":[1,2],"expression":"@parameters(''fixture'')","text":"caf\u00e9"}}}'
    [System.IO.File]::WriteAllText($file, $fixture)
    $script:BundleVersion = (Get-Content -LiteralPath (Join-Path $bundle 'VERSION') -Raw -Encoding UTF8).Trim()
    foreach ($version in @("$script:BundleVersion`n", "$script:BundleVersion`r`n", 'future-version')) {
        $json = Get-VersionedParametersContent $file $version | ConvertFrom-Json
        Assert-True ($json.LogicAppVersion.value -ceq $version.Trim()) 'Version was not derived from the supplied bundle.'
        Assert-True (($json.fixture | ConvertTo-Json -Depth 100 -Compress) -ceq (($fixture | ConvertFrom-Json).fixture | ConvertTo-Json -Depth 100 -Compress)) 'Other parameters changed.'
        Assert-True (([System.IO.File]::ReadAllText($file)) -ceq $fixture) 'Source parameters were modified.'
    }
    Assert-Throws { Get-VersionedParametersContent $file '' }
    Assert-Throws { Get-VersionedParametersContent $file '  ' }
    Assert-Throws { Get-VersionedParametersContent (Join-Path $temp 'missing.json') 'test' }
    [System.IO.File]::WriteAllText($file, '{}')
    Assert-Throws { Get-VersionedParametersContent $file 'test' }
    [System.IO.File]::WriteAllText($file, $fixture)

    # Mock both external commands, so even the upload/read-back path stays offline.
    function az { $global:LASTEXITCODE = 0; return 'local-test-placeholder' }
    $script:uploaded = $null
    $script:mismatch = $false
    function Invoke-RestMethod {
        param($Method, $Uri, $Headers, $Body, $ContentType)
        if ($Method -eq 'PUT') { $script:uploaded = $Body | ConvertFrom-Json; return }
        if ($script:mismatch) { return @{ LogicAppVersion = @{ value = 'wrong-version' } } }
        return $script:uploaded
    }
    $script:ManagementApiBase = 'https://unused.invalid'
    Assert-True (Deploy-Parameters 'test' 'test' 'test' $file) 'Matching read-back should pass.'
    Assert-True ($script:uploaded.LogicAppVersion.value -ceq $script:BundleVersion) 'Upload used the template version.'
    $script:mismatch = $true
    Assert-True (-not (Deploy-Parameters 'test' 'test' 'test' $file)) 'Mismatched read-back should fail.'

    # These guards must fail before Azure CLI is consulted.
    foreach ($mode in @('skip', 'missing', 'empty')) {
        $args = @('-NoProfile', '-File', $scriptPath, '-SubscriptionId', 'test', '-ResourceGroup', 'test', '-LogicAppName', 'test', '-WorkflowsPath', $temp)
        if ($mode -eq 'skip') { $args += '-SkipParameters' }
        elseif ($mode -eq 'empty') { [System.IO.File]::WriteAllText((Join-Path $temp 'VERSION'), '  ') }
        $output = & (Join-Path $PSHOME 'pwsh') @args 2>&1 | Out-String
        Assert-True ($LASTEXITCODE -ne 0 -and $output.Contains('Version preparation failed')) "Preflight did not reject $mode."
    }
    Write-Output 'Version deployment checks passed'
}
finally {
    Remove-Item -LiteralPath $file -ErrorAction SilentlyContinue
    $versionFile = Join-Path $temp 'VERSION'
    if (Test-Path -LiteralPath $versionFile) { Remove-Item -LiteralPath $versionFile }
    Remove-Item -LiteralPath $temp
}
