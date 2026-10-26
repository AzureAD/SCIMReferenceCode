/*
 * Local regression checks against workflow JSON, not the Azure Logic Apps runtime.
 * No cloud calls or endpoint credentials are used. See README.md in this directory.
 * From the repository/worktree root:
 * node --test .\Microsoft.SCIM.LogicAppValidationTemplate\StandardLogicApp\tests\minimal-auth-selection.test.cjs
 */
const assert = require('node:assert/strict');
const {test} = require('node:test');
const fs = require('node:fs');
const path = require('node:path');
const {execFileSync} = require('node:child_process');
const {isDeepStrictEqual} = require('node:util');
const root = path.resolve(__dirname, '..');
const load = name => JSON.parse(fs.readFileSync(path.join(root, name), 'utf8'));
// Pin the merged Google fixes so preservation checks do not move with master.
const baseline = name => JSON.parse(execFileSync('git', ['show', `fde52cc:Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp/${name}`], {cwd: root, encoding: 'utf8', maxBuffer: 20e6}));
const child = load('SCIMTests_Workflow.json').definition.actions;
const parent = load('Orchestrator_Workflow.json').definition.actions;
const userUpdateActions = workflow => workflow.definition.actions.UserTests_Scope.actions
    .Update_User_Test.actions['Update_User_Test_-_Actions'].actions;
const googleRequiredUpdateActions = workflow => workflow.definition.actions.Protect_Google_Required_User_Attributes
    .actions.ForEach_Google_User_Target.actions.Check_Google_Target_Required.actions;

