﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sinphinity.Models;

namespace NunitTests
{
	internal class MelodyAnalyzerDiscretizationTests
	{
		private List<Note> notes;
		private List<Bar> bars;


		[SetUp]
		public void Init()
		{
			var notesAsJson = @"[{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":108,""StartSinceBeginningOfSongInTicks"":960,""EndSinceBeginningOfSongInTicks"":978,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":18,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":104,""StartSinceBeginningOfSongInTicks"":1024,""EndSinceBeginningOfSongInTicks"":1040,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":77,""Volume"":96,""StartSinceBeginningOfSongInTicks"":1056,""EndSinceBeginningOfSongInTicks"":1080,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":108,""StartSinceBeginningOfSongInTicks"":1155,""EndSinceBeginningOfSongInTicks"":1165,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":10,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":122,""StartSinceBeginningOfSongInTicks"":1248,""EndSinceBeginningOfSongInTicks"":1264,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":79,""Volume"":121,""StartSinceBeginningOfSongInTicks"":1344,""EndSinceBeginningOfSongInTicks"":1360,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":112,""StartSinceBeginningOfSongInTicks"":1539,""EndSinceBeginningOfSongInTicks"":1554,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":15,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":101,""StartSinceBeginningOfSongInTicks"":1600,""EndSinceBeginningOfSongInTicks"":1614,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":14,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":77,""Volume"":108,""StartSinceBeginningOfSongInTicks"":1632,""EndSinceBeginningOfSongInTicks"":1656,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":112,""StartSinceBeginningOfSongInTicks"":1728,""EndSinceBeginningOfSongInTicks"":1743,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":15,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":82,""Volume"":127,""StartSinceBeginningOfSongInTicks"":1824,""EndSinceBeginningOfSongInTicks"":1840,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":127,""StartSinceBeginningOfSongInTicks"":1920,""EndSinceBeginningOfSongInTicks"":1936,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":108,""StartSinceBeginningOfSongInTicks"":2115,""EndSinceBeginningOfSongInTicks"":2130,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":15,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":97,""StartSinceBeginningOfSongInTicks"":2176,""EndSinceBeginningOfSongInTicks"":2190,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":14,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":87,""Volume"":108,""StartSinceBeginningOfSongInTicks"":2208,""EndSinceBeginningOfSongInTicks"":2238,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":30,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":84,""Volume"":112,""StartSinceBeginningOfSongInTicks"":2304,""EndSinceBeginningOfSongInTicks"":2320,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":116,""StartSinceBeginningOfSongInTicks"":2403,""EndSinceBeginningOfSongInTicks"":2418,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":15,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":108,""StartSinceBeginningOfSongInTicks"":2464,""EndSinceBeginningOfSongInTicks"":2478,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":14,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":79,""Volume"":116,""StartSinceBeginningOfSongInTicks"":2496,""EndSinceBeginningOfSongInTicks"":2512,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":77,""Volume"":112,""StartSinceBeginningOfSongInTicks"":2592,""EndSinceBeginningOfSongInTicks"":2616,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":85,""Volume"":116,""StartSinceBeginningOfSongInTicks"":2688,""EndSinceBeginningOfSongInTicks"":2706,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":18,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":85,""Volume"":108,""StartSinceBeginningOfSongInTicks"":2752,""EndSinceBeginningOfSongInTicks"":2766,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":14,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":84,""Volume"":121,""StartSinceBeginningOfSongInTicks"":2784,""EndSinceBeginningOfSongInTicks"":2808,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":122,""StartSinceBeginningOfSongInTicks"":2880,""EndSinceBeginningOfSongInTicks"":2895,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":15,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":82,""Volume"":122,""StartSinceBeginningOfSongInTicks"":2979,""EndSinceBeginningOfSongInTicks"":2992,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":13,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":122,""StartSinceBeginningOfSongInTicks"":3072,""EndSinceBeginningOfSongInTicks"":3084,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":12,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":116,""StartSinceBeginningOfSongInTicks"":3264,""EndSinceBeginningOfSongInTicks"":3288,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":116,""StartSinceBeginningOfSongInTicks"":3328,""EndSinceBeginningOfSongInTicks"":3348,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":20,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":77,""Volume"":96,""StartSinceBeginningOfSongInTicks"":3360,""EndSinceBeginningOfSongInTicks"":3390,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":30,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":108,""StartSinceBeginningOfSongInTicks"":3456,""EndSinceBeginningOfSongInTicks"":3472,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":127,""StartSinceBeginningOfSongInTicks"":3555,""EndSinceBeginningOfSongInTicks"":3565,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":10,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":79,""Volume"":112,""StartSinceBeginningOfSongInTicks"":3648,""EndSinceBeginningOfSongInTicks"":3664,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":112,""StartSinceBeginningOfSongInTicks"":3843,""EndSinceBeginningOfSongInTicks"":3856,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":13,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":99,""StartSinceBeginningOfSongInTicks"":3904,""EndSinceBeginningOfSongInTicks"":3915,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":11,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":77,""Volume"":116,""StartSinceBeginningOfSongInTicks"":3936,""EndSinceBeginningOfSongInTicks"":3960,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":122,""StartSinceBeginningOfSongInTicks"":4032,""EndSinceBeginningOfSongInTicks"":4056,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":82,""Volume"":127,""StartSinceBeginningOfSongInTicks"":4128,""EndSinceBeginningOfSongInTicks"":4146,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":18,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":127,""StartSinceBeginningOfSongInTicks"":4224,""EndSinceBeginningOfSongInTicks"":4248,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":112,""StartSinceBeginningOfSongInTicks"":4416,""EndSinceBeginningOfSongInTicks"":4440,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":75,""Volume"":99,""StartSinceBeginningOfSongInTicks"":4480,""EndSinceBeginningOfSongInTicks"":4500,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":20,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":87,""Volume"":108,""StartSinceBeginningOfSongInTicks"":4512,""EndSinceBeginningOfSongInTicks"":4560,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":48,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":84,""Volume"":103,""StartSinceBeginningOfSongInTicks"":4608,""EndSinceBeginningOfSongInTicks"":4638,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":30,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":116,""StartSinceBeginningOfSongInTicks"":4704,""EndSinceBeginningOfSongInTicks"":4720,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":116,""StartSinceBeginningOfSongInTicks"":4768,""EndSinceBeginningOfSongInTicks"":4780,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":12,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":79,""Volume"":105,""StartSinceBeginningOfSongInTicks"":4800,""EndSinceBeginningOfSongInTicks"":4816,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":16,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":77,""Volume"":116,""StartSinceBeginningOfSongInTicks"":4896,""EndSinceBeginningOfSongInTicks"":4920,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":24,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":85,""Volume"":116,""StartSinceBeginningOfSongInTicks"":4995,""EndSinceBeginningOfSongInTicks"":5010,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":15,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":85,""Volume"":112,""StartSinceBeginningOfSongInTicks"":5056,""EndSinceBeginningOfSongInTicks"":5065,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":9,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":84,""Volume"":121,""StartSinceBeginningOfSongInTicks"":5088,""EndSinceBeginningOfSongInTicks"":5106,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":18,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":127,""StartSinceBeginningOfSongInTicks"":5184,""EndSinceBeginningOfSongInTicks"":5196,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":12,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":82,""Volume"":127,""StartSinceBeginningOfSongInTicks"":5283,""EndSinceBeginningOfSongInTicks"":5295,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":12,""PitchBending"":null},{""Guid"":""00000000-0000-0000-0000-000000000000"",""Pitch"":80,""Volume"":127,""StartSinceBeginningOfSongInTicks"":5379,""EndSinceBeginningOfSongInTicks"":5388,""IsPercussion"":false,""Voice"":0,""SubVoice"":0,""Instrument"":11,""DurationInTicks"":9,""PitchBending"":null}]";
			notes = JsonConvert.DeserializeObject<List<Note>>(notesAsJson);
			var barsAsJson = @"[{""BarNumber"":1,""TicksFromBeginningOfSong"":0,""EndTick"":384,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":2,""TicksFromBeginningOfSong"":384,""EndTick"":768,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":3,""TicksFromBeginningOfSong"":768,""EndTick"":1152,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":4,""TicksFromBeginningOfSong"":1152,""EndTick"":1536,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":5,""TicksFromBeginningOfSong"":1536,""EndTick"":1920,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":6,""TicksFromBeginningOfSong"":1920,""EndTick"":2304,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":7,""TicksFromBeginningOfSong"":2304,""EndTick"":2688,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":8,""TicksFromBeginningOfSong"":2688,""EndTick"":3072,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":9,""TicksFromBeginningOfSong"":3072,""EndTick"":3456,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":10,""TicksFromBeginningOfSong"":3456,""EndTick"":3840,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":11,""TicksFromBeginningOfSong"":3840,""EndTick"":4224,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":12,""TicksFromBeginningOfSong"":4224,""EndTick"":4608,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":13,""TicksFromBeginningOfSong"":4608,""EndTick"":4992,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":14,""TicksFromBeginningOfSong"":4992,""EndTick"":5376,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":false,""TempoInMicrosecondsPerQuarterNote"":652164},{""BarNumber"":15,""TicksFromBeginningOfSong"":5376,""EndTick"":5760,""TimeSignature"":{""Numerator"":4,""Denominator"":4},""KeySignature"":{""key"":-3,""scale"":0},""LengthInTicks"":384,""HasTriplets"":true,""TempoInMicrosecondsPerQuarterNote"":652164}]";
			bars = JsonConvert.DeserializeObject<List<Bar>>(barsAsJson);
		}



		private List<long> GetDurations(List<Note> notes)
		{
			var retObj = new List<long>();
			for (var i = 0; i < notes.Count - 1; i++)
			{
				retObj.Add(notes[i + 1].StartSinceBeginningOfSongInTicks - notes[i].StartSinceBeginningOfSongInTicks);
			}
			return retObj;
		}
		[Test]
		public void CheckFixStrangeDurations()
        {
			var badDurations = "29,32,90,91,92,93,94,95,96,97,98,99,101,102,188,193,194,196,190,192,197,3,5,92";
			var quant = badDurations.Split(',').Length;
			var goodDurations = "32,32,96,96,96,96,96,96,96,96,96,96,96,96,192,192,192,192,192,192,192,3,5,88";
			var quanti = goodDurations.Split(',').Length;
			//var phrase = new PhraseMetrics(badDurations);
			//Assert.AreEqual(goodDurations, phrase.AsString);
              

		}

	}
}

