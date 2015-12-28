using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
    public class ElementModelNode : ElementNode
    {
        private bool isCommon = false;


        public bool IsCommon
        {
            get { return isCommon; }
            set { isCommon = value; }
        }
        

        #region Node Members
        public override string ToString(StopAt stopAt) {
            String result = "ElementModel: " + base.ToString(stopAt);
            return result;
        }
        #endregion
    }
            
    
}
