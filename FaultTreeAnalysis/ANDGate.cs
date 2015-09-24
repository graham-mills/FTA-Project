using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// FT AND Node
    /// </summary>
    public class ANDGate : Gate
    {
        public ANDGate(int id, string name)
        {
            ID = id;
            Name = name;
            Logic = GateLogic.AND;
        }

        /// <summary>
        /// Retrieve existing or generate new Cutsets
        /// </summary>
        /// <returns>Node's Cutsets</returns>
        public override CutsetGroup GetCutSets()
        {
            lock (locker)
            {
                if (!SetsGenerated)
                    GenerateCutSets();
                return Cutsets;
            }
        }

        /// <summary>
        /// Generate Cutsets from child nodes
        /// </summary>
        public override void GenerateCutSets()
        {
            if (Optimisations.Parallelise && Optimisations.ParalleliseBranching)
            {
                GenerateCutsetsParallel();
            }
            else GenerateCutsetsSerial();

            if (IsModule)
            {
                // Root node is always module, need to expand all contained modules 
                if (IsRoot)
                    base.ExpandModules();
                else base.Modularise();
            }
            // Flag cutsets generated
            SetsGenerated = true;
        }

        /// <summary>
        /// Generates Cutsets concurrently
        /// </summary>
        public void GenerateCutsetsParallel()
        {
            Cutsets = CutsetGroup.Create();

            // Standard method
            Parallel.For(0, Children.Count, Optimisations.ParallelOptions, i =>
            {
                Cutsets.CombineCutsets(Children[i].GetCutSets());
            });

            // ThreadPool branching method
            //int workerThreads, completionThreads;
            //ThreadInfo tInfo;
            //for (int i = 0; i < Children.Count; ++i)
            //{
            //    ThreadPool.GetAvailableThreads(out workerThreads, out completionThreads);
            //    if (workerThreads > 0)
            //    {
            //        tInfo = new ThreadInfo();
            //        tInfo.ChildBranch = i;
            //        ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadCallBack), tInfo);
            //    }
            //    else
            //    {
            //        Cutsets.CombineCutsets(Children[i].GetCutSets());
            //        lock (this)
            //        {
            //            ++CompleteBranches;
            //        }
            //    }
            //}
            //// Wait for branches to finish generation
            //bool complete = false;
            //while (!complete)
            //{
            //    if (CompleteBranches == Children.Count)
            //        complete = true;
            //}
        }

        /// <summary>
        /// Generates Cutsets serially
        /// </summary>
        public void GenerateCutsetsSerial()
        {
            Cutsets = CutsetGroup.Create();

            for (int i = 0; i < Children.Count; ++i)
            {
                // Multiply (AND) child cutsets
                Cutsets.CombineCutsets(Children[i].GetCutSets());
            }
        }

        /// <summary>
        /// Single thread cutset generation. Used for ThreadPool branching method.
        /// </summary>
        /// <param name="t">ThreadInfo</param>
        public void ThreadCallBack(object t)
        {
            int childBranch = ((ThreadInfo)t).ChildBranch;
            Cutsets.CombineCutsets(Children[childBranch].GetCutSets());
            lock (this)
            {
                ++CompleteBranches;
            }
        }
        
    }
}
