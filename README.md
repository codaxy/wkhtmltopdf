**This project is no longer maintained. For advanced PDF reporting please take a look at [CxReports](https://www.cx-reports.com/)**

Codaxy.WkHtmlToPdf
------------------

This is a small C# wrapper utility around wkhtmltopdf console tool.
You can use it to easily convert HTML reports to PDF.

NuGet
-----
NuGet includes single source file in your project.
```
Install-Package Codaxy.WkHtmlToPdf
```
Usage
-----
```
PdfConvert.ConvertHtmlToPdf(new PdfDocument 
{ 
    Url = "http://wkhtmltopdf.org/",
    HeaderLeft = "[title]",
    HeaderRight = "[date] [time]",
    FooterCenter = "Page [page] of [topage]"

}, new PdfOutput 
{
    OutputFilePath = "wkhtmltopdf-page.pdf"
});
```
Licence
-------
This project is available under MIT Licence.
