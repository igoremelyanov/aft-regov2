## Sql server session store initialization

**InitializeSQLServerSessionStore.sql** is a script for creating all necessary objects in database to enable using that database as a storage for sessions data.

To regenerate that file following command may be used:
_C:/Windows/Microsoft.NET/Framework/v4.0.30319/aspnet_regsql.exe -d \_\_DATABASENAME\_\_ -ssadd -sstype c -sqlexportonly C:/Projects/aft-regov2/Infrastructure/Infrastructure/DataAccess/SessionStorage/InitializeSQLServerSessionStore.sql_

**NB**: 
1) you may need to replace pathes to aspnet_regsql.exe and InitializeSQLServerSessionStore.sql accordingly to your environment
2) \_\_DATABASENAME\_\_ will be replaced with actual database name by ApplicationSeeder when seeding, so do not change it manually