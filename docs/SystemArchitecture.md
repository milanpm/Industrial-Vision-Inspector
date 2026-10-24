# Industrial Vision Inspection System - Architecture

## Overview
An automated optical inspection (AOI) engine designed in C# utilizing OpenCV (OpenCvSharp). It processes industrial images via preprocessing, Otsu's thresholding, and contour/blob analysis to perform real-time Pass/Fail quality control.

## Key Features
- **Image Preprocessing**: Grayscale conversion and Gaussian Blur to reduce high-frequency noise.
- **Binarization**: Otsu's automatic thresholding for robust foreground-background segmentation.
- **Blob & Contour Analysis**: Extracts geometric contours, calculates bounding boxes and area sizes to detect defects.
- **Automated Grading**: Generates clear Pass/Fail decisions and visual overlays (bounding boxes and area metrics).

## Tech Stack
- **Language**: C# (.NET 9.0)
- **Computer Vision Library**: OpenCvSharp4
- **Development Environment**: PIXEL-STUDIO Local Workstation
