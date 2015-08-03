﻿CREATE TABLE [NuFridge].[Package] (
    [Id]                       INT             IDENTITY (1, 1) NOT NULL,
    [FeedId]                   INT             NOT NULL,
    [PackageId]                NVARCHAR (4000) NOT NULL,
    [PackageIdHash]            AS              (CONVERT([varbinary](16),hashbytes('MD5',[PackageId]))) PERSISTED,
    [Title]                    NVARCHAR (4000) NULL,
    [Version]                  NVARCHAR (4000) NOT NULL,
    [VersionMajor]             INT             NOT NULL,
    [VersionMinor]             INT             NOT NULL,
    [VersionBuild]             INT             NOT NULL,
    [VersionRevision]          INT             NOT NULL,
    [VersionSpecial]           NVARCHAR (255) NULL,
    [IsPrerelease]             BIT             NOT NULL,
    [Listed]                   BIT             NOT NULL,
    [Copyright]                NVARCHAR (4000) NULL,
    [LastUpdated]              DATETIME2 (7)   NULL,
    [LicenseUrl]               NVARCHAR (MAX)  NULL,
    [ProjectUrl]               NVARCHAR (MAX)  NULL,
    [RequireLicenseAcceptance] BIT             NOT NULL,
    [Tags]                     NVARCHAR (MAX)  NULL,
    [Owners]                   NVARCHAR (MAX)  NULL,
    [IconUrl]                  NVARCHAR (MAX)  NULL,
    [Authors]                  NVARCHAR (4000) NULL,
    [Description]              NVARCHAR (MAX)  NULL,
    [ReleaseNotes]             NVARCHAR (MAX)  NULL,
    [Summary]                  NVARCHAR (MAX)  NULL,
    [Hash]                     NVARCHAR (4000) NOT NULL,
    [DownloadCount]            INT             NOT NULL,
    [Created]                  DATETIME2 (7)   NULL,
    [Published]                DATETIME2 (7)   NOT NULL,
    [Dependencies]             NVARCHAR (MAX)  NULL,
    [SupportedFrameworks]      NVARCHAR (MAX)  NULL,
    [DevelopmentDependency]    BIT             NOT NULL,
    [VersionDownloadCount]     INT             NOT NULL,
    [Language]                 NVARCHAR (4000) NULL,
    [ReportAbuseUrl]           NVARCHAR (MAX)  NULL
);
GO

CREATE CLUSTERED INDEX [IX_NuFridgePackage_FeedId_PackageId]
    ON [NuFridge].[Package]([FeedId] ASC, [PackageIdHash] ASC);
GO


DECLARE @Id INT
DECLARE @ReturnValue NVARCHAR(MAX)
DECLARE @query NVARCHAR(MAX)

PRINT 'Getting a list of existing feeds'

SELECT * INTO   #PackagesTemp FROM   [NuFridge].[Feed]

DECLARE @tableName NVARCHAR(MAX)


WHILE EXISTS(SELECT * FROM #PackagesTemp)
Begin

    SELECT TOP 1 @Id = Id FROM #PackagesTemp



	SET @tableName = 'Package_' + CONVERT(NVARCHAR(MAX),@Id)

	PRINT 'Checking if a table called ' + @tableName + ' exists'

		IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'NuFridge' 
                 AND  TABLE_NAME = @tableName))