// Evaluate only the expression subset used below, with simulated action outputs.
// This is not a general interpreter: it does not model runtime scheduling or lazy branches.
function evaluate(expression, context) {
    if (typeof expression === 'object') {
        const [op, args] = Object.entries(expression)[0];
        if (op === 'not') return !evaluate(args, context);
        const values = args.map(value => evaluate(value, context));
        return op === 'and' ? values.every(Boolean) : op === 'or' ? values.some(Boolean) : values[0] === values[1];
    }
    if (typeof expression !== 'string' || !expression.startsWith('@')) return expression;
    const source = expression.slice(1);
    let position = 0;
    const functions = {
        parameters: name => context.parameters?.[name], variables: name => context.variables?.[name],
        triggerBody: () => context.triggerBody, body: name => context.bodies?.[name],
        actions: name => context.actions?.[name], outputs: name => context.outputs?.[name],
        items: name => context.items?.[name],
        equals: isDeepStrictEqual, not: value => !value, and: (...args) => args.every(Boolean),
        or: (...args) => args.some(Boolean), if: (condition, yes, no) => condition ? yes : no,
        coalesce: (...args) => args.find(value => value != null), json: JSON.parse,
        string: value => typeof value === 'object' ? JSON.stringify(value) : String(value),
        contains: (value, item) => value !== null && typeof value === 'object' && !Array.isArray(value)
            ? Object.hasOwn(value, item) : value?.includes(item) || false,
        toLower: value => value.toLowerCase(),
        replace: (value, search, replacement) => value.replaceAll(search, replacement),
        first: value => value?.[0],
        last: value => value?.at(-1),
        split: (value, delimiter) => value.split(delimiter),
        setProperty: (value, key, item) => ({...value, [key]: item}),
        removeProperty: (value, key) => Object.fromEntries(Object.entries(value).filter(([name]) => name !== key)),
        empty: value => value == null || value === '' ||
            (typeof value === 'object' && Object.keys(value).length === 0),
        ticks: value => {
            const fraction = /\.(\d+)Z$/.exec(value)?.[1] || '';
            return BigInt(Date.parse(value.replace(/\.\d+Z$/, 'Z'))) * 10000n +
                BigInt(fraction.padEnd(7, '0')) + 621355968000000000n;
        },
        sub: (a, b) => a - b,
        div: (a, b) => Number(a / BigInt(b)),
    };
    const space = () => { while (position < source.length && /\s/.test(source[position])) position++; };
    function value() {
        space();
        let result;
        if (source[position] === "'") {
            const start = ++position;
            while (source[position] !== "'") { if (position >= source.length) throw Error('Unclosed string'); position++; }
            result = source.slice(start, position++);
        } else {
            const match = /^(?:[A-Za-z_]\w*|\d+)/.exec(source.slice(position));
            assert.ok(match, source.slice(position));
            position += match[0].length;
            space();
            if (source[position] === '(') {
                position++;
                const args = [];
                space();
                while (source[position] !== ')') {
                    args.push(value()); space();
                    if (source[position] !== ',') break;
                    position++;
                }
                assert.equal(source[position++], ')');
                assert.ok(functions[match[0]], match[0]);
                result = functions[match[0]](...args);
            } else result = /^\d+$/.test(match[0]) ? Number(match[0]) : {true: true, false: false, null: null}[match[0]];
        }
        space();
        while (source[position] === '?' || source[position] === '[') {
            if (source[position] === '?') position++;
            assert.equal(source[position++], '[');
            const key = value();
            assert.equal(source[position++], ']');
            result = result?.[key];
            space();
        }
        return result;
    }
    const result = value();
    assert.equal(position, source.length, source.slice(position));
    return result;
}
const methods = ['OAuth2ClientCredentials', 'FederatedIdentity'];
function authenticationContext(context) {
    context.outputs ??= {};
    context.outputs.Compose_Google_Federated_Auth_Bypass = evaluate(child.Compose_Google_Federated_Auth_Bypass.inputs, context);
    context.outputs.Compose_Authentication_Model_Result = evaluate(child.Compose_Authentication_Model_Result.inputs, context);
    return context;
}
function paginationContext(context) {
    context.outputs ??= {};
    context.outputs.Compose_Pagination_Model_Results = Object.fromEntries(
        Object.entries(parent.Compose_Pagination_Model_Results.inputs)
            .map(([key, expression]) => [key, evaluate(expression, context)]));
    return context;
}
test('only the selected existing branch runs; EnabledTests remains honored', () => {
    for (const method of [...methods, '', 'invalid']) {
        for (const enabled of ['All', 'SCIMTests', 'UserTests', 'Validate_Credentials_Test', 'Federated_Identity_Test']) {
            const context = authenticationContext({parameters: {selectedAuthenticationMethod: method}, triggerBody: {EnabledTests: enabled}});
            for (const [index, name] of ['Validate_Credentials_Test', 'Federated_Identity_Test'].entries()) {
                assert.equal(evaluate(child.SCIMTests_Scope.actions[name].expression, context),
                    method === methods[index] && ['All', 'SCIMTests', name].includes(enabled));
            }
        }
    }
});
test('selected missing, skipped or failed output cannot be rescued by the other method', () => {
    for (const [index, method] of methods.entries()) {
        for (const outcome of ['PASSED', 'FAILED', 'SKIPPED', undefined]) {
            // Keep the unselected result PASSED to detect an accidental return to OR logic.
            const variables = {ValidateCredentialsTestOutputs: {overallResult: 'PASSED'}, FederatedIdentityTestOutputs: {overallResult: 'PASSED'}};
            variables[index ? 'FederatedIdentityTestOutputs' : 'ValidateCredentialsTestOutputs'] = {overallResult: outcome};
            const actual = evaluate(child.Compose_Final_Results.inputs.authenticationModelResult,
                authenticationContext({parameters: {selectedAuthenticationMethod: method}, variables}));
            assert.equal(actual === 'PASSED', outcome === 'PASSED');
        }
    }
    assert.equal(evaluate(child.Compose_Final_Results.inputs.authenticationModelResult,
        authenticationContext({parameters: {selectedAuthenticationMethod: ''}})), 'FAILED');
});
test('parent requires selected PASSED while preserving independent semantic failures', () => {
    const original = baseline('Orchestrator_Workflow.json').definition.actions.Evaluate_Test_Results.inputs.overallLogicAppResult;
    const current = parent.Evaluate_Test_Results.inputs.overallLogicAppResult;
    // Only the user-pagination gate changes from the merged Google baseline.
    assert.equal(current.replace("coalesce(outputs('Compose_Pagination_Model_Results')?['userPaginationResult'],'FAILED')",
        "coalesce(body('Get_SCIMTests_Output_Content')?['SCIMUserPaginationTestOutputs']?['overallResult'],'SKIPPED')"), original);
    const outputs = {};
    for (const file of ['UserTests_Workflow.json', 'GroupTests_Workflow.json', 'SCIMTests_Workflow.json']) {
        const final = load(file).definition.actions.Compose_Final_Results.inputs;
        outputs[`Get_${file.replace('_Workflow.json', '')}_Output_Content`] =
            Object.fromEntries(Object.keys(final).filter(key => key.endsWith('Outputs')).map(key => [key, {overallResult: 'PASSED'}]));
    }
    const context = {bodies: {...outputs, Call_Initialization_Workflow: {isGroupSupported: true}, Poll_SCIMTests_Status: {properties: {status: 'Succeeded'}}}};
    for (const result of ['PASSED', 'FAILED', 'SKIPPED', undefined]) {
        context.bodies.Get_SCIMTests_Output_Content.authenticationModelResult = result;
        assert.equal(evaluate(current, paginationContext(context)), result === 'PASSED' ? 'Succeeded' : 'Failed');
    }
    context.bodies.Get_SCIMTests_Output_Content.authenticationModelResult = 'PASSED';
    context.bodies.Get_UserTests_Output_Content.CreateUserTestOutputs.overallResult = 'FAILED';
    assert.equal(evaluate(current, paginationContext(context)), 'Failed');
});
test('Google WIF keeps individual results unchanged and accepts its exception only in the aggregate', () => {
    const reportedResult = child.Compose_Final_Results.inputs.FederatedIdentityTestOutputs;
    const parentResult = parent.Final_TestResults.inputs.testResults.find(entry => entry.testName === 'Federated_Identity_Test');
    for (const baseAddress of ['https://example.GOOGLEAPIS.COM/scim', 'https://scim.example.test', undefined]) {
        for (const method of [...methods, 'invalid']) {
            for (const enabled of ['All', 'SCIMTests', 'Federated_Identity_Test', 'UserTests', undefined]) {
                for (const result of ['SKIPPED', 'PASSED', 'FAILED', undefined]) {
                    const raw = {overallResult: result, result: result ?? 'missing', summary: {original: true}};
                    const context = authenticationContext({
                        parameters: {selectedAuthenticationMethod: method},
                        triggerBody: {EnabledTests: enabled, initializationData: {baseAddress}},
                        variables: {FederatedIdentityTestOutputs: raw},
                    });
                    const reported = evaluate(reportedResult, context);
                    assert.deepEqual(reported, raw);
                    assert.equal(evaluate(parentResult.testResult, {bodies: {
                        Get_SCIMTests_Output_Content: {FederatedIdentityTestOutputs: reported},
                    }}), reported.result);
                    const exempted = baseAddress?.includes('GOOGLEAPIS.COM') && method === 'FederatedIdentity';
                    assert.equal(evaluate(child.Compose_Final_Results.inputs.authenticationModelBypassed, context), Boolean(exempted));
                    if (exempted && result === 'SKIPPED') {
                        assert.equal(evaluate(child.Compose_Final_Results.inputs.authenticationModelResult, context), 'PASSED');
                        assert.equal(evaluate(child.SCIMTests_Scope.actions.Federated_Identity_Test.expression, context), false);
                        assert.match(evaluate(child.Compose_Final_Results.inputs.authenticationModelBypassReason, context),
                            /standalone authentication diagnostics skipped/);
                    } else if (!exempted && method === 'FederatedIdentity') {
                        assert.equal(evaluate(child.Compose_Final_Results.inputs.authenticationModelResult, context), result ?? 'FAILED');
                    }
                    assert.equal(raw.overallResult, result);
                }
            }
        }
    }
});
test('Google pagination aggregate accepts only enabled explicit skips and preserves individual results', () => {
    for (const baseAddress of ['https://example.GOOGLEAPIS.COM/scim', 'https://scim.example.test', '', undefined]) {
        for (const enabled of ['All', 'SCIMTests', 'UserTests', 'SCIM_User_Pagination_Test', 'SCIM_Group_Pagination_Test', undefined]) {
            for (const result of ['PASSED', 'SKIPPED', 'FAILED', undefined]) {
                const context = {
                    parameters: {EnabledTests: enabled},
                    bodies: {
                        Call_Initialization_Workflow: {baseAddress},
                        Get_SCIMTests_Output_Content: {
                            SCIMUserPaginationTestOutputs: {overallResult: result},
                            SCIMGroupPaginationTestOutputs: {overallResult: result},
                        },
                    },
                };
                const original = structuredClone(context.bodies);
                const aggregate = paginationContext(context).outputs.Compose_Pagination_Model_Results;
                for (const [key, selector] of [['userPaginationResult', 'SCIM_User_Pagination_Test'],
                    ['groupPaginationResult', 'SCIM_Group_Pagination_Test']]) {
                    const exempted = baseAddress?.includes('GOOGLEAPIS.COM') &&
                        ['All', 'SCIMTests', selector].includes(enabled) && result === 'SKIPPED';
                    assert.equal(aggregate[key], exempted ? 'PASSED' : result ?? 'FAILED',
                        `${baseAddress}, ${enabled}, ${key}, ${result}`);
                }
                assert.deepEqual(context.bodies, original);
                assert.equal(aggregate.googleSkipExceptionReason !== null, Boolean(baseAddress?.includes('GOOGLEAPIS.COM')));
            }
        }
    }
});
test('Google pagination aggregate reaches readiness and final results without hiding other failures', () => {
    const bodies = {Call_Initialization_Workflow: {baseAddress: 'https://example.googleapis.com/scim',
        isGroupSupported: true, isManagerAttributeSupported: true, targetDirectoryUserAttributes: [{name: 'manager'}]}};
    for (const file of ['UserTests_Workflow.json', 'GroupTests_Workflow.json', 'SCIMTests_Workflow.json']) {
        const final = load(file).definition.actions.Compose_Final_Results.inputs;
        bodies[`Get_${file.replace('_Workflow.json', '')}_Output_Content`] =
            Object.fromEntries(Object.keys(final).filter(key => key.endsWith('Outputs')).map(key => [key, {overallResult: 'PASSED'}]));
    }
    Object.assign(bodies.Get_SCIMTests_Output_Content, {authenticationModelResult: 'PASSED',
        FederatedIdentityTestOutputs: {overallResult: 'SKIPPED', result: 'SKIPPED'},
        SCIMUserPaginationTestOutputs: {overallResult: 'SKIPPED', result: 'SKIPPED'},
        SCIMGroupPaginationTestOutputs: {overallResult: 'SKIPPED', result: 'SKIPPED'}});
    const context = paginationContext({parameters: {EnabledTests: 'All'}, bodies});
    const overall = parent.Evaluate_Test_Results.inputs.overallLogicAppResult;
    const optional = parent.Evaluate_Test_Results.inputs.optionalSuccessTestsResult;
    assert.equal(evaluate(overall, context), 'Succeeded');
    assert.equal(evaluate(optional, context), 'PASSED');
    const federation = parent.Final_TestResults.inputs.testResults.find(entry => entry.testName === 'Federated_Identity_Test');
    assert.equal(evaluate(federation.testResult, context), 'SKIPPED');
    context.outputs.Evaluate_Test_Results = {paginationModelResults:
        evaluate(parent.Evaluate_Test_Results.inputs.paginationModelResults, context)};
    assert.deepEqual(evaluate(parent.Final_TestResults.inputs.paginationModelResults, context),
        context.outputs.Compose_Pagination_Model_Results);
    for (const name of ['SCIM_User_Pagination_Test', 'SCIM_Group_Pagination_Test']) {
        const result = parent.Final_TestResults.inputs.testResults.find(entry => entry.testName === name);
        assert.equal(evaluate(result.testResult, context), 'SKIPPED');
    }
    bodies.Get_UserTests_Output_Content.CreateUserTestOutputs.overallResult = 'FAILED';
    assert.equal(evaluate(overall, context), 'Failed');
    bodies.Get_UserTests_Output_Content.CreateUserTestOutputs.overallResult = 'PASSED';
    bodies.Poll_SCIMTests_Status = {properties: {status: 'Failed'}};
    assert.equal(evaluate(overall, context), 'Failed');
    delete bodies.Poll_SCIMTests_Status;
    for (const result of ['FAILED', undefined]) {
        bodies.Get_SCIMTests_Output_Content.SCIMUserPaginationTestOutputs = {overallResult: result};
        assert.equal(evaluate(overall, paginationContext(context)), 'Failed');
    }
    bodies.Get_SCIMTests_Output_Content.SCIMUserPaginationTestOutputs = {overallResult: 'SKIPPED'};
    bodies.Get_SCIMTests_Output_Content.SCIMGroupPaginationTestOutputs = {overallResult: 'FAILED'};
    assert.equal(evaluate(overall, paginationContext(context)), 'Succeeded');
    assert.equal(evaluate(optional, context), 'WARNING');
    context.parameters.EnabledTests = 'UserTests';
    assert.equal(evaluate(overall, paginationContext(context)), 'Failed');
    assert.ok(parent.Evaluate_Test_Results.runAfter.Compose_Pagination_Model_Results.includes('Succeeded'));
    assert.ok(parent.Compose_Pagination_Model_Results.runAfter.Get_SCIMTests_Output_Content.includes('Failed'));
});
test('Google empty-update provenance requires a nonempty original payload removed by filtering', () => {
    const expression = load('Initialization_Workflow.json').definition.actions
        .Response_Initialization_Data.inputs.body.googleUpdateBodyEmptiedByFiltering;
    for (const [baseAddress, original, filtered, skipped, expected] of [
        ['https://example.GOOGLEAPIS.COM/scim', {otherMails: ['']}, {}, ['otherMails'], true],
        ['https://example.googleapis.com/scim', {}, {}, ['otherMails'], false],
        ['https://example.googleapis.com/scim', null, {}, ['otherMails'], false],
        ['https://example.googleapis.com/scim', {displayName: 'Updated'}, {displayName: 'Updated'}, [], false],
        ['https://example.googleapis.com/scim', {otherMails: ['']}, {}, [], false],
        ['https://example.googleapis.com/scim', {otherMails: ['']}, null, ['otherMails'], false],
        ['https://scim.example.test', {otherMails: ['']}, {}, ['otherMails'], false],
    ]) {
        assert.equal(evaluate(expression, {
            bodies: {Filter_BaseAddress_Connectivity_Parameter: [{value: baseAddress}]},
            outputs: {Compose_Final_Dynamic_Update_User_Body: original},
            variables: {googleUpdateUserBody: filtered, googleSkippedUpdateSources: skipped},
        }), expected);
    }
});
test('Google filtering prunes only nested containers emptied by removing a mapped source', () => {
    const initialization = load('Initialization_Workflow.json');
    const protection = initialization.definition.actions.Protect_Google_Required_User_Attributes;
    const actions = googleRequiredUpdateActions(initialization);
    const exception = initialization.definition.actions.Response_Initialization_Data.inputs.body.googleUpdateBodyEmptiedByFiltering;
    const source = 'employeeOrgData.costCenter';
    for (const [original, nonemptyValues, expected, exempted] of [
        [{employeeOrgData: {costCenter: ''}}, [], {}, true],
        [{employeeOrgData: {costCenter: null}}, [], {}, true],
        [{employeeOrgData: {costCenter: '', division: 'Engineering'}}, [],
            {employeeOrgData: {division: 'Engineering'}}, false],
        [{employeeOrgData: {costCenter: ''}, displayName: 'Updated'}, [], {displayName: 'Updated'}, false],
        [{employeeOrgData: {}}, [], {employeeOrgData: {}}, false],
        [{displayName: 'Updated'}, [], {displayName: 'Updated'}, false],
        [{employeeOrgData: {costCenter: '123'}}, ['123'], {employeeOrgData: {costCenter: '123'}}, false],
        [{employeeOrgData: {costCenter: false}}, [false], {employeeOrgData: {costCenter: false}}, false],
        [{employeeOrgData: {costCenter: 0}}, [0], {employeeOrgData: {costCenter: 0}}, false],
    ]) {
        const context = {
            items: {ForEach_Google_User_Target: {source}},
            variables: {googleUpdateUserBody: original, googleSkippedUpdateSources: nonemptyValues.length ? [] : [source]},
            bodies: {
                Filter_Google_Nonempty_Update_Values: nonemptyValues,
                Filter_BaseAddress_Connectivity_Parameter: [{value: 'https://example.googleapis.com/scim'}],
            },
            outputs: {Compose_Dynamic_ArrayProperties: [], Compose_Final_Dynamic_Update_User_Body: original},
        };
        assert.equal(evaluate(protection.expression, context), true);
        context.outputs.Compose_Google_Update_Parent_After_Removal =
            evaluate(actions.Compose_Google_Update_Parent_After_Removal.inputs, context);
        const filtered = evaluate(actions.Compose_Google_Protected_Update_Body.inputs, context);
        assert.deepEqual(filtered, expected);
        assert.deepEqual(context.variables.googleUpdateUserBody, original, 'compose must not mutate the input');
        context.variables.googleUpdateUserBody = filtered;
        assert.equal(evaluate(exception, context), exempted);
        const userActions = userUpdateActions(load('UserTests_Workflow.json'));
        assert.equal(evaluate(userActions.Compose_Google_Empty_Update_Exception.inputs, {
            triggerBody: {initializationData: {
                baseAddress: 'https://example.googleapis.com/scim', updateUserBody: filtered,
                googleUpdateBodyEmptiedByFiltering: evaluate(exception, context),
                googleSkippedUpdateSources: context.variables.googleSkippedUpdateSources,
            }},
            variables: {UpdateUserTestOutputs: {overallResult: 'PASSED'}},
            actions: {Set_UpdateUser_CreatePhase_Output_Variables: {status: 'Succeeded'}},
        }), exempted);
        context.bodies.Filter_BaseAddress_Connectivity_Parameter[0].value = 'https://scim.example.test';
        assert.equal(evaluate(protection.expression, context), false, 'other providers do not run this filtering');
        assert.equal(evaluate(exception, context), false);
    }
    assert.deepEqual(actions.Compose_Google_Update_Parent_After_Removal.runAfter,
        {Filter_Google_Nonempty_Update_Values: ['Succeeded']});
    assert.deepEqual(actions.Compose_Google_Protected_Update_Body.runAfter,
        {Compose_Google_Update_Parent_After_Removal: ['Succeeded']});
});
test('Google filtering-only empty updates pass explicitly without executing the update phase', () => {
    const actions = userUpdateActions(load('UserTests_Workflow.json'));
    const context = {
        triggerBody: {initializationData: {
            baseAddress: 'https://example.GOOGLEAPIS.COM/scim',
            updateUserBody: {}, googleUpdateBodyEmptiedByFiltering: true,
            googleSkippedUpdateSources: ['otherMails'],
        }},
        variables: {UpdateUserTestOutputs: {overallResult: 'PASSED'}},
        actions: {Set_UpdateUser_CreatePhase_Output_Variables: {status: 'Succeeded'}},
        outputs: {},
    };
    const check = current => {
        current.outputs.Compose_Google_Empty_Update_Exception =
            evaluate(actions.Compose_Google_Empty_Update_Exception.inputs, current);
        return {
            update: evaluate(actions.Update_User_Check_Create_Phase_Status.expression, current),
            exception: evaluate(actions.Apply_Google_Empty_Update_Exception.expression, current),
        };
    };
    assert.deepEqual(check(context), {update: false, exception: true});
    const result = actions.Apply_Google_Empty_Update_Exception.actions.Set_Google_Empty_Update_Outputs.inputs.value;
    assert.equal(result.overallResult, 'PASSED');
    assert.match(result.result, /^success.*Google exception.*no update PATCH or update verification was executed/);
    for (const [key, value] of [
        ['baseAddress', 'https://scim.example.test'],
        ['googleUpdateBodyEmptiedByFiltering', false],
        ['googleUpdateBodyEmptiedByFiltering', undefined],
        ['googleSkippedUpdateSources', []],
        ['updateUserBody', {displayName: 'Updated'}],
        ['updateUserBody', null],
    ]) {
        const current = structuredClone(context);
        current.triggerBody.initializationData[key] = value;
        assert.deepEqual(check(current), {update: true, exception: false}, key);
    }
    for (const overallResult of ['FAILED', 'SKIPPED']) {
        const current = structuredClone(context);
        current.variables.UpdateUserTestOutputs.overallResult = overallResult;
        assert.deepEqual(check(current), {update: false, exception: false});
    }
    const failedSetup = structuredClone(context);
    failedSetup.actions.Set_UpdateUser_CreatePhase_Output_Variables.status = 'Failed';
    assert.equal(check(failedSetup).exception, false);
    assert.deepEqual(actions.Update_User_Check_Create_Phase_Status.runAfter,
        {Compose_Google_Empty_Update_Exception: ['Succeeded']});
    assert.deepEqual(actions.Compose_Google_Empty_Update_Exception.runAfter,
        {Set_UpdateUser_CreatePhase_Output_Variables: ['Succeeded', 'Failed', 'Skipped']});
    assert.deepEqual(actions.Apply_Google_Empty_Update_Exception.runAfter,
        {Update_User_Check_Create_Phase_Status: ['Succeeded', 'Skipped']});
    assert.deepEqual(actions.Update_Cleanup_Delete_User.runAfter,
        {Apply_Google_Empty_Update_Exception: ['Succeeded', 'Failed', 'TimedOut', 'Skipped']});
    const guard = actions.Update_User_Check_Create_Phase_Status.actions.Update_User_Verify_Update_And_Provisioning
        .actions.Validate_User_Update_Attributes.actions.Validate_Google_User_Update_Attributes;
    assert.equal(guard.inputs.schema.minProperties, 1, 'non-exempt empty Google updates still fail');
});
test('token protocols and unrelated semantic source remain unchanged; naming and bundle version agree', () => {
    const old = baseline('SCIMTests_Workflow.json').definition.actions;
    for (const name of Object.keys(old.SCIMTests_Scope.actions)) {
        const actual = structuredClone(child.SCIMTests_Scope.actions[name]);
        // Permit the two new selection gates, but no changes inside their protocol actions.
        if (['Validate_Credentials_Test', 'Federated_Identity_Test'].includes(name)) actual.expression = old.SCIMTests_Scope.actions[name].expression;
        assert.deepEqual(actual, old.SCIMTests_Scope.actions[name], name);
    }
    const initialization = load('Initialization_Workflow.json');
    delete initialization.definition.actions.Response_Initialization_Data.inputs.body.googleUpdateBodyEmptiedByFiltering;
    const protection = googleRequiredUpdateActions(initialization);
    delete protection.Compose_Google_Update_Parent_After_Removal;
    protection.Compose_Google_Protected_Update_Body =
        googleRequiredUpdateActions(baseline('Initialization_Workflow.json')).Compose_Google_Protected_Update_Body;
    assert.deepEqual(initialization, baseline('Initialization_Workflow.json'));
    const users = load('UserTests_Workflow.json');
    const updated = userUpdateActions(users);
    const original = userUpdateActions(baseline('UserTests_Workflow.json'));
    delete updated.Compose_Google_Empty_Update_Exception;
    delete updated.Apply_Google_Empty_Update_Exception;
    updated.Update_User_Check_Create_Phase_Status.expression = original.Update_User_Check_Create_Phase_Status.expression;
    updated.Update_User_Check_Create_Phase_Status.runAfter = original.Update_User_Check_Create_Phase_Status.runAfter;
    updated.Update_Cleanup_Delete_User.runAfter = original.Update_Cleanup_Delete_User.runAfter;
    assert.deepEqual(users, baseline('UserTests_Workflow.json'));
    assert.deepEqual(load('GroupTests_Workflow.json'), baseline('GroupTests_Workflow.json'));
    // VERSION is authoritative; future releases should not require a test-only version edit.
    const version = fs.readFileSync(path.join(root, 'VERSION'), 'utf8').trim();
    assert.notEqual(version, '', 'VERSION must contain a bundle version');
    assert.equal(load('Orchestrator_Parameters.json').LogicAppVersion.value, '');
    assert.equal(load('Orchestrator_Parameters.json').selectedAuthenticationMethod.value, '');
    for (const name of ['ValidateLogicAppRun-Standard.ps1']) {
        assert.equal(fs.readFileSync(path.join(root, name), 'utf8').replace(/\r\n/g, '\n'),
            execFileSync('git', ['show', `8bdf02e:Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp/${name}`], {cwd: root, encoding: 'utf8'}).replace(/\r\n/g, '\n'));
    }
    for (const name of ['scim-onboarding.agent.md', 'SetupLogicApp-Standard-Agent.md', 'SCIM-Validation-Test-Overview.md']) {
        const text = fs.readFileSync(path.join(root, name), 'utf8');
        assert.ok(text.includes('`FederatedIdentity`'), name);
        assert.ok(!text.includes('`WorkloadIdentityFederation`'), name);
    }
    assert.equal(parent.Final_TestResults.inputs.selectedAuthenticationMethod, "@parameters('selectedAuthenticationMethod')");
});

