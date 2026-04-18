using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BECOSOFT.Utilities.Helpers.Websites {
    public static class HtmlTagHelper {
        public static string GenerateImageTag(ImageTagProperties properties) {
            var builder = new StringBuilder();

            // Create source tags
            if (properties.ImageSources.HasAny()) {
                foreach (var imageSource in properties.ImageSources) {
                    var media = imageSource.MaxWidth.HasValue ? $"max-width: {imageSource.MaxWidth.Value}px" : $"min-width: {imageSource.MinWidth.GetValueOrDefault()}px";
                    builder.Append("<source srcset=\"{0}\" media=\"({1})\" alt=\"{2}\" ", imageSource.Url, media, properties.AlternativeText);

                    if (properties.Attributes.HasAny()) {
                        foreach (var attribute in properties.Attributes) {
                            builder.Append("{0}=\"{1}\" ", attribute.Key, attribute.Value);
                        }
                    }

                    if (imageSource.Type.HasValue()) {
                        builder.Append(" type=\"{0}\" ", imageSource.Type);
                    }

                    if (imageSource.Width.HasValue) {
                        builder.Append("width=\"{0}px\" ", imageSource.Width.Value);
                    }
                    if (imageSource.Height.HasValue) {
                        builder.Append("height=\"{0}px\" ", imageSource.Height.Value);
                    }

                    builder.Append(" />");
                }
            }

            // Create image tag if url is provided
            if (properties.Url.HasValue()) {
                builder.Append("<img ");
                builder.Append("src=\"{0}\" ", properties.Url);
                if (properties.AlternativeText.HasValue()) {
                    builder.Append("alt=\"{0}\" ", properties.AlternativeText);
                }
                if (properties.Attributes.HasAny()) {
                    foreach (var attribute in properties.Attributes) {
                        builder.Append("{0}=\"{1}\" ", attribute.Key, attribute.Value);
                    }
                }
                if (properties.Width.HasValue) {
                    builder.Append("width=\"{0}px\"", properties.Width.Value);
                }
                if (properties.Height.HasValue) {
                    builder.Append("height=\"{0}px\"", properties.Height.Value);
                }
                if (properties.UseLazyLoading) {
                    builder.Append("loading", "lazy");
                }
                builder.Append(" />");
            }
            return builder.ToString();
        }
    }

    public class ImageTagProperties {
        public string Url { get; set; }
        public string AlternativeText { get; set; }
        public KeyValueList<string, string> Attributes { get; set; }
        public List<ImageSource> ImageSources { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public bool UseLazyLoading { get; set; }
    }

    public class ImageSource {
        public string Url { get; set; }
        public int? MinWidth { get; set; }
        public int? MaxWidth { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public string Type { get; set; }
    }
}
