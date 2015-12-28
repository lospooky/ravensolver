using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeStructures;
using Utilities;
using System.Collections;

namespace RavenTreeFunctions
{
    public class RFNumericProgression : RavenFunction
    {
        #region IRavenFunction Members

        private List<int> numericIncrement = new List<int>(3);

        public RFNumericProgression(Tree tree, SearchDirection allowedDirections, Boolean testMode)
            : base(tree, allowedDirections, testMode) {
            processableAttributes = new List<Tree.ProcessableAttributes>();
            processableAttributes.Add(Tree.ProcessableAttributes.ObjectsCount);
//            processableAttributes.Add(Tree.ProcessableAttributes.AbsolutePositionInstances);
        }


        public override Object ProcessAttributeList(List<Object> attributeValues, int outerIterator) {
            Object preProcessedValue = PreProcessAttributeList(attributeValues);
            if (!preProcessedValue.Equals(false)) {
                return preProcessedValue;
            }

            bool innerList = false;
            List<int> listCount;

            if (attributeValues[0] is List<Object>) {
                innerList = true;
                listCount = (from av in attributeValues select ((List<Object>)av).Count).ToList();
            }
            else if (attributeValues[0] is int) {
                listCount = (from av in attributeValues select (int)av).ToList();
            }
            else {
                throw new ArgumentException("RFNumericProgression only accepts list of int or list of list of objects as argument.");
            }

            if (listCount.Count >= 3) {
                for (int i = 0; i < listCount.Count - 2; i++) {
                    if (listCount[i] == listCount[i + 1] || listCount[i + 1] - listCount[i] != listCount[i + 1] - listCount[i])
                        return null;
                }
                for (int i = numericIncrement.Count - 1; i > outerIterator; i--)
                    numericIncrement.RemoveAt(i);

                numericIncrement.Add(listCount[1] - listCount[0]);

                for (int i = 0; i<numericIncrement.Count-2; i++) {
                    if (numericIncrement[i+2] - numericIncrement[i+1] != numericIncrement[i+1] - numericIncrement[i]) {
                        numericIncrement.RemoveAt(numericIncrement.Count-1);
                        return null;
                    }
                }

                return listCount[listCount.Count-1] + listCount[1] - listCount[0];
            }
            else {
                if (listCount[0] == listCount[1])
                    return null;

                numericIncrement.Add(listCount[1] - listCount[0]);

                for (int i = 0; i < numericIncrement.Count - 2; i++) {
                    if (numericIncrement[i + 2] - numericIncrement[i + 1] != numericIncrement[i + 1] - numericIncrement[i]) {
                        numericIncrement.RemoveAt(numericIncrement.Count - 1);
                        return null;
                    }
                }
                if (!innerList)
                    return listCount[listCount.Count - 1] + listCount[1] - listCount[0];
                else
                    throw new NotImplementedException("List of list is pending implementation :)");
            }
            
        }


        public override RavenFunctionResult RunAlgorithmIndependently(Tree.ProcessableAttributes soughtAttribute) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
