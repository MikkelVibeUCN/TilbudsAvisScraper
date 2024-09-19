using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XImgproc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TilbudsAvisLibrary
{
    public class ImageProcessing
    {
        public void ProcessAllImagesInFolder(string folderPath)
        {
            string[] imagePaths = Directory.GetFiles(folderPath, "*.jpg");

            foreach (string imagePath in imagePaths)
            {
                string fileName = Path.GetFileName(imagePath);
                string outputFolderPath = Path.Combine(folderPath, "output");

                if (!outputFolderPath.Equals(imagePath, StringComparison.OrdinalIgnoreCase))
                {
                    ProcessImage(imagePath, fileName[fileName.Length-5]);
                }
            }
        }
        public void ProcessImage(string imagePath, int pageNumber)
        {
            // Step 1: Load the image
            Mat img = CvInvoke.Imread(imagePath, ImreadModes.Color);

            // Step 2: Convert to grayscale
            Mat grayImg = new Mat();
            CvInvoke.CvtColor(img, grayImg, ColorConversion.Bgr2Gray);

            // Step 3: Apply Gaussian Blur to reduce noise
            CvInvoke.GaussianBlur(grayImg, grayImg, new Size(3, 3), 0);

            // Step 4: Detect edges using Canny (edge detection)
            Mat cannyEdges = new Mat();
            CvInvoke.Canny(grayImg, cannyEdges, 100, 200);

            // Get the height and width of the image
            int height = cannyEdges.Rows;
            int width = cannyEdges.Cols;

            // Calculate the row indices for top 30% and bottom 20%
            int topBoundary = (int)(0.30 * height);
            int bottomBoundary = (int)(0.80 * height);

            // Set top 30% region to black
            using (Mat topRegion = new Mat(cannyEdges, new Rectangle(0, 0, width, topBoundary)))
            {
                topRegion.SetTo(new MCvScalar(0));
            }

            // Set bottom 20% region to black
            using (Mat bottomRegion = new Mat(cannyEdges, new Rectangle(0, bottomBoundary, width, height - bottomBoundary)))
            {
                bottomRegion.SetTo(new MCvScalar(0));
            }

            // Additional Debugging Step: Show the image after edge detection
            CvInvoke.Imshow("Canny Edges", cannyEdges);  // Display the image with edges
            CvInvoke.WaitKey(0);  // Wait for a key press before closing the window

            // Step 5: Find contours (possible bounding boxes for prices)
            var contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            CvInvoke.FindContours(cannyEdges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // Step 10: Display or save the result
            string outputPath = @"C:\Users\mikvc\source\repos\TilbudsAvisScraper\ScraperConsole\bin\Debug\net8.0\RemaImages\output\output" + pageNumber + ".jpg";
            CvInvoke.Imwrite(outputPath, cannyEdges);  // Save the image with merged rectangles
            CvInvoke.Imshow("Detected Price Tags", img);  // Display the image with merged rectangles
            CvInvoke.WaitKey(0);  // Wait for a key press before closing the window
            CvInvoke.DestroyAllWindows();
        }
    }
}