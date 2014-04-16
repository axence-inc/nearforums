CREATE TABLE Flags
(
	FlagId serial NOT NULL,
	TopicId int NOT NULL,
	MessageId int NULL,
	Ip varchar(39) NOT NULL,
	FlagDate timestamp NOT NULL,
	
	CONSTRAINT PK_Flags PRIMARY KEY (FlagId)
);

CREATE TABLE Forums
(
	ForumId serial NOT NULL,
	ForumName varchar(255) NOT NULL,
	ForumShortName varchar(32) NOT NULL,
	ForumDescription text NOT NULL,
	CategoryId int NOT NULL,
	UserId int NOT NULL,
	ForumCreationDate timestamp NOT NULL,
	ForumLastEditDate timestamp NOT NULL,
	ForumLastEditUser int NOT NULL,
	Active boolean NOT NULL,
	ForumTopicCount int NOT NULL,
	ForumMessageCount int NOT NULL,
	ForumOrder int NOT NULL,
	ReadAccessGroupId smallint NULL,
	PostAccessGroupId smallint NOT NULL,
	
	CONSTRAINT PK_Forums PRIMARY KEY (ForumId)
);

CREATE TABLE ForumsCategories
(
	CategoryId serial NOT NULL,
	CategoryName varchar(255) NOT NULL,
	CategoryOrder int NOT NULL,
	
	CONSTRAINT PK_ForumsCategories PRIMARY KEY(CategoryId)
);

CREATE TABLE Messages
(
	TopicId int NOT NULL,
	MessageId int NOT NULL,
	MessageBody text NOT NULL,
	MessageCreationDate timestamp NOT NULL,
	MessageLastEditDate timestamp NOT NULL,
	UserId int NOT NULL,
	ParentId int NULL,
	Active boolean NOT NULL,
	EditIp varchar(39) NULL,
	MessageLastEditUser int NOT NULL,
	
	CONSTRAINT PK_Messages PRIMARY KEY(TopicId,MessageId)
);

CREATE TABLE PageContents
(
	PageContentId serial NOT NULL,
	PageContentTitle varchar(128) NOT NULL,
	PageContentBody text NOT NULL,
	PageContentShortName varchar(128) NOT NULL,
	PageContentEditDate timestamp NOT NULL,
	
	CONSTRAINT PK_PageContents PRIMARY KEY (PageContentId)
);

CREATE TABLE Settings
(
	SettingKey varchar(256) NOT NULL,
	SettingValue text NULL,
	SettingDate timestamp NOT NULL,
	
	CONSTRAINT PK_Settings PRIMARY KEY (SettingKey)
);

CREATE TABLE Tags
(
	Tag varchar(50) NOT NULL,
	TopicId int NOT NULL,
	
	CONSTRAINT PK_Tags PRIMARY KEY (Tag ,TopicId)
);

CREATE TABLE Templates
(
	TemplateId serial NOT NULL,
	TemplateKey varchar(64) NOT NULL,
	TemplateDescription varchar(256) NULL,
	TemplateIsCurrent boolean NOT NULL,
	TemplateDate timestamp NOT NULL,
	
	CONSTRAINT PK_Templates PRIMARY KEY (TemplateId)
);

CREATE TABLE Topics
(
	TopicId serial NOT NULL,
	TopicTitle varchar(256) NOT NULL,
	TopicShortName varchar(64) NOT NULL,
	TopicDescription text NOT NULL,
	TopicCreationDate timestamp NOT NULL,
	TopicLastEditDate timestamp NOT NULL,
	TopicViews int NOT NULL,
	TopicReplies int NOT NULL,
	UserId int NOT NULL,
	TopicTags varchar(256) NOT NULL,
	ForumId int NOT NULL,
	TopicLastEditUser int NOT NULL,
	TopicLastEditIp varchar(39) NOT NULL,
	Active boolean NOT NULL,
	TopicIsClose boolean NOT NULL,
	TopicOrder int NULL,
	LastMessageId int NULL,
	MessagesIdentity int NOT NULL,
	ReadAccessGroupId smallint NULL,
	PostAccessGroupId smallint NOT NULL,
	
	CONSTRAINT PK_Topics PRIMARY KEY (TopicId)
);

CREATE TABLE TopicsSubscriptions
(
	TopicId int NOT NULL,
	UserId int NOT NULL,
	
	CONSTRAINT PK_TopicsSubscriptions PRIMARY KEY (TopicId,UserId)
);

CREATE TABLE Users
(
	UserId serial NOT NULL,
	UserName varchar(50) NOT NULL,
	UserProfile text NULL,
	UserSignature text NULL,
	UserGroupId smallint NOT NULL,
	Active boolean NOT NULL,
	UserBirthDate timestamp NULL,
	UserWebsite varchar(255) NULL,
	UserGuid char(32) NOT NULL,
	UserTimezone decimal(9, 2) NOT NULL,
	UserEmail varchar(100) NULL,
	UserEmailPolicy int NULL,
	UserPhoto varchar(1024) NULL,
	UserRegistrationDate timestamp NOT NULL,
	UserExternalProfileUrl varchar(255) NULL,
	UserProvider varchar(32) NOT NULL,
	UserProviderId varchar(64) NOT NULL,
	UserProviderLastCall timestamp NOT NULL,
	PasswordResetGuid varchar(100) NULL,
	PasswordResetGuidExpireDate timestamp NULL,
	WarningStart timestamp NULL,
	WarningRead boolean NULL,
	SuspendedStart timestamp NULL,
	SuspendedEnd timestamp NULL,
	BannedStart timestamp NULL,
	ModeratorReasonFull text NULL,
	ModeratorReason int NULL,
	ModeratorUserId int NULL,
	
	CONSTRAINT PK_Users PRIMARY KEY(UserId)
);

CREATE TABLE UsersGroups
(
	UserGroupId smallint NOT NULL,
	UserGroupName varchar(50) NOT NULL,
	
	CONSTRAINT PK_UsersGroups PRIMARY KEY(UserGroupId)
);


CREATE FUNCTION Split (s varchar(512), sep char(1)) RETURNS TABLE(pn int, part text) AS $$
BEGIN
	RETURN QUERY (
		    WITH RECURSIVE Pieces(pn, start, stop) AS (
			SELECT 1, 1, POSITION(sep in s)
		      UNION ALL
		        SELECT Pieces.pn + 1, Pieces.stop + 1, Pieces.stop + POSITION(sep in SUBSTR(s, Pieces.stop + 1))
		        FROM Pieces
		        WHERE Pieces.stop >= Pieces.start
		    )
		    SELECT Pieces.pn AS position,
			SUBSTRING(s, Pieces.start, CASE WHEN Pieces.stop >= Pieces.start THEN Pieces.stop-Pieces.start ELSE 512 END) AS part
		    FROM Pieces
	    );
END;
$$ LANGUAGE plpgsql;

CREATE VIEW MessagesComplete
AS
SELECT
	M.TopicId
	,M.MessageId
	,M.MessageBody
	,M.MessageCreationDate
	,M.MessageLastEditDate
	,M.ParentId
	,M.UserId
	,M.Active
	,U.UserName
	,U.UserSignature
	,U.UserGroupId
	,G.UserGroupName
	,U.UserPhoto
	,U.UserRegistrationDate
FROM
	Messages M
	INNER JOIN Users U ON U.UserId = M.UserId
	INNER JOIN UsersGroups G ON G.UserGroupId = U.UserGroupId
	LEFT JOIN Messages P ON P.TopicId = M.TopicId AND P.MessageId = M.ParentId AND P.Active = TRUE;

CREATE VIEW TopicsComplete 
AS
SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,U.UserName
		,F.ForumId
		,F.ForumName
		,F.ForumShortName
		,CASE 
			WHEN COALESCE(T.ReadAccessGroupId, -1) >= COALESCE(F.ReadAccessGroupId, -1) THEN T.ReadAccessGroupId
			ELSE F.ReadAccessGroupId END AS ReadAccessGroupId
		,CASE 
			WHEN T.PostAccessGroupId >= COALESCE(F.ReadAccessGroupId,-1) THEN T.PostAccessGroupId -- Do not inherit post access
			ELSE F.ReadAccessGroupId END AS PostAccessGroupId -- use the parent read access, if greater
	FROM
		Topics T
		INNER JOIN Users U ON U.UserId = T.UserId
		INNER JOIN Forums F ON F.ForumId = T.ForumId
	WHERE
		T.Active = TRUE
		AND
		F.Active = TRUE;


CREATE UNIQUE INDEX IX_Flags ON Flags (TopicId, MessageId, Ip);

CREATE UNIQUE INDEX IX_Forums_ForumShortName ON Forums (ForumShortName);

CREATE INDEX IX_Topics_ForumId_Active ON Topics (Active,ForumId);

CREATE INDEX IX_Users ON Users (UserProvider, UserProviderId);

ALTER TABLE Flags   ADD  CONSTRAINT FK_Flags_Messages FOREIGN KEY(TopicId, MessageId)
REFERENCES Messages (TopicId, MessageId);

ALTER TABLE Forums   ADD  CONSTRAINT FK_Forums_ForumsCategories FOREIGN KEY(CategoryId)
REFERENCES ForumsCategories (CategoryId);

ALTER TABLE Forums   ADD  CONSTRAINT FK_Forums_Users FOREIGN KEY(UserId)
REFERENCES Users (UserId);

ALTER TABLE Forums   ADD  CONSTRAINT FK_Forums_Users_LastEdit FOREIGN KEY(ForumLastEditUser)
REFERENCES Users (UserId);

ALTER TABLE Forums   ADD  CONSTRAINT FK_Forums_UsersGroups_Post FOREIGN KEY(PostAccessGroupId)
REFERENCES UsersGroups (UserGroupId);

ALTER TABLE Forums   ADD  CONSTRAINT FK_Forums_UsersGroups_Read FOREIGN KEY(ReadAccessGroupId)
REFERENCES UsersGroups (UserGroupId);

ALTER TABLE Messages   ADD  CONSTRAINT FK_Messages_Topics FOREIGN KEY(TopicId)
REFERENCES Topics (TopicId);

ALTER TABLE Messages   ADD  CONSTRAINT FK_Messages_Users FOREIGN KEY(UserId)
REFERENCES Users (UserId);

ALTER TABLE Tags   ADD  CONSTRAINT FK_Tags_Topics FOREIGN KEY(TopicId)
REFERENCES Topics (TopicId);

ALTER TABLE Topics   ADD  CONSTRAINT FK_Topics_Forums FOREIGN KEY(ForumId)
REFERENCES Forums (ForumId);

