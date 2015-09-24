using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Set of Events
    /// </summary>
    public class Cutset
    {
        public List<Event> Events { get; set; }
        public BinaryKey BinaryKey { get; set; }

        public Cutset()
        {
            Events = new List<Event>();
            if (Optimisations.BinaryKey) BinaryKey = new BinaryKey();
        }
        public Cutset(Event firstEvent) : this()
        {
            Events.Add(firstEvent);
            if (Optimisations.BinaryKey) BinaryKey.AddEvent(firstEvent);
        }

        public int GetOrder()
        {
            return Events.Count;
        }

        /// <summary>
        /// Add event to set if it doesn't already contain it
        /// </summary>
        /// <param name="newEvent">New Event to add</param>
        public void AddEvent(Event newEvent)
        {
            // May be possible to replace ContainsEvent method with a comparison of this set's BK with the event's BK
            if (ContainsEvent(newEvent))
            {
                return;
            }
            else
            {
                Events.Add(newEvent);
            }

            if (Optimisations.BinaryKey)
            {
                BinaryKey.AddEvent(newEvent);
            }
            
        }

        public void AddEvents(List<Event> newEvents)
        {
            for (int i = 0; i < newEvents.Count; ++i)
                AddEvent(newEvents[i]);
        }

        /// <summary>
        /// Check set for existing Event
        /// </summary>
        /// <param name="evt">Event to match</param>
        /// <returns>True or false</returns>
        public bool ContainsEvent(Event evt)
        {
            if (Events.Count == 0) return false;

            for(int i = 0; i < Events.Count; ++i)
            {
                if (Events[i] == evt)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check set for subset
        /// </summary>
        /// <param name="c">Cutset to match as subset</param>
        /// <returns>True or false</returns>
        public bool ContainsSet(Cutset c)
        {
           if (c.Events.Count > Events.Count) return false;
           else return CompareEvents(c);
        }

        /// <summary>
        /// Returns true if checked set's events all exist in this one
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool CompareEvents(Cutset c)
        {
            for (int i = 0; i < c.Events.Count; ++i)
            {
                if (!ContainsEvent(c.Events[i]))
                    return false;
            }
            return true;
        }

        public ReductionResult CheckRedundancy(Cutset c)
        {
            if (Optimisations.BinaryKey)
            {
                return BinaryKey.CheckRedundancy(c.BinaryKey);
            }
            else
            {
                return BoringRedundancyCheck(c);
            }
        }

        private ReductionResult BoringRedundancyCheck(Cutset c)
        {
            if (GetOrder() > c.GetOrder())
            {
                // If new cutset is contained by this set, this set is made redundant
                if (ContainsSet(c))
                {
                    return ReductionResult.CAUSES_REDUNDANCY;
                }

            }
            else if (GetOrder() == c.GetOrder())
            {
                // If checked cutset is identical to this cutset, it is redundant
                if (ContainsSet(c))
                {
                    return ReductionResult.REDUNDANT;
                }
            }
            else if (GetOrder() < c.GetOrder())
            {
                // If checked cutset contains this set, it is redundant
                if (c.ContainsSet(this))
                {
                    return ReductionResult.REDUNDANT;
                }
            }
            return ReductionResult.NOT_REDUNDANT;
        }

        /// <summary>
        /// Check for a ModuleEvent in the set
        /// </summary>
        /// <returns>True or false</returns>
        public bool ContainsModule()
        {

            for (int i = 0; i < Events.Count; ++i)
            {
                if (Events[i] is ModuleEvent)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Expands ModuleEvents into their contained Events
        /// </summary>
        /// <returns>Expanded CutsetGroup</returns>
        public CutsetGroup ExpandModules()
        {
            CutsetGroup newCutsets = CutsetGroup.Create();

            for (int i = 0; i < Events.Count; ++i )
            {
                if(Events[i] is BasicEvent)
                    newCutsets.CombineCutsets(CutsetGroup.Create(new Cutset(Events[i])));
                else
                {
                    newCutsets.CombineCutsets(Events[i].Cutsets);
                }
            }

            return newCutsets;
        }
    }
}
