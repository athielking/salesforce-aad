# End to End Enterprise Salesforce and Azure AD

This is a setup guide to integrating your enterprises custom web apps, API's, Azure Active Directory, and Salesforce Organization

## App 1 - Single Page App Client

Found under `/spa`. This is an angular app which uses `@azure/msal-angular` to authenticate with Azure Active Directory using OAuth and the Implicit Flow.

<a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/media/tutorial-v2-angular/diagram-auth-flow-spa-angular.svg" rel="implicit-flow">![implicit-flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/media/tutorial-v2-angular/diagram-auth-flow-spa-angular.svg)

[Reference](https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-angular)

## App 2 - .NET Core Web API

Found under `/SpaApi`. This is a .NET Core api which uses the MSAL to authorize users against its endpoints from Azure Active Directory.

Once the user is authorized to the web api.  The app then generates a new access token to salesforce using the [JWT Bearer](https://help.salesforce.com/articleView?id=remoteaccess_oauth_jwt_flow.htm&type=5) flow.

Once the salesforce access token is granted, the web API is able to access the protected salesforce api.

## App 3 - Salesforce Scratch Org

Found under `/azure-ad` this is the SFDX project definition for the Salesforce Scratch Org. This contains all of the necessary config files for setting up SSO and creating the Connected App which the `SpaApi` connects to Salesforce through.

- [Create A Connected App](https://help.salesforce.com/articleView?id=connected_app_create.htm&type=5)
- [REST API Guide](https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/intro_what_is_rest_api.htm)


## Unstructured Notes
- Both the SpaClient and SpaApi projects must be registered within Azure AD.
    - [Single Page App](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-spa-overview)
    - [Web Api](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-overview)
    
- There is an additional OAuth flow for the WebApi which may be useful in the future for connecting to other api's or azure sql db's.  This is the 'On-behalf-of flow' and uses your previously access token coming in from the client, and then exchanges it for an access token to a downstream API (similar to the kerberos 'double hop') 

<a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/media/v2-oauth2-on-behalf-of-flow/protocols-oauth-on-behalf-of-flow.png" rel="on-behalf-of-flow">![on-behalf-of-flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/media/v2-oauth2-on-behalf-of-flow/protocols-oauth-on-behalf-of-flow.png)

- [Reference](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-api-call-api-overview)
- [On Behalf of Flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)

- Helpful example of On-Behalf-Of Flow for Azure Sql [https://github.com/Dayzure/AzureSQLDelegatedAuth](https://github.com/Dayzure/AzureSQLDelegatedAuth)
- The SAML Single Sign on settings will probably change if you use this to replicate a setup for yourself.  Scratch org names are generated at at random so everywhere you see my org domain `ability-enterprise-9701-dev-ed.cs68` you'll need to replace it with your own.
- [Setup Salesforce SSO](https://docs.microsoft.com/en-us/azure/active-directory/saas-apps/salesforce-tutorial)
- In order to get the JWT Bearer flow to work properly with the Salesforce API you need to sign the JWT's you create on your api with a certificate. Basic steps for the cert are below.
    - You Generate a `.cer` file and a `.key` file.
    - `openssl req -x509 -newkey rsa:4096 -sha256 -keyout salesforce.key -out salesforce.crt -subj "/CN=localhost" -days 600`
    - For windows, it doesn't like the two separate files so you need to combine them in to a `.pfx` file
    - `openssl pkcs12 -export -name "localhost" -out salesforce.pfx -inkey salesforce.key -in salesforce.crt`
    - You'll use the `.pfx` file on the api as it contains the private key which you'll sign your JWT's with.
    - The `.cer` file is uploaded to the Salesforce Connected app and used to verify the server sending the token to it.