ALTER TABLE Topics   ADD  CONSTRAINT FK_Topics_Users FOREIGN KEY(UserId)
REFERENCES Users (UserId);

ALTER TABLE Topics   ADD  CONSTRAINT FK_Topics_Users_LastEdit FOREIGN KEY(TopicLastEditUser)
REFERENCES Users (UserId);

ALTER TABLE Topics   ADD  CONSTRAINT FK_Topics_UsersGroups_Post FOREIGN KEY(PostAccessGroupId)
REFERENCES UsersGroups (UserGroupId);

ALTER TABLE Topics   ADD  CONSTRAINT FK_Topics_UsersGroups_Read FOREIGN KEY(ReadAccessGroupId)
REFERENCES UsersGroups (UserGroupId);

ALTER TABLE TopicsSubscriptions   ADD  CONSTRAINT FK_TopicsSubscriptions_Topics FOREIGN KEY(TopicId)
REFERENCES Topics (TopicId);

ALTER TABLE TopicsSubscriptions   ADD  CONSTRAINT FK_TopicsSubscriptions_Users FOREIGN KEY(UserId)
REFERENCES Users (UserId);

ALTER TABLE Users   ADD  CONSTRAINT FK_Users_UsersGroups FOREIGN KEY(UserGroupId)
REFERENCES UsersGroups (UserGroupId);

drop FUNCTION IF EXISTS SPUsersWarnDismiss(int);
CREATE FUNCTION SPUsersWarnDismiss(aUserId int)
	RETURNS int
AS $$
	UPDATE Users
	SET
		WarningRead = TRUE
	WHERE 
		UserId = aUserId
	returning 1;
$$ LANGUAGE SQL;

CREATE FUNCTION SPUsersWarn(aUserId int, aModeratorUserId int, aModeratorReason int, aModeratorReasonFull text) RETURNS int
AS $$
	UPDATE Users
	SET
		WarningStart = CAST(NOW() at time zone 'utc' AS date)
		, WarningRead = FALSE
		, ModeratorReason = aModeratorReason
		, ModeratorReasonFull = aModeratorReasonFull
		, ModeratorUserId = aModeratorUserId
	WHERE 
		UserId = aUserId;
	SELECT 0;
$$ LANGUAGE SQL;

CREATE FUNCTION SPUsersUpdatePasswordResetGuid(aUserId int, aPasswordResetGuid varchar(100) ,aPasswordResetGuidExpireDate timestamp)
RETURNS int
AS $$
	UPDATE Users
	SET
		PasswordResetGuid = aPasswordResetGuid
		,PasswordResetGuidExpireDate = aPasswordResetGuidExpireDate
	WHERE
		UserId = @UserId;
	SELECT 0;
$$ LANGUAGE SQL;

CREATE FUNCTION SPUsersUpdateEmail(aUserId int, aUserEmail varchar(100), aUserEmailPolicy int) RETURNS int
AS $$
	UPDATE Users
	SET
		UserEmail = aUserEmail
		,UserEmailPolicy = aUserEmailPolicy
	WHERE
		UserId = aUserId;
	SELECT 0;
$$ LANGUAGE SQL;

CREATE FUNCTION SPUsersUpdate(aUserId int
	,aUserName varchar(50)
	,aUserProfile text
	,aUserSignature text
	,aUserBirthDate timestamp
	,aUserWebsite varchar(255)
	,aUserTimezone decimal(9,2)
	,aUserEmail varchar(100)
	,aUserEmailPolicy int
	,aUserPhoto varchar(1024)
	,aUserExternalProfileUrl varchar(255))
RETURNS int
AS $$
	UPDATE Users
	SET 
	UserName = aUserName
	,UserProfile = aUserProfile
	,UserSignature = aUserSignature
	,UserBirthDate = aUserBirthDate
	,UserWebsite = aUserWebsite
	,UserTimezone = aUserTimezone
	,UserEmail = aUserEmail
	,UserEmailPolicy = aUserEmailPolicy
	,UserPhoto = aUserPhoto
	,UserExternalProfileUrl = aUserExternalProfileUrl
	WHERE 
		UserId = aUserId;
	SELECT 0;
$$ LANGUAGE SQL;

CREATE FUNCTION SPUsersSuspend
(
	aUserId int
	, aModeratorUserId int
	, aModeratorReason int
	, aModeratorReasonFull text
	, aSuspendedEnd timestamp
)
RETURNS int
AS $$
	UPDATE Users
	SET
		SuspendedStart = CAST(NOW() at time zone 'utc' AS date)
		, SuspendedEnd = aSuspendedEnd
		, ModeratorReason = aModeratorReason
		, ModeratorReasonFull = aModeratorReasonFull
		, ModeratorUserId = aModeratorUserId
	WHERE 
		UserId = aUserId;
	SELECT 0;
$$ LANGUAGE SQL;

CREATE FUNCTION SPUsersPromote(aUserId int) RETURNS int
AS $$
DECLARE lUserGroupId int;
BEGIN
	SELECT lUserGroupId = UserGroupId FROM Users WHERE UserId = aUserId;
	SELECT lUserGroupId = MIN(UserGroupId) FROM UsersGroups WHERE UserGroupId > lUserGroupId;

	IF lUserGroupId IS NOT NULL THEN
		UPDATE Users
		SET
			UserGroupId = lUserGroupId
		WHERE
			UserId = aUserId;
	END IF;
	RETURN 0;
END;
$$ LANGUAGE plpgsql;

CREATE FUNCTION SPUsersGetByProvider(aProvider varchar(32), aProviderId varchar(64))
RETURNS TABLE
	(UserId int,
	UserName varchar(50),
	UserGroupId smallint,
	UserGuid char(32),
	UserTimeZone decimal(9, 2),
	UserExternalProfileUrl varchar(255),
	UserProviderLastCall timestamp,
	UserEmail varchar(100),
	UserProfile text,
	UserSignature text,
	WarningStart timestamp,
	WarningRead boolean,
	SuspendedStart timestamp,
	SuspendedEnd timestamp,
	BannedStart timestamp,
	ModeratorReasonFull text,
	ModeratorReason int,
	ModeratorUserId int)
AS $$
	SELECT 
		U.UserId
		,U.UserName
		,U.UserGroupId
		,U.UserGuid
		,U.UserTimeZone
		,U.UserExternalProfileUrl
		,U.UserProviderLastCall
		,U.UserEmail
		,U.UserProfile
		,U.UserSignature
		,U.WarningStart
		,U.WarningRead
		,U.SuspendedStart
		,U.SuspendedEnd
		,U.BannedStart
		,U.ModeratorReasonFull
		,U.ModeratorReason
		,U.ModeratorUserId
	FROM
		Users U
	WHERE
		UserProvider = aProvider
		AND
		UserProviderId = aProviderId
		AND
		U.Active = TRUE;
$$ LANGUAGE SQL;

DROP FUNCTION IF EXISTS SPUsersInsertFromProvider(aUserName varchar(50)
	,aUserProfile text
	,aUserSignature text
	,aUserGroupId smallint
	,aUserBirthDate timestamp
	,aUserWebsite varchar(255)
	,aUserGuid char(32)
	,aUserTimezone decimal(9,2)
	,aUserEmail varchar(100)
	,aUserEmailPolicy int
	,aUserPhoto varchar(1024)
	,aUserExternalProfileUrl varchar(255)
	,aUserProvider varchar(32)
	,aUserProviderId varchar(64));
CREATE FUNCTION SPUsersInsertFromProvider
	(aUserName varchar(50)
	,aUserProfile text
	,aUserSignature text
	,aUserGroupId smallint
	,aUserBirthDate timestamp
	,aUserWebsite varchar(255)
	,aUserGuid char(32)
	,aUserTimezone decimal(9,2)
	,aUserEmail varchar(100)
	,aUserEmailPolicy int
	,aUserPhoto varchar(1024)
	,aUserExternalProfileUrl varchar(255)
	,aUserProvider varchar(32)
	,aUserProviderId varchar(64))
	returns setof Users
-- RETURNS TABLE
-- 	(UserId int,
-- 	UserName varchar(50),
-- 	UserGroupId smallint,
-- 	UserGuid char(32),
-- 	UserTimeZone decimal(9, 2),
-- 	UserExternalProfileUrl varchar(255),
-- 	UserProviderLastCall timestamp,
-- 	UserEmail varchar(100),
-- 	UserProfile text,
-- 	UserSignature text,
-- 	WarningStart timestamp,
-- 	WarningRead boolean,
-- 	SuspendedStart timestamp,
-- 	SuspendedEnd timestamp,
-- 	BannedStart timestamp,
-- 	ModeratorReasonFull text,
-- 	ModeratorReason int,
-- 	ModeratorUserId int)
AS $$
DECLARE 
	newUser Users%rowtype;
	
BEGIn
	--If it is the first active user -> make it an admin
	IF NOT EXISTS (SELECT * FROM Users WHERE Active)
	THEN
		SELECT MAX(ug.UserGroupId) AS aUserGroupId 
		INTO aUserGroupId
		FROM UsersGroups ug;
	END IF;

	INSERT INTO Users
	   (UserName
	   ,UserProfile
	   ,UserSignature
	   ,UserGroupId
	   ,Active
	   ,UserBirthDate
	   ,UserWebsite
	   ,UserGuid
	   ,UserTimezone
	   ,UserEmail
	   ,UserEmailPolicy
	   ,UserPhoto
	   ,UserRegistrationDate
	   ,UserExternalProfileUrl
	   ,UserProvider
	   ,UserProviderId
	   ,UserProviderLastCall)
	VALUES (aUserName
	   ,aUserProfile
	   ,aUserSignature
	   ,aUserGroupId
	   ,TRUE --Active
	   ,aUserBirthDate
	   ,aUserWebsite
	   ,aUserGuid
	   ,aUserTimezone
	   ,aUserEmail
	   ,aUserEmailPolicy
	   ,aUserPhoto
	   ,CAST(NOW() at time zone 'utc' AS date) --RegitrationDate
	   ,aUserExternalProfileUrl
	   ,aUserProvider
	   ,aUserProviderId
	   ,CAST(NOW() at time zone 'utc' AS date) --UserProviderLastCall
		)
	RETURNING Users.*
	INTO newUser;

	return next newUser;
	return;
END;
$$ LANGUAGE plpgsql;

CREATE FUNCTION SPUsersGroupsGetAll() RETURNS TABLE (UserGroupId smallint, UserGroupName varchar(50))
AS $$
	SELECT 
		UserGroupId
		,UserGroupName
	FROM
		UsersGroups
	ORDER BY 
		UserGroupId ASC;
$$ LANGUAGE SQL;

