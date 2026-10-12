/*
 * File Name: VisionInspectionEngine.cs
 * Date: 2026-10-12
 * Author: milanpm (xenrl@naver.com)
 * Description: Core vision inspection engine utilizing OpenCV (OpenCvSharp) for 
 *              image preprocessing, thresholding, and blob/contour analysis 
 *              to perform automated Pass/Fail quality control.
 */

using System;
using OpenCvSharp;

namespace VisionInspector.Test
{
    public class VisionInspectionEngine
    {
        /// <summary>
        /// 입력된 영상을 바탕으로 비전 검사(Blob/Contour 분석)를 수행합니다.
        /// </summary>
        public InspectionResult Inspect(string inputImagePath, double minArea, double maxArea)
        {
            var result = new InspectionResult();

            using Mat srcImage = Cv2.ImRead(inputImagePath, ImreadModes.Color);
            if (srcImage.Empty())
            {
                throw new Exception("이미지를 불러올 수 없습니다.");
            }

            using Mat grayImage = new Mat();
            using Mat blurImage = new Mat();
            Cv2.CvtColor(srcImage, grayImage, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(grayImage, blurImage, new Size(5, 5), 0);

            using Mat threshImage = new Mat();
            Cv2.Threshold(blurImage, threshImage, 0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

            Cv2.FindContours(threshImage, out Point[][] contours, out _, 
                RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            int defectCount = 0;
            bool overallPass = true;

            Mat displayImage = srcImage.Clone();
            for (int i = 0; i < contours.Length; i++)
            {
                double area = Cv2.ContourArea(contours[i]);
                if (area < 50) continue;

                bool isPass = (area >= minArea && area <= maxArea);
                Scalar boxColor = isPass ? Scalar.Lime : Scalar.Red;

                if (!isPass)
                {
                    defectCount++;
                    overallPass = false;
                }

                Rect boundingBox = Cv2.BoundingRect(contours[i]);
                Cv2.Rectangle(displayImage, boundingBox, boxColor, 2);
                Cv2.PutText(displayImage, $"Area: {area:F0}", new Point(boundingBox.X, boundingBox.Y - 5),
                    HersheyFonts.HersheySimplex, 0.5, boxColor, 1);
            }

            result.IsOverallPass = overallPass;
            result.DefectCount = defectCount;
            result.ProcessedImage = displayImage;

            return result;
        }
    }

    public class InspectionResult
    {
        public bool IsOverallPass { get; set; }
        public int DefectCount { get; set; }
        public Mat ProcessedImage { get; set; }
    }
}
