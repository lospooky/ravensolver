using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Utilities;

namespace TreeStructures
{
    public class VisualizeTree
    {
        static string[] ColourValues = new string[] { 
            "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000", 
            "800000", "008000", "000080", "808000", "800080", "008080", "808080", 
            "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0", 
            "400000", "004000", "000040", "404000", "400040", "004040", "404040", 
            "200000", "002000", "000020", "202000", "200020", "002020", "202020", 
            "600000", "006000", "000060", "606000", "600060", "006060", "606060", 
            "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0", 
            "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0"
        };
        static BrushConverter bc = new BrushConverter();

        private Tree tree;
        private Canvas c;
        private Dictionary<Tree.AttributeNames, VisualizationNode> attributes = new Dictionary<Tree.AttributeNames, VisualizationNode>();
        private Dictionary<AttributeNode, VisualizationNode> attributeValues = new Dictionary<AttributeNode, VisualizationNode>();
        private Dictionary<ElementModelNode, VisualizationNode> elementModels = new Dictionary<ElementModelNode, VisualizationNode>();
        private Dictionary<ElementInstanceNode, VisualizationNode> elementInstances = new Dictionary<ElementInstanceNode, VisualizationNode>();
        private Dictionary<RelativeInstancePosition, VisualizationNode> relativePostions = new Dictionary<RelativeInstancePosition, VisualizationNode>();
        private Dictionary<GroupNode, VisualizationNode> groupNodes = new Dictionary<GroupNode, VisualizationNode>();
        private Dictionary<AbsoluteInstancePosition, VisualizationNode> absolutePostions = new Dictionary<AbsoluteInstancePosition, VisualizationNode>();
        private Dictionary<int, Dictionary<AbsoluteInstancePosition, VisualizationNode>> leaves = new Dictionary<int, Dictionary<AbsoluteInstancePosition, VisualizationNode>>();
        private Dictionary<CellNode, VisualizationNode> cells = new Dictionary<CellNode, VisualizationNode>();

        private List<VisualizationLink> visualizationLinks = new List<VisualizationLink>();


        public const double totalHeight = 35;
        public const double labelWidth = 125;
        public const double labelHeight = 30;
        public const double attributeValueSpacing = 15;

        public const double attributesLeft = 10;
        public const double attributeValuesLeft = attributesLeft + labelWidth + 30;
        public const double elementModelsLeft = attributeValuesLeft + labelWidth + 80;
        public const double elementInstancesLeft = elementModelsLeft + labelWidth + 50;
        public const double relativePositionsLeft = elementInstancesLeft + labelWidth + 50;
        public const double groupNodesLeft = relativePositionsLeft + labelWidth + 50;
        public const double absolutePositionsLeft = groupNodesLeft + labelWidth + 50;
        public const double leavesLeft = absolutePositionsLeft + labelWidth + 50;
        public const double cellsLeft = leavesLeft + labelWidth + 50;

        public static Thickness borderThickness = new Thickness(1);
        public static Brush borderBrush = Brushes.DarkGray;
        public static Brush backgroundBrush = Brushes.White;
        public static Brush hoverBorderBrush = Brushes.Black;
        public static Brush litBorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
        public static Brush commonNodeBrush = Brushes.LightBlue;

