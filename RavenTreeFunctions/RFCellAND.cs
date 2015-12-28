using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeStructures;
using Utilities;
using System.Collections;

namespace RavenTreeFunctions
{
    public class RFCellAND : RavenFunction
    {
        #region RavenFunction Members
        int numCells = -1;

        public RFCellAND(Tree tree, SearchDirection allowedDirections, Boolean testMode) : base(tree, allowedDirections, testMode) {
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

            //Logging.logInfo("RFCellAND (row " + outerIterator + ") on: ");
            //foreach (Object o in attributeValues)
            //    Logging.logInfo(" " + (o == null ? "null" : o.ToString()));

            List<Object> allCellObjects = new List<Object>();

            for (int i=0; i<attributeValues.Count; i++) {
                if (i == 0) {
                    List<Object> cellObjects = (List<Object>)attributeValues[i];
                    foreach (Object cellObject in cellObjects)
                        allCellObjects.Add((AbsoluteInstancePosition)cellObject);
                }
                else if (i != numCells - 1) {
                    List<Object> cellObjects = (List<Object>)attributeValues[i];
                    for (int j=allCellObjects.Count-1; j>=0; j--) {
                        if (!cellObjects.Contains(allCellObjects[j]))
                            allCellObjects.RemoveAt(j);
                    }
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
            //Logging.logInfo("RFCellAND succeeded for row " + outerIterator);
            return true;
        }


        #endregion
    }
}
