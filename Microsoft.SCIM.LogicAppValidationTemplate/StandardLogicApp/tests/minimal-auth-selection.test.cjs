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
const root = path.resolve(__dirname, '..');
const load = name => JSON.parse(fs.readFileSync(path.join(root, name), 'utf8'));
// Pin the pre-feature master revision so preservation checks do not move with master.
const baseline = name => JSON.parse(execFileSync('git', ['show', `8bdf02e:Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp/${name}`], {cwd: root, encoding: 'utf8', maxBuffer: 20e6}));
const child = load('SCIMTests_Workflow.json').definition.actions;
const parent = load('Orchestrator_Workflow.json').definition.actions;

// Evaluate only the expression subset used below, with simulated action outputs.
// This is not a general interpreter: it does not model runtime scheduling or lazy branches.
function evaluate(expression, context) {
    if (typeof expression === 'object') {
        const [op, args] = Object.entries(expression)[0];
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
        equals: (a, b) => a === b, not: value => !value, and: (...args) => args.every(Boolean),
        or: (...args) => args.some(Boolean), if: (condition, yes, no) => condition ? yes : no,
        coalesce: (...args) => args.find(value => value != null), json: JSON.parse,
        string: value => typeof value === 'object' ? JSON.stringify(value) : String(value),
        contains: (value, item) => value?.includes(item) || false,
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
            const match = /^[A-Za-z_]\w*/.exec(source.slice(position));
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
            } else result = {true: true, false: false, null: null}[match[0]];
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
test('only the selected existing branch runs; EnabledTests remains honored', () => {
    for (const method of [...methods, '', 'invalid']) {
        for (const enabled of ['All', 'SCIMTests', 'UserTests', 'Validate_Credentials_Test', 'Federated_Identity_Test']) {
            const context = {parameters: {selectedAuthenticationMethod: method}, triggerBody: {EnabledTests: enabled}};
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
            const actual = evaluate(child.Compose_Final_Results.inputs.authenticationModelResult, {parameters: {selectedAuthenticationMethod: method}, variables});
            assert.equal(actual === 'PASSED', outcome === 'PASSED');
        }
    }
    assert.equal(evaluate(child.Compose_Final_Results.inputs.authenticationModelResult, {parameters: {selectedAuthenticationMethod: ''}}), 'FAILED');
});
test('parent requires selected PASSED while preserving independent semantic failures', () => {
    const original = baseline('Orchestrator_Workflow.json').definition.actions.Evaluate_Test_Results.inputs.overallLogicAppResult;
    const current = parent.Evaluate_Test_Results.inputs.overallLogicAppResult;
    // Reversing only the authentication predicate must recover the original semantic gates.
    assert.equal(current.replace("not(equals(coalesce(body('Get_SCIMTests_Output_Content')?['authenticationModelResult'],'FAILED'),'PASSED'))",
        "equals(coalesce(body('Get_SCIMTests_Output_Content')?['authenticationModelResult'],'FAILED'),'FAILED')"), original);
    const outputs = {};
    for (const file of ['UserTests_Workflow.json', 'GroupTests_Workflow.json', 'SCIMTests_Workflow.json']) {
        const final = load(file).definition.actions.Compose_Final_Results.inputs;
        outputs[`Get_${file.replace('_Workflow.json', '')}_Output_Content`] =
            Object.fromEntries(Object.keys(final).filter(key => key.endsWith('Outputs')).map(key => [key, {overallResult: 'PASSED'}]));
    }
    const context = {bodies: {...outputs, Call_Initialization_Workflow: {isGroupSupported: true}, Poll_SCIMTests_Status: {properties: {status: 'Succeeded'}}}};
    for (const result of ['PASSED', 'FAILED', 'SKIPPED', undefined]) {
        context.bodies.Get_SCIMTests_Output_Content.authenticationModelResult = result;
        assert.equal(evaluate(current, context), result === 'PASSED' ? 'Succeeded' : 'Failed');
    }
    context.bodies.Get_SCIMTests_Output_Content.authenticationModelResult = 'PASSED';
    context.bodies.Get_UserTests_Output_Content.CreateUserTestOutputs.overallResult = 'FAILED';
    assert.equal(evaluate(current, context), 'Failed');
});
test('token protocols and semantic source remain unchanged; naming and bundle version agree', () => {
    const old = baseline('SCIMTests_Workflow.json').definition.actions;
    for (const name of Object.keys(old.SCIMTests_Scope.actions)) {
        const actual = structuredClone(child.SCIMTests_Scope.actions[name]);
        // Permit the two new selection gates, but no changes inside their protocol actions.
        if (['Validate_Credentials_Test', 'Federated_Identity_Test'].includes(name)) actual.expression = old.SCIMTests_Scope.actions[name].expression;
        assert.deepEqual(actual, old.SCIMTests_Scope.actions[name], name);
    }
    for (const name of ['Initialization_Workflow.json', 'UserTests_Workflow.json', 'GroupTests_Workflow.json']) assert.deepEqual(load(name), baseline(name));
    // VERSION is authoritative; future releases should not require a test-only version edit.
    const version = fs.readFileSync(path.join(root, 'VERSION'), 'utf8').trim();
    assert.notEqual(version, '', 'VERSION must contain a bundle version');
    assert.equal(load('Orchestrator_Parameters.json').LogicAppVersion.value, version);
    assert.equal(load('Orchestrator_Parameters.json').selectedAuthenticationMethod.value, '');
    for (const name of ['Deploy-LogicAppWorkflows.ps1', 'ValidateLogicAppRun-Standard.ps1']) {
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
