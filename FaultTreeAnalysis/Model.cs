using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// System Model
    /// </summary>
    public class Model
    {
        public Dictionary<int, Node> Nodes { get; set; }
        public string Name { get; set; }
        public List<FaultTree> FaultTrees { get; set; }

        public Model(string inputXML)
        {
            Event.EventCounter = 0;
            Nodes = new Dictionary<int, Node>();
            FaultTrees = new List<FaultTree>();
            ParseXML(inputXML);
        }

        /// <summary>
        /// Load XML model into memory.
        /// </summary>
        /// <param name="inputXML">XML filepath</param>
        private void ParseXML(string inputXML)
        {
            Console.WriteLine("Parsing file " + inputXML);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(inputXML);
            XmlElement root = xmlDocument["HiP-HOPS_Results"];
            Name = root.GetAttribute("model");
            XmlNode faultTreesNode = root["FaultTrees"];

            foreach (XmlElement e in faultTreesNode)
            {
                switch(e.Name)
                {
                    case "FMEA":
                        ParseXMLEvents(e);
                        break;
                    case "FaultTree":
                        ParseXMLFaultTree(e);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Load model events from XML node
        /// </summary>
        /// <param name="fmea">FMEA XML NODE</param>
        private void ParseXMLEvents(XmlNode fmea)
        {
            if(Optimisations.BinaryKey) CountXMLEvents(fmea);

            foreach(XmlNode component in fmea.ChildNodes)
            {
                foreach(XmlElement failureEvent in component["Events"])
                {
                    Event newEvent;
                    if (failureEvent.Name == "NormalEvent")
                        newEvent = new NormalEvent(failureEvent);
                    else // Basic Events, Circle Events
                        newEvent = new BasicEvent(failureEvent);

                    AddNode(newEvent);
                }
            }
        }

        /// <summary>
        /// Counts total events of model and updates BK key count
        /// </summary>
        /// <param name="fmea"></param>
        private void CountXMLEvents(XmlNode fmea)
        {
            // Find total number of events to set KeyCount for binary key
            int eventCount = 0;
            foreach (XmlNode component in fmea.ChildNodes)
            {
                eventCount += component["Events"].ChildNodes.Count;
            }
            BinaryKey.UpdateKeyCount(eventCount);
        }

        /// <summary>
        /// Load fault tree from XML node
        /// </summary>
        /// <param name="faultTreeElement">XML FaultTree element</param>
        private void ParseXMLFaultTree(XmlElement faultTreeElement)
        {
            FaultTrees.Add(new FaultTree(faultTreeElement, this));
        }

        /// <summary>
        /// Get event from events dictionary
        /// </summary>
        /// <param name="eventID">ID of required event</param>
        /// <returns></returns>
        public Node GetNode(int nodeID)
        {
            if (Nodes.ContainsKey(nodeID))
            {
                return Nodes[nodeID];
            }
            else return null;
        }

        /// <summary>
        /// Add new node to collection
        /// </summary>
        /// <param name="newNode">New Node</param>
        public void AddNode(Node newNode)
        {
            Nodes.Add(newNode.ID, newNode);
        }

        /// <summary>
        /// Output model as XML
        /// </summary>
        /// <param name="filename">Output filename</param>
        public void OutputXML(string filename)
        {
            XmlDocument output = new XmlDocument();
            output.AppendChild(output.CreateElement("Model"));
            output["Model"].SetAttribute("name", Name);
            foreach(FaultTree t in FaultTrees)
            {
                t.OutputXML(output);
            }
            output.Save(filename);
        }

        /// <summary>
        /// Generate cutsets for each tree
        /// </summary>
        public void Analyse()
        {
            foreach (FaultTree t in FaultTrees)
            {
                t.Analyse();
            }
        }

        /// <summary>
        /// Output FT to console
        /// </summary>
        public void PrintTrees()
        {
            foreach (FaultTree t in FaultTrees)
                t.PrintTree();
        }

    }
}
