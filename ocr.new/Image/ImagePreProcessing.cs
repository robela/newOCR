using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FredsImageMagickScripts;
using ImageMagick;
using Ocr.Base.Autotrim;

namespace ocr.@new.Image
{
    class ImagePreProcessing
{
      
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
        public static void EnhanceImageQuality(string imageDirectory, string enhancedImage)
        {

            var EnhancedImage = new Uri(enhancedImage).LocalPath;

            // AutotrimScript.RemoveLines2(imageDirectory); 
            if (!Directory.Exists(EnhancedImage))
                Directory.CreateDirectory(EnhancedImage);

            if (imageDirectory != null && !IsFileLocked(imageDirectory))
            {
                try
                {
                    DirectoryInfo enhancedImageDir = new DirectoryInfo(EnhancedImage);
                    using (MagickImage image = new MagickImage(imageDirectory))
                    {
                        TextCleanerScript cleaner = new TextCleanerScript();
;
                        var new2Mage2 = cleaner.ConvertToGrayscale2(image);
                        //image.Despeckle();
                        new2Mage2.Magnify();
                        new2Mage2.Density = new Density(300, 300);
                       // new2Mage2 = AutotrimScript.RemoveLines2(new2Mage2);
                        new2Mage2.Write(enhancedImageDir + @"\" + (Path.GetFileNameWithoutExtension(imageDirectory ) + "-Darken" + Path.GetExtension(imageDirectory)));

                        if (File.Exists(imageDirectory))
                        {
                            //if (!File.Exists(enhancedImageDir + Path.GetFileName(imageDirectory)))
                            //    File.Copy(imageDirectory, enhancedImageDir + (Path.GetFileNameWithoutExtension(imageDirectory)) + "-Darken" + Path.GetExtension(imageDirectory));
                           // File.Delete(imageDirectory);
                        }
                       // whiteB.BorderColorLocation = new Coordinate(10, 10);
                    }

                }
                catch (Exception ex)
                {
                    var kk = ex.Data.Values;
                    
                }
            }
        }

        public static IMagickImage EnhanceImageQuality(MagickImage image)
        {
            TextCleanerScript cleaner = new TextCleanerScript();
            //AutotrimScript.RemoveLines(imageDirectory);
            AutotrimScript whiteB = new AutotrimScript();
            // WhiteB.InnerTrim = true;
           
            ;
            var new2Mage2 = cleaner.ConvertToGrayscale2(image);
            //image.Despeckle();
           // new2Mage2.Magnify();
            new2Mage2.Density = new Density(300, 300);

            //image.Despeckle();
           
            //new2Mage2 = AutotrimScript.RemoveLines2(new2Mage2);
            return new2Mage2;
        }
    }
}