test('final result uses the deployed version and two single-capture validation timestamps', () => {
    const final = parent.Final_TestResults.inputs;
    assert.equal(final.LogicAppVersion, "@parameters('LogicAppVersion')");
    assert.equal(parent.Compose_Debug_WorkflowObject.inputs.validationStartedAtUtc, '@utcNow()');
    assert.equal(parent.Evaluate_Test_Results.inputs.validationCompletedAtUtc, '@utcNow()');
    for (const [start, end, seconds] of [
        ['2026-10-21T13:00:00Z', '2026-10-21T13:33:12Z', 1992],
        ['2026-10-21T23:59:59.9000000Z', '2026-10-22T00:00:01.1000000Z', 1],
    ]) {
        const context = {parameters: {LogicAppVersion: 'future-version'}, outputs: {
            Compose_Debug_WorkflowObject: {validationStartedAtUtc: start},
            Evaluate_Test_Results: {validationCompletedAtUtc: end},
        }};
        assert.equal(evaluate(final.LogicAppVersion, context), 'future-version');
        assert.equal(evaluate(final.validationStartedAtUtc, context), start);
        assert.equal(evaluate(final.validationCompletedAtUtc, context), end);
        assert.equal(evaluate(final.validationStartedAtEpochSeconds, context), Math.floor(Date.parse(start) / 1000));
        assert.equal(evaluate(final.validationCompletedAtEpochSeconds, context), Math.floor(Date.parse(end) / 1000));
        assert.equal(evaluate(final.validationDurationSeconds, context), seconds);
    }
});

test('PowerShell deployment derives version without modifying source parameters', () => {
    const output = execFileSync('pwsh', ['-NoProfile', '-File', path.join(__dirname, 'version-deployment.test.ps1')],
        {cwd: root, encoding: 'utf8'});
    assert.ok(output.includes('Version deployment checks passed'), output);
});
