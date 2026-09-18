IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Companies] (
    [CompanyId] int NOT NULL IDENTITY,
    [CompanyCode] nvarchar(50) NOT NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([CompanyId])
);

CREATE TABLE [AppUsers] (
    [UserId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [Username] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AppUsers] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_AppUsers_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [CompanyDatabases] (
    [CompanyDatabaseId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [ServerName] nvarchar(200) NOT NULL,
    [DatabaseName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_CompanyDatabases] PRIMARY KEY ([CompanyDatabaseId]),
    CONSTRAINT [FK_CompanyDatabases_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [Devices] (
    [DeviceId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [DeviceCode] nvarchar(50) NOT NULL,
    [DeviceName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Devices] PRIMARY KEY ([DeviceId]),
    CONSTRAINT [FK_Devices_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [Subscriptions] (
    [SubscriptionId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [PlanName] nvarchar(100) NOT NULL,
    [MonthlyFee] decimal(18,2) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([SubscriptionId]),
    CONSTRAINT [FK_Subscriptions_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AppUsers_CompanyId] ON [AppUsers] ([CompanyId]);

CREATE UNIQUE INDEX [IX_AppUsers_Email] ON [AppUsers] ([Email]);

CREATE UNIQUE INDEX [IX_Companies_CompanyCode] ON [Companies] ([CompanyCode]);

CREATE INDEX [IX_CompanyDatabases_CompanyId] ON [CompanyDatabases] ([CompanyId]);

CREATE UNIQUE INDEX [IX_Devices_CompanyId_DeviceCode] ON [Devices] ([CompanyId], [DeviceCode]);

CREATE INDEX [IX_Subscriptions_CompanyId] ON [Subscriptions] ([CompanyId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260908133645_InitialMasterDriveConnect', N'10.0.11');

COMMIT;
GO

