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
DROP TABLE IF EXISTS "VoiceStyles"
GO
DROP TABLE IF EXISTS "Styles"
GO
DROP TABLE IF EXISTS "VoicePersonalities"
GO
DROP TABLE IF EXISTS "Personalities"
GO
DROP TABLE IF EXISTS "VoiceScenarios"
GO
DROP TABLE IF EXISTS "Scenarios"
GO
DROP TABLE IF EXISTS "Voices"
GO
DROP TABLE IF EXISTS "Languages"
GO
CREATE TABLE "Tokens" (
    "TokenID" INT IDENTITY PRIMARY KEY ,
    "Hash" nvarchar (64) NOT NULL ,
    "Created" "datetime" NOT NULL ,
    "Used" bit Not NULL default 0,

)
CREATE TABLE "Languages" (
    "LanguageID" INT IDENTITY PRIMARY KEY ,
    "Name" nvarchar (32) NOT NULL UNIQUE ,
    "NativeName" nvarchar (32) NOT NULL ,
    "Tag" nvarchar(8) NOT NULL UNIQUE ,
)
GO
CREATE TABLE "Voices" (
    "VoiceID" INT IDENTITY PRIMARY KEY ,
    "LanguageID"  INT FOREIGN KEY REFERENCES Languages(LanguageID),
    "DisplayName" nvarchar (32) NOT NULL ,
    "LocalName" nvarchar (32) NOT NULL ,
    "ShortName" nvarchar (64) NOT NULL UNIQUE ,
    "Gender" nvarchar (8) NOT NULL ,
    "Locale" nvarchar (16) NOT NULL ,
    "LocaleName" nvarchar (32) NOT NULL ,
    "SampleRateHertz" int NOT NULL ,
    "VoiceType" nvarchar (8) NOT NULL ,
    "Status" nvarchar(32) NOT NULL ,
    "WordsPerMinute" INT NOT NULL,
)
GO
CREATE TABLE Styles (
                        StyleID INT IDENTITY PRIMARY KEY,
                        StyleName NVARCHAR(32) NOT NULL UNIQUE
);
GO
CREATE TABLE VoiceStyles (
                             VoiceID INT NOT NULL FOREIGN KEY REFERENCES Voices(VoiceID) ON DELETE CASCADE,
                             StyleID INT NOT NULL FOREIGN KEY REFERENCES Styles(StyleID) ON DELETE CASCADE,
                             PRIMARY KEY (VoiceID, StyleID)  -- Composite PK ensures unique voice-style pairs
);
GO
-- Table to store unique TailoredScenarios
CREATE TABLE Scenarios (
                                   ScenarioID INT IDENTITY PRIMARY KEY,
                                   ScenarioName NVARCHAR(32) NOT NULL UNIQUE
);
GO
-- Many-to-many relationship between Voices and TailoredScenarios
CREATE TABLE VoiceScenarios (
                                        VoiceID INT NOT NULL FOREIGN KEY REFERENCES Voices(VoiceID) ON DELETE CASCADE,
                                        ScenarioID INT NOT NULL FOREIGN KEY REFERENCES Scenarios(ScenarioID) ON DELETE CASCADE,
                                        PRIMARY KEY (VoiceID, ScenarioID)
);

GO
-- Table to store unique VoicePersonalities
CREATE TABLE Personalities (
                                    PersonalityID INT IDENTITY PRIMARY KEY,
                                    PersonalityName NVARCHAR(32) NOT NULL UNIQUE
);
GO
-- Many-to-many relationship between Voices and VoicePersonalities
CREATE TABLE VoicePersonalities (
                                         VoiceID INT NOT NULL FOREIGN KEY REFERENCES Voices(VoiceID) ON DELETE CASCADE,
                                         PersonalityID INT NOT NULL FOREIGN KEY REFERENCES Personalities(PersonalityID) ON DELETE CASCADE,
                                         PRIMARY KEY (VoiceID, PersonalityID)
);
GO
CREATE TABLE "Titles" (
    "TitleID" INT IDENTITY PRIMARY KEY,
    "Title" nvarchar (64) NOT NULL UNIQUE ,
    "NumPhrases" int NOT NULL,
    "OriginalLanguageID" INT FOREIGN KEY REFERENCES Languages(LanguageID),
)
GO
CREATE TABLE "Phrases" (
    "PhraseID" INT IDENTITY PRIMARY KEY,
    "TitleID" INT FOREIGN KEY REFERENCES Titles(TitleID),
)
GO
CREATE TABLE "Translates" (
    "PhraseID" INT FOREIGN KEY REFERENCES Phrases(PhraseID),
    "LanguageID" INT FOREIGN KEY REFERENCES Languages(LanguageID),
    "Phrase" nvarchar (128) NOT NULL ,
    "PhraseHint" nvarchar (128) NOT NULL ,
    PRIMARY KEY ("PhraseID", "LanguageID")
)
go
SET IDENTITY_INSERT "Languages" ON
go
ALTER TABLE "Languages" NOCHECK CONSTRAINT ALL
go
INSERT INTO "Languages" ("LanguageID", "Name", "Tag", "NativeName") VALUES (-1, 'Not a Language', 'NaL', 'Not a Language')
go
SET IDENTITY_INSERT "Languages" OFF
go
SET IDENTITY_INSERT "Titles" ON
go
ALTER TABLE "Titles" NOCHECK CONSTRAINT ALL
go
INSERT INTO "Titles" ("TitleID", "Title", "NumPhrases", "OriginalLanguageID")VALUES (-1, 'Not a Title', 0, '-1')
SET IDENTITY_INSERT "Titles" OFF


