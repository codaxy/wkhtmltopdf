using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Web;

namespace Codaxy.WkHtmlToPdf
{
    public class PdfConvertException : Exception
    {
        public PdfConvertException(String msg) : base(msg) { }
    }

    public class PdfConvertTimeoutException : PdfConvertException
    {
        public PdfConvertTimeoutException() : base("HTML to PDF conversion process has not finished in the given period.") { }
    }

	public class PdfOutput
	{
		public String OutputFilePath { get; set; }
		public Stream OutputStream { get; set; }
		public Action<PdfDocument, byte[]> OutputCallback { get; set; }
	}

	public class PdfDocument
	{
		public String Url { get; set; }
		public String HeaderUrl { get; set; }
		public String FooterUrl { get; set; }
		public object State { get; set; }
	}

	public class PdfConvertEnvironment
	{
		public String TempFolderPath { get; set; }
		public String WkHtmlToPdfPath { get; set; }
		public int Timeout { get; set; }
		public bool Debug { get; set; }
	}

    public class PdfConvert
    {
		static PdfConvertEnvironment _e;

		public static PdfConvertEnvironment Environment
		{
			get
			{
				if (_e == null)
					_e = new PdfConvertEnvironment
					{
						TempFolderPath = Path.GetTempPath(),
						WkHtmlToPdfPath = Path.Combine(OSUtil.GetProgramFilesx86Path(), @"wkhtmltopdf\wkhtmltopdf.exe"),
						Timeout = 60000
					};
				return _e;
			}
		}

		public static void ConvertHtmlToPdf(PdfDocument document, PdfOutput output)
		{
			ConvertHtmlToPdf(document, null, output);
		}

		public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
			if (environment == null)
				environment = Environment;

			String outputPdfFilePath;
			bool delete;
			if (woutput.OutputFilePath != null)
			{
				outputPdfFilePath = woutput.OutputFilePath;
				delete = false;
			}
			else
			{
				outputPdfFilePath = Path.Combine(environment.TempFolderPath, String.Format("{0}.pdf", Guid.NewGuid()));
				delete = true;
			}

			if (!File.Exists(environment.WkHtmlToPdfPath))
				throw new PdfConvertException(String.Format("File '{0}' not found. Check if wkhtmltopdf application is installed.", environment.WkHtmlToPdfPath));

            ProcessStartInfo si;

            StringBuilder paramsBuilder = new StringBuilder();
            paramsBuilder.Append("--page-size A4 ");
            //paramsBuilder.Append("--redirect-delay 0 "); not available in latest version
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
            
			paramsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, outputPdfFilePath);
           

            si = new ProcessStartInfo();
            si.CreateNoWindow = !environment.Debug;
			si.FileName = environment.WkHtmlToPdfPath;
            si.Arguments = paramsBuilder.ToString();
            si.UseShellExecute = false;
            si.RedirectStandardError = !environment.Debug;

			try
			{
				using (var process = new Process())
				{
					process.StartInfo = si;
					process.Start();

					if (!process.WaitForExit(environment.Timeout))
						throw new PdfConvertTimeoutException();

                    if (!File.Exists(outputPdfFilePath))
                    {
                        if (process.ExitCode != 0)
                        {
                            var error = si.RedirectStandardError ? process.StandardError.ReadToEnd() : String.Format("Process exited with code {0}.", process.ExitCode);
                            throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Wkhtmltopdf output: \r\n{1}", document.Url, error));
                        }

                        throw new PdfConvertException(String.Format("Html to PDF conversion of '{0}' failed. Reason: Output file '{1}' not found.", document.Url, outputPdfFilePath));
                    }

					if (woutput.OutputStream != null)
					{
						using (Stream fs = new FileStream(outputPdfFilePath, FileMode.Open))
						{
							byte[] buffer = new byte[32 * 1024];
							int read;

							while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
								woutput.OutputStream.Write(buffer, 0, read);
						}
					}

					if (woutput.OutputCallback != null)
					{
						woutput.OutputCallback(document, File.ReadAllBytes(outputPdfFilePath));
					}
				}
			}
			finally
			{
				if (delete && File.Exists(outputPdfFilePath))
					File.Delete(outputPdfFilePath);
			}
        }
    }

	class OSUtil
	{
		public static string GetProgramFilesx86Path()
		{
			if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
			{
				return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
			}
			return Environment.GetEnvironmentVariable("ProgramFiles");
		}
	}

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
}
