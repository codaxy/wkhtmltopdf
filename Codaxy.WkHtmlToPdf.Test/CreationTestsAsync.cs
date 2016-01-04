using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Codaxy.WkHtmlToPdf.Test
{
    [TestClass]
    public class CreationTestsAsync
    {
        [TestMethod]
        public async Task ConvertUrlAsync()
        {
            await PdfConvert.ConvertHtmlToPdfAsync(new PdfDocument { Url = "http://www.codaxy.com" }, new PdfOutput
            {
                OutputFilePath = "async_codaxy.pdf"
            });
        }

        [TestMethod]
        public async Task ConvertStringAsync()
        {
            await PdfConvert.ConvertHtmlToPdfAsync(new PdfDocument { Url = "-", Html = "<html><h1>test</h1></html>" }, new PdfOutput
            {
                OutputFilePath = "async_inline.pdf"
            });
        }

        [TestMethod]
        public async Task ConvertUrlHeaderFooterAsync()
        {
            await PdfConvert.ConvertHtmlToPdfAsync(new PdfDocument
            {
                Url = "http://www.codaxy.com",
                HeaderLeft = "[title]",
                HeaderRight = "[date] [time]",
                FooterCenter = "Page [page] of [topage]"

            }, new PdfOutput
            {
                OutputFilePath = "async_codaxy_hf.pdf"
            });


        }

        [TestMethod]
        public async Task ConvertStringChineseAsync()
        {
            await PdfConvert.ConvertHtmlToPdfAsync(new PdfDocument { Url = "-", Html = "<html><h1>測試</h1></html>" }, new PdfOutput
            {
                OutputFilePath = "async_inline_cht.pdf"
            });
        }
    }
}
