using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Codaxy.WkHtmlToPdf.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;

            PdfConvert.Environment.Debug = false;
            PdfConvert.ConvertHtmlToPdf(new PdfDocument { Url = "http://www.codaxy.com" }, new PdfOutput
            {
                OutputFilePath = "codaxy.pdf"
            });

            PdfConvert.ConvertHtmlToPdf(new PdfDocument
            {
                Url = "http://www.codaxy.com",
                HeaderLeft = "[title]",
                HeaderRight = "[date] [time]",
                FooterCenter = "Page [page] of [topage]",
                FooterFontSize = "10",
                HeaderFontSize = "20",
                HeaderFontName = "Comic Sans MS",
                FooterFontName = "Helvetica"

            }, new PdfOutput
            {
                OutputFilePath = "codaxy_hf.pdf"
            });

            PdfConvert.ConvertHtmlToPdf(
                new PdfDocument { Html = "<html><h1>test</h1></html>" },
                new PdfOutput { OutputFilePath = "inline.pdf" }
            );

            PdfConvert.ConvertHtmlToPdf(
                new PdfDocument { Html = "<html><h1>測試</h1></html>" },
                new PdfOutput { OutputFilePath = "inline_cht.pdf" }
            );


            //PdfConvert.ConvertHtmlToPdf("http://tweakers.net", "tweakers.pdf");
        }
    }
}