CREATE FUNCTION SPUsersGroupsGet(aUserGroupId smallint = 1) RETURNS TABLE (UserGroupId smallint, UserGroupName varchar(50))
AS $$
SELECT 
	UserGroupId
	,UserGroupName
FROM
	UsersGroups
WHERE
	UserGroupId = aUserGroupId;
	
$$ LANGUAGE SQL;



DROP FUNCTION IF EXISTS public.SPUsersGetTestUser();
CREATE FUNCTION public.SPUsersGetTestUser() RETURNS SETOF public.Users
	LANGUAGE sql
AS $$
	SELECT *
	FROM
		Users U
	WHERE
		U.Active
	ORDER BY
		U.UserGroupId DESC
	LIMIT 1;
$$;

CREATE OR REPLACE FUNCTION public.SPSettingsGet(key varchar(256)) RETURNS SETOF public.Settings
	LANGUAGE sql
	STABLE
AS $$
	SELECT 
		s.SettingKey, s.SettingValue, s.SettingDate
	FROM 
		Settings AS s
	WHERE
		s.SettingKey = key;
$$;

DROP FUNCTION IF EXISTS SPSETTINGSSET(VARCHAR, TEXT);
CREATE FUNCTION public.SPSettingsSet(key varchar(256), val TEXT) RETURNS INT
	LANGUAGE plpgsql
AS $$
BEGIN
	IF EXISTS (SELECT SettingDate 
		FROM Settings as s
		WHERE s.settingkey = key
		LIMIT 1)
	THEN
		UPDATE Settings	
		SET SettingValue = val,
			SettingDate = now()
		WHERE SettingKey= key;
	ELSE
		INSERT INTO Settings (SettingKey, SettingValue, SettingDate)
		VALUES (key, val, now());
	END IF;

	RETURN 1;
END; 
$$;


DROP FUNCTION IF EXISTS public.SPForumsGetByCategory(smallint);
CREATE FUNCTION public.SPForumsGetByCategory(
		UserGroupId smallint=NULL) 
	RETURNS TABLE(
		ForumId int,
		ForumName varchar(255),
		ForumShortName varchar(32),
		ForumDescription text,
		UserId int,
		ForumCreationDate timestamp,
		ForumTopicCount int,
		ForumMessageCount int,
		CategoryId int,
		CategoryName varchar(255))
	LANGUAGE SQL
AS $$
SELECT
	F.ForumId
	,F.ForumName
	,F.ForumShortName
	,F.ForumDescription
	,F.UserId
	,F.ForumCreationDate
	,F.ForumTopicCount
	,F.ForumMessageCount
	,C.CategoryId
	,C.CategoryName
FROM
	ForumsCategories C
	INNER JOIN Forums F ON F.CategoryId = C.CategoryId
WHERE
	F.Active
	AND COALESCE(F.ReadAccessGroupId,-1) <= COALESCE(@UserGroupId,-1)
ORDER BY
	C.CategoryOrder,
	F.ForumOrder
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetByForum(int, int, int, int);
CREATE FUNCTION public.SPTopicsGetByForum(
		ForumId int = 2,
		StartIndex int = 0,
		Length int = 10,
		UserGroupId int = null) 
	RETURNS TABLE(
		TopicId INT,
		TopicTitle VARCHAR(255),
		TopicShortName VARCHAR(64),
		TopicDescription TEXT,
		TopicCreationDate TIMESTAMP,
		TopicViews INT,
		TopicReplies INT,
		UserId INT,
		TopicTags VARCHAR(256),
		TopicIsClose BOOLEAN,
		TopicOrder INT,
		LastMessageId INT,
		UserName VARCHAR,
		MessageCreationDate TIMESTAMP,
		MessageUserId INT,
		MessageUserName VARCHAR,
		ReadAccessGroupId SMALLINT,
		PostAccessGroupId SMALLINT)
	LANGUAGE SQL
AS $$
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,T.UserName
		,M.MessageCreationDate
		,M.UserId AS MessageUserId
		,MU.UserName AS MessageUserName
		,T.ReadAccessGroupId
		,T.PostAccessGroupId
	FROM
		TopicsComplete T
		LEFT JOIN public.Messages AS M 
			ON M.TopicId = T.TopicId AND M.MessageId = T.LastMessageId AND M.Active
		LEFT JOIN Users AS MU 
			ON MU.UserId = M.UserId
	WHERE
		T.ForumId = SPTopicsGetByForum.ForumId
		AND COALESCE(T.ReadAccessGroupId,-1) <= COALESCE(SPTopicsGetByForum.UserGroupId,-1)
	OFFSET SPTopicsGetByForum.StartIndex LIMIT SPTopicsGetByForum.Length
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGet(int);
CREATE FUNCTION public.SPTopicsGet(TopicId int=1) 
	RETURNS setof TopicsComplete
	LANGUAGE sql
AS $$
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,T.UserName
		,T.ForumId
		,T.ForumName
		,T.ForumShortName
		,T.ReadAccessGroupId
		,T.PostAccessGroupId
	FROM 
		TopicsComplete T
	WHERE
		T.TopicId = SPTopicsGet.TopicId;
$$;

DROP FUNCTION IF EXISTS public.SPForumsGetByShortName(VARCHAR);
CREATE FUNCTION public.SPForumsGetByShortName(ShortName VARCHAR(32)) 
	RETURNS TABLE (
		ForumId INT,
		ForumName VARCHAR,
		ForumShortName VARCHAR,
		ForumDescription TEXT,
		UserId INT,
		ForumCreationDate TIMESTAMP,
		ForumTopicCount INT,
		ForumMessageCount INT,
		CategoryId INT,
		CategoryName VARCHAR,
		ReadAccessGroupId SMALLINT,
		PostAccessGroupId SMALLINT
	)
	LANGUAGE SQL
AS $$
	SELECT
		F.ForumId
		,F.ForumName
		,F.ForumShortName
		,F.ForumDescription
		,F.UserId
		,F.ForumCreationDate
		,F.ForumTopicCount
		,F.ForumMessageCount
		,C.CategoryId
		,C.CategoryName
		,F.ReadAccessGroupId
		,F.PostAccessGroupId
	FROM
		Forums F 
		INNER JOIN ForumsCategories C ON F.CategoryId = C.CategoryId
	WHERE
		F.ForumShortName = ShortName
		AND F.Active
$$;

DROP FUNCTION IF EXISTS public.SPTopicsUpdate(
	TopicId int, 
	TopicTitle varchar(256), 
	TopicDescription text, 
	UserId int, 
	TopicTags varchar(256),
	TopicOrder int, 
	Ip varchar (39),
	ReadAccessGroupId smallint,
	PostAccessGroupId smallint);
	
CREATE FUNCTION public.SPTopicsUpdate(
		TopicId int, 
		TopicTitle varchar(256), 
		TopicDescription text, 
		UserId int, 
		TopicTags varchar(256),
		TopicOrder int, 
		Ip varchar (39),
		ReadAccessGroupId smallint,
		PostAccessGroupId smallint) 
	RETURNS int
	LANGUAGE plpgsql
AS $$
DECLARE 
	PreviousTags varchar(256);

BEGIN 
	SELECT t.TopicTags
	INTO PreviousTags
	FROM Topics AS t
	WHERE t.TopicId = SPTopicsUpdate.TopicId;

	IF TopicOrder IS NOT NULL
	THEN
		SELECT @TopicOrder = MAX(TopicOrder)+1 FROM Topics;
		SELECT @TopicOrder = COALESCE(@TopicOrder, 1);
	END IF;

	UPDATE Topics AS T
	SET
		TopicTitle = SPTopicsUpdate.TopicTitle
		,TopicDescription = SPTopicsUpdate.TopicDescription
		,TopicLastEditDate = NOW()
		,TopicTags = SPTopicsUpdate.TopicTags
		,TopicLastEditUser = SPTopicsUpdate.UserId
		,TopicLastEditIp = SPTopicsUpdate.Ip
		,TopicOrder = SPTopicsUpdate.TopicOrder
		,ReadAccessGroupId = SPTopicsUpdate.ReadAccessGroupId
		,PostAccessGroupId = SPTopicsUpdate.PostAccessGroupId
	WHERE
		T.TopicId = SPTopicsUpdate.TopicId;

	--Edit tags
	PERFORM public.SPTagsInsert(TopicTags, TopicId, PreviousTags);
	RETURN 1;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTagsInsert(VARCHAR, int, varchar);
CREATE FUNCTION public.SPTagsInsert(Tags VARCHAR(256), TopicId int, PreviousTags varchar(256) = NULL) 
	RETURNS INT
	LANGUAGE plpgsql
AS $$
DECLARE
	rowcount int;
BEGIN
	IF PreviousTags IS NOT NULL
	THEN
		DELETE FROM Tags AS t
		WHERE
			t.Tag = ANY(regexp_split_to_array(PreviousTags, E'\\s+'))
			AND t.TopicId = SPTagsInsert.TopicId;
	END IF;

	INSERT INTO Tags(Tag,TopicId)
	SELECT
		T, TopicId FROM UNNEST(regexp_split_to_array(tags, E'\\s+')) AS t;

	GET DIAGNOSTICS rowcount = ROW_COUNT;
	RETURN rowcount;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsSubscriptionsInsert(int, int);
CREATE FUNCTION public.SPTopicsSubscriptionsInsert(TopicId int, UserId int) 
	RETURNS int
	LANGUAGE plpgsql
AS $$
DECLARE 
	rowCount int = 0;
BEGIN
	IF NOT EXISTS (SELECT SUB.TopicId FROM TopicsSubscriptions AS sub WHERE sub.TopicId = SPTopicsSubscriptionsInsert.TopicId AND sub.UserID = SPTopicsSubscriptionsInsert.UserId)
	THEN
		INSERT INTO TopicsSubscriptions
			(TopicId, UserId)
		VALUES
			(SPTopicsSubscriptionsInsert.TopicId, SPTopicsSubscriptionsInsert.UserId);
		rowCount = 1;
	END IF;

	RETURN rowCount;
END;
$$;

DROP FUNCTION IF EXISTS public.SPForumsCategoriesGetAl();
CREATE FUNCTION public.SPForumsCategoriesGetAll()
	RETURNS SETOF ForumsCategories
	LANGUAGE sql
AS
$$
	SELECT 
		CategoryId
		,CategoryName
		,CategoryOrder
	FROM
		ForumsCategories
	ORDER BY
		CategoryOrder
$$;

DROP FUNCTION IF EXISTS public.SPForumsGetUsedShortNames(varchar, varchar);
CREATE FUNCTION public.SPForumsGetUsedShortNames(ForumShortName varchar(32), SearchShortName varchar(32))
	RETURNS SETOF varchar
	LANGUAGE plpgsql
