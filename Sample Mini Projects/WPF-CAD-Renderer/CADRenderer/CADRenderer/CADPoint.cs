using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADRenderer
{
    public struct CADPoint
    {
        public string StepNo { get; set; }
        public string StepName { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        public string CoordType { get; set; }

        public string Doodle { get; set; }
    }
}
