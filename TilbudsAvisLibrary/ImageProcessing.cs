using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XImgproc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

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
                    ProcessImage(imagePath, fileName[fileName.Length - 5]);
                }
            }
        }

        public void ProcessImage(string imagePath, int pageNumber)
        {
            // Step 1: Load the image
            Mat img = CvInvoke.Imread(imagePath, ImreadModes.Color);

            Mat cannyEdges = ConvertImageToCanny(img);

            DrawLinesAroundProducts(cannyEdges, img);

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

        private void DrawLinesAroundProducts(Mat cannyImage, Mat originalImage)
        {
            int height = cannyImage.Rows;
            int width = cannyImage.Cols;

            int leftRightMargin = (int)(cannyImage.Cols * 0.05);
            int topMargin = (int)(cannyImage.Rows * 0.20);
            int bottomMargin = (int)(cannyImage.Rows * 0.10);

            Rectangle roi = new Rectangle(leftRightMargin, topMargin,
                                          cannyImage.Cols - 2 * leftRightMargin,
                                          cannyImage.Rows - topMargin - bottomMargin);

            Mat roiImage = new Mat(cannyImage, roi);

            unsafe
            {
                List<Point[]> verticalLines = new List<Point[]>();
                List<Point[]> horizontalLines = new List<Point[]>();

                byte* roiData = (byte*)roiImage.DataPointer;
                int stride = roiImage.Step;

                // Draw and save horizontal lines
                for (int y = 0; y < roiImage.Rows; y++)
                {
                    bool isBlackRow = true;

                    for (int x = 0; x < roiImage.Cols; x++)
                    {
                        byte pixelValue = roiData[y * stride + x];

                        if (pixelValue != 0)
                        {
                            isBlackRow = false;
                            break;
                        }
                    }

                    if (isBlackRow)
                    {
                        Point startPoint = new Point(leftRightMargin, y + topMargin);
                        Point endPoint = new Point(originalImage.Cols - leftRightMargin, y + topMargin);

                        horizontalLines.Add(new Point[] { startPoint, endPoint });
                    }
                }
                Console.WriteLine(horizontalLines.Count);
                GroupAndDeleteLines(horizontalLines);
                Console.WriteLine(horizontalLines.Count);


                // Draw and save vertical lines
                for (int x = 0; x < roiImage.Cols; x++)
                {
                    bool isBlackColumn = true;

                    for (int y = 0; y < roiImage.Rows; y++)
                    {
                        byte pixelValue = roiData[y * stride + x];

                        if (pixelValue != 0)
                        {
                            isBlackColumn = false;
                            break;
                        }
                    }

                    if (isBlackColumn)
                    {
                        Point startPoint = new Point(x + leftRightMargin, topMargin);
                        Point endPoint = new Point(x + leftRightMargin, originalImage.Rows - bottomMargin);

                        verticalLines.Add(new Point[] { startPoint, endPoint });

                    }
                }
                GroupAndDeleteLines(verticalLines);

                DrawPoints(originalImage, horizontalLines, verticalLines);
            }
        }

        private void DrawPoints(Mat originalImage, List<Point[]> horizontalLines, List<Point[]> verticalLines)
        {
            foreach (var line in horizontalLines)
            {
                Point startPoint = line[0];
                Point endPoint = line[1];

                CvInvoke.Circle(originalImage, startPoint, 5, new MCvScalar(255, 0, 0), -1);
                CvInvoke.Circle(originalImage, endPoint, 5, new MCvScalar(0, 0, 255), -1);
            }

            foreach (var line in verticalLines)
            {
                Point startPoint = line[0];
                Point endPoint = line[1];

                CvInvoke.Circle(originalImage, startPoint, 5, new MCvScalar(0, 255, 0), -1);
                CvInvoke.Circle(originalImage, endPoint, 5, new MCvScalar(0, 50, 0), -1);
            }
        }


        private Mat ConvertImageToCanny(Mat img)
        {
            // Step 2: Convert to grayscale
            Mat grayImg = new Mat();
            CvInvoke.CvtColor(img, grayImg, ColorConversion.Bgr2Gray);

            // Step 3: Apply Gaussian Blur to reduce noise
            CvInvoke.GaussianBlur(grayImg, grayImg, new Size(3, 3), 0);

            // Step 4: Detect edges using Canny (edge detection)
            Mat cannyEdges = new Mat();
            CvInvoke.Canny(grayImg, cannyEdges, 100, 200);

            return cannyEdges;
        }

        private void GroupAndDeleteLines(List<Point[]> lines)
        {
            List<List<Point[]>> groupedLines = new List<List<Point[]>>();

            // Sort the lines by their y-coordinate
            lines.Sort((line1, line2) => line1[0].Y.CompareTo(line2[0].Y));

            // Group the lines that are close to each other
            foreach (Point[] line in lines)
            {
                bool addedToGroup = false;

                foreach (List<Point[]> group in groupedLines)
                {
                    // Check if the current line is close to any of the lines in the group
                    if (Math.Abs(line[0].Y - group[0][0].Y) <= 300)
                    {
                        group.Add(line);
                        addedToGroup = true;
                        break;
                    }
                }

                // If the current line is not close to any of the groups, create a new group
                if (!addedToGroup)
                {
                    groupedLines.Add(new List<Point[]> { line });
                }
            }

            // Delete all lines except the one in the center of each group
            foreach (List<Point[]> group in groupedLines)
            {
                if (group.Count > 1)
                {
                    int centerIndex = group.Count / 2;
                    Point[] centerLine = group[centerIndex];

                    // Process all lines except the central line
                    foreach (Point[] line in group)
                    {
                        if (line != centerLine)
                        {
                            lines.Remove(line);
                            // Delete or process the line as needed
                            // Example: lines.Remove(line);
                        }
                    }
                }
            }
        }

    }
}