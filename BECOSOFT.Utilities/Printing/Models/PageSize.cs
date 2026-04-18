using BECOSOFT.Utilities.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Printing.Models {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PageSize : IEquatable<PageSize> {
        /// <summary>
        /// Paper Sizes are defined in hundredths of an inch.
        /// https://docs.microsoft.com/en-us/dotnet/api/system.drawing.printing.papersize
        /// </summary>
        private const int DefaultPrinterDpi = 100;
        private const int DefaultDpi = 300;
        private const double MmPerInch = 25.4;
        private const int A4PixelHeight = 3508;

        private string DebuggerDisplay => $"{MillimeterWidth}mm x {MillimeterHeight}mm - {Width}px x {Height}px (DPI: {Dpi})";

        /// <summary>
        /// A0 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A0 = new PageSize(841, 1189, paperSize: Models.PaperSize.A0);

        /// <summary>
        /// A1 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A1 = new PageSize(594, 841, paperSize: Models.PaperSize.A1);

        /// <summary>
        /// A2 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A2 = new PageSize(420, 594, paperSize: Models.PaperSize.A2);

        /// <summary>
        /// A3 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A3 = new PageSize(297, 420, paperSize: Models.PaperSize.A3);

        /// <summary>
        /// A4 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A4 = new PageSize(210, 297, paperSize: Models.PaperSize.A4);

        /// <summary>
        /// A5 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A5 = new PageSize(148, 210, paperSize: Models.PaperSize.A5);

        /// <summary>
        /// A6 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A6 = new PageSize(105, 148, paperSize: Models.PaperSize.A6);

        /// <summary>
        /// A7 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A7 = new PageSize(74, 105, paperSize: Models.PaperSize.A7);

        /// <summary>
        /// A8 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A8 = new PageSize(52, 74, paperSize: Models.PaperSize.A8);

        /// <summary>
        /// A9 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A9 = new PageSize(37, 52, paperSize: Models.PaperSize.A9);

        /// <summary>
        /// A10 portrait page size with 300 DPI.
        /// </summary>
        public static PageSize A10 = new PageSize(26, 37, paperSize: Models.PaperSize.A10);

        /// <summary>
        /// A0 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A0Landscape = new PageSize(1189, 841, paperSize: Models.PaperSize.A0);

        /// <summary>
        /// A1 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A1Landscape = new PageSize(841, 594, paperSize: Models.PaperSize.A1);

        /// <summary>
        /// A2 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A2Landscape = new PageSize(594, 420, paperSize: Models.PaperSize.A2);

        /// <summary>
        /// A3 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A3Landscape = new PageSize(420, 297, paperSize: Models.PaperSize.A3);

        /// <summary>
        /// A4 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A4Landscape = new PageSize(297, 210, paperSize: Models.PaperSize.A4);

        /// <summary>
        /// A5 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A5Landscape = new PageSize(210, 148, paperSize: Models.PaperSize.A5);

        /// <summary>
        /// A6 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A6Landscape = new PageSize(148, 105, paperSize: Models.PaperSize.A6);

        /// <summary>
        /// A7 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A7Landscape = new PageSize(105, 74, paperSize: Models.PaperSize.A7);

        /// <summary>
        /// A8 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A8Landscape = new PageSize(74, 52, paperSize: Models.PaperSize.A8);

        /// <summary>
        /// A9 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A9Landscape = new PageSize(52, 37, paperSize: Models.PaperSize.A9);

        /// <summary>
        /// A10 landscape page size with 300 DPI.
        /// </summary>
        public static PageSize A10Landscape = new PageSize(37, 26, paperSize: Models.PaperSize.A10);

        /// <summary>
        /// A collection of all defined <see cref="PageSize"/> objects
        /// </summary>
        public static List<PageSize> PageSizes = new List<PageSize> {
            A0,
            A1,
            A2,
            A3,
            A4,
            A5,
            A6,
            A7,
            A8,
            A9,
            A10,
            A0Landscape,
            A1Landscape,
            A2Landscape,
            A3Landscape,
            A4Landscape,
            A5Landscape,
            A6Landscape,
            A7Landscape,
            A8Landscape,
            A9Landscape,
            A10Landscape,
        };

        /// <summary>
        /// Width in pixels
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height in pixels
        /// </summary>
        public int Height { get; }

        public int Dpi { get; }

        public decimal? OffsetX { get; private set; }
        public decimal? OffsetY { get; private set; }

        public decimal? Gap { get; private set; }
        
        /// <summary>
        /// Indicates that the page size is an A-sized paper
        /// </summary>
        public bool IsStandardizedFormat { get; private set; }
        /// <summary>
        /// If <see cref="PaperSize"/> has a value, it indicates which A-sized paper this page size is.
        /// </summary>
        public PaperSize? PaperSize { get; private set; }

        /// <summary>
        /// Returns the orientation of the page size, based on the <see cref="Height"/> and the <see cref="Width"/>.
        /// </summary>
        public PaperOrientation Orientation => Height < Width ? PaperOrientation.Landscape : PaperOrientation.Portrait;

        /// <summary>
        /// Width in millimeter
        /// </summary>
        public int MillimeterWidth { get; }

        /// <summary>
        /// Height in millimeter
        /// </summary>
        public int MillimeterHeight { get; }

        /// <summary>
        /// Scale relative to <see cref="A4"/>
        /// </summary>
        public double Scale {
            get {
                if (!PageSizes.Contains(this)) {
                    return 1;
                }
                var scale = ((double)(Width < Height ? Height : Width)) / A4PixelHeight;
                return Math.Round(scale, 2, MidpointRounding.AwayFromZero);
            }
        }

        public bool IsFromPageDefinition { get; private set; }

        /// <summary>
        /// Constructs a <see cref="PageSize"/>PageSize object
        /// </summary>
        /// <param name="width">Width in mm</param>
        /// <param name="height">Height in mm</param>
        /// <param name="dpi">Dpi of the PageSize</param>
        /// <param name="paperSize">Standardized A-format paper size (if applicable)</param>
        private PageSize(int width, int height, int dpi = DefaultDpi, PaperSize? paperSize = null) {
            MillimeterWidth = width;
            MillimeterHeight = height;
            Dpi = dpi;
            Width = ConvertToPixels(MillimeterWidth, Dpi);
            Height = ConvertToPixels(MillimeterHeight, Dpi);
            IsStandardizedFormat = paperSize.HasValue;
            PaperSize = paperSize;
        }

        public static PageSize FromPixels(int pixelWidth, int pixelHeight, int dpi = DefaultDpi) {
            var millimeterWidth = ConvertToMillimeter(pixelWidth, dpi);
            var millimeterHeight = ConvertToMillimeter(pixelHeight, dpi);
            return new PageSize(millimeterWidth, millimeterHeight, dpi);
        }

        public static PageSize FromMillimeters(int millimeterWidth, int millimeterHeight, int dpi = DefaultDpi) {
            return new PageSize(millimeterWidth, millimeterHeight, dpi);
        }


        private static int ConvertToPixels(int measurement, int dpi) {
            return (int)(dpi * measurement / MmPerInch);
        }

        private static int ConvertToMillimeter(int pixels, int dpi) {
            return (int)(pixels * MmPerInch / dpi);
        }

        /// <summary>
        /// Returns the <see cref="PageSize"/> associated with the <see cref="PaperSize"/> and <see cref="PaperOrientation"/>
        /// </summary>
        /// <param name="size"><see cref="PaperSize"/> of the <see cref="PageSize"/></param>
        /// <param name="orientation"><see cref="PaperOrientation"/> of the <see cref="PageSize"/></param>
        /// <returns>The associated <see cref="PageSize"/></returns>
        public static PageSize From(PaperSize size, PaperOrientation orientation) {
            var index = (byte)size;
            var offset = (byte)orientation;
            index += (byte)(offset * PageSizes.Count / 2);
            if (index > PageSizes.Count) {
                index = 0;
            }

            return PageSizes[index];
        }

        public static PageSize From(IPageSizeDefinition definition) {
            if (definition == null) { return null; }
            return new PageSize(definition.Width, definition.Height, definition.Dpi) {
                IsFromPageDefinition = definition.OverrulePageSizeDefinition ?? true,
                Gap = definition.Gap,
                OffsetX = definition.OffsetX,
                OffsetY = definition.OffsetY,
            };
        }

        /// <summary>
        /// Creates a new <see cref="PageSize"/> based on the current <see cref="MillimeterWidth"/> and <see cref="MillimeterHeight"/> with the <see cref="DefaultPrinterDpi"/>.
        /// </summary>
        /// <returns></returns>
        public PageSize ToPrinterPageSize() {
            return FromMillimeters(MillimeterWidth, MillimeterHeight, DefaultPrinterDpi);
        }

        public PageSize WithScale(double scale) {
            return FromMillimeters((MillimeterWidth * scale).To<int>(), (MillimeterHeight * scale).To<int>(), Dpi);
        }

        public bool Equals(PageSize other) {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return Width == other.Width && Height == other.Height && Dpi == other.Dpi &&
                   MillimeterWidth == other.MillimeterWidth && MillimeterHeight == other.MillimeterHeight;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }
            return Equals((PageSize)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Width;
                hashCode = (hashCode * 397) ^ Height;
                hashCode = (hashCode * 397) ^ Dpi;
                hashCode = (hashCode * 397) ^ MillimeterWidth;
                hashCode = (hashCode * 397) ^ MillimeterHeight;
                return hashCode;
            }
        }
    }
}
