using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    public static class Optimisations
    {
        public const bool ParalleliseRedundancyCheck = false;
        public const bool ParalleliseBranching = true;

        public static bool Contract = false;
        public static bool Modularise = false;
        public static bool Catalog = false;
        public static bool BinaryKey = false;
        public static bool Parallelise = false;

        public static ParallelOptions ParallelOptions = new ParallelOptions();
    }

    public class ThreadInfo
    {
        public int ChildBranch { get; set; }
    }
}
