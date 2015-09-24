using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultTreeAnalysis
{
    public enum ReductionResult
    {
        REDUNDANT,
        NOT_REDUNDANT,
        CAUSES_REDUNDANCY
    }
}
