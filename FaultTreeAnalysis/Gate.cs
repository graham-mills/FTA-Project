using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// FT non-terminating node
    /// </summary>
    public class Gate : Node
    {
        public enum GateLogic
        {
            OR,
            AND,
            NONE
        }

        public List<Node> Children { get; set; }
        public GateLogic Logic { get; set; }
        public bool IsModule { get; set; }
        public int MinimumVisit { get; set; }
        public int MaximumVisit { get; set; }
        public bool SetsGenerated { get; set; }
        public int CompleteBranches { get; set; }

        public Gate()
        {
            Children = new List<Node>();
            IsModule = false;
            SetsGenerated = false;
        }
        
        /// <summary>
        /// Reset children's visit properties
        /// </summary>
        public override void ResetVisits()
        {
            base.ResetVisits();
            foreach (Node n in Children)
                n.ResetVisits();
        }

        public override void OutputXML(XmlDocument document, XmlNode parent)
        {
            // Create gate element
            XmlElement gateElement;
            if (Logic == GateLogic.AND)
                gateElement = document.CreateElement("And");
            else if (Logic == GateLogic.OR)
                gateElement = document.CreateElement("Or");
            else if (Logic == GateLogic.NONE)
                gateElement = document.CreateElement("OutputDeviation");
            else return;
            gateElement.SetAttribute("ID", ID.ToString());

            // Append name node
            XmlNode nameNode = document.CreateNode(XmlNodeType.Element, "Name", "");
            nameNode.InnerText = Name;
            gateElement.AppendChild(nameNode);

            // Append visit info
            XmlNode isModuleNode = document.CreateNode(XmlNodeType.Element, "Module", "");
            isModuleNode.InnerText = IsModule.ToString();
            gateElement.AppendChild(isModuleNode);
            XmlNode firstVisitNode = document.CreateNode(XmlNodeType.Element, "FirstVisit", "");
            firstVisitNode.InnerText = FirstVisit.ToString();
            gateElement.AppendChild(firstVisitNode);
            XmlNode lastVisitNode = document.CreateNode(XmlNodeType.Element, "LastVisit", "");
            lastVisitNode.InnerText = LastVisit.ToString();
            gateElement.AppendChild(lastVisitNode);

            // Append children
            if (Children.Count > 0)
            {
                XmlNode childrenNode = document.CreateNode(XmlNodeType.Element, "Children", "");
                foreach (Node child in Children)
                {
                    child.OutputXML(document, childrenNode);
                }
                gateElement.AppendChild(childrenNode);
            }
            parent.AppendChild(gateElement);
        }

        /// <summary>
        /// Flags gate as Module
        /// </summary>
        public override void IdentifyModules()
        {
            foreach(Node n in Children)
            {
                n.IdentifyModules();
            }

            // If minimum first visit of all children is above this node's first visit
            // .. and last visit of children is lower than this node's last visit
            // .. then node is a module (children are unique to this gate)
            if (FindMinimumVisit() > FirstVisit && FindMaximumVisit() < LastVisit)
                IsModule = true;
            else IsModule = false;
        }

        /// <summary>
        /// Set visit properties
        /// </summary>
        /// <param name="counter"></param>
        public override void VisitNode(ref int counter)
        {
            ++counter;
            if (FirstVisit == 0) FirstVisit = counter;
            foreach (Node n in Children)
                n.VisitNode(ref counter);
            LastVisit = ++counter;
        }

        public override int FindMinimumVisit()
        {
            if (Children.Count > 0)
            {
                int min = Children[0].FindMinimumVisit();
                int current;
                for (var i = 1; i < Children.Count; ++i)
                {
                    current = Children[i].FindMinimumVisit();
                    if (current < min) min = current;
                }
                MinimumVisit = min;
            }
            return MinimumVisit;
        }

        public override int FindMaximumVisit()
        {
            if (Children.Count > 0)
            {
                int max = Children[0].FindMaximumVisit();
                int current;
                for (var i = 1; i < Children.Count; ++i)
                {
                    current = Children[i].FindMaximumVisit();
                    if (current > max) max = current;
                }
                MaximumVisit = max;
            }
            return MaximumVisit;
        }

        /// <summary>
        /// Pack node's Cutsets into ModuleEvent
        /// </summary>
        public void Modularise()
        {
            ModuleEvent moduleEvent = new ModuleEvent(ID, Cutsets);
            Cutset moduleCutset = new Cutset(moduleEvent);
            Cutsets = CutsetGroup.Create(moduleCutset);
        }

        public void ExpandModules()
        {
            Cutsets.ExpandModules();
        }
    }
}
