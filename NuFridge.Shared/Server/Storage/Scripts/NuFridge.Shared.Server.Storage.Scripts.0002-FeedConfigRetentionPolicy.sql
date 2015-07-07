ALTER TABLE [NuFridge].[FeedConfiguration] 
ADD [RetentionPolicyEnabled] bit NOT NULL DEFAULT(0)

ALTER TABLE [NuFridge].[FeedConfiguration] 
ADD [MaxReleasePackages] int NOT NULL DEFAULT(10)

ALTER TABLE [NuFridge].[FeedConfiguration] 
ADD [MaxPrereleasePackages] int NOT NULL DEFAULT(10)