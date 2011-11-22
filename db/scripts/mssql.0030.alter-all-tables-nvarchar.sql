﻿ALTER TABLE [dbo].[ForumsCategories] ALTER COLUMN [CategoryName] NVARCHAR(255) NOT NULL;
ALTER TABLE [dbo].[Topics] ALTER COLUMN [TopicTitle] NVARCHAR(256) NOT NULL;
ALTER TABLE [dbo].[Topics] ALTER COLUMN [TopicShortName] NVARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Topics] ALTER COLUMN [TopicDescription] NVARCHAR(MAX) NOT NULL;
ALTER TABLE [dbo].[Topics] ALTER COLUMN [TopicTags] NVARCHAR(256) NOT NULL;
-- ALTER TABLE [dbo].[Topics] ALTER COLUMN [TopicLastEditIp] NVARCHAR(15) NOT NULL; Ip columns will be altered to allow Ip6
ALTER TABLE [dbo].[Forums] ALTER COLUMN [ForumName] NVARCHAR(255) NOT NULL;

GO
DROP INDEX [IX_Forums_ForumShortName] ON [dbo].[Forums] WITH ( ONLINE = OFF )
GO
ALTER TABLE [dbo].[Forums] ALTER COLUMN [ForumShortName] NVARCHAR(32) NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Forums_ForumShortName] ON [dbo].[Forums] 
(
	[ForumShortName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO

ALTER TABLE [dbo].[Forums] ALTER COLUMN [ForumDescription] NVARCHAR(MAX) NOT NULL;
ALTER TABLE [dbo].[Messages] ALTER COLUMN [MessageBody] NVARCHAR(MAX) NOT NULL;
-- ALTER TABLE [dbo].[Messages] ALTER COLUMN [EditIp] NVARCHAR(15) NULL; Ip columns will be altered to allow Ip6
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserName] NVARCHAR(50) NOT NULL;
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserProfile] NVARCHAR(MAX) NULL;
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserSignature] NVARCHAR(MAX) NULL;
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserWebsite] NVARCHAR(255) NULL;
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserEmail] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserPhoto] NVARCHAR(1024) NULL;
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserExternalProfileUrl] NVARCHAR(255) NULL;

GO
DROP INDEX [IX_Users] ON [dbo].[Users] WITH ( ONLINE = OFF );
GO
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserProvider] NVARCHAR(32) NOT NULL;
ALTER TABLE [dbo].[Users] ALTER COLUMN [UserProviderId] NVARCHAR(64) NOT NULL;
CREATE NONCLUSTERED INDEX [IX_Users] ON [dbo].[Users] 
(
	[UserProvider] ASC,
	[UserProviderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO

ALTER TABLE [dbo].[Users] ALTER COLUMN [PasswordResetGuid] NVARCHAR(100) NULL;

GO
ALTER TABLE [dbo].[Tags] DROP CONSTRAINT [PK_Tags];
GO
ALTER TABLE [dbo].[Tags] ALTER COLUMN [Tag] NVARCHAR(50) NOT NULL;
GO
ALTER TABLE [dbo].[Tags] ADD  CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Tag] ASC,
	[TopicId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON);
GO

-- ALTER TABLE [dbo].[Flags] ALTER COLUMN [Ip] NVARCHAR(15) NOT NULL; Ip columns will be altered to allow Ip6
ALTER TABLE [dbo].[PageContents] ALTER COLUMN [PageContentTitle] NVARCHAR(128) NOT NULL;
ALTER TABLE [dbo].[PageContents] ALTER COLUMN [PageContentBody] NVARCHAR(MAX) NOT NULL;
ALTER TABLE [dbo].[PageContents] ALTER COLUMN [PageContentShortName] NVARCHAR(128) NOT NULL;
ALTER TABLE [dbo].[UsersGroups] ALTER COLUMN [UserGroupName] NVARCHAR(50) NOT NULL;
ALTER TABLE [dbo].[Templates] ALTER COLUMN [TemplateKey] NVARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Templates] ALTER COLUMN [TemplateDescription] NVARCHAR(256) NULL;