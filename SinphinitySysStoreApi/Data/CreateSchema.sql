--------------------------------------------
/* Remove all foreign keys */
--------------------------------------------

IF (OBJECT_ID('dbo.[FK_Band_Style_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.Bands DROP CONSTRAINT FK_Band_Style_Id
	
IF (OBJECT_ID('dbo.[FK_MidiStats_Song_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.Songs DROP CONSTRAINT FK_MidiStats_Song_Id
	
IF (OBJECT_ID('dbo.[FK_SongData_Songs_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.SongData DROP CONSTRAINT FK_SongData_Songs_Id
	
/* SongsSimplifications */
IF (OBJECT_ID('dbo.[FK_SongsSimplifications_SongData_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.SongsSimplifications DROP CONSTRAINT FK_SongsSimplifications_SongData_Id
	
/* Phrases */
IF (OBJECT_ID('dbo.[FK_PhrasesOccurrences_Songs_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhrasesOccurrences DROP CONSTRAINT FK_PhrasesOccurrences_Songs_Id
	
IF (OBJECT_ID('dbo.[FK_PhrasesOccurrences_Phrases_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhrasesOccurrences DROP CONSTRAINT FK_PhrasesOccurrences_Phrases_Id
	
IF (OBJECT_ID('dbo.[FK_PhraseSong_Songs]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhraseSong DROP CONSTRAINT FK_PhraseSong_Songs

IF (OBJECT_ID('dbo.[FK_PhraseSong_Phrases]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhraseSong DROP CONSTRAINT FK_PhraseSong_Phrases

IF (OBJECT_ID('dbo.FK_BandPhrase_Phrases', 'F') IS NOT NULL)
	ALTER TABLE dbo.BandPhrase DROP CONSTRAINT FK_BandPhrase_Phrases
	
IF (OBJECT_ID('dbo.[FK_BandPhrase_Bands]', 'F') IS NOT NULL)
	ALTER TABLE dbo.BandPhrase DROP CONSTRAINT FK_BandPhrase_Bands

IF (OBJECT_ID('dbo.[FK_PhraseStyle_Styles]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhraseStyle DROP CONSTRAINT FK_PhraseStyle_Styles	

IF (OBJECT_ID('dbo.FK_PhraseStyle_Phrases', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhraseStyle DROP CONSTRAINT FK_PhraseStyle_Phrases
	
IF (OBJECT_ID('dbo.[FK_PhrasesLinks_Phrases1]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhrasesLinks DROP CONSTRAINT FK_PhrasesLinks_Phrases1
	
IF (OBJECT_ID('dbo.[FK_PhrasesLinks_Phrases2]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhrasesLinks DROP CONSTRAINT FK_PhrasesLinks_Phrases2	

IF (OBJECT_ID('dbo.[FK_PhrasesLinks_Songs]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PhrasesLinks DROP CONSTRAINT FK_PhrasesLinks_Songs	
	
IF (OBJECT_ID('dbo.[FK_Phrases_Segment_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.Phrases DROP CONSTRAINT FK_Phrases_Segment_Id
	
	
	
IF OBJECT_ID('dbo.Styles', 'U') IS NOT NULL 
  DROP TABLE dbo.Styles

CREATE TABLE Styles(
	Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	[Name] NVARCHAR(60) NULL UNIQUE
)
ALTER TABLE Styles ADD CONSTRAINT UC_Styles UNIQUE (Name);

	
IF OBJECT_ID('dbo.Bands', 'U') IS NOT NULL 
  DROP TABLE dbo.Bands

CREATE TABLE Bands(
	Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	[Name] NVARCHAR(100) NOT NULL UNIQUE,
	StyleId BIGINT NOT NULL,	
    CONSTRAINT FK_Band_Style_Id FOREIGN KEY  (StyleId) REFERENCES Styles(Id)
)
IF OBJECT_ID('dbo.MidiStats', 'U') IS NOT NULL 
  DROP TABLE dbo.MidiStats
  
CREATE TABLE MidiStats(
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL ,
	DurationInSeconds INT NULL,
	DurationInTicks BIGINT NULL,
	HasMoreThanOneChannelPerTrack bit NULL,
	HasMoreThanOneInstrumentPerTrack bit NULL,
	HighestPitch INT NULL,
	LowestPitch INT NULL,
	TotalDifferentPitches INT NULL,
	TotalUniquePitches INT NULL,	
	TempoInBeatsPerMinute INT NULL,
	TotalTracks INT NULL,
    TotalTracksWithoutNotes INT NULL,
    TotalBassTracks INT NULL,
	TotalChordTracks INT NULL,
	TotalMelodicTracks INT NULL,
    TotalPercussionTracks INT NULL,
	TotalInstruments INT NULL,
    InstrumentsAsString NVARCHAR(400)  NULL,
	TotalPercussionInstruments INT NULL,
    TotalChannels INT NULL,
	TotalTempoChanges INT NULL,
    TotalEvents INT NULL,
	TotalNoteEvents INT NULL,
	TotalPitchBendEvents INT NULL,
	TotalControlChangeEvents INT NULL,
    TotalProgramChangeEvents INT NULL,
	TotalSustainPedalEvents INT NULL,
	TotalChannelIndependentEvents INT NULL
)

	
IF OBJECT_ID('dbo.Songs', 'U') IS NOT NULL 
  DROP TABLE dbo.Songs

CREATE TABLE Songs(
	Id BIGINT IDENTITY(1,1) primary key clustered NOT NULL,
	[Name] nvarchar(500) NOT NULL,
	BandId BIGINT NULL,
	StyleId BIGINT NOT NULL,
	MidiStatsId BIGINT NOT NULL,
	MidiBase64Encoded nvarchar(max) NOT NULL,
	IsSongProcessed BIT NOT NULL DEFAULT 0,
	ArePhrasesExtracted BIT NOT NULL DEFAULT 0,
	IsMidiCorrect BIT NOT NULL DEFAULT 0,
	CantBeProcessed BIT NOT NULL DEFAULT 0,
	Bars VARCHAR(MAX) NULL,
	TempoChanges VARCHAR(MAX) NULL,
	AverageTempoInBeatsPerMinute BIGINT NULL,
    CONSTRAINT FK_MidiStats_Song_Id FOREIGN KEY (MidiStatsId) REFERENCES MidiStats(Id)
)

IF OBJECT_ID('dbo.SongData', 'U') IS NOT NULL 
  DROP TABLE dbo.SongData

CREATE TABLE SongData(
	Id BIGINT IDENTITY(1,1) primary key clustered NOT NULL,
	SongId BIGINT NOT NULL,
	MidiBase64Encoded nvarchar(max) NOT NULL,
	Bars VARCHAR(MAX) NULL,
	TempoChanges VARCHAR(MAX) NULL,
    CONSTRAINT FK_SongData_Songs_Id FOREIGN KEY (SongId) REFERENCES Songs(Id)
)
	

IF OBJECT_ID('dbo.SongsSimplifications', 'U') IS NOT NULL 
  DROP TABLE dbo.SongsSimplifications

CREATE TABLE SongsSimplifications(
	Id BIGINT IDENTITY(1,1) primary key clustered NOT NULL,
	SongDataId BIGINT NOT NULL,
	Version BIGINT NOT NULL,
	Notes VARCHAR(MAX) NOT NULL,
	NumberOfVoices BIGINT NOT NULL,
    CONSTRAINT FK_SongsSimplifications_SongData_Id FOREIGN KEY (SongDataId) REFERENCES SongData(Id)
)

IF OBJECT_ID('dbo.Segments', 'U') IS NOT NULL 
  DROP TABLE dbo.Segments
  
CREATE TABLE Segments (
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	TotalNotes INT NOT NULL,
	DurationInTicks BIGINT  NULL,
	NoteDensityTimes10 INT NOT NULL,
	MaxDurationVariationTimes10 INT NOT NULL,
	PitchDirections VARCHAR(1000) NOT NULL,
	PitchStep INT NOT NULL,
	PitchRange INT NOT NULL,
	AbsPitchVariationTimes10 INT NOT NULL,
	RelPitchVariationTimes10 INT NOT NULL,
	AverageIntervalTimes10 INT NOT NULL,
	MonotonyTimes10 INT NOT NULL,
	Hash VARCHAR(1000) NOT NULL
)	
CREATE INDEX IX_PSegments_Hash ON Segments (Hash)


IF OBJECT_ID('dbo.Phrases', 'U') IS NOT NULL 
  DROP TABLE dbo.Phrases
  
CREATE TABLE Phrases (
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	MetricsAsString VARCHAR(1000) NOT NULL,
	PitchesAsString VARCHAR(1000) NOT NULL,
	MetricsAccumAsString VARCHAR(1000) NULL,
	PitchesAccumAsString VARCHAR(1000) NULL,
	SkeletonMetricsAsString VARCHAR(1000) NULL,
	SkeletonPitchesAsString VARCHAR(1000) NULL,
	Equivalences VARCHAR(MAX) NULL,
	SegmentId BIGINT NOT NULL,
    CONSTRAINT FK_Phrases_Segment_Id FOREIGN KEY (SegmentId) REFERENCES Segments(Id)
)	

CREATE INDEX IX_Phrases_MetricsAsString ON Phrases (MetricsAsString)
CREATE INDEX IX_Phrases_PitchesAsString ON Phrases (PitchesAsString)


IF OBJECT_ID('dbo.PhrasesOccurrences', 'U') IS NOT NULL 
  DROP TABLE dbo.PhrasesOccurrences
  
CREATE TABLE PhrasesOccurrences (
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	SongId BIGINT NOT NULL,
	PhraseId BIGINT NOT NULL,
	Voice TINYINT NOT NULL,
	Instrument TINYINT NOT NULL,
	BarNumber INT NOT NULL,	
	Beat INT NOT NULL,
	StartTick BIGINT NOT NULL,
	EndTick BIGINT NOT NULL,
	StartingPitch INT NULL,
    CONSTRAINT FK_PhrasesOccurrences_Songs_Id  FOREIGN KEY (SongId) REFERENCES Songs(Id),
    CONSTRAINT FK_PhrasesOccurrences_Phrases_Id  FOREIGN KEY (PhraseId) REFERENCES Phrases(Id)
)

CREATE INDEX IX_PhrasesOccurrences_StartTick ON PhrasesOccurrences (StartTick)
CREATE INDEX IX_PhrasesOccurrences_EndTick ON PhrasesOccurrences (EndTick)
CREATE INDEX IX_PhrasesOccurrences_Voice ON PhrasesOccurrences (Voice)

IF OBJECT_ID('dbo.PhraseSong', 'U') IS NOT NULL 
  DROP TABLE dbo.PhraseSong
  
CREATE TABLE PhraseSong(
	PhrasesId bigint NOT NULL,
	SongsId bigint NOT NULL,
    CONSTRAINT PK_PhraseSong PRIMARY KEY NONCLUSTERED (PhrasesId, SongsId),
    CONSTRAINT FK_PhraseSong_Phrases FOREIGN KEY (PhrasesId) REFERENCES Phrases (Id) ON DELETE CASCADE,
    CONSTRAINT FK_PhraseSong_Songs FOREIGN KEY (SongsId) REFERENCES Songs (Id) ON DELETE CASCADE
)

IF OBJECT_ID('dbo.BandPhrase', 'U') IS NOT NULL 
  DROP TABLE dbo.BandPhrase
  
CREATE TABLE BandPhrase(
	PhrasesId bigint NOT NULL,
	BandsId bigint NOT NULL,
    CONSTRAINT PK_PhraseBand PRIMARY KEY NONCLUSTERED (PhrasesId, BandsId),
    CONSTRAINT FK_BandPhrase_Phrases FOREIGN KEY (PhrasesId) REFERENCES Phrases (Id) ON DELETE CASCADE,
    CONSTRAINT FK_BandPhrase_Bands FOREIGN KEY (BandsId) REFERENCES Bands (Id) ON DELETE CASCADE
)

IF OBJECT_ID('dbo.PhraseStyle', 'U') IS NOT NULL 
  DROP TABLE dbo.PhraseStyle
  
CREATE TABLE PhraseStyle(
	PhrasesId bigint NOT NULL,
	StylesId bigint NOT NULL,
    CONSTRAINT PK_PhraseStyle PRIMARY KEY NONCLUSTERED (PhrasesId, StylesId),
    CONSTRAINT FK_PhraseStyle_Phrases FOREIGN KEY (PhrasesId) REFERENCES Phrases (Id) ON DELETE CASCADE,
    CONSTRAINT [FK_PhraseStyle_Styles] FOREIGN KEY (StylesId) REFERENCES Styles (Id) ON DELETE CASCADE
)


IF OBJECT_ID('dbo.PhrasesLinks', 'U') IS NOT NULL 
  DROP TABLE dbo.PhrasesLinks
  
CREATE TABLE PhrasesLinks(
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	PhraseId1 bigint NOT NULL,
	PhraseId2 bigint NOT NULL,
	ShiftInTicks bigint NOT NULL,
	PitchShift int NOT NULL,	
	SongId BIGINT NOT NULL,
	Phrase1Start BIGINT NOT NULL,
	Instrument1 TINYINT NOT NULL,
	Instrument2 TINYINT NOT NULL,
    CONSTRAINT [FK_PhrasesLinks_Phrases1] FOREIGN KEY (PhraseId1) REFERENCES Phrases (Id),
    CONSTRAINT [FK_PhrasesLinks_Phrases2] FOREIGN KEY (PhraseId2) REFERENCES Phrases (Id),
    CONSTRAINT [FK_PhrasesLinks_Songs] FOREIGN KEY (SongId) REFERENCES Songs (Id)	
)


