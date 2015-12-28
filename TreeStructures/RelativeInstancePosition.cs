using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utilities;

namespace TreeStructures
{
    public class RelativeInstancePosition : InstanceNode, IHasCommonAttributes
    {
        ElementInstanceNode instanceNode;

        private static CommonAttributesNode commonAttributes;

        public RelativeInstancePosition(ElementInstanceNode instanceNode):base() {
            this.instanceNode = instanceNode;
            instanceNode.AddChildren(this);
        }

        public override bool SameAttributesAs(Node other) {
            if (other is AbsoluteInstancePosition)
                if (((AbsoluteInstancePosition)other).Instance != this.Instance)
                    return false;
            return base.SameAttributesAs(other);
        }

        public ElementInstanceNode Instance {
            get { return instanceNode; }
            set { instanceNode = value; }
        }

        public static CommonAttributesNode CommonAttributesSTA {
            get { return commonAttributes; }
            set { commonAttributes = value; }
        }

        public Point RelativePosition {
            get {
                double? x = (double?)GetAttribute(Tree.AttributeNames.CenterXRelative);
                double? y = (double?)GetAttribute(Tree.AttributeNames.CenterYRelative);
                if (x == null)
                    x = 0;
                if (y == null)
                    y = 0;
                Point thisP = new Point(
                    x.Value,
                    y.Value
                );
                WpfGeometryHelper.RoundPoint(ref thisP);
                return thisP;
            }
        }

        public override void DrawIn(System.Windows.Controls.Canvas canvas, double xTranslation, double yTranslation) {
            double x;
            double y;
            x = (double)GetAttribute(Tree.AttributeNames.CenterXRelative);
            y = (double)GetAttribute(Tree.AttributeNames.CenterYRelative);
            instanceNode.DrawIn(canvas, xTranslation + x, yTranslation + y);
        }

        public override string ToString(StopAt stopAt) {
            double x;
            double y;
            x = (double)GetAttribute(Tree.AttributeNames.CenterXRelative);
            y = (double)GetAttribute(Tree.AttributeNames.CenterYRelative);
            
            x = Math.Round(x, 3);
            y = Math.Round(y, 3);
            return "Relative translation (" + x + ", " + y + ") of {" + instanceNode.ToString(stopAt) + "}";
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
