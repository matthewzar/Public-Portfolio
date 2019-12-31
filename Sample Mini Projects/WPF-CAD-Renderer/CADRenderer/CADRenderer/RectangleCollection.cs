using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CADRenderer
{
    public class RectangleCollection
    {
        private RectangleF[] originalRectangles;
        public List<CADPoint> CADPoints { get; private set; }

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        public float width => maxX - minX;
        public float height => maxY - minY;
        public static int PixelsPerPoint = 10;

        public int Count => originalRectangles == null ? 0 : originalRectangles.Length;
        
        public static RectangleCollection GetCollectionFromCSV(string csvFile)
        {
            var passedHeader = false;
            List<CADPoint> parsedCadData = new List<CADPoint>();
            foreach (var line in File.ReadLines(csvFile))
            {
                //Don't work on any empty lines
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (passedHeader)
                {
                    var split = line.Split(',');
                    parsedCadData.Add(new CADPoint()
                    {
                        StepNo = split[1],
                        StepName = split[2],
                        X = float.Parse(split[15]),
                        Y = float.Parse(split[16]),
                        CoordType = "H",
                    });
                    parsedCadData.Add(new CADPoint()
                    {
                        StepNo = split[1],
                        StepName = split[2],
                        X = float.Parse(split[17]),
                        Y = float.Parse(split[18]),
                        CoordType = "L",
                    });
                }

                passedHeader = passedHeader || IsDataHeaderLine(line);
            }

            return new RectangleCollection(parsedCadData);
        }

        private RectangleCollection(List<CADPoint> cadPoints)
        {
            CADPoints = cadPoints;
            PopulateRectangleArray();

            //Find the most extreme offsets
            foreach (var rect in originalRectangles)
            {
                minX = Math.Min(rect.X, minX);
                minY = Math.Min(rect.Y, minY);
                maxX = Math.Max(rect.X, maxX);
                maxY = Math.Max(rect.Y, maxY);
            }
        }

        private void PopulateRectangleArray()
        {
            originalRectangles = originalRectangles == null ? new RectangleF[CADPoints.Count] : originalRectangles;
            
            for (int i = 0; i < CADPoints.Count; i++)
            {
                var currentPoint = CADPoints[i];
                originalRectangles[i] = new RectangleF(currentPoint.X, currentPoint.Y, PixelsPerPoint, PixelsPerPoint);
            }
        }

        private static bool IsDataHeaderLine(string line)
        {
            var split = line.Split(',');
            return split.Length > 2 && split[0] == "AST" && split[1].StartsWith("STEP-No.");
        }

        public int? FindClosestPointIndexToCoordinates(double xClick, double yClick, int sourceWidth, int sourceHeight)
        {
            var newXClick = width / sourceWidth * xClick;
            var newYClick = height / sourceHeight * yClick;
            
//            var newXClick = (width / sourceWidth) * xClick - xOffset / sourceWidth * xOffset;
//            var newYClick = (height / sourceHeight) * yClick - yOffset / sourceHeight * yOffset;

            var positiveRectangles = GetAllPositiveDimensionsClone();
            int? ret = null;
            double currentDistance = double.MaxValue;
            Console.WriteLine($"xClick:{xClick}, yClick:{yClick}, newXClick:{newXClick}, newYClick:{newYClick}");
            for (int i = 0; i < CADPoints.Count; i++)
            {
                var currentRect = positiveRectangles[i];

                var newDistance = getOffsetRectangleCentre(newXClick, newYClick, currentRect);
                if (newDistance < currentDistance)
                {
                    Console.WriteLine($"Current Point {currentRect.ToString()}, DistanceToPoint: {newDistance}");
                    currentDistance = newDistance;
                    ret = i;
                }
            }

            return ret;
        }

        private double getOffsetRectangleCentre(double x, double y, RectangleF target)
        {
            var centreX = (target.Left + target.Right) / 2.0;
            var centreY = (target.Top + target.Bottom) / 2.0;

            return Math.Sqrt(Math.Pow(centreX - x, 2) + Math.Pow(centreY - y, 2));
        }
        
        public Nullable<CADPoint> FindPointFromCoordinates(double xClick, double yClick, int sourceWidth, int sourceHeight)
        {
            var result = FindClosestPointIndexToCoordinates(xClick, yClick, sourceWidth, sourceHeight);

            if (result == null) return null;
            return CADPoints[(int)result];
        }

        public static float Scaler = 1;
        public static float xShiftPercent = 0;
        public static float yShiftPercent = 0;

        /// <summary>
        /// Clones the collections layout, while offsetting all values by the most
        /// extreme values on either axis (i.e. shift every coord to be positive).
        /// </summary>
        /// <param name="scalingFactor"></param>
        /// <param name="invertXAxis"></param>
        /// <param name="invertYAxis"></param>
        /// <returns></returns>
        public RectangleF[] GetAllPositiveDimensionsClone(float scalingFactor = 1f, bool invertXAxis = false, bool invertYAxis = false)
        {
            var ret = new RectangleF[originalRectangles.Length];

            for (int i = 0; i < ret.Length; i++)
            {
                var toClone = originalRectangles[i];

                //Move everyone based on the extreme XY coords. 
                var newY = toClone.Y - minY;
                newY = invertYAxis ? height - newY : newY;
                var newX = toClone.X - minX;
                newX = invertXAxis ? width - newX : newX;

                //Zoom in (spreads all points further apart)
                newY *= Scaler;
                newX *= Scaler;

                //Offsets the X and Y coordinates without changing zoom level
                newY += Scaler*height * yShiftPercent;
                newX += Scaler*width * xShiftPercent;



                ret[i] = new RectangleF(newX * scalingFactor,
                    newY * scalingFactor,
                    toClone.Width, toClone.Height);
            }

            return ret;
        }
    }
}
