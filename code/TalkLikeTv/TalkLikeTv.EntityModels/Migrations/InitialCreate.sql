IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Languages] (
    [LanguageID] int NOT NULL IDENTITY,
    [Platform] nvarchar(16) NOT NULL,
    [Name] nvarchar(32) NOT NULL,
    [NativeName] nvarchar(32) NOT NULL,
    [Tag] nvarchar(8) NOT NULL,
    CONSTRAINT [PK__Language__B938558B157F7949] PRIMARY KEY ([LanguageID])
);

CREATE TABLE [Personalities] (
    [PersonalityID] int NOT NULL IDENTITY,
    [PersonalityName] nvarchar(32) NOT NULL,
    CONSTRAINT [PK__Personal__CD053C54F187B8DC] PRIMARY KEY ([PersonalityID])
);

CREATE TABLE [Scenarios] (
    [ScenarioID] int NOT NULL IDENTITY,
    [ScenarioName] nvarchar(32) NOT NULL,
    CONSTRAINT [PK__Scenario__0DF6D1A39A1B5B0E] PRIMARY KEY ([ScenarioID])
);

CREATE TABLE [Styles] (
    [StyleID] int NOT NULL IDENTITY,
    [StyleName] nvarchar(32) NOT NULL,
    CONSTRAINT [PK__Styles__8AD147A057377767] PRIMARY KEY ([StyleID])
);

CREATE TABLE [Tokens] (
    [TokenID] int NOT NULL IDENTITY,
    [Hash] nvarchar(64) NOT NULL,
    [Created] datetime NOT NULL,
    [Used] bit NOT NULL,
    CONSTRAINT [PK__Tokens__658FEE8A40067671] PRIMARY KEY ([TokenID])
);

CREATE TABLE [Titles] (
    [TitleID] int NOT NULL IDENTITY,
    [TitleName] nvarchar(64) NOT NULL,
    [Description] nvarchar(256) NULL,
    [NumPhrases] int NOT NULL,
    [OriginalLanguageID] int NULL,
    CONSTRAINT [PK__Titles__757589E6D98CFFD1] PRIMARY KEY ([TitleID]),
    CONSTRAINT [FK__Titles__Original__5748DA5E] FOREIGN KEY ([OriginalLanguageID]) REFERENCES [Languages] ([LanguageID])
);

CREATE TABLE [Voices] (
    [VoiceID] int NOT NULL IDENTITY,
    [Platform] nvarchar(16) NOT NULL,
    [LanguageID] int NULL,
    [DisplayName] nvarchar(32) NOT NULL,
    [LocalName] nvarchar(32) NOT NULL,
    [ShortName] nvarchar(64) NOT NULL,
    [Gender] nvarchar(8) NOT NULL,
    [Locale] nvarchar(16) NOT NULL,
    [LocaleName] nvarchar(32) NOT NULL,
    [SampleRateHertz] int NOT NULL,
    [VoiceType] nvarchar(8) NOT NULL,
    [Status] nvarchar(32) NOT NULL,
    [WordsPerMinute] int NOT NULL,
    CONSTRAINT [PK__Voices__D870D587186CF99C] PRIMARY KEY ([VoiceID]),
    CONSTRAINT [FK__Voices__Language__3F7150CD] FOREIGN KEY ([LanguageID]) REFERENCES [Languages] ([LanguageID])
);

CREATE TABLE [Phrases] (
    [PhraseID] int NOT NULL IDENTITY,
    [TitleID] int NULL,
    CONSTRAINT [PK__Phrases__0DBA0EA20BE30369] PRIMARY KEY ([PhraseID]),
    CONSTRAINT [FK__Phrases__TitleID__5A254709] FOREIGN KEY ([TitleID]) REFERENCES [Titles] ([TitleID]) ON DELETE CASCADE
);

CREATE TABLE [VoicePersonalities] (
    [VoiceID] int NOT NULL,
    [PersonalityID] int NOT NULL,
    CONSTRAINT [PK__VoicePer__84A08642C51F02F2] PRIMARY KEY ([VoiceID], [PersonalityID]),
    CONSTRAINT [FK__VoicePers__Perso__5378497A] FOREIGN KEY ([PersonalityID]) REFERENCES [Personalities] ([PersonalityID]) ON DELETE CASCADE,
    CONSTRAINT [FK__VoicePers__Voice__52842541] FOREIGN KEY ([VoiceID]) REFERENCES [Voices] ([VoiceID]) ON DELETE CASCADE
);

