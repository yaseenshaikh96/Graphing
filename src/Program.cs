/*
[]                          // first row taken up by magic
[oxxxxxxxxxxxxxxxxxxxx]     // screen starts one row down in console
[xxxxxxxxxxxxxxxxxxxxx]     // o = screen[0, 0]
[xxxxxxxxxxxxxxxxxxxxu]     // u = screen[2,15]

<-------------->    // this entire line in console is hidden as
[Xpow2         ]    // after printing the screen
[ |            ]    // it goes to a new line
[2|            ]    
[ |            ]    // screen[getLength(0), getlength(1)]
[1|            ]
[ |            ]
[-+------------]
[ |  1    2    ]
*/

using System.Runtime.InteropServices;

public class Program
{
    public delegate float floatFuncFloat(float x);
    public static string? msg;
    public const int GRAPH_PADDING = 2;
    public const int AXIS_PADDING = 3;

    public static void Main(string[] args)
    {
        const int width = 120, height = 30;
        SetWindowSize(width, height);

        (floatFuncFloat, string)[] sampleFunctions = {
            (SampleFunctions.XPow2, "Xpow2")
        };

        char[,] screen = new char[height - 1, width]; // screen[y,x] == screen[39, 160] // first row in console is taken by magic

        Point<int> origin = new Point<int>(screen.GetLength(1) / 2, screen.GetLength(0) / 2);
        Point<float> minValue = new Point<float>(-5, -9);
        Point<float> maxValue = new Point<float>(5, 9);
        Point<int> indicatorDensity = new Point<int>(8, 3);

        char[,] axis = MakeAxis(screen, minValue, maxValue, indicatorDensity);
        char[,] graphBorder = MakeBorder(screen);

        WriteToBuffer(screen, graphBorder, new Point<int>(0, 0));
        WriteToBuffer(screen, axis, new Point<int>(GRAPH_PADDING, GRAPH_PADDING));
        WriteToBuffer(screen, "functionName", new Point<int>(0, 0));

        Display(screen);

        System.Console.Write(msg);
        System.Console.Read();
    }

