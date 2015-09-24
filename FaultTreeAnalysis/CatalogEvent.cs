using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Catalog Basic Event Category
    /// </summary>
    public class CatalogEvent
    {
        public Event Event { get; set; }
        public int ID { get; set; }
        public List<CatalogOrderedCutsets> OrderedCutsets { get; set; }

        public Catalog Catalog { get; set; }

        public CatalogEvent(Event evt, Cutset firstCutset, Catalog catalog)
        {
            OrderedCutsets = new List<CatalogOrderedCutsets>();
            Event = evt;
            Catalog = catalog;
            ID = evt.ID;
            OrderedCutsets.Add(new CatalogOrderedCutsets(firstCutset, Catalog));
        }

        /// <summary>
        /// Gets the Cutsets stored in the CatalogEvent
        /// </summary>
        /// <returns>Cutset List</returns>
        public List<Cutset> GetCutsetList()
        {
            List<Cutset> cutsets = new List<Cutset>();
            for(int i = 0; i < OrderedCutsets.Count; ++i)
            {
                cutsets.AddRange(OrderedCutsets[i].Cutsets);
            }
            return cutsets;
        }

        /// <summary>
        /// Check for existing Cutset
        /// </summary>
        /// <param name="cutset">Cutset to match</param>
        /// <returns>True or false</returns>
        public bool ContainsSet(Cutset cutset)
        {
            CatalogOrderedCutsets existingOrder = FindOrderedCutsets(cutset.GetOrder());
            if (existingOrder != null)
            {
                return existingOrder.ContainsSet(cutset);
            }
            else return false;
        }

        /// <summary>
        /// Add cutset to existing CatalogOrderedCutsets or create new
        /// </summary>
        /// <param name="cutset">Cutset to add</param>
        public void AddCutset(Cutset cutset)
        {
            CatalogOrderedCutsets existingOrder = FindOrderedCutsets(cutset.GetOrder());
            if (existingOrder != null)
                existingOrder.AddCutset(cutset);
            else OrderedCutsets.Add(new CatalogOrderedCutsets(cutset, Catalog));
        }

        /// <summary>
        /// Find existing CatalogOrderedCutsets by order
        /// </summary>
        /// <param name="order">Order to match</param>
        /// <returns>Matched CatalogOrderedCutsets or null</returns>
        public CatalogOrderedCutsets FindOrderedCutsets(int order)
        {
            for(int i = 0; i < OrderedCutsets.Count; ++i)
            {
                if (OrderedCutsets[i].Order == order)
                    return OrderedCutsets[i];
            }
            return null;
        }

        /// <summary>
        /// Removes a Cutset
        /// </summary>
        /// <param name="cutset">Cutset to remove</param>
        public void RemoveCutset(Cutset cutset)
        {
            CatalogOrderedCutsets existingOrder = FindOrderedCutsets(cutset.GetOrder());
            if(existingOrder != null)
            {
                existingOrder.RemoveCutset(cutset);
            }
        }

        /// <summary>
        /// Checks for Cutset causing redundancy/made redundant
        /// </summary>
        /// <param name="cutset">Cutset to check</param>
        /// <returns>True or false</returns>
        public bool IsRedundant(Cutset cutset)
        {
            for(int i = 0; i < OrderedCutsets.Count; ++i)
            {
                if (OrderedCutsets[i].IsRedundant(cutset))
                    return true;
            }
            return false;
        }


    }
}
