using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    /// <summary>
    /// Input/Output Deviation Node
    /// </summary>
    public class NULLGate : Gate
    {
        public NULLGate(int id, string name)
        {
            ID = id;
            Name = name;
            Logic = GateLogic.NONE;
        }

        public override CutsetGroup GetCutSets()
        {
            if (Cutsets == null)
                GenerateCutSets();
            return Cutsets;
        }

        public override void GenerateCutSets()
        {
            Cutsets = Children[0].GetCutSets();
        }
    }
}
