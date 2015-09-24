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
    /// FT Node
    /// </summary>
    public class Node
    {
        public CutsetGroup Cutsets { get; set; }

        protected readonly object locker = new object();

        public int ID { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Description { get; set; }

        public int FirstVisit { get; set; }

        public int LastVisit { get; set; }

        public bool IsRoot { get; set; }

        public virtual void OutputXML(XmlDocument document, XmlNode parent) { }

        public virtual CutsetGroup GetCutSets() { return null; }

        public virtual void GenerateCutSets() { return; }

        public virtual void IdentifyModules() { return; }

        public virtual int FindMinimumVisit() { return 0; }

        public virtual int FindMaximumVisit() { return 0; }

        public virtual void VisitNode(ref int counter)
        {
            ++counter;
            if (FirstVisit == 0) FirstVisit = counter;
            LastVisit = counter;
        }

        /// <summary>
        /// Reset Node Visit members to 0
        /// </summary>
        public virtual void ResetVisits()
        {
            // Nodes visit info should be reset if they are present in multiple trees
            FirstVisit = 0;
            LastVisit = 0;
        }
    }

    
}
