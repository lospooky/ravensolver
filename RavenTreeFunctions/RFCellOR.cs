using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeStructures;
using Utilities;
using System.Collections;

namespace RavenTreeFunctions
{
    public class RFCellOR : RavenFunction
    {
        #region RavenFunction Members
        int numCells = -1;

        public RFCellOR(Tree tree, SearchDirection allowedDirections, Boolean testMode) : base(tree, allowedDirections, testMode) {
            processableAttributes = new List<Tree.ProcessableAttributes> {
                Tree.ProcessableAttributes.CellObjects
            };
            this.AddObjectsAtWill = true; //Bypasses objectcount
        }


        public override Object ProcessAttributeList(List<Object> attributeValues, int outerIterator) {
            Object preProcessedValue = PreProcessAttributeList(attributeValues);
            if (!preProcessedValue.Equals(false)) {
                return preProcessedValue;
            }

            if (outerIterator == 1)
                numCells = attributeValues.Count;

            //Logging.logInfo("RFCellAddition (row " + outerIterator + ") on: ");
            //foreach (Object o in attributeValues)
            //    Logging.logInfo(" " + (o == null ? "null" : o.ToString()));

            List<Object> allCellObjects = new List<Object>();

            for (int i=0; i<attributeValues.Count; i++) {
                if (i != numCells - 1) {
                    List<Object> cellObjects = (List<Object>)attributeValues[i];
                    foreach (Object cellObject in cellObjects)
                        allCellObjects.Add((AbsoluteInstancePosition)cellObject);
                }
                if (i == attributeValues.Count -1) {
                    if (attributeValues.Count == numCells) {
                        if (((List<Object>)attributeValues[i]).Count != allCellObjects.Count)
                            return null;
                        foreach (AbsoluteInstancePosition aip in allCellObjects) {
                            if (!((List<Object>)attributeValues[i]).Contains(aip))
                                return null;
                        }
                    }
                    else {
                        return allCellObjects;
                    }
                }
            }
            //Logging.logInfo("RFCellAddition succeeded for row " + outerIterator);
            return true;
        }


        #endregion
    }
}
