using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Cutset List Structure
    /// </summary>
    public class CutsetList : CutsetGroup
    {
        public List<Cutset> Cutsets { get; set; }

        private List<Cutset> RedundantCutsets { get; set; }

        private ReductionResult ReductionResult { get; set; }

        public CutsetList()
        {
            Cutsets = new List<Cutset>();
            RedundantCutsets = new List<Cutset>();
        }

        public CutsetList(Cutset firstCutset) : this()
        {
            AddCutset(firstCutset);
        }

        public override int Count()
        {
            return Cutsets.Count;
        }

        public override void AddCutset(Cutset cutset, bool checkRedundancy = true)
        {
            lock (locker)
            {
                if (!checkRedundancy || !IsRedundant(cutset))
                {
                    Cutsets.Add(cutset);
                }
            }
        }

        /// <summary>
        /// Add another CutsetList's sets to this
        /// </summary>
        /// <param name="c">CutsetList to add</param>
        public override void AddCutsets(CutsetGroup c, bool checkRedundancy = true)
        {
            for (int i = 0; i < c.Count(); ++i)
            {
                AddCutset(((CutsetList)c).Cutsets[i], checkRedundancy);
            }
        }

        /// <summary>
        /// Check if Cutset is made redundant/causes redundancy
        /// </summary>
        /// <param name="c">Cutset to check</param>
        /// <returns>True or false</returns>
        public bool IsRedundant(Cutset c)
        {
            if (Cutsets.Count == 0) return false;

            if (Optimisations.Parallelise && Optimisations.ParalleliseRedundancyCheck)
            {
                return CheckRedundancyParallel(c);
            }
            else
            {
                return CheckRedundancySerial(c);
            }
        }

        /// <summary>
        /// Redundancy check in serial
        /// </summary>
        /// <param name="c">Cutset to check</param>
        /// <returns>True of false</returns>
        private bool CheckRedundancySerial(Cutset c)
        {
            for (int i = 0; i < Cutsets.Count; ++i)
            {
                ComparisonCounter++;
                ReductionResult result = Cutsets[i].CheckRedundancy(c);

                if (result == ReductionResult.REDUNDANT)
                {
                    return true;
                }
                else if (result == ReductionResult.CAUSES_REDUNDANCY)
                {
                    Cutsets.RemoveAt(i--);
                }
            }
            return false;
        }

        /// <summary>
        /// Redundancy check in parallel (slower)
        /// </summary>
        /// <param name="c">Cutset to check</param>
        /// <returns>True or false</returns>
        private bool CheckRedundancyParallel(Cutset c)
        {
            RedundantCutsets.Clear();
            int threadCount = Optimisations.ParallelOptions.MaxDegreeOfParallelism;
            if (Cutsets.Count < threadCount) threadCount = Cutsets.Count; 
            bool isRedundant = false;
            Parallel.For(0, threadCount, Optimisations.ParallelOptions, (i, state) =>
            {
                // Divide Cutset list up into equal ranges
                int range = (int) Math.Ceiling((double)Cutsets.Count/threadCount);
                int start = range * i;
                int end = range * (i+1);
                if (end > Cutsets.Count) end = Cutsets.Count;
                if(CheckRangeRedundancy(start, end, c))
                {
                    isRedundant = true;
                    state.Break();
                }
            });

            // Remove any made-redundant cutsets
            for (int i = 0; i < RedundantCutsets.Count; ++i )
            {
                Cutsets.Remove(RedundantCutsets[i]);
            }
            return isRedundant;
        }

        /// <summary>
        /// Performs redundancy check over a divided range of Cutsets
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="end">End index</param>
        /// <param name="c">Cutset to check</param>
        /// <returns>True or false</returns>
        private bool CheckRangeRedundancy(int start, int end, Cutset c)
        {
            bool isRedundant = false;
            for(int i = start; i < end; ++i)
            {
                ComparisonCounter++;
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


        /// <summary>
        /// Check for existing Cutset
        /// </summary>
        /// <param name="c">Cutset to match</param>
        /// <returns>True or false</returns>
        public override bool ContainsSet(Cutset c)
        {
            foreach (Cutset set in Cutsets)
            {
                if (c.GetOrder() == set.GetOrder() && set.ContainsSet(c))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Combine (AND) Cutsets with another CutsetList's
        /// </summary>
        /// <param name="cutsets">CutsetList to merge with</param>
        public override void CombineCutsets(CutsetGroup cutsets)
        {
            CutsetList c = (CutsetList)cutsets;
            if (Count() == 0) Cutsets = c.Cutsets;
            else
            {
                CutsetList newCutsets = new CutsetList();
                Cutset currentSet, comparedSet, newSet;
                for (int i = 0; i < Cutsets.Count; ++i)
                {
                    currentSet = Cutsets[i];
                    for (int j = 0; j < c.Cutsets.Count; ++j)
                    {
                        comparedSet = c.Cutsets[j];
                        newSet = new Cutset();
                        newSet.AddEvents(currentSet.Events);
                        newSet.AddEvents(comparedSet.Events);
                        newCutsets.AddCutset(newSet);
                    }
                }
                Cutsets = newCutsets.Cutsets;
            }
        }

        /// <summary>
        /// Replace ModuleEvents with their contained Events
        /// </summary>
        public override void ExpandModules()
        {
            for (int i = 0; i < Cutsets.Count; ++i)
            {
                if (Cutsets[i].ContainsModule())
                {
                    CutsetGroup expandedSets = Cutsets[i].ExpandModules();
                    Cutsets.RemoveAt(i--);
                    AddCutsets(expandedSets, false);
                }
            }
        }
    }
}
