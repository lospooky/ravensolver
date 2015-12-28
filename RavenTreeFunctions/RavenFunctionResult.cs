using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenTreeFunctions
{
    public class RavenFunctionResult
    {
        public int elementsAdded { get; set; }
        public int attributeConnectionsMade { get; set; }
        public int modelObjectConnectiosMade { get; set; }
        public bool succeeded { get; set; }
    }
}
