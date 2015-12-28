using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeStructures;
using Utilities;
using System.Collections;

namespace RavenTreeFunctions
{
    public class RFIdentity : RavenFunction
    {
        #region RavenFunction Members

        public RFIdentity(Tree tree, SearchDirection allowedDirections, Boolean testMode) : base(tree, allowedDirections, testMode) {
            processableAttributes = new List<Tree.ProcessableAttributes> {
                Tree.ProcessableAttributes.ObjectsCount,
                Tree.ProcessableAttributes.CellObjects,
                Tree.ProcessableAttributes.AbsolutePositionInstances,
                Tree.ProcessableAttributes.ElementModels
            };
        }


        public override Object ProcessAttributeList(List<Object> attributeValues, int outerIterator) {
            Object preProcessedValue = PreProcessAttributeList(attributeValues);
            if (!preProcessedValue.Equals(false)) {
                return preProcessedValue;
            }

            //Logging.logInfo("RFIdentity (row " + outerIterator + ") on: ");
            //foreach (Object o in attributeValues)
            //    Logging.logInfo(" " + (o==null ? "null" : o.ToString()));
            
            Object firstValue = attributeValues[0];
            if (firstValue == null && outerIterator < 3 && CellNode.CommonElements != null) {
                firstValue = attributeValues[1];
            }
            for (int i=1; i<attributeValues.Count; i++) {
                if (attributeValues[i] == firstValue)
                    continue;

                //if (attributeValues[i] == null && outerIterator < 3 && CellNode.CommonElements != null) {
                //    attributeValues[i] = firstValue;
                //}

                if (firstValue == null || attributeValues[i] == null)
                    return null;
                if (!attributeValues[i].Equals(firstValue)) {
                    return null;
                }
            }
            //Logging.logInfo(firstValue + " found");
            return (firstValue!=null)? firstValue : true;
        }


        #endregion
    }
}
