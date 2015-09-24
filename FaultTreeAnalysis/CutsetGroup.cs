using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Cutsets Data Structure
    /// </summary>
    public class CutsetGroup
    {
        public static int ComparisonCounter = 0;

        protected readonly object locker = new object();

        /// <summary>
        /// Constructs Catalog or CutsetList based on optimisation used
        /// </summary>
        /// <returns>New CutsetGroup</returns>
        public static CutsetGroup Create()
        {
            if (Optimisations.Catalog)
                return new Catalog();
            else return new CutsetList();
        }
        public static CutsetGroup Create(Cutset firstCutset)
        {
            if (Optimisations.Catalog)
                return new Catalog(firstCutset);
            else return new CutsetList(firstCutset);
        }

        public virtual int Count() { return 0; }

        public virtual void AddCutset(Cutset cutset, bool checkRedundancy = true) { return; }

        public virtual void AddCutsets(CutsetGroup cutsets, bool checkRedundancy = true) { return; }

        public virtual void CombineCutsets(CutsetGroup cutsets) { return; }

        public virtual bool ContainsSet(Cutset cutset) { return false; }

        public virtual void ExpandModules() { return; }


        
    }
}