        #region Constructors
        public VisualizeTree(Tree tree, Canvas c) {
            this.tree = tree;
            this.c = c;

            VisualizationNode common = null;
            VisualizationNode vn;
            Label l;

            //Top level attributes
            foreach (KeyValuePair<Tree.AttributeNames, Dictionary<Object, AttributeNode>> attribute in tree.Attributes) {
                int currentValueCount = attribute.Value.Count;

                l = new Label();
                l.Content = attribute.Key;
                vn = new VisualizationNode(this, l);
                attributes.Add(attribute.Key, vn);

                //Canvas.SetLeft(l, 0);
                Canvas.SetZIndex(l, 10);
                //Canvas.SetTop(l, lastAttributeHeight);


                //Attribute values
                foreach (KeyValuePair<Object, AttributeNode> attributeValue in attribute.Value) {
                    l = new Label();
                    l.Content = attributeValue.Key.ToString() + " (" + attributeValue.Value.Children.Count + ")";
                    l.ToolTip = attributeValue.Key.ToString() + " (" + attributeValue.Value.Children.Count + ")";

                    Canvas.SetZIndex(l, 10);
                    //Canvas.SetLeft(l, attributeValuesLeft);
                    //Canvas.SetTop(l, lastHeight);

                    vn = new VisualizationNode(this, l);
                    if (ElementInstanceNode.CommonAttributesSTA.Attributes.Contains(attributeValue.Value))
                        l.Background = new SolidColorBrush(Color.FromArgb(64, 0, 255, 0));

                    attributeValues.Add(attributeValue.Value, vn);
                    vn.AddParent(attributes[attribute.Key]);

                }
            }


            //ElementModels
            foreach (ElementModelNode elementModel in tree.ElementModelNodes) {
                l = new Label();
                l.Content = "ElementModel";

                l.ToolTip = elementModel.ToString();
                Canvas.SetZIndex(l, 10);

                vn = new VisualizationNode(this, l);
                elementModels.Add(elementModel, vn);

                foreach(AttributeNode a in elementModel.Attributes)
                    vn.AddParent(attributeValues[a]);
            }

            //ElementInstances
            foreach (ElementInstanceNode elementInstance in tree.ElementInstanceNodes) {
                l = new Label();


                //Tooltip
                StackPanel tt = new StackPanel();
                tt.Orientation = Orientation.Vertical;
                tt.HorizontalAlignment = HorizontalAlignment.Center;
                TextBlock tb = new TextBlock();
                tb.Text = elementInstance.ToString();
                tb.MaxWidth = 400;
                tb.TextWrapping = TextWrapping.Wrap;
                Canvas ttVis = new Canvas();
                ttVis.Height = 200;
                ttVis.Width = 200;
                elementInstance.DrawIn(ttVis);
                Border ttB = new Border();
                ttB.BorderBrush = Brushes.LightGray;
                ttB.Background = Brushes.White;
                ttB.BorderThickness = new Thickness(1);
                ttB.Width = 200;
                ttB.Child = ttVis;
                tt.Children.Add(ttB);
                tt.Children.Add(tb);
                l.ToolTip = tt;
                
                
                Canvas.SetZIndex(l, 10);

                vn = new VisualizationNode(this, l);

                l.Content = "ElementInstance";

                elementInstances.Add(elementInstance, vn);


                foreach (AttributeNode a in elementInstance.Attributes)
                    vn.AddParent(attributeValues[a]);

                foreach (ElementModelNode model in elementInstance.Models) {
                    vn.AddParent(elementModels[model]);
                }
            }



            //RelativeInstancePositions
            foreach (RelativeInstancePosition relativeInstance in tree.RelativeInstancePositions) {
                l = new Label();


                //Tooltip
                StackPanel tt = new StackPanel();
                tt.Orientation = Orientation.Vertical;
                tt.HorizontalAlignment = HorizontalAlignment.Center;
                TextBlock tb = new TextBlock();
                tb.Text = relativeInstance.ToString();
                tb.MaxWidth = 400;
                tb.TextWrapping = TextWrapping.Wrap;
                Canvas ttVis = new Canvas();
                ttVis.Height = 200;
                ttVis.Width = 200;
                relativeInstance.DrawIn(ttVis);
                Border ttB = new Border();
                ttB.BorderBrush = Brushes.LightGray;
                ttB.Background = Brushes.White;
                ttB.BorderThickness = new Thickness(1);
                ttB.Width = 200;
                ttB.Child = ttVis;
                tt.Children.Add(ttB);
                tt.Children.Add(tb);
                l.ToolTip = tt;


                Canvas.SetZIndex(l, 10);

                vn = new VisualizationNode(this, l);

                l.Content = "RelativePosition";

                relativePostions.Add(relativeInstance, vn);


                foreach (AttributeNode a in relativeInstance.Attributes)
                    vn.AddParent(attributeValues[a]);

                vn.AddParent(elementInstances[(ElementInstanceNode)relativeInstance.Instance]);
            }


            //Groups
            foreach (GroupNode group in tree.GroupNodes) {
                l = new Label();

                l.Content = "Group";

                //Tooltip
                StackPanel tt = new StackPanel();
                tt.Orientation = Orientation.Vertical;
                tt.HorizontalAlignment = HorizontalAlignment.Center;
                TextBlock tb = new TextBlock();
                tb.Text = group.ToString();
                tb.MaxWidth = 400;
                tb.TextWrapping = TextWrapping.Wrap;
                Canvas ttVis = new Canvas();
                ttVis.Height = 200;
                ttVis.Width = 200;
                group.DrawIn(ttVis);
                Border ttB = new Border();
                ttB.BorderBrush = Brushes.LightGray;
                ttB.Background = Brushes.White;
                ttB.BorderThickness = new Thickness(1);
                ttB.Width = 200;
                ttB.Child = ttVis;
                tt.Children.Add(ttB);
                tt.Children.Add(tb);
                l.ToolTip = tt;

                Canvas.SetZIndex(l, 10);

                vn = new VisualizationNode(this, l);
                groupNodes.Add(group, vn);

                foreach (AttributeNode a in group.Attributes)
                    vn.AddParent(attributeValues[a]);

                foreach (RelativeInstancePosition groupElement in group.GroupElements) {
                    vn.AddParent(relativePostions[groupElement]);
                }

            }



            //AbsoluteInstancePositions
            foreach (AbsoluteInstancePosition absoluteInstance in tree.AbsoluteInstancePositions) {
                l = new Label();


                //Tooltip
                StackPanel tt = new StackPanel();
                tt.Orientation = Orientation.Vertical;
                tt.HorizontalAlignment = HorizontalAlignment.Center;
                TextBlock tb = new TextBlock();
                tb.Text = absoluteInstance.ToString();
                tb.MaxWidth = 400;
                tb.TextWrapping = TextWrapping.Wrap;
                Canvas ttVis = new Canvas();
                ttVis.Height = 200;
                ttVis.Width = 200;
                absoluteInstance.DrawIn(ttVis);
                Border ttB = new Border();
                ttB.BorderBrush = Brushes.LightGray;
                ttB.Background = Brushes.White;
                ttB.BorderThickness = new Thickness(1);
                ttB.Width = 200;
                ttB.Child = ttVis;
                tt.Children.Add(ttB);
                tt.Children.Add(tb);
                l.ToolTip = tt;


                Canvas.SetZIndex(l, 10);

                vn = new VisualizationNode(this, l);

                if (absoluteInstance is CommonAbsolutePositionNode) {
                    l.Content = "CommonPositionNode";
                    l.Background = commonNodeBrush;
                }
                else
                    l.Content = "AbsolutePosition";

                absolutePostions.Add(absoluteInstance, vn);


                foreach (AttributeNode a in absoluteInstance.Attributes)
                    vn.AddParent(attributeValues[a]);

                if (absoluteInstance.Instance is ElementInstanceNode)
                    vn.AddParent(elementInstances[(ElementInstanceNode) absoluteInstance.Instance]);
                else if (absoluteInstance.Instance!=null)
                    vn.AddParent(groupNodes[(GroupNode) absoluteInstance.Instance]);
            }


            //Leaves
            
            foreach (CellNode cell in tree.Cells) {

                int currentLeafCount = cell.CellObjects.Count;

                leaves.Add(cell.CellNumber, new Dictionary<AbsoluteInstancePosition, VisualizationNode>());

                l = new Label();
                l.Content = "Cell " + cell.CellNumber;


                //Tooltip
                StackPanel tt = new StackPanel();
                tt.Orientation = Orientation.Vertical;
                tt.HorizontalAlignment = HorizontalAlignment.Center;
                TextBlock tb = new TextBlock();
                tb.Text = "Cell " + cell.CellNumber + ". " + cell.TotalCellObjectsCount + " objects.";
                tb.MaxWidth = 400;
                tb.TextWrapping = TextWrapping.Wrap;
                Canvas ttVis = new Canvas();
                ttVis.Height = 200;
                ttVis.Width = 200;
                cell.DrawIn(ttVis);
                Border ttB = new Border();
                ttB.BorderBrush = Brushes.LightGray;
                ttB.Background = Brushes.White;
                ttB.BorderThickness = new Thickness(1);
                ttB.Width = 200;
                ttB.Child = ttVis;
                tt.Children.Add(ttB);
                tt.Children.Add(tb);
                l.ToolTip = tt;
                
                vn = new VisualizationNode(this, l);
                cells.Add(cell, vn);


                if (common != null)
                     AddVisualisationLink(vn, common);

                //Canvas.SetLeft(l, 0);
                Canvas.SetZIndex(l, 10);
                //Canvas.SetTop(l, lastAttributeHeight);


                

                foreach (AbsoluteInstancePosition leaf in cell.CellObjects) {
                    l = new Label();

                    l.Content = "Cell Object";

                    //Tooltip
                    tt = new StackPanel();
                    tt.Orientation = Orientation.Vertical;
                    tt.HorizontalAlignment = HorizontalAlignment.Center;
                    tb = new TextBlock();
                    tb.Text = leaf.ToString();
                    tb.MaxWidth = 400;
                    tb.TextWrapping = TextWrapping.Wrap;
                    ttVis = new Canvas();
                    ttVis.Height = 200;
                    ttVis.Width = 200;
                    leaf.DrawIn(ttVis);
                    ttB = new Border();
                    ttB.BorderBrush = Brushes.LightGray;
                    ttB.Background = Brushes.White;
                    ttB.BorderThickness = new Thickness(1);
                    ttB.Width = 200;
                    ttB.Child = ttVis;
                    tt.Children.Add(ttB);
                    tt.Children.Add(tb);
                    l.ToolTip = tt;


                    Canvas.SetZIndex(l, 10);
                    //Canvas.SetLeft(l, leafLeft);
                    //Canvas.SetTop(l, lastHeight);

                    vn = new VisualizationNode(this, l);
                    leaves[cell.CellNumber].Add(leaf, vn);

                    cells[cell].AddParent(vn);

                    //foreach (AttributeNode p in leaf.Attributes) {
                    //    vn.AddParent(attributeValues[p]);
                    //}
                    AddVisualisationLink(vn, absolutePostions[leaf]);
                }
            }
        }
        #endregion

