using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// System Event / Terminal Node
    /// </summary>
    public class Event : Node
    {
        public static int EventCounter = 0;
        public int KeyBit { get; set; }
        public BinaryKey BinaryKey { get; set; }
        public override int FindMinimumVisit()
        {
            return FirstVisit;
        }
        public override int FindMaximumVisit()
        {
            return LastVisit;
        }

        /// <summary>
        /// Gives the Event a position for indexing in the BinaryKey
        /// </summary>
        protected void SetKeyBit()
        {
            if(Optimisations.BinaryKey)
            {
                KeyBit = EventCounter;
                EventCounter += BinaryKey.KeyOffSet;
                //BinaryKey = new BinaryKey();
                //BinaryKey.AddEvent(this);
            }
        }
    }
}
