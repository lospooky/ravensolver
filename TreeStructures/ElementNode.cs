using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
    public abstract class ElementNode : Node
    {
        #region Node Members
        public override string ToString(StopAt stopAt) {
            return AttributesToString(stopAt);
        }
        #endregion
    }
   
}
