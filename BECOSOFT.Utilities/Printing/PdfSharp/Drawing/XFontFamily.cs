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
using BECOSOFT.Utilities.Printing.PdfSharp.Fonts;
using BECOSOFT.Utilities.Printing.PdfSharp.Fonts.OpenType;
using System;
using GdiFont = System.Drawing.Font;
using GdiFontFamily = System.Drawing.FontFamily;
using GdiFontStyle = System.Drawing.FontStyle;

namespace BECOSOFT.Utilities.Printing.PdfSharp.Drawing {
    /// <summary>
    /// Defines a group of typefaces having a similar basic design and certain variations in styles.
    /// </summary>
    public sealed class XFontFamily {
        /// <summary>
        /// Initializes a new instance of the <see cref="XFontFamily"/> class.
        /// </summary>
        /// <param name="familyName">The family name of a font.</param>
        public XFontFamily(string familyName) {
            FamilyInternal = FontFamilyInternal.GetOrCreateFromName(familyName, true);
        }

        internal XFontFamily(string familyName, bool createPlatformObjects) {
            FamilyInternal = FontFamilyInternal.GetOrCreateFromName(familyName, createPlatformObjects);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XFontFamily"/> class from FontFamilyInternal.
        /// </summary>
        private XFontFamily(FontFamilyInternal fontFamilyInternal) {
            FamilyInternal = fontFamilyInternal;
        }

        //public XFontFamily(GdiFontFamily gdiFontFamily)
        //{
        //    FamilyInternal = FontFamilyInternal.GetOrCreateFromGdi(gdiFontFamily);
        //}

        internal static XFontFamily CreateFromName_not_used(string name, bool createPlatformFamily) {
            var fontFamily = new XFontFamily(name);
            if (createPlatformFamily) {
                //fontFamily._gdiFamily = new System.Drawing.FontFamily(name);
            }
            return fontFamily;
        }

        /// <summary>
        /// An XGlyphTypeface for a font source that comes from a custom font resolver
        /// creates a solitary font family exclusively for it.
        /// </summary>
        internal static XFontFamily GetOrCreateFontFamily(string name) {
            // Custom font resolver face names must not clash with platform family names.
            var fontFamilyInternal = FontFamilyCache.GetFamilyByName(name);
            if (fontFamilyInternal == null) {
                fontFamilyInternal = FontFamilyInternal.GetOrCreateFromName(name, false);
                fontFamilyInternal = FontFamilyCache.CacheOrGetFontFamily(fontFamilyInternal);
            }

            // Create font family and save it in cache. Do not try to create platform objects.
            return new XFontFamily(fontFamilyInternal);
        }

        internal static XFontFamily GetOrCreateFromGdi(GdiFont font) {
            var fontFamilyInternal = FontFamilyInternal.GetOrCreateFromGdi(font.FontFamily);
            return new XFontFamily(fontFamilyInternal);
        }

        /// <summary>
        /// Gets the name of the font family.
        /// </summary>
        public string Name => FamilyInternal.Name;

        /// <summary>
        /// Returns the cell ascent, in design units, of the XFontFamily object of the specified style.
        /// </summary>
        public int GetCellAscent(XFontStyle style) {
            var descriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptor(Name, style);
            var result = descriptor.Ascender;
            return result;
        }

        /// <summary>
        /// Returns the cell descent, in design units, of the XFontFamily object of the specified style.
        /// </summary>
        public int GetCellDescent(XFontStyle style) {
            var descriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptor(Name, style);
            var result = descriptor.Descender;
            return result;
        }

        /// <summary>
        /// Gets the height, in font design units, of the em square for the specified style.
        /// </summary>
        public int GetEmHeight(XFontStyle style) {
            var descriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptor(Name, style);
            var result = descriptor.UnitsPerEm;
            return result;
        }

        /// <summary>
        /// Returns the line spacing, in design units, of the FontFamily object of the specified style.
        /// The line spacing is the vertical distance between the base lines of two consecutive lines of text.
        /// </summary>
        public int GetLineSpacing(XFontStyle style) {
            var descriptor = (OpenTypeDescriptor)FontDescriptorCache.GetOrCreateDescriptor(Name, style);
            var result = descriptor.LineSpacing;
            return result;
        }

        //public string GetName(int language);

        /// <summary>
        /// Indicates whether the specified FontStyle enumeration is available.
        /// </summary>
        public bool IsStyleAvailable(XFontStyle style) {
            var xStyle = (XGdiFontStyle)style & XGdiFontStyle.BoldItalic;
            if (GdiFamily != null) {
                return GdiFamily.IsStyleAvailable((GdiFontStyle)xStyle);
            }
            return false;
        }

        /// <summary>
        /// Returns an array that contains all the FontFamily objects associated with the current graphics context.
        /// </summary>
        [Obsolete("Use platform API directly.")]
        public static XFontFamily[] Families {
            get { throw new InvalidOperationException("Obsolete and not implemted any more."); }
        }

        /// <summary>
        /// Returns an array that contains all the FontFamily objects available for the specified 
        /// graphics context.
        /// </summary>
        [Obsolete("Use platform API directly.")]
        public static XFontFamily[] GetFamilies(XGraphics graphics) {
            throw new InvalidOperationException("Obsolete and not implemted any more.");
        }

        /// <summary>
        /// Gets the underlying GDI+ font family object.
        /// Is null if the font was created by a font resolver.
        /// </summary>
        internal GdiFontFamily GdiFamily => FamilyInternal.GdiFamily;

        /// <summary>
        /// The implementation sigleton of font family;
        /// </summary>
        internal FontFamilyInternal FamilyInternal;
    }
}