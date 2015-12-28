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
    public class RFGlobalTranslation : RavenFunction
    {
        protected List<Point> firstRowTranslations = null;

        #region RavenFunction Members

        public RFGlobalTranslation(Tree tree, SearchDirection allowedDirections, Boolean testMode) : base(tree, allowedDirections, testMode) {
            processableAttributes = new List<Tree.ProcessableAttributes> {
                Tree.ProcessableAttributes.AbsolutePositionInstances
            };
        }


        public override Object ProcessAttributeList(List<Object> attributeValues, int outerIterator) {
            Object preProcessedValue = PreProcessAttributeList(attributeValues);
            if (!preProcessedValue.Equals(false)) {
                return preProcessedValue;
            }

            //Logging.logInfo("RFGlobalTranslation (row " + outerIterator + ") on: ");
            //foreach (Object o in attributeValues)
            //    Logging.logInfo(" " + (o == null ? "null" : o.ToString()));

            foreach (Object value in attributeValues)
                if (value == null)
                    return null;

            if (attributeValues.Count < 2)
                return null;
            else if (!(attributeValues[0] is AbsoluteInstancePosition) || !(attributeValues[1] is AbsoluteInstancePosition)) {
                throw new ArgumentException("RFTranslation can only handle AbsolutePositionNodes");
            }
            Point? firstValue = ((AbsoluteInstancePosition) attributeValues[1]).TranslationOf((AbsoluteInstancePosition)attributeValues[0]);
            if (firstValue == null || (firstValue.Value.X == 0 && firstValue.Value.Y == 0))
                return null;

            if (outerIterator == 1)
                firstRowTranslations = new List<Point>(attributeValues.Count - 1);

            for (int i=1; i<attributeValues.Count; i++) {
                if (attributeValues[i] == null)
                    return null;
                Point? newTrans = ((AbsoluteInstancePosition)attributeValues[i]).TranslationOf((AbsoluteInstancePosition)attributeValues[i - 1]);
                if (newTrans == null)
                    return null;

                if (outerIterator == 1) {
                    if (firstRowTranslations.Count == i - 1)
                        firstRowTranslations.Add((Point)newTrans);
                    else
                        firstRowTranslations[i - 1] = (Point)newTrans;
                }
                else {
                    //TODO: maybe rewrite to horiz/vertical processing
                    if (!firstRowTranslations[i - 1].Equals((Point)newTrans))
                        return null;
                }
            }

            if (attributeValues.Count - 1 == firstRowTranslations.Count) {
                //Logging.logInfo("Return true:");
                //foreach (Point p in firstRowTranslations)
                //    Logging.logInfo(" " + p.ToString());
                return true;
            }
            else if (attributeValues.Count == firstRowTranslations.Count) {
                //One to predict
                //Logging.logInfo("Translation " + firstRowTranslations[firstRowTranslations.Count - 1] + " found!!");

                Point newPos = ((AbsoluteInstancePosition)attributeValues[attributeValues.Count - 1]).AbsolutePosition;
                newPos.Offset(firstRowTranslations[firstRowTranslations.Count - 1].X, firstRowTranslations[firstRowTranslations.Count - 1].Y);

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
            else {
                return null;
            }
        }


        #endregion
    }
}
