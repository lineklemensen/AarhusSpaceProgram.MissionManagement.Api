IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = 'ASP')
    BEGIN
        CREATE LOGIN [ASP] WITH PASSWORD = 'MaAs3P4$$';
    END
GO
USE [MissionManagementASP];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'ASP')
    BEGIN
        CREATE USER [ASP] FOR LOGIN [ASP];
        ALTER ROLE db_owner ADD MEMBER [ASP];
    END
GO