AS $$
/*
	Gets used short names for forums
	returns:
		IF NOT USED SHORTNAME: empty result set
		IF USED SHORTNAME: resultset with amount of rows used
*/
DECLARE 
	CurrentValue varchar(32);
BEGIN
	SELECT 
		f.ForumShortName
	INTO CurrentValue
	FROM 
		Forums AS f
	WHERE
		f.ForumShortName = SPForumsGetUsedShortNames.ForumShortName;
		

	IF CurrentValue IS NULL
	THEN
		RETURN QUERY
		SELECT NULL::varchar As ForumShortName WHERE 1=0;
	ELSE
		RETURN QUERY
		SELECT 
			f.ForumShortName
		FROM
			Forums AS f
		WHERE
			f.ForumShortName LIKE SPForumsGetUsedShortNames.SearchShortName || '%';
	END IF;

	RETURN;
END;
$$;

DROP FUNCTION IF EXISTS public.SPForumsInsert(varchar, varchar, TEXT, int, int, smallint, smallint);
CREATE FUNCTION public.SPForumsInsert(
		ForumName varchar(255),
		ForumShortName varchar(32),
		ForumDescription TEXT,
		CategoryId int,
		UserId int,
		ReadAccessGroupId smallint,
		PostAccessGroupId smallint)
	RETURNS int
	LANGUAGE plpgsql
AS $$
BEGIN
	INSERT INTO Forums (
		ForumName
		,ForumShortName
		,ForumDescription
		,CategoryId
		,UserId
		,ForumCreationDate
		,ForumLastEditDate
		,ForumLastEditUser
		,Active
		,ForumTopicCount
		,ForumMessageCount
		,ForumOrder
		,ReadAccessGroupId
		,PostAccessGroupId
	) VALUES (
		ForumName
		,ForumShortName
		,ForumDescription
		,CategoryId
		,UserId
		,now()
		,now()
		,UserId
		,TRUE
		,0
		,0
		,0
		,ReadAccessGroupId
		,PostAccessGroupId);
		
	return 1;
END;
$$;

DROP FUNCTION IF EXISTS public.SPForumsDelete(varchar);
CREATE FUNCTION public.SPForumsDelete(ForumShortName varchar(32)) 
	RETURNS INT
	LANGUAGE plpgsql
AS $$
DECLARE
	updatedForumId int;
BEGIN
	UPDATE Forums
	SET
		Active = FALSE
	WHERE
		Forums.ForumShortName = SPForumsDelete.ForumShortName
	RETURNING ForumId
	INTO updatedForumId;

	IF updatedForumId IS NOT NULL
	THEN 
		RETURN 1;
	ELSE
		RETURN 0;
	END IF;
END;
$$;

SELECT * FROM SPForumsDelete('unit-test-forum');

DROP FUNCTION IF EXISTS public.SPForumsUpdate(varchar, varchar, text, int, int, smallint, smallint);
CREATE FUNCTION public.SPForumsUpdate(
		ForumShortName varchar(32)
		,ForumName varchar(255)
		,ForumDescription Text
		,CategoryId int
		,UserId int
		,ReadAccessGroupId smallint
		,PostAccessGroupId smallint)
	RETURNS VOID
	LANGUAGE SQL
AS $$
	UPDATE Forums
	SET
		ForumName = SPForumsUpdate.ForumName
		,ForumDescription = SPForumsUpdate.ForumDescription 
		,CategoryId = SPForumsUpdate.CategoryId
		,ForumLastEditDate = now()
		,ForumLastEditUser = SPForumsUpdate.UserId
		,ReadAccessGroupId = SPForumsUpdate.ReadAccessGroupId
		,PostAccessGroupId = SPForumsUpdate.PostAccessGroupId
	WHERE
		ForumShortName = SPForumsUpdate.ForumShortName;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetLatest(int);
CREATE FUNCTION public.SPTopicsGetLatest(
		UserGroupId int = null)
	RETURNS TABLE(
		TopicId INT,
		TopicTitle VARCHAR,
		TopicShortName VARCHAR,
		TopicDescription TEXT,
		TopicCreationDate TIMESTAMP,
		TopicViews INT,
		TopicReplies INT,
		UserId INT,
		TopicTags VARCHAR,
		TopicIsClose BOOLEAN,
		TopicOrder INT,
		LastMessageId INT,
		UserName VARCHAR,
		MessageCreationDate TIMESTAMP
	)
	LANGUAGE sql
AS $$
	/*
		Gets the latest messages in all forums	
	*/
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,T.UserName
		,M.MessageCreationDate
	FROM
		TopicsComplete T
		LEFT JOIN Messages M 
			ON M.TopicId = T.TopicId AND M.MessageId = T.LastMessageId AND M.Active
	WHERE
		COALESCE(T.ReadAccessGroupId,-1) <= COALESCE(@UserGroupId,-1)
	ORDER BY T.TopicId desc
	LIMIT 20;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetByForumLatest(int, int, int, int);
CREATE FUNCTION public.SPTopicsGetByForumLatest(
		ForumId int = 2,
		StartIndex int = 0,
		Length int = 10,
		UserGroupId int = null)
	RETURNS TABLE(
		TopicId INT,
		TopicTitle VARCHAR,
		TopicShortName VARCHAR,
		TopicDescription TEXT,
		TopicCreationDate TIMESTAMP,
		TopicViews INT,
		TopicReplies INT,
		UserId INT,
		TopicTags VARCHAR,
		TopicIsClose BOOLEAN,
		TopicOrder INT,
		LastMessageId INT,
		UserName VARCHAR,
		MessageCreationDate TIMESTAMP,
		MessageUserId INT,
		MessageUserName VARCHAR,
		ReadAccessGroupId SMALLINT,
		PostAccessGroupId SMALLINT
	)
	LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,T.UserName
		,M.MessageCreationDate
		,M.UserId AS MessageUserId
		,MU.UserName AS MessageUserName
		,T.ReadAccessGroupId
		,T.PostAccessGroupId
	FROM
		TopicsComplete T
		LEFT OUTER JOIN Messages M 
			ON M.TopicId = T.TopicId 
				AND M.MessageId = T.LastMessageId 
				AND M.Active
		LEFT JOIN Users MU ON MU.UserId = M.UserId
	WHERE
		T.ForumId = SPTopicsGetByForumLatest.ForumId
		AND
		COALESCE(T.ReadAccessGroupId,-1) <= COALESCE(SPTopicsGetByForumLatest.UserGroupId,-1)
	OFFSET SPTopicsGetByForumLatest.StartIndex LIMIT SPTopicsGetByForumLatest.Length;
END;
$$;
	
DROP FUNCTION IF EXISTS public.SPTagsGetMostViewed(int, bigint);
CREATE FUNCTION public.SPTagsGetMostViewed(
		ForumId int=2,
		Top bigint=5)
	RETURNS TABLE(
		tag varchar,
		tagViews BIGINT,
		weight NUMERIC)
	LANGUAGE plpgsql
AS $$
BEGIN
	--DROP TABLE IF EXISTS selectedTags;
	CREATE LOCAL TEMPORARY TABLE selectedTags (tag VARCHAR, tagViews BIGINT, weight REAL) ON COMMIT DROP;

	INSERT INTO selectedTags
	-- SELECT
-- 			T.TAG, 
-- 			T.TAGVIEWS, 
-- 			(TAGVIEWS*100.00)/SUM(CASE WHEN TAGVIEWS > 0 THEN TAGVIEWS ELSE 1 END)  AS WEIGHT
-- 		FROM (
		(SELECT
			Tags.Tag
			,SUM(T.TopicViews) As TagViews
			,COUNT(T.TopicId) As TopicCount 
		FROM
			Tags
			INNER JOIN Topics T ON Tags.TopicId = T.TopicId
		WHERE
			T.ForumId = SPTagsGetMostViewed.ForumId
			AND T.Active
		GROUP BY
			Tags.Tag
		ORDER BY TagViews desc
		LIMIT SPTagsGetMostViewed.Top);
		-- )  AS T
-- 		ORDER BY Tag;

	return QUERY 
	SELECT 
		t.Tag,
		t.TagViews,
		(t.TagViews * 100.00) / CASE WHEN total.TotalViews = 0 THEN 1 ELSE total.TotalViews END AS Weight
	FROM selectedTags t
	JOIN (SELECT SUM(st.TAGVIEWS) AS TOTALVIEWS FROM SelectedTags st GROUP BY true) AS total ON true;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsInsert(varchar, varchar, TEXT, int, varchar, varchar, int, varchar, smallint, smallint, OUT int);
CREATE FUNCTION public.SPTopicsInsert (
		TopicTitle varchar(255),
		TopicShortName varchar(64),
		TopicDescription TEXT,
		UserId int,
		TopicTags varchar(256),
		Forum varchar(32),
		TopicOrder int,
		Ip varchar (39),
		ReadAccessGroupId smallint,
		PostAccessGroupId smallint,
		OUT TopicId int)
	RETURNS int
	LANGUAGE plpgsql
AS $$
	/*
	- Inserts topic
	- Insert tags
	- Updates recount on father
	*/
DECLARE
	ForumId int;
BEGIN
	SELECT f.ForumId 
	INTO ForumId
	FROM Forums f WHERE ForumShortName = SPTopicsInsert.Forum;
	TopicTags = LOWER(TopicTags);

	IF SPTopicsInsert.TopicOrder IS NOT NULL
	THEN
		SELECT TopicOrder = MAX(TopicOrder)+1 FROM Topics;
		SELECT TopicOrder = ISNULL(@TopicOrder, 1);
	END IF;

	
	INSERT INTO public.Topics (
		TopicTitle
		,TopicShortName
		,TopicDescription
		,TopicCreationDate
		,TopicLastEditDate
		,TopicViews
		,TopicReplies
		,UserId
		,TopicTags
		,ForumId
		,TopicLastEditUser
		,TopicLastEditIp
		,Active
		,TopicIsClose
		,TopicOrder
		,MessagesIdentity
		,ReadAccessGroupId
		,PostAccessGroupId
	) VALUES (
		SPTopicsInsert.TopicTitle,
		SPTopicsInsert.TopicShortName,
		SPTopicsInsert.TopicDescription,
		now(),
		now(),
		0,--TopicViews
		0,--TopicReplies
		SPTopicsInsert.UserId,
		SPTopicsInsert.TopicTags,
		ForumId,
		SPTopicsInsert.UserId,
		SPTopicsInsert.Ip,
		TRUE,--Active
		FALSE,--TopicIsClose
		SPTopicsInsert.TopicOrder,
		0,--MessageIdentity
		SPTopicsInsert.ReadAccessGroupId,
		SPTopicsInsert.PostAccessGroupId)
	RETURNING Topics."topicid"
	INTO SPTopicsInsert.TopicId;

	--Add tags
	PERFORM public.SPTagsInsert(Tags:=TopicTags, TopicId:=TopicId);

	--Recount
	PERFORM public.SPForumsUpdateRecount(ForumId:=ForumId);

	return;
