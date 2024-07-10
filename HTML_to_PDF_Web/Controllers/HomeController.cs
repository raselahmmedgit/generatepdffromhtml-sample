using HTML_to_PDF_Web.Core;
using HTML_to_PDF_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace HTML_to_PDF_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger,
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Generate()
        {
            try
            {
                var stream = await ConvertRazorViewToPDF();

                return File(stream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf, "Sample.pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region RazorView To PDF

        /// <summary>
        /// Get MemoryStream
        /// </summary>
        private async Task<MemoryStream> ConvertRazorViewToPDF()
        {
            var htmlView = await this.RenderRazorViewAsync(this, "Sample", false);

            //Convert url to pdf
            PdfDocument document = GetPdfDocument(htmlView, PdfDocumentTypeEnum.GeneralPage, PdfPageOrientation.Portrait);

            MemoryStream stream = new MemoryStream();

            //Save and close the output PDF document
            document.Save(stream);
            document.Close();

            return stream;
        }

        /// <summary>
        /// Adds header to the document
        /// </summary>
        /// <param name="width"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        private PdfPageTemplateElement AddHeader(float width, string title, string description)
        {
            Syncfusion.Drawing.RectangleF rect = new Syncfusion.Drawing.RectangleF(0, 0, width, 50);
            //Create a new instance of PdfPageTemplateElement class.     
            PdfPageTemplateElement header = new PdfPageTemplateElement(rect);
            PdfGraphics g = header.Graphics;

            //Draw title.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
            PdfSolidBrush brush = new PdfSolidBrush(Syncfusion.Drawing.Color.FromArgb(44, 71, 120));
            float x = (width / 2) - (font.MeasureString(title).Width) / 2;
            g.DrawString(title, font, brush, new Syncfusion.Drawing.RectangleF(x, (rect.Height / 4) + 3, font.MeasureString(title).Width, font.Height));

            //Draw description
            brush = new PdfSolidBrush(Syncfusion.Drawing.Color.Gray);
            font = new PdfStandardFont(PdfFontFamily.Helvetica, 16, PdfFontStyle.Bold);
            PdfStringFormat format = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Bottom);
            g.DrawString(description, font, brush, new Syncfusion.Drawing.RectangleF(0, 0, header.Width, header.Height - 8), format);

            //Draw some lines in the header
            PdfPen pen = new PdfPen(Syncfusion.Drawing.Color.DarkBlue, 0.7f);
            g.DrawLine(pen, 0, 0, header.Width, 0);
            pen = new PdfPen(Syncfusion.Drawing.Color.DarkBlue, 2f);
            g.DrawLine(pen, 0, 03, header.Width + 3, 03);
            pen = new PdfPen(Syncfusion.Drawing.Color.DarkBlue, 2f);
            g.DrawLine(pen, 0, header.Height - 3, header.Width, header.Height - 3);
            g.DrawLine(pen, 0, header.Height, header.Width, header.Height);

            return header;
        }

        /// <summary>
        /// Adds footer to the document
        /// </summary>
        /// <param name="width"></param>
        /// <param name="footerText"></param>
        private PdfPageTemplateElement AddFooter(float width, string footerText)
        {
            Syncfusion.Drawing.RectangleF rect = new Syncfusion.Drawing.RectangleF(0, 0, width, 50);
            //Create a new instance of PdfPageTemplateElement class.
            PdfPageTemplateElement footer = new PdfPageTemplateElement(rect);
            PdfGraphics g = footer.Graphics;

            // Draw footer text.
            PdfSolidBrush brush = new PdfSolidBrush(Syncfusion.Drawing.Color.Gray);
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 6, PdfFontStyle.Bold);
            float x = (width / 2) - (font.MeasureString(footerText).Width) / 2;
            g.DrawString(footerText, font, brush, new Syncfusion.Drawing.RectangleF(x, g.ClientSize.Height - 10, font.MeasureString(footerText).Width, font.Height));

            //Create page number field
            PdfPageNumberField pageNumber = new PdfPageNumberField(font, brush);

            //Create page count field
            PdfPageCountField count = new PdfPageCountField(font, brush);

            PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);
            compositeField.Bounds = footer.Bounds;
            compositeField.Draw(g, new Syncfusion.Drawing.PointF(470, 40));

            return footer;

        }

        /// <summary>
        /// Get pdf document
        /// </summary>
        /// <param name="htmlView"></param>
        /// <param name="pdfDocumentTypeEnum"></param>
        /// <param name="orientation"></param>
        private PdfDocument GetPdfDocument(string htmlView, PdfDocumentTypeEnum pdfDocumentTypeEnum, PdfPageOrientation orientation)
        {
            string myHostUrl = $"{_httpContextAccessor?.HttpContext?.Request.Scheme}://{_httpContextAccessor?.HttpContext?.Request.Host}";
            HtmlToPdfConverter htmlConverter = GetHtmlToPdfConverter(orientation);
            if (orientation == PdfPageOrientation.Landscape)
            {
                htmlConverter.ConverterSettings.Margin.Top = 45F;
                htmlConverter.ConverterSettings.Margin.Left = 65F;
                htmlConverter.ConverterSettings.Margin.Right = 31F;
                htmlConverter.ConverterSettings.Margin.Bottom = 20F;
            }
            if (orientation == PdfPageOrientation.Portrait)
            {
                htmlConverter.ConverterSettings.Margin.Top = 45F;
                htmlConverter.ConverterSettings.Margin.Left = 62F;
                htmlConverter.ConverterSettings.Margin.Right = 31F;
                htmlConverter.ConverterSettings.Margin.Bottom = 40F;
            }

            if (pdfDocumentTypeEnum == PdfDocumentTypeEnum.Forwardingletter)
            {
                htmlConverter.ConverterSettings.Margin.Top = 65F;
                htmlConverter.ConverterSettings.Margin.Left = 75F;
                htmlConverter.ConverterSettings.Margin.Right = 31F;
                htmlConverter.ConverterSettings.Margin.Bottom = 60F;
            }
            if (pdfDocumentTypeEnum == PdfDocumentTypeEnum.HeaderLogo)
            {
                htmlConverter.ConverterSettings.Margin.Top = 20F;
                htmlConverter.ConverterSettings.Margin.Left = 75F;
                htmlConverter.ConverterSettings.Margin.Right = 25F;
            }
            if (pdfDocumentTypeEnum == PdfDocumentTypeEnum.FooterBrand)
            {
                htmlConverter.ConverterSettings.Margin.Top = 10F;
                htmlConverter.ConverterSettings.Margin.Left = 75F;
                htmlConverter.ConverterSettings.Margin.Right = 13F;
                htmlConverter.ConverterSettings.Margin.Bottom = 12F;
            }
            if (pdfDocumentTypeEnum == PdfDocumentTypeEnum.BlankPage)
            {
                htmlConverter.ConverterSettings.Margin.Left = 25F;
                htmlConverter.ConverterSettings.Margin.Right = 25F;
                htmlConverter.ConverterSettings.Margin.Top = 25F;
                htmlConverter.ConverterSettings.Margin.Bottom = 10F;
            }
            if (pdfDocumentTypeEnum == PdfDocumentTypeEnum.IndexPage)
            {
                htmlConverter.ConverterSettings.Margin.Right = 25F;
            }

            PdfDocument pdfDocument = htmlConverter.Convert(htmlView, myHostUrl);
            return pdfDocument;
        }

        public HtmlToPdfConverter GetHtmlToPdfConverter(PdfPageOrientation orientation)
        {
            int width = 1360;
            int height = 768;
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            
            WebKitConverterSettings converterSettings = new WebKitConverterSettings();
            converterSettings.WebKitPath = Path.Combine(_env.ContentRootPath, @"Utility\QtBinariesWindows");

            //BlinkConverterSettings converterSettings = new BlinkConverterSettings();

            converterSettings.Orientation = orientation;
            converterSettings.PdfPageSize = PdfPageSize.A4;
            converterSettings.DisableWebKitWarning = true;
            converterSettings.EnableBookmarks = true;
            converterSettings.EnableHyperLink = true;
            converterSettings.EnableRepeatTableFooter = true;
            //converterSettings.EnableRepeatTableHeader = true;
            converterSettings.EnableRepeatTableHeader = false;
            converterSettings.EnableJavaScript = true;
            converterSettings.EnableOfflineMode = true;
            converterSettings.HtmlEncoding = Encoding.Unicode;
            if (orientation == PdfPageOrientation.Landscape)
            {
                height = Convert.ToInt32(height * 1.5);
                width = Convert.ToInt32(width * 1.5);
            }
            converterSettings.WebKitViewPort = new Syncfusion.Drawing.Size(height, width);

            converterSettings.PdfHeader = this.AddHeader(converterSettings.PdfPageSize.Width, "Sample PDF", " ");
            converterSettings.PdfFooter = this.AddFooter(converterSettings.PdfPageSize.Height, "@Copyright 2024");

            //Assign settings to HTML converter
            htmlConverter.ConverterSettings = converterSettings;

            return htmlConverter;
        }

        /// <summary>
        /// Render Razor View to the string
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="viewName"></param>
        /// <param name="partial"></param>
        /// <returns></returns>
        public async Task<string> RenderRazorViewAsync(Controller controller, string viewName, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }
            
            using (var writer = new StringWriter())
            {

                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                ViewEngineResult viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );
                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }

        #endregion
    }
}