    static void SetWindowSize(int x, int y)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        System.Console.SetWindowSize(1, 1);
        System.Console.SetBufferSize(x, y);
        System.Console.SetWindowSize(x, y);
        System.Console.SetWindowPosition(0, 0);
    }

    static char[,] MakeAxis(char[,] screen, Point<float> minValue, Point<float> maxValue, Point<int> indicatorDensity)
    {
        Point<int> maxScreenPosPad = new Point<int>(screen.GetLength(1) - GRAPH_PADDING, screen.GetLength(0) - GRAPH_PADDING);
        char[,] axis = new char[maxScreenPosPad.y, maxScreenPosPad.x];
        Point<int> origin;
        Point<float> originVal;
        {
            Point<int> originAbs;
            originAbs.x = ValueToScrPos(maxValue.x, minValue.x, 0, maxScreenPosPad.x);
            originAbs.y = ValueToScrPos(maxValue.y, minValue.y, 0, maxScreenPosPad.y);

            if (originAbs.x > maxScreenPosPad.x - 1 - 2 - AXIS_PADDING)
                originAbs.x = maxScreenPosPad.x - 1 - 2 - AXIS_PADDING;
            if (originAbs.y > maxScreenPosPad.y + 1 - AXIS_PADDING)
                originAbs.y = maxScreenPosPad.y + 1 - AXIS_PADDING;
            if (originAbs.x < AXIS_PADDING + 2)
                originAbs.x = AXIS_PADDING + 2;
            if (originAbs.y < AXIS_PADDING + 1)
                originAbs.y = AXIS_PADDING + 1;

            originVal.x = ScrPosToValue(maxValue.x, minValue.x, maxScreenPosPad.x, originAbs.x);
            originVal.y = ScrPosToValue(maxValue.y, minValue.y, maxScreenPosPad.y, originAbs.y);

            origin = new Point<int>(originAbs.x, maxScreenPosPad.y - originAbs.y);
        }

        // axis[origin.y + 1, origin.x + 1] = ToChar((int)originVal.x);
        // axis[origin.y - 1, origin.x - 1] = ToChar((int)originVal.y);

        Point<int> minPos;
        minPos.x = ValueToScrPos(maxValue.x, minValue.x, minValue.x, maxScreenPosPad.x);
        minPos.y = ValueToScrPos(maxValue.y, minValue.y, minValue.y, maxScreenPosPad.y);
        Point<int> maxPos;
        maxPos.x = ValueToScrPos(maxValue.x, minValue.x, maxValue.x, maxScreenPosPad.x);
        maxPos.y = ValueToScrPos(maxValue.y, minValue.y, maxValue.y, maxScreenPosPad.y);


        {
            Point<int> indicatorRowCol = new Point<int>(origin.x - 1, origin.y + 1);
            for (int y = GRAPH_PADDING - 1; y < maxScreenPosPad.y - GRAPH_PADDING; y++)
                axis[y, origin.x] = '|';
            for (int x = GRAPH_PADDING - 1; x < maxScreenPosPad.x - GRAPH_PADDING; x++)
                axis[origin.y, x] = '-';
            axis[origin.y, origin.x] = '+';

            for (int x = 1; x < maxScreenPosPad.x; x++)
            {
                if (x % indicatorDensity.x != 0) continue;
                int xPos = (int)Remap(0, maxScreenPosPad.x, minPos.x, maxPos.x, x);
                float xVal = ScrPosToValue(maxValue.x, minValue.x, maxScreenPosPad.x, xPos);
                string xValstr = ToString(xVal);
                WriteToBuffer(axis, xValstr, new Point<int>(xPos - 2, indicatorRowCol.y));
                axis[indicatorRowCol.y - 1, xPos] = '+';
            }

            for (int y = 1; y < maxScreenPosPad.y; y++)
            {
                if (y % indicatorDensity.y != 0) continue;
                int yPos = (int)Remap(0, maxScreenPosPad.y, minPos.y, maxPos.y, y);
                float yVal = ScrPosToValue(maxValue.y, minValue.y, maxScreenPosPad.y, yPos);
                yPos = maxScreenPosPad.y - yPos;
                string yValStr = ToString(yVal);
                WriteToBuffer(axis, yValStr, new Point<int>(indicatorRowCol.x - 3, yPos));
                axis[yPos, indicatorRowCol.x + 1] = '+';
            }
        }

        msg = "origin: " + origin.x + ", " + origin.y + "; originval: " + originVal.x + ", " + originVal.y;

        return axis;
    }

    static char[,] MakeBorder(char[,] screen)
    {
        Point<int> maxScreenPos = new Point<int>(screen.GetLength(1) - 1, screen.GetLength(0));
        char[,] border = new char[maxScreenPos.y, maxScreenPos.x];
        for (int x = GRAPH_PADDING; x < maxScreenPos.x - GRAPH_PADDING; x++)
        {
            border[GRAPH_PADDING, x] = '-';
            border[maxScreenPos.y - GRAPH_PADDING, x] = '-';
        }
        for (int y = GRAPH_PADDING; y < maxScreenPos.y - GRAPH_PADDING; y++)
        {
            border[y, GRAPH_PADDING] = '|';
            border[y, maxScreenPos.x - GRAPH_PADDING] = '|';
        }
        border[GRAPH_PADDING, GRAPH_PADDING] = '+';
        border[maxScreenPos.y - GRAPH_PADDING, GRAPH_PADDING] = '+';
        border[GRAPH_PADDING, maxScreenPos.x - GRAPH_PADDING] = '+';
        border[maxScreenPos.y - GRAPH_PADDING, maxScreenPos.x - GRAPH_PADDING] = '+';
        return border;
    }


    static void WriteToBuffer(char[,] buffer, string text, Point<int> point)
    {
        for (int i = 0; i < text.Length; i++)
            buffer[point.y, point.x + i] = text[i];
    }
    static void WriteToBuffer(char[,] buffer, char[,] value, Point<int> point)
    {
        for (int x = 0; x < value.GetLength(1); x++)
            for (int y = 0; y < value.GetLength(0); y++)
                if (!(value[y, x] == '\0'))
                    buffer[y + point.y, x + point.x] = value[y, x];
    }

    static void Display(char[,] screen)
    {
        for (int y = 0; y < screen.GetLength(0); y++)
        {
            string line = "";
            for (int x = 0; x < screen.GetLength(1); x++)
            {
                line += screen[y, x];
            }
            System.Console.Write(line);
        }
    }

    static char ToChar(int value) => (char)(value + '0');

    static string ToString(float value)
    {
        string output = value.ToString();
        int index;
        if (output.Length > 3)
            index = 3;
        else
            index = output.Length;

        output = output.Substring(0, index);
        return output.PadLeft(3);
    } // three most significant digit


    static float Map(int x, int in_min, int in_max, int out_min, int out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
    static int ValueToScrPos(float maxValue, float minValue, float currentValue, int maxScreenPos)
    {
        return System.Convert.ToInt32(Remap(minValue, maxValue, 0, maxScreenPos, currentValue));
    }
    static float ScrPosToValue(float maxValue, float minValue, int maxScreenPos, int currentPos)
    {
        return Remap(0, maxScreenPos, minValue, maxValue, currentPos);
    }

    static float Lerp(float a, float b, float t) => (1f - t) * a + b * t;
    static float InvLerp(float a, float b, float value) => (value - a) / (b - a);
    static float Remap(float iMin, float iMax, float oMin, float oMax, float value)
    {
        float t = InvLerp(iMin, iMax, value);
        return Lerp(oMin, oMax, t);
    }
}

