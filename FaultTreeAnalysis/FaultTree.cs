using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// System FT
    /// </summary>
    public class FaultTree
    {
        private int visitCounter = 0;

        public FaultTree(XmlElement faultTreeElement, Model model)
        {
            Model = model;
            Timer = new Stopwatch();
            ParseXML(faultTreeElement);
            if (Optimisations.Modularise)
            {
                VisitNodes();
                IdentifyModules();
            }
        }

        public Gate RootNode { get; set; }
        public Model Model { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Stopwatch Timer { get; set; }

        /// <summary>
        /// Read FT into memory
        /// </summary>
        /// <param name="faultTreeElement">FaultTree XML element</param>
        private void ParseXML(XmlElement faultTreeElement)
        {
            ID = int.Parse(faultTreeElement.GetAttribute("ID"));
            Name = faultTreeElement["Name"].InnerText;
            Description = faultTreeElement["Description"].InnerText;
            XmlNode outputDeviationNode = faultTreeElement["OutputDeviation"];

            // Start recursive parsing of tree.
            ParseChildren(outputDeviationNode["Children"], null);
        }

        /// <summary>
        /// Reads in XML nodes from children element
        /// </summary>
        /// <param name="children">XML children element containing child nodes</param>
        /// <param name="parent">Parent to add child nodes to</param>
        private void ParseChildren(XmlElement children, Gate parent)
        {
            foreach(XmlElement child in children)
            {
                Node childNode = null;
                switch(child.Name)
                {
                    case "Or":
                        childNode = ParseOrGateNode(child);
                        break;
                    case "And":
                        childNode = ParseAndGateNode(child);
                        break;
                    case "Event":
                        childNode = ParseEventNode(child);
                        break;
                    case "InputDeviation":
                    case "OutputDeviation":
                        // If contracting, skip to children and ignore this node
                        if (Optimisations.Contract)
                        {
                            ParseChildren(child["Children"], parent);
                        }
                        else childNode = ParseDeviationNode(child);
                        break;
                    case "PCCF":
                        childNode = ParseDeviationNode(child);
                        break;
                }
                


                // First node (root) has no parent, contracted(removed) nodes are set to null
                if (parent == null && RootNode == null)
                {
                    RootNode = (Gate)childNode;
                    // Set root node property to prevent modularising it
                    RootNode.IsRoot = true;
                }
                else if (childNode != null)
                {
                    if (Optimisations.Contract && childNode is Gate)
                    {
                        // Contract nodes with a single child
                        // .. else contract repeated AND/OR gates
                        // .. else add as normal
                        if (((Gate)childNode).Children.Count == 1)
                            parent.Children.Add(((Gate)childNode).Children[0]);
                        else if (((Gate)childNode).Logic == parent.Logic)
                        {
                            parent.Children.AddRange(((Gate)childNode).Children);
                        }
                        else parent.Children.Add(childNode);
                    }
                    else parent.Children.Add(childNode);
                }
            }
        }

        /// <summary>
        /// Retrieve existing or create new AND node
        /// </summary>
        /// <param name="andNode">And's XML element</param>
        /// <returns>New or existing ANDGate node</returns>
        private Node ParseAndGateNode(XmlElement andNode)
        {
            int id = int.Parse(andNode.GetAttribute("ID"));
            ANDGate existingNode = (ANDGate) Model.GetNode(id);
            if (existingNode != null)
            {
                return existingNode;
            }
            else
            {
                ANDGate newNode = new ANDGate(id, andNode["Name"].InnerText);
                Model.AddNode(newNode);
                ParseChildren(andNode["Children"], newNode);
                return newNode;
            }

        }

        /// <summary>
        /// Retrieve existing or create new OR node
        /// </summary>
        /// <param name="orNode">Or's XML element</param>
        /// <returns>New or existing ORGate node</returns>
        private Node ParseOrGateNode(XmlElement orNode)
        {
            int id = int.Parse(orNode.GetAttribute("ID"));
            ORGate existingNode = (ORGate)Model.GetNode(id);
            if (existingNode != null)
            {
                return existingNode;
            }
            else
            {
                ORGate newNode = new ORGate(id, orNode["Name"].InnerText);
                Model.AddNode(newNode);
                ParseChildren(orNode["Children"], newNode);
                return newNode;
            }
        }

        /// <summary>
        /// Retrieve existing or create new xDeviation node for uncontracted trees
        /// Treated as a gate node with no logic
        /// </summary>
        /// <param name="deviationNode">XML Deviation Node</param>
        /// <returns>Gate Node</returns>
        private Gate ParseDeviationNode(XmlElement deviationNode)
        {
            int id = int.Parse(deviationNode.GetAttribute("ID"));
            NULLGate existingNode = (NULLGate)Model.GetNode(id);
            if (existingNode != null)
            {
                return existingNode;
            }
            else
            {
                NULLGate newNode = new NULLGate(id, deviationNode["Name"].InnerText);
                Model.AddNode(newNode);
                ParseChildren(deviationNode["Children"], newNode);
                return newNode;
            }
        }

        /// <summary>
        /// Retrieve existing basic event node from model
        /// </summary>
        /// <param name="eventNode">Event's XML element</param>
        /// <returns>BasicEvent node</returns>
        private Node ParseEventNode(XmlElement eventNode)
        {
            Node e = Model.GetNode(int.Parse(eventNode.GetAttribute("ID")));
            return e;
        }

        /// <summary>
        /// Start recursive traversal of tree to set visit properties
        /// </summary>
        private void VisitNodes()
        {
            RootNode.VisitNode(ref visitCounter);
        }

        /// <summary>
        /// Start recursive traversal of tree to identify modules 
        /// </summary>
        private void IdentifyModules()
        {
            RootNode.IdentifyModules();
            RootNode.ResetVisits();
        }

        /// <summary>
        /// Output tree to XML document
        /// </summary>
        /// <param name="document">XML document</param>
        public void OutputXML(XmlDocument document)
        {
            XmlElement tree = document.CreateElement("FaultTree");
            tree.SetAttribute("ID", ID.ToString());
            XmlElement model = document["Model"];

            // Append tree name.
            XmlNode nameNode = document.CreateNode(XmlNodeType.Element, "Name", "");
            nameNode.InnerText = Name;
            tree.AppendChild(nameNode);

            // Append tree description.
            XmlNode descriptionNode = document.CreateNode(XmlNodeType.Element, "Description", "");
            descriptionNode.InnerText = Description;
            tree.AppendChild(descriptionNode);

            // Create output deviation node.
            XmlNode outputDeviationNode = document.CreateNode(XmlNodeType.Element, "OutputDeviation", "");
            // Name it.
            XmlNode outputNameNode = document.CreateNode(XmlNodeType.Element, "Name", "");
            outputNameNode.InnerText = Name;
            outputDeviationNode.AppendChild(outputNameNode);

            // Append children to output deviation.
            XmlNode childrenNode = document.CreateNode(XmlNodeType.Element, "Children", "");
            RootNode.OutputXML(document, childrenNode);
            outputDeviationNode.AppendChild(childrenNode);

            // Append output deviation to tree.
            tree.AppendChild(outputDeviationNode);

            // Append tree to model.
            model.AppendChild(tree);
        }

        /// <summary>
        /// Start recursive cutset generation
        /// </summary>
        public void Analyse()
        {
            Console.Write("Analyzing tree " + ID.ToString() + ".. ");
            Timer.Start();
            RootNode.GenerateCutSets();
            Timer.Stop();
            Console.WriteLine("Complete");
        }

        /// <summary>
        /// Output FT to console
        /// </summary>
        public void PrintTree()
        {
            string cutsetCount;

            if (Optimisations.Catalog)
            {
                List<Cutset> cutsets = ((Catalog)RootNode.Cutsets).GetCutsetList();
                cutsetCount = cutsets.Count.ToString();
            }
            else
            {
                cutsetCount = RootNode.Cutsets.Count().ToString();
            }
            Console.WriteLine("{0,-15}{1,-20}{2,-15}", "Tree:" + ID.ToString(), "Cutsets:" + cutsetCount, "Time:" + Timer.Elapsed.ToString());
        }

    }
}
