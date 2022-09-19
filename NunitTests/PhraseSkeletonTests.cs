
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
	public class PhraseSkeletonTests
	{
		private List<Note> notes = new List<Note>();



		[SetUp]
		public void Init()
		{
		}



		[Test]
		public void RemoveShortNotesBetweenLongNotesWorksAsExpected()
		{
			notes.Clear();
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 0, EndSinceBeginningOfSongInTicks = 96, Pitch = 50, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 96, EndSinceBeginningOfSongInTicks = 192, Pitch = 51, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 192, EndSinceBeginningOfSongInTicks = 288, Pitch = 52, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 288, EndSinceBeginningOfSongInTicks = 384, Pitch = 53, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 384, EndSinceBeginningOfSongInTicks = 472, Pitch = 54, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 472, EndSinceBeginningOfSongInTicks = 475, Pitch = 55, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 475, EndSinceBeginningOfSongInTicks = 478, Pitch = 56, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 478, EndSinceBeginningOfSongInTicks = 480, Pitch = 57, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 480, EndSinceBeginningOfSongInTicks = 576, Pitch = 58, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 576, EndSinceBeginningOfSongInTicks = 672, Pitch = 59, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 672, EndSinceBeginningOfSongInTicks = 768, Pitch = 60, Voice = 0 });

			var phrase = new Phrase(notes);

			var newlMetrics = "96*7";
			var newPitches = "1,1,1,1,1,4,1";
			var phraseWithShortNotesRemoved = PhraseSkeleton.RemoveShortNotesBetweenLongNotes(phrase);
			Assert.AreEqual(newPitches, phraseWithShortNotesRemoved.PitchesAsString);
			Assert.AreEqual(newlMetrics, phraseWithShortNotesRemoved.MetricsAsString);
		}
		[Test]
		public void RemovePassingNotesWorksAsExpected()
		{
			notes.Clear();
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 0, EndSinceBeginningOfSongInTicks = 96, Pitch = 50, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 96, EndSinceBeginningOfSongInTicks = 192, Pitch = 51, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 192, EndSinceBeginningOfSongInTicks = 288, Pitch = 52, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 288, EndSinceBeginningOfSongInTicks = 384, Pitch = 53, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 384, EndSinceBeginningOfSongInTicks = 480, Pitch = 54, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 480, EndSinceBeginningOfSongInTicks = 576, Pitch = 52, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 576, EndSinceBeginningOfSongInTicks = 672, Pitch = 51, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 672, EndSinceBeginningOfSongInTicks = 768, Pitch = 40, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 768, EndSinceBeginningOfSongInTicks = 864, Pitch = 60, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 864, EndSinceBeginningOfSongInTicks = 960, Pitch = 59, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 960, EndSinceBeginningOfSongInTicks = 1056, Pitch = 60, Voice = 0 });

			var phrase = new Phrase(notes);

			var newlMetrics = "384,288,96,96,96";
			var newPitches = "4,-14,20,-1,1";
			var phraseWithPassingNotesRemoved = PhraseSkeleton.RemovePassingNotes(phrase);
			Assert.AreEqual(newPitches, phraseWithPassingNotesRemoved.PitchesAsString);
			Assert.AreEqual(newlMetrics, phraseWithPassingNotesRemoved.MetricsAsString);
		}

		[Test]
		public void RRemoveRepeatingNotesWorksAsExpected()
		{
			notes.Clear();
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 0, EndSinceBeginningOfSongInTicks = 96, Pitch = 50, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 96, EndSinceBeginningOfSongInTicks = 192, Pitch = 51, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 192, EndSinceBeginningOfSongInTicks = 288, Pitch = 52, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 288, EndSinceBeginningOfSongInTicks = 384, Pitch = 53, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 384, EndSinceBeginningOfSongInTicks = 480, Pitch = 53, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 480, EndSinceBeginningOfSongInTicks = 576, Pitch = 53, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 576, EndSinceBeginningOfSongInTicks = 672, Pitch = 51, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 672, EndSinceBeginningOfSongInTicks = 768, Pitch = 40, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 768, EndSinceBeginningOfSongInTicks = 864, Pitch = 60, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 864, EndSinceBeginningOfSongInTicks = 960, Pitch = 59, Voice = 0 });
			notes.Add(new Note { StartSinceBeginningOfSongInTicks = 960, EndSinceBeginningOfSongInTicks = 1056, Pitch = 60, Voice = 0 });

			var phrase = new Phrase(notes);

			var newlMetrics = "96,96,96,288,96,96,96,96";
			var newPitches = "1,1,1,-2,-11,20,-1,1";
			var phraseWithRepeatingNotesRemoved = PhraseSkeleton.RemoveRepeatingNotes(phrase);
			Assert.AreEqual(newPitches, phraseWithRepeatingNotesRemoved.PitchesAsString);
			Assert.AreEqual(newlMetrics, phraseWithRepeatingNotesRemoved.MetricsAsString);
		}
	}
}
