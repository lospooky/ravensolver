using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TreeStructures
{
    public class GroupNode : InstanceNode, IGroupNode, IEquatable<GroupNode>
    {
        protected List<RelativeInstancePosition> groupElements = new List<RelativeInstancePosition>();

        #region Constructors
        public GroupNode() {
        }
        public GroupNode(List<RelativeInstancePosition> groupElements)
        {
            AddElements(groupElements);
        }

        #endregion

        #region Public Methods

        public void AddElements(RelativeInstancePosition element)
        {
            if(!GroupElements.Contains(element))
            {
                groupElements.Add(element);
                element.AddChildren(this);
            }
        }

        public void AddElements(List<RelativeInstancePosition> elements)
        {
            foreach (RelativeInstancePosition el in elements)
                AddElements(el);
        }


        #endregion


        #region Properties
        public List<RelativeInstancePosition> GroupElements
        {
            get { return groupElements; }
        }

        #endregion


        #region Node Members
        public override string ToString(StopAt stopAt) {
            String result = "GroupNode: ";
            if (attributes.Count != 0) {
                result += "Attributes: {";
                foreach (AttributeNode n in attributes)
                    result += n.ToString(stopAt) + ", ";
                result = result.Remove(result.Length - 2) + "}, ";
            }
            result += "GroupElements: {";
            foreach (RelativeInstancePosition n in groupElements)
                result += n.ToString(stopAt);
            result = result.Remove(result.Length - 2) + "}";
            return result;
        }
        #endregion

        #region IVisualNode Members

        public override void DrawIn(Canvas canvas, double xTranslation, double yTranslation) {
            foreach (RelativeInstancePosition n in groupElements) {
                n.DrawIn(canvas, xTranslation, yTranslation);
            }
        }


        #endregion

        #region IEquatable<GroupNode> Members

        bool IEquatable<GroupNode>.Equals(GroupNode other) {
            if (this.GroupElements.Count != other.GroupElements.Count)
                return false;
            foreach (RelativeInstancePosition member in this.GroupElements) {
                if (!other.GroupElements.Contains(member))
                    return false;
            }
            return true;
        }

        #endregion
    }
}
