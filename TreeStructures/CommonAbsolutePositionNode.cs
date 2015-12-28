using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
    public class CommonAbsolutePositionNode : AbsoluteInstancePosition
    {
        public CommonAbsolutePositionNode(InstanceNode instanceNode) : base(instanceNode) {}
        
        public CommonAbsolutePositionNode(AbsoluteInstancePosition absolutePositionNode) : base(absolutePositionNode.Instance) {
            absolutePositionNode.Instance.Children.Remove(absolutePositionNode.Instance);
            absolutePositionNode.Instance = null;

            this.InsertInCell(absolutePositionNode.Cells);
            foreach (CellNode cell in absolutePositionNode.Cells) {
                cell.AddCellObject(this);
                cell.RemoveCellObject(absolutePositionNode);
            }
            for (int i = absolutePositionNode.Attributes.Count - 1; i >= 0; i--) {
                this.AddAttributes(absolutePositionNode.Attributes[i]);
                absolutePositionNode.RemoveAttributes(absolutePositionNode.Attributes[i]);
            }
            AddChildren(absolutePositionNode.Children);
            
        }

        public override String ToString(StopAt stopAt) {
            return "Common" + base.ToString(stopAt);
        }
    }
}
