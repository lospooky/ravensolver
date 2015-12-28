using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TreeStructures
{
    public class CellNode : InstanceNode
    {
        private int cellNumber = 0;
        private static CommonAbsolutePositionNode commonElements;
        private List<AbsoluteInstancePosition> cellObjects = new List<AbsoluteInstancePosition>();

        #region Constructors
        public CellNode(int cellNumber) {
            this.cellNumber = cellNumber;
        }
        #endregion

        #region Properties
        public int TotalCellObjectsCount {
            get {
                int count;

                if (commonElements==null)
                    count=0;
                else if (commonElements.Instance is ElementInstanceNode)
                    count=1;
                else
                    count = ((GroupNode)commonElements.Instance).GroupElements.Count;
                

                foreach (AbsoluteInstancePosition node in cellObjects) {
                    if (node.Instance is ElementInstanceNode)
                        count++;
                    else if (node.Instance != null)
                        count += ((GroupNode)node.Instance).GroupElements.Count;
                }
                return count;
            }
        }
        #endregion

        #region Public Methods

        public void AddCellObject(AbsoluteInstancePosition instance) 
        {
            if(!cellObjects.Contains(instance))
            {
                cellObjects.Add(instance);
                instance.Children.Add(this);
            }
        }

        public void RemoveCellObject(AbsoluteInstancePosition instance)
        {
            if(cellObjects.Contains(instance))
            {
                cellObjects.Remove(instance);
                instance.Children.Remove(this);
            }
        }
        #endregion

        #region InstanceNode Members
        public override void DrawIn(Canvas canvas, double xTranslation, double yTranslation) {
            foreach (AbsoluteInstancePosition n in cellObjects) {
                n.DrawIn(canvas, xTranslation, yTranslation);
            }
            if (commonElements!=null)
                commonElements.DrawIn(canvas, xTranslation, yTranslation);
        }
        #endregion

        #region Node Members
        public override string ToString(StopAt stopAt) {
            String result = "Cell " + CellNumber + ": " + AttributesToString(stopAt) + " Elements: {";

            foreach (Node n in cellObjects)
                result += "\n" + n.ToString(stopAt);
            result += "}";
            return result;
        }
        #endregion

        #region Properties
        public int CellNumber
        {
            get { return cellNumber; }
        }

        public List<AbsoluteInstancePosition> CellObjects
        {
            get { return cellObjects; }
        }
        #endregion

        public static CommonAbsolutePositionNode CommonElements
        {
            get{return commonElements;}
            set { commonElements = value; }
        }
    }
}
