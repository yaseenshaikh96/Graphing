﻿/*
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

    public static void Main(string[] args)
    {
        const int width = 120, height = 30;
        SetWindowSize(width, height);

        (floatFuncFloat, string)[] sampleFunctions = {
            (SampleFunctions.XPow2, "Xpow2")
        };

        char[,] screen = new char[height - 1, width]; // screen[y,x] == screen[39, 160] // first row in console is taken by magic

        Point origin = new Point(screen.GetLength(1) / 2, screen.GetLength(0) / 2);
        Point maxValue = new Point(5, 5);

        WriteToScreenBuffer(screen, "functionName", new Point(0, 0));
        // WriteFunctionToBuffer(screen, sampleFunctions[0].Item1, origin, maxValue);
        DrawAxis(screen, origin, maxValue);

        Display(screen);

        System.Console.Write("press any key to continue...");
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

    static void WriteToScreenBuffer(char[,] screen, string text, Point point)
    {
        for (int i = 0; i < text.Length; i++)
            screen[point.y, point.x + i] = text[i];
    }

    static void DrawAxis(char[,] screen, Point origin, Point maxValue)
    {

        for (int y = 1; y < screen.GetLength(0) - 1; y++) // first, last row kept empty. first row for function name
            screen[y, origin.x] = '|';

        for (int x = 1; x < screen.GetLength(1) - 1; x++) // firat, last col kept empty
            screen[origin.y, x] = '-';

        screen[origin.y, origin.x] = '+';
        AddIndicator(screen, origin, maxValue, 5);


        static void AddIndicator(
            char[,] screen,
            Point origin, Point maxValue,
            int numOfIndocator = 5)
        {
            int distBetnIndicatorY = screen.GetLength(0) / numOfIndocator;
            int distBetnIndicatorX = screen.GetLength(0) / numOfIndocator;
            distBetnIndicatorX *= 2;
            int yIndicatorCol = origin.x - 1; // opposite meaning, screen[y, x], screen[y, yIndocatorCol].
            int xIndicatorRow = origin.y + 1; // yIndicatorCol is a X value. it is fixed so y can vary

            screen[xIndicatorRow, origin.x + 1] = '0';                           // near origin x
            screen[origin.y - 1, yIndicatorCol] = '0';                           // near origin y
            screen[xIndicatorRow, screen.GetLength(1) - 2] = ToChar((int)ScreenPosToValue(screen.GetLength(1), screen.GetLength(1) - origin.x, maxValue.x)); // x +
            screen[xIndicatorRow, 2] = ToChar((int)ScreenPosToValue(screen.GetLength(1), origin.x + 1, maxValue.x));                                         // x -
            screen[1, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), origin.y + 1, maxValue.y));                                         // y +
            screen[screen.GetLength(0) - 2, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), screen.GetLength(0) - origin.y, maxValue.y)); // y -
            screen[screen.GetLength(0) - 2, yIndicatorCol - 1] = '-';
            screen[xIndicatorRow, 1] = '-';


            for (int y = 1; y < origin.y; y++)
                if (y % distBetnIndicatorY == 0)
                    screen[origin.y - y, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), y, maxValue.y));

            for (int x = 1; x < screen.GetLength(1) - origin.x; x++)
                if (x % distBetnIndicatorX == 0)
                    screen[xIndicatorRow, origin.x + x] = ToChar((int)ScreenPosToValue(screen.GetLength(1), x, maxValue.x));

            for (int x = 1; x < origin.x; x++)
                if (x % distBetnIndicatorX == 0)
                {
                    screen[xIndicatorRow, origin.x - x] = ToChar((int)ScreenPosToValue(screen.GetLength(1), x, maxValue.x));
                    screen[xIndicatorRow, origin.x - x - 1] = '-';
                }
            for (int y = 1; y < screen.GetLength(0) - origin.y; y++)
                if (y % distBetnIndicatorY == 0)
                {
                    screen[origin.y + y, yIndicatorCol] = ToChar((int)ScreenPosToValue(screen.GetLength(0), y, maxValue.y));
                    screen[origin.y + y, yIndicatorCol - 1] = '-';
                }


        }
    }

    static float ScreenPosToValue(int maxScreenPos, int currentPos, float maxValue)
    {
        float ratio = currentPos / (float)maxScreenPos;
        return 2 * maxValue * ratio; // twice cause origin to one end and also to another end
    }

    static void WriteFunctionToBuffer(char[,] screen, floatFuncFloat func, Point origin, Point maxValue)
    {
        Point maxScreenPos = new Point(screen.GetLength(1), screen.GetLength(0));

        for (int xPos = 0; xPos < screen.GetLength(1) / 2; xPos++)
        {
            float xVal = ScreenPosToValue(maxScreenPos.x, xPos, maxValue.x);
            float yVal = func(xVal);
            int yPos = ValueToScrPos(maxValue.y, yVal, maxScreenPos.y);
            if (origin.y - yPos < maxScreenPos.y / 2)
                screen[origin.y - yPos, xPos + origin.x] = 'o';
        }
        // for (int x = 0; x > -screen.GetLength(1) / 2; x--)
        // {

        // }

        // for (int x = -screen.GetLength(1) / 2; x < screen.GetLength(1) / 2; x++)
        // {
        //     int y = (int)func(screenPosToValue(screen.GetLength(1), x, xEnd));
        //     int xPos = ScreenPos(originX, screen.GetLength(1), x, xEnd);
        //     int yPos = screen.GetLength(0) - ScreenPos(originY, screen.GetLength(0), y, yEnd);

        //     if (yPos > screen.GetLength(0) - 3 / 2)
        //         screen[originY - y, x + screen.GetLength(1) / 2] = 'o';
        // }
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

    static float Map(int x, int in_min, int in_max, int out_min, int out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }


    static int ValueToScrPos(float maxValue, float currentValue, int maxScreenPos)
    {
        float ratio = currentValue / maxValue;
        return (int)(ratio * maxScreenPos);
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