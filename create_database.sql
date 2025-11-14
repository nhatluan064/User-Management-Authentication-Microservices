-- Tạo database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AuthServiceDb')
BEGIN
    CREATE DATABASE AuthServiceDb;
END
GO

USE AuthServiceDb;
GO

-- Tạo bảng Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(100) NOT NULL UNIQUE,
        Email NVARCHAR(200) UNIQUE,
        FullName NVARCHAR(200),
        PasswordHash NVARCHAR(255),
        IsLocalUser BIT NOT NULL DEFAULT 0,
        AdDomain NVARCHAR(200),
        AdObjectId NVARCHAR(200),
        Microsoft365Id NVARCHAR(200),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2
    );
END
GO

-- Tạo bảng Departments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
BEGIN
    CREATE TABLE Departments (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50),
        Description NVARCHAR(500),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Tạo bảng Roles
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        DepartmentId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE CASCADE
    );
END
GO

-- Tạo bảng UserDepartments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserDepartments')
BEGIN
    CREATE TABLE UserDepartments (
        UserId INT NOT NULL,
        DepartmentId INT NOT NULL,
        RoleId INT,
        AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        PRIMARY KEY (UserId, DepartmentId),
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
        FOREIGN KEY (RoleId) REFERENCES Roles(Id)
    );
END
GO

-- Tạo bảng Delegations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Delegations')
BEGIN
    CREATE TABLE Delegations (
        Id INT PRIMARY KEY IDENTITY(1,1),
        DelegatorId INT NOT NULL,
        DelegateeId INT NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL,
        Reason NVARCHAR(1000),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        PdfPath NVARCHAR(500),
        EmailSent BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (DelegatorId) REFERENCES Users(Id),
        FOREIGN KEY (DelegateeId) REFERENCES Users(Id)
    );
END
GO

-- Tạo indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
BEGIN
    CREATE INDEX IX_Users_Username ON Users(Username);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
BEGIN
    CREATE INDEX IX_Users_Email ON Users(Email);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_AdObjectId')
BEGIN
    CREATE INDEX IX_Users_AdObjectId ON Users(AdObjectId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Delegations_DelegateeId')
BEGIN
    CREATE INDEX IX_Delegations_DelegateeId ON Delegations(DelegateeId);
END
GO

PRINT 'Database schema created successfully!';
GO

