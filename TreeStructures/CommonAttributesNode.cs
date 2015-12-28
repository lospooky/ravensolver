using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
    public class CommonAttributesNode:Node
    {

        #region Node Members
        public override string ToString(StopAt stopAt) {
            String result = "CommonAttributes: " + AttributesToString(stopAt);
            return result;
        }
        #endregion
    }
}
