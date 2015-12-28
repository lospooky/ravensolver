using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using Utilities;
using System.Windows.Media;
using System.Windows;

namespace TreeStructures
{
    public class ElementInstanceNode : InstanceNode, IHasCommonAttributes
    {
        private List<ElementModelNode> models;

        private static CommonAttributesNode commonAttributes;

        #region Constructors
        public ElementInstanceNode() {
            models = new List<ElementModelNode>();
        }
        public ElementInstanceNode(ElementModelNode elementModel) {
            models = new List<ElementModelNode>();
            models.Add(elementModel);
        }
        public ElementInstanceNode(List<ElementModelNode> elementModels) {
            models = elementModels;
        }
        #endregion

        #region Public Methods
        public void AddModels(ElementModelNode model)
        {
            if (!Models.Contains(model))
            {
                Models.Add(model);
                model.AddChildren(this);
            }
        }


        public Object GetModelAttribute(Tree.AttributeNames attribute) {
            return (from m in models select m.GetAttribute(attribute)).FirstOrDefault();
        }
        public List<Object> GetModelAttributes(Tree.AttributeNames attribute) {
            List<List<Object>> attrList = (from m in models select m.GetAttributes(attribute)).ToList();
            List<Object> ret = new List<Object>();
            foreach (List<Object> attr in attrList) {
                ret.AddRange(attr);
            }
            return ret;
        }
        public Object HasModelAttribute(Tree.AttributeNames attribute) {
            return models.Any(m => m.HasAttribute(attribute));
        }
        public Object HasModelAttribute(Tree.AttributeNames attribute, Object value) {
            return models.Any(m => m.HasAttribute(attribute, value));
        }


        public List<Object> GetModelAndOwnAttributes(Tree.AttributeNames attribute) {
            List<Object> ret = GetAttributes(attribute);
            ret.AddRange(GetModelAttributes(attribute));
            return ret;
        }


        public void AddModels(List<ElementModelNode> models)
        {
            foreach (ElementModelNode model in models)
                AddModels(model);
        }

        public void RemoveModels(ElementModelNode model)
        {
            if(Models.Contains(model))
            {
                Models.Remove(model);
                model.RemoveChildren(this);
            }
        }

        public void RemoveModels(List<ElementModelNode> models)
        {
            for (int i=models.Count-1; i>=0; i--) 
                RemoveModels(models[i]);
        }



        #endregion
        
        #region Properties
        public List<ElementModelNode> Models {
            get { return models; }
            protected set { models = value; }
        }
        #endregion

        public static CommonAttributesNode CommonAttributesSTA
        {
            get { return commonAttributes; }
            set { commonAttributes = value; }
        }

        #region IInstanceNode Members
        public override void DrawIn(Canvas canvas, double xTranslation, double yTranslation) {

            Shape el;
            WpfGeometryHelper gh;
            PointCollection vertexPoints = new PointCollection();
            List<AttributeNode> allAttributes = new List<AttributeNode>();
            
            
            allAttributes.AddRange(attributes);

            foreach (ElementModelNode model in models) {
                allAttributes.AddRange(model.Attributes);
            }

            allAttributes.AddRange(commonAttributes.Attributes);


            List<AttributeNode> vertexPointsAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.VertexPoints select attribute).ToList();
            foreach (AttributeNode p in vertexPointsAttribute) {
                Point pt = new Point(((Point)p.Value).X + xTranslation, ((Point)p.Value).Y + yTranslation);
                vertexPoints.Add(pt);
            }

