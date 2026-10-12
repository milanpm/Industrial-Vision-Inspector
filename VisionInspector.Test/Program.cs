/*
 * File Name: Program.cs
 * Date: 2026-10-12
 * Author: milanpm (xenrl@naver.com)
 * Description: All-in-one console test runner and vision inspection engine 
 *              utilizing OpenCvSharp for automated Pass/Fail quality control.
 */

using System;
using System.IO;
using OpenCvSharp;

namespace VisionInspector.Test
{
    // 비전 검사 엔진 클래스 통합
    public class VisionInspectionEngine
    {
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
                if (area < 50) continue; // 노이즈 제거

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
        public Mat ProcessedImage { get; set; } = new Mat(); // 기본값 초기화 추가
    }

    // 프로그램 실행 진입점
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Industrial Vision Inspection Test Started ===");

            string testImagePath = "test_sample.jpg"; 

            // 테스트용 이미지가 없으면 더미 이미지 생성
            if (!File.Exists(testImagePath))
            {
                Console.WriteLine("테스트용 이미지가 없어 더미 이미지를 생성합니다...");
                using (Mat dummy = new Mat(300, 300, MatType.CV_8UC3, new Scalar(255, 255, 255)))
                {
                    Cv2.Circle(dummy, new Point(150, 150), 40, new Scalar(0, 0, 0), -1);
                    Cv2.ImWrite(testImagePath, dummy);
                }
            }

            try
            {
                var engine = new VisionInspectionEngine();
                var result = engine.Inspect(testImagePath, 100, 10000);

                Console.WriteLine($"[검사 완료]");
                Console.WriteLine($"- 전체 판정 결과 (Overall Pass): {(result.IsOverallPass ? "PASS" : "FAIL")}");
                Console.WriteLine($"- 검출된 불량/객체 수: {result.DefectCount}");

                string outputImagePath = "result_output.jpg";
                Cv2.ImWrite(outputImagePath, result.ProcessedImage);
                Console.WriteLine($"- 결과 이미지가 '{outputImagePath}' 경로에 저장되었습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[오류 발생]: {ex.Message}");
            }

            Console.WriteLine("=== Test Finished ===");
        }
    }
}
