using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeStructures;
using Utilities;


/* Functions
 * - Identity
 * - Distribution of 3
 * - Add n
 * - A + B (both image and value)
 * - A - B (image and value)
 * - XOR (image)
 */

namespace RavenTreeFunctions
{
    public delegate int GetCellNumberDelegate(int outerIteratorValue, int innerIteratorValue);

    public abstract class RavenFunction
    {
        [Flags]
        public enum SearchDirection {
            None = 0,
            Horizontal = 0x01,
            Vertical = 0x02,
            HorizontalAndVertical = 0x03,
            DiagonalDownRight=0x04,
            DiagonalDownLeft=0x08,
            BothDiagonals=0x0C,
            AllDirections=0x0F,     //All of the above
            Free=0x10               //All local processing - neighbours etc
        };


        /// <summary>
        /// The function works with these attributes. {{A},{B}} means A or B, separately. {{A,B},{C,D}} means A and B or C and D
        /// Needs to be instantiated in function!
        /// </summary>
        public List<Tree.ProcessableAttributes> processableAttributes {get; protected set;}
        public List<Tree.ProcessableAttributes> processIndependentAttributes {get; protected set; }

        protected Tree tree;
        protected SearchDirection allowedDirections;
        protected Boolean testMode;

        public Boolean AddObjectsAtWill {get; set;}

        #region Constructors
        public RavenFunction(Tree tree, SearchDirection allowedDirections, Boolean testMode) {
            this.tree = tree;
            this.allowedDirections = allowedDirections;
            this.testMode = testMode;
        }
        #endregion

        #region Public overridable functions
        /// <summary>
        /// Lets the function itself decide how to parse the tree, based on allowedDirections
        /// </summary>
        /// <returns></returns>
        public virtual RavenFunctionResult RunAlgorithmIndependently(Tree.ProcessableAttributes soughtAttribute) {
            throw new NotImplementedException(this.GetType().ToString() + " cannot be run independently");
        }

        public RavenFunctionResult RunAlgorithm(Tree.ProcessableAttributes soughtAttribute) {
            Logging.logInfo("Trying " + this.GetType().ToString() + " on " + soughtAttribute);

            if (processableAttributes.Contains(soughtAttribute)) {
                if (processIndependentAttributes!=null &&  processIndependentAttributes.Contains(soughtAttribute))
                    return RunAlgorithmIndependently(soughtAttribute);
                else
                    return RunAlgorithmSupervised(soughtAttribute);
            }
            else {
                throw new ArgumentException(soughtAttribute + " can not be processed by " + this.GetType().ToString());
            }
        }

        /// <summary>
        /// Test if the function holds for the list of attributes. Returns next value in the series or null
        /// </summary>
        /// <param name="attributeValues">list of attributes to produce the next value for</param>
        /// <returns>Object Attribute value if function holds (or true if it holds but no value can be predicted), null otherwise</returns>
        public virtual Object ProcessAttributeList(List<Object> attributeValues, int outerIterator) {
            throw new MethodAccessException(this.GetType().ToString() + " has not implemented ProcessAttributeList.");
        }

        #endregion

        #region Protected functions
        /// <summary>
        /// Preprocesses the attributeValues of ProcessAttributeValues.
        /// </summary>
        /// <param name="attributeValues"></param>
        /// <returns>False for values to passed on. All other value should be returned bypassing ProcessAttributeList</returns>
        protected Object PreProcessAttributeList(List<Object> attributeValues) {
            if (attributeValues.Count == 0) {
                return null;
            }
            else {
                return false;
            }
        }
        #endregion


