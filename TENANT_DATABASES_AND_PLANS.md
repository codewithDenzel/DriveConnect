# DriveConnect tenant databases and plans

## Database layout

DriveConnect uses one master database for system-wide records and one separate tenant database for each company.

- **Master database**: companies, company-to-database mappings, users, branches, and subscriptions.
- **Tenant A database**: CRM data for the Basic-plan company.
- **Tenant B database**: CRM data for the Pro-plan company.
- **Tenant C database**: CRM data for the Pro Max-plan company.

The API resolves the tenant database from the company's active `CompanyDatabases` row. It does not choose a tenant database from a name supplied by the user. Every company must have its own mapping, with the correct database server and database name.

## Hosting setup still required

The code cannot create databases inside the hosting provider's control panel. Create the four databases there first and use separate database users/permissions where the host supports them. The master database should not be the same physical database as a tenant database.

Apply the `TenantDriveConnectDbContext` migrations to each tenant database and the `MasterDriveConnectDbContext` migrations to the master database. Do not apply tenant migrations to the master database. Keep a backup before moving existing data.

After provisioning the databases, update each company's `CompanyDatabases` row in the **master database** so its `CompanyId` points to its own tenant database. Example only; replace the company IDs and database names with the real values:

```sql
SELECT CompanyId, ServerName, DatabaseName, IsActive
FROM dbo.CompanyDatabases
ORDER BY CompanyId;
```

Expected mapping concept:

| Company | CompanyId (example only) | DatabaseName (example only) | Plan |
|---|---:|---|---|
| Tenant A | 1 | DriveConnectTenantA | Basic |
| Tenant B | 2 | DriveConnectTenantB | Pro |
| Tenant C | 3 | DriveConnectTenantC | Pro Max |

The Super Admin connects to the master database; it does not need a tenant CRM database.

## Connection credentials

The API uses `ConnectionStrings__DefaultConnection` for the master database. Tenant connection credentials are looked up by company ID using these settings:

- `TenantCredentials__Tenant1__UserId` and `TenantCredentials__Tenant1__Password`
- `TenantCredentials__Tenant2__UserId` and `TenantCredentials__Tenant2__Password`
- `TenantCredentials__Tenant3__UserId` and `TenantCredentials__Tenant3__Password`

The numbers must match each company's actual `CompanyId`; if the company IDs differ, configure the matching `Tenant{CompanyId}` credentials instead. Use the same tenant database's server and name as configured in `CompanyDatabases`. Do not commit real passwords or connection strings.

The background sync worker also resolves the cloud tenant database by company ID. Once databases are separated, configure the matching tenant credentials for each company on the API host as well.

## Plan feature rules

- **Basic**: static dashboard, CRM data collection, and exportable reports. No promotions, branching management, or Business Intelligence analytics.
- **Pro**: Basic features plus promotions. No branching management or Business Intelligence analytics.
- **Pro Max**: all tenant features, including promotions, Business Intelligence analytics, and branching management.

The server publishes these flags through `GET /tenant/{companyId}/features`. Promotion endpoints reject Basic-plan tenants, and branch create/update/delete endpoints reject plans without branching. The WinForms app reads the same flags to hide unavailable navigation items.

A tenant without a current active subscription defaults to the Basic feature set. Existing CRM data endpoints remain available; this is a plan feature gate, not a data deletion.

## Provisioning caution

The current hosted database may still contain master tables and tenant tables together from the previous setup. Do not change production mappings until the master database and each tenant database have been created, migrations applied, and any needed records copied and verified. Keep the existing database as a recoverable backup during the move.
