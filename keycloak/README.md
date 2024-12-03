## Authentication Flow

This project implements the Direct Access Grants flow. This enables support for Direct Access Grants, which means that the client has access to the username/password of the user and exchanges it directly with the Keycloak server for an access token. In terms of OAuth2 specification, this enables support of 'Resource Owner Password Credentials Grant' for this client.

oc process -f https://raw.githubusercontent.com/keycloak/keycloak-quickstarts/latest/openshift/keycloak.yaml `
    -p KEYCLOAK_ADMIN=admin `
    -p KEYCLOAK_ADMIN_PASSWORD=admin `
    -p NAMESPACE=keycloak | oc create -f -


$KEYCLOAK_URL = "https://$(oc get route keycloak --template='{{ .spec.host }}')"
Write-Output ""
Write-Output "Keycloak:                 $KEYCLOAK_URL"
Write-Output "Keycloak Admin Console:   $KEYCLOAK_URL/admin"
Write-Output "Keycloak Account Console: $KEYCLOAK_URL/realms/myrealm/account"
Write-Output ""
