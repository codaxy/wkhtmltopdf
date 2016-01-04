using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Codaxy.WkHtmlToPdf.Test
{
    [TestClass]
    public class CreationTests
    {
        [TestMethod]
        public void ConvertUrl()
        {
            PdfConvert.ConvertHtmlToPdf(new PdfDocument { Url = "http://www.codaxy.com" }, new PdfOutput
            {
                OutputFilePath = "codaxy.pdf"
            });
        }

        [TestMethod]
        public void ConvertString()
        {
            PdfConvert.ConvertHtmlToPdf(new PdfDocument { Url = "-", Html = "<html><h1>test</h1></html>" }, new PdfOutput
            {
                OutputFilePath = "inline.pdf"
            });
        }

        [TestMethod]
        public void ConvertUrlHeaderFooter()
        {
            PdfConvert.ConvertHtmlToPdf(new PdfDocument
            {
                Url = "http://www.codaxy.com",
                HeaderLeft = "[title]",
                HeaderRight = "[date] [time]",
                FooterCenter = "Page [page] of [topage]"

            }, new PdfOutput
            {
                OutputFilePath = "codaxy_hf.pdf"
            });
        }

        [TestMethod]
        public void ConvertStringChinese()
        {
            PdfConvert.ConvertHtmlToPdf(new PdfDocument { Url = "-", Html = "<html><h1>測試</h1></html>" }, new PdfOutput
            {
                OutputFilePath = "inline_cht.pdf"
            });
        }
    }
}
