using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace TreeStructures
{
    public enum StopAt
    {
        None,
        AttributeValueNode,
        CommonAttributeNode,
        CommonElementsNode,
        ElementModelNode,
        ElementInstanceNode,
        GroupModelNode,
        GroupInstanceNode,
        IGroupNode,
        IModelNode,
        Leaf,
        CellNode,
        Here
    }

    public abstract class Node
    {
        protected List<Node> children = new List<Node>();
        protected List<AttributeNode> attributes = new List<AttributeNode>();


        #region Constructors
        public Node(AttributeNode attributeParent) {
            this.AddAttributes(attributeParent);
        }

        public Node(List<AttributeNode> attributeParents) {
            this.AddAttributes(attributeParents);
        }

        public Node() {
        }
        #endregion

         
        #region Abstract Methods
        public abstract String ToString(StopAt stopAt);
        public sealed override String ToString() { return this.ToString(StopAt.None); }
        #endregion

        #region Private Methods
        private Boolean StopHere(StopAt stopAt) {
            switch (stopAt) {
                case StopAt.Here:
                    return true;
                case StopAt.None:
                    return false;
                case StopAt.Leaf:
                    throw new NotImplementedException();
                default:
                    Type t = Type.GetType(stopAt.ToString(), true, true);
                    return (t == this.GetType() || this.GetType().IsSubclassOf(t));
            }

        }
        #endregion

        #region Protected Methods
        protected String AttributesToString(StopAt stopAt) {
            String result = "";
            if (attributes.Count != 0) {
                result += attributes.Count + " Attributes: {";
                foreach (Node n in attributes)
                    result += n.ToString(stopAt) + ", ";
                result = result.Remove(result.Length - 2) + "} ";
            }
            return result;
        }
        #endregion

        #region Public Methods

        public virtual bool SameAttributesAs(Node other) {
            if (this.Attributes.Count != other.Attributes.Count)
                return false;
            else {
                foreach (AttributeNode attribute in other.Attributes)
                    if (!this.Attributes.Contains(attribute))
                        return false;
            }

            return true;
        }

        public void AddAttributes(AttributeNode attribute) {
            if (Attributes.Count > 10) { }
                //throw new Exception("hmm");
            if (!Attributes.Contains(attribute))
            {
                Attributes.Add(attribute);
                attribute.AddChildren(this);
            }
        }

        public void AddAttributes(List<AttributeNode> attributes) {
            foreach (AttributeNode n in attributes)
                AddAttributes(n);
        }

        public void RemoveAttributes(AttributeNode attribute)
        {
            if (Attributes.Contains(attribute))
            {
                Attributes.Remove(attribute);
                attribute.RemoveChildren(this);
            }
        }

        public void RemoveAttributes(List<AttributeNode> attributes)
        {
            for(int i = attributes.Count - 1; i>=0; i--)
            //foreach (AttributeNode attribute in attributes)
                RemoveAttributes(attributes[i]);
        }

        public void AddChildren(Node child)
        {
            if (!Children.Contains(child))
                Children.Add(child);
        }

        public void AddChildren(List<Node> children)
        {
            foreach (Node c in children)
                AddChildren(c);
        }

        public virtual void RemoveChildren(Node child)
        {
            if(Children.Contains(child))
                Children.Remove(child);
        }

        public void RemoveChildren(List<Node> children)
        {
            foreach (Node child in children)
                RemoveChildren(child);
        }


        public Object GetAttribute(Tree.AttributeNames key) {
            AttributeNode attrib = attributes.FirstOrDefault(attr => attr.Key == key);
            Object attribO = (attrib == null) ? null : attrib.Value;
            if (attribO == null && this is IHasCommonAttributes) {
                attribO = ((IHasCommonAttributes)this).CommonAttributes.GetAttribute(key);
            }
            return attribO;
        }

        public List<Object> GetAttributes(Tree.AttributeNames key) {
            List<Object> attrib = (from a in attributes where a.Key == key select a.Value).ToList();

            if (attrib.Count==0 && this is IHasCommonAttributes) {
                return ((IHasCommonAttributes)this).CommonAttributes.GetAttributes(key);
            }
            return attrib;
        }

        public AttributeNode GetAttributeNode(Tree.AttributeNames key) {
            AttributeNode attrib = attributes.FirstOrDefault(attr => attr.Key == key);

            if (attrib == null && this is IHasCommonAttributes) {
                attrib = ((IHasCommonAttributes)this).CommonAttributes.GetAttributeNode(key);
            }
            return attrib;
        }

        public Boolean HasAttribute(Tree.AttributeNames key, Object value) {
            bool ret = (attributes.Any(attr => attr.Key == key && attr.Value == value));
            if (ret == false && this is IHasCommonAttributes) {
                ret = ((IHasCommonAttributes)this).CommonAttributes.HasAttribute(key,value);
            }
            return ret;
        }

        public Boolean HasAttribute(Tree.AttributeNames key) {
            bool ret = (attributes.Any(attr => attr.Key == key));
            if (ret == false && this is IHasCommonAttributes) {
                ret = ((IHasCommonAttributes)this).CommonAttributes.HasAttribute(key);
            }
            return ret;
        }
        #endregion


        #region Properties

        public List<Node> Children {
            get { return children; }
            set { this.children = value; }
        }


        public virtual List<AttributeNode> Attributes {
            get { return attributes; }
            set { this.attributes = value; }
        }

        public virtual bool isLeaf {
            get {
                if (Children.Count == 0)
                    return true;
                else
                    return false;
            }
        }

        public bool isRoot {
            get {
                if (Attributes.Count == 0 && (this is AttributeNode || this is CommonAttributesNode))
                    return true;
                else
                    return false;
            }
        }

        public virtual bool isOrphan {
            get {
                if (Attributes.Count == 0 && (this is ElementInstanceNode || this is GroupNode))
                    return true;
                else
                    return false;
            }
        }

        #endregion


    }
}
