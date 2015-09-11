ALTER TABLE [NuFridge].[Package] 
ADD [Created] datetime2 NULL

ALTER TABLE [NuFridge].[Package] 
DROP COLUMN [Published]

ALTER TABLE [NuFridge].[Package] 
ADD [Published] datetime2 NOT NULL

ALTER TABLE [NuFridge].[Package] 
ADD [Dependencies] NVARCHAR(MAX) NULL

ALTER TABLE [NuFridge].[Package] 
ADD [SupportedFrameworks] NVARCHAR(MAX) NULL

ALTER TABLE [NuFridge].[Package] 
ADD [DevelopmentDependency] BIT NOT NULL

ALTER TABLE [NuFridge].[Package] 
ADD [VersionDownloadCount] INT NOT NULL

ALTER TABLE [NuFridge].[Package] 
ADD [Language] NVARCHAR(4000) NULL

ALTER TABLE [NuFridge].[Package] 
ADD [ReportAbuseUrl] NVARCHAR(4000) NULL