BEGIN
	set @query = 'INSERT INTO [NuFridge].[Package] ([PackageId]
		   ,[FeedId]
           ,[Title]
           ,[Version]
           ,[VersionMajor]
           ,[VersionMinor]
           ,[VersionBuild]
           ,[VersionRevision]
           ,[VersionSpecial]
           ,[IsAbsoluteLatestVersion]
           ,[IsLatestVersion]
           ,[IsPrerelease]
           ,[Listed]
           ,[Copyright]
           ,[LastUpdated]
           ,[LicenseUrl]
           ,[ProjectUrl]
           ,[RequireLicenseAcceptance]
           ,[Tags]
           ,[Owners]
           ,[IconUrl]
           ,[Authors]
           ,[Description]
           ,[ReleaseNotes]
           ,[Summary]
           ,[Hash]
           ,[DownloadCount]
           ,[Created]
           ,[Published]
           ,[Dependencies]
           ,[SupportedFrameworks]
           ,[DevelopmentDependency]
           ,[VersionDownloadCount]
           ,[Language]
           ,[ReportAbuseUrl])
	SELECT [PackageId]
		   ,' + CONVERT(NVARCHAR(MAX),@Id) + '
           ,[Title]
           ,[Version]
           ,[VersionMajor]
           ,[VersionMinor]
           ,[VersionBuild]
           ,[VersionRevision]
           ,[VersionSpecial]
           ,[IsAbsoluteLatestVersion]
           ,[IsLatestVersion]
           ,[IsPrerelease]
           ,[Listed]
           ,[Copyright]
           ,[LastUpdated]
           ,[LicenseUrl]
           ,[ProjectUrl]
           ,[RequireLicenseAcceptance]
           ,[Tags]
           ,[Owners]
           ,[IconUrl]
           ,[Authors]
           ,[Description]
           ,[ReleaseNotes]
           ,[Summary]
           ,[Hash]
           ,[DownloadCount]
           ,[Created]
           ,[Published]
           ,[Dependencies]
           ,[SupportedFrameworks]
           ,[DevelopmentDependency]
           ,[VersionDownloadCount]
           ,[Language]
           ,[ReportAbuseUrl]
	FROM [NuFridge].[' + @tableName + ']'

	PRINT 'Importing existing packages from [NuFridge].[' + @tableName + '] into [NuFridge].[Package]' 

	execute sp_executesql @query

	PRINT 'Imported existing packages from [NuFridge].[' + @tableName + '] into [NuFridge].[Package]' 

    DELETE #PackagesTemp WHERE Id = @Id

	PRINT 'Dropping old packages table for feed id ' + CONVERT(NVARCHAR(MAX),@Id)

	SET @query = 'sp_DropNuFridgePackageTable ' + CONVERT(NVARCHAR(MAX),@Id)

	execute sp_executesql @query
	END
	ELSE
	BEGIN
	    DELETE #PackagesTemp WHERE Id = @Id
	END
END
GO

CREATE PROCEDURE [NuFridge].[GetAllPackages]
	@feedId int
AS

with cte as
(
   SELECT 
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY PackageIdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk
WHERE pk.FeedId = 1

)

SELECT IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END, IsLatestVersion = CASE WHEN (
SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.PackageId = PackageId AND Listed = 1
) = rn THEN 1 ELSE 0 END, * FROM cte as ctee

GO

CREATE NONCLUSTERED INDEX [IX_NuFridgePackage_Version]
    ON [NuFridge].[Package]([PackageIdHash] ASC, [Listed] DESC, [VersionMajor] DESC, [VersionMinor] DESC, [VersionBuild] DESC, [VersionRevision] DESC, [IsPrerelease] ASC, [VersionSpecial] DESC)
    INCLUDE([PackageId]);

GO

CREATE TABLE [dbo].[AuditLogs] (
    [AuditLogId]   INT                IDENTITY (1, 1) NOT NULL,
    [UserName]     NVARCHAR (100)     NOT NULL,
    [EventDateUTC] DATETIMEOFFSET (7) NOT NULL,
    [EventType]    INT                NOT NULL,
    [TableName]    NVARCHAR (256)     NOT NULL,
    [RecordId]     NVARCHAR (256)     NOT NULL
);

GO

CREATE TABLE [dbo].[AuditLogDetails] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [ColumnName]    NVARCHAR (256)  NOT NULL,
    [OriginalValue] NVARCHAR (4000) NULL,
    [NewValue]      NVARCHAR (4000) NULL,
    [AuditLogId]    INT             NULL
);

GO