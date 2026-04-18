#region PDFsharp - A .NET library for processing PDF

//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using BECOSOFT.Utilities.Printing.PdfSharp.Drawing.Enums;
using BECOSOFT.Utilities.Printing.PdfSharp.Internal;
using BECOSOFT.Utilities.Printing.PdfSharp.Root;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BECOSOFT.Utilities.Printing.PdfSharp.Drawing {
    /// <summary>
    /// Represents a series of connected lines and curves.
    /// </summary>
    public sealed class XGraphicsPath {
        /// <summary>
        /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
        /// </summary>
        public XGraphicsPath() {
            try {
                Lock.EnterGdiPlus();
                _gdipPath = new GraphicsPath();
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
        /// </summary>
        public XGraphicsPath(PointF[] points, byte[] types, XFillMode fillMode) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath = new GraphicsPath(points, types, (FillMode)fillMode);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public XGraphicsPath Clone() {
            var path = (XGraphicsPath)MemberwiseClone();
            try {
                Lock.EnterGdiPlus();
                path._gdipPath = _gdipPath.Clone() as GraphicsPath;
            } finally { Lock.ExitGdiPlus(); }
            return path;
        }

        // ----- AddLine ------------------------------------------------------------------------------

        /// <summary>
        /// Adds a line segment to current figure.
        /// </summary>
        public void AddLine(Point pt1, Point pt2) {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        /// <summary>
        /// Adds  a line segment to current figure.
        /// </summary>
        public void AddLine(PointF pt1, PointF pt2) {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        /// <summary>
        /// Adds  a line segment to current figure.
        /// </summary>
        public void AddLine(XPoint pt1, XPoint pt2) {
            AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
        }

        /// <summary>
        /// Adds  a line segment to current figure.
        /// </summary>
        public void AddLine(double x1, double y1, double x2, double y2) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddLine((float)x1, (float)y1, (float)x2, (float)y2);
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddLines -----------------------------------------------------------------------------

        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(Point[] points) {
            AddLines(XGraphics.MakeXPointArray(points, 0, points.Length));
        }

        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(PointF[] points) {
            AddLines(XGraphics.MakeXPointArray(points, 0, points.Length));
        }

        /// <summary>
        /// Adds a series of connected line segments to current figure.
        /// </summary>
        public void AddLines(XPoint[] points) {
            if (points == null) {
                throw new ArgumentNullException("points");
            }

            var count = points.Length;
            if (count == 0) {
                return;
            }
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddLines(XGraphics.MakePointFArray(points));
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddBezier ----------------------------------------------------------------------------

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(Point pt1, Point pt2, Point pt3, Point pt4) {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4) {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(XPoint pt1, XPoint pt2, XPoint pt3, XPoint pt4) {
            AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
        }

        /// <summary>
        /// Adds a cubic Bézier curve to the current figure.
        /// </summary>
        public void AddBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddBezier((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3, (float)x4, (float)y4);
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddBeziers ---------------------------------------------------------------------------

        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(Point[] points) {
            AddBeziers(XGraphics.MakeXPointArray(points, 0, points.Length));
        }

        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(PointF[] points) {
            AddBeziers(XGraphics.MakeXPointArray(points, 0, points.Length));
        }

        /// <summary>
        /// Adds a sequence of connected cubic Bézier curves to the current figure.
        /// </summary>
        public void AddBeziers(XPoint[] points) {
            if (points == null) {
                throw new ArgumentNullException("points");
            }

            var count = points.Length;
            if (count < 4) {
                throw new ArgumentException("At least four points required for bezier curve.", "points");
            }

            if ((count - 1) % 3 != 0) {
                throw new ArgumentException("Invalid number of points for bezier curve. Number must fulfil 4+3n.",
                                            "points");
            }
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddBeziers(XGraphics.MakePointFArray(points));
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddCurve -----------------------------------------------------------------------

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(Point[] points) {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length));
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points) {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length));
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points) {
            AddCurve(points, 0.5);
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(Point[] points, double tension) {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points, double tension) {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points, double tension) {
            var count = points.Length;
            if (count < 2) {
                throw new ArgumentException("AddCurve requires two or more points.", "points");
            }
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddCurve(XGraphics.MakePointFArray(points), (float)tension);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(Point[] points, int offset, int numberOfSegments, float tension) {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), offset, numberOfSegments, tension);
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension) {
            AddCurve(XGraphics.MakeXPointArray(points, 0, points.Length), offset, numberOfSegments, tension);
        }

        /// <summary>
        /// Adds a spline curve to the current figure.
        /// </summary>
        public void AddCurve(XPoint[] points, int offset, int numberOfSegments, double tension) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddCurve(XGraphics.MakePointFArray(points), offset, numberOfSegments, (float)tension);
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddArc -------------------------------------------------------------------------------

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(Rectangle rect, double startAngle, double sweepAngle) {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(RectangleF rect, double startAngle, double sweepAngle) {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(XRect rect, double startAngle, double sweepAngle) {
            AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure.
        /// </summary>
        public void AddArc(double x, double y, double width, double height, double startAngle, double sweepAngle) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddArc((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds an elliptical arc to the current figure. The arc is specified WPF like.
        /// </summary>
        public void AddArc(XPoint point1, XPoint point2, XSize size, double rotationAngle, bool isLargeArg, XSweepDirection sweepDirection) {
            DiagnosticsHelper.HandleNotImplemented("XGraphicsPath.AddArc");
        }

        // ----- AddRectangle -------------------------------------------------------------------------

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(Rectangle rect) {
            AddRectangle(new XRect(rect));
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(RectangleF rect) {
            AddRectangle(new XRect(rect));
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(XRect rect) {
            try {
                Lock.EnterGdiPlus();
                // If rect is empty GDI+ removes the rect from the path.
                // This is not intended if the path is used for clipping.
                // See http://forum.pdfsharp.net/viewtopic.php?p=9433#p9433
                // _gdipPath.AddRectangle(rect.ToRectangleF());

                // Draw the rectangle manually.
                _gdipPath.StartFigure();
                _gdipPath.AddLines(new PointF[] { rect.TopLeft.ToPointF(), rect.TopRight.ToPointF(), rect.BottomRight.ToPointF(), rect.BottomLeft.ToPointF() });
                _gdipPath.CloseFigure();
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a rectangle to this path.
        /// </summary>
        public void AddRectangle(double x, double y, double width, double height) {
            AddRectangle(new XRect(x, y, width, height));
        }

        // ----- AddRectangles ------------------------------------------------------------------------

        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(Rectangle[] rects) {
            var count = rects.Length;
            for (var idx = 0; idx < count; idx++) {
                AddRectangle(rects[idx]);
            }

            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddRectangles(rects);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(RectangleF[] rects) {
            var count = rects.Length;
            for (var idx = 0; idx < count; idx++) {
                AddRectangle(rects[idx]);
            }

            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddRectangles(rects);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a series of rectangles to this path.
        /// </summary>
        public void AddRectangles(XRect[] rects) {
            var count = rects.Length;
            for (var idx = 0; idx < count; idx++) {
                try {
                    Lock.EnterGdiPlus();
                    _gdipPath.AddRectangle(rects[idx].ToRectangleF());
                } finally { Lock.ExitGdiPlus(); }
            }
        }

        // ----- AddRoundedRectangle ------------------------------------------------------------------

        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(Rectangle rect, Size ellipseSize) {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }

        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(RectangleF rect, SizeF ellipseSize) {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }

        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(XRect rect, SizeF ellipseSize) {
            AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, ellipseSize.Width, ellipseSize.Height);
        }

        /// <summary>
        /// Adds a rectangle with rounded corners to this path.
        /// </summary>
        public void AddRoundedRectangle(double x, double y, double width, double height, double ellipseWidth, double ellipseHeight) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.StartFigure();
                _gdipPath.AddArc((float)(x + width - ellipseWidth), (float)y, (float)ellipseWidth, (float)ellipseHeight, -90, 90);
                _gdipPath.AddArc((float)(x + width - ellipseWidth), (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 0, 90);
                _gdipPath.AddArc((float)x, (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 90, 90);
                _gdipPath.AddArc((float)x, (float)y, (float)ellipseWidth, (float)ellipseHeight, 180, 90);
                _gdipPath.CloseFigure();
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddEllipse ---------------------------------------------------------------------------

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(Rectangle rect) {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(RectangleF rect) {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(XRect rect) {
            AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Adds an ellipse to the current path.
        /// </summary>
        public void AddEllipse(double x, double y, double width, double height) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddEllipse((float)x, (float)y, (float)width, (float)height);
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddPolygon ---------------------------------------------------------------------------

        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(Point[] points) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddPolygon(points);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(PointF[] points) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddPolygon(points);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a polygon to this path.
        /// </summary>
        public void AddPolygon(XPoint[] points) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddPolygon(XGraphics.MakePointFArray(points));
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddPie -------------------------------------------------------------------------------

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(Rectangle rect, double startAngle, double sweepAngle) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddPie(rect, (float)startAngle, (float)sweepAngle);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(RectangleF rect, double startAngle, double sweepAngle) {
            AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(XRect rect, double startAngle, double sweepAngle) {
            AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
        }

        /// <summary>
        /// Adds the outline of a pie shape to this path.
        /// </summary>
        public void AddPie(double x, double y, double width, double height, double startAngle, double sweepAngle) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddPie((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddClosedCurve ------------------------------------------------------------------------

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(Point[] points) {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), 0.5);
        }

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(PointF[] points) {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), 0.5);
        }

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(XPoint[] points) {
            AddClosedCurve(points, 0.5);
        }

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(Point[] points, double tension) {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(PointF[] points, double tension) {
            AddClosedCurve(XGraphics.MakeXPointArray(points, 0, points.Length), tension);
        }

        /// <summary>
        /// Adds a closed curve to this path.
        /// </summary>
        public void AddClosedCurve(XPoint[] points, double tension) {
            if (points == null) {
                throw new ArgumentNullException("points");
            }
            var count = points.Length;
            if (count == 0) {
                return;
            }
            if (count < 2) {
                throw new ArgumentException("Not enough points.", "points");
            }

            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddClosedCurve(XGraphics.MakePointFArray(points), (float)tension);
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddPath ------------------------------------------------------------------------------

        /// <summary>
        /// Adds the specified path to this path.
        /// </summary>
        public void AddPath(XGraphicsPath path, bool connect) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddPath(path._gdipPath, connect);
            } finally { Lock.ExitGdiPlus(); }
        }

        // ----- AddString ----------------------------------------------------------------------------

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, Point origin, XStringFormat format) {
            AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
        }

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, PointF origin, XStringFormat format) {
            AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
        }

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, XPoint origin,
                              XStringFormat format) {
            try {
                if (family.GdiFamily == null) {
                    throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));
                }

                var p = origin.ToPointF();
                p.Y += SimulateBaselineOffset(family, style, emSize, format);

                try {
                    Lock.EnterGdiPlus();
                    _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, p, format.RealizeGdiStringFormat());
                } finally { Lock.ExitGdiPlus(); }
            } catch {
                throw;
            }
        }

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, Rectangle layoutRect, XStringFormat format) {
            if (family.GdiFamily == null) {
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));
            }

            var rect = new Rectangle(layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height);
            rect.Offset(new Point(0, (int)SimulateBaselineOffset(family, style, emSize, format)));

            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, rect, format.RealizeGdiStringFormat());
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, RectangleF layoutRect, XStringFormat format) {
            if (family.GdiFamily == null) {
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));
            }

            var rect = new RectangleF(layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height);
            rect.Offset(new PointF(0, SimulateBaselineOffset(family, style, emSize, format)));

            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Calculates the offset for BaseLine positioning simulation:
        /// In GDI we have only Near, Center and Far as LineAlignment and no BaseLine. For XLineAlignment.BaseLine StringAlignment.Near is returned.
        /// We now return the negative drawed ascender height.
        /// This has to be added to the LayoutRect/Origin before each _gdipPath.AddString().
        /// </summary>
        /// <param name="family"></param>
        /// <param name="style"></param>
        /// <param name="emSize"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private float SimulateBaselineOffset(XFontFamily family, XFontStyle style, double emSize, XStringFormat format) {
            var font = new XFont(family.Name, emSize, style);

            if (format.LineAlignment == XLineAlignment.BaseLine) {
                var lineSpace = font.GetHeight();
                var cellSpace = font.FontFamily.GetLineSpacing(font.Style);
                var cellAscent = font.FontFamily.GetCellAscent(font.Style);
                var cellDescent = font.FontFamily.GetCellDescent(font.Style);
                var cyAscent = lineSpace * cellAscent / cellSpace;
                cyAscent = lineSpace * font.CellAscent / font.CellSpace;
                return (float)-cyAscent;
            }
            return 0;
        }

        /// <summary>
        /// Adds a text string to this path.
        /// </summary>
        public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, XRect layoutRect,
                              XStringFormat format) {
            if (s == null) {
                throw new ArgumentNullException("s");
            }

            if (family == null) {
                throw new ArgumentNullException("family");
            }

            if (format == null) {
                format = XStringFormats.Default;
            }

            if (format.LineAlignment == XLineAlignment.BaseLine && layoutRect.Height != 0) {
                throw new InvalidOperationException(
                    "DrawString: With XLineAlignment.BaseLine the height of the layout rectangle must be 0.");
            }

            if (s.Length == 0) {
                return;
            }

            var font = new XFont(family.Name, emSize, style);
            //Gfx.DrawString(text, font.Realize_GdiFont(), brush.RealizeGdiBrush(), rect,
            //  format != null ? format.RealizeGdiStringFormat() : null);

            if (family.GdiFamily == null) {
                throw new NotFiniteNumberException(PSSR.NotImplementedForFontsRetrievedWithFontResolver(family.Name));
            }

            var rect = layoutRect.ToRectangleF();
            rect.Offset(new PointF(0, SimulateBaselineOffset(family, style, emSize, format)));

            try {
                Lock.EnterGdiPlus();
                _gdipPath.AddString(s, family.GdiFamily, (int)style, (float)emSize, rect, format.RealizeGdiStringFormat());
            } finally { Lock.ExitGdiPlus(); }
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Closes the current figure and starts a new figure.
        /// </summary>
        public void CloseFigure() {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.CloseFigure();
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Starts a new figure without closing the current figure.
        /// </summary>
        public void StartFigure() {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.StartFigure();
            } finally { Lock.ExitGdiPlus(); }
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets an XFillMode that determines how the interiors of shapes are filled.
        /// </summary>
        public XFillMode FillMode {
            get { return _fillMode; }
            set {
                _fillMode = value;
                try {
                    Lock.EnterGdiPlus();
                    _gdipPath.FillMode = (FillMode)value;
                } finally { Lock.ExitGdiPlus(); }
            }
        }

        private XFillMode _fillMode;

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten() {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.Flatten();
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten(XMatrix matrix) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.Flatten(matrix.ToGdiMatrix());
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
        /// </summary>
        public void Flatten(XMatrix matrix, double flatness) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.Flatten(matrix.ToGdiMatrix(), (float)flatness);
            } finally { Lock.ExitGdiPlus(); }
        }

        // --------------------------------------------------------------------------------------------

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.Widen(pen.RealizeGdiPen());
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen, XMatrix matrix) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix());
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
        /// by the specified pen.
        /// </summary>
        public void Widen(XPen pen, XMatrix matrix, double flatness) {
            try {
                Lock.EnterGdiPlus();
                _gdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix(), (float)flatness);
            } finally { Lock.ExitGdiPlus(); }
        }

        /// <summary>
        /// Grants access to internal objects of this class.
        /// </summary>
        public XGraphicsPathInternals Internals => new XGraphicsPathInternals(this);

        /// <summary>
        /// Gets access to underlying GDI+ graphics path.
        /// </summary>
        internal GraphicsPath _gdipPath;
    }
}