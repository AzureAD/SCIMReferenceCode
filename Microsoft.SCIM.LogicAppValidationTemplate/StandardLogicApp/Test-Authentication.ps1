[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$scim = Get-Content -LiteralPath "$root\SCIMTests_Workflow.json" -Raw | ConvertFrom-Json -Depth 100
$authentication = Get-Content -LiteralPath "$root\Authentication_Workflow.json" -Raw | ConvertFrom-Json -Depth 100
$orchestrator = Get-Content -LiteralPath "$root\Orchestrator_Workflow.json" -Raw | ConvertFrom-Json -Depth 100
$parameters = Get-Content -LiteralPath "$root\Orchestrator_Parameters.json" -Raw | ConvertFrom-Json -Depth 100
$script:Assertions = 0

function Assert-Equal($Actual, $Expected, [string]$Message) {
    $script:Assertions++
    if (($Actual | ConvertTo-Json -Depth 50 -Compress) -cne ($Expected | ConvertTo-Json -Depth 50 -Compress)) {
        throw "$Message`: expected '$Expected', got '$Actual'."
    }
}

function Invoke-WdlExpression([string]$Expression, [hashtable]$Context) {
    $source = $Expression.TrimStart('@')
    if ($source.StartsWith('{')) { $source = $source.Substring(1, $source.Length - 2) }
    $tokens = [Collections.Generic.List[object]]::new()
    $pattern = "\G\s*(?:(?<literal>'(?:[^']|'')*')|(?<word>[A-Za-z_][A-Za-z0-9_]*)|(?<number>-?\d+)|(?<symbol>\?\[|[\(\),\]\[]))"
    $position = 0
    while ($position -lt $source.Length) {
        $match = [regex]::Match($source, $pattern, [Text.RegularExpressions.RegexOptions]::None, [timespan]::FromSeconds(1))
        if (-not $match.Success -or $match.Index -ne $position) {
            throw "Unsupported WDL syntax at '$($source.Substring($position))'."
        }
        $kind = @('literal','word','number','symbol') | Where-Object {$match.Groups[$_].Success} | Select-Object -First 1
        $tokens.Add(@{kind=$kind;value=$match.Groups[$kind].Value})
        $position += $match.Length
        # \G is relative to the input; consume the recognized prefix explicitly.
        $source = $source.Substring($position)
        $position = 0
    }
    $state = @{index=0;tokens=$tokens;context=$Context}
    $result = Read-WdlValue $state
    if ($state.index -ne $tokens.Count) { throw 'Trailing WDL tokens.' }
    return $result.value
}

function Read-WdlValue([hashtable]$State) {
    $token = $State.tokens[$State.index++]
    if ($token.kind -eq 'literal') {
        $value = $token.value.Substring(1,$token.value.Length-2).Replace("''","'")
    } elseif ($token.kind -eq 'number') {
        $value = [long]$token.value
    } elseif ($token.value -in @('true','false','null')) {
        $value = switch ($token.value) { 'true' {$true} 'false' {$false} 'null' {$null} }
    } else {
        if ($State.tokens[$State.index++].value -ne '(') { throw 'Expected WDL function call.' }
        $arguments = [Collections.Generic.List[object]]::new()
        if ($State.tokens[$State.index].value -ne ')') {
            do {
                $arguments.Add((Read-WdlValue $State).value)
                if ($State.tokens[$State.index].value -ne ',') { break }
                $State.index++
            } while ($true)
        }
        if ($State.tokens[$State.index++].value -ne ')') { throw 'Unclosed WDL call.' }
        $a = $arguments.ToArray()
        $value = switch ($token.value) {
            'parameters' { ,$State.context.parameters[$a[0]] }
            'variables' { ,$State.context.variables[$a[0]] }
            'outputs' { ,$State.context.outputs[$a[0]] }
            'body' { ,$State.context.body[$a[0]] }
            'actions' { ,$State.context.actions[$a[0]] }
            'triggerBody' { $State.context.triggerBody }
            'if' { if ($a[0]) { ,$a[1] } else { ,$a[2] } }
            'and' { $a -notcontains $false }
            'or' { $a -contains $true }
            'not' { -not $a[0] }
            'equals' { ($a[0] | ConvertTo-Json -Depth 30 -Compress) -ceq ($a[1] | ConvertTo-Json -Depth 30 -Compress) }
            'contains' { if ($a[0] -is [string]) {$a[0].Contains([string]$a[1])} else {$a[0] -ccontains $a[1]} }
            'empty' { $null -eq $a[0] -or $a[0].Count -eq 0 -or $a[0] -ceq '' }
            'length' { if ($a[0] -is [string]) {$a[0].Length} else {@($a[0]).Count} }
            'createArray' { ,$a }
            'json' { ,(ConvertFrom-Json -InputObject $a[0] -Depth 50 -NoEnumerate) }
            'union' { ,@($a | ForEach-Object {$_} | Select-Object -Unique) }
            'intersection' { ,@($a[0] | Where-Object {$_ -cin $a[1]} | Select-Object -Unique) }
            'coalesce' { ,($a | Where-Object {$null -ne $_} | Select-Object -First 1) }
            'concat' { ($a | ForEach-Object {[string]$_}) -join '' }
            'ticks' { ([datetimeoffset]::Parse($a[0])).Ticks }
            'utcNow' { $State.context.now }
            'addMinutes' { ([datetimeoffset]::Parse($a[0])).AddMinutes([double]$a[1]).ToString('o') }
            'lessOrEquals' { $a[0] -le $a[1] }
            'string' { if ($a[0] -is [string]) {$a[0]} else {ConvertTo-Json -InputObject $a[0] -Depth 30 -Compress} }
            default { throw "Unsupported WDL function '$($token.value)'." }
        }
    }
    while ($State.index -lt $State.tokens.Count -and $State.tokens[$State.index].value -in @('[','?[')) {
        $State.index++
        $key = (Read-WdlValue $State).value
        if ($State.tokens[$State.index++].value -ne ']') { throw 'Unclosed WDL property access.' }
        $value = if ($null -eq $value) {$null} elseif ($value -is [Collections.IDictionary]) {$value[$key]} else {$value.$key}
    }
    return [pscustomobject]@{value=$value}
}

function Invoke-WdlCondition($Condition, [hashtable]$Context) {
    if ($Condition -is [string]) {
        if ($Condition.StartsWith('@')) { return Invoke-WdlExpression $Condition $Context }
        return $Condition
    }
    if ($Condition -isnot [pscustomobject]) { return $Condition }
    $property = @($Condition.PSObject.Properties)[0]
    $values = @($property.Value | ForEach-Object {Invoke-WdlCondition $_ $Context})
    switch ($property.Name) {
        'and' { return $values -notcontains $false }
        'or' { return $values -contains $true }
        'contains' { return $values[0] -ccontains $values[1] }
        'equals' { return $values[0] -ceq $values[1] }
        default { throw "Unsupported structured condition '$($property.Name)'." }
    }
}

function New-TestContext([string[]]$Methods, [string]$OAuth, [string]$Wif) {
    @{
        parameters=@{supportedAuthenticationMethods=$Methods;scimTokenEndpoint='';federatedTokenEndpoint='';scimBearerToken='static-sentinel';scimAuthenticationMethod='Bearer'}
        variables=@{ValidateCredentialsTestOutputs=@{overallResult=$OAuth};FederatedIdentityTestOutputs=@{overallResult=$Wif};BearerConnectivityTestOutputs=@{overallResult='PASSED'};SCIMAccessTokenCache=@{}}
        outputs=@{Compose_Authentication_Methods=$Methods}
        body=@{}
        actions=@{}
        triggerBody=@{EnabledTests='All';initializationData=@{targetDirectoryUserAttributes=@()}}
        now='2026-01-01T00:00:00Z'
    }
}

$cases = @(
    @{methods=@('OAuth2ClientCredentials');oauth='PASSED';wif='NOT_APPLICABLE';expected='PASSED'},
    @{methods=@('WorkloadIdentityFederation');oauth='NOT_APPLICABLE';wif='PASSED';expected='PASSED'},
    @{methods=@('OAuth2ClientCredentials','WorkloadIdentityFederation');oauth='PASSED';wif='PASSED';expected='PASSED'},
    @{methods=@('OAuth2ClientCredentials','WorkloadIdentityFederation');oauth='PASSED';wif='FAILED';expected='FAILED'},
    @{methods=@('OAuth2ClientCredentials','WorkloadIdentityFederation');oauth='FAILED';wif='PASSED';expected='FAILED'},
    @{methods=@('OAuth2ClientCredentials','WorkloadIdentityFederation');oauth='FAILED';wif='FAILED';expected='FAILED'},
    @{methods=@('OAuth2ClientCredentials');oauth='FAILED';wif='PASSED';expected='FAILED'},
    @{methods=@('WorkloadIdentityFederation');oauth='PASSED';wif='SKIPPED';expected='FAILED'},
    @{methods=@('Bearer');oauth='NOT_APPLICABLE';wif='NOT_APPLICABLE';expected='NOT_GALLERY_READY'},
    @{methods=@();oauth='SKIPPED';wif='SKIPPED';expected='FAILED'},
    @{methods=@('invalid','OAuth2ClientCredentials');oauth='PASSED';wif='SKIPPED';expected='FAILED'},
    @{methods=@('OAuth2ClientCredentials','WorkloadIdentityFederation');oauth='PASSED';wif='NOT_APPLICABLE';expected='FAILED'},
    @{methods=@('OAuth2ClientCredentials','WorkloadIdentityFederation');oauth='PASSED';wif=$null;expected='FAILED'},
    @{methods=@('Bearer','OAuth2ClientCredentials');oauth='PASSED';wif='NOT_APPLICABLE';expected='PASSED'}
)
foreach ($case in $cases) {
    $context = New-TestContext $case.methods $case.oauth $case.wif
    $actual = Invoke-WdlExpression $authentication.definition.actions.Compose_Authentication_Model_Result.inputs $context
    Assert-Equal $actual $case.expected "Auth matrix $($case.methods -join ',')"
}
$oauth = New-TestContext @('OAuth2ClientCredentials') 'PASSED' 'NOT_APPLICABLE'
Assert-Equal (Invoke-WdlCondition $authentication.definition.actions.Authentication_Tests.actions.Validate_Credentials_Test.expression $oauth) $true 'Declared OAuth executes'
Assert-Equal (Invoke-WdlCondition $authentication.definition.actions.Authentication_Tests.actions.Federated_Identity_Test.expression $oauth) $false 'Undeclared WIF makes no token request'
$wif = New-TestContext @('WorkloadIdentityFederation') 'NOT_APPLICABLE' 'PASSED'
Assert-Equal (Invoke-WdlCondition $authentication.definition.actions.Authentication_Tests.actions.Validate_Credentials_Test.expression $wif) $false 'Undeclared OAuth makes no token request'
Assert-Equal (Invoke-WdlCondition $authentication.definition.actions.Authentication_Tests.actions.Federated_Identity_Test.expression $wif) $true 'Declared WIF executes'
$mixedBearer = New-TestContext @('Bearer','OAuth2ClientCredentials') 'PASSED' 'NOT_APPLICABLE'
$mixedBearer.variables.BearerConnectivityTestOutputs.overallResult = 'FAILED'
Assert-Equal (Invoke-WdlExpression $authentication.definition.actions.Compose_Authentication_Model_Result.inputs $mixedBearer) 'FAILED' 'Every selected method includes static-token connectivity'
Assert-Equal (Invoke-WdlCondition $authentication.definition.actions.Authentication_Tests.actions.Bearer_Connectivity_Test.expression $oauth) $false 'OAuth does not execute static-token connectivity'
$legacy = New-TestContext @() 'SKIPPED' 'PASSED'
$legacy.parameters.federatedTokenEndpoint = 'https://example.invalid/exchange'
Assert-Equal @(Invoke-WdlExpression $scim.definition.actions.Compose_Authentication_Methods.inputs $legacy) @('WorkloadIdentityFederation') 'Legacy WIF-only inference'
$legacy.parameters.federatedTokenEndpoint = ''
$legacy.parameters.scimTokenEndpoint = 'https://example.invalid/token'
Assert-Equal @(Invoke-WdlExpression $scim.definition.actions.Compose_Authentication_Methods.inputs $legacy) @('OAuth2ClientCredentials') 'Legacy OAuth-only inference'
$defaults = @($authentication.definition.actions.PSObject.Properties | Where-Object {$_.Value.type -eq 'InitializeVariable'} | ForEach-Object {$_.Value.inputs.variables})
foreach ($variable in $defaults | Where-Object name -in @('ValidateCredentialsTestOutputs','FederatedIdentityTestOutputs')) {
    $result = Invoke-WdlExpression $variable.value.overallResult $oauth
    $expected = if ($variable.name -eq 'ValidateCredentialsTestOutputs') {'SKIPPED'} else {'NOT_APPLICABLE'}
    Assert-Equal $result $expected 'Applicability output default'
}
$both = New-TestContext @('OAuth2ClientCredentials','WorkloadIdentityFederation') 'PASSED' 'PASSED'
foreach ($context in @($oauth,$wif,$both)) {
    foreach ($name in @('SchemaDiscoverabilityTestOutputs','SCIMNullUpdateTestOutputs','SCIMUserCreateTestOutputs','SCIMGroupCreateTestOutputs','SCIMUserUpdateTestOutputs','SCIMGroupUpdateTestOutputs','SCIMUpdateManagerTestOutputs')) {
        $context.variables[$name] = @{overallResult='PASSED'}
    }
    $missingChild = New-TestContext @('OAuth2ClientCredentials') 'PASSED' 'NOT_APPLICABLE'
    Assert-Equal (Invoke-WdlExpression $scim.definition.actions.Compose_Authentication_Model_Result.inputs $missingChild) 'FAILED' 'Missing authentication workflow output fails closed'
    $missingChild.body.Call_Authentication_Workflow = @{authenticationModelResult='PASSED'}
    Assert-Equal (Invoke-WdlExpression $scim.definition.actions.Compose_Authentication_Model_Result.inputs $missingChild) 'PASSED' 'SCIM suite propagates the observed authentication result'
    foreach ($case in $cases) {
        $context = New-TestContext $case.methods $case.oauth $case.wif
        foreach ($name in @('SchemaDiscoverabilityTestOutputs','SCIMNullUpdateTestOutputs','SCIMUserCreateTestOutputs','SCIMGroupCreateTestOutputs','SCIMUserUpdateTestOutputs','SCIMGroupUpdateTestOutputs','SCIMUpdateManagerTestOutputs')) {
            $context.variables[$name] = @{overallResult='PASSED'}
        }
        $context.outputs.Compose_Authentication_Model_Result = Invoke-WdlExpression $authentication.definition.actions.Compose_Authentication_Model_Result.inputs $context
        $expected = if ($case.expected -eq 'PASSED') {'Passed'} else {'Failed'}
        Assert-Equal (Invoke-WdlExpression $scim.definition.actions.Compose_Final_Results.inputs.status $context) $expected 'Selected-method aggregate controls suite success'
    }
    $context.outputs.Compose_Authentication_Model_Result = 'PASSED'
    Assert-Equal (Invoke-WdlExpression $scim.definition.actions.Compose_Final_Results.inputs.status $context) 'Passed' 'Undeclared authentication does not fail the SCIM suite'
    $context.variables.SchemaDiscoverabilityTestOutputs.overallResult = 'FAILED'
    Assert-Equal (Invoke-WdlExpression $scim.definition.actions.Compose_Final_Results.inputs.status $context) 'Failed' 'Core failure is not hidden by valid auth'
}

function Get-Actions($Actions, [int]$Depth=0) {
    foreach ($property in $Actions.PSObject.Properties) {
        [pscustomobject]@{name=$property.Name;action=$property.Value;siblings=$Actions;depth=$Depth}
        if ($property.Value.actions) { Get-Actions $property.Value.actions ($Depth + 1) }
        if ($property.Value.else.actions) { Get-Actions $property.Value.else.actions ($Depth + 1) }
        foreach ($case in $property.Value.cases.PSObject.Properties) {
            if ($case.Value.actions) { Get-Actions $case.Value.actions ($Depth + 1) }
        }
        if ($property.Value.default.actions) { Get-Actions $property.Value.default.actions ($Depth + 1) }
    }
}
$requestCount = 0
foreach ($file in Get-ChildItem -LiteralPath $root -Filter '*_Workflow.json') {
    $workflow = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json -Depth 100
    Assert-Equal ([IO.File]::ReadAllText($file.FullName).Contains("`r")) $false "Workflow JSON uses LF line endings: $($file.Name)"
    $actions = @(Get-Actions $workflow.definition.actions)
    Assert-Equal ($actions.Count -le 500) $true "Workflow remains within the 500-action limit: $($file.Name), $($actions.Count)"
    Assert-Equal ($file.BaseName.Length -le 32) $true "Standard workflow name stays within 32 characters: $($file.BaseName)"
    Assert-Equal @($actions | Group-Object name | Where-Object Count -gt 1).Count 0 "Unique action names in $($file.Name)"
    foreach ($entry in $actions) {
        Assert-Equal ($entry.name.Length -le 80) $true "Action name stays within 80 characters: $($entry.name)"
        Assert-Equal ($entry.depth -le 8) $true "Action nesting stays within eight levels: $($entry.name)"
        foreach ($dependency in $entry.action.runAfter.PSObject.Properties.Name) {
            Assert-Equal ($dependency -in @($entry.siblings.PSObject.Properties.Name)) $true "Dependency exists in the same scope: $($entry.name) -> $dependency"
        }
        if ($entry.action.type -eq 'InitializeVariable') {
            Assert-Equal @($entry.action.inputs.variables).Count 1 'Standard runtime initializes one variable per action'
        }
        $securedProperties = @($entry.action.runtimeConfiguration.secureData.properties | Where-Object {$_})
        if ($entry.action.type -in @('InitializeVariable','SetVariable','AppendToArrayVariable','AppendToStringVariable')) {
            Assert-Equal $securedProperties.Count 0 'Variable actions do not use unsupported secureData'
        }
        if ($entry.action.type -in @('Response','Compose','ParseJson')) {
            Assert-Equal @($securedProperties | Where-Object {$_ -ne 'inputs'}).Count 0 'Response and Compose support secured inputs, not secured outputs'
        }
    }
    foreach ($entry in $actions | Where-Object {$_.action.type -eq 'Http' -and $_.action.inputs.headers.Authorization -is [string] -and $_.action.inputs.headers.Authorization.Contains("parameters('scimBearerToken')")}) {
        if ($file.Name -eq 'Authentication_Workflow.json') { continue }
        $requestCount++
        $action = $entry.action
        $dependency = @($action.runAfter.PSObject.Properties.Name)
        Assert-Equal $dependency.Count 1 'SCIM request has exactly one auth dependency'
        $ensure = $entry.siblings.($dependency[0])
        Assert-Equal $ensure.type 'If' 'SCIM request waits for token cache guard'
        $context = New-TestContext @('OAuth2ClientCredentials') 'PASSED' 'NOT_APPLICABLE'
        $context.parameters.scimAuthenticationMethod = 'OAuth2ClientCredentials'
        Assert-Equal (Invoke-WdlExpression $ensure.expression $context) $true 'Empty OAuth cache refreshes'
        $context.variables.SCIMAccessTokenCache = @{accessToken='minted-token';expiresAt='2026-01-01T00:30:00Z'}
        Assert-Equal (Invoke-WdlExpression $ensure.expression $context) $false 'Unexpired OAuth cache is reused'
        Assert-Equal (Invoke-WdlExpression $action.inputs.headers.Authorization $context) 'Bearer minted-token' 'OAuth never falls back to the static token'
        $context.variables.SCIMAccessTokenCache.expiresAt = '2026-01-01T00:04:00Z'
        Assert-Equal (Invoke-WdlExpression $ensure.expression $context) $true 'Near-expiry OAuth cache refreshes'
        $context.parameters.scimAuthenticationMethod = 'WorkloadIdentityFederation'
        Assert-Equal (Invoke-WdlExpression $ensure.expression $context) $true 'Near-expiry WIF cache refreshes'
        Assert-Equal (Invoke-WdlExpression $action.inputs.headers.Authorization $context) 'Bearer minted-token' 'WIF never falls back to a static token'
        $context.parameters.scimAuthenticationMethod = 'Bearer'
        Assert-Equal (Invoke-WdlExpression $ensure.expression $context) $false 'Bearer transport makes no OAuth request'
        Assert-Equal (Invoke-WdlExpression $action.inputs.headers.Authorization $context) 'Bearer static-sentinel' 'Existing bearer behavior is preserved'
    }
}
Assert-Equal $requestCount 99 'All direct SCIM requests are covered'
Assert-Equal $parameters.scimAuthenticationMethod.value 'Bearer' 'Legacy transport remains the default'
Assert-Equal @($parameters.scimAuthenticationMethod.allowedValues) @('Bearer','OAuth2ClientCredentials','WorkloadIdentityFederation') 'All supported direct-request transports are available'
$tokenWorkflow = Get-Content -LiteralPath "$root\SCIMToken_Workflow.json" -Raw | ConvertFrom-Json -Depth 100
$tokenActions = @(Get-Actions $tokenWorkflow.definition.actions)
$existingWifActions = @(Get-Actions $authentication.definition.actions.Authentication_Tests.actions.Federated_Identity_Test.actions)
foreach ($entry in $existingWifActions | Where-Object {$_.action.type -eq 'Http'}) {
    $tokenAction = @($tokenActions | Where-Object name -eq $entry.name)
    Assert-Equal $tokenAction.Count 1 "WIF exchange is reused: $($entry.name)"
    Assert-Equal $tokenAction[0].action.inputs $entry.action.inputs "WIF exchange contract preserved: $($entry.name)"
    Assert-Equal @($tokenAction[0].action.runtimeConfiguration.secureData.properties) @('inputs','outputs') 'WIF exchange secrets are protected'
}
Assert-Equal $scim.definition.actions.Compose_Final_Results.inputs.authenticationModelResult "@outputs('Compose_Authentication_Model_Result')" 'SCIM suite emits the missing authentication result'
Assert-Equal $scim.definition.actions.SCIMTests_Scope.actions.Call_Authentication_Workflow.inputs.host.workflow.id 'Authentication_Workflow' 'Authentication runs in a separate horizontal child workflow'
Assert-Equal $authentication.definition.actions.Return_Authentication_Results.inputs.body.authenticationPolicy 'AllSelected' 'Authentication policy is explicit in run output'
Assert-Equal $parameters.LogicAppVersion.value (Get-Content -LiteralPath "$root\VERSION" -Raw).Trim() 'VERSION is the only bundle and agent version source'
$versionBytes = [IO.File]::ReadAllBytes("$root\VERSION")
Assert-Equal @($versionBytes | Where-Object {$_ -gt 127 -or $_ -eq 13}).Count 0 'VERSION is ASCII without BOM or CRLF'
Assert-Equal ([IO.File]::ReadAllText("$root\VERSION")) ($parameters.LogicAppVersion.value + "`n") 'VERSION has exactly one LF terminator'
Assert-Equal ([IO.File]::ReadAllText("$root\Orchestrator_Parameters.json").Contains("`r")) $false 'Parameter JSON uses LF line endings'
foreach ($name in @('Evaluate_Test_Results','Final_TestResults')) {
    Assert-Equal $orchestrator.definition.actions.$name.inputs.logicAppVersion "@parameters('LogicAppVersion')" 'Run artifacts include deployed bundle version'
    Assert-Equal $orchestrator.definition.actions.$name.inputs.authenticationPolicy 'AllSelected' 'Orchestrator output identifies AND policy'
}
$authenticationTests = @($orchestrator.definition.actions.Final_TestResults.inputs.testResults | Where-Object authenticationMethod -in @('OAuth2ClientCredentials','WorkloadIdentityFederation'))
Assert-Equal $authenticationTests.Count 2 'The existing report inventory retains both authentication test names'
foreach ($test in $authenticationTests) {
    Assert-Equal $test.testCategory 'Declared/StrictSuccess' 'Each declared authentication test is individually strict'
}
if (-not $orchestrator.definition.actions.Evaluate_Test_Results.inputs.overallLogicAppResult.Contains("not(equals(coalesce(body('Get_SCIMTests_Output_Content')?['authenticationModelResult'],'FAILED'),'PASSED'))")) {
    throw 'Orchestrator must reject missing or non-passing authentication readiness.'
}
Write-Output "PASS: $script:Assertions assertions; auth policy matrix and $requestCount direct SCIM requests verified."
