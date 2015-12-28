using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Utilities;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace TreeStructures
{
    public class Tree
    {
        //Top level - attributes
        private Dictionary<AttributeNames, Dictionary<Object, AttributeNode>> attributes = new Dictionary<AttributeNames, Dictionary<Object, AttributeNode>>();


        private List<ElementModelNode> elementModelNodes = new List<ElementModelNode>();

        private List<RelativeInstancePosition> relativeInstancePositions = new List<RelativeInstancePosition>();


        private List<ElementInstanceNode> elementInstanceNodes = new List<ElementInstanceNode>();
        private List<GroupNode> groupNodes = new List<GroupNode>();

        private List<AbsoluteInstancePosition> absoluteInstancePositions = new List<AbsoluteInstancePosition>();


        //Bottom level - leaves, one per visual object
        //All are IInstanceNodes

        private List<CellNode> cells = new List<CellNode>(9);


        private List<CellNode> solutionCells = new List<CellNode>(8);

        #region Constructor
        //We now work from -0.5 to +0.5 graphically, width and height 0<w,h<1
        public Tree(Canvas c, String keyword)
        {
            CellNode.CommonElements = null;
            ElementInstanceNode.CommonAttributesSTA = new CommonAttributesNode();
            AbsoluteInstancePosition.CommonAttributesSTA = new CommonAttributesNode();
            RelativeInstancePosition.CommonAttributesSTA = new CommonAttributesNode();

            for (int i = 0; i < 9; i++)
            {
                cells.Add(new CellNode(i + 1));
            }

            Logging.logInfo("---");
            Logging.logInfo("Building Tree Structure:");

            //Logging.logInfo("Adding static top level attributes...");
            setUpAttributes();
            Logging.logInfo(attributes.Count + " Attributes Added");

            if (keyword.Equals(Keywords.Problem) || keyword.Equals(Keywords.Candidates))
                initializeAttributeValues(c, keyword);

            Logging.logInfo("-\n" + keyword + " Cells Parsed: " + elementInstanceNodes.Count + " elements added at leaf level.");

            Logging.logInfo("Extracting properties common to all leaves...");
            extractCommonAttributes();
            Logging.logInfo("-");
            Logging.logInfo("Merging duplicate element instances..");
            mergeEqualElementInstances();
            
            Logging.logInfo("Instances: " + ElementInstanceNodes.Count);

            Logging.logInfo("-");
            Logging.logInfo("Merging duplicate absolute position instances...");
            mergeEqualAbsolutePositions();


            Logging.logInfo("Extracting properties common to some leaves...");

            extractPartialAttributes();
            Logging.logInfo(elementModelNodes.Count + " Partially common properties found");


            Logging.logInfo("Merging partially common properties into element model nodes...");
            createElementModels();
            Logging.logInfo(elementModelNodes.Count + " Element models created");
            Logging.logInfo("-");
            foreach (CellNode cellie in cells) {
                Logging.logInfo(cellie.ToString());
            }
            extractCommonCellObjects();
            Logging.logInfo("Aggregating Element Models into Group Models...");
            createGroupModels();
            Logging.logInfo("-");
            Logging.logInfo("---");
            mergeGroups();

        }
        #endregion

        #region Public Methods


        public AttributeNode AddAttribute(AttributeNames attribute, Object value)
        {
            if (value is Point) {
                Point p = (Point)value;
                value = WpfGeometryHelper.RoundPoint(ref p);
            }
            if (attribute == AttributeNames.StrokeDashArray || attribute == AttributeNames.PathGeometry) {
                AttributeNode sda = attributes[attribute].FirstOrDefault(attr => attr.Key.ToString() == value.ToString()).Value;
                if (sda != null)
                    return sda;
            }

            if (!attributes[attribute].ContainsKey(value)) {
                attributes[attribute].Add(value, new AttributeNode(attribute, value));
            }
            return attributes[attribute][value];
        }

        #endregion


        #region Private Methods


        private void setUpAttributes()
        {
            foreach (AttributeNames an in Enum.GetValues(typeof(AttributeNames)))
            {
                attributes.Add(an, new Dictionary<object, AttributeNode>());
            }
        }

        private void initializeAttributeValues(UIElement el, String keyword)
        {
            Logging.logInfo("Parsing " + keyword.ToString().Replace("Grid", "") + " Cells...\n-");
            if (el is Canvas && ((Canvas)el).Name == keyword)
                initializeAttributeValues(el, true);
        }

        private void initializeAttributeValues(UIElement el, bool isFullGrid)
        {
            if (isFullGrid) //first call, whole problem/candidates grid
                foreach (UIElement cell in ((Canvas)el).Children)
                {
                    if (!(cell is Canvas) || ((Canvas)cell).Name.Equals(Keywords.Solution))
                        continue;
                    initializeAttributeValues(cell, false);
                }
            else if (el is Canvas) // cell
            {
                int cellNumber = cellName2cellNumber(((Canvas)el).Name);
                Logging.logInfo("Cell #" + cellNumber + ":");

                foreach (UIElement child in ((Canvas)el).Children)
                    initializeAttributeValues(child, cellNumber);
            }
        }

        private void initializeAttributeValues(UIElement el, int cellNumber)
        {
            ElementInstanceNode n = new ElementInstanceNode();
            AbsoluteInstancePosition pos = new AbsoluteInstancePosition(n);
            elementInstanceNodes.Add(n);
            absoluteInstancePositions.Add(pos);

            cells[cellNumber - 1].AddCellObject(pos);
            Type elType = el.GetType();
            String elTypeString = elType.Name;
            n.AddAttributes(AddAttribute(AttributeNames.Type, elTypeString));

            if (el is FrameworkElement) { //All are...
                WpfGeometryHelper gh = new WpfGeometryHelper((FrameworkElement)el);

                pos.AddAttributes(AddAttribute(AttributeNames.CenterX, gh.center.X));
                pos.AddAttributes(AddAttribute(AttributeNames.CenterY, gh.center.Y));
                n.AddAttributes(AddAttribute(AttributeNames.Width, gh.boundingBox.Width));
                n.AddAttributes(AddAttribute(AttributeNames.Height, gh.boundingBox.Height));
                n.AddAttributes(AddAttribute(AttributeNames.VertexPointsCount, gh.vertexPoints.Count));
                foreach (Point p in gh.vertexPoints) {
                    n.AddAttributes(AddAttribute(AttributeNames.VertexPoints, p));
                }

                if (gh.pathGeometry != null) {
                    n.AddAttributes(AddAttribute(AttributeNames.PathGeometry, gh.pathGeometry));
                }
            }
            else
                throw new Exception();
            if (el is Shape)
            { //All are...
                //TODO: Add support for fill texture and line styles
                Shape s = (Shape)el;
                if (s.Fill != null) n.AddAttributes(AddAttribute(AttributeNames.FillBrush, s.Fill));
                if (s.Stroke != null) n.AddAttributes(AddAttribute(AttributeNames.StrokeBrush, s.Stroke));
                if (s.StrokeDashArray != null && s.StrokeDashArray.Count != 0) n.AddAttributes(AddAttribute(AttributeNames.StrokeDashArray, s.StrokeDashArray));
                if (s.StrokeDashOffset != 0) n.AddAttributes(AddAttribute(AttributeNames.StrokeDashOffset, s.StrokeDashOffset));
                if (s.StrokeEndLineCap != 0) n.AddAttributes(AddAttribute(AttributeNames.StrokeEndLineCap, s.StrokeEndLineCap));
                if (s.RenderTransform != null) {
                    if (s.RenderTransform is RotateTransform) {
                        n.AddAttributes(AddAttribute(AttributeNames.RotateTransformAngle, ((RotateTransform)s.RenderTransform).Angle));
                        n.AddAttributes(AddAttribute(AttributeNames.RotateTransformCenterX, ((RotateTransform)s.RenderTransform).CenterX));
                        n.AddAttributes(AddAttribute(AttributeNames.RotateTransformCenterY, ((RotateTransform)s.RenderTransform).CenterY));
                    }
                }
                n.AddAttributes(AddAttribute(AttributeNames.StrokeThickness, s.StrokeThickness));
            }

            Logging.logInfo("Adding Leaf: " + n.ToString());

        }

        private int cellName2cellNumber(string cellName)
        {
            int trimIndex = 0;

            if (cellName.Contains("Problem"))
                trimIndex = 11;
            else if (cellName.Contains("Answer"))
                trimIndex = 10;

            return Int32.Parse(cellName.Substring(trimIndex));
        }

        private void extractCommonAttributes()
        {
            int numberOfElements = ElementInstanceNodes.Count;

            ElementInstanceNode.CommonAttributesSTA = new CommonAttributesNode();

            foreach (KeyValuePair<AttributeNames, Dictionary<Object, AttributeNode>> kvp in attributes)
            {
                foreach (KeyValuePair<Object, AttributeNode> attributeValues in kvp.Value)
                {
                    //All the leaves/elements are the children
                    if (attributeValues.Value.Children.Count == numberOfElements)
                    {
                        if (kvp.Key == AttributeNames.CenterX || kvp.Key == AttributeNames.CenterY) {
                            AbsoluteInstancePosition.CommonAttributesSTA.AddAttributes(attributeValues.Value);
                            foreach (AbsoluteInstancePosition element in absoluteInstancePositions)
                                element.RemoveAttributes(attributeValues.Value);
                        }
                        else {
                            ElementInstanceNode.CommonAttributesSTA.AddAttributes(attributeValues.Value);
                            foreach (ElementInstanceNode element in ElementInstanceNodes)
                                element.RemoveAttributes(attributeValues.Value);
                        }
                    }
                }
            }

            if (ElementInstanceNode.CommonAttributesSTA.Attributes.Count != 0)
            {
                Logging.logInfo(ElementInstanceNode.CommonAttributesSTA.Attributes.Count + " properties are common to all nodes:");
                Logging.logInfo(ElementInstanceNode.CommonAttributesSTA.ToString());

            }
            else
                Logging.logInfo("No properties are common to all element instances.");

            if (AbsoluteInstancePosition.CommonAttributesSTA.Attributes.Count != 0) {
                Logging.logInfo(AbsoluteInstancePosition.CommonAttributesSTA.Attributes.Count + " properties are common to all nodes:");
                Logging.logInfo(AbsoluteInstancePosition.CommonAttributesSTA.ToString());

            }
            else
                Logging.logInfo("No properties are common to all absolute positions.");

            Logging.logInfo("Cells after propagation:");
            foreach (CellNode c in cells) {
                Logging.logInfo(c.ToString());
            }
        }

        private void mergeEqualElementInstances()
        {
            //for (int i = ElementInstanceNodes.Count - 1; i >= 0; i--)
            int i = 0;

            do
            {
                bool startOver = false;
                var query = from ElementInstanceNode ea in ElementInstanceNodes
                            where (ea != ElementInstanceNodes[i] && ElementInstanceNodes[i].SameAttributesAs(ea))
                            select ea;

                if (query.Count() > 0)
                {
                    //foreach (ElementInstanceNode el in query)
                    List<ElementInstanceNode> duplicates = query.ToList();
                    for (int j = duplicates.Count() - 1; j >= 0; j--)
                    {
                        for (int k = duplicates[j].Children.Count - 1; k>=0; k--) {
                            if (duplicates[j].Children[k] is AbsoluteInstancePosition) {
                                AbsoluteInstancePosition instancePos = (AbsoluteInstancePosition)duplicates[j].Children[k];
                                instancePos.Instance.Children.Remove(instancePos);
                                instancePos.Instance = elementInstanceNodes[i];
                                instancePos.Instance.Children.Add(instancePos);
                            }
                        }
                        duplicates[j].RemoveAttributes(duplicates[j].Attributes);
                        ElementInstanceNodes.Remove(duplicates[j]);
                    }
                    startOver = true;
                }

                if (startOver)
                    i = 0;
                else
                    i++;

            } while (i < ElementInstanceNodes.Count);
        }


        private void mergeEqualAbsolutePositions() {
            int i = 0;

            do {
                bool startOver = false;
                var query = from AbsoluteInstancePosition ea in absoluteInstancePositions
                            where (ea != absoluteInstancePositions[i] && absoluteInstancePositions[i].SameAttributesAs(ea))
                            select ea;

                if (query.Count() > 0) {
                    //foreach (ElementInstanceNode el in query)
                    List<AbsoluteInstancePosition> duplicates = query.ToList();
                    for (int j = duplicates.Count() - 1; j >= 0; j--) {
                        for (int k = duplicates[j].Children.Count - 1; k >= 0; k--) {
                            if (duplicates[j].Children[k] is CellNode) {
                                CellNode cell = (CellNode)duplicates[j].Children[k];
                                duplicates[j].Children.Remove(cell);
                                cell.RemoveCellObject(duplicates[j]);
                                cell.AddCellObject(absoluteInstancePositions[i]);
                            }
                        }
                        duplicates[j].RemoveAttributes(duplicates[j].Attributes);
                        absoluteInstancePositions.Remove(duplicates[j]);
                    }
                    startOver = true;
                }

                if (startOver)
                    i = 0;
                else
                    i++;

            } while (i < absoluteInstancePositions.Count);
        }


        private void extractPartialAttributes()
        {
            elementModelNodes = new List<ElementModelNode>();

            foreach (KeyValuePair<AttributeNames, Dictionary<Object, AttributeNode>> kvp in attributes) {


                AttributeNames attribute = kvp.Key;

                foreach (KeyValuePair<Object, AttributeNode> attributeValues in kvp.Value) {
                    //Used to use descendants
                    if (attributeValues.Value.Children.Count > 1) {
                        ElementModelNode model = new ElementModelNode();
                        elementModelNodes.Add(model);

                        //instance taken care of first, cause else children includes the model :S
                        List<Node> instances = attributeValues.Value.Children;
                        for (int i = instances.Count - 1; i >= 0; i--) {
                            if (!(instances[i] is ElementInstanceNode))
                                continue; //AbsoluteInstanceNode for CenterX & Y
                            ((ElementInstanceNode)instances[i]).AddModels(model);
                            instances[i].RemoveAttributes(attributeValues.Value);
                        }
                        model.AddAttributes(attributeValues.Value);
                    }
                }

                //Remove models with no children
                for (int j = elementModelNodes.Count - 1; j >= 0; j--) {
                    if (elementModelNodes[j].Children.Count == 0) {
                        for (int k=elementModelNodes[j].Attributes.Count-1; k>=0; k--)
                            elementModelNodes[j].RemoveAttributes(elementModelNodes[j].Attributes[k]);
                        elementModelNodes.RemoveAt(j);
                    }
                }
            }
        }

        private void createElementModels()
        {
            //TODO BUG: reorders parents (attributes), matters for vertexpoints for polygons for example. 
            //Needs fix or other vertexpoints implementation

            ChildrenComparer<ElementModelNode> nc = new ChildrenComparer<ElementModelNode>();
            elementModelNodes.Sort(nc);

            int i = 0;
            do
            {
                bool atLeastOnePartialMerge = false;
                var query = from ElementModelNode n in elementModelNodes
                            where (n.Children.Count >= elementModelNodes[i].Children.Count) &&
                                  !(n.Equals(elementModelNodes[i]))
                            select n;

                query = query.ToList();
                foreach (ElementModelNode q in query)
                {
                    bool equivalency;
                    if (q.Children.Count == elementModelNodes[i].Children.Count)
                        equivalency = true;
                    else
                        equivalency = false;

                    if (childrenCompleteIntersection(elementModelNodes[i], q))
                    {
                        elementModelNodes[i].AddAttributes(q.Attributes);
                        bool noMoreChildren = false;

                        foreach (ElementInstanceNode instance in elementModelNodes[i].Children)
                            instance.RemoveModels(q);

                        if (!equivalency)
                        {
                            if (q.Children.Count == 0)
                                noMoreChildren = true;
                            if (!atLeastOnePartialMerge)
                                atLeastOnePartialMerge = true;
                        }
                        if (equivalency || noMoreChildren)
                        {
                            q.RemoveAttributes(q.Attributes);
                            elementModelNodes.Remove(q);
                        }
                    }
                }

                if (atLeastOnePartialMerge)
                {
                    elementModelNodes.Sort(nc);
                    i = 0;
                }
                else
                    i++;

            } while (i < elementModelNodes.Count);


            //IMPORTANT!
            //Takes care of one-time appearing models (multi-appearing attrib sets)
            //with some non-modeled attributes. Merges attributes into the model.

            foreach (ElementModelNode m in elementModelNodes)
            {
                if (m.Children.Count == 1)
                {
                    ElementInstanceNode child = (ElementInstanceNode)m.Children[0];

                    List<AttributeNode> sparseAttribs = child.Attributes;

                    if (sparseAttribs.Count() > 0)
                    {
                        m.AddAttributes(sparseAttribs);
                        child.RemoveAttributes(sparseAttribs);
                    }
                }
            }
        }


        private void extractCommonCellObjects()
        {
            var query = from AbsoluteInstancePosition el in AbsoluteInstancePositions
                        where el.CellNumbers.Count == 8
                        select el;
            List<AbsoluteInstancePosition> commonElements = query.ToList();
            
            foreach (AbsoluteInstancePosition aip in commonElements)
            {
                foreach (CellNode cell in Cells)
                    if (cell.CellNumber < 9)
                        cell.RemoveCellObject(aip);
                absoluteInstancePositions.Remove(aip);
            }

            if (commonElements.Count == 1)
            {
                CommonAbsolutePositionNode common = new CommonAbsolutePositionNode(commonElements[0]);
                CellNode.CommonElements = common;
                absoluteInstancePositions.Add(common);

            }
            else if (commonElements.Count > 1)
            {
                
                GroupNode commonGroup = new GroupNode();
                foreach (AbsoluteInstancePosition ap in commonElements) {
                    RelativeInstancePosition rp = new RelativeInstancePosition((ElementInstanceNode)ap.Instance);
                    rp.AddAttributes(AddAttribute(AttributeNames.CenterXRelative, ap.GetAttribute(AttributeNames.CenterX)));
                    rp.AddAttributes(AddAttribute(AttributeNames.CenterYRelative, ap.GetAttribute(AttributeNames.CenterY)));
                    relativeInstancePositions.Add(rp);
                    commonGroup.AddElements(rp);
                }
                groupNodes.Add(commonGroup);
                CommonAbsolutePositionNode common = new CommonAbsolutePositionNode(commonGroup);
                common.AddAttributes(AddAttribute(AttributeNames.CenterX, 0.0));
                common.AddAttributes(AddAttribute(AttributeNames.CenterY, 0.0));
                absoluteInstancePositions.Add(common);
                CellNode.CommonElements = common;
            }

        }



        private void createGroupModels()
        {
            ChildrenComparer<ElementInstanceNode> nc = new ChildrenComparer<ElementInstanceNode>();
            elementInstanceNodes.Sort(nc);

            //Builds cell appearance lists
            List<int[]> appearances = new List<int[]>();
            foreach (AbsoluteInstancePosition ap in AbsoluteInstancePositions)
            {
                bool add = true;
                foreach (int[] cellset in appearances)
                {
                    if (arrayEquality(ap.CellNumbers.ToArray(), cellset))
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    appearances.Add(ap.CellNumbers.ToArray());
            }

            //Finds out what is to merge where
            foreach (int[] arr in appearances)
            {
                var query = (from ap in AbsoluteInstancePositions
                             where arrayEquality(ap.CellNumbers.ToArray(), arr)
                             select ap).ToList();

                if (query.Count() > 1)
                {
                    List<RelativeInstancePosition> relatives = new List<RelativeInstancePosition>();

                    double minX = Double.MaxValue;
                    double minY = Double.MaxValue;
                    double maxX = Double.MinValue;
                    double maxY = Double.MinValue;

                    foreach (AbsoluteInstancePosition ap in query)
                    {

                        for (int i = ap.CellNumbersCount - 1; i >= 0; i--)
                            ap.RemoveFromCell(ap.Cells[i]);
                        ap.Instance.RemoveChildren(ap);

                        Point center = new Point((double)ap.GetAttribute(AttributeNames.CenterX),(double)ap.GetAttribute(AttributeNames.CenterY));
                        WpfGeometryHelper.RoundPoint(ref center);


                        foreach (Object p in ((ElementInstanceNode)ap.Instance).GetModelAndOwnAttributes(AttributeNames.VertexPoints)) {
                            if (((Point)p).X + center.X < minX) minX = ((Point)p).X + center.X;
                            if (((Point)p).Y + center.Y < minY) minY = ((Point)p).Y + center.Y;
                            if (((Point)p).X + center.X > maxX) maxX = ((Point)p).X + center.X;
                            if (((Point)p).Y + center.Y > maxY) maxY = ((Point)p).Y + center.Y;
                        }

                        RelativeInstancePosition rp = new RelativeInstancePosition((ElementInstanceNode)ap.Instance);
                        rp.AddAttributes(AddAttribute(AttributeNames.CenterXRelative, center.X));
                        rp.AddAttributes(AddAttribute(AttributeNames.CenterYRelative, center.Y));


                        relativeInstancePositions.Add(rp);
                        relatives.Add(rp);



                        ap.Instance = null;
                        AbsoluteInstancePositions.Remove(ap);
                    }
                    GroupNode group = new GroupNode(relatives);
                    GroupNodes.Add(group);
                    AbsoluteInstancePosition grpabspos = new AbsoluteInstancePosition(group);
                    AbsoluteInstancePositions.Add(grpabspos);


                    Rect boundingBox = new Rect(minX + (maxX - minX)/2.0, minY + (maxY - minY)/2.0, maxX - minX, maxY - minY);


                    grpabspos.AddAttributes(AddAttribute(AttributeNames.CenterX, boundingBox.Left));
                    grpabspos.AddAttributes(AddAttribute(AttributeNames.CenterY, boundingBox.Top));

                    foreach (RelativeInstancePosition rel in group.GroupElements) {
                        Point center = new Point((double)rel.GetAttribute(AttributeNames.CenterXRelative), (double)rel.GetAttribute(AttributeNames.CenterYRelative));
                        rel.RemoveAttributes(rel.GetAttributeNode(AttributeNames.CenterXRelative));
                        rel.RemoveAttributes(rel.GetAttributeNode(AttributeNames.CenterYRelative));

                        WpfGeometryHelper.RoundPoint(ref center);

                        rel.AddAttributes(AddAttribute(AttributeNames.CenterXRelative, center.X - boundingBox.X));
                        rel.AddAttributes(AddAttribute(AttributeNames.CenterYRelative, center.Y - boundingBox.Y));
                    }



                    foreach (int i in arr)
                        grpabspos.InsertInCell(Cells[i - 1]);

                    //Logging.logInfo("BBox: " + minX + "," + minY + ";" + minX + "," + maxY + ";" + maxX + "," + maxY + ";" + minX + "," + maxY);

                }
            }

            for (int i = groupNodes.Count - 1; i >= 1; i--) {
                for (int j = i - 1; j >= 0; j--) {
                    if (groupNodes[i].Equals(groupNodes[j])) {
                        for (int k = groupNodes[i].Children.Count - 1; k >= 0; k--) {
                            ((AbsoluteInstancePosition)groupNodes[i].Children[k]).Instance = groupNodes[j];
                            groupNodes[j].Children.Add(groupNodes[i].Children[k]);
                            groupNodes[i].Children.RemoveAt(k);
                        }
                        groupNodes.RemoveAt(i);
                    }
                }
            }
        }

        private void mergeGroups()
        {
            List<int[]> fullinstancesets = new List<int[]>();
            foreach (GroupNode g in GroupNodes)
            {
                List<int> instanceset = new List<int>();
                foreach (RelativeInstancePosition r in g.GroupElements)
                    instanceset.Add(ElementInstanceNodes.IndexOf(r.Instance));
                fullinstancesets.Add(instanceset.ToArray());
            }

            List<int[]> distinctinstancesets = new List<int[]>(fullinstancesets.Count);
            foreach (int[] arr in fullinstancesets)
                distinctinstancesets.Add(arr.Distinct().ToArray());


            for (int i = 0; i < GroupNodes.Count; i++)
            {
                int occurrances = numberOfTimes(distinctinstancesets[i], fullinstancesets[i]);
                if (occurrances > 1)
                {
                    bool mergeToSomething = false;
                    int mergeWith = 0;
                    foreach (int[] arr in fullinstancesets)
                    {
                        if (arrayEquality(arr, distinctinstancesets[i]))
                        {
                            mergeToSomething = true;
                            mergeWith = fullinstancesets.IndexOf(arr);
                            break;
                        }
                    }
                    if (mergeToSomething)
                    {
                        Dictionary<RelativeInstancePosition, List<Tuple<RelativeInstancePosition, Point>>> smallToLargeMatch = new Dictionary<RelativeInstancePosition, List<Tuple<RelativeInstancePosition, Point>>>();
                        Logging.logInfo(mergeWith + "->" + i);
                        GroupNode basal = GroupNodes[mergeWith];
                        GroupNode multi = GroupNodes[i];

                        var basepairs = from r1 in basal.GroupElements
                                        from r2 in basal.GroupElements
                                        where ElementInstanceNodes.IndexOf(r1.Instance) < ElementInstanceNodes.IndexOf(r2.Instance)
                                        select new {
                                                ID1 = ElementInstanceNodes.IndexOf(r1.Instance),
                                                ID2 = ElementInstanceNodes.IndexOf(r2.Instance),
                                                OffsetX = (double)r1.GetAttribute(AttributeNames.CenterXRelative)-(double)r2.GetAttribute(AttributeNames.CenterXRelative),
                                                OffsetY = (double)r1.GetAttribute(AttributeNames.CenterYRelative)-(double)r2.GetAttribute(AttributeNames.CenterYRelative),
                                                R1 = r1,
                                                R2 = r2
                                                };

                        var multipairs = from r1 in multi.GroupElements
                                         from r2 in multi.GroupElements
                                         where ElementInstanceNodes.IndexOf(r1.Instance) < ElementInstanceNodes.IndexOf(r2.Instance)
                                         select new
                                         {
                                             ID1 = ElementInstanceNodes.IndexOf(r1.Instance),
                                             ID2 = ElementInstanceNodes.IndexOf(r2.Instance),
                                             
                                             //BIG PROBLEM: Floating Point Errors (?)
                                             OffsetX = (double)r1.GetAttribute(AttributeNames.CenterXRelative) - (double)r2.GetAttribute(AttributeNames.CenterXRelative),
                                             OffsetY = (double)r1.GetAttribute(AttributeNames.CenterYRelative) - (double)r2.GetAttribute(AttributeNames.CenterYRelative),
                                             R1 = r1,
                                             R2 = r2,
                                         };

                        var matches = from b in basepairs
                                      from m in multipairs
                                      where b.ID1 == m.ID1 &&
                                            b.ID2 == m.ID2 &&
                                            b.OffsetX == m.OffsetX &&
                                            b.OffsetY == m.OffsetY
                                      select new {
                                          B1 = b.R1,
                                          B2 = b.R2,
                                          B1OffsetX = (double)m.R1.GetAttribute(AttributeNames.CenterXRelative) - (double)b.R1.GetAttribute(AttributeNames.CenterXRelative),
                                          B1OffsetY = (double)m.R1.GetAttribute(AttributeNames.CenterYRelative) - (double)b.R1.GetAttribute(AttributeNames.CenterYRelative),
                                          B2OffsetX = (double)m.R2.GetAttribute(AttributeNames.CenterXRelative) - (double)b.R2.GetAttribute(AttributeNames.CenterXRelative),
                                          B2OffsetY = (double)m.R2.GetAttribute(AttributeNames.CenterYRelative) - (double)b.R2.GetAttribute(AttributeNames.CenterYRelative),
                                          M1 = m.R1, 
                                          M2 = m.R2
                                      };

                        Dictionary<Point, AbsoluteInstancePosition> offsetGroups = new Dictionary<Point, AbsoluteInstancePosition>();


                        foreach(var match in matches) {
                            if (!smallToLargeMatch.ContainsKey(match.B1))
                                smallToLargeMatch.Add(match.B1, new List<Tuple<RelativeInstancePosition,Point>>());
                            Tuple<RelativeInstancePosition, Point> t1 = new Tuple<RelativeInstancePosition, Point>(match.M1, new Point(match.B1OffsetX, match.B1OffsetY));
                            if (!smallToLargeMatch[match.B1].Contains(t1))
                                smallToLargeMatch[match.B1].Add(t1);

                            if (!smallToLargeMatch.ContainsKey(match.B2))
                                smallToLargeMatch.Add(match.B2, new List<Tuple<RelativeInstancePosition, Point>>());
                            Tuple<RelativeInstancePosition, Point> t2 = new Tuple<RelativeInstancePosition, Point>(match.M2, new Point(match.B2OffsetX, match.B2OffsetY));
                            if (!smallToLargeMatch[match.B1].Contains(t2))
                                smallToLargeMatch[match.B1].Add(t2);
                        }


                        foreach (KeyValuePair<RelativeInstancePosition, List<Tuple<RelativeInstancePosition,Point>>> kvp in smallToLargeMatch) {
                            for (int kvpI = kvp.Value.Count - 1; kvpI >= 0; kvpI--) {
                                AbsoluteInstancePosition currentGroupPosition;

                                object xpos = multi.Children[0].GetAttribute(AttributeNames.CenterX);
                                object ypos = multi.Children[0].GetAttribute(AttributeNames.CenterY);

                                Point oldP = new Point((double)xpos, (double)ypos);
                                oldP.Offset(kvp.Value[kvpI].Second.X, kvp.Value[kvpI].Second.Y);
                                WpfGeometryHelper.RoundPoint(ref oldP);

                                if (offsetGroups.ContainsKey(oldP))
                                    currentGroupPosition = offsetGroups[kvp.Value[kvpI].Second];
                                else {
                                    currentGroupPosition = new AbsoluteInstancePosition(basal);
                                    offsetGroups.Add(oldP, currentGroupPosition);

                                    currentGroupPosition.AddAttributes(AddAttribute(AttributeNames.CenterX, oldP.X));
                                    currentGroupPosition.AddAttributes(AddAttribute(AttributeNames.CenterY, oldP.Y));

                                    List<CellNode> cells = ((AbsoluteInstancePosition)multi.Children[0]).Cells;
                                    currentGroupPosition.InsertInCell(cells);

                                    absoluteInstancePositions.Add(currentGroupPosition);
                                }


                                kvp.Value[kvpI].First.RemoveChildren(multi);
                                multi.GroupElements.Remove(kvp.Value[kvpI].First);

                            }
                        }

                        
                    }
                }
            }

            //Remove groups with only one occurance
            for (int g = groupNodes.Count - 1; g >= 0; g--) {
                if (groupNodes[g].Children.Count == 1 && groupNodes[g].Children[0].Children.Count == 1) {
                    GroupNode gr = groupNodes[g];
                    AbsoluteInstancePosition grAp = (AbsoluteInstancePosition)gr.Children[0];
                    Point grPos = grAp.AbsolutePosition;
                    foreach (RelativeInstancePosition ge in gr.GroupElements) {
                        Point relPos = ge.RelativePosition;
                        relPos.Offset(grPos.X, grPos.Y);
                        WpfGeometryHelper.RoundPoint(ref relPos);

                        AbsoluteInstancePosition newAp = new AbsoluteInstancePosition(ge.Instance);

                        newAp.AddAttributes(AddAttribute(AttributeNames.CenterX, relPos.X));
                        newAp.AddAttributes(AddAttribute(AttributeNames.CenterY, relPos.Y));

                        absoluteInstancePositions.Add(newAp);
                        newAp.InsertInCell(grAp.Cells);

                        relativeInstancePositions.Remove(ge);
                    }
                    absoluteInstancePositions.Remove(grAp);
                    grAp.RemoveFromCell(grAp.Cells);

                    groupNodes.RemoveAt(g);
                }
            }


            //Clean up empty nodes
            for (int r = relativeInstancePositions.Count - 1; r >= 0; r--) {
                if (relativeInstancePositions[r].Children.Count == 0)
                    relativeInstancePositions.RemoveAt(r);
            }

            for (int gn = groupNodes.Count - 1; gn >= 0; gn--) {
                if (groupNodes[gn].GroupElements.Count == 0) {
                    for (int ap = groupNodes[gn].Children.Count - 1; ap >= 0; ap-- ) {
                        ((AbsoluteInstancePosition)groupNodes[gn].Children[ap]).RemoveFromCell(((AbsoluteInstancePosition)groupNodes[gn].Children[ap]).Cells);
                        absoluteInstancePositions.Remove((AbsoluteInstancePosition)groupNodes[gn].Children[ap]);
                        groupNodes[gn].Children.RemoveAt(ap);
                    }
                    groupNodes.RemoveAt(gn);
                }
            }
        }

        private int numberOfTimes(int[] a1, int[] a2)
        {
            int count = 0;
            foreach (int n in a1)
                if (count == 0)
                    count = a2.Count(x => x == n);
                else if (count != a2.Count(x => x == n))
                    return 0;

            return count;
        }



        private bool childrenCompleteIntersection(Node n1, Node n2)
        {
            if (n1.Children.Count > n2.Children.Count)
            {
                Node temp = n1;
                n1 = n2;
                n2 = temp;
            }

            foreach (Node c in n1.Children)
                if (!n2.Children.Contains(c))
                    return false;

            return true;
        }


        private bool isContainedin(int[] shortarr, int[] longarr)
        {
            if (shortarr.Length > longarr.Length)
                return false;
            else
                foreach (int i in shortarr)
                    if (!longarr.Contains(i))
                        return false;
            return true;
        }

        private bool arrayEquality(int[] a1, int[] a2)
        {
            if (a1.Count() != a2.Count())
                return false;
            else
                foreach (int i in a1)
                    if (!a2.Contains(i))
                        return false;
            return true;
        }
        #endregion


        #region Properties

        public List<CellNode> Cells
        {
            get { return cells; }
            set { cells = value; }
        }

        public List<ElementModelNode> ElementModelNodes
        {
            get { return elementModelNodes; }
        }
        public List<ElementInstanceNode> ElementInstanceNodes
        {
            get { return elementInstanceNodes; }
            set { elementInstanceNodes = value; }
        }
        public List<GroupNode> GroupNodes
        {
            get { return groupNodes; }
        }
        public List<AbsoluteInstancePosition> AbsoluteInstancePositions {
            get { return absoluteInstancePositions; }
        }
        public List<RelativeInstancePosition> RelativeInstancePositions {
            get { return relativeInstancePositions; }
        }


        public Dictionary<AttributeNames, Dictionary<Object, AttributeNode>> Attributes
        {
            get { return attributes; }
        }

        public double AverageCellObjectsNumber {
            get {
                return (from co in cells select co.CellObjects.Count).Average();
            }
        }
        #endregion



        public enum AttributeNames { Type, CenterX, CenterY, CenterXRelative, CenterYRelative, Width, Height, VertexPoints, VertexPointsCount, FillBrush, StrokeBrush, StrokeThickness, StrokeDashArray, StrokeDashOffset, StrokeEndLineCap, RotateTransformAngle, RotateTransformCenterX, RotateTransformCenterY, PathGeometry }
        public enum ProcessableAttributes { ObjectsCount, CellObjects, AbsolutePositionInstances, ElementInstances, ElementModels, Attributes }
    }
}
