using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    public class ModuleEvent : Event
    {
        public ModuleEvent(int id, CutsetGroup cutsets)
        {
            ID = id;
            base.SetKeyBit();
            Cutsets = cutsets;
        }

        public override CutsetGroup GetCutSets()
        {
            return Cutsets;
        }
    }
}