        #region Public functions
        /// <summary>
        /// Runs through the attributes at hand and calls ProcessAttributeList to check the values
        /// </summary>
        /// <returns></returns>
        protected RavenFunctionResult RunAlgorithmSupervised(Tree.ProcessableAttributes soughtAttribute) { 
            //var allAttributes = from l in tree.Leaves select l.AllAttributes;

            String concreteFunctionName = this.GetType().ToString().Substring(this.GetType().ToString().LastIndexOf(".") + 1);

            if (soughtAttribute == Tree.ProcessableAttributes.ObjectsCount) {
                Object functionResult = null;
                GetCellNumberDelegate GetCellNumber;
                   

                foreach (SearchDirection direction in GetSearchDirectionsList(allowedDirections)) {
                    GetCellNumber = GetCellNumberFunction(direction);

                    Logging.logInfo("Looking for " + concreteFunctionName + " in ObjectsCount in direction " + direction);

                    for (int outerIterator = 1; outerIterator <= 3; outerIterator++) {
                        List<Object> attributeValues = new List<Object>();
                        for (int innerIterator = 1; innerIterator <= 3; innerIterator++) {
                            if (innerIterator == 3 && outerIterator == 3)
                                continue;
                            attributeValues.Add(tree.Cells[GetCellNumber(outerIterator, innerIterator)-1].CellObjects.Count);
                        }
                        functionResult = ProcessAttributeList(attributeValues, outerIterator);
                        if (functionResult == null) { //Function failed
                            break;
                        }
                    }

                    if (functionResult == null) {
                        Logging.logInfo("Function " + concreteFunctionName + " was not able to find any new connections for " + soughtAttribute + ".");
                        continue;
                    }

                    //Function succeeded!
                    int resultInt = (int)functionResult;
                    Logging.logInfo("Function " + concreteFunctionName + " succeeded.");
                    Logging.logInfo("Adding " + resultInt + " objects...");
                    CellNode solutionCell = tree.Cells[8];

                    RavenFunctionResult result = new RavenFunctionResult();

                    for (int i = 0; i < resultInt; i++) {
                        AbsoluteInstancePosition newNode = new AbsoluteInstancePosition();
                        solutionCell.AddCellObject(newNode);
                        tree.AbsoluteInstancePositions.Add(newNode);
                    }
                    
                    result.elementsAdded = resultInt;
                    result.attributeConnectionsMade = 3 * resultInt;

                    Logging.logInfo("---");
                    result.succeeded = true;
                    return result;
                }
            }
            else if (soughtAttribute == Tree.ProcessableAttributes.CellObjects) {
                Object functionResult = null;
                GetCellNumberDelegate GetCellNumber;


                foreach (SearchDirection direction in GetSearchDirectionsList(allowedDirections)) {
                    GetCellNumber = GetCellNumberFunction(direction);

                    Logging.logInfo("Looking for " + concreteFunctionName + " in CellObjects in direction " + direction);

                    for (int outerIterator = 1; outerIterator <= 3; outerIterator++) {
                        List<Object> attributeValues = new List<Object>();
                        for (int innerIterator = 1; innerIterator <= 3; innerIterator++) {
                            if (innerIterator == 3 && outerIterator == 3)
                                continue;
                            List<Object> cellObjects = new List<object>(3);
                            foreach (Object o in tree.Cells[GetCellNumber(outerIterator, innerIterator) - 1].CellObjects)
                                cellObjects.Add(o);
                            attributeValues.Add(cellObjects);
                        }
                        functionResult = ProcessAttributeList(attributeValues, outerIterator);
                        if (functionResult == null) { //Function failed
                            break;
                        }
                    }

                    if (functionResult == null) {
                        Logging.logInfo("Function " + concreteFunctionName + " was not able to find any new connections for " + soughtAttribute + ".");
                        continue;
                    }

                    //Function succeeded!
                    List<AbsoluteInstancePosition> result = new List<AbsoluteInstancePosition>(((List<Object>)functionResult).Count);
                    foreach (Object o in (List<Object>)functionResult)
                        result.Add((AbsoluteInstancePosition)o);
                    int resultInt = result.Count;
                    Logging.logInfo("Function " + concreteFunctionName + " succeeded.");
                    Logging.logInfo("Found " + resultInt + " objects...");
                    CellNode solutionCell = tree.Cells[8];


                    if (tree.Cells[8].CellObjects.Count == 0 && AddObjectsAtWill) {
                        for (int f = 0; f < resultInt; f++) {
                            tree.Cells[8].CellObjects.Add(new AbsoluteInstancePosition());
                            tree.AbsoluteInstancePositions.Add(tree.Cells[8].CellObjects[f]);
                        }
                    }

                    RavenFunctionResult rfResult = new RavenFunctionResult();

                    for (int found = result.Count - 1; found >= 0; found--) {
                        //End of direction iterator
                        for (int sco = tree.Cells[8].CellObjects.Count - 1; sco >= 0; sco--) {
                            if (tree.Cells[8].CellObjects[sco] == ((AbsoluteInstancePosition)result[found])) {
                                result.RemoveAt(found);
                                break;
                            }
                            
                            if (tree.Cells[8].CellObjects[sco].Instance == null || AddObjectsAtWill==true && sco == 0) { //not processed
                                if (tree.Cells[8].CellObjects[sco].Instance == null) {
                                    tree.AbsoluteInstancePositions.Remove(tree.Cells[8].CellObjects[sco]);
                                    tree.Cells[8].RemoveCellObject(tree.Cells[8].CellObjects[sco]);
                                }
                                tree.Cells[8].AddCellObject((AbsoluteInstancePosition)result[found]);
                                Logging.logInfo("AbsolutePosition added " + result[found].ToString());
                                result.RemoveAt(found);
                                rfResult.elementsAdded += 1;
                                break;
                            }
                            else if (sco == 0) {
                                Logging.logError("More Instances found than could be added! Instance left were:");
                                foreach (Object foundObject in result)
                                    Logging.logInfo(((AbsoluteInstancePosition)foundObject).ToString());
                            }
                        }
                    }

                    Logging.logInfo("---");
                    rfResult.succeeded = true;
                    return rfResult;
                }
            }
            else if (soughtAttribute == Tree.ProcessableAttributes.AbsolutePositionInstances) {
                Object functionResult = null;
                GetCellNumberDelegate GetCellNumber;
                RavenFunctionResult result = new RavenFunctionResult();
                int cell1Iterator, cell2Iterator, cell3Iterator;


                foreach (SearchDirection direction in GetSearchDirectionsList(allowedDirections)) {
                    GetCellNumber = GetCellNumberFunction(direction);
                    int cell1IteratorMax=0;
                    int cell2IteratorMax=0;
                    int cell3IteratorMax=0;


                    List<int> cell1IteratorLast = new List<int> { -1, -1, -1 };
                    List<int> cell2IteratorLast = new List<int> { -1, -1, -1 };
                    List<int> cell3IteratorLast = new List<int> { -1, -1, -1 };

                    List<Object> foundObjects = new List<object>();

                    Logging.logInfo("Looking for " + concreteFunctionName + " in AbsolutePostion.Instance in direction " + direction);


                    for (int outerIterator = 1; outerIterator <= 3; outerIterator++) {
                        List<Object> attributeValues = new List<Object>();

                        cell1Iterator = (tree.Cells[GetCellNumber(outerIterator, 1) - 1].CellObjects.Count == 0) ? -1 : 0; // zero objets => null is added, same for other cells
                        //                            cell1Iterator = -1;
                        if (tree.Cells[GetCellNumber(outerIterator, 1) - 1].CellObjects.Count < tree.AverageCellObjectsNumber)
                            cell1Iterator = -1;
                        cell1Iterator = Math.Max(cell1Iterator, cell1IteratorLast[outerIterator - 1]);
                        //                            cell1IteratorMax = (outerIterator == 1) ? firstCellIterator + 1 : tree.Cells[GetCellNumber(outerIterator, 1) - 1].CellObjects.Count;
                        cell1IteratorMax = tree.Cells[GetCellNumber(outerIterator, 1) - 1].CellObjects.Count;


                        for (; cell1Iterator < cell1IteratorMax; cell1Iterator++) {
                            if (cell1Iterator == -1)
                                attributeValues.Add(null);
                            else
                                attributeValues.Add(tree.Cells[GetCellNumber(outerIterator, 1) - 1].CellObjects[cell1Iterator]);
                            cell1IteratorLast[outerIterator - 1] = cell1Iterator;

                            cell2Iterator = (tree.Cells[GetCellNumber(outerIterator, 2) - 1].CellObjects.Count == 0) ? -1 : 0;
                            //cell2Iterator = (cell1Iterator == -1) ? 0 : -1;
                            if (cell1Iterator!=-1 && tree.Cells[GetCellNumber(outerIterator, 2) - 1].CellObjects.Count < tree.AverageCellObjectsNumber)
                                cell2Iterator = -1;
                            //if (cell2Iterator == 0 && (tree.Cells[GetCellNumber(outerIterator, 2) - 1].CellObjects.Count == 0))
                            //    continue;

                            cell2Iterator = Math.Max(cell2Iterator, cell2IteratorLast[outerIterator - 1]);
                            cell2IteratorMax = tree.Cells[GetCellNumber(outerIterator, 2) - 1].CellObjects.Count;

                            for (; cell2Iterator < cell2IteratorMax; cell2Iterator++) {
                                if (cell2Iterator == -1)
                                    attributeValues.Add(null);
                                else
                                    attributeValues.Add(tree.Cells[GetCellNumber(outerIterator, 2) - 1].CellObjects[cell2Iterator]);
                                cell2IteratorLast[outerIterator - 1] = cell2Iterator;

                                if (outerIterator != 3) {

                                    cell3IteratorMax = tree.Cells[GetCellNumber(outerIterator, 3) - 1].CellObjects.Count;
                                    cell3Iterator = (tree.Cells[GetCellNumber(outerIterator, 3) - 1].CellObjects.Count == 0) ? -1 : 0;
                                    //cell3Iterator = (cell1Iterator == -1 || cell2Iterator == -1) ? 0 : -1;
                                    //if (cell3Iterator == 0 && (tree.Cells[GetCellNumber(outerIterator, 3) - 1].CellObjects.Count == 0))
                                    //    continue;

                                    if (cell2Iterator != -1 && tree.Cells[GetCellNumber(outerIterator, 3) - 1].CellObjects.Count < tree.AverageCellObjectsNumber)
                                        cell3Iterator = -1;

                                    cell3Iterator = Math.Max(cell3Iterator, cell3IteratorLast[outerIterator - 1]);

                                    for (; cell3Iterator < cell3IteratorMax; cell3Iterator++) {
                                        if (cell3Iterator == -1)
                                            attributeValues.Add(null);
                                        else
                                            attributeValues.Add(tree.Cells[GetCellNumber(outerIterator, 3) - 1].CellObjects[cell3Iterator]);

                                        cell3IteratorLast[outerIterator - 1] = cell3Iterator;

                                        functionResult = ProcessAttributeList(attributeValues, outerIterator);

                                        attributeValues.RemoveAt(2);
                                        if (functionResult != null)
                                            break;

                                        //cell3IteratorLast[outerIterator - 1] = -1;
                                    }
                                    if (cell3Iterator == cell3IteratorMax)
                                        cell3IteratorLast[outerIterator - 1] = -1;
                                }
                                else {
                                    functionResult = ProcessAttributeList(attributeValues, outerIterator);
                                }
                                attributeValues.RemoveAt(1);

                                if (functionResult != null)
                                    break;

                                //cell2IteratorLast[outerIterator - 1] = -1;
                            }
                            if (cell2Iterator == cell2IteratorMax)
                                cell2IteratorLast[outerIterator - 1] = -1;


                            if (outerIterator == 3 && functionResult != null) {
                                //New thingie found! Add it!
                                foundObjects.Add(functionResult);
                                functionResult = null;
                                attributeValues.RemoveAt(0);
                                cell2IteratorLast[2] = -1;
                                continue;
                            }
                            attributeValues.RemoveAt(0);
                            if (functionResult != null)
                                break;

                            //cell1IteratorLast[outerIterator - 1] = -1;
                        }
                        if (cell1Iterator == cell1IteratorMax)
                            cell1IteratorLast[outerIterator - 1] = -1;

                        if (functionResult == null) {
                            //No match found or last one
                            //Check if all options are depleted

                            //oi = outerIterator
                            int oi;

                            for (oi = outerIterator; oi >= 1; oi--) {
                                if (cell3IteratorLast[oi - 1] != -1 && cell3IteratorLast[oi - 1] < cell3IteratorMax) {
                                    cell3IteratorLast[oi - 1]++;
                                    break;
                                }
                                if (cell2IteratorLast[oi - 1] != -1 && cell2IteratorLast[oi - 1] < cell2IteratorMax) {
                                    cell2IteratorLast[oi - 1]++;
                                    cell3IteratorLast[oi - 1] = -1;
                                    break;
                                }
                                if (cell1IteratorLast[oi - 1] != -1 && cell1IteratorLast[oi - 1] < cell1IteratorMax) {
                                    cell1IteratorLast[oi - 1]++;
                                    cell2IteratorLast[oi - 1] = -1;
                                    cell3IteratorLast[oi - 1] = -1;
                                    break;
                                }
                            }
                            if (oi >= 1) {
                                outerIterator = oi - 1;
                                for (int oi2 = oi + 1; oi2 <= 3; oi2++) {
                                    cell1IteratorLast[oi2 - 1] = -1;
                                    cell2IteratorLast[oi2 - 1] = -1;
                                    cell3IteratorLast[oi2 - 1] = -1;
                                }
                                continue;
                            }
                            break;
                        }
                    }

                    for (int found = foundObjects.Count-1; found>=0; found--) {
                        result.succeeded = true;
                        //End of direction iterator
                        if (tree.Cells[8].CellObjects.Count == 0 && AddObjectsAtWill) {
                            for (int f = 0; f < foundObjects.Distinct<Object>().Count(); f++) {
                                tree.Cells[8].CellObjects.Add(new AbsoluteInstancePosition());
                                tree.AbsoluteInstancePositions.Add(tree.Cells[8].CellObjects[f]);
                            }
                        }
                        for (int sco = tree.Cells[8].CellObjects.Count - 1; sco >= 0; sco--) {
                            if (tree.Cells[8].CellObjects[sco] == ((AbsoluteInstancePosition)foundObjects[found])) {
                                foundObjects.RemoveAt(found);
                                break;
                            }
                            if (tree.Cells[8].CellObjects[sco].Instance == null || AddObjectsAtWill==true && sco == 0) { //not processed
                                if (tree.Cells[8].CellObjects[sco].Instance == null) {
                                    tree.AbsoluteInstancePositions.Remove(tree.Cells[8].CellObjects[sco]);
                                    tree.Cells[8].RemoveCellObject(tree.Cells[8].CellObjects[sco]);
                                }
                                tree.Cells[8].AddCellObject((AbsoluteInstancePosition)foundObjects[found]);
                                Logging.logInfo("AbsolutePosition added " + foundObjects[found].ToString());
                                foundObjects.RemoveAt(found);
                                result.elementsAdded += 1;
                                break;
                            }
                            else if (sco == 0) {
                                Logging.logError("More Instances found than could be added! Instance left were:");
                                result.succeeded = false;
                                foreach (Object foundObject in foundObjects)
                                    Logging.logInfo(((AbsoluteInstancePosition)foundObject).ToString());
                            }
                        }
                    }
                }
                Logging.logInfo("---");
                return result;
            }
            else if (soughtAttribute == Tree.ProcessableAttributes.ElementModels) {
                Object functionResult = null;
                GetCellNumberDelegate GetCellNumber;
                RavenFunctionResult result = new RavenFunctionResult();
                int cell1Iterator, cell2Iterator, cell3Iterator;


                foreach (SearchDirection direction in GetSearchDirectionsList(allowedDirections)) {
                    GetCellNumber = GetCellNumberFunction(direction);
                    int cell1IteratorMax = 0;
                    int cell2IteratorMax = 0;
                    int cell3IteratorMax = 0;


                    List<int> cell1IteratorLast = new List<int> { -1, -1, -1 };
                    List<int> cell2IteratorLast = new List<int> { -1, -1, -1 };
                    List<int> cell3IteratorLast = new List<int> { -1, -1, -1 };

                    List<Object> foundObjects = new List<object>();

                    Logging.logInfo("Looking for " + concreteFunctionName + " in ElementModels in direction " + direction);

                    

                    for (int outerIterator = 1; outerIterator <= 3; outerIterator++) {
                        List<Object> attributeValues = new List<Object>();


                        List<ElementModelNode> cell1EM = new List<ElementModelNode>();
                        List<ElementModelNode> cell2EM = new List<ElementModelNode>();
                        List<ElementModelNode> cell3EM = new List<ElementModelNode>();

                        var cell1q = (from ei in (from e in tree.Cells[GetCellNumber(outerIterator, 1) - 1].CellObjects where e.Instance is ElementInstanceNode && ((ElementInstanceNode)e.Instance).Models != null && ((ElementInstanceNode)e.Instance).Models.Count != 0 select (ElementInstanceNode)e.Instance) select ei.Models);
                        foreach (var q11 in cell1q)
                            foreach (var q12 in q11)
                                cell1EM.Add(q12);

                        var cell2q = (from ei in (from e in tree.Cells[GetCellNumber(outerIterator, 2) - 1].CellObjects where e.Instance is ElementInstanceNode && ((ElementInstanceNode)e.Instance).Models != null && ((ElementInstanceNode)e.Instance).Models.Count != 0 select (ElementInstanceNode)e.Instance) select ei.Models);
                        foreach (var q21 in cell2q)
                            foreach (var q22 in q21)
                                cell2EM.Add(q22);

                        var cell3q = (from ei in (from e in tree.Cells[GetCellNumber(outerIterator, 3) - 1].CellObjects where e.Instance is ElementInstanceNode && ((ElementInstanceNode)e.Instance).Models != null && ((ElementInstanceNode)e.Instance).Models.Count != 0 select (ElementInstanceNode)e.Instance) select ei.Models);
                        foreach (var q31 in cell3q)
                            foreach (var q32 in q31)
                                cell3EM.Add(q32);



                        cell1Iterator = Math.Max(0, cell1IteratorLast[outerIterator - 1]);
                        cell1IteratorMax = cell1EM.Count;


                        for (; cell1Iterator < cell1IteratorMax; cell1Iterator++) {
                            attributeValues.Add(cell1EM[cell1Iterator]);
                            cell1IteratorLast[outerIterator - 1] = cell1Iterator;


                            cell2Iterator = Math.Max(0, cell2IteratorLast[outerIterator - 1]);
                            cell2IteratorMax = cell2EM.Count;

                            for (; cell2Iterator < cell2IteratorMax; cell2Iterator++) {
                                attributeValues.Add(cell2EM[cell2Iterator]);
                                cell2IteratorLast[outerIterator - 1] = cell2Iterator;

                                if (outerIterator != 3) {

                                    cell3IteratorMax = cell3EM.Count;
                                    cell3Iterator = Math.Max(0, cell3IteratorLast[outerIterator - 1]);

                                    for (; cell3Iterator < cell3IteratorMax; cell3Iterator++) {
                                        attributeValues.Add(cell3EM[cell3Iterator]);

                                        cell3IteratorLast[outerIterator - 1] = cell3Iterator;

                                        functionResult = ProcessAttributeList(attributeValues, outerIterator);

                                        attributeValues.RemoveAt(2);
                                        if (functionResult != null)
                                            break;

                                        //cell3IteratorLast[outerIterator - 1] = -1;
                                    }
                                    if (cell3Iterator == cell3IteratorMax)
                                        cell3IteratorLast[outerIterator - 1] = -1;
                                }
                                else {
                                    functionResult = ProcessAttributeList(attributeValues, outerIterator);
                                }
                                attributeValues.RemoveAt(1);

                                if (functionResult != null)
                                    break;

                                //cell2IteratorLast[outerIterator - 1] = -1;
                            }
                            if (cell2Iterator == cell2IteratorMax)
                                cell2IteratorLast[outerIterator - 1] = -1;


                            if (outerIterator == 3 && functionResult != null) {
                                //New thingie found! Add it!
                                foundObjects.Add(functionResult);
                                functionResult = null;
                                attributeValues.RemoveAt(0);
                                cell2IteratorLast[2] = -1;
                                continue;
                            }
                            attributeValues.RemoveAt(0);
                            if (functionResult != null)
                                break;

                            //cell1IteratorLast[outerIterator - 1] = -1;
                        }
                        if (cell1Iterator == cell1IteratorMax)
                            cell1IteratorLast[outerIterator - 1] = -1;

                        if (functionResult == null) {
                            //No match found or last one
                            //Check if all options are depleted

                            //oi = outerIterator
                            int oi;

                            for (oi = outerIterator; oi >= 1; oi--) {
                                if (cell3IteratorLast[oi - 1] != -1 && cell3IteratorLast[oi - 1] < cell3IteratorMax) {
                                    cell3IteratorLast[oi - 1]++;
                                    break;
                                }
                                if (cell2IteratorLast[oi - 1] != -1 && cell2IteratorLast[oi - 1] < cell2IteratorMax) {
                                    cell2IteratorLast[oi - 1]++;
                                    cell3IteratorLast[oi - 1] = -1;
                                    break;
                                }
                                if (cell1IteratorLast[oi - 1] != -1 && cell1IteratorLast[oi - 1] < cell1IteratorMax) {
                                    cell1IteratorLast[oi - 1]++;
                                    cell2IteratorLast[oi - 1] = -1;
                                    cell3IteratorLast[oi - 1] = -1;
                                    break;
                                }
                            }
                            if (oi >= 1) {
                                outerIterator = oi - 1;
                                for (int oi2 = oi + 1; oi2 <= 3; oi2++) {
                                    cell1IteratorLast[oi2 - 1] = -1;
                                    cell2IteratorLast[oi2 - 1] = -1;
                                    cell3IteratorLast[oi2 - 1] = -1;
                                }
                                continue;
                            }
                            break;
                        }
                    }

                    for (int found = foundObjects.Count - 1; found >= 0; found--) {
                        //End of direction iterator
                        ElementModelNode foundModel = (ElementModelNode)foundObjects[found];

                        Logging.logInfo("Found " + foundModel);


                        for (int sco = tree.Cells[8].CellObjects.Count - 1; sco >= 0; sco--) {
                            if (tree.Cells[8].CellObjects[sco].Instance != null && !(tree.Cells[8].CellObjects[sco].Instance is ElementInstanceNode))
                                continue;

                            if (tree.Cells[8].CellObjects[sco].Instance == null) {
                                tree.Cells[8].CellObjects[sco].Instance = new ElementInstanceNode();
                                tree.ElementInstanceNodes.Add((ElementInstanceNode) tree.Cells[8].CellObjects[sco].Instance);
                            }

                            ElementInstanceNode currentInstance = (ElementInstanceNode) tree.Cells[8].CellObjects[sco].Instance;

                            bool hasSomeAttributes = false;
                            foreach (ElementModelNode m in currentInstance.Models) {
                                foreach (AttributeNode a in foundModel.Attributes) {
                                    if (m.HasAttribute(a.Key)) {
                                        hasSomeAttributes = true;
                                        break;
                                    }
                                }
                                if (hasSomeAttributes)
                                    break;
                            }
                            if (hasSomeAttributes)
                                continue;

                            currentInstance.Models.Add(foundModel);

                            Logging.logInfo("Model added: " + foundModel);
                            foundObjects.RemoveAt(found);
                            result.elementsAdded += 1;
                            break;

                            if (sco == 0) {
                                Logging.logError("More models found than could be added! Models left were:");
                                foreach (Object foundObject in foundObjects)
                                    Logging.logInfo(((ElementModelNode)foundObject).ToString());
                            }
                        }
                    }
                }
                Logging.logInfo("---");
                result.succeeded = true;
                return result;
            }
            
            return new RavenFunctionResult();
        }
        #endregion


