using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ocr.@new.Image;
using ocr.@new.Processing;

namespace ocr.@new
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            var tesseractPath = solutionDirectory + @"\tesseract-master.1153";
            string dirImage = @"c:\test\image";
            var testFiles = Directory.EnumerateFiles(dirImage);

            var maxDegreeOfParallelism = Environment.ProcessorCount;
            //Parallel.ForEach(testFiles, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, (fileName) =>
            //{
            //    if(!fileName.EndsWith("- Darken.png"))
            //    ImagePreProcessing.EnhanceImageQuality(fileName, dirImage);

            //});
            Parallel.ForEach(testFiles, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, (fileName) =>
            {
                var ci = new CultureInfo("en-US");
                var ext = new List<string> { ".pdf" };
                if (fileName.EndsWith(".pdf",true, ci))
                     PdfProcessing.ConvertPdfV(fileName, dirImage);

            });
            Parallel.ForEach(testFiles, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, (fileName) =>
            {
                var imageFile = File.ReadAllBytes(fileName);
                var text = ParseText(tesseractPath, imageFile, fileName , "eng");
                Console.WriteLine("File:" + fileName + "\n" + text + "\n");
            });

            stopwatch.Stop();
            var tm= stopwatch.Elapsed;

        }
        private static string ParseText(string tesseractPath, byte[] imageFile, string fileName, params string[] lang)
        {
            string output = string.Empty;
            var tempOutputFile = Path.GetTempPath() + Guid.NewGuid();
            var tempImageFile = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(tempImageFile, imageFile);

                ProcessStartInfo info = new ProcessStartInfo();
                info.WorkingDirectory = tesseractPath;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.UseShellExecute = false;
                info.FileName = "cmd.exe";
                info.Arguments =
                    "/c tesseract.exe " +
                    // Image file.
                    tempImageFile + " " +
                    // Output file (tesseract add '.txt' at the end)
                    tempOutputFile +
                    // Languages.
                    " -l " + string.Join("+", lang);

                // Start tesseract.
                Process process = Process.Start(info);
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    // Exit code: success.
                    output = File.ReadAllText(tempOutputFile + ".txt");
                }
                else
                {
                    throw new Exception("Error. Tesseract stopped with an error code = " + process.ExitCode);
                }
            }
            finally
            {
                File.Delete(tempImageFile);
                if(File.Exists(@"c:\test\" + Path.GetFileName(fileName) + ".txt"))
                    File.Delete(@"c:\test\" + Path.GetFileName(fileName) + ".txt");
                File.Copy(tempOutputFile + ".txt",@"c:\test\"+ Path.GetFileName(fileName)  + ".txt");
                File.Delete(tempOutputFile + ".txt");
            }
            return output;
        }
    }
}
