
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ocr.@new.Image;
using Tesseract;
using Path = System.IO.Path;

namespace ocr.@new.Processing
{
    public class PdfProcessing
    {
       
       
        public static async Task<bool> ConvertPdfV(string fileName, string imageDir)
        {
            
            MagickReadSettings settings = new MagickReadSettings();
            var ci = new CultureInfo("en-US");
            settings.Density = new Density(300, 300);
            if (!Directory.Exists(imageDir))
                Directory.CreateDirectory(imageDir);
            DirectoryInfo dr = new DirectoryInfo(imageDir + @"\");
            if (!IsFileLocked(fileName))

            {
                try
                {
                   
                      
                   
                        using (MagickImageCollection images = new MagickImageCollection())
                        {
                            // Add all the pages of the pdf file to the collection
                            images.Read(string.Format(fileName), settings);
                            Bitmap copy = null;
                            if (images.Count == 1)
                           
                            {
                                images[0]=ImagePreProcessing.EnhanceImageQuality((MagickImage) images[0]);
                                var image2 = new Bitmap(images[0].ToBitmap());
                                copy = new Bitmap(image2.Width, image2.Height,
                                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                                using (Graphics gr = Graphics.FromImage(copy))
                                {
                                    gr.DrawImage(image2, new Rectangle(0, 0, copy.Width, copy.Height));
                                }

                            // var  rowlist = PerformingOcr.DoOcr2(copy, Path.GetFileNameWithoutExtension(fileName));
                            copy?.Save(dr + Path.GetFileNameWithoutExtension(fileName)
                                       + ".tif");
                            copy?.Dispose();
                            image2?.Dispose();
                                // }
                               
                            }
                            else
                            {
                                using (MagickImage approvedlolcat2 = (MagickImage) images.AppendVertically())
                                {
                                   var approvedlolcat = ImagePreProcessing.EnhanceImageQuality(approvedlolcat2);
                                    copy = new Bitmap(approvedlolcat.Width * 1 / 2, approvedlolcat.Height * 1 / 2,
                                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                                    using (Graphics gr = Graphics.FromImage(copy))
                                    {
                                        gr.DrawImage(approvedlolcat.ToBitmap(),
                                            new Rectangle(0, 0, copy.Width, copy.Height));
                                    }

                                // rowlist = PerformingOcr.DoOcr2(copy, Path.GetFileNameWithoutExtension(fileName));

                                copy?.Save(dr + Path.GetFileNameWithoutExtension(fileName)
                                           + ".tif");

                            }
                            }
                            
                        }
                    
                }
                catch (Exception ex)
                {
                    var q = ex.Message;
                    // do nothing for now  
                   
                }
                if (File.Exists(fileName))
                {

                  
                        
                    try
                    {
                       
                        File.Delete(fileName);
                    }
                    catch (Exception ex)
                    {
                        return false;
                        // ignored
                    }
                }
            }
            
            return true;

        }
        public static bool IsFileLocked(string file)
        {
            FileStream stream = null;
            FileInfo currentFileInfo = new FileInfo(file);
            try
            {
                stream = currentFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        //public void Bradley(ref Bitmap tmp)
        //{
        //    BradleyLocalThresholding filter = new BradleyLocalThresholding();
        //    // filter.AssertCanApply(tmp.PixelFormat);
        //    filter.ApplyInPlace(tmp);
        //}


    }
}
