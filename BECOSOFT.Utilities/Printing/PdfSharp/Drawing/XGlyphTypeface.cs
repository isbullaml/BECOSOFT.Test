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
using BECOSOFT.Utilities.Printing.PdfSharp.Internal;
using System;
using System.Diagnostics;
using System.Globalization;
using GdiFont = System.Drawing.Font;
using GdiFontStyle = System.Drawing.FontStyle;

namespace BECOSOFT.Utilities.Printing.PdfSharp.Drawing {
    /// <summary>
    /// Specifies a physical font face that corresponds to a font file on the disk or in memory.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal sealed class XGlyphTypeface {
        // Implementation Notes
        // XGlyphTypeface is the centerpiece for font management. There is a one to one relationship
        // between XFont an XGlyphTypeface.
        //
        // * Each XGlyphTypeface can belong to one or more XFont objects.
        // * An XGlyphTypeface hold an XFontFamily.
        // * XGlyphTypeface hold a reference to an OpenTypeFontface. 
        // * 
        //

        private const string KeyPrefix = "tk:"; // "typeface key"

        private XGlyphTypeface(string key, XFontFamily fontFamily, XFontSource fontSource, XStyleSimulations styleSimulations, GdiFont gdiFont) {
            _key = key;
            _fontFamily = fontFamily;
            _fontSource = fontSource;

            _fontface = OpenTypeFontface.CetOrCreateFrom(fontSource);
            Debug.Assert(ReferenceEquals(_fontSource.Fontface, _fontface));

            _gdiFont = gdiFont;

            _styleSimulations = styleSimulations;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XGlyphTypeface"/> class by a font source.
        /// </summary>
        public XGlyphTypeface(XFontSource fontSource) {
            var familyName = fontSource.Fontface.name.Name;
            _fontFamily = new XFontFamily(familyName, false);
            _fontface = fontSource.Fontface;
            _isBold = _fontface.os2.IsBold;
            _isItalic = _fontface.os2.IsItalic;

            _key = ComputeKey(familyName, _isBold, _isItalic);
            //_fontFamily =xfont  FontFamilyCache.GetFamilyByName(familyName);
            _fontSource = fontSource;

            Initialize();
        }

        public static XGlyphTypeface GetOrCreateFrom(string familyName, FontResolvingOptions fontResolvingOptions) {
            // Check cache for requested type face.
            var typefaceKey = ComputeKey(familyName, fontResolvingOptions);
            XGlyphTypeface glyphTypeface;
            try {
                // Lock around TryGetGlyphTypeface and AddGlyphTypeface.
                Lock.EnterFontFactory();
                if (GlyphTypefaceCache.TryGetGlyphTypeface(typefaceKey, out glyphTypeface)) {
                    // Just return existing one.
                    return glyphTypeface;
                }

                // Resolve typeface by FontFactory.
                var fontResolverInfo = FontFactory.ResolveTypeface(familyName, fontResolvingOptions, typefaceKey);
                if (fontResolverInfo == null) {
                    // No fallback - just stop.
                    throw new InvalidOperationException("No appropriate font found.");
                }

                GdiFont gdiFont = null;
                // Now create the font family at the first.
                XFontFamily fontFamily;
                var platformFontResolverInfo = fontResolverInfo as PlatformFontResolverInfo;
                if (platformFontResolverInfo != null) {
                    // Case: fontResolverInfo was created by platform font resolver
                    // and contains platform specific objects that are reused.
                    // Reuse GDI+ font from platform font resolver.
                    gdiFont = platformFontResolverInfo.GdiFont;
                    fontFamily = XFontFamily.GetOrCreateFromGdi(gdiFont);
                } else {
                    // Case: fontResolverInfo was created by custom font resolver.

                    // Get or create font family for custom font resolver retrieved font source.
                    fontFamily = XFontFamily.GetOrCreateFontFamily(familyName);
                }

                // We have a valid font resolver info. That means we also have an XFontSource object loaded in the cache.
                var fontSource = FontFactory.GetFontSourceByFontName(fontResolverInfo.FaceName);
                Debug.Assert(fontSource != null);

                // Each font source already contains its OpenTypeFontface.
                glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource, fontResolverInfo.StyleSimulations, gdiFont);
                GlyphTypefaceCache.AddGlyphTypeface(glyphTypeface);
            } finally { Lock.ExitFontFactory(); }
            return glyphTypeface;
        }

        public static XGlyphTypeface GetOrCreateFromGdi(GdiFont gdiFont) {
            // $TODO THHO Lock???
            var typefaceKey = ComputeKey(gdiFont);
            XGlyphTypeface glyphTypeface;
            if (GlyphTypefaceCache.TryGetGlyphTypeface(typefaceKey, out glyphTypeface)) {
                // We have the glyph typeface already in cache.
                return glyphTypeface;
            }

            var fontFamily = XFontFamily.GetOrCreateFromGdi(gdiFont);
            var fontSource = XFontSource.GetOrCreateFromGdi(typefaceKey, gdiFont);

            // Check if styles must be simulated.
            var styleSimulations = XStyleSimulations.None;
            if (gdiFont.Bold && !fontSource.Fontface.os2.IsBold) {
                styleSimulations |= XStyleSimulations.BoldSimulation;
            }
            if (gdiFont.Italic && !fontSource.Fontface.os2.IsItalic) {
                styleSimulations |= XStyleSimulations.ItalicSimulation;
            }

            glyphTypeface = new XGlyphTypeface(typefaceKey, fontFamily, fontSource, styleSimulations, gdiFont);
            GlyphTypefaceCache.AddGlyphTypeface(glyphTypeface);

            return glyphTypeface;
        }

