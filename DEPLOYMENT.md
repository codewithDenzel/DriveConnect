# DriveConnect Deployment Preparation

## Local development

DriveConnect uses LocalDB for local development. Production database credentials must not be stored in the Git repository.

## MonsterASP production configuration

Set the following environment variables in the MonsterASP control panel:

- ASPNETCORE_ENVIRONMENT = Production
- ConnectionStrings__DefaultConnection = the production master database connection string
- TenantCredentials__Tenant1__UserId = the tenant database username
- TenantCredentials__Tenant1__Password = the tenant database password

ASP.NET Core maps double underscores in environment variable names to nested configuration keys. For example:

TenantCredentials__Tenant1__Password

maps to:

TenantCredentials:Tenant1:Password

After changing environment variables, restart the website.

## Publishing

1. Create or select the MonsterASP website.
2. Activate WebDeploy in the MonsterASP control panel.
3. Download the WebDeploy publishing profile.
4. In Visual Studio, right-click DriveConnect.api and select Publish.
5. Import the MonsterASP WebDeploy profile.
6. Publish DriveConnect.api.
7. Configure the production environment variables.
8. Restart the website.
9. Open the deployed /health endpoint and verify that the API returns status ok.

Never commit SQL usernames, SQL passwords, WebDeploy passwords, publish settings, or production connection strings containing passwords.


## Offline sync configuration

The local API stores CRM changes in the local tenant database first. When internet access is restored, the local API can upload queued changes to the deployed MonsterASP API.

For the local API, configure:

- Sync__Enabled = true
- Sync__RemoteBaseUrl = the deployed MonsterASP API URL
- Sync__SharedSecret = a private shared value

For the MonsterASP API, configure:

- Sync__Enabled = false
- Sync__SharedSecret = the same private shared value

The shared secret is sent in the X-DriveConnect-Sync-Key header. Do not commit the shared secret to GitHub.
