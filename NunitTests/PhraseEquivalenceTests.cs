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

	public class PhraseEquivalenceTests
	{

		private List<Note> notes = new List<Note>();



		[SetUp]
		public void Init()
		{
		}



		[Test]
		public void EquivalenceCalculationWorksAsExpected()
		{
			var phrase1 = new Phrase("48,12,12,24", "-1,-2,2,1");
			var phrase2 = new Phrase("48,12,12,24", "-2,-1,1,2");
			var input = new List<ExtractedPhrase> { new ExtractedPhrase { Phrase = phrase1 }, new ExtractedPhrase { Phrase = phrase2 } };
			PhraseAnalysis.AddEquivalences(input);
		}
	}
}