END;
$$;

DROP FUNCTION IF EXISTS public.SPForumsUpdateRecount(INT);
CREATE FUNCTION public.SPForumsUpdateRecount(ForumId int)
	RETURNS void
	LANGUAGE plpgsql
AS $$
/*
	RECOUNTS THE CHILDREN MESSAGES AND TOPICS
*/
DECLARE 
	COUNTS RECORD;
	ForumTopicCount int;
	ForumMessageCount int;
BEGIN

	SELECT
		COUNT(TopicId) AS ForumTopicCount,
		SUM(TopicReplies) AS ForumMessageCount
	INTO counts
	FROM Topics t
	WHERE
		t.ForumId = SPForumsUpdateRecount.ForumId
		AND Active;

	UPDATE Forums
	SET 
		ForumTopicCount = COALESCE(counts.ForumTopicCount, 0)
		,ForumMessageCount = COALESCE(counts.ForumMessageCount, 0)
	WHERE	
		Forums.ForumId = SPForumsUpdateRecount.ForumId;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsDelete(int, int, varchar);
CREATE FUNCTION public.SPTopicsDelete(TopicId int, UserId int, Ip varchar (39))
	RETURNS void
	LANGUAGE plpgsql
AS $$
/*
- SETS THE TOPIC ACTIVE=0
- UPDATES RECOUNT ON FORUM
*/
DECLARE 
	ForumId int;
BEGIN
	SELECT t.ForumId 
	INTO ForumId
	FROM Topics t WHERE t.TopicId = SPTopicsDelete.TopicId;

	UPDATE Topics
	SET
		Active = FALSE,
		TopicLastEditDate = now(),
		TopicLastEditUser = SPTopicsDelete.UserId,
		TopicLastEditIp = SPTopicsDelete.Ip
	WHERE
		Topics.TopicId = SPTopicsDelete.TopicId;

	PERFORM public.SPForumsUpdateRecount(ForumId);
END;
$$;

DROP FUNCTION IF EXISTS public.SPUsersWarn(int, int, int, text);
CREATE FUNCTION public.SPUsersWarn (
		UserId int,
		ModeratorUserId int,
		ModeratorReason int,
		ModeratorReasonFull text)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Users
	SET
		WarningStart = now()
		, WarningRead = FALSE
		, ModeratorReason = SPUsersWarn.ModeratorReason
		, ModeratorReasonFull = SPUsersWarn.ModeratorReasonFull
		, ModeratorUserId = SPUsersWarn.ModeratorUserId
	WHERE 
		UserId = SPUsersWarn.UserId
$$;

DROP FUNCTION IF EXISTS public.SPUsersDelete(int);
CREATE FUNCTION public.SPUsersDelete(UserId int)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Users U
	SET	
		Active = FALSE
	WHERE 
		U.UserId = SPUsersDelete.UserId;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetUnanswered();
CREATE FUNCTION public.SPTopicsGetUnanswered()
	RETURNS TABLE (
		TopicId int
		,TopicTitle varchar
		,TopicShortName varchar
		,TopicDescription text
		,TopicCreationDate timestamp
		,TopicViews int
		,TopicReplies int
		,UserId int
		,TopicTags varchar
		,TopicIsClose boolean
		,TopicOrder int
		,LastMessageId int
		,UserName varchar
		,MessageCreationDate timestamp
		,ForumId int
		,ForumName varchar
		,ForumShortName varchar
	)
	LANGUAGE sql
AS $$
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,T.UserName
		,M.MessageCreationDate
		,T.ForumId
		,T.ForumName
		,T.ForumShortName
	FROM
		TopicsComplete T
		LEFT JOIN Messages M ON M.TopicId = T.TopicId AND M.MessageId = T.LastMessageId AND M.Active
	WHERE
		T.TopicReplies = 0 -- Unanswered
		AND T.TopicOrder IS NULL -- Not sticky	
	ORDER BY 
		TopicId DESC;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetByForumUnanswered(int, int);
CREATE FUNCTION public.SPTopicsGetByForumUnanswered(
		ForumId int = 2,
		UserGroupId int = null)
	RETURNS TABLE(
		TopicId INT
		,TopicTitle VARCHAR
		,TopicShortName VARCHAR
		,TopicDescription TEXT
		,TopicCreationDate TIMESTAMP
		,TopicViews INT
		,TopicReplies INT
		,UserId INT
		,TopicTags VARCHAR
		,TopicIsClose BOOLEAN
		,TopicOrder INT
		,LastMessageId INT
		,UserName VARCHAR
		,MessageCreationDate TIMESTAMP
		,MessageUserId INT
		,MessageUserName VARCHAR
		,ReadAccessGroupId SMALLINT
		,PostAccessGroupId SMALLINT)
	LANGUAGE sql
AS $$
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,T.UserName
		,M.MessageCreationDate
		,M.UserId AS MessageUserId
		,MU.UserName AS MessageUserName
		,T.ReadAccessGroupId
		,T.PostAccessGroupId
	FROM
		TopicsComplete T
		LEFT JOIN Messages M ON M.TopicId = T.TopicId AND M.MessageId = T.LastMessageId AND M.Active
		LEFT JOIN Users MU ON MU.UserId = M.UserId
	WHERE
		T.ForumId = @ForumId
		AND
		T.TopicReplies = 0 -- Unanswered
		AND
		T.TopicOrder IS NULL -- Not sticky	
		AND
		COALESCE(T.ReadAccessGroupId,-1) <= COALESCE(@UserGroupId,-1)
	ORDER BY 
		TopicViews DESC, TopicId DESC;
$$;

DROP FUNCTION IF EXISTS public.SPUsersBan (int, int, int, TEXT);
CREATE FUNCTION public.SPUsersBan (
		UserId int
		, ModeratorUserId int
		, ModeratorReason int
		, ModeratorReasonFull TEXT)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Users
	SET
		BannedStart = now()
		, ModeratorReason = SPUsersBan.ModeratorReason
		, ModeratorReasonFull = SPUsersBan.ModeratorReasonFull
		, ModeratorUserId = SPUsersBan.ModeratorUserId
	WHERE 
		UserId = @UserId;
$$;

DROP FUNCTION IF EXISTS public.SPUsersGet(int);
CREATE FUNCTION public.SPUsersGet(
		UserId int = 11)
	RETURNS TABLE (
		UserId INT
		,UserName VARCHAR
		,UserProfile VARCHAR
		,UserSignature VARCHAR
		,UserGroupId SMALLINT
		,UserBirthDate TIMESTAMP
		,UserWebsite VARCHAR
		,UserTimezone decimal(9, 2)
		,UserPhoto VARCHAR
		,UserRegistrationDate TIMESTAMP
		,UserExternalProfileUrl VARCHAR
		,UserEmail VARCHAR
		,UserEmailPolicy INT
		--,UserGroupId INT
		,UserGroupName VARCHAR
		,WarningStart TIMESTAMP
		,WarningRead BOOLEAN
		,SuspendedStart TIMESTAMP
		,SuspendedEnd TIMESTAMP
		,BannedStart TIMESTAMP
		,ModeratorReasonFull VARCHAR
		,ModeratorReason INT
		,ModeratorUserId INT
	)
	LANGUAGE sql
AS $$
	SELECT
		U.UserId
		,U.UserName
		,U.UserProfile
		,U.UserSignature
		,U.UserGroupId
		,U.UserBirthDate
		,U.UserWebsite
		,U.UserTimezone
		,U.UserPhoto
		,U.UserRegistrationDate
		,U.UserExternalProfileUrl
		,U.UserEmail
		,U.UserEmailPolicy
		--,UG.UserGroupId
		,UG.UserGroupName
		,U.WarningStart
		,U.WarningRead
		,U.SuspendedStart
		,U.SuspendedEnd
		,U.BannedStart
		,U.ModeratorReasonFull
		,U.ModeratorReason
		,U.ModeratorUserId
	FROM
		Users U
		INNER JOIN UsersGroups UG ON UG.UserGroupId = U.UserGroupId
	WHERE
		U.UserId = SPUsersGet.UserId;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetMessagesByUser(int);
CREATE FUNCTION public.SPTopicsGetMessagesByUser(
		UserId int)
	RETURNS TABLE(
		TopicId INT
		,MessageId INT
		,MessageCreationDate TIMESTAMP
		,TopicTitle VARCHAR
		,TopicShortName VARCHAR
		,TopicDescription TEXT
		,TopicCreationDate TIMESTAMP
		,TopicViews INT
		,TopicReplies INT
		,UserId INT
		,TopicTags VARCHAR
		,TopicIsClose BOOLEAN
		,TopicOrder INT)
	LANGUAGE SQL
AS $$
	/*
	Gets the messages posted by the user grouped by topic
	*/
	SELECT
		T.TopicId
		,M.MessageId
		,M.MessageCreationDate
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
	FROM
		TopicsComplete T
		INNER JOIN Messages M ON M.TopicId = T.TopicId
	WHERE
		M.UserId = SPTopicsGetMessagesByUser.UserId
	ORDER BY T.TopicId desc, M.MessageId desc;
$$;

DROP FUNCTION IF EXISTS public.SPMessagesFlagsGetAll();
CREATE FUNCTION public.SPMessagesFlagsGetAll()
	RETURNS TABLE(
		TopicId INT
		,MessageId INT
		,TotalFlags INT
		,TopicTitle VARCHAR
		,TopicShortName VARCHAR
		,ForumId INT
		,ForumShortName VARCHAR
		,ForumName VARCHAR
		,MessageBody TEXT
		,UserName VARCHAR
		,UserId INT)
	LANGUAGE sql
AS $$
	/*
		Lists all flagged messages (not topics)
	*/
	SELECT
		F.TopicId
		,F.MessageId
		,COUNT(FlagId)::INT AS TotalFlags
		,T.TopicTitle
		,T.TopicShortName
		,Forums.ForumId
		,Forums.ForumShortName
		,Forums.ForumName
		,M.MessageBody
		,M.UserName
		,M.UserId
	FROM
		Flags F
		INNER JOIN Topics T ON T.TopicId = F.TopicId
		INNER JOIN Forums ON Forums.ForumId = T.ForumId
		INNER JOIN MessagesComplete M ON M.TopicId = T.TopicId AND M.MessageId = F.MessageId
	WHERE
		T.Active
		AND M.Active
	GROUP BY	
		F.TopicId
		,F.MessageId
		,T.TopicTitle
		,T.TopicShortName
		,Forums.ForumId
		,Forums.ForumShortName
		,Forums.ForumName
		,M.MessageBody
		,M.UserName
		,M.UserId
	ORDER BY COUNT(FlagId) DESC, F.TopicId;
