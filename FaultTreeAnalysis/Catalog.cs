using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Catalog Data Structure
    /// </summary>
    public class Catalog : CutsetGroup
    {
        public List<CatalogEvent> Events { get; set; }
        private int _count { get; set; }

        public Catalog()
        {
            Events = new List<CatalogEvent>();
            _count = 0;
        }

        public Catalog(Cutset firstCutset) : this()
        {
            AddCutset(firstCutset);
        }

        public override int Count()
        {
            return _count;
        }

        /// <summary>
        /// Add single cutset to catalog
        /// </summary>
        /// <param name="cutset">New cutset</param>
        public override void AddCutset(Cutset cutset, bool checkRedundancy = true)
        {
            lock (locker)
            {
                // Check for redundancy before adding
                if (!checkRedundancy || !IsRedundant(cutset))
                {
                    // Add cutset for each of its basic events
                    for (int i = 0; i < cutset.Events.Count; ++i)
                    {
                        AddCutsetForEvent(cutset, cutset.Events[i]);
                    }
                    // Update unique cutset count
                    ++_count;
                }
            }
        }

        /// <summary>
        /// Add another catalog's cutsets to this one
        /// </summary>
        /// <param name="cutsets">Catalog to add</param>
        public override void AddCutsets(CutsetGroup cutsets, bool checkRedundancy = true)
        {
            List<Cutset> newCutsets = ((Catalog)cutsets).GetCutsetList();
            for(int i  = 0; i < newCutsets.Count; ++i)
            {
                AddCutset(newCutsets[i], checkRedundancy);
            }
        }

        /// <summary>
        /// Adds cutset to existing catalog event or creates new
        /// </summary>
        /// <param name="cutset">Cutset to add</param>
        /// <param name="evt">Event to add for</param>
        public void AddCutsetForEvent(Cutset cutset, Event evt)
        {
            // Get CatalogEvent
            CatalogEvent existingEvent = FindCatalogEvent(evt);
            if (existingEvent != null)
                existingEvent.AddCutset(cutset);
            else Events.Add(new CatalogEvent(evt, cutset, this));
        }

        /// <summary>
        /// Check if cutset is redundant or causes redundancy in catalog
        /// </summary>
        /// <param name="cutset">Cutset to check</param>
        /// <returns>True or false</returns>
        public bool IsRedundant(Cutset cutset)
        {
            for(int i = 0; i < cutset.Events.Count; ++i)
            {
                CatalogEvent existingEvent = FindCatalogEvent(cutset.Events[i]);
                if(existingEvent != null)
                {
                    if (existingEvent.IsRedundant(cutset))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove all instances of a cutset in the catalog
        /// </summary>
        /// <param name="cutset">Cutset to remove</param>
        public void RemoveCutset(Cutset cutset)
        {
            for(int i = 0; i < cutset.Events.Count; ++i)
            {
                // Remove cutset for each CatalogEvent
                FindCatalogEvent(cutset.Events[i]).RemoveCutset(cutset);
            }
            // Update unique cutset count
            --_count;
        }

        /// <summary>
        /// Find existing CatalogEvent by Event
        /// </summary>
        /// <param name="evt">Event to find</param>
        /// <returns>Matched CatalogEvent or null</returns>
        public CatalogEvent FindCatalogEvent(Event evt)
        {
            for(int i = 0; i < Events.Count; ++i)
            {
                if (Events[i].ID == evt.ID)
                    return Events[i];
            }
            return null;
        }

        /// <summary>
        /// Get union set of Catalog's Cutsets
        /// </summary>
        /// <returns>Cutset List</returns>
        public List<Cutset> GetCutsetList()
        {
            List<Cutset> cutsets = new List<Cutset>();
            for(int i = 0; i < Events.Count; ++i)
            {
                List<Cutset> eventSets = Events[i].GetCutsetList();
                cutsets = cutsets.Union(eventSets).ToList();
            }
            return cutsets;
        }

        /// <summary>
        /// Combine (AND) with another catalog's cutsets
        /// </summary>
        /// <param name="cutsets">Catalog to combine</param>
        public override void CombineCutsets(CutsetGroup cutsets)
        {
            if (Count() == 0)
            {
                Events = ((Catalog)cutsets).Events;
                _count = ((Catalog)cutsets).Count();
            }
            else
            {
                // Create new catalog to store combined sets
                Catalog newCatalog = new Catalog();
                List<Cutset> thisCutsets = GetCutsetList();
                List<Cutset> thatCutsets = ((Catalog)cutsets).GetCutsetList();

                Cutset currentSet, comparedSet, newSet;
                for (int i = 0; i < thisCutsets.Count; ++i)
                {
                    currentSet = thisCutsets[i];
                    for (int j = 0; j < thatCutsets.Count; ++j)
                    {
                        comparedSet = thatCutsets[j];
                        newSet = new Cutset();
                        newSet.AddEvents(currentSet.Events);
                        newSet.AddEvents(comparedSet.Events);
                        newCatalog.AddCutset(newSet);
                    }
                }
                // Replace this catalog events with new ones and update count
                Events = newCatalog.Events;
                _count = newCatalog.Count();
            }
        }

        /// <summary>
        /// Check for existing cutset in catalog
        /// </summary>
        /// <param name="cutset">Cutset to match</param>
        /// <returns>True or false</returns>
        public override bool ContainsSet(Cutset cutset)
        {
            CatalogEvent existingEvent = FindCatalogEvent(cutset.Events[0]);
            if(existingEvent != null)
            {
                return existingEvent.ContainsSet(cutset);
            }
            else return false;
        }

        /// <summary>
        /// Finds ModuleEvents and expands into their BasicEvents
        /// </summary>
        public override void ExpandModules()
        {
            // Create new catalog to store result of expanding modules
            Catalog newCatalog = new Catalog();
            List<Cutset> unionSets = GetCutsetList();
            
            for(int i = 0; i < unionSets.Count; ++i)
            {
                if(unionSets[i].ContainsModule())
                {
                    List<Cutset> expandedSets = ((Catalog)unionSets[i].ExpandModules()).GetCutsetList();
                    // Remove ModuleEvents
                    unionSets.RemoveAt(i--);
                    // Add it's contained Events - Nested modules are added to the list and expanded in later iterations
                    unionSets.AddRange(expandedSets);
                }
            }

            // Add fully expanded sets to new catalog
            for(int i = 0; i < unionSets.Count; ++i)
            {
                newCatalog.AddCutset(unionSets[i], false);
            }
            // Replace this catalog's events with the expanded catalog and update count
            Events = newCatalog.Events;
            _count = newCatalog.Count();
        }

        
    }
}
