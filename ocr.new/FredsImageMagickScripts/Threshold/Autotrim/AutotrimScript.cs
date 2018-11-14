using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
namespace Ocr.Base.Autotrim
{
    /// <summary>
    /// Automatically trim a (nearly) uniform color border around an image. If the image is rotated,
    /// one can trim to the bounding box around the image area or alternately trim to the maximum
    /// central area that contains no border pixels. The excess border does not have to completely
    /// surround the image. It may be only on one side. However, one must identify a coordinate
    /// within the border area for the algorithm to extract the base border color and also specify
    /// a fuzz value when the border color is not uniform. For simple border trimming of a normally
    /// oriented image or the bounding box of a rotated image, you may err somewhat towards larger
    /// than optimal fuzz values. For images that contain rotated picture data, when you want to
    /// trim to the central area, you should choose the smallest fuzz value that is appropriate.
    /// For images that contain rotated picture data, an estimate of the rotation angle is needed
    /// for the algorithm to work. However, setting the rotation angle to zero will let the
    /// algorithm determine the rotation angle. The resulting trim is usually pretty good for
    /// angles >= 5 degrees. If the result is off a little, you may use the left/right/top/bottom
    /// arguments to adjust the automatically determined trim region.
    /// </summary>
    public  class AutotrimScript
    {
        private MagickColor _borderColor;

        private class Line
        {
            public int X1;
            public int X2;
            public int Y;

            public Line(int x, int y)
            {
                X1 = x;
                X2 = x;
                Y = y;
            }
        }

        private void Crop(MagickImage image, MagickGeometry area)
        {
            
            image.Crop(area.X, area.Y, area.Width, area.Height);
            image.RePage();
        }

        private MagickColor GetBorderColor(MagickImage image)
        {
            try
            {
                //using (PixelCollection pixels = image.GetReadOnlyPixels())
                //{
                //    return pixels.GetPixel((int) BorderColorLocation.X, (int) BorderColorLocation.Y).ToColor();
                //}
                return (Color.White);
            }
            catch (Exception ex)
            {
                return (Color.White);
            }
        }
        // remove lines
        private static void RemoveLines(MagickImage original, MagickImage image, string geometryValue)
        {
            image.Scale(new MagickGeometry(geometryValue));
            MagickGeometry geometry = new MagickGeometry(original.Width, original.Height);
            geometry.IgnoreAspectRatio = true;
            image.Scale(geometry);

            image.AutoLevel();
            image.Shave(20, 20);
            image.Despeckle();
            image.ReduceNoise();
            
            image.Morphology(MorphologyMethod.Erode, Kernel.Diamond, 2);
            using (MagickImage clone = (MagickImage) image.Clone())
            {
                image.Negate();
                image.Composite(original, Gravity.Center);
            }
        }

        public static void RemoveLines(string filePath)
        {
            using (MagickImage image = new MagickImage(filePath)) // image001.tif
            {
                image.RePage(); // +repage
                //image.RemoveArtifact(filePath);
                using (MagickImage original = (MagickImage) image.Clone())
                {
                    using (MagickImage firstClone = (MagickImage) original.Clone()) // -clone 0
                    {
                        RemoveLines(original, firstClone, "x1!"); // -scale x1!
                        using (MagickImage secondClone = (MagickImage) firstClone.Clone()) // -clone 0
                        {
                            RemoveLines(secondClone, firstClone, "1x!"); // -scale x1!
                            //secondClone.Write(@"D:\result2.tiff");
                        }
                    }
                }
            }
        }
        public static MagickImage RemoveLines2(MagickImage firstClone)
        {
           
             
                        firstClone.Negate();
                        // firstClone.Composite(original, CompositeOperator.da);
                        firstClone.Morphology(MorphologyMethod.Thinning, "rectangle:1x30+0+0");


                        firstClone.Negate();
                        //firstClone.Write(@"d:\result2.tiff");
                        return firstClone;
                 
            
        }
        //private static void RemoveLines(MagickImage original, MagickImage image, string geometryValue)
        //{
        //    // -scale x1! and -scale 1x!
        //    image.Scale(new MagickGeometry(geometryValue));

        //    // -scale 321x522! (resize to original width/height)
        //    MagickGeometry geometry = new MagickGeometry(original.Width, original.Height);
        //    geometry.IgnoreAspectRatio = true;
        //    image.Scale(geometry);