$$;

DROP FUNCTION IF EXISTS public.SPPageContentsGetUsedShortNames(varchar, varchar);
CREATE FUNCTION public.SPPageContentsGetUsedShortNames(
		PageContentShortName varchar(32), 
		SearchShortName varchar(32))
	RETURNS TABLE(
		ForumShortName VARCHAR)
	LANGUAGE plpgsql
AS $$
	/*
		Gets used short names for PageContents
		returns:
			IF NOT USED SHORTNAME: empty result set
			IF USED SHORTNAME: resultset with amount of rows used
	*/
BEGIN
	IF NOT EXISTS (SELECT pc.PageContentShortName FROM  PageContents pc WHERE pc.PageContentShortName = SPPageContentsGetUsedShortNames.PageContentShortName)
	THEN
		RETURN QUERY
		SELECT NULL::VARCHAR As ForumShortName WHERE 1=0;
	ELSE
		RETURN QUERY
		SELECT 
			pc.PageContentShortName
		FROM
			PageContents pc
		WHERE
			pc.PageContentShortName LIKE SPPageContentsGetUsedShortNames.SearchShortName || '%'
			OR pc.PageContentShortName = SPPageContentsGetUsedShortNames.PageContentShortName;
	END IF;
END;
$$;

DROP FUNCTION IF EXISTS public.SPPageContentsInsert(varchar, varchar, TEXT);
CREATE FUNCTION public.SPPageContentsInsert(
		PageContentShortName varchar(128),
		PageContentTitle varchar(128),
		PageContentBody TEXT)
	RETURNS void
	LANGUAGE sql
AS $$
	INSERT INTO PageContents (
		PageContentTitle
		,PageContentBody
		,PageContentShortName
		,PageContentEditDate)
	VALUES (
		SPPageContentsInsert.PageContentTitle
		,SPPageContentsInsert.PageContentBody
		,SPPageContentsInsert.PageContentShortName
		,now());
$$;

DROP FUNCTION IF EXISTS public.SPPageContentsUpdate(varchar, varchar, TEXT);
CREATE FUNCTION public.SPPageContentsUpdate(
		PageContentShortName varchar(128),
		PageContentTitle varchar(128),
		PageContentBody TEXT)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE PageContents 
	SET
		PageContentTitle = SPPageContentsUpdate.PageContentTitle
		,PageContentBody = SPPageContentsUpdate.PageContentBody
		,PageContentEditDate = now()
	WHERE
		PageContentShortName = SPPageContentsUpdate.PageContentShortName;
$$;

DROP FUNCTION IF EXISTS public.SPPageContentsGet(varchar);
CREATE FUNCTION public.SPPageContentsGet(
	PageContentShortName varchar(128)='about')
	RETURNS TABLE (
		PageContentId INT
		,PageContentTitle VARCHAR
		,PageContentBody VARCHAR
		,PageContentShortName VARCHAR)
	LANGUAGE sql
AS $$
	SELECT
		pc.PageContentId
		,pc.PageContentTitle
		,pc.PageContentBody
		,pc.PageContentShortName
	FROM
		public.PageContents pc
	WHERE
		PageContentShortName = SPPageContentsGet.PageContentShortName;
$$;

DROP FUNCTION IF EXISTS public.SPPageContentsGetAll();
CREATE FUNCTION public.SPPageContentsGetAll()
	RETURNS TABLE (
		PageContentId INT
		,PageContentTitle VARCHAR
		,PageContentBody VARCHAR
		,PageContentShortName VARCHAR)
	LANGUAGE sql
AS $$
	SELECT
		PageContentId
		,PageContentTitle
		,PageContentBody
		,PageContentShortName
	FROM
		public.PageContents
	ORDER BY
		PageContentTitle;
$$;

DROP FUNCTION IF EXISTS public.SPPageContentsDelete(varchar);
CREATE FUNCTION public.SPPageContentsDelete(
		PageContentShortName varchar(128))
	RETURNS void
	LANGUAGE sql
AS $$
	DELETE FROM PageContents 
	WHERE
		PageContentShortName = SPPageContentsDelete.PageContentShortName;
$$;

DROP FUNCTION IF EXISTS public.SPMessagesGetByTopicFrom(int, int, int);
CREATE FUNCTION public.SPMessagesGetByTopicFrom(
		TopicId int=1,
		FirstMsg int=13,
		Amount int=10)
	RETURNS TABLE (
		TopicId INT
		,MessageId INT
		,MessageBody VARCHAR
		,MessageCreationDate TIMESTAMP
		,MessageLastEditDate TIMESTAMP
		,ParentId INT
		,UserId INT
		,UserName VARCHAR
		,UserSignature VARCHAR
		,UserGroupId SMALLINT
		,UserGroupName VARCHAR
		,UserPhoto VARCHAR
		,UserRegistrationDate TIMESTAMP
		,Active BOOLEAN)
	LANGUAGE sql
AS $$
	SELECT 
		M.TopicId
		,M.MessageId
		,M.MessageBody
		,M.MessageCreationDate
		,M.MessageLastEditDate
		,M.ParentId
		,UserId
		,UserName
		,UserSignature
		,UserGroupId
		,UserGroupName
		,UserPhoto
		,UserRegistrationDate
		,M.Active
	FROM 
		public.MessagesComplete M
	WHERE 
		M.TopicId = SPMessagesGetByTopicFrom.TopicId
	OFFSET SPMessagesGetByTopicFrom.FirstMsg LIMIT SPMessagesGetByTopicFrom.Amount;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetByUser(int, int);
CREATE FUNCTION public.SPTopicsGetByUser(
		UserId int,
		UserGroupId int = null)
	RETURNS TABLE(
		TopicId INT
		,TopicTitle VARCHAR
		,TopicShortName VARCHAR
		,TopicDescription VARCHAR
		,TopicCreationDate TIMESTAMP
		,TopicViews INT
		,TopicReplies INT
		,UserId INT
		,TopicTags VARCHAR
		,TopicIsClose BOOLEAN
		,TopicOrder INT
		,LastMessageId INT
		,UserName VARCHAR
		,MessageCreationDate TIMESTAMP
		,ReadAccessGroupId SMALLINT
		,PostAccessGroupId SMALLINT)
	LANGUAGE sql
AS $$
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.TopicDescription
		,T.TopicCreationDate
		,T.TopicViews
		,T.TopicReplies
		,T.UserId
		,T.TopicTags
		,T.TopicIsClose
		,T.TopicOrder
		,T.LastMessageId
		,T.UserName
		,M.MessageCreationDate
		,T.ReadAccessGroupId
		,T.PostAccessGroupId
	FROM
		TopicsComplete T
		LEFT JOIN Messages M ON M.TopicId = T.TopicId AND M.MessageId = T.LastMessageId AND M.Active
	WHERE
		T.UserId = SPTopicsGetByUser.UserId
		AND COALESCE(T.ReadAccessGroupId,-1) <= COALESCE(SPTopicsGetByUser.UserGroupId,-1)
	ORDER BY T.TopicId DESC;
$$;

DROP FUNCTION IF EXISTS public.SPTemplatesGetCurrent();
CREATE FUNCTION public.SPTemplatesGetCurrent()
	RETURNS TABLE(
		TemplateId INT
		,TemplateKey VARCHAR
		,TemplateDescription VARCHAR)
	LANGUAGE sql
AS $$
	SELECT
		TemplateId
		,TemplateKey
		,TemplateDescription
	FROM
		Templates
	WHERE
		TemplateIsCurrent;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsSubscriptionsDelete(int, int, char);
CREATE FUNCTION public.SPTopicsSubscriptionsDelete(
		TopicId int,
		UserId int,
		Userguid char(32))
	RETURNS void
	LANGUAGE sql
AS $$
	DELETE FROM TopicsSubscriptions
	WHERE TopicsSubscriptions.TopicId IN 
		(SELECT S.TopicId
		FROM TopicsSubscriptions S
		INNER JOIN Users U ON U.UserId = S.UserId
		WHERE
			S.TopicId = SPTopicsSubscriptionsDelete.TopicId
			AND S.UserId = SPTopicsSubscriptionsDelete.UserId	
			AND U.UserGuid = SPTopicsSubscriptionsDelete.UserGuid);
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetByRelated(varchar, varchar, varchar, varchar, varchar, varchar, int, int, int);
CREATE FUNCTION public.SPTopicsGetByRelated(
		Tag1 varchar(50)='problem'
		,Tag2 varchar(50)='installation'
		,Tag3 varchar(50)='copy'
		,Tag4 varchar(50)=null
		,Tag5 varchar(50)=null
		,Tag6 varchar(50)=null
		,TopicId int=1
		,Amount int=5
		,UserGroupId int = null)
	RETURNS TABLE(
		TagCount INT
		,TopicId INT
		,TopicTitle VARCHAR
		,TopicShortName VARCHAR 
		,TopicDescription VARCHAR
		,TopicCreationDate TIMESTAMP
		,TopicViews INT
		,TopicReplies INT
		,ForumId INT
		,ForumName VARCHAR
		,ForumShortName VARCHAR
		,TopicIsClose BOOLEAN
		,TopicOrder INT
		,ReadAccessGroupId SMALLINT
		,PostAccessGroupId SMALLINT)
	LANGUAGE sql
AS $$
	WITH TagsParams (Tag) AS (
		SELECT SPTopicsGetByRelated.Tag1
		UNION
		SELECT SPTopicsGetByRelated.Tag2
		UNION
		SELECT SPTopicsGetByRelated.Tag3
		UNION
		SELECT SPTopicsGetByRelated.Tag4
		UNION
		SELECT SPTopicsGetByRelated.Tag5
		UNION
		SELECT SPTopicsGetByRelated.Tag6
	)
	SELECT
		Ta.TagCount
		,Topics.TopicId
		,Topics.TopicTitle
		,Topics.TopicShortName
		,Topics.TopicDescription
		,Topics.TopicCreationDate
		,Topics.TopicViews
		,Topics.TopicReplies
		,Topics.ForumId
		,Topics.ForumName
		,Topics.ForumShortName
		,Topics.TopicIsClose
		,Topics.TopicOrder
		,Topics.ReadAccessGroupId
		,Topics.PostAccessGroupId
	FROM
		(
		SELECT 
			T.TopicId
			,COUNT(T.Tag)::INT AS TagCount
		FROM 
			Tags T
			INNER JOIN TagsParams P ON T.Tag=P.Tag
		WHERE
			T.Tag=P.Tag
		GROUP BY
			T.TopicId
		)
		Ta
		INNER JOIN TopicsComplete Topics ON Topics.TopicId = Ta.TopicId
	WHERE
		Topics.TopicId <> SPTopicsGetByRelated.TopicId
		AND COALESCE(Topics.ReadAccessGroupId,-1) <= COALESCE(SPTopicsGetByRelated.UserGroupId,-1)
	ORDER BY
		Topics.TopicViews DESC
	LIMIT SPTopicsGetByRelated.Amount;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsGetByTag(varchar, int, int);