        #region Public functions
        public void Visualize(Boolean includeNonModelLinks, Boolean onlySolution) {
            c.Children.Clear();
            double cellsCount = cells.Count;
            double leavesCount = (from cs in cells.Values select cs.Parents.Count).Sum();

            double maxCount = Math.Max(attributeValues.Count + attributes.Count, leavesCount + cellsCount);


            //Attributes and attributevalues
            double attributesCount = attributes.Count;
            double attributeValuesCount = attributeValues.Count;

            double heightIncrement = maxCount / (attributesCount + attributeValuesCount) * totalHeight;
            double currentHeight = 0;

            double currentHeightInner = heightIncrement;

            foreach (KeyValuePair<Tree.AttributeNames, VisualizationNode> vnP in attributes) {
                currentHeight += heightIncrement * (1 + vnP.Value.Children.Count) / 2.0;
                vnP.Value.DrawIn(c, attributesLeft, currentHeight);
                currentHeight += heightIncrement * (1 + vnP.Value.Children.Count) / 2.0;

                foreach (VisualizationNode vn in vnP.Value.Children) {
                    vn.DrawIn(c, attributeValuesLeft, currentHeightInner);
                    currentHeightInner += heightIncrement;
                }
                currentHeightInner += heightIncrement;
            }

            //ElementModels
            double elementModelsCount = elementModels.Count;
            heightIncrement = maxCount / elementModelsCount * totalHeight;
            currentHeight = heightIncrement / 2;

            foreach (KeyValuePair<ElementModelNode, VisualizationNode> vn in elementModels) {
                vn.Value.DrawIn(c, elementModelsLeft, currentHeight);
                currentHeight += heightIncrement;
            }


            //ElementInstances
            double elementInstancesCount = elementInstances.Count;
            heightIncrement = maxCount / elementInstancesCount * totalHeight;
            currentHeight = heightIncrement / 2;

            foreach (KeyValuePair<ElementInstanceNode, VisualizationNode> vn in elementInstances) {
                vn.Value.DrawIn(c, elementInstancesLeft, currentHeight);
                currentHeight += heightIncrement;
            }



            //RelativeInstancePositions
            double relativePositionsCount = relativePostions.Count;
            heightIncrement = maxCount / relativePositionsCount * totalHeight;
            currentHeight = heightIncrement / 2;


            foreach (KeyValuePair<RelativeInstancePosition, VisualizationNode> vn in relativePostions) {
                vn.Value.DrawIn(c, relativePositionsLeft, currentHeight);
                currentHeight += heightIncrement;
            }


            //Group Nodes
            double groupNodesCount = groupNodes.Count;
            heightIncrement = maxCount / groupNodesCount * totalHeight;
            currentHeight = heightIncrement / 2;

            foreach (KeyValuePair<GroupNode, VisualizationNode> vn in groupNodes) {
                vn.Value.DrawIn(c, groupNodesLeft, currentHeight);
                currentHeight += heightIncrement;
            }



            //AbsoluteInstancePositions
            double absolutePositionsCount = absolutePostions.Count;
            heightIncrement = maxCount / absolutePositionsCount * totalHeight;
            currentHeight = heightIncrement / 2;


            foreach (KeyValuePair<AbsoluteInstancePosition, VisualizationNode> vn in absolutePostions) {
                vn.Value.DrawIn(c, absolutePositionsLeft, currentHeight);
                currentHeight += heightIncrement;
            }


            //Leaves
            //cellCount and leavesCount defined top
            
            heightIncrement = maxCount / (cellsCount + leavesCount) * totalHeight;
            currentHeight = 0;

            currentHeightInner = heightIncrement;

            foreach (KeyValuePair<CellNode, VisualizationNode> vnP in cells) {
                currentHeight += heightIncrement * (1 + vnP.Value.Parents.Count) / 2.0;
                vnP.Value.DrawIn(c, cellsLeft, currentHeight);
                currentHeight += heightIncrement * (1 + vnP.Value.Parents.Count) / 2.0;

                foreach (VisualizationNode vn in vnP.Value.Parents) {
                    vn.DrawIn(c, leavesLeft, currentHeightInner);
                    currentHeightInner += heightIncrement;
                }
                currentHeightInner += heightIncrement;
            }

            c.Height = (maxCount + 1) * totalHeight;
            c.Width = cellsLeft + labelWidth + 20;


            //Link
            foreach (VisualizationLink v in visualizationLinks)
                v.DrawIn(c);

        }

        
        public void AddVisualisationLink(VisualizationNode vn1, VisualizationNode vn2) {
            VisualizationLink v = new VisualizationLink(vn1, vn2);
            if (!this.VisualizationLinks.Contains(v)) {
                this.VisualizationLinks.Add(v);
                v.AddToNodes();
            }
        }
        #endregion

        #region Public properties
        public List<VisualizationLink> VisualizationLinks {
            get { return visualizationLinks; }
        }
        #endregion

   
    }
}
