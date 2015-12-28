using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Codaxy.WkHtmlToPdf
{
    /// <summary>
    /// An exception that can occur whilst converting HTML to PDF.
    /// </summary>
    public class PdfConvertException : Exception
    {
        public PdfConvertException(String msg) : base(msg) { }
    }

    /// <summary>
    /// An exception representing a failed state wherin a conversion time-out has occurred.
    /// </summary>
    public class PdfConvertTimeoutException : PdfConvertException
    {
        public PdfConvertTimeoutException() : base("HTML to PDF conversion process has not finished in the given period.") { }
    }

    /// <summary>
    /// The output's can ben single or multiple.
    /// </summary>
	public class PdfOutput
    {
        /// <summary>
        /// The file path for the output document.
        /// </summary>
		public String OutputFilePath { get; set; }
        /// <summary>
        /// The output stream, pdf data will be written to the stream.
        /// </summary>
		public Stream OutputStream { get; set; }
        /// <summary>
        /// Callback that will be called once conversion has completed.
        /// </summary>
		public Action<PdfDocument, byte[]> OutputCallback { get; set; }
    }

    /// <summary>
    /// The object representing a document to be converted to PDF or already converted.
    /// </summary>
	public class PdfDocument
    {
        /// <summary>
        /// The URL of which to fetch the HTML document for the document body.
        /// </summary>
		public String Url { get; set; }
        /// <summary>
        /// An HTML string used as the base file for conversion.
        /// </summary>
		public String Html { get; set; }
        /// <summary>
        /// URL of which to fetch the HTML document for the document header.
        /// </summary>
		public String HeaderUrl { get; set; }
        /// <summary>
        /// URL of which to fetch the HTML document for the document footer.
        /// </summary>
		public String FooterUrl { get; set; }
        /// <summary>
        /// HTML string used to render the left part of the document header.
        /// </summary>
        public String HeaderLeft { get; set; }
        /// <summary>
        /// HTML string used to render the center part of the document header.
        /// </summary>
        public String HeaderCenter { get; set; }
        /// <summary>
        /// HTML string used to render the right part of the document header.
        /// </summary>
        public String HeaderRight { get; set; }
        /// <summary>
        /// HTML string used to render the left part of the document footer.
        /// </summary>
        public String FooterLeft { get; set; }
        /// <summary>
        /// HTML string used to render the center of the document footer.
        /// </summary>
        public String FooterCenter { get; set; }
        /// <summary>
        /// HTML string used to render the right part of the document footer.
        /// </summary>
        public String FooterRight { get; set; }
        /// <summary>
        /// An object representing the document state.
        /// </summary>
		public object State { get; set; }
        /// <summary>
        /// A dictionary containing cookies to be send to the server whilst fetching HTML documents.
        /// </summary>
        public Dictionary<String, String> Cookies { get; set; }
        /// <summary>
        /// A dictionary containing the extra parameters send to 'wkhtmltopdf' utility.
        /// </summary>
        public Dictionary<String, String> ExtraParams { get; set; }
    }

    public class PdfConvertEnvironment
    {
        /// <summary>
        /// The path to store the processed files temporarily
        /// </summary>
        public string TempFolderPath { get; set; }
        /// <summary>
        /// The path to the wkhtmltopdf executable file.
        /// </summary>
		public String WkHtmlToPdfPath { get; set; }
        /// <summary>
        /// The processing timeout.
        /// </summary>
		public int Timeout { get; set; }
        /// <summary>
        /// Determines whether this is a debugging run.
        /// </summary>
		public bool Debug { get; set; }
    }

    /// <summary>
    /// Contains all helpers for HTML to PDF conversion.
    /// </summary>
    public class PdfConvert
    {
        private const short BUFFER_SIZE = 4096;

        private static PdfConvertEnvironment _e;

        /// <summary>
        /// The Pdf converter settings, if not set a default will be returned.
        /// </summary>
		public static PdfConvertEnvironment Environment
        {
            get
            {
                if (_e == null)
                    _e = new PdfConvertEnvironment
                    {
                        TempFolderPath = Path.GetTempPath(),
                        WkHtmlToPdfPath = GetWkhtmlToPdfExeLocation(),
                        Timeout = 60000
                    };
                return _e;
            }
        }

        private static string GetWkhtmlToPdfExeLocation()
        {
            string programFilesPath = System.Environment.GetEnvironmentVariable("ProgramFiles");
            string filePath = Path.Combine(programFilesPath, @"wkhtmltopdf\wkhtmltopdf.exe");

            if (File.Exists(filePath))
                return filePath;

            string programFilesx86Path = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            filePath = Path.Combine(programFilesx86Path, @"wkhtmltopdf\wkhtmltopdf.exe");

            if (File.Exists(filePath))
                return filePath;

            filePath = Path.Combine(programFilesPath, @"wkhtmltopdf\bin\wkhtmltopdf.exe");
            if (File.Exists(filePath))
                return filePath;

            return Path.Combine(programFilesx86Path, @"wkhtmltopdf\bin\wkhtmltopdf.exe");
        }

        private static bool IsEmptyUrl(string url)
        {
            return string.IsNullOrEmpty(url) ? true : url == "-" ? true : false;
        }

        private static string BuildParams(PdfDocument document, string outputPath)
        {
            StringBuilder paramsBuilder = new StringBuilder();
            paramsBuilder.Append("--page-size A4 ");

            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                paramsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
                paramsBuilder.Append("--margin-top 25 ");
                paramsBuilder.Append("--header-spacing 5 ");
            }

            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                paramsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
                paramsBuilder.Append("--margin-bottom 25 ");
                paramsBuilder.Append("--footer-spacing 5 ");
            }

            if (!string.IsNullOrEmpty(document.HeaderLeft))
                paramsBuilder.AppendFormat("--header-left \"{0}\" ", document.HeaderLeft);

            if (!string.IsNullOrEmpty(document.FooterCenter))
                paramsBuilder.AppendFormat("--header-center \"{0}\" ", document.HeaderCenter);

            if (!string.IsNullOrEmpty(document.FooterCenter))
                paramsBuilder.AppendFormat("--header-right \"{0}\" ", document.HeaderRight);

            if (!string.IsNullOrEmpty(document.FooterLeft))
                paramsBuilder.AppendFormat("--footer-left \"{0}\" ", document.FooterLeft);

            if (!string.IsNullOrEmpty(document.FooterCenter))
                paramsBuilder.AppendFormat("--footer-center \"{0}\" ", document.FooterCenter);

            if (!string.IsNullOrEmpty(document.FooterCenter))
                paramsBuilder.AppendFormat("--footer-right \"{0}\" ", document.FooterRight);

            if (document.ExtraParams != null)
                foreach (var extraParam in document.ExtraParams)
                    paramsBuilder.AppendFormat("--{0} {1} ", extraParam.Key, extraParam.Value);

            if (document.Cookies != null)
                foreach (var cookie in document.Cookies)
                    paramsBuilder.AppendFormat("--cookie {0} {1} ", cookie.Key, HttpUtility.UrlEncode(cookie.Value));

            if (!IsEmptyUrl(document.Url))
                paramsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, outputPath);
            else
                paramsBuilder.AppendFormat("- \"{0}\"", outputPath);

            return paramsBuilder.ToString();
        }

        /// <summary>
        /// Converts a PdfDocument object from HTML to PDF using default environment settings.
        /// </summary>
        /// <param name="document">The PDF input document.</param>
        /// <param name="output">An object holding the output settings.</param>
        public static void ConvertHtmlToPdf(PdfDocument document, PdfOutput output)
        {
            ConvertHtmlToPdf(document, null, output);
        }

        /// <summary>
        /// Converts a PdfDocument object from HTML to PDf.
        /// </summary>
        /// <param name="document">The PDF input document.</param>
        /// <param name="environment">The wkhtml enviromental settings object.</param>
        /// <param name="woutput">An object holding the output settings.</param>
        public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            if (IsEmptyUrl(document.Url) && string.IsNullOrEmpty(document.Html))
                throw new PdfConvertException(
                    String.Format("You must supply a HTML string, if you have entered the url: {0}",
                        document.Url)
                );

            if (environment == null)
                environment = Environment;

            if (!File.Exists(environment.WkHtmlToPdfPath))
                throw new PdfConvertException(
                    String.Format("File '{0}' not found. Check if wkhtmltopdf application is installed.",
                        environment.WkHtmlToPdfPath));

            string PdfOutputPath;
            if (woutput.OutputFilePath == null)
                PdfOutputPath = Path.Combine(Environment.TempFolderPath, Path.GetRandomFileName());
            else
                PdfOutputPath = woutput.OutputFilePath;

            var error = new StringBuilder();

            using (var output = new MemoryStream())
            using (Process process = new Process())
            {
                process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                process.StartInfo.Arguments = BuildParams(document, PdfOutputPath);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;

                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    DataReceivedEventHandler errorHandler = (sender, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += errorHandler;
                    process.Start();
                    process.BeginErrorReadLine();

                    if (document.Html != null && IsEmptyUrl(document.Url))
                        using (var stream = process.StandardInput)
                            stream.Write(document.Html);

                    if (process.WaitForExit(environment.Timeout) && errorWaitHandle.WaitOne())
                    {
                        if (process.ExitCode != 0)
                            throw new PdfConvertException(
                                String.Format("Html to PDF conversion of document failed. Wkhtmltopdf output: \r\n{1}",
                                document.Url, error));
                        else
                        {
                            if (woutput.OutputStream != null || woutput.OutputCallback != null)
                            {
                                int read;
                                byte[] buff = new byte[BUFFER_SIZE];
                                using (var fs = new FileStream(PdfOutputPath, FileMode.Open))
                                    while ((read = fs.Read(buff, 0, BUFFER_SIZE)) > 0)
                                        output.Write(buff, 0, read);

                                output.Position = 0;
                                if (woutput.OutputStream != null)
                                    while ((read = output.Read(buff, 0, BUFFER_SIZE)) > 0)
                                        woutput.OutputStream.Write(buff, 0, read);
                                if (woutput.OutputCallback != null)
                                    woutput.OutputCallback(document, output.ToArray());
                            }
                        }
                    }
                    else
                    {
                        if (!process.HasExited)
                            process.Kill();

                        throw new PdfConvertTimeoutException();
                    }

                    process.ErrorDataReceived -= errorHandler;
                    if (woutput.OutputFilePath == null)
                        File.Delete(PdfOutputPath);
                }
            }
        }

        /// <summary>
        /// Converts a PdfDocument object from HTML to PDF using default environment settings.
        /// </summary>
        /// <param name="document">The PDF input document.</param>
        /// <param name="output">An object holding the output settings.</param>
        public static Task ConvertHtmlToPdfAsync(PdfDocument document, PdfOutput output)
        {
            return Task.Factory.StartNew(() =>
            {
                ConvertHtmlToPdf(document, null, output);
            });
        }

        /// <summary>
        /// Converts a PdfDocument object from HTML to PDf.
        /// </summary>
        /// <param name="document">The PDF input document.</param>
        /// <param name="environment">The wkhtml enviromental settings object.</param>
        /// <param name="woutput">An object holding the output settings.</param>
        public static Task ConvertHtmlToPdfAsync(PdfDocument document, PdfConvertEnvironment environment, PdfOutput output)
        {
            return Task.Factory.StartNew(() =>
            {
                ConvertHtmlToPdf(document, environment, output);
            });
        }
    }
}

//class OSUtil
//{
//    public static string GetProgramFilesx86Path()
//    {
//        if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
//        {
//            return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
//        }
//        return Environment.GetEnvironmentVariable("ProgramFiles");
//    }
//}

//public static class HttpResponseExtensions
//{
//    public static void SendFileForDownload(this HttpResponse response, String filename, byte[] content)
//    {
//        SetFileDownloadHeaders(response, filename);
//        response.OutputStream.Write(content, 0, content.Length);
//        response.Flush();
//    }

//    public static void SendFileForDownload(this HttpResponse response, String filename)
//    {
//        SetFileDownloadHeaders(response, filename);
//        response.TransmitFile(filename);
//        response.Flush();
//    }

//    public static void SetFileDownloadHeaders(this HttpResponse response, String filename)
//    {
//        FileInfo fi = new FileInfo(filename);
//        response.ContentType = "application/force-download";
//        response.AddHeader("Content-Disposition", "attachment; filename=\"" + fi.Name + "\"");
//    }
//}