CREATE FUNCTION public.SPTopicsGetByTag(
		Tag varchar(50)='forum'
		,ForumId int=2
		,UserGroupId int = null)
	RETURNS TABLE (
		TopicId INT
		,TopicTitle VARCHAR
		,TopicShortName VARCHAR
		,TopicDescription VARCHAR
		,TopicCreationDate TIMESTAMP
		,TopicViews INT
		,TopicReplies INT
		,UserId INT
		,TopicTags VARCHAR
		,TopicIsClose BOOLEAN
		,TopicOrder INT
		,LastMessageId INT
		,UserName VARCHAR
		,MessageCreationDate TIMESTAMP
		,MessageUserId INT
		,MessageUserName VARCHAR
		,ReadAccessGroupId SMALLINT
		,PostAccessGroupId SMALLINT)
	LANGUAGE sql
AS $$
	SELECT
			T.TopicId
			,T.TopicTitle
			,T.TopicShortName
			,T.TopicDescription
			,T.TopicCreationDate
			,T.TopicViews
			,T.TopicReplies
			,T.UserId
			,T.TopicTags
			,T.TopicIsClose
			,T.TopicOrder
			,T.LastMessageId
			,T.UserName
			,M.MessageCreationDate
			,M.UserId AS MessageUserId
			,MU.UserName AS MessageUserName
			,T.ReadAccessGroupId
			,T.PostAccessGroupId
	FROM
		Tags
		INNER JOIN TopicsComplete T ON T.TopicId = Tags.TopicId
		LEFT JOIN Messages M ON M.TopicId = T.TopicId AND M.MessageId = T.LastMessageId AND M.Active
		LEFT JOIN Users MU ON MU.UserId = M.UserId
	WHERE
		Tags.Tag LIKE SUBSTRING(SPTopicsGetByTag.Tag, 1, LENGTH(SPTopicsGetByTag.Tag)-1) || '%'
		AND T.ForumId = SPTopicsGetByTag.ForumId
		AND COALESCE(T.ReadAccessGroupId,-1) <= COALESCE(SPTopicsGetByTag.UserGroupId,-1)
	ORDER BY TopicOrder DESC, TopicViews DESC;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsAddVisit(int);
CREATE FUNCTION public.SPTopicsAddVisit(
		TopicId int=2)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Topics
	SET
		TopicViews = TopicViews+1
	WHERE
		TopicId = SPTopicsAddVisit.TopicId;
$$;

DROP FUNCTION IF EXISTS public.SPMessagesFlag(int, int, varchar);
CREATE FUNCTION public.SPMessagesFlag (
		TopicId int=1
		,MessageId int=1
		,Ip varchar (39)='127.0.0.1')
	RETURNS void
	LANGUAGE plpgsql
AS $$
BEGIN
	IF NOT EXISTS (
		SELECT * 
		FROM Flags f 
		WHERE f.TopicId=SPMessagesFlag.TopicId 
			AND f.IP=SPMessagesFlag.Ip 
			AND (f.MessageId = SPMessagesFlag.MessageId OR (SPMessagesFlag.MessageId IS NULL AND f.MessageId IS NULL)))
	THEN
		INSERT INTO Flags
			(TopicId, MessageId, Ip, FlagDate)
		VALUES
			(SPMessagesFlag.TopicId, SPMessagesFlag.MessageId, SPMessagesFlag.Ip, now());
	END IF;
END;
$$;

DROP FUNCTION IF EXISTS public.SPForumsCategoriesInsert (varchar, int);
CREATE FUNCTION public.SPForumsCategoriesInsert(
		categoryName varchar(255), 
		categoryOrder int)
	RETURNS ForumsCategories
	LANGUAGE sql
AS $$
	INSERT INTO ForumsCategories
	(
		CategoryName,
		CategoryOrder
	)
	VALUES
	(
		SPForumsCategoriesInsert.categoryName,
		SPForumsCategoriesInsert.categoryOrder
	)returning forumscategories.*;
$$;

DROP FUNCTION IF EXISTS public.SPForumsCategoriesGet(int);
CREATE FUNCTION public.SPForumsCategoriesGet(
		CategoryId int)
	RETURNS SETOF ForumsCategories
	LANGUAGE sql
AS $$
	SELECT * FROM ForumsCategories WHERE CategoryId = SPForumsCategoriesGet.CategoryId;
$$;

DROP FUNCTION IF EXISTS public.SPForumsCategoriesUpdate(int, varchar, int);
CREATE FUNCTION public.SPForumsCategoriesUpdate(
		CategoryId int,
		CategoryName varchar(255),
		CategoryOrder int)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE ForumsCategories
	SET
		CategoryName = SPForumsCategoriesUpdate.CategoryName,
		CategoryOrder = SPForumsCategoriesUpdate.CategoryOrder
	WHERE
		CategoryId = SPForumsCategoriesUpdate.CategoryId;
$$;

DROP FUNCTION IF EXISTS public.SPForumsCategoriesGetForumsCountPerCategory(int);
CREATE FUNCTION public.SPForumsCategoriesGetForumsCountPerCategory(
		CategoryId int)
	RETURNS int
	LANGUAGE sql
AS $$
	SELECT  count(*)::INT as NoofForums FROM Forums WHERE CategoryId = SPForumsCategoriesGetForumsCountPerCategory.CategoryId;
$$;

DROP FUNCTION IF EXISTS public.SPForumsCategoriesDelete(int);
CREATE FUNCTION public.SPForumsCategoriesDelete(
		categoryId int) 
	RETURNS int
	LANGUAGE plpgsql
AS $$
DECLARE 
	deleted int;
BEGIN
	DELETE FROM ForumsCategories fc WHERE fc.CategoryId = SPForumsCategoriesDelete.categoryId
	RETURNING 1
	INTO deleted;

	IF deleted IS NOT NULL 
	THEN 
		return 1;
	ELSE
		return 0;
	END IF;

	RAISE NOTICE 'DELETED : %', deleted;

	RETURN deleted;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsSubscriptionsGetByTopic(int);
CREATE FUNCTION public.SPTopicsSubscriptionsGetByTopic(
		TopicId int)
	RETURNS TABLE(
		UserId INT
		,UserName VARCHAR
		,UserEmail VARCHAR
		,UserEmailPolicy INT
		,UserGuid CHAR)
	LANGUAGE sql
AS $$
	/*
		Gets the active users subscribed to a topic.
		Checks read access of topic vs user role
	*/
	SELECT
		U.UserId
		,U.UserName
		,U.UserEmail
		,U.UserEmailPolicy
		,U.UserGuid
	FROM
		TopicsSubscriptions S
		INNER JOIN Topics T ON T.TopicId = S.TopicId
		INNER JOIN Users U ON U.UserId = S.UserId
	WHERE
		S.TopicId = SPTopicsSubscriptionsGetByTopic.TopicId
		AND U.Active
		AND U.UserGroupId >= COALESCE(T.ReadAccessGroupId, -1)
$$;

DROP FUNCTION IF EXISTS public.SPMessagesInsert(int, text, int, OUT int, varchar, int);
CREATE FUNCTION public.SPMessagesInsert(
		TopicId int
		,MessageBody TEXT
		,UserId int
		,OUT MessageId int
		,Ip varchar(39)
		,ParentId int)
	RETURNS INT
	LANGUAGE plpgsql
AS $$
BEGIN
	UPDATE Topics T
	SET
		MessagesIdentity = T.MessagesIdentity+1
	WHERE
		T.TopicId = SPMessagesInsert.TopicId
	RETURNING T.MessagesIdentity
	INTO SPMessagesInsert.MessageId;

	INSERT INTO Messages (
		TopicId
		,MessageId
		,MessageBody
		,MessageCreationDate
		,MessageLastEditDate
		,MessageLastEditUser
		,UserId
		,Active
		,EditIp
		,ParentId
	) VALUES (
		SPMessagesInsert.TopicId
		,SPMessagesInsert.MessageId
		,SPMessagesInsert.MessageBody
		,now()
		,now()
		,SPMessagesInsert.UserId
		,SPMessagesInsert.UserId
		,TRUE
		,SPMessagesInsert.Ip
		,SPMessagesInsert.ParentId
	);


	
	--Update topic
	PERFORM SPTopicsUpdateLastMessage(TopicId:=SPMessagesInsert.TopicId, MessageId:=SPMessagesInsert.MessageId);
	--Update forums
	PERFORM SPForumsUpdateLastMessage(TopicId:=SPMessagesInsert.TopicId, MessageId:=SPMessagesInsert.MessageId);

	RETURN;
	END;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsUpdateLastMessage(int, int);
CREATE FUNCTION public.SPTopicsUpdateLastMessage(
		TopicId int
		,MessageId int)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Topics
	SET
		TopicReplies = TopicReplies + 1
		,LastMessageId = SPTopicsUpdateLastMessage.MessageId
	WHERE
		TopicId = SPTopicsUpdateLastMessage.TopicId;
$$;

DROP FUNCTION IF EXISTS public.SPForumsUpdateLastMessage(int, int);
CREATE FUNCTION public.SPForumsUpdateLastMessage(
		TopicId int
		,MessageId int)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Forums f
	SET
		ForumMessageCount = f.ForumMessageCount + 1
	FROM
		Topics T
		--INNER JOIN Forums F ON F.ForumId = T.ForumId
	WHERE 
		f.ForumId = t.ForumId
		AND T.TopicId = SPForumsUpdateLastMessage.TopicId;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsMove(int, int, int, varchar);
CREATE FUNCTION public.SPTopicsMove(
		TopicId int
		,ForumId int
		,UserId int
		,Ip varchar (39))
	RETURNS INT
	LANGUAGE plpgsql
AS $$
DECLARE 
	PreviousForumId int;
