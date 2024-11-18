using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ImageASCIIMatrix;

class Program
{
    static readonly char[] chars = { ' ', '.', ',', '-', '~', ':', ';', '=', '!', '*', '#', '$', '@' };
    static readonly int cellWidth = 10, cellHeight = 15;
    static readonly int width = 800, height = 600;
    static readonly int newWidth = width / cellWidth, newHeight = height / cellHeight;
    static readonly double norm = 255.0 / chars.Length;
    static int fontSpeed = 0;
    static Mat matrixWindow = new Mat(height, width, DepthType.Cv8U, 3);
    static VideoCapture capture = new VideoCapture(0);

    static void Main(string[] args)
    {
        capture.Set(CapProp.FrameWidth, width);
        capture.Set(CapProp.FrameHeight, height);

        double fpsStartTime = 0;
        double fps = 0;

        while (true)
        {
            Mat frame = new Mat();
            capture.Read(frame);
            if (frame.IsEmpty) break;

            Mat result = frame.Clone();
            Matrix(result);

            Framerate(ref fpsStartTime, ref fps);

            CvInvoke.Imshow("result", matrixWindow);

            if (CvInvoke.WaitKey(1) == 27) break;
        }

        CvInvoke.DestroyAllWindows();

    }

    static void Matrix(Mat image)
    {
        matrixWindow.SetTo(new MCvScalar(0, 0, 0));

        Mat smallImage = new Mat();
        CvInvoke.Resize(image, smallImage, new Size(newWidth, newHeight), interpolation: Inter.Linear);
        Mat grayImage = new Mat();
        CvInvoke.CvtColor(smallImage, grayImage, ColorConversion.Bgr2Gray);

        for (int i = 0; i < newHeight; i++)
        {
            for (int j = 0; j < newWidth; j++)
            {
                int intensity = grayImage.GetValue(i, j);
                int charIndex = (int)(intensity / norm);
                var color = smallImage.GetValue(i, j);
                int G = color;

                if (G == 0)
                {
                    if ((i * cellHeight + fontSpeed) < height)
                    {
                        MatrixBackground(i, j);
                    }
                    else
                    {
                        fontSpeed = 0;
                    }
                }
                else
                {
                    char c = chars[charIndex];
                    CvInvoke.PutText(matrixWindow, c.ToString(), new Point(j * cellWidth + 5, i * cellHeight + 12), FontFace.HersheySimplex, 0.4, new MCvScalar(0, G, 0), 1);
                }
            }
        }

        fontSpeed++;
    }

    static void MatrixBackground(int i, int j)
    {
        Random rand = new Random();
        int matrixText = rand.Next(0, 2);
        CvInvoke.PutText(matrixWindow, matrixText.ToString(), new Point(j * cellWidth, i * cellHeight + fontSpeed), FontFace.HersheySimplex, 0.4, new MCvScalar(80, 120 - i * 3, 0), 1);
    }

    static void Framerate(ref double startTime, ref double fps)
    {
        double endTime = DateTime.Now.TimeOfDay.TotalSeconds;
        double timeDiff = endTime - startTime;
        fps = 1.0 / timeDiff;
        startTime = endTime;
        string fpsText = $"FPS: {fps:F1}";
        CvInvoke.PutText(matrixWindow, fpsText, new Point(10, 20), FontFace.HersheySimplex, 0.5, new MCvScalar(255, 0, 255), 1);
    }
}

public static class MatExtension
{
    public static dynamic GetValues(this Mat mat, int row, int col)
    {
        var value = CreateElement3Channels(mat.Depth);
        Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 3);
        return value;
    }

    public static dynamic GetValue(this Mat mat, int channel, int row, int col)
    {
        var value = CreateElement3Channels(mat.Depth);
        Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 3);
        return value[channel];
    }

    public static dynamic GetValue(this Mat mat, int row, int col)
    {
        var value = CreateElement(mat.Depth);
        Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
        return value[0];
    }

    public static void SetValues(this Mat mat, int row, int col, dynamic value)
    {
        Marshal.Copy(value, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 3);
    }

    public static void SetValue(this Mat mat, int channel, int row, int col, dynamic value)
    {
        var element = GetValues(mat, row, col);
        var target = CreateElement(element, value, channel);
        Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 3);
    }

    public static void SetValue(this Mat mat, int row, int col, dynamic value)
    {
        var target = CreateElement(mat.Depth, value);
        Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
    }

    private static dynamic CreateElement(dynamic element, dynamic value, int channel)
    {
        element[channel] = value;
        return element;
    }

    private static dynamic CreateElement(DepthType depthType, dynamic value)
    {
        var element = CreateElement(depthType);
        element[0] = value;
        return element;
    }

    private static dynamic CreateElement3Channels(DepthType depthType)
    {
        if (depthType == DepthType.Cv8S)
        {
            return new sbyte[3];
        }

        if (depthType == DepthType.Cv8U)
        {
            return new byte[3];
        }

        if (depthType == DepthType.Cv16S)
        {
            return new short[3];
        }

        if (depthType == DepthType.Cv16U)
        {
            return new ushort[3];
        }

        if (depthType == DepthType.Cv32S)
        {
            return new int[3];
        }

        if (depthType == DepthType.Cv32F)
        {
            return new float[3];
        }

        if (depthType == DepthType.Cv64F)
        {
            return new double[3];
        }

        return new float[3];
    }

    private static dynamic CreateElement(DepthType depthType)
    {
        if (depthType == DepthType.Cv8S)
        {
            return new sbyte[1];
        }

        if (depthType == DepthType.Cv8U)
        {
            return new byte[1];
        }

        if (depthType == DepthType.Cv16S)
        {
            return new short[1];
        }

        if (depthType == DepthType.Cv16U)
        {
            return new ushort[1];
        }

        if (depthType == DepthType.Cv32S)
        {
            return new int[1];
        }

        if (depthType == DepthType.Cv32F)
        {
            return new float[1];
        }

        if (depthType == DepthType.Cv64F)
        {
            return new double[1];
        }

        return new float[1];
    }

}