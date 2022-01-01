IF (OBJECT_ID('dbo.[FK_PatternOccurrences_Pattern_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PatternOccurrences DROP CONSTRAINT FK_PatternOccurrences_Pattern_Id
	
IF (OBJECT_ID('dbo.[FK_PatternOccurrences_Song_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PatternOccurrences DROP CONSTRAINT FK_PatternOccurrences_Song_Id
	
IF (OBJECT_ID('dbo.[FK_Band_Style_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.Bands DROP CONSTRAINT FK_Band_Style_Id
	
IF (OBJECT_ID('dbo.[FK_MidiStats_Song_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.Songs DROP CONSTRAINT FK_MidiStats_Song_Id
	
IF (OBJECT_ID('dbo.[FK_BasicPatternsPatterns_Pattern_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.BasicPatternsPatterns DROP CONSTRAINT FK_BasicPatternsPatterns_Pattern_Id
	
IF (OBJECT_ID('dbo.[FK_BasicPatternsPatterns_BasicPattern_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.BasicPatternsPatterns DROP CONSTRAINT FK_BasicPatternsPatterns_BasicPattern_Id

IF (OBJECT_ID('dbo.[FK_SongsSimplifications_Song_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.SongsSimplifications DROP CONSTRAINT FK_SongsSimplifications_Song_Id

IF (OBJECT_ID('dbo.[FK_PatternsSongs_Pattern_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PatternsSongs DROP CONSTRAINT FK_PatternsSongs_Pattern_Id

IF (OBJECT_ID('dbo.[FK_PatternsSongs_Song_Id]', 'F') IS NOT NULL)
	ALTER TABLE dbo.PatternsSongs DROP CONSTRAINT FK_PatternsSongs_Song_Id
	
	

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
	ArePatternsExtracted BIT NOT NULL DEFAULT 0,
	IsMidiCorrect BIT NOT NULL DEFAULT 0,
	CantBeProcessed BIT NOT NULL DEFAULT 0,
	Bars VARCHAR(MAX) NULL,
	TempoChanges VARCHAR(MAX) NULL,
	AverageTempoInBeatsPerMinute BIGINT NULL,
    CONSTRAINT FK_MidiStats_Song_Id FOREIGN KEY (MidiStatsId) REFERENCES MidiStats(Id)
)

IF OBJECT_ID('dbo.SongsSimplifications', 'U') IS NOT NULL 
  DROP TABLE dbo.SongsSimplifications

CREATE TABLE SongsSimplifications(
	Id BIGINT IDENTITY(1,1) primary key clustered NOT NULL,
	SongDataId BIGINT NOT NULL,
	Version BIGINT NOT NULL,
	Notes VARCHAR(MAX) NOT NULL,
	NumberOfVoices BIGINT NOT NULL,
    CONSTRAINT FK_SongsSimplifications_SongData_Id FOREIGN KEY (SongDataId) REFERENCES SongsData(Id)
)
	


IF OBJECT_ID('dbo.Patterns', 'U') IS NOT NULL 
  DROP TABLE dbo.Patterns
  
CREATE TABLE Patterns (
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	AsString VARCHAR(4000) NOT NULL,
	DurationInTicks BIGINT NOT NULL,
	NumberOfNotes INT NOT NULL, 
	[Range] INT NOT NULL, 
	IsMonotone BIT NOT NULL,
	Step INT NOT NULL
)
CREATE INDEX IX_Patterns_AsString ON Patterns (AsString)



IF OBJECT_ID('dbo.BasicPatterns', 'U') IS NOT NULL 
	DROP TABLE dbo.BasicPatterns
  
CREATE TABLE BasicPatterns (
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	AsString VARCHAR(4000) NOT NULL,
	NumberOfNotes INT NOT NULL, 
	[Range] INT NOT NULL, 
	IsMonotone BIT NOT NULL,
	Step INT NOT NULL
)

CREATE INDEX IX_BasicPatterns_AsString ON BasicPatterns (AsString)

IF OBJECT_ID('dbo.BasicPatternsPatterns', 'U') IS NOT NULL 
  DROP TABLE dbo.BasicPatternsPatterns
  
CREATE TABLE BasicPatternsPatterns (
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	PatternId BIGINT NOT NULL,
	BasicPatternId BIGINT NOT NULL,	
    CONSTRAINT FK_BasicPatternsPatterns_Pattern_Id FOREIGN KEY (PatternId) REFERENCES Patterns(Id),
    CONSTRAINT FK_BasicPatternsPatterns_BasicPattern_Id FOREIGN KEY (BasicPatternId) REFERENCES BasicPatterns(Id)	
)

IF OBJECT_ID('dbo.PatternOccurrences', 'U') IS NOT NULL 
  DROP TABLE dbo.PatternOccurrences
  
CREATE TABLE PatternOccurrences (
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	SongId BIGINT NOT NULL,
	PatternId BIGINT NOT NULL,
	Voice TINYINT NOT NULL,
	BarNumber BIGINT NOT NULL,	
	Beat BIGINT NOT NULL,
	Tick BIGINT NOT NULL
    CONSTRAINT FK_PatternOccurrences_Pattern_Id FOREIGN KEY (PatternId) REFERENCES Patterns(Id),
    CONSTRAINT FK_PatternOccurrences_Song_Id  FOREIGN KEY (SongId) REFERENCES Songs(Id)
)

CREATE INDEX IX_PatternOccurrences_Tick ON PatternOccurrences (Tick)
CREATE INDEX IX_PatternOccurrences_Voice ON PatternOccurrences (Voice)
CREATE INDEX IX_PatternOccurrences_SongId ON PatternOccurrences (SongId)
CREATE INDEX IX_PatternOccurrences_PatternId ON PatternOccurrences (PatternId)



IF OBJECT_ID('dbo.PatternsSongs', 'U') IS NOT NULL 
  DROP TABLE dbo.PatternsSongs
  
CREATE TABLE PatternsSongs(
	Id BIGINT IDENTITY(1,1) PRIMARY KEY clustered NOT NULL,
	SongId BIGINT NOT NULL,
	PatternId BIGINT NOT NULL,
    CONSTRAINT FK_PatternsSongs_Pattern_Id FOREIGN KEY (PatternId) REFERENCES Patterns(Id),
    CONSTRAINT FK_PatternsSongs_Song_Id  FOREIGN KEY (SongId) REFERENCES Songs(Id)
)


SET IDENTITY_INSERT Styles ON

insert into Styles(Id, [Name])
values (1, 'Classic')

insert into Styles(Id, [Name])
values (2, 'Rock')

insert into Styles(Id, [Name])
values (3, 'Jazz')

insert into Styles(Id, [Name])
values (4, 'Reggae')

insert into Styles(Id, [Name])
values (5, 'Country')

insert into Styles(Id, [Name])
values (6, 'Soul')

insert into Styles(Id, [Name])
values (7, 'Blues')

insert into Styles(Id, [Name])
values (8, 'Electronic Dance')

insert into Styles(Id, [Name])
values (9, 'World')

insert into Styles(Id, [Name])
values (10, 'Religious')


SET IDENTITY_INSERT Styles OFF

insert into bands([Name], StyleId) values ('John Sebastian Bach', 1)
insert into bands([Name], StyleId) values ('Wolfgang Amadeus Mozart', 1)
insert into bands([Name], StyleId) values ('Ludwig van Beethoven', 1)
insert into bands([Name], StyleId) values ('Frederic Chopin', 1)
insert into bands([Name], StyleId) values ('Antonio Vivaldi', 1)
insert into bands([Name], StyleId) values ('George Frideric Handel', 1)
insert into bands([Name], StyleId) values ('Joseph Haydn', 1)
insert into bands([Name], StyleId) values ('Franz Schubert', 1)
insert into bands([Name], StyleId) values ('Franz Liszt', 1)
insert into bands([Name], StyleId) values ('Johannes Brahms', 1)
insert into bands([Name], StyleId) values ('AC DC', 2)
insert into bands([Name], StyleId) values ('Aerosmith', 2)
insert into bands([Name], StyleId) values ('Aha', 2)
insert into bands([Name], StyleId) values ('Al Stewart', 2)
insert into bands([Name], StyleId) values ('Alan Parsons', 2)
insert into bands([Name], StyleId) values ('Alice Cooper', 2)
insert into bands([Name], StyleId) values ('Alphaville', 2)
insert into bands([Name], StyleId) values ('America', 2)
insert into bands([Name], StyleId) values ('Asia', 2)
insert into bands([Name], StyleId) values ('Beach Boys', 2)
insert into bands([Name], StyleId) values ('Beatles', 2)
insert into bands([Name], StyleId) values ('Bee Gees', 2)
insert into bands([Name], StyleId) values ('Black Sabbath', 2)
insert into bands([Name], StyleId) values ('Boney M', 2)
insert into bands([Name], StyleId) values ('Boston', 2)
insert into bands([Name], StyleId) values ('Bread', 2)
insert into bands([Name], StyleId) values ('Bruce Hornsby', 2)
insert into bands([Name], StyleId) values ('Bryan Adams', 2)
insert into bands([Name], StyleId) values ('Cat Stevens', 2)
insert into bands([Name], StyleId) values ('Chicago', 2)
insert into bands([Name], StyleId) values ('Creedence Clearwater Revival', 2)
insert into bands([Name], StyleId) values ('Counting Crows', 2)
insert into bands([Name], StyleId) values ('Crosby Stills Nash', 2)
insert into bands([Name], StyleId) values ('Deep Purple', 2)
insert into bands([Name], StyleId) values ('Def Leppard', 2)
insert into bands([Name], StyleId) values ('Depeche Mode', 2)
insert into bands([Name], StyleId) values ('Dire Straits', 2)
insert into bands([Name], StyleId) values ('Doobie Brothers', 2)
insert into bands([Name], StyleId) values ('Doors', 2)
insert into bands([Name], StyleId) values ('Duran Duran', 2)
insert into bands([Name], StyleId) values ('Eagle-Eye Cherry', 2)
insert into bands([Name], StyleId) values ('Eagles', 2)
insert into bands([Name], StyleId) values ('Edgar Winter', 2)
insert into bands([Name], StyleId) values ('Electric Light Orchestra', 2)
insert into bands([Name], StyleId) values ('Elton John', 2)
insert into bands([Name], StyleId) values ('Emerson Lake Palmer', 2)
insert into bands([Name], StyleId) values ('Enigma', 2)
insert into bands([Name], StyleId) values ('Europe', 2)
insert into bands([Name], StyleId) values ('Focus', 2)
insert into bands([Name], StyleId) values ('Foreigner', 2)
insert into bands([Name], StyleId) values ('Frankie Goes To Hollywood', 2)
insert into bands([Name], StyleId) values ('Free', 2)
insert into bands([Name], StyleId) values ('Genesis', 2)
insert into bands([Name], StyleId) values ('George Harrison', 2)
insert into bands([Name], StyleId) values ('Gerry Rafferty', 2)
insert into bands([Name], StyleId) values ('Golden Earring', 2)
insert into bands([Name], StyleId) values ('Grateful Dead', 2)
insert into bands([Name], StyleId) values ('Guns and Roses', 2)
insert into bands([Name], StyleId) values ('Irene Cara', 2)
insert into bands([Name], StyleId) values ('James Taylor', 2)
insert into bands([Name], StyleId) values ('Jean Luc Ponty', 2)
insert into bands([Name], StyleId) values ('Jethro Tull', 2)
insert into bands([Name], StyleId) values ('Jimmy Hendrix', 2)
insert into bands([Name], StyleId) values ('Joan Jett', 2)
insert into bands([Name], StyleId) values ('Joe Cocker', 2)
insert into bands([Name], StyleId) values ('Joe Walsh', 2)
insert into bands([Name], StyleId) values ('John Lennon', 2)
insert into bands([Name], StyleId) values ('John Miles', 2)
insert into bands([Name], StyleId) values ('Kansas', 2)
insert into bands([Name], StyleId) values ('KC and the Sunshine Band', 2)
insert into bands([Name], StyleId) values ('Led Zeppelin', 2)
insert into bands([Name], StyleId) values ('Lenny Kravitz', 2)
insert into bands([Name], StyleId) values ('Manfred Mann', 2)
insert into bands([Name], StyleId) values ('Michael Jackson', 2)
insert into bands([Name], StyleId) values ('Midnight Oil', 2)
insert into bands([Name], StyleId) values ('Mike and the Mechanics', 2)
insert into bands([Name], StyleId) values ('Mike Oldfield', 2)
insert into bands([Name], StyleId) values ('Nirvana', 2)
insert into bands([Name], StyleId) values ('Oasis', 2)
insert into bands([Name], StyleId) values ('Pet Shop Boys', 2)
insert into bands([Name], StyleId) values ('Peter Frampton', 2)
insert into bands([Name], StyleId) values ('Peter Gabriel', 2)
insert into bands([Name], StyleId) values ('Phil Collins', 2)
insert into bands([Name], StyleId) values ('Pink Floyd', 2)
insert into bands([Name], StyleId) values ('Police', 2)
insert into bands([Name], StyleId) values ('Queen', 2)
insert into bands([Name], StyleId) values ('Rainbow', 2)
insert into bands([Name], StyleId) values ('REM', 2)
insert into bands([Name], StyleId) values ('Reo Speedwagon', 2)
insert into bands([Name], StyleId) values ('Rick Wakeman', 2)
insert into bands([Name], StyleId) values ('Roberta Flack', 2)
insert into bands([Name], StyleId) values ('Rolling Stones', 2)
insert into bands([Name], StyleId) values ('Roxette', 2)
insert into bands([Name], StyleId) values ('Rush', 2)
insert into bands([Name], StyleId) values ('Santana', 2)
insert into bands([Name], StyleId) values ('Sash', 2)
insert into bands([Name], StyleId) values ('Scorpions', 2)
insert into bands([Name], StyleId) values ('Seal', 2)
insert into bands([Name], StyleId) values ('Simply Red', 2)
insert into bands([Name], StyleId) values ('Sting', 2)
insert into bands([Name], StyleId) values ('Styx', 2)
insert into bands([Name], StyleId) values ('Supertramp', 2)
insert into bands([Name], StyleId) values ('Survivor', 2)
insert into bands([Name], StyleId) values ('Tears for Fears', 2)
insert into bands([Name], StyleId) values ('The Connells', 2)
insert into bands([Name], StyleId) values ('The Cult', 2)
insert into bands([Name], StyleId) values ('The Guess Who', 2)
insert into bands([Name], StyleId) values ('The Kinks', 2)
insert into bands([Name], StyleId) values ('The Knack', 2)
insert into bands([Name], StyleId) values ('The Mamas and the Papas', 2)
insert into bands([Name], StyleId) values ('The Outfield', 2)
insert into bands([Name], StyleId) values ('The Smiths', 2)
insert into bands([Name], StyleId) values ('The Verve', 2)
insert into bands([Name], StyleId) values ('Thin Lizzy', 2)
insert into bands([Name], StyleId) values ('Tom Petty', 2)
insert into bands([Name], StyleId) values ('Toto', 2)
insert into bands([Name], StyleId) values ('U2', 2)
insert into bands([Name], StyleId) values ('UB40', 2)
insert into bands([Name], StyleId) values ('Ugly Kid Joe', 2)
insert into bands([Name], StyleId) values ('Van Halen', 2)
insert into bands([Name], StyleId) values ('Village People', 2)
insert into bands([Name], StyleId) values ('Whitesnake', 2)
insert into bands([Name], StyleId) values ('Yes', 2)
insert into bands([Name], StyleId) values ('ZZ Top', 2)

insert into bands([Name], StyleId) values ('Chick Corea', 3)
insert into bands([Name], StyleId) values ('Herbie Hancock', 3)
insert into bands([Name], StyleId) values ('Weather Report', 3)