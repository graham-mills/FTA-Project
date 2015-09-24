using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Catalog Set Order Category
    /// </summary>
    public class CatalogOrderedCutsets
    {
        public int Order { get; set; }
        public List<Cutset> Cutsets;
        private List<Cutset> RedundantCutsets;
        public Catalog Catalog { get; set; }
        

        public CatalogOrderedCutsets(Cutset firstCutset, Catalog catalog)
        {
            Cutsets = new List<Cutset>();
            RedundantCutsets = new List<Cutset>();
            Order = firstCutset.GetOrder();
            Catalog = catalog;
            Cutsets.Add(firstCutset);
        }

        /// <summary>
        /// Check for existing Cutset
        /// </summary>
        /// <param name="cutset">Cutset to match</param>
        /// <returns>True or false</returns>
        public bool ContainsSet(Cutset cutset)
        {
            for(int i = 0; i < Cutsets.Count; ++i)
            {
                if (Cutsets[i].ContainsSet(cutset))
                    return true;
            }
            return false;
        }

        public void AddCutset(Cutset cutset)
        {
            Cutsets.Add(cutset);
        }

        public void RemoveCutset(Cutset cutset)
        {
            Cutsets.Remove(cutset);
        }

        /// <summary>
        /// Check Cutset for redundancy/causing redundancy
        /// </summary>
        /// <param name="cutset">Cutset to check</param>
        /// <returns>True or false</returns>
        public bool IsRedundant(Cutset cutset)
        {
            if (Optimisations.Parallelise && Optimisations.ParalleliseRedundancyCheck)
                return CheckRedundancyParallel(cutset);
            else return CheckRedundancySerial(cutset);
        }

        public bool CheckRedundancySerial(Cutset cutset)
        {
            for (int i = 0; i < Cutsets.Count; ++i)
            {
                ++CutsetGroup.ComparisonCounter;
                ReductionResult result = Cutsets[i].CheckRedundancy(cutset);
                if (result == ReductionResult.REDUNDANT)
                {
                    return true;
                }
                else if (result == ReductionResult.CAUSES_REDUNDANCY)
                {
                    Catalog.RemoveCutset(Cutsets[i--]);
                }
            }
            return false;
        }

        public bool CheckRedundancyParallel(Cutset cutset)
        {
            RedundantCutsets.Clear();
            int threadCount = Optimisations.ParallelOptions.MaxDegreeOfParallelism;
            if (Cutsets.Count < threadCount) threadCount = Cutsets.Count;
            bool isRedundant = false;
            Parallel.For(0, threadCount, Optimisations.ParallelOptions, (i, state) =>
            {
                int range = (int)Math.Ceiling((double)Cutsets.Count / threadCount);
                int start = range * i;
                int end = range * (i + 1);
                if (end > Cutsets.Count) end = Cutsets.Count;
                if (CheckRangeRedundancy(start, end, cutset))
                {
                    isRedundant = true;
                    state.Break();
                }
            });

            for (int i = 0; i < RedundantCutsets.Count; ++i)
            {
                Catalog.RemoveCutset(RedundantCutsets[i]);
            }
            return isRedundant;
        }

        private bool CheckRangeRedundancy(int start, int end, Cutset c)
        {
            bool isRedundant = false;
            for (int i = start; i < end; ++i)
            {
                ++CutsetGroup.ComparisonCounter;
                Cutset set = Cutsets[i];
                ReductionResult result = set.CheckRedundancy(c);

                if (result == ReductionResult.REDUNDANT)
                {
                    isRedundant = true;
                    break;
                }
                else if (result == ReductionResult.CAUSES_REDUNDANCY)
                {
                    lock (RedundantCutsets)
                    {
                        RedundantCutsets.Add(set);
                    }
                }
            }
            return isRedundant;
        }
    }
}
