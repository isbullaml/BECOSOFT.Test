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

using BECOSOFT.Utilities.Printing.Models;
using BECOSOFT.Utilities.Printing.PdfSharp.Drawing;
using BECOSOFT.Utilities.Printing.PdfSharp.Root.Enums;
using System;

namespace BECOSOFT.Utilities.Printing.PdfSharp.Root {
    /// <summary>
    /// Converter from <see cref="PdfPageSize"/> to <see cref="XSize"/>.
    /// </summary>
    public static class PageSizeConverter {
        public static XSize ToSize(PageSize value) {
            return new XSize(value.Width, value.Height);
        }
        /// <summary>
        /// Converts the specified page size enumeration to a pair of values in point.
        /// </summary>
        public static XSize ToSize(PdfPageSize value) {
            // The international definitions are:
            //   1 inch == 25.4 mm
            //   1 inch == 72 point
            switch (value) {
                // Source http://www.din-formate.de/reihe-a-din-groessen-mm-pixel-dpi.html
                case PdfPageSize.A0:
                    return new XSize(2384, 3370);

                case PdfPageSize.A1:
                    return new XSize(1684, 2384);

                case PdfPageSize.A2:
                    return new XSize(1191, 1684);

                case PdfPageSize.A3:
                    return new XSize(842, 1191);

                case PdfPageSize.A4:
                    return new XSize(595, 842);

                case PdfPageSize.A5:
                    return new XSize(420, 595);

                case PdfPageSize.RA0:
                    return new XSize(2438, 3458);

                case PdfPageSize.RA1:
                    return new XSize(1729, 2438);

                case PdfPageSize.RA2:
                    return new XSize(1219, 1729);

                case PdfPageSize.RA3:
                    return new XSize(865, 1219);

                case PdfPageSize.RA4:
                    return new XSize(609, 865);

                case PdfPageSize.RA5:
                    return new XSize(433, 609);

                case PdfPageSize.B0:
                    return new XSize(2835, 4008);

                case PdfPageSize.B1:
                    return new XSize(2004, 2835);

                case PdfPageSize.B2:
                    return new XSize(1417, 2004);

                case PdfPageSize.B3:
                    return new XSize(1001, 1417);

                case PdfPageSize.B4:
                    return new XSize(709, 1001);

                case PdfPageSize.B5:
                    return new XSize(499, 709);

                // The non-ISO sizes ...

                case PdfPageSize.Quarto: // 8 x 10 inch²
                    return new XSize(576, 720);

                case PdfPageSize.Foolscap: // 8 x 13 inch²
                    return new XSize(576, 936);

                case PdfPageSize.Executive: // 7.5 x 10 inch²
                    return new XSize(540, 720);

                case PdfPageSize.GovernmentLetter: // 8 x 10.5 inch²
                    return new XSize(576, 756);

                case PdfPageSize.Letter: // 8.5 x 11 inch²
                    return new XSize(612, 792);

                case PdfPageSize.Legal: // 8.5 x 14 inch²
                    return new XSize(612, 1008);

                case PdfPageSize.Ledger: // 17 x 11 inch²
                    return new XSize(1224, 792);

                case PdfPageSize.Tabloid: // 11 x 17 inch²
                    return new XSize(792, 1224);

                case PdfPageSize.Post: // 15.5 x 19.25 inch²
                    return new XSize(1126, 1386);

                case PdfPageSize.Crown: // 20 x 15 inch²
                    return new XSize(1440, 1080);

                case PdfPageSize.LargePost: // 16.5 x 21 inch²
                    return new XSize(1188, 1512);

                case PdfPageSize.Demy: // 17.5 x 22 inch²
                    return new XSize(1260, 1584);

                case PdfPageSize.Medium: // 18 x 23 inch²
                    return new XSize(1296, 1656);

                case PdfPageSize.Royal: // 20 x 25 inch²
                    return new XSize(1440, 1800);

                case PdfPageSize.Elephant: // 23 x 28 inch²
                    return new XSize(1565, 2016);

                case PdfPageSize.DoubleDemy: // 23.5 x 35 inch²
                    return new XSize(1692, 2520);

                case PdfPageSize.QuadDemy: // 35 x 45 inch²
                    return new XSize(2520, 3240);

                case PdfPageSize.STMT: // 5.5 x 8.5 inch²
                    return new XSize(396, 612);

                case PdfPageSize.Folio: // 8.5 x 13 inch²
                    return new XSize(612, 936);

                case PdfPageSize.Statement: // 5.5 x 8.5 inch²
                    return new XSize(396, 612);

                case PdfPageSize.Size10x14: // 10 x 14 inch²
                    return new XSize(720, 1008);
            }
            throw new ArgumentException("Invalid PageSize.", "value");
        }
    }
}