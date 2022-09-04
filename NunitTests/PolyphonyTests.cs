using NUnit.Framework;
using Sinphinity.Models;
using SinphinityProcMelodyAnalyser.MelodyLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NunitTests
{
	public class PolyphonyTests
	{

		private List<Note> notes = new List<Note>();



		[SetUp]
		public void Init()
		{
		}



		[Test]
		public void IsTrackPolyphonicWorksAsExpected()
		{
			notes.Clear();
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 0, EndSinceBeginningOfSongInTicks = 96, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 48, EndSinceBeginningOfSongInTicks = 144, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 144, EndSinceBeginningOfSongInTicks = 192, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 192, EndSinceBeginningOfSongInTicks = 384, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 210, EndSinceBeginningOfSongInTicks = 380, Voice = 0, Guid = Guid.NewGuid() });

			Assert.IsFalse(SongPreprocess.IsTrackPolyphonic(notes));

			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 20, EndSinceBeginningOfSongInTicks = 400, Voice = 0, Guid = Guid.NewGuid() });
			Assert.IsTrue(SongPreprocess.IsTrackPolyphonic(notes));
		}


		[Test]
		public void IsChordsTrackWorksAsExpected()
		{
			notes.Clear();
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 0, EndSinceBeginningOfSongInTicks = 96, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 2, EndSinceBeginningOfSongInTicks = 97, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 144, EndSinceBeginningOfSongInTicks = 192, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 143, EndSinceBeginningOfSongInTicks = 195, Voice = 0, Guid = Guid.NewGuid() });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 290, EndSinceBeginningOfSongInTicks = 380, Voice = 0, Guid = Guid.NewGuid() });


			Assert.IsTrue(SongPreprocess.IsChordsTrack(notes));

			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 20, EndSinceBeginningOfSongInTicks = 400, Voice = 0, Guid = Guid.NewGuid() });
			Assert.IsFalse(SongPreprocess.IsChordsTrack(notes));
		}

		[Test]
		public void EquivalencesDetectedAsExpected()
		{
			var phrase1 = new Phrase("24*7", "2,2,1,-3,2,-4,7");
			var phrase2 = new Phrase("24*7", "2,2,1,-3,2,-4,8");
			var ep = new List<ExtractedPhrase>();
			ep.Add(new ExtractedPhrase { Phrase = phrase1 });
			ep.Add(new ExtractedPhrase { Phrase = phrase2 });

			var dist = PhraseDistance.GetPitchDistance(phrase1, phrase2);
			PhraseAnalysis.AddEquivalences(ep);


		}


		[Test]
		public void GetNonContiguousPatternsAsExpected()
		{
			notes.Clear();
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 0, EndSinceBeginningOfSongInTicks = 10, Pitch = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 10, EndSinceBeginningOfSongInTicks = 20, Pitch = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 20, EndSinceBeginningOfSongInTicks = 30, Pitch = 27 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 30, EndSinceBeginningOfSongInTicks = 40, Pitch = 45 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 40, EndSinceBeginningOfSongInTicks = 50, Pitch = 23 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 50, EndSinceBeginningOfSongInTicks = 60, Pitch = 45 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 60, EndSinceBeginningOfSongInTicks = 70, Pitch = 12 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 70, EndSinceBeginningOfSongInTicks = 80, Pitch = 66 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 80, EndSinceBeginningOfSongInTicks = 90, Pitch = 32 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 90, EndSinceBeginningOfSongInTicks = 100, Pitch = 56 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 100, EndSinceBeginningOfSongInTicks = 110, Pitch = 78 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 110, EndSinceBeginningOfSongInTicks = 120, Pitch =23 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 120, EndSinceBeginningOfSongInTicks = 130, Pitch =89 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 130, EndSinceBeginningOfSongInTicks = 140, Pitch =0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 140, EndSinceBeginningOfSongInTicks = 150, Pitch =0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 150, EndSinceBeginningOfSongInTicks = 160, Pitch =0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 160, EndSinceBeginningOfSongInTicks = 170, Pitch =33 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 170, EndSinceBeginningOfSongInTicks = 180, Pitch =33 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 180, EndSinceBeginningOfSongInTicks = 190, Pitch =33 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 190, EndSinceBeginningOfSongInTicks = 200, Pitch =19 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 200, EndSinceBeginningOfSongInTicks = 210, Pitch =27 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 210, EndSinceBeginningOfSongInTicks = 220, Pitch =45 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 220, EndSinceBeginningOfSongInTicks = 230, Pitch =23 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 230, EndSinceBeginningOfSongInTicks = 240, Pitch =45 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 240, EndSinceBeginningOfSongInTicks = 250, Pitch =12 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 250, EndSinceBeginningOfSongInTicks = 260, Pitch =66 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 260, EndSinceBeginningOfSongInTicks = 270, Pitch =13 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 270, EndSinceBeginningOfSongInTicks = 280, Pitch =55 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 280, EndSinceBeginningOfSongInTicks = 290, Pitch =68 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 290, EndSinceBeginningOfSongInTicks = 300, Pitch =27 });

			var pitchesFindings = PatternDetection.GetNonContiguousPatterns(notes, PhraseTypeEnum.Pitches);
			var notesFindings = PatternDetection.GetNonContiguousPatterns(notes, PhraseTypeEnum.Both);

		}
	}
}