        #region Static functions
        /// <summary>
        /// 1-based function to get cell number from x and y (row and col)
        /// </summary>
        /// <returns>Cell number</returns>
        static public int GetCellNumberRowCol(int row, int col) {
            return (row - 1) * 3 + col;
        }
        static public int GetCellNumberColRow(int col, int row) {
            int res = (row - 1) * 3 + col;
            return res;
        }
        static public int GetCellNumberDiagDownRight(int primaryAxis, int secondaryAxis) {
            int res = (secondaryAxis - 1) * 3 + (secondaryAxis + primaryAxis - 1) % 3 + 1;
            return res;
        }
        static public int GetCellNumberDiagDownLeft(int primaryAxis, int secondaryAxis) {
            int res = (secondaryAxis - 1) * 3 + (primaryAxis - secondaryAxis + 2) % 3 + 1;
            return res;
        }
        #endregion


        #region properties

        public String RelevantAttributesString {
            get {
                return String.Join(", ", processableAttributes.Select(rel => rel.ToString()).ToArray());
            }
        }
        #endregion


        #region SearchDirection functions
        public List<SearchDirection> GetSearchDirectionsList(SearchDirection searchDirections) {
            List<SearchDirection> searchDirList = new List<SearchDirection>();
            for (SearchDirection dir = (SearchDirection)1; (int)dir < (int)allowedDirections; dir = (SearchDirection)(((int)dir) * 2)) {
                searchDirList.Add(dir);
            }
            return searchDirList;
        }

        public GetCellNumberDelegate GetCellNumberFunction(SearchDirection direction) {
            switch (direction) {
                case SearchDirection.Horizontal:
                    return new GetCellNumberDelegate(GetCellNumberRowCol);
                case SearchDirection.Vertical:
                    return new GetCellNumberDelegate(GetCellNumberColRow);
                case SearchDirection.DiagonalDownRight:
                    return new GetCellNumberDelegate(GetCellNumberDiagDownRight);
                case SearchDirection.DiagonalDownLeft:
                    return new GetCellNumberDelegate(GetCellNumberDiagDownLeft);
                default:
                    throw new ArgumentException("The direction supplied to GetCellNumberFunction is not supported");
            }
        }
        #endregion
    }
}
