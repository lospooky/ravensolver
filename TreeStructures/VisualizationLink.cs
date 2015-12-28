using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TreeStructures
{
    public class VisualizationLink : IEquatable<VisualizationLink>
    {
        private Line line;
        private VisualizationNode v1;
        private VisualizationNode v2;

        private static Brush strokeBrush = Brushes.LightGray;
        private static double strokeThickness = 1;

        private static Brush hoverStrokeBrush = Brushes.Black;
        private static double hoverStrokeThickness = 2;

        public VisualizationLink(VisualizationNode vn1, VisualizationNode vn2) {
            if (vn1 == null || vn2 == null || vn1 == vn2)
                throw new ArgumentException("VisualisationNodes cannot be null or equal!");

            v1 = vn1;
            v2 = vn2;

            line = new Line();
            line.Stroke = strokeBrush;
            line.StrokeThickness = strokeThickness;
        }

        public void AddToNodes() {
            v1.VisualisationLinks.Add(this);
            v2.VisualisationLinks.Add(this);
            v1.MouseOver += new MouseHoverEvent(Node1MouseIn);
            v1.MouseOut += new MouseHoverEvent(Node1MouseOut);
            v2.MouseOver += new MouseHoverEvent(Node2MouseIn);
            v2.MouseOut += new MouseHoverEvent(Node2MouseOut);
        }

        void Node1MouseIn() {
            this.line.Stroke = hoverStrokeBrush;
            this.line.StrokeThickness = hoverStrokeThickness;
            Canvas.SetZIndex(this.line, 5);
            v2.OnCascadedMouseOver();
        }

        void Node1MouseOut() {
            this.line.Stroke = strokeBrush;
            this.line.StrokeThickness = strokeThickness;
            Canvas.SetZIndex(this.line, 0);
            v2.OnCascadedMouseOut();
        }

        void Node2MouseIn() {
            this.line.Stroke = hoverStrokeBrush;
            this.line.StrokeThickness = hoverStrokeThickness;
            Canvas.SetZIndex(this.line, 5);
            v1.OnCascadedMouseOver();
        }

        void Node2MouseOut() {
            this.line.Stroke = strokeBrush;
            this.line.StrokeThickness = strokeThickness;
            Canvas.SetZIndex(this.line, 0);
            v1.OnCascadedMouseOut();
        }


        public void DrawIn(Canvas c) {
            if (v1.Position.X < v2.Position.X) {
                line.X1 = v1.Position.X;
                line.Y1 = v1.Position.Y;
                line.X2 = v2.Position.X;
                line.Y2 = v2.Position.Y;
            }
            else {
                line.X2 = v1.Position.X;
                line.Y2 = v1.Position.Y;
                line.X1 = v2.Position.X;
                line.Y1 = v2.Position.Y;
            }
            line.X1 += VisualizeTree.labelWidth;
            line.Y1 += VisualizeTree.labelHeight / 2;
            line.Y2 += VisualizeTree.labelHeight / 2;
            if (!c.Children.Contains(line))
                c.Children.Add(line);
        }

        #region IEquatable<VisualizationLink> Members

        public bool Equals(VisualizationLink other) {
            if ((this.v1 == other.v1 || this.v2 == other.v1) &&
                (this.v1 == other.v2 || this.v2 == other.v2))
                return true;
            else
                return false;
        }

        #endregion

    }
}
