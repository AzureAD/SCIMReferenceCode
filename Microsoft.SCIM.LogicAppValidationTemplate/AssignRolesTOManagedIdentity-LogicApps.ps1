$miObjId = ""
$graphAppId="00000003-0000-0000-c000-000000000000"
$graphSpId = az ad sp list --filter "appId eq '$graphAppId'" --query "[0].id" -o tsv 

$roleValue="Application.Read.All"
$roleId = az ad sp show --id $graphSpId --query "appRoles[?value=='$roleValue'].id" -o tsv
$body = @{ principalId=$miObjId; resourceId=$graphSpId; appRoleId=$roleId } | ConvertTo-Json
az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$miObjId/appRoleAssignments" --headers "Content-Type=application/json" --body "$body"

$roleValue="User.DeleteRestore.All"
$roleId = az ad sp show --id $graphSpId --query "appRoles[?value=='$roleValue'].id" -o tsv
$body = @{ principalId=$miObjId; resourceId=$graphSpId; appRoleId=$roleId } | ConvertTo-Json
az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$miObjId/appRoleAssignments" --headers "Content-Type=application/json" --body "$body"

$roleValue="Synchronization.ReadWrite.All"
$roleId = az ad sp show --id $graphSpId --query "appRoles[?value=='$roleValue'].id" -o tsv
$body = @{ principalId=$miObjId; resourceId=$graphSpId; appRoleId=$roleId } | ConvertTo-Json
az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$miObjId/appRoleAssignments" --headers "Content-Type=application/json" --body "$body"
$roleValue="AppRoleAssignment.ReadWrite.All"
$roleId = az ad sp show --id $graphSpId --query "appRoles[?value=='$roleValue'].id" -o tsv
$body = @{ principalId=$miObjId; resourceId=$graphSpId; appRoleId=$roleId } | ConvertTo-Json
az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$miObjId/appRoleAssignments" --headers "Content-Type=application/json" --body "$body"
$roleValue="AuditLog.Read.All"
$roleId = az ad sp show --id $graphSpId --query "appRoles[?value=='$roleValue'].id" -o tsv
$body = @{ principalId=$miObjId; resourceId=$graphSpId; appRoleId=$roleId } | ConvertTo-Json
az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$miObjId/appRoleAssignments" --headers "Content-Type=application/json" --body "$body"
$roleValue="User.ReadWrite.All"
$roleId = az ad sp show --id $graphSpId --query "appRoles[?value=='$roleValue'].id" -o tsv
$body = @{ principalId=$miObjId; resourceId=$graphSpId; appRoleId=$roleId } | ConvertTo-Json
az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$miObjId/appRoleAssignments" --headers "Content-Type=application/json" --body "$body"
$roleValue="Group.ReadWrite.All"
$roleId = az ad sp show --id $graphSpId --query "appRoles[?value=='$roleValue'].id" -o tsv
$body = @{ principalId=$miObjId; resourceId=$graphSpId; appRoleId=$roleId } | ConvertTo-Json
az rest --method POST --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$miObjId/appRoleAssignments" --headers "Content-Type=application/json" --body "$body"
