/*
** Copyright Microsoft, Inc. 1994 - 2000
** All Rights Reserved.
*/

SET NOCOUNT ON
GO

set quoted_identifier on
GO

/* Set DATEFORMAT so that the date strings are interpreted correctly regardless of
   the default DATEFORMAT on the server.
*/
SET DATEFORMAT mdy
GO
CREATE DATABASE Talkliketv
GO
USE Talkliketv
GO
DROP TABLE IF EXISTS "Translates"
GO
DROP TABLE IF EXISTS "Tokens"
GO
DROP TABLE IF EXISTS "Phrases"
GO
DROP TABLE IF EXISTS "Titles"
GO
DROP TABLE IF EXISTS "Languages"
GO
CREATE TABLE "Tokens" (
                        "TokenId" "int" IDENTITY (1, 1) NOT NULL,
                        "Hash" nvarchar (64) NOT NULL ,
                        "Created" "datetime" NOT NULL ,
                        "Used" bit Not NULL default 0,
                        CONSTRAINT "PK_Tokens" PRIMARY KEY CLUSTERED 
                        (
                            "TokenId"
                        )

)
CREATE TABLE "Languages" (
                            "LanguageId" "int" IDENTITY (1, 1) NOT NULL ,
                            "Language" nvarchar (32) NOT NULL ,
                            "Tag" nvarchar(8) NOT NULL
                            CONSTRAINT "PK_Language" PRIMARY KEY  CLUSTERED
                            (
                               "LanguageId"
                            )

)
CREATE TABLE "Titles" (
                            "TitleId" "int" IDENTITY (1, 1) NOT NULL ,
                            "Title" nvarchar (64) NOT NULL UNIQUE ,
                            "NumPhrases" int NOT NULL,
                            "OriginalLanguageId" int NOT NULL ,
                            CONSTRAINT "PK_Titles" PRIMARY KEY  CLUSTERED
                            (
                                "TitleId"
                            ),
                            CONSTRAINT "FK_Titles_OriginalLanguage" FOREIGN KEY
                            (
                                "OriginalLanguageId"
                            ) REFERENCES "dbo"."Languages" (
                                "LanguageId"
                            )
)
CREATE TABLE "Phrases" (
                            "PhraseId" "int" IDENTITY (1, 1) NOT NULL PRIMARY KEY ,
                            "TitleId" "int" NOT NULL REFERENCES Titles ON DELETE CASCADE
)
CREATE TABLE "Translates" (
                              "PhraseId" int NOT NULL REFERENCES Phrases ON DELETE CASCADE,
                              "LanguageId" int NOT NULL REFERENCES Languages ON DELETE CASCADE,
                              "Phrase" nvarchar (128) NULL ,
                              phrase_hint nvarchar (128) NULL ,
                              PRIMARY KEY ("PhraseId", "LanguageId")
)
go
SET IDENTITY_INSERT "Languages" ON
go
ALTER TABLE "Languages" NOCHECK CONSTRAINT ALL
go
INSERT INTO "Languages" ("LanguageId", "Language", "Tag") VALUES (-1, 'Not a Language', 'NaL')
go
SET IDENTITY_INSERT "Languages" OFF
go
SET IDENTITY_INSERT "Titles" ON
go
ALTER TABLE "Titles" NOCHECK CONSTRAINT ALL
go
INSERT INTO "Titles" ("TitleId", "Title", "NumPhrases", "OriginalLanguageId")VALUES (-1, 'Not a Title', 0, '-1')
SET IDENTITY_INSERT "Titles" OFF