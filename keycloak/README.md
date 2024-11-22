## Authentication Flow

This project implements the Direct Access Grants flow. This enables support for Direct Access Grants, which means that the client has access to the username/password of the user and exchanges it directly with the Keycloak server for an access token. In terms of OAuth2 specification, this enables support of 'Resource Owner Password Credentials Grant' for this client.