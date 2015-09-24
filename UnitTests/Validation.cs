using FaultTreeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace UnitTests
{
    public class Validation
    {
        public static void AnalyseModel(string modelPath, bool passCriteria)
        {
            Optimisations.Parallelise = true;
            Optimisations.Modularise = true;
            Optimisations.Contract = true;
            Optimisations.BinaryKey = true;
            Optimisations.Catalog = true;
            int threads = 8;

            Optimisations.ParallelOptions.MaxDegreeOfParallelism = threads;
            ThreadPool.SetMaxThreads(threads, threads);


            Stopwatch analysisTimer = new Stopwatch();
            Model model = new Model(modelPath + "/FaultTrees.xml");
            analysisTimer.Start();
            model.Analyse();
            analysisTimer.Stop();
            Console.WriteLine("Analysis Time: " + analysisTimer.Elapsed.ToString());
            ValidateModel(model, modelPath, passCriteria);
        }

        public static void ValidateModel(Model model, string modelPath, bool passCriteria)
        {
            foreach (FaultTree t in model.FaultTrees)
            {
                ValidateTree(t, modelPath, passCriteria);
            }
        }

        public static void ValidateTree(FaultTree tree, string modelPath, bool passCriteria)
        {
            XmlDocument treeDoc = new XmlDocument();
            treeDoc.Load(modelPath + "/CutSets(" + tree.ID + ").xml");
            XmlElement treesElement = treeDoc["HiP-HOPS_Results"]["FaultTrees"];
            XmlElement treeElement = treesElement["FaultTree"];
            XmlElement cutsetsSummary = treeElement["CutSetsSummary"];

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Validating Tree:" + tree.ID.ToString());
            
            bool validCount = CountCutsets(tree, cutsetsSummary);
            bool validTree = ValidateCutsets(tree, treeElement["AllCutSets"]);
            bool valid = validCount && validTree;
            Assert.AreEqual(passCriteria, valid);
        }

        public static bool CountCutsets(FaultTree tree, XmlElement cutsetSummary)
        {
            if(cutsetSummary == null)
            {
                Console.WriteLine("Count - No XML data");
                return true;
            }
            bool valid = true;
            int childOrder, maxOrder = 0;
            foreach (XmlElement child in cutsetSummary.ChildNodes)
            {
                childOrder = int.Parse(child.GetAttribute("order"));
                if (childOrder > maxOrder) maxOrder = childOrder;
            }
            int[] orderCount = new int[maxOrder + 1];
            int[] validCount = new int[maxOrder];
            List<Cutset> cutsets;
            if (Optimisations.Catalog)
                cutsets = ((Catalog)tree.RootNode.Cutsets).GetCutsetList();
            else cutsets = ((CutsetList)tree.RootNode.Cutsets).Cutsets;

            int setOrder;
            for (int i = 0; i < cutsets.Count; ++i)
            {
                setOrder = 0;
                for (int j = 0; j < cutsets[i].Events.Count; ++j )
                {
                    if (cutsets[i].Events[j] is BasicEvent)
                        ++setOrder;
                }

                if (setOrder > maxOrder)
                    orderCount[orderCount.Length - 1]++;
                else
                    orderCount[setOrder - 1]++;
            }

            int order;
            foreach (XmlElement e in cutsetSummary.ChildNodes)
            {
                order = int.Parse(e.GetAttribute("order"));
                validCount[order - 1] = int.Parse(e.InnerText);
                Console.WriteLine("Order " + order.ToString() + ": " + orderCount[order - 1].ToString() + "(" + validCount[order - 1].ToString() + ")");
                if (orderCount[order - 1] != validCount[order - 1])
                    valid = false;
            }

            if (orderCount[orderCount.Length - 1] > 0) valid = false;

            if (valid) Console.WriteLine("Count - OK");
            else Console.WriteLine("Count - INVALID");
            return valid;
        }

        public static bool ValidateCutsets(FaultTree tree, XmlElement cutsetsElement)
        {
            bool valid = true;

            if (cutsetsElement != null)
            {
                Cutset testSet;
                foreach (XmlElement cutsets in cutsetsElement)
                {
                    foreach (XmlElement cutset in cutsets)
                    {
                        testSet = new Cutset();
                        foreach (XmlElement e in cutset["Events"])
                        {
                            testSet.AddEvent((Event)tree.Model.GetNode(int.Parse(e.GetAttribute("ID"))));
                        }
                        if (tree.RootNode.Cutsets.ContainsSet(testSet))
                            continue;
                        else valid = false;
                    }
                }
                if (valid) Console.WriteLine("Cutsets - OK");
                else Console.WriteLine("Cutsets - INVALID");
            }
            else Console.WriteLine("Cutsets - No XML data");

            return valid;
        }
    }
}
