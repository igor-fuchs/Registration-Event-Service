-- =============================================================================
-- SQL Server initialization script for local development
-- =============================================================================

-- 1. Create the database
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'RegistrationEventService')
BEGIN
    CREATE DATABASE [RegistrationEventService];
    PRINT '✔ Database [RegistrationEventService] created.';
END
ELSE
    PRINT '⏭ Database [RegistrationEventService] already exists.';
GO

USE [RegistrationEventService];
GO

-- 2. Create login and database user (service_login / SenhaForte@123)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'service_login')
BEGIN
    CREATE LOGIN [service_login]
        WITH PASSWORD = N'SenhaForte@123',
             DEFAULT_DATABASE = [RegistrationEventService],
             CHECK_POLICY = OFF,
             CHECK_EXPIRATION = OFF;
    PRINT '✔ Login [service_login] created.';
END
ELSE
    PRINT '⏭ Login [service_login] already exists.';
GO

USE [RegistrationEventService];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'service_login')
BEGIN
    CREATE USER [service_login] FOR LOGIN [service_login];
    PRINT '✔ Database user [service_login] created.';
END
ELSE
    PRINT '⏭ Database user [service_login] already exists.';
GO

-- 3. Assign roles db_datareader and db_datawriter
-- -----------------------------------------------------------------------------
ALTER ROLE [db_datareader] ADD MEMBER [service_login];
ALTER ROLE [db_datawriter] ADD MEMBER [service_login];
PRINT '✔ Roles db_datareader and db_datawriter assigned to [service_login].';
GO

-- 4. Create the [auth] schema
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'auth')
BEGIN
    EXEC('CREATE SCHEMA [auth] AUTHORIZATION [dbo]');
    PRINT '✔ Schema [auth] created.';
END
ELSE
    PRINT '⏭ Schema [auth] already exists.';
GO

-- Grant service_login permissions on the auth schema
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[auth] TO [service_login];
PRINT '✔ Permissions on [auth] schema granted to [service_login].';
GO

-- 4.1 Create the [catalog] schema
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'catalog')
BEGIN
    EXEC('CREATE SCHEMA [catalog] AUTHORIZATION [dbo]');
    PRINT '✔ Schema [catalog] created.';
END
ELSE
    PRINT '⏭ Schema [catalog] already exists.';
GO

-- Grant service_login permissions on the catalog schema
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[catalog] TO [service_login];
PRINT '✔ Permissions on [catalog] schema granted to [service_login].';
GO

-- 5. Create table [auth].[Users]
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t
               JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = N'auth' AND t.name = N'Users')
BEGIN
    CREATE TABLE [auth].[Users]
    (
        [Id]        INT            IDENTITY(1,1) NOT NULL,
        [Name]      NVARCHAR(100)  NOT NULL,
        [Email]     NVARCHAR(255)  NOT NULL,
        [CreatedAt] DATETIME2      NOT NULL,

        CONSTRAINT [PK_Users]     PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Users_Email] UNIQUE ([Email])
    );
    PRINT '✔ Table [auth].[Users] created.';
END
ELSE
    PRINT '⏭ Table [auth].[Users] already exists.';
GO

-- 6. Seed two sample users
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [auth].[Users] WHERE [Email] = N'alice@example.com')
BEGIN
    INSERT INTO [auth].[Users] ([Name], [Email], [CreatedAt])
    VALUES (N'Alice Johnson', N'alice@example.com', SYSUTCDATETIME());
    PRINT '✔ User "Alice Johnson" seeded.';
END
ELSE
    PRINT '⏭ User "Alice Johnson" already exists.';

IF NOT EXISTS (SELECT 1 FROM [auth].[Users] WHERE [Email] = N'bob@example.com')
BEGIN
    INSERT INTO [auth].[Users] ([Name], [Email], [CreatedAt])
    VALUES (N'Bob Smith', N'bob@example.com', SYSUTCDATETIME());
    PRINT '✔ User "Bob Smith" seeded.';
END
ELSE
    PRINT '⏭ User "Bob Smith" already exists.';
GO

-- 7. Create table [catalog].[Products]
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t
               JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = N'catalog' AND t.name = N'Products')
BEGIN
    CREATE TABLE [catalog].[Products]
    (
        [Id]          INT            IDENTITY(1,1) NOT NULL,
        [Name]        NVARCHAR(150)  NOT NULL,
        [Sku]         NVARCHAR(64)   NOT NULL,
        [Supplier]    NVARCHAR(150)  NOT NULL,
        [Price]       DECIMAL(18,2)  NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [CreatedAt]   DATETIME2      NOT NULL,

        CONSTRAINT [PK_Products]       PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Products_Sku]   UNIQUE ([Sku])
    );
    PRINT '✔ Table [catalog].[Products] created.';
END
ELSE
    PRINT '⏭ Table [catalog].[Products] already exists.';
GO

-- 8. Seed two sample products
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [catalog].[Products] WHERE [Sku] = N'SKU-0001')
BEGIN
    INSERT INTO [catalog].[Products]
        ([Name], [Sku], [Supplier], [Price], [Description], [CreatedAt])
    VALUES
        (N'Wireless Mouse', N'SKU-0001', N'Northwind Supplies', 49.90, N'Ergonomic wireless mouse', SYSUTCDATETIME());
    PRINT '✔ Product "Wireless Mouse" seeded.';
END
ELSE
    PRINT '⏭ Product "Wireless Mouse" already exists.';

IF NOT EXISTS (SELECT 1 FROM [catalog].[Products] WHERE [Sku] = N'SKU-0002')
BEGIN
    INSERT INTO [catalog].[Products]
        ([Name], [Sku], [Supplier], [Price], [Description], [CreatedAt])
    VALUES
        (N'USB-C Hub', N'SKU-0002', N'Contoso Hardware', 129.00, N'7-in-1 USB-C hub', SYSUTCDATETIME());
    PRINT '✔ Product "USB-C Hub" seeded.';
END
ELSE
    PRINT '⏭ Product "USB-C Hub" already exists.';
GO

PRINT '====================================================================';
PRINT '  Database initialization completed successfully!';
PRINT '====================================================================';
GO
