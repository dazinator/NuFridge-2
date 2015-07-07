IF NOT EXISTS (
SELECT  schema_name
FROM    information_schema.schemata
WHERE   schema_name = 'NuFridge' ) 
BEGIN
EXEC sp_executesql N'CREATE SCHEMA NuFridge'
END

CREATE TABLE [NuFridge].[ApiKey] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ApiKey_Id] PRIMARY KEY CLUSTERED,
[UserId] int NOT NULL,
[ApiKeyHashed] NVARCHAR(100) NOT NULL,
[Created] DATETIMEOFFSET NOT NULL
)
ALTER TABLE [NuFridge].[ApiKey] ADD CONSTRAINT [UQ_ApiKeyUnique] UNIQUE([ApiKeyHashed])


CREATE TABLE [NuFridge].[Feed] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Feed_Id] PRIMARY KEY CLUSTERED,
[Name] NVARCHAR(100) NOT NULL
)
ALTER TABLE [NuFridge].[Feed] ADD CONSTRAINT [UQ_FeedNameUnique] UNIQUE([Name])

CREATE TABLE [NuFridge].[FeedConfiguration] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_FeedConfiguration_Id] PRIMARY KEY CLUSTERED,
[PackagesDirectory] NVARCHAR(1000) NOT NULL,
[FeedId] int NOT NULL
)

CREATE TABLE [NuFridge].[Package] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Package_Id] PRIMARY KEY CLUSTERED,
[FeedId] int NOT NULL FOREIGN KEY REFERENCES [NuFridge].[Feed](Id),
[PackageId] NVARCHAR(255) NOT NULL,
[Title] NVARCHAR(255) NULL,
[Version] NVARCHAR(255) NOT NULL,
[VersionMajor] int NOT NULL,
[VersionMinor] int NOT NULL,
[VersionBuild] int NOT NULL,
[VersionRevision] int NOT NULL,
[VersionSpecial] NVARCHAR(255) NULL,
[IsAbsoluteLatestVersion] bit NOT NULL,
[IsLatestVersion] bit NOT NULL,
[IsPrerelease] bit NOT NULL,
[Listed] bit NOT NULL,
[Copyright] nvarchar(4000) NULL,
[LastUpdated] datetime2 NULL,
[Published] DATETIMEOFFSET NULL,
[LicenseUrl] NVARCHAR(4000) NULL,
[ProjectUrl] NVARCHAR(2000) NULL,
[RequireLicenseAcceptance] bit NOT NULL,
[Tags] NVARCHAR(4000) NULL,
[Owners] NVARCHAR(4000) NULL,
[IconUrl] NVARCHAR(2000) NULL,
[Authors] NVARCHAR(4000) NULL,
[Description] NVARCHAR(4000) NOT NULL,
[ReleaseNotes] NVARCHAR(4000) NULL,
[Summary] NVARCHAR(4000) NULL,
[Hash] NVARCHAR(255) NOT NULL,
[DownloadCount] int CONSTRAINT DF_DownloadCount DEFAULT 0 NOT NULL
)

CREATE TABLE [NuFridge].[User] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_User_Id] PRIMARY KEY CLUSTERED,
[Username] NVARCHAR(100) NOT NULL,
[IsActive] BIT NOT NULL,
[IsService] BIT NOT NULL,
[IdentificationToken] UNIQUEIDENTIFIER NOT NULL
)
ALTER TABLE [NuFridge].[User] ADD CONSTRAINT [UQ_UserUsernameUnique] UNIQUE([Username])

CREATE TABLE [NuFridge].[UserRole] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserRole_Id] PRIMARY KEY CLUSTERED,
[Name] NVARCHAR(100) NOT NULL
)
ALTER TABLE [NuFridge].[UserRole] ADD CONSTRAINT [UQ_UserRoleNameUnique] UNIQUE([Name])


CREATE TABLE [NuFridge].[Statistic] (
[Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Statistic_Id] PRIMARY KEY CLUSTERED,
[Name] NVARCHAR(100) NOT NULL,
[Json] NVARCHAR(MAX) NOT NULL
)
ALTER TABLE [NuFridge].[Statistic] ADD CONSTRAINT [UQ_StatisticNameUnique] UNIQUE([Name])