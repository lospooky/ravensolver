using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeStructures;
using Utilities;
using System.Collections;

namespace RavenTreeFunctions
{
    public class RFDistributeionOfThree : RavenFunction
    {
        #region IRavenFunction Members

        private List<Object> validObjects;

        public RFDistributeionOfThree(Tree tree, SearchDirection allowedDirections, Boolean testMode)
            : base(tree, allowedDirections, testMode) {
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

            if (outerIterator == 1) {
                validObjects = new List<Object>(attributeValues.Count);
                validObjects.AddRange(attributeValues);

                //Logging.logInfo("Distribution of three on: ");
                //foreach (Object o in validObjects)
                //    Logging.logInfo(" " + (o==null ? "null" : o.ToString()));

                if (validObjects[0] == null && !(validObjects[1] is int)
                    || !(validObjects[0] is int)) {
                    foreach (Object o in validObjects) {
                        if ((from v in validObjects where v==o || (v!=null && v.Equals(o)) select v).Count() != 1)
                            return null;
                    }
                }

                return true;
            }
            else {
                List<Object> temp = new List<Object>(validObjects.Count);
                temp.AddRange(validObjects);


                //Logging.logInfo("Testing: ");
                //foreach (Object o in attributeValues)
                //    Logging.logInfo(" " + (o==null ? "null" : o.ToString()));

                foreach (Object obj in attributeValues) {
                    if (!(obj is List<Object>)) {
                        if (!temp.Contains(obj))
                            return null;
                        temp.Remove(obj);
                    }
                    else if (obj is List<Object>) {
                        bool foundMatch = false;
                        foreach (Object l in (List<Object>)validObjects) {
                            if (((List<Object>)l).Count == ((List<Object>)obj).Count) {
                                foundMatch = true;
                                foreach (Object o in (List<Object>)obj) {
                                    if (!((List<Object>)l).Contains(o)) {
                                        foundMatch = false;
                                        break;
                                    }
                                    foundMatch = true;
                                }
                                if (foundMatch) {
                                    temp.Remove(l);
                                    break;
                                }
                            }
                        }
                        if (!foundMatch)
                            return null;
                    }
                }
                if (temp.Count == 0)
                    return true;
                else if (temp.Count == 1)
                    return temp[0];
                else
                    return null;
            }
            
        }



        #endregion
    }
}
