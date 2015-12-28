using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeStructures;
using Utilities;
using System.Collections;
using System.Windows;

namespace RavenTreeFunctions
{
    public class RFRowBasedTranslation : RavenFunction
    {
        #region RavenFunction Members

        public RFRowBasedTranslation(Tree tree, SearchDirection allowedDirections, Boolean testMode) : base(tree, allowedDirections, testMode) {
            processableAttributes = new List<Tree.ProcessableAttributes> {
                Tree.ProcessableAttributes.AbsolutePositionInstances
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


            if (attributeValues.Count < 2)
                return null;
            else if (!(attributeValues[0] is AbsoluteInstancePosition) || !(attributeValues[1] is AbsoluteInstancePosition)) {
                if (attributeValues[0] == null || attributeValues[1] == null)
                    return null;
                throw new ArgumentException("RFTranslation can only handle AbsolutePositionNodes");
            }

            Point? firstValue = ((AbsoluteInstancePosition) attributeValues[1]).TranslationOf((AbsoluteInstancePosition)attributeValues[0]);
            if (firstValue == null || (firstValue.Value.X == 0 && firstValue.Value.Y == 0))
                return null;

            for (int i=2; i<attributeValues.Count; i++) {
                Point? newTrans = ((AbsoluteInstancePosition)attributeValues[i]).TranslationOf((AbsoluteInstancePosition)attributeValues[i - 1]);
                if (newTrans == null)
                    return null;

                if (!newTrans.Equals(firstValue))
                    return null;
            }
            Logging.logInfo("Translation " + firstValue + " found");

            Point newPos = ((AbsoluteInstancePosition)attributeValues[attributeValues.Count - 1]).AbsolutePosition;
            newPos.Offset(firstValue.Value.X, firstValue.Value.Y);

            InstanceNode instance = ((AbsoluteInstancePosition)attributeValues[attributeValues.Count - 1]).Instance;
            AbsoluteInstancePosition foundAP = null;
            foreach (Node apN in instance.Children) {
                AbsoluteInstancePosition ap = apN as AbsoluteInstancePosition;
                if (ap == null)
                    continue;
                if (ap.AbsolutePosition.Equals(newPos)) {
                    foundAP = ap;
                    break;
                }
            }
            if (foundAP == null) {
                foundAP = new AbsoluteInstancePosition(instance);
                tree.AbsoluteInstancePositions.Add(foundAP);
                foundAP.AddAttributes(tree.AddAttribute(Tree.AttributeNames.CenterX, newPos.X));
                foundAP.AddAttributes(tree.AddAttribute(Tree.AttributeNames.CenterY, newPos.Y));
            }
            return foundAP;
        }


        #endregion
    }
}
