using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace TreeStructures
{
    public delegate void MouseHoverEvent();

    public class VisualizationNode
    {
        public event MouseHoverEvent MouseOver;
        public event MouseHoverEvent MouseOut;

        private Label label;
        private List<VisualizationNode> parents = new List<VisualizationNode>();
        private List<VisualizationNode> children = new List<VisualizationNode>();
        private Point p;
        private List<VisualizationLink> visualisationLinks = new List<VisualizationLink>();
        private VisualizeTree vt;

        private Boolean isIncluded = true;



        public VisualizationNode(VisualizeTree t, Label l) {
            Initialize(t, l);
        }


        public VisualizationNode(VisualizeTree t, Label l, Boolean isIncluded) {
            this.isIncluded = isIncluded;
            Initialize(t, l);
        }

        private void Initialize(VisualizeTree t, Label l) {
            l.Height = VisualizeTree.labelHeight;
            l.Width = VisualizeTree.labelWidth;
            l.BorderBrush = VisualizeTree.borderBrush;
            l.BorderThickness = VisualizeTree.borderThickness;
            l.Background = VisualizeTree.backgroundBrush;
            l.VerticalAlignment = VerticalAlignment.Center;
            l.HorizontalAlignment = HorizontalAlignment.Center;

            this.vt = t;
            label = l;
            l.MouseEnter += new System.Windows.Input.MouseEventHandler(l_MouseEnter);
            l.MouseLeave += new System.Windows.Input.MouseEventHandler(l_MouseLeave);
        }

        public virtual void OnMouseOver() {
            if (MouseOver != null)
                MouseOver();
        }
        public virtual void OnMouseOut() {
            if (MouseOut != null)
                MouseOut();
        }

        public void OnCascadedMouseOver() {
            label.BorderBrush = VisualizeTree.litBorderBrush;
        }

        public void OnCascadedMouseOut() {
            label.BorderBrush = VisualizeTree.borderBrush;
        }

        void l_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            label.BorderBrush = VisualizeTree.borderBrush;
            OnMouseOut();
        }

        void l_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            label.BorderBrush = VisualizeTree.hoverBorderBrush;
            OnMouseOver();
        }

        public List<VisualizationLink> VisualisationLinks {
            get { return visualisationLinks; }
            set { visualisationLinks = value; }
        }

        public Boolean IsIncluded {
            get { return isIncluded; }
            set { isIncluded = value; }
        }

        public Label Label {
            get { return label; }
            set { label = value; }
        }

        public List<VisualizationNode> Parents {
            get { return parents; }
            set { parents = value; }
        }

        public List<VisualizationNode> Children {
            get { return children; }
            set { children = value; }
        }
        public Point Position {
            get { return p; }
            set { p = value; }
        }

        public void AddChild(VisualizationNode n) {
            n.Parents.Add(this);
            children.Add(n);
        }
        public void AddParent(VisualizationNode n) {
            n.Children.Add(this);
            parents.Add(n);
            vt.AddVisualisationLink(this, n);
        }

        public void DrawIn(Canvas c) {
            Canvas.SetLeft(label, p.X);
            Canvas.SetTop(label, p.Y);
            c.Children.Add(label);
        }

        public void DrawIn(Canvas c, double x, double y) {
            p = new Point(x, y);
            DrawIn(c);
        }

    }
}
