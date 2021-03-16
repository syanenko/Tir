using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using UnityEngine;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;


public class Shot : MonoBehaviour
{
    // Number of displays in system
    public int displays = 0;

    // Current working mode
    public enum Mode
    {
        NoCamera,
        Preview,
        Calibrate,
        Shoot
    }

    public Mode mode;

    // Camera index in system cameras list
    public int cameraIndex;

    // Calibration parameters
    public int chessWidth;
    public int chessHeight;
    public int squareSize;
    public int waitChessboardSeconds;
    public float minShotInterval;

    // HSV theshold values
    /*
    public int hueLow;
    public int hueHigh;
    public int satLow;
    public int satHigh;
    */
    public int valueLow;
    public int valueHigh;

    // Blur values
    public int blurSize;
    public int blurSigmaX;
    public int blurSigmaY;

    // Circle detection values
    public double resolutionRatio;
    public double minDistCentersRatio;
    public double param1;
    public double param2;
    public int minRadius;
    public int maxRadius;

    // Debug frames recording 
    public bool recordFrames = false;
    public string framesLocation = "с:/temp/shots/frame_";

    private int frameCount = 0;

    // Camera
    private VideoCapture vc;
    private Mat frame;
    private Mat grayFrame;
    bool newFrame = false;

    // Camera calibration
    private VectorOfPointF corners;
    private Mat cameraMatrix;
    private Mat distCoeffs;
    Mat[] _rvecs, _tvecs;
    GameObject chessboard;
    float timer = 0;

    private Image<Hsv, Byte> image;
    private Image<Gray, Byte> imageThreshed;
    private Image<Gray, Byte> imageThreshedTemp;

    Mat perspTransform;

    private float ax, ay;
    private float vcHeightMargin;
    private float kx;
    private float ky;

    //
    // Start
    //
    void Start()
    {
        LoadSettings();

        // Camera calibration staff
        frame = new Mat();
        grayFrame = new Mat();
        corners = new VectorOfPointF();
        cameraMatrix = new Mat(3, 3, DepthType.Cv64F, 1);
        distCoeffs = new Mat(8, 1, DepthType.Cv64F, 1);

        // Circles reference coefficiants
        ax = ay = 0;

        // Hide calibration patter by default
        chessboard = GameObject.Find("Chessboard");
        chessboard.SetActive(false);

        // Initial mode
        // DEBUG !!!
        // mode = Mode.Preview;
        mode = Mode.Calibrate;
        // mode = Mode.Shoot;

        // Camera capture
        // DEBUG - fails without cameraIndex !!!

        WebCamDevice[] devices = WebCamTexture.devices;
        foreach (WebCamDevice d in devices)
        {
            Debug.Log("--- WEBCAM: " + d.name);
        }

        try
        {
            vc = new VideoCapture(cameraIndex);
            vc.ImageGrabbed += Vc_ImageGrabbed;
            vc.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Can not start capture - cameraIndex not available");
            mode = Mode.NoCamera;
            return;
        }

        // Init displays

        displays = Display.displays.Length;

        /*
        Display.displays[0].SetRenderingResolution(1280, 720);
        Display.displays[1].SetRenderingResolution(1280, 720);
        */

        /*
        Display.displays[0].Activate(1280, 720, 60);
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
        */
        /*
                Display.displays[0].Activate(1280, 720, 60);

                if (Screen.fullScreen)
                { 
                    Screen.fullScreen = false;
                }
        */

        /*
        Display.displays[0].SetParams(1280, 720, 0, 0);
        Display.displays[1].SetParams(1280, 720, 0, 0);

        for (int i = 0; i < Display.displays.Length; i++)
        {
            // Display.displays[i].Activate(Display.displays[i].systemWidth, Display.displays[i].systemHeight, 60);
            Display.displays[i].Activate(Display.displays[i].renderingWidth, Display.displays[i].renderingHeight, 60);
        }*/
    }

    //
    // Vc_ImageGrabbed
    //
    private void Vc_ImageGrabbed(object sender, EventArgs e)
    {
        if (!vc.Retrieve(frame))
        {
            Debug.Log("--- Vc_ImageGrabbed:Retrieve() == false");
            return;
        }

        newFrame = true;
    }


