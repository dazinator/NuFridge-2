CREATE TABLE [NuFridge].[Package] (
    [PrimaryId]                INT             IDENTITY (1, 1) NOT NULL,
    [FeedId]                   INT             NOT NULL,
    [Id]                NVARCHAR (4000) NOT NULL,
    [IdHash]            AS              (CONVERT([varbinary](16),hashbytes('MD5',[Id]))) PERSISTED,
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
    [ReportAbuseUrl]           NVARCHAR (MAX)  NULL,
	[PackageSize]			   BIGINT		   NOT NULL
);
GO

CREATE CLUSTERED INDEX [IX_NuFridgePackage_FeedId_Id]
    ON [NuFridge].[Package]([FeedId] ASC, [IdHash] ASC);
GO


DECLARE @Id INT
DECLARE @ReturnValue NVARCHAR(MAX)
DECLARE @query NVARCHAR(MAX)

IF OBJECT_ID('tempdb..#PackagesTemp') IS NOT NULL
   DROP TABLE #PackagesTemp

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
	set @query = 'INSERT INTO [NuFridge].[Package] ([Id]
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
           ,[ReportAbuseUrl]
		   ,[PackageSize])
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
		   ,0
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

DROP TABLE #PackagesTemp

GO

CREATE PROCEDURE [NuFridge].[GetAllPackages]
	@feedId int
AS

with cte as
(
   SELECT 
   pk.*, 
      ROW_NUMBER() OVER (PARTITION BY IdHash ORDER BY Listed DESC, VersionMajor DESC, VersionMinor DESC, VersionBuild DESC, VersionRevision DESC,  IsPrerelease ASC, VersionSpecial DESC) AS rn
FROM  [NuFridge].[Package] as pk
WHERE pk.FeedId = 1

)

SELECT IsAbsoluteLatestVersion = CASE WHEN rn = 1 THEN 1 ELSE 0 END, IsLatestVersion = CASE WHEN (
SELECT TOP(1) rn FROM cte where IsPrerelease = 0 AND ctee.Id = Id AND Listed = 1
) = rn THEN 1 ELSE 0 END, * FROM cte as ctee

GO

CREATE NONCLUSTERED INDEX [IX_NuFridgePackage_Version]
    ON [NuFridge].[Package]([IdHash] ASC, [Listed] DESC, [VersionMajor] DESC, [VersionMinor] DESC, [VersionBuild] DESC, [VersionRevision] DESC, [IsPrerelease] ASC, [VersionSpecial] DESC)
    INCLUDE([Id]);

GO