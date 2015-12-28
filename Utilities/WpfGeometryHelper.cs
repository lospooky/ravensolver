using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Utilities
{
    public class WpfGeometryHelper
    {
        public Point center;
        public Rect boundingBox;
        public PointCollection vertexPoints;
        public PathGeometry pathGeometry;

        public WpfGeometryHelper(PointCollection vertexPoints) {
            if (vertexPoints.Count == 0) {
                Logging.logError("Node does not have any vertices!");
                this.vertexPoints = new PointCollection();
                this.boundingBox = new Rect(0, 0, 0, 0);
                return;
            }

            double minX = Double.MaxValue;
            double minY = Double.MaxValue;
            double maxX = Double.MinValue;
            double maxY = Double.MinValue;

            foreach (Point p in vertexPoints) {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            double x = minX;
            double y = minY;
            double w = maxX - minX;
            double h = maxY - minY;
            boundingBox = scaleBackRect(x, y, w, h);

            this.vertexPoints = new PointCollection(vertexPoints.Count);
            foreach (Point p in vertexPoints) {
                x = p.X * 200.0 + 100;
                y = p.Y * 200.0 + 100;
                this.vertexPoints.Add(new Point(x, y));
            }
        }


        public WpfGeometryHelper(FrameworkElement el) {
            double x = Double.IsNaN(Canvas.GetLeft(el)) ? 0 :  Canvas.GetLeft(el);
            double y = Double.IsNaN(Canvas.GetTop(el)) ? 0 : Canvas.GetTop(el);
            double w = Double.IsNaN(el.Width) ? 0 : el.Width;
            double h = Double.IsNaN(el.Height) ? 0 : el.Height;

            //TODO: Improve path handling and add similair attributes to all shapes
            //TODO: Add support for rotation transform

            if (el is Rectangle || el is Ellipse) {
                boundingBox = scaleRect(x,y,w,h);
                center = getCenter(boundingBox);
                boundingBox.Offset(-center.X, -center.Y);
                
                vertexPoints = new PointCollection(4);
                vertexPoints.Add(boundingBox.TopLeft);
                vertexPoints.Add(boundingBox.BottomLeft);
                vertexPoints.Add(boundingBox.BottomRight);
                vertexPoints.Add(boundingBox.TopRight);
            }
            else if (el is Line) {
                Line l = (Line)el;
                x = x + Math.Min(l.X1, l.X2);
                y = y + Math.Min(l.Y1, l.Y2);
                w = Math.Abs(l.X1 - l.X2);
                h = Math.Abs(l.Y1 - l.Y2);
                
                boundingBox = scaleRect(x, y, w, h);
                center = getCenter(boundingBox);
                boundingBox.Offset(-center.X, -center.Y);
                

                vertexPoints = new PointCollection(2);

                Point p1 = new Point(l.X1, l.Y1);
                Point p2 = new Point(l.X2, l.Y2);
                p1.X = ((p1.X - 100) / 200.0);
                p1.Y = ((p1.Y - 100) / 200.0);
                p2.X = ((p2.X - 100) / 200.0);
                p2.Y = ((p2.Y - 100) / 200.0);

                p1.Offset(-center.X, -center.Y);
                p2.Offset(-center.X, -center.Y);

                vertexPoints.Add(p1);
                vertexPoints.Add(p2);

            }
            else if (el is Polygon) {
                double cX = x;
                double cY = y;
                double minX = Double.MaxValue;
                double minY = Double.MaxValue;
                double maxX = Double.MinValue;
                double maxY = Double.MinValue;

                Polygon pol = (Polygon)el;

                foreach (Point p in pol.Points) {
                    if (p.X < minX) minX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y > maxY) maxY = p.Y;
                }

                x += minX;
                y += minY;
                w = maxX - minX;
                h = maxY - minY;
                boundingBox = scaleRect(x, y, w, h);
                center = getCenter(boundingBox);
                boundingBox.Offset(-center.X, -center.Y);

                vertexPoints = new PointCollection(pol.Points.Count);
                foreach (Point p in pol.Points) {
                    x = ((p.X + cX - 100) / 200.0);
                    y = ((p.Y + cY - 100) / 200.0);
                    vertexPoints.Add(new Point(x-center.X, y-center.Y));
                }
            }
            else if (el is Path) {
                Path p = (Path)el;
                

                boundingBox = scaleRect(p.Data.Bounds.Left, p.Data.Bounds.Top,p.Data.Bounds.Width,p.Data.Bounds.Height);


                if (!(p.Data is PathGeometry)) {
                    throw new Exception("Paths needs to be specified as PathGeometries (retains paths), not StreamGeometries (binarizes data). Compare D02 vs D02_old for details.");
                }

                PathGeometry pg = (PathGeometry)p.Data;
                pathGeometry = pg.Clone();


                PointCollection tempPoints = new PointCollection();

                vertexPoints = new PointCollection();
                foreach (PathFigure pf in pg.Figures) {
                    Logging.logInfo("Closed: " + pf.IsClosed + "\n" + pf.ToString());
                    vertexPoints.Add(pf.StartPoint);
                    foreach (PathSegment ps in pf.Segments) {
                        Logging.logInfo(ps.GetType() + ": " + ps.ToString());
                        if (ps is ArcSegment) {
                            ArcSegment arc = (ArcSegment)ps;
                            tempPoints.Add(arc.Point);
                        }
                        else if (ps is BezierSegment) {
                            //TODO: Should we add Bezier control points also?
                            BezierSegment bez = (BezierSegment) ps;
                            tempPoints.Add(bez.Point3);
                        }
                        else if (ps is LineSegment) {
                            LineSegment ls = (LineSegment)ps;
                            tempPoints.Add(ls.Point);
                        }
                        else if (ps is PolyBezierSegment) {
                            PolyBezierSegment pBez = (PolyBezierSegment)ps;
                            for (int i = 0; i < pBez.Points.Count; i++) {
                                if ((i + 1) % 3 == 0)
                                    tempPoints.Add(pBez.Points[i]);
                            }
                        }
                        else if (ps is PolyLineSegment) {
                            PolyLineSegment pls = (PolyLineSegment)ps;
                            foreach (Point pt in pls.Points) {
                                tempPoints.Add(pt);
                            }
                        }
                        else if (ps is PolyQuadraticBezierSegment) {
                            PolyQuadraticBezierSegment pqBez = (PolyQuadraticBezierSegment)ps;
                            for (int i = 0; i < pqBez.Points.Count; i++) {
                                if ((i + 1) % 2 == 0)
                                    tempPoints.Add(pqBez.Points[i]);
                            }
                        }
                        else if (ps is QuadraticBezierSegment) {
                            QuadraticBezierSegment qBez = (QuadraticBezierSegment)ps;
                            tempPoints.Add(qBez.Point2);
                        }
                        else {
                            throw new Exception("Type " + ps.GetType().ToString() + " cannot be handled!");
                        }
                    }
                    if (pf.IsClosed) {
                        tempPoints.RemoveAt(vertexPoints.Count - 1);
                    }
                }

                double cX = x;
                double cY = y;
                double minX = Double.MaxValue;
                double minY = Double.MaxValue;
                double maxX = Double.MinValue;
                double maxY = Double.MinValue;

                foreach (Point pt in tempPoints) {
                    if (pt.X < minX) minX = pt.X;
                    if (pt.Y < minY) minY = pt.Y;
                    if (pt.X > maxX) maxX = pt.X;
                    if (pt.Y > maxY) maxY = pt.Y;
                }
                
                x += minX;
                y += minY;
                w = maxX - minX;
                h = maxY - minY;
                boundingBox = scaleRect(x, y, w, h);
                center = getCenter(boundingBox);
                boundingBox.Offset(-center.X, -center.Y);


                foreach (Point pt in tempPoints) {
                    x = ((pt.X + cX - 100) / 200.0);
                    y = ((pt.Y + cY - 100) / 200.0);
                    vertexPoints.Add(new Point(x - center.X, y - center.Y));
                }
                
                Console.WriteLine("");
                //p2.Figures[0].Segments[0].
                
                
            }
            else {
                throw new NotImplementedException("Element is of type " + el.GetType().ToString() + ", which is not implemented in WpfGeometryHelper.");
            }
        }

        private static Point getCenter(Rect r) {
            return new Point(r.X + r.Width / 2.0, r.Y + r.Height / 2.0);
        }

        public Rect scaleRect(double x, double y, double w, double h) {
            w = (w / 200.0);
            h = (h / 200.0);
            x = ((x - 100) / 200.0);
            y = ((y - 100) / 200.0);
            
            return new Rect(x, y, w, h);
        }

        public Rect scaleBackRect(double x, double y, double w, double h) {
            w = w * 200.0;
            h = h * 200.0;
            x = x * 200.0 + 100;
            y = y * 200.0 + 100;
            return new Rect(x, y, w, h);
        }

        public static Point RoundPoint(ref Point p) {
            p.X = Math.Round(p.X, 3);
            p.Y = Math.Round(p.Y, 3);
            return p;
        }
    }
}
