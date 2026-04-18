using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Helpers.Websites {
    public enum DeviceType {
        Smartphone,
        Tablet,
        Desktop
    }

    public class DeviceWidthProperties {
        public int ScreenMaxWidth { get; }
        public int ContainerMaxWidth { get; set; }
        public double ColumnWidth { get; }
        public DeviceType DeviceType { get; set; }
        public bool DifferentForFluid { get; set; }

        public DeviceWidthProperties(int screenMaxWidth, int containerMaxWidth, DeviceType deviceType, bool differentForFluid = false) {
            ScreenMaxWidth = screenMaxWidth;
            ContainerMaxWidth = containerMaxWidth;
            DeviceType = deviceType;
            ColumnWidth = ContainerMaxWidth / 12;
            DifferentForFluid = differentForFluid;
        }
    }

    public static class HtmlConstants {
        /// <summary>
        /// 
        /// </summary>
        public static class Bootstrap4 {
            public static List<DeviceWidthProperties> DeviceWidthProperties = new List<DeviceWidthProperties>() {
                    new DeviceWidthProperties(320, 320, DeviceType.Smartphone),
                    new DeviceWidthProperties(480, 480, DeviceType.Smartphone),
                    new DeviceWidthProperties(575, 540, DeviceType.Smartphone),
                    new DeviceWidthProperties(767, 720, DeviceType.Tablet, true),
                    new DeviceWidthProperties(991, 960, DeviceType.Tablet, true),
                    new DeviceWidthProperties(1199, 1140, DeviceType.Desktop, true),
                    new DeviceWidthProperties(1599, 1530, DeviceType.Desktop, true),
                    new DeviceWidthProperties(1920, 1530, DeviceType.Desktop, true),
                    new DeviceWidthProperties(2560, 2560, DeviceType.Desktop, true),
                };

            public static List<ImageSource> GetImageSources(string imageurl, int[] imageWidthHeight, DeviceType deviceType, int nrOfCols, bool inFluidRow,
                int[] spacings = null,
                string imageType = "image/webp", double[] dpis = null) {
                if (dpis == null) {
                    dpis = new double[] { 1, 1.75, 2, 3 };
                }

                var spacingTop = 0;
                var spacingRight = 0;
                var spacingBottom = 0;
                var spacingLeft = 0;

                if (spacings.Length == 4) {
                    spacingTop = spacings[0];
                    spacingRight = spacings[1];
                    spacingBottom = spacings[2];
                    spacingLeft = spacings[3];
                }

                var imageSources = new List<ImageSource>();

                foreach (var deviceWidthProperty in DeviceWidthProperties.Where(d => d.DeviceType == deviceType)) {

                    if(inFluidRow && !deviceWidthProperty.DifferentForFluid)
                    {
                        deviceWidthProperty.ContainerMaxWidth = deviceWidthProperty.ScreenMaxWidth;
                    }

                    if (imageWidthHeight[0] <= 0
                        || imageWidthHeight[1] <= 0) {
                        continue;
                    }

                    var widtHeightResult = ((double)imageWidthHeight[0] / (double)imageWidthHeight[1]);
                    var widthInCol = (int) (deviceWidthProperty.ColumnWidth * nrOfCols) - spacingLeft - spacingRight;
                    var heightInCol = (int)(widthInCol / widtHeightResult) - spacingTop - spacingBottom;

                    if (imageWidthHeight[0] < widthInCol
                    || imageWidthHeight[1] < heightInCol) {
                        continue;
                    }

                    var srcsetUrls = new List<string>();
                    foreach (var dpi in dpis) {
                        var currentWidth = (int)(widthInCol * dpi);
                        srcsetUrls.Add($"{imageurl}&width={currentWidth} {dpi.ToString().Replace(',', '.')}x");

                        //CurrentWidth to big? Don't bother adding more
                        if (imageWidthHeight[0] < currentWidth) {
                            break;
                        }
                    }

                    if (srcsetUrls.IsEmpty()) {
                        continue;
                    }

                    imageSources.Add(new ImageSource {
                        Type = imageType,
                        MaxWidth = deviceWidthProperty.ScreenMaxWidth,
                        Width = widthInCol,
                        Height = heightInCol,
                        Url = string.Join(",", srcsetUrls),
                    });
                }

                return imageSources;
            }
        }
    }
}
