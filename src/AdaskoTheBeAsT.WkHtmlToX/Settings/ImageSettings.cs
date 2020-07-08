#nullable enable
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Settings
{
    public class ImageSettings
        : ISettings
    {
        /// <summary>
        /// left/x coordinate of the window to capture in pixels. E.g. "200".
        /// </summary>
        [WkHtml("crop.left")]
        public string? CropLeft { get; set; }

        /// <summary>
        /// top/y coordinate of the window to capture in pixels. E.g. "200".
        /// </summary>
        [WkHtml("crop.top")]
        public string? CropTop { get; set; }

        /// <summary>
        /// Width of the window to capture in pixels. E.g. "200".
        /// </summary>
        [WkHtml("crop.width")]
        public string? CropWidth { get; set; }

        /// <summary>
        /// Height of the window to capture in pixels. E.g. "200".
        /// </summary>
        [WkHtml("crop.height")]
        public string? CropHeight { get; set; }

        /// <summary>
        /// Path of file used to load and store cookies.
        /// </summary>
        [WkHtml("load.cookieJar")]
        public string? CookieJar { get; set; }

        /// <summary>
        /// When outputting a PNG or SVG, make the white background transparent.
        /// Must be either "true" or "false".
        /// </summary>
        [WkHtml("transparent")]
        public bool? Transparent { get; set; }

        /// <summary>
        /// The URL or path of the input file, if "-" stdin is used. E.g. "http://google.com".
        /// </summary>
        [WkHtml("in")]
        public string? In { get; set; }

        /// <summary>
        /// The path of the output file, if "-" stdout is used, if empty the content is stored to a internalBuffer.
        /// </summary>
        [WkHtml("out")]
        public string? Out { get; set; }

        /// <summary>
        /// The output format to use, must be either "", "jpg", "png", "bmp" or "svg".
        /// </summary>
        [WkHtml("fmt")]
        public string? Format { get; set; }

        /// <summary>
        /// The with of the screen used to render is pixels, e.g "800".
        /// </summary>
        [WkHtml("screenWidth")]
        public string? ScreenWidth { get; set; }

        /// <summary>
        /// Should we expand the screenWidth if the content does not fit? must be either "true" or "false".
        /// </summary>
        [WkHtml("smartWidth")]
        public bool? SmartWidth { get; set; }

        /// <summary>
        /// The compression factor to use when outputting a JPEG image. E.g. "94".
        /// </summary>
        [WkHtml("quality")]
        public string? Quality { get; set; }

        public LoadSettings LoadSettings { get; set; } = new LoadSettings();

        public WebSettings WebSettings { get; set; } = new WebSettings();
    }
}
