using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    public class BinaryKey
    {
        public const int Length = 64;

        public const int KeyOffSet = 5;
        public const int MaxModules = 10;

        public static int KeyCount { get; set; }

        public ulong Index { get; set; }
        public ulong[] Keys { get; set; }

        public int MaxSet { get; set; }

        public BinaryKey()
        {
            //Keys = new ulong[Length];
            Keys = new ulong[KeyCount];
        }

        public static void UpdateKeyCount(int eventCount)
        {
            /*
             Find required keys to index the events
             i.e. If there are 190 events read in, then we need 3 keys of Length 64 (3*64=192) to contain all event values
             and the remaining keys are unused.
             Plus MaxModules to accomodate any later found module events (need a better way to accommodate possible module events)
             KeyOffSet will apply spacing between events so they are not indexed in sequence
             i.e. KeyOffSet 1 = 1110 0000 0000
                  KeyOffSet 3 = 1001 0010 0100
           */
            double x = (double)((eventCount * KeyOffSet) + (MaxModules*KeyOffSet)) / Length;
            KeyCount = (int)Math.Ceiling(x);
        }

        public void AddEvent(Event evt)
        {
            // The first parsed event will have KeyBit 0 which will be incremented for following events
            // This allows to store a possible of 4096 events in the binary key, regardless of their ID attributes
            // Only needs some of the keys initialised in the constructor
            SetBit(evt.KeyBit);

            // Using the event's ID attribute in the binary key will only allow events with ID's < 4096 to be indexed
            // Needs all keys initialised in the constructor
            // nytt60 uses ID's ~3500, but there are larger models where the ID will be too big to store in the key
            //SetBit(evt.ID);
            
            // Could extend the BK into a expandable tree for either method of storing events?
        }

        /// <summary>
        /// Set key bit to 1
        /// </summary>
        /// <param name="bit">Index to set true</param>
        private void SetBit(int bit)
        {
            int key = bit % Length;
            int index = bit / Length;
            Keys[index] |= Shift(key);
            Index |= Shift(index);

            // MaxSet finds the highest index bit used in a key
            //if (index > MaxSet) MaxSet = index;
        }

        private ulong Shift(int bits)
        {
            return ((ulong)1 << bits);
        }

        /// <summary>
        /// Original redundancy check with KeyCount limiting # of keys to check
        /// </summary>
        /// <param name="newKey"></param>
        /// <returns>ReductionResult</returns>
        public ReductionResult CheckRedundancy(BinaryKey newKey)
        {
            //return CheckRedundancyOpt(newKey);
            ulong indexResult = Index & newKey.Index;
            if (indexResult != Index && indexResult != newKey.Index)
            {
                return ReductionResult.NOT_REDUNDANT;
            }
            else
            {
                int r1 = 0;
                int r2 = 0;
                ulong keyResult;
                for (int i = 0; i < KeyCount; ++i)
                {
                    keyResult = Keys[i] & newKey.Keys[i];
                    if (keyResult == Keys[i]) r1++;
                    if (keyResult == newKey.Keys[i]) r2++;
                    if (r1 < i && r2 < i) return ReductionResult.NOT_REDUNDANT;
                }
                if (r1 == KeyCount && r2 == KeyCount) return ReductionResult.REDUNDANT;
                else if (r1 == KeyCount) return ReductionResult.REDUNDANT;
                else if (r2 == KeyCount) return ReductionResult.CAUSES_REDUNDANCY;
            }
            return ReductionResult.NOT_REDUNDANT;
        }

        /// <summary>
        /// Martin's optimised redundancy check
        /// </summary>
        /// <param name="newKey"></param>
        /// <returns>ReductionResult</returns>
        public ReductionResult CheckRedundancyOpt(BinaryKey newKey)
        {
            ulong indexResult = Index & newKey.Index;
            if (indexResult != Index && indexResult != newKey.Index)
            {
                return ReductionResult.NOT_REDUNDANT;
            }
            else
            {
                int r1 = 0;
                int r2 = 0;
                ulong keyResult;
                int limit = Math.Max(MaxSet, newKey.MaxSet) + 1;
                for (int i = 0; i < limit; ++i)
                {
                    keyResult = Keys[i] & newKey.Keys[i];
                    if (keyResult == Keys[i]) ++r1; 
                    if (keyResult == newKey.Keys[i]) ++r2;
                    if (r1 < i && r2 < i) return ReductionResult.NOT_REDUNDANT;
                }
                if (r1 == limit && r2 == limit) return ReductionResult.REDUNDANT;
                else if (r1 == limit) return ReductionResult.REDUNDANT;
                else if (r2 == limit) return ReductionResult.CAUSES_REDUNDANCY;
            }
            return ReductionResult.NOT_REDUNDANT;
        }
    }
}