        //    image.AutoLevel(); // -auto-level
        //    image.Threshold(50); // -threshold 50%
        //    image.Morphology(MorphologyMethod.Erode, Kernel.Diamond, 2); // -morphology erode diamond:2

        //    // ( -clone 1 )
        //    using (MagickImage clone = image.Clone())
        //    {
        //        image.Negate(); // -negate (this needs to be done after the clone in Magick.NET)

        //        // -compose over -composite (one of the images is used as a clipmask because there are 3 images)
        //        image.ClipMask = clone.Clone();
        //        image.Composite(original, Gravity.Center);
        //        image.ClipMask = null;
        //    }
        //}

        //public static void RemoveLines()
        //{
        //    using (MagickImage image = new MagickImage("image001.tif")) // image001.tif
        //    {
        //        image.RePage(); // +repage

        //        using (MagickImage original = image.Clone())
        //        {
        //            using (MagickImage firstClone = original.Clone()) // -clone 0
        //            {
        //                RemoveLines(original, firstClone, "x1!"); // -scale x1!
        //                using (MagickImage secondClone = firstClone.Clone()) // -clone 0
        //                {
        //                    RemoveLines(secondClone, firstClone, "1x!"); // -scale x1!
        //                    firstClone.Write("result2.tiff");
        //                }
        //            }
        //        }
        //    }
        //}
        // end of remove line 
       

     
        private void ExecuteOuterTrim(MagickImage image)
        {
            image.BackgroundColor = _borderColor;
            image.ColorFuzz = ColorFuzz;
            image.Trim();
            image.RePage();

            MagickGeometry geometry = new MagickGeometry(0, 0, image.Width, image.Height);
            //ShiftGeometry(geometry);
            Crop(image, geometry);
        }

        private bool IsBorderColor(PixelCollection pixels, int x, int y)
        {
            try
            {
                MagickColor color = pixels.GetPixel(x, y).ToColor();
                return color.FuzzyEquals(_borderColor, ColorFuzz);
            }
            catch(Exception ex)
            {
                return false;
            }
        }

       
        private static void SwapPoints(Line[] points)
        {
            Line swap;
            if (points[0].Y > points[1].Y)
            {
                swap = points[0];
                points[0] = points[1];
                points[1] = swap;
            }
            if (points[3].Y < points[2].Y)
            {
                swap = points[2];
                points[2] = points[3];
                points[3] = swap;
            }
        }

        private static MagickGeometry TestGeometry(MagickGeometry geometry, Line line1, Line line2)
        {
            int x = Math.Max(line1.X1, line2.X1);
            int y = line1.Y;

            int width = Math.Min(line1.X2, line2.X2) - x;
            int height = line2.Y - line1.Y;

            MagickGeometry newGeometry = new MagickGeometry(x, y, width, height);

            return newGeometry > geometry ? newGeometry : geometry;
        }

        /// <summary>
        /// Creates a new instance of the AutotrimScript class.
        /// </summary>
        public AutotrimScript()
        {
            Reset();
        }

        /// <summary>
        /// Any location within the border area for the algorithm to find the base border color.
        /// </summary>
       

        /// <summary>
        /// The fuzz amount specified as a percent 0 to 100. The default is zero which indicates that
        /// border is a uniform color. Larger values are needed when the border is not a uniform color
        /// and to trim the border of the rotated area where the image data is a blend with the
        /// border color.
        /// </summary>
        public Percentage ColorFuzz
        {
            get;
            set;
        }

        /// <summary>
        /// Mode of trim. Default is outer trim (false).
        /// </summary>
        public bool InnerTrim
        {
            get;
            set;
        }

        /// <summary>
        /// The number of extra pixels to shift the trim of the image.
        /// </summary>
      
        /// <summary>
        /// Automatically unrotates a rotated image and trims the surrounding border.
        /// </summary>
        /// <param name="input">The image to execute the script on.</param>
        public MagickImage Execute(MagickImage input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            MagickImage output = (MagickImage) input.Clone();
            _borderColor = GetBorderColor(output);

          
                ExecuteOuterTrim(output);

            return output;
        }

        /// <summary>
        /// Resets the script to the default setttings.
        /// </summary>
        public void Reset()
        {
           
        }
    }
}