        public XFontFamily FontFamily => _fontFamily;
        private readonly XFontFamily _fontFamily;

        internal OpenTypeFontface Fontface => _fontface;
        private readonly OpenTypeFontface _fontface;

        public XFontSource FontSource => _fontSource;
        private readonly XFontSource _fontSource;

        private void Initialize() {
            _familyName = _fontface.name.Name;
            if (string.IsNullOrEmpty(_faceName) || _faceName.StartsWith("?")) {
                _faceName = _familyName;
            }
            _styleName = _fontface.name.Style;
            _displayName = _fontface.name.FullFontName;
            if (string.IsNullOrEmpty(_displayName)) {
                _displayName = _familyName;
                if (string.IsNullOrEmpty(_styleName)) {
                    _displayName += " (" + _styleName + ")";
                }
            }

            // Bold, as defined in OS/2 table.
            _isBold = _fontface.os2.IsBold;
            // Debug.Assert(_isBold == (_fontface.os2.usWeightClass > 400), "Check font weight.");

            // Italic, as defined in OS/2 table.
            _isItalic = _fontface.os2.IsItalic;
        }

        /// <summary>
        /// Gets the name of the font face. This can be a file name, an uri, or a GUID.
        /// </summary>
        internal string FaceName => _faceName;

        private string _faceName;

        /// <summary>
        /// Gets the English family name of the font, for example "Arial".
        /// </summary>
        public string FamilyName => _familyName;

        private string _familyName;

        /// <summary>
        /// Gets the English subfamily name of the font,
        /// for example "Bold".
        /// </summary>
        public string StyleName => _styleName;

        private string _styleName;

        /// <summary>
        /// Gets the English display name of the font,
        /// for example "Arial italic".
        /// </summary>
        public string DisplayName => _displayName;

        private string _displayName;

        /// <summary>
        /// Gets a value indicating whether the font weight is bold.
        /// </summary>
        public bool IsBold => _isBold;

        private bool _isBold;

        /// <summary>
        /// Gets a value indicating whether the font style is italic.
        /// </summary>
        public bool IsItalic => _isItalic;

        private bool _isItalic;

        public XStyleSimulations StyleSimulations => _styleSimulations;
        private XStyleSimulations _styleSimulations;

        /// <summary>
        /// Gets the suffix of the face name in a PDF font and font descriptor.
        /// The name based on the effective value of bold and italic from the OS/2 table.
        /// </summary>
        private string GetFaceNameSuffix() {
            // Use naming of Microsoft Word.
            if (IsBold) {
                return IsItalic ? ",BoldItalic" : ",Bold";
            }
            return IsItalic ? ",Italic" : "";
        }

        internal string GetBaseName() {
            var name = DisplayName;
            var ich = name.IndexOf("bold", StringComparison.OrdinalIgnoreCase);
            if (ich > 0) {
                name = name.Substring(0, ich) + name.Substring(ich + 4, name.Length - ich - 4);
            }
            ich = name.IndexOf("italic", StringComparison.OrdinalIgnoreCase);
            if (ich > 0) {
                name = name.Substring(0, ich) + name.Substring(ich + 6, name.Length - ich - 6);
            }
            //name = name.Replace(" ", "");
            name = name.Trim();
            name += GetFaceNameSuffix();
            return name;
        }

        /// <summary>
        /// Computes the bijective key for a typeface.
        /// </summary>
        internal static string ComputeKey(string familyName, FontResolvingOptions fontResolvingOptions) {
            // Compute a human readable key.
            var simulationSuffix = "";
            if (fontResolvingOptions.OverrideStyleSimulations) {
                switch (fontResolvingOptions.StyleSimulations) {
                    case XStyleSimulations.BoldSimulation:
                        simulationSuffix = "|b+/i-";
                        break;
                    case XStyleSimulations.ItalicSimulation:
                        simulationSuffix = "|b-/i+";
                        break;
                    case XStyleSimulations.BoldItalicSimulation:
                        simulationSuffix = "|b+/i+";
                        break;
                    case XStyleSimulations.None: break;
                    default: throw new ArgumentOutOfRangeException("fontResolvingOptions");
                }
            }
            var key = KeyPrefix + familyName.ToLowerInvariant()
                                + (fontResolvingOptions.IsItalic ? "/i" : "/n")          // normal / oblique / italic  
                                + (fontResolvingOptions.IsBold ? "/700" : "/400") + "/5" // Stretch.Normal
                                + simulationSuffix;
            return key;
        }

        /// <summary>
        /// Computes the bijective key for a typeface.
        /// </summary>
        internal static string ComputeKey(string familyName, bool isBold, bool isItalic) {
            return ComputeKey(familyName, new FontResolvingOptions(FontHelper.CreateStyle(isBold, isItalic)));
        }

        internal static string ComputeKey(GdiFont gdiFont) {
            var name1 = gdiFont.Name;
            var name2 = gdiFont.OriginalFontName;
            var name3 = gdiFont.SystemFontName;

            var name = name1;
            var style = gdiFont.Style;

            var key = KeyPrefix + name.ToLowerInvariant() + ((style & GdiFontStyle.Italic) == GdiFontStyle.Italic ? "/i" : "/n") + ((style & GdiFontStyle.Bold) == GdiFontStyle.Bold ? "/700" : "/400") + "/5"; // Stretch.Normal
            return key;
        }

        public string Key => _key;
        private readonly string _key;

        internal GdiFont GdiFont => _gdiFont;

        private readonly GdiFont _gdiFont;

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        internal string DebuggerDisplay => string.Format(CultureInfo.InvariantCulture, "{0} - {1} ({2})", FamilyName, StyleName, FaceName); // ReSharper restore UnusedMember.Local
    }
}