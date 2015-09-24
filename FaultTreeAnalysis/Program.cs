using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FaultTreeAnalysis
{
    public class Program
    {
        static Model model;
        static string inputXML;
        static string outputXML = "output.xml";
        static void Main(string[] args)
        {
            // Parse args
            for (var i = 0; i < args.Length; ++i)
            {
                switch(args[i].ToLower())
                {
                    case "-i":
                    case "-input":
                        inputXML = args[i + 1];
                        break;
                    case "-o":
                    case "-output":
                        outputXML = args[i + 1];
                        break;
                    case "-c":
                    case "-contract":
                    case "-cn":
                        Optimisations.Contract = true;
                        break;
                    case "-p":
                    case "-parallel":
                        Optimisations.Parallelise = true;
                        break;
                    case "-m":
                    case "-modularise":
                        Optimisations.Modularise = true;
                        break;
                    case "-catalog":
                    case "-ct":
                        Optimisations.Catalog = true;
                        break;
                    case "-bitcompare":
                    case "-bk":
                    case "-binarykey":
                        Optimisations.BinaryKey = true;
                        break;
                    case "-threads":
                    case "-t":
                        int threads = int.Parse(args[i + 1]);
                        Optimisations.ParallelOptions.MaxDegreeOfParallelism = threads;
                        ThreadPool.SetMaxThreads(threads, threads);
                        break;
                }
            }

            if (inputXML != null)
            {
                Stopwatch analysisTimer = new Stopwatch();
                model = new Model(inputXML);
                // Start analysis
                analysisTimer.Start();
                model.Analyse();
                analysisTimer.Stop();
                // Finish analysis
                model.OutputXML(outputXML);
                model.PrintTrees();
                Console.WriteLine("Total Analysis Time: " + analysisTimer.Elapsed);
                Console.WriteLine("Cut Set Comparisons: " + CutsetGroup.ComparisonCounter.ToString());
                Console.WriteLine("Finished");
            }
            else Console.WriteLine("Usage:\n\n\tFaultTreeAnalysis.exe [-i input.xml]\n\nOptional Parameters:\n\n\t[-o output.xml]\n\t[-cn | -contract]\n\t[-p | -parallel]\n\t[-m | -modularise]\n\t[-ct | -catalog]\n\t[-bk | -binarykey]\n\t[-threads n]\n");
            Console.WriteLine("Press ENTER");
            Console.ReadLine();
        }

        
    }
}
