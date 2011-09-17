del *.nupkg
mkdir Codaxy.WkHtmlToPdf\content\
copy ..\Tests\Codaxy.WkHtmlToPdf.Tests\PdfConvert.cs Codaxy.WkHtmlToPdf\content\
nuget pack Codaxy.WkHtmlToPdf\Codaxy.WkHtmlToPdf.nuspec