CREATE TABLE [VoiceScenarios] (
    [VoiceID] int NOT NULL,
    [ScenarioID] int NOT NULL,
    CONSTRAINT [PK__VoiceSce__F8AFB89D9B0E5462] PRIMARY KEY ([VoiceID], [ScenarioID]),
    CONSTRAINT [FK__VoiceScen__Scena__4CCB4BEB] FOREIGN KEY ([ScenarioID]) REFERENCES [Scenarios] ([ScenarioID]) ON DELETE CASCADE,
    CONSTRAINT [FK__VoiceScen__Voice__4BD727B2] FOREIGN KEY ([VoiceID]) REFERENCES [Voices] ([VoiceID]) ON DELETE CASCADE
);

CREATE TABLE [VoiceStyles] (
    [VoiceID] int NOT NULL,
    [StyleID] int NOT NULL,
    CONSTRAINT [PK__VoiceSty__C0DDC1FD59FFE951] PRIMARY KEY ([VoiceID], [StyleID]),
    CONSTRAINT [FK__VoiceStyl__Style__461E4E5C] FOREIGN KEY ([StyleID]) REFERENCES [Styles] ([StyleID]) ON DELETE CASCADE,
    CONSTRAINT [FK__VoiceStyl__Voice__452A2A23] FOREIGN KEY ([VoiceID]) REFERENCES [Voices] ([VoiceID]) ON DELETE CASCADE
);

CREATE TABLE [Translates] (
    [PhraseID] int NOT NULL,
    [LanguageID] int NOT NULL,
    [Phrase] nvarchar(128) NOT NULL,
    [PhraseHint] nvarchar(128) NOT NULL,
    CONSTRAINT [PK__Translat__A6298BFA0F5D3F2A] PRIMARY KEY ([PhraseID], [LanguageID]),
    CONSTRAINT [FK__Translate__Langu__5DF5D7ED] FOREIGN KEY ([LanguageID]) REFERENCES [Languages] ([LanguageID]) ON DELETE CASCADE,
    CONSTRAINT [FK__Translate__Phras__5D01B3B4] FOREIGN KEY ([PhraseID]) REFERENCES [Phrases] ([PhraseID]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [UQ__Language__737584F60620E8D3] ON [Languages] ([Name]);

CREATE UNIQUE INDEX [UQ__Language__C4516413E29C3320] ON [Languages] ([Tag]);

CREATE UNIQUE INDEX [UQ__Personal__6DD1E173CBB571F5] ON [Personalities] ([PersonalityName]);

CREATE INDEX [IX_Phrases_TitleID] ON [Phrases] ([TitleID]);

CREATE UNIQUE INDEX [UQ__Scenario__ADC5B11D78260FCB] ON [Scenarios] ([ScenarioName]);

CREATE UNIQUE INDEX [UQ__Styles__23564EE60ED94D48] ON [Styles] ([StyleName]);

CREATE INDEX [IX_Titles_OriginalLanguageID] ON [Titles] ([OriginalLanguageID]);

CREATE UNIQUE INDEX [UQ__Titles__252BE89C0BF9B926] ON [Titles] ([TitleName]);

CREATE INDEX [IX_Translates_LanguageID] ON [Translates] ([LanguageID]);

CREATE INDEX [IX_VoicePersonalities_PersonalityID] ON [VoicePersonalities] ([PersonalityID]);

CREATE INDEX [IX_Voices_LanguageID] ON [Voices] ([LanguageID]);

CREATE UNIQUE INDEX [UQ__Voices__A6160FD1C3922DD5] ON [Voices] ([ShortName]);

CREATE INDEX [IX_VoiceScenarios_ScenarioID] ON [VoiceScenarios] ([ScenarioID]);

CREATE INDEX [IX_VoiceStyles_StyleID] ON [VoiceStyles] ([StyleID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250324005910_InitialBaseline', N'9.0.2');

COMMIT;
GO

