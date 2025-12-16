-- Create database
IF DB_ID('AzureEntraTest') IS NULL
BEGIN
    CREATE DATABASE AzureEntraTest;
END
GO

USE AzureEntraTest;
GO

-- Roles table
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        RoleId   INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(100) NOT NULL UNIQUE
    );
END
GO

-- Users table
IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId        INT IDENTITY(1,1) PRIMARY KEY,
        UserName      NVARCHAR(100) NOT NULL UNIQUE,
        FirstName     NVARCHAR(100) NOT NULL,
        LastName      NVARCHAR(100) NOT NULL,
        Email         NVARCHAR(256) NOT NULL,
        SecretId      NVARCHAR(100) NULL,
        ClientId      NVARCHAR(100) NULL,
        AzureObjectId UNIQUEIDENTIFIER NULL,
        IsActive      BIT NOT NULL DEFAULT(1),
        CONSTRAINT UX_Users_Email UNIQUE (Email)
    );
END
GO

-- UserRoles table
IF OBJECT_ID('dbo.UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)
            REFERENCES dbo.Roles(RoleId)
    );
END
GO

-- AzureGroupToRole table (optional mapping Azure groups -> app roles)
IF OBJECT_ID('dbo.AzureGroupToRole', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AzureGroupToRole
    (
        AzureGroupToRoleId INT IDENTITY(1,1) PRIMARY KEY,
        AzureGroupId       UNIQUEIDENTIFIER NOT NULL,
        RoleId             INT NOT NULL
            CONSTRAINT FK_AzureGroupToRole_Roles
            REFERENCES dbo.Roles(RoleId)
    );
END
GO

-- Seed Roles (SuperAdmin, Admin, + 16 roles)
IF NOT EXISTS (SELECT 1 FROM dbo.Roles)
BEGIN
    INSERT INTO dbo.Roles (RoleName)
    VALUES
        ('SuperAdmin'),
        ('Admin'),
        ('Role01'),
        ('Role02'),
        ('Role03'),
        ('Role04'),
        ('Role05'),
        ('Role06'),
        ('Role07'),
        ('Role08'),
        ('Role09'),
        ('Role10'),
        ('Role11'),
        ('Role12'),
        ('Role13'),
        ('Role14'),
        ('Role15'),
        ('Role16');
END
GO

-- Seed Users
IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (UserName, FirstName, LastName, Email, SecretId, ClientId)
    VALUES
        ('superadmin', 'Super', 'Admin', 'ballone.juan@gmail.com', NULL, NULL),
        ('admin1',     'Alice', 'Admin', 'alice.admin@example.com', NULL, NULL),
        ('user1',      'Bob',   'User',  'bob.user@example.com',    NULL, NULL),
        ('user2',      'Carol', 'User',  'carol.user@example.com',  NULL, NULL);
END
GO

-- Assign roles (assumes RoleId 1=SuperAdmin, 2=Admin, 3..=Role01..)
-- superadmin
IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = 1)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES
        (1, 1),
        (1, 2),
        (1, 3),
        (1, 4),
        (1, 5);
END
GO

-- admin1
IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = 2)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES
        (2, 2),
        (2, 3),
        (2, 6);
END
GO

-- user1
IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = 3)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES
        (3, 3),
        (3, 4);
END
GO

-- user2
IF NOT EXISTS (SELECT 1 FROM dbo.UserRoles WHERE UserId = 4)
BEGIN
    INSERT INTO dbo.UserRoles (UserId, RoleId)
    VALUES
        (4, 5);
END
GO