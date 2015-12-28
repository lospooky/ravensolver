using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
    public class AttributeNode:Node
    {

        protected KeyValuePair<Tree.AttributeNames, Object> attribute;

        #region Constructors
        public AttributeNode(Tree.AttributeNames key, object value)
        {
            attribute = new KeyValuePair<Tree.AttributeNames,object>(key, value);
        }
        #endregion

        #region Properties
        public Tree.AttributeNames Key {
            get {
                return attribute.Key;
            }
        }
        public Object Value {
            get {
                return attribute.Value;
            }
        }
        #endregion

        #region Node Members

        public override String ToString(StopAt stopAt) {
            String result = "";
            if (Value is List<object>) {
                result += Key + " : {";
                foreach (object val in ((List<object>)Value)) {
                    result += val + ", ";
                }
                result = result.Remove(result.Length - 2);
                result += "}";
            }
            else {
                result += Key + " : " + Value;
            }
            return result;

        }
        #endregion

    }
}