// static float MYScreenPosToValue(int maxScreenPos, int currentScrenPos, float minValue, float maxValue)
// {
//     float ratio = currentScrenPos / (float)maxScreenPos;
//     float valueDiff = Math.Abs(maxValue - minValue);
//     float absValue = valueDiff * ratio;
//     return absValue + minValue;
// }
//
// static int ValueToScreenPos(int currentValue, int maxValue, int screenMax)
// {
//     // current/max = 0-1;
//     // 0--------------SM
//     // float ratio = currentLength / (float)totalLength;
//     // return 2 * valueEnd * ratio;
//     int ratio = currentValue / maxValue;
//     return ratio * screenMax;
// }
//
// static int ScreenPos(int origin, int totalLength, int currentLength, float endValue)
// {
//     // current/total = 0-1
//     // endValue * above = 0-endValue
//     // 0---------------O--------TL
//     // 0------------------------EV
//     int totalLengthN = -(totalLength - (totalLength - origin));
//     int totalLengthP = totalLength - origin;
//     return (int)Map(currentLength, 0, totalLength, totalLengthN, totalLengthP);
// }
// static void DrawAxis(char[,] screen, Point origin, Point maxValue)
// {

//     for (int y = 1; y < screen.GetLength(0) - 1; y++) // first, last row kept empty. first row for function name
//         screen[y, origin.x] = '|';

//     for (int x = 1; x < screen.GetLength(1) - 1; x++) // firat, last col kept empty
//         screen[origin.y, x] = '-';

//     screen[origin.y, origin.x] = '+';
//     AddIndicator(screen, origin, maxValue, 5);


//     static void AddIndicator(
//         char[,] screen,
//         Point origin, Point maxValue,
//         int numOfIndocator = 5)
//     {
//         int distBetnIndicatorY = screen.GetLength(0) / numOfIndocator;
//         int distBetnIndicatorX = screen.GetLength(0) / numOfIndocator;
//         distBetnIndicatorX *= 2;
//         int yIndicatorCol = origin.x - 1; // opposite meaning, screen[y, x], screen[y, yIndocatorCol].
//         int xIndicatorRow = origin.y + 1; // yIndicatorCol is a X value. it is fixed so y can vary

//         screen[xIndicatorRow, origin.x + 1] = '0';                           // near origin x
//         screen[origin.y - 1, yIndicatorCol] = '0';                           // near origin y
//         screen[xIndicatorRow, screen.GetLength(1) - 2] = ToChar((int)ScreenPosToValue(screen.GetLength(1), screen.GetLength(1) - origin.x, maxValue.x)); // x +
//         screen[xIndicatorRow, 2] = ToChar((int)ScreenPosToValue(screen.GetLength(1), origin.x + 1, maxValue.x));                                         // x -
//         screen[1, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), origin.y + 1, maxValue.y));                                         // y +
//         screen[screen.GetLength(0) - 2, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), screen.GetLength(0) - origin.y, maxValue.y)); // y -
//         screen[screen.GetLength(0) - 2, yIndicatorCol - 1] = '-';
//         screen[xIndicatorRow, 1] = '-';


//         for (int y = 1; y < origin.y; y++)
//             if (y % distBetnIndicatorY == 0)
//                 screen[origin.y - y, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), y, maxValue.y));

//         for (int x = 1; x < screen.GetLength(1) - origin.x; x++)
//             if (x % distBetnIndicatorX == 0)
//                 screen[xIndicatorRow, origin.x + x] = ToChar((int)ScreenPosToValue(screen.GetLength(1), x, maxValue.x));

//         for (int x = 1; x < origin.x; x++)
//             if (x % distBetnIndicatorX == 0)
//             {
//                 screen[xIndicatorRow, origin.x - x] = ToChar((int)ScreenPosToValue(screen.GetLength(1), x, maxValue.x));
//                 screen[xIndicatorRow, origin.x - x - 1] = '-';
//             }
//         for (int y = 1; y < screen.GetLength(0) - origin.y; y++)
//             if (y % distBetnIndicatorY == 0)
//             {
//                 screen[origin.y + y, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), y, maxValue.y));
//                 screen[origin.y + y, yIndicatorCol - 1] = '-';
//             }


//     }
// }
// static void WriteFunctionToBuffer(char[,] screen, floatFuncFloat func, Point origin, Point maxValue)
// {
//     // Point maxScreenPos = new Point(screen.GetLength(1), screen.GetLength(0));

//     // for (int xPos = 0; xPos < screen.GetLength(1) / 2; xPos++)
//     // {
//     //     float xVal = ScreenPosToValue(maxScreenPos.x, xPos, maxValue.x);
//     //     float yVal = func(xVal);
//     //     int yPos = ValueToScrPos(maxValue.y, yVal, maxScreenPos.y);
//     //     if (origin.y - yPos < maxScreenPos.y / 2)
//     //         screen[origin.y - yPos, xPos + origin.x] = 'o';
//     // }
//     // for (int x = 0; x > -screen.GetLength(1) / 2; x--)
//     // {

//     // }

//     // for (int x = -screen.GetLength(1) / 2; x < screen.GetLength(1) / 2; x++)
//     // {
//     //     int y = (int)func(screenPosToValue(screen.GetLength(1), x, xEnd));
//     //     int xPos = ScreenPos(originX, screen.GetLength(1), x, xEnd);
//     //     int yPos = screen.GetLength(0) - ScreenPos(originY, screen.GetLength(0), y, yEnd);

//     //     if (yPos > screen.GetLength(0) - 3 / 2)
//     //         screen[originY - y, x + screen.GetLength(1) / 2] = 'o';
//     // }
// }