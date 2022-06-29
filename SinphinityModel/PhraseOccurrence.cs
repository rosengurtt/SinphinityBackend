using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    public class PhraseOccurrence
    {
        public byte Voice { get; set; }
        public long BarNumber { get; set; }
        public long Beat { get; set; }
        public long StartTick { get; set; }
        public long EndTick { get; set; }
        public Phrase Phrase { get; set; }
        public Song Song { get; set; }
        public  Band Band{ get; set; }
    }
}