            AttributeNode typeAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.Type select attribute).FirstOrDefault();
            if (typeAttribute == null) {
                throw new Exception("Cannot draw object without type");
            }
            switch ((String)typeAttribute.Value) {
                case "Ellipse":
                    el = new Ellipse();
                    gh = new WpfGeometryHelper(vertexPoints);
                    el.Height = gh.boundingBox.Height;
                    el.Width = gh.boundingBox.Width;
                    Canvas.SetLeft(el, gh.boundingBox.Left);
                    Canvas.SetTop(el, gh.boundingBox.Top);
                    canvas.Children.Add(el);
                    break;
                case "Rectangle":
                    el = new Rectangle();
                    gh = new WpfGeometryHelper(vertexPoints);
                    el.Height = gh.boundingBox.Height;
                    el.Width = gh.boundingBox.Width;
                    Canvas.SetLeft(el, gh.boundingBox.Left);
                    Canvas.SetTop(el, gh.boundingBox.Top);
                    canvas.Children.Add(el);
                    break;
                case "Line":
                    el = new Line();
                    gh = new WpfGeometryHelper(vertexPoints);
                    ((Line)el).X1 = gh.vertexPoints[0].X;
                    ((Line)el).Y1 = gh.vertexPoints[0].Y;
                    ((Line)el).X2 = gh.vertexPoints[1].X;
                    ((Line)el).Y2 = gh.vertexPoints[1].Y;
                    canvas.Children.Add(el);
                    break;
                case "Polygon":
                    el = new Polygon();
                    gh = new WpfGeometryHelper(vertexPoints);
                    ((Polygon)el).Points = gh.vertexPoints;
                    canvas.Children.Add(el);
                    break;
                case "Path":
                    el = new Path();
                    ((Path)el).Data = (PathGeometry)(from attribute in allAttributes where attribute.Key == Tree.AttributeNames.PathGeometry select attribute).First().Value;
                    canvas.Children.Add(el);
                    break;
                default:
                    Logging.logError("The type " + typeAttribute.Value.ToString() + " cannot be drawn in (Node.DrawIn)");
                    return;
            }
            
            AttributeNode strokeAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.StrokeBrush select attribute).FirstOrDefault();
            if (strokeAttribute != null)
                el.Stroke = (Brush)strokeAttribute.Value;
            
            AttributeNode strokeThicknessAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.StrokeThickness select attribute).FirstOrDefault();
            if (strokeThicknessAttribute != null)
                el.StrokeThickness = (double)strokeThicknessAttribute.Value;
            else {
                el.StrokeThickness = 1;
                el.Stroke = Brushes.Red;
            }

            AttributeNode strokeDashArrayAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.StrokeDashArray select attribute).FirstOrDefault();
            if (strokeDashArrayAttribute != null)
                el.StrokeDashArray = (DoubleCollection)strokeDashArrayAttribute.Value;

            AttributeNode strokeDashOffsetAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.StrokeDashOffset select attribute).FirstOrDefault();
            if (strokeDashOffsetAttribute != null)
                el.StrokeDashOffset = (double)strokeDashOffsetAttribute.Value;

            AttributeNode fillAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.FillBrush select attribute).FirstOrDefault();
            if (fillAttribute != null)
                el.Fill = (Brush)fillAttribute.Value;

            AttributeNode rotationAttribute = (from attribute in allAttributes where attribute.Key == Tree.AttributeNames.RotateTransformAngle select attribute).FirstOrDefault();
            if (rotationAttribute != null) {
                double rotationCenterX = (double)(from attribute in allAttributes where attribute.Key == Tree.AttributeNames.RotateTransformCenterX select attribute).First().Value;
                double rotationCenterY = (double)(from attribute in allAttributes where attribute.Key == Tree.AttributeNames.RotateTransformCenterY select attribute).First().Value;
                el.RenderTransform = new RotateTransform((double)rotationAttribute.Value, rotationCenterX, rotationCenterY);
            }

        }
        #endregion


        #region Node Members
        public override string ToString(StopAt stopAt) {
            String result = "ElementInstance: " + AttributesToString(stopAt);
            if (models.Count != 0) {
                result += "Models: {";
                foreach(ElementModelNode model in models)
                    result += model.ToString(stopAt) + ", ";
                result.Remove(result.Length-2);
            }
            return result;
        }
        #endregion

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