BEGIN
	SELECT T.ForumId 
	INTO PreviousForumId
	FROM Topics T WHERE T.TopicId = SPTopicsMove.TopicId;
	
	UPDATE Topics T
	SET
		ForumId = SPTopicsMove.ForumId
		,TopicLastEditDate = now()
		,TopicLastEditUser = SPTopicsMove.UserId
		,TopicLastEditIp = SPTopicsMove.Ip
	WHERE
		T.TopicId = SPTopicsMove.TopicId;

	PERFORM SPForumsUpdateRecount(SPTopicsMove.ForumId);
	PERFORM SPForumsUpdateRecount(PreviousForumId);

	RETURN 1;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsClose(int, int, varchar);
CREATE FUNCTION public.SPTopicsClose(
	TopicId int
	,UserId int
	,Ip varchar (39))
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Topics
	SET
		TopicIsClose = TRUE
		,TopicLastEditDate = now()
		,TopicLastEditUser = SPTopicsClose.UserId
		,TopicLastEditIp = SPTopicsClose.Ip
	WHERE
		TopicId = SPTopicsClose.TopicId;
$$;

DROP FUNCTION IF EXISTS public.SPMessagesFlagsClear(int, int);
CREATE FUNCTION public.SPMessagesFlagsClear(
		TopicId int=1
		,MessageId int=1)
	RETURNS void
	LANGUAGE sql
AS $$
	DELETE FROM 
		Flags
	WHERE
		TopicId = SPMessagesFlagsClear.TopicId
		AND MessageId = SPMessagesFlagsClear.MessageId;
$$;

DROP FUNCTION IF EXISTS public.SPMessagesDelete(int, int, int);
CREATE FUNCTION public.SPMessagesDelete(
	TopicId int
	,MessageId int
	,UserId int)
	RETURNS void
	LANGUAGE sql
AS $$ 
	UPDATE Messages
	SET
		Active = FALSE
		,MessageLastEditDate = now()
		,MessageLastEditUser = SPMessagesDelete.UserId
	WHERE
		TopicId = SPMessagesDelete.TopicId
		AND MessageId = SPMessagesDelete.MessageId;
$$;


DROP FUNCTION IF EXISTS public.SPUsersGetAll();
CREATE FUNCTION public.SPUsersGetAll()
	RETURNS TABLE(
		UserId INT
		,UserName VARCHAR
		,UserProfile VARCHAR
		,UserSignature VARCHAR
		,UserGroupId SMALLINT
		,UserBirthDate TIMESTAMP
		,UserWebsite VARCHAR
		,UserTimezone DECIMAL(9,2)
		,UserPhoto VARCHAR
		,UserRegistrationDate TIMESTAMP
		,UserGroupName VARCHAR)
	LANGUAGE sql
AS $$
	SELECT
		U.UserId
		,U.UserName
		,U.UserProfile
		,U.UserSignature
		,U.UserGroupId
		,U.UserBirthDate
		,U.UserWebsite
		,U.UserTimezone
		,U.UserPhoto
		,U.UserRegistrationDate
		,UG.UserGroupName
	FROM
		Users U
		INNER JOIN UsersGroups UG ON UG.UserGroupId = U.UserGroupId
	WHERE
		U.Active
	ORDER BY 
		U.UserName;
$$;

DROP FUNCTION IF EXISTS public.SPUsersDemote(int);
CREATE FUNCTION public.SPUsersDemote(
		UserId int)
	RETURNS void
	LANGUAGE plpgsql
AS $$
DECLARE 
	lUserGroupId int;
BEGIN
	SELECT u.UserGroupId INTO lUserGroupId FROM Users u WHERE u.UserId = SPUsersDemote.UserId;
	SELECT MAX(u.UserGroupId) INTO lUserGroupId FROM UsersGroups u WHERE u.UserGroupId < lUserGroupId;

	IF lUserGroupId IS NOT NULL
	THEN
		UPDATE Users u
		SET
			UserGroupId = lUserGroupId
		WHERE
			u.UserId = SPUsersDemote.UserId;
	END IF;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTemplatesGetAll();
CREATE FUNCTION public.SPTemplatesGetAll()
	RETURNS TABLE(
		TemplateId INT
		,TemplateKey VARCHAR
		,TemplateDescription VARCHAR
		,TemplateIsCurrent BOOLEAN)
	LANGUAGE sql
AS $$
	SELECT
		TemplateId
		,TemplateKey
		,TemplateDescription
		,TemplateIsCurrent
	FROM
		Templates
$$;

DROP FUNCTION IF EXISTS public.SPTemplatesInsert(varchar, varchar, OUT int);
CREATE FUNCTION public.SPTemplatesInsert(
	TemplateKey varchar(64)
	,TemplateDescription varchar(256)
	,OUT TemplateId int)
	RETURNS int
	LANGUAGE plpgsql
AS $$
BEGIN
	SELECT t.TemplateId
	INTO SPTemplatesInsert.TemplateId
	FROM Templates t WHERE t.TemplateKey = SPTemplatesInsert.TemplateKey;
	
	IF SPTemplatesInsert.TemplateId IS NULL
	THEN
		INSERT INTO Templates (
			TemplateKey
			,TemplateDescription
			,TemplateDate
			,TemplateIsCurrent)
		VALUES (
			SPTemplatesInsert.TemplateKey
			,TemplateDescription
			,now()
			,FALSE)
		RETURNING Templates.TemplateId
		INTO SPTemplatesInsert.TemplateId;
	ELSE
		UPDATE Templates
		SET
			TemplateDescription = SPTemplatesInsert.TemplateDescription
			,TemplateDate = NOW()
		WHERE 
			Templates.TemplateKey=SPTemplatesInsert.TemplateKey;
	END IF;
END;
$$;

DROP FUNCTION IF EXISTS public.SPTemplatesUpdateCurrent(int);
CREATE FUNCTION public.SPTemplatesUpdateCurrent(
		TemplateId int)
	RETURNS void
	LANGUAGE sql
AS $$
	UPDATE Templates
	SET
		TemplateIsCurrent = 
			CASE WHEN TemplateId = SPTemplatesUpdateCurrent.TemplateId THEN TRUE ELSE FALSE END;
$$;

DROP FUNCTION IF EXISTS public.SPTemplatesGet(int);
CREATE FUNCTION public.SPTemplatesGet(
		TemplateId INT)
	RETURNS TABLE(
		TemplateId INT
		,TemplateKey VARCHAR
		,TemplateDescription VARCHAR
		,TemplateIsCurrent BOOLEAN)
	LANGUAGE sql
AS $$
	SELECT
		TemplateId
		,TemplateKey
		,TemplateDescription
		,TemplateIsCurrent
	FROM
		Templates
	WHERE
		TemplateId = SPTemplatesGet.TemplateId;
$$;

DROP FUNCTION IF EXISTS public.SPTemplatesDelete(int);
CREATE FUNCTION public.SPTemplatesDelete(
		TemplateId int)
	RETURNS void
	LANGUAGE sql
AS $$
	DELETE FROM Templates t WHERE t.TemplateId = SPTemplatesDelete.TemplateId;
$$;

DROP FUNCTION IF EXISTS public.SPTopicsSubscriptionsGetByUser(int);
CREATE FUNCTION public.SPTopicsSubscriptionsGetByUser(
		UserId int=21)
	RETURNS TABLE (
		TopicId int,
		TopicTitle varchar,
		TopicShortName varchar,
		ForumId int,
		ForumName varchar,
		ForumShortName varchar)
	LANGUAGE sql
AS $$
	SELECT
		T.TopicId
		,T.TopicTitle
		,T.TopicShortName
		,T.ForumId
		,T.ForumName
		,T.ForumShortName
	FROM
		TopicsSubscriptions S
		INNER JOIN TopicsComplete T ON T.TopicId = S.TopicId
	WHERE
		S.UserId = SPTopicsSubscriptionsGetByUser.UserId
	ORDER BY
		S.TopicId DESC;
$$;

DROP FUNCTION IF EXISTS public.SPMessagesGetByTopicLatest(int);
CREATE FUNCTION public.SPMessagesGetByTopicLatest(
	TopicId int=2)
	RETURNS TABLE(
		TopicId int
		,MessageId int
		,MessageBody varchar
		,MessageCreationDate timestamp
		,MessageLastEditDate timestamp
		,ParentId int
		,UserId int
		,UserName varchar
		,UserSignature varchar
		,UserGroupId smallint
		,UserGroupName varchar
		,Active boolean)
	LANGUAGE sql
AS $$
	SELECT 
		M.TopicId
		,M.MessageId
		,M.MessageBody
		,M.MessageCreationDate
		,M.MessageLastEditDate
		,M.ParentId
		,UserId
		,UserName
		,UserSignature
		,UserGroupId
		,UserGroupName
		,M.Active
	FROM 
		public.MessagesComplete M
	WHERE 
		M.TopicId = SPMessagesGetByTopicLatest.TopicId
	ORDER BY
		TopicId, MessageId DESC
	LIMIT 20;
$$;

drop FUNCTION IF EXISTS public.SPTopicsOpen(int, int, varchar (39));
CREATE FUNCTION public.SPTopicsOpen(
		TopicId int
		,UserId int
		,Ip varchar (39))
	returns void
	language sql
AS $$
	UPDATE Topics
	SET
		TopicIsClose = FALSE
		,TopicLastEditDate = now()
		,TopicLastEditUser = SPTopicsOpen.UserId
		,TopicLastEditIp = SPTopicsOpen.Ip
	WHERE
		TopicId = SPTopicsOpen.TopicId
$$;

DROP function IF EXISTS public.SPUsersSuspend(int, int, int, text, timestamp without time zone );
CREATE function public.SPUsersSuspend(
		UserId int
		, ModeratorUserId int
		, ModeratorReason int
		, ModeratorReasonFull text
		, SuspendedEnd timestamp without time zone)
	returns void
	language sql
AS $$
	UPDATE Users
	SET
		SuspendedStart = now()
		, SuspendedEnd = SPUsersSuspend.SuspendedEnd
		, ModeratorReason = SPUsersSuspend.ModeratorReason
		, ModeratorReasonFull = SPUsersSuspend.ModeratorReasonFull
		, ModeratorUserId = SPUsersSuspend.ModeratorUserId
	WHERE 
		UserId = SPUsersSuspend.UserId;
$$;

﻿INSERT INTO public.UsersGroups (UserGroupId, UserGroupName) VALUES (1, 'Member');
INSERT INTO public.UsersGroups (UserGroupId, UserGroupName) VALUES (2, 'Trusted member');
INSERT INTO public.UsersGroups (UserGroupId, UserGroupName) VALUES (3, 'Moderator');
INSERT INTO public.UsersGroups (UserGroupId, UserGroupName) VALUES (10, 'Admin');