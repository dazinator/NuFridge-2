ALTER TABLE [NuFridge].[FeedConfiguration]
ADD [RetentionPolicyDeletePackages] bit NOT NULL 
CONSTRAINT DF_RetentionPolicyDeletePackages DEFAULT 1