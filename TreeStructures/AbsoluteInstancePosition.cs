using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utilities;

namespace TreeStructures
{
    public class AbsoluteInstancePosition : InstanceNode, IHasCommonAttributes
    {
        InstanceNode instanceNode;

        private static CommonAttributesNode commonAttributes;

        public AbsoluteInstancePosition() : base() {

        }

        public AbsoluteInstancePosition(InstanceNode instanceNode):base() {
            this.instanceNode = instanceNode;
            instanceNode.AddChildren(this);
        }

        public override bool SameAttributesAs(Node other) {
            if (other is AbsoluteInstancePosition)
                if (((AbsoluteInstancePosition)other).Instance != this.Instance)
                    return false;
            return base.SameAttributesAs(other);
        }

        public InstanceNode Instance {
            get { return instanceNode; }
            set { instanceNode = value; }
        }

        public static CommonAttributesNode CommonAttributesSTA {
            get { return commonAttributes; }
            set { commonAttributes = value; }
        }


        public Point AbsolutePosition {
            get {
                double? x = (double?)GetAttribute(Tree.AttributeNames.CenterX);
                double? y = (double?)GetAttribute(Tree.AttributeNames.CenterY);
                if (x==null)
                    x=0;
                if (y==null)
                    y=0;
                Point thisP = new Point(
                    x.Value,
                    y.Value
                );
                WpfGeometryHelper.RoundPoint(ref thisP);
                return thisP;
            }
        }

        public void InsertInCell(CellNode cell) {
            cell.AddCellObject(this);
        }

        public void InsertInCell(List<CellNode> cells) {
            foreach (CellNode cell in cells)
                InsertInCell(cell);
        }


        public void RemoveFromCell(CellNode cell) {
            cell.RemoveCellObject(this);
        }

        public void RemoveFromCell(List<CellNode> cells) {
            for (int i = cells.Count - 1; i >= 0; i--)
                RemoveFromCell(cells[i]);
        }

        public List<CellNode> Cells {
            get { return (from c in children where c is CellNode select (CellNode)c).ToList(); }
        }

        public List<int> CellNumbers {
            get {
                List<CellNode> cells = Cells;
                List<int> cellNumbers = new List<int>();
                foreach (CellNode cell in cells) {
                    if (!cellNumbers.Contains(cell.CellNumber))
                        cellNumbers.Add(cell.CellNumber);
                }
                return cellNumbers;
            }
        }

        public int CellNumbersCount {
            get {
                return CellNumbers.Count;
            }
        }

        public override void DrawIn(System.Windows.Controls.Canvas canvas, double xTranslation, double yTranslation) {
            double x=0;
            double y=0;

            if (HasAttribute(Tree.AttributeNames.CenterX))
                x = (double)GetAttribute(Tree.AttributeNames.CenterX);
            if (HasAttribute(Tree.AttributeNames.CenterY))
                y = (double)GetAttribute(Tree.AttributeNames.CenterY);
            
            if (instanceNode!=null)
                instanceNode.DrawIn(canvas, xTranslation + x, yTranslation + y);
        }

        public override string ToString(StopAt stopAt) {
            double x=0;
            double y=0;
            
            if (HasAttribute(Tree.AttributeNames.CenterX))
                x = (double)GetAttribute(Tree.AttributeNames.CenterX);
            if (HasAttribute(Tree.AttributeNames.CenterY))
                y = (double)GetAttribute(Tree.AttributeNames.CenterY);
            
            x = Math.Round(x, 3);
            y = Math.Round(y, 3);
            if (instanceNode != null)
                return "Absolute translation (" + x + ", " + y + ") of {" + instanceNode.ToString(stopAt) + "}";
            else 
                return "Absolute translation (" + x + ", " + y + ") of null - no instanceNode set.";
        }

        /// <summary>
        /// Checks if this absolutepositionnode is a translation of another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Translation of other node to become this, null if not possible</returns>
        public Point? TranslationOf(AbsoluteInstancePosition other) {
            if (this.Instance != other.Instance)
                return null;

            Point thisP = this.AbsolutePosition;
            Point otherP = other.AbsolutePosition;
            Point retP = new Point(thisP.X - otherP.X, thisP.Y - otherP.Y);
            WpfGeometryHelper.RoundPoint(ref retP);
            return retP;
        }

        #region IHasCommonAttributes Members

        public CommonAttributesNode CommonAttributes {
            get { return CommonAttributesSTA; }
        }


        public bool HasCommonAttribute(Tree.AttributeNames attribute) {
            return CommonAttributesSTA.HasAttribute(attribute);
        }

        #endregion
    }
}