    //
    // Update
    //
    void Update()
    {
        SetControlLimits();
        CheckMouseShot();

        if (newFrame)
        {
            switch (mode)
            {
                // case Mode.NoCamera: WaitForCamera(); break;
                case Mode.Preview: Preview(frame); break;
                case Mode.Calibrate: Calibrate(frame); break;
                case Mode.Shoot: DetectShoot(frame); break;
            }

            newFrame = false;
        }
    }


    //
    // Start shooting
    //
    public void StartrShooting()
    {
        mode = Shot.Mode.Shoot;
        Debug.Log("Shot:StartrShooting: mode set to: " + mode);
    }


    //
    // Stop shooting
    //
    public void StopShooting()
    {
        mode = Shot.Mode.Preview;
        Debug.Log("Shot:StopShooting: mode set to: " + mode);
    }


    //
    // Reset settings to default values
    //
    public void ResetToDefault()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
    }

    //
    // Load settings
    //
    public void LoadSettings()
    {
        Debug.Log("Loading setting");
        // Camera
        cameraIndex = PlayerPrefs.GetInt("cameraIndex");

        // Calibration parameters
        chessWidth         = PlayerPrefs.GetInt("chessWidth");
        chessHeight        = PlayerPrefs.GetInt("chessHeight");
        squareSize         = PlayerPrefs.GetInt("squareSize");
        waitChessboardSeconds = PlayerPrefs.GetInt("waitChessboardSeconds");

        // HSV theshold values
        /*
        hueLow  = PlayerPrefs.GetInt("hueLow");
        hueHigh = PlayerPrefs.GetInt("hueHigh");
        satLow  = PlayerPrefs.GetInt("satLow");
        satHigh = PlayerPrefs.GetInt("satHigh");
        */
        valueLow  = PlayerPrefs.GetInt("valueLow");
        valueHigh = PlayerPrefs.GetInt("valueHigh");

        // Blur values
        blurSize   = PlayerPrefs.GetInt("blurSize");
        blurSigmaX = PlayerPrefs.GetInt("blurSigmaX");
        blurSigmaY = PlayerPrefs.GetInt("blurSigmaY");

        // Circle detection values
        resolutionRatio     = PlayerPrefs.GetFloat("resolutionRatio");
        minDistCentersRatio = PlayerPrefs.GetFloat("minDistCentersRatio");
        param1              = PlayerPrefs.GetFloat("param1");
        param2              = PlayerPrefs.GetFloat("param2");
        minRadius           = PlayerPrefs.GetInt("minRadius");
        maxRadius           = PlayerPrefs.GetInt("maxRadius");

        // First time init
        if (chessWidth == 0)
        {
            Debug.Log("Resetting to default values");

            // Camera
            cameraIndex = 0;

            // Calibration parameters
            chessWidth = 13;
            chessHeight = 6;
            squareSize = 5;
            waitChessboardSeconds = 2;

            // HSV theshold values
            /*
            hueLow = 0;
            hueHigh = 5;
            satLow = 0;
            satHigh = 255;
            */
            valueLow = 220;
            valueHigh = 255;

            // Blur values
            blurSize = 15;
            blurSigmaX = 0;
            blurSigmaY = 0;

            // Circle detection values
            resolutionRatio = 1;
            minDistCentersRatio = 1;
            param1 = 2;
            param2 = 1;
            minRadius = 1;
            maxRadius = 10;
        }
    }

    //
    // Save settings
    //
    public void SaveSettings()
    {
        Debug.Log("Saving settings");

        // Camera
        PlayerPrefs.SetInt("cameraIndex", cameraIndex);

        // Calibration parameters
        PlayerPrefs.SetInt("chessWidth", chessWidth);
        PlayerPrefs.SetInt("chessHeight", chessHeight);
        PlayerPrefs.SetInt("squareSize", squareSize);
        PlayerPrefs.SetInt("waitChessboardSeconds", waitChessboardSeconds);

        // HSV theshold values
        /*
        PlayerPrefs.SetInt("hueLow", hueLow);
        PlayerPrefs.SetInt("hueHigh", hueHigh);
        PlayerPrefs.SetInt("satLow", satLow);
        PlayerPrefs.SetInt("satHigh", satHigh);
        */
        PlayerPrefs.SetInt("valueLow", valueLow);
        PlayerPrefs.SetInt("valueHigh", valueHigh);

        // Blur values
        PlayerPrefs.SetInt("blurSize", blurSize);
        PlayerPrefs.SetInt("blurSigmaX", blurSigmaX);
        PlayerPrefs.SetInt("blurSigmaY", blurSigmaY);

        // Circle detection values
        PlayerPrefs.SetFloat("resolutionRatio", (float)resolutionRatio);
        PlayerPrefs.SetFloat("minDistCentersRatio", (float)minDistCentersRatio);
        PlayerPrefs.SetFloat("param1", (float)param1);
        PlayerPrefs.SetFloat("param2", (float)param2);
        PlayerPrefs.SetInt("minRadius", minRadius);
        PlayerPrefs.SetInt("maxRadius", maxRadius);
    }

    //
    // Waiting for cameraIndex turn on
    //
    /*
    void WaitForCamera()
    {
        // Delay before repeat attempt 
        timer += Time.deltaTime;
        if (timer < waitChessboardSeconds)
        {
            return;
        }
        timer = 0;

        // Atempt for cameraIndex init
        Start();
    }
    */

    //
    // View current frame
    // 
    void Preview(Mat frame)
    {
        CvInvoke.Imshow("Camera window", frame);
    }

    //
    // Calibrate cameraIndex and align to projector screen borders
    //
    void Calibrate(Mat frame)
    {
        // Delay before repeat attempt 
        timer += Time.deltaTime;
        if (timer < waitChessboardSeconds)
        {
            return;
        }
        timer = 0;

        // Show calibration pattern
        chessboard.SetActive(true);

        CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

        Size patternSize = new Size(chessWidth, chessHeight);

        bool found = CvInvoke.FindChessboardCorners(grayFrame, patternSize, corners, CalibCbType.AdaptiveThresh | CalibCbType.NormalizeImage); // CalibCbType.NormalizeImage | | CalibCbType.FastCheck ?
        if (found)
        {
            CvInvoke.DrawChessboardCorners(frame, patternSize, corners, found);

            // Increase accuracy
            CvInvoke.CornerSubPix(grayFrame, corners, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

            // Perpare objects list

            var objectList = new List<MCvPoint3D32f>();
            for (int i = 0; i < chessHeight; i++)
            {
                for (int j = 0; j < chessWidth; j++)
                {
                    objectList.Add(new MCvPoint3D32f(j * squareSize, i * squareSize, 0.0F));
                }
            }

            MCvPoint3D32f[][] cornersObjectList;
            PointF[][] cornersPointsList;
            VectorOfPointF[] cornersPointsVec;

            int _frameArrayBufferLength = 1;

            cornersObjectList = new MCvPoint3D32f[_frameArrayBufferLength][];
            cornersPointsList = new PointF[_frameArrayBufferLength][];
            cornersPointsVec = new VectorOfPointF[_frameArrayBufferLength];

            cornersObjectList[0] = objectList.ToArray();
            cornersPointsList[0] = corners.ToArray();

            //
            // Calibrate
            //
            double error = CvInvoke.CalibrateCamera(cornersObjectList, cornersPointsList, grayFrame.Size,
                                                    cameraMatrix, distCoeffs, CalibType.RationalModel, new MCvTermCriteria(30, 0.1), out _rvecs, out _tvecs);
            // Align to chessboard bordes
            PointF[] ptSrc = new PointF[4];
            ptSrc[0] = new PointF(cornersPointsList[0][0].X, cornersPointsList[0][0].Y); // Upper left
            ptSrc[1] = new PointF(cornersPointsList[0][chessWidth - 1].X, cornersPointsList[0][chessWidth - 1].Y); // Upper right
            ptSrc[2] = new PointF(cornersPointsList[0][chessWidth * (chessHeight - 1)].X, cornersPointsList[0][chessWidth * (chessHeight - 1)].Y); // Lower left
            ptSrc[3] = new PointF(cornersPointsList[0][(chessWidth * chessHeight) - 1].X, cornersPointsList[0][(chessWidth * chessHeight) - 1].Y); // Lower right

            CvInvoke.Circle(frame, Point.Round(ptSrc[0]), 10, new MCvScalar(0, 255, 0), 2);
            CvInvoke.Circle(frame, Point.Round(ptSrc[1]), 10, new MCvScalar(0, 255, 0), 2);
            CvInvoke.Circle(frame, Point.Round(ptSrc[2]), 10, new MCvScalar(0, 255, 0), 2);

            PointF[] atDst = new PointF[4];

            int cellSize = (vc.Width / (chessWidth + 3)); // 40

            float heightMargin = ((float)vc.Height - ((float)cellSize * 5F)) / 2F; // 140
            float widthMargin = (float)cellSize * 2;

            int vcActiveHeight = (cellSize * 9);
            vcHeightMargin = (vc.Height - vcActiveHeight) / 2; // 60

            kx = (float)Screen.width / (float)vc.Width;
            ky = (float)Screen.height / (float)vcActiveHeight; // 3

            atDst[0] = new PointF(widthMargin, heightMargin);                        // Up Left
            atDst[1] = new PointF(vc.Width - widthMargin, heightMargin);             // Up Right
            atDst[2] = new PointF(widthMargin, vc.Height - heightMargin);            // Low Right
            atDst[3] = new PointF(vc.Width - widthMargin, vc.Height - heightMargin); // Low Left

            CvInvoke.Circle(frame, Point.Round(atDst[0]), 10, new MCvScalar(0, 0, 255), 2);
            CvInvoke.Circle(frame, Point.Round(atDst[1]), 10, new MCvScalar(0, 0, 255), 2);
            CvInvoke.Circle(frame, Point.Round(atDst[2]), 10, new MCvScalar(0, 0, 255), 2);

            perspTransform = CvInvoke.GetPerspectiveTransform(ptSrc, atDst);

            // Enter shoot mode with delay
            Debug.Log("Calibration done - entering shoot mode");
            chessboard.SetActive(false);
            mode = Mode.Preview;
            StartCoroutine(SetModeWithDelay(Mode.Shoot));
        }
        else
        {
            Debug.Log("Calibration failed - chesssboard pattern not found");
        }

        CvInvoke.Imshow("Camera window", frame);
    }

    //
    // Set mode with delay
    //
    IEnumerator SetModeWithDelay(Mode _mode)
    {
        yield return new WaitForSeconds(waitChessboardSeconds);
        mode = _mode;
    }

    //
    // Processing image
    //
    void DetectShoot(Mat frame)
    {
        // DEBUG !!!
        // return;

        // Delay before repeat attempt
        /*
        timer += Time.deltaTime;
        if (timer < minShotInterval)
        {
            return;
        }*/

        // Thresholding

        /* Old empiric variant */
        /*
        CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);
        CvInvoke.Undistort(grayFrame, frame, cameraMatrix, distCoeffs);
        CvInvoke.WarpPerspective(frame, frame, perspTransform, frame.Size, Inter.Cubic, Warp.Default);
       
        image = frame.ToImage<Hsv, Byte>();
        Hsv low = new Hsv(hueLow, satLow, valueLow);
        Hsv high = new Hsv(hueHigh, satHigh, valueHigh);
        imageThreshed = image.InRange(low, high);
        */

        // New full range thresholding variant
        /*
        image = frame.ToImage<Hsv, Byte>();
        Hsv low = new Hsv(hueLow, satLow, valueLow);
        Hsv high = new Hsv(hueHigh, satHigh, valueHigh);
        imageThreshed = image.InRange(low, high);
        */

        // Thresholding by value (brightness) only

        imageThreshed = frame.ToImage<Gray, Byte>();
        imageThreshed = imageThreshed.InRange(new Gray(valueLow), new Gray(valueHigh));

        // Framing
        imageThreshedTemp = imageThreshed.Clone();
        CvInvoke.Undistort(imageThreshed, imageThreshedTemp, cameraMatrix, distCoeffs);
        CvInvoke.WarpPerspective(imageThreshedTemp, imageThreshed, perspTransform, frame.Size, Inter.Cubic, Warp.Default);

        // Bluring
        CvInvoke.GaussianBlur(imageThreshed, imageThreshed, new Size(blurSize, blurSize), blurSigmaX, blurSigmaY);

        // DEBUG !!!
        // CvInvoke.Imshow("Camera window", imageThreshed);
        // imageThreshed.Save(framesLocation + (frameCount++) + ".png");
        // return;

        // Detecting shot circle
        CircleF[] circles = CvInvoke.HoughCircles(imageThreshed, HoughType.Gradient, resolutionRatio, imageThreshed.Height / minDistCentersRatio, param1, param2, minRadius, maxRadius);

        // Debug.Log("--- Circles detected: total :" + circles.Length);

        for (int i = 0; i < circles.Length; i++)
        {
            ax = (ax + circles[i].Center.X) / 2;
            ay = (ay + circles[i].Center.Y) / 2;
            float dx = Mathf.Abs(circles[i].Center.X - ax);
            float dy = Mathf.Abs(circles[i].Center.Y - ay);

            // Bgr color = new Bgr(255, 255, 255);
            Gray color = new Gray(255);
            CvInvoke.Circle(imageThreshed, Point.Round(circles[i].Center), (int)circles[i].Radius, color.MCvScalar, 2);

            if (recordFrames)
            { 
                imageThreshed.Save(framesLocation + (frameCount++) + ".png");
            }

            // timer = 0; // Set delay detection timer

            // Debug.Log("Circle: X: " + circles[i].Center.X + " Y: " + circles[i].Center.Y + " R: " + circles[i].Radius + " dx: " + dx + " dy: " + dy);
            // Debug.Log("gwHeight: " + vc.Height + "vcHeightMargin: " + vcHeightMargin + "ky: " + ky);

            // Converted to screen space
            ProcessShot(new Vector3(circles[i].Center.X * kx, (vc.Height - (circles[i].Center.Y + vcHeightMargin)) * ky));
        }

        CvInvoke.Imshow("Camera window", imageThreshed);
    }

    //
    // Dummy mouse shot
    //  
    void CheckMouseShot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 shot = Input.mousePosition;
            shot.z = 10; // Step from the cameraIndex
            ProcessShot(shot);
        }
    }

    //
    // Processing shot (in screen space)
    //
    void ProcessShot(Vector3 shot)
    {
        if (mode != Mode.Shoot)
        {
            Debug.Log("ProcessShot: shot is not fixed (current mode is '" + mode + "') shot: " + shot);
            return;
        }

        // Debug.Log("ProcessShot: shot fixed: " + shot);

        EventManager.ShotDone(shot);
        return;
    }


    //
    // Set control limits
    //
    void SetControlLimits()
    {
        /*
        // Hue
        if (hueHigh< 0) hueHigh = 0;
        if (hueHigh > 180) hueHigh = 180;
        if (hueHigh<hueLow) hueHigh = hueLow;

        if (hueLow< 0) hueLow = 0;
        if (hueLow > 180) hueLow = 180;
        if (hueLow > hueHigh) hueLow = hueHigh;

        // Saturation
        if (satHigh< 0) satHigh = 0;
        if (satHigh > 255) satHigh = 255;
        if (satHigh<satLow) satHigh = satLow;

        if (satLow< 0) satLow = 0;
        if (satLow > 255) satLow = 255;
        if (satLow > satHigh) satLow = satHigh;
        */

        // Value
        if (valueHigh< 0) valueHigh = 0;
        if (valueHigh > 255) valueHigh = 255;
        if (valueHigh<valueLow) valueHigh = valueLow;

        if (valueLow< 0) valueLow = 0;
        if (valueLow > 255) valueLow = 255;
        if (valueLow > valueHigh) valueLow = valueHigh;

        // Blur
        if (blurSize <= 0) blurSize = 1;
        if (blurSize % 2 == 0) blurSize++;
        if (blurSize > 511) blurSize = 511;
    }

    //
    // Clean up
    //
    void OnApplicationQuit()
    {
        SaveSettings();
        vc.Stop();
        vc.Dispose();
        CvInvoke.DestroyAllWindows();
    }
}
