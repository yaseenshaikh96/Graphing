/*
[]                          // first row taken up by magic
[oxxxxxxxxxxxxxxxxxxxx]     // screen starts one row down in console
[xxxxxxxxxxxxxxxxxxxxx]     // o = screen[0, 0]
[xxxxxxxxxxxxxxxxxxxxu]     // u = screen[2,15]

<-------------->    // this entire line in console is hidden as
[Xpow2         ]    // after printing the screen
[ |            ]    // it goes to a new line
[2|            ]    
[ |            ]
[1|            ]
[ |            ]
[-+------------]
[ |  1    2    ]
*/

using System.Runtime.InteropServices;

public class Program
{
    public static void Main(string[] args)
    {
        const int width = 120, height = 30;
        SetWindowSize(width, height);

        SampleFunctions.floatFuncFloatOutString[] sampleFunctions = {
            SampleFunctions.Xpow2
        };

        string functionName = "";
        sampleFunctions[0](0, out functionName);
        functionName = "XPow2";

        char[,] screen = new char[height - 1, width]; // screen[y,x] == screen[39, 160] // first row in console is taken by magic

        WriteToScreenBuffer(screen, functionName, 0, 0);
        DrawAxis(screen, 0, 9, 0, 9);

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

    static void WriteToScreenBuffer(char[,] screen, string text, int x, int y)
    {
        for (int i = 0; i < text.Length; i++)
            screen[y, x + i] = text[i];
    }

    static void DrawAxis(char[,] screen, float xStart, float xEnd, float yStart, float yEnd)
    {
        int xAxisRow = screen.GetLength(0) - 3;
        int yAxisCol = 2;

        xAxisRow = screen.GetLength(0) / 2; // test
        yAxisCol = screen.GetLength(1) / 2; // test

        for (int y = 1; y < screen.GetLength(0) - 1; y++) // first, last row kept empty. first row for function name
            screen[y, yAxisCol] = '|';

        for (int x = 1; x < screen.GetLength(1) - 1; x++) // firat, last col kept empty
            screen[xAxisRow, x] = '-';

        screen[xAxisRow, yAxisCol] = '+';
        AddIndicator(screen, xStart, xEnd, yStart, yEnd, xAxisRow, yAxisCol, 5);


        static void AddIndicator(
            char[,] screen,
            float xStart, float xEnd,
            float yStart, float yEnd,
            int xAxisRow, int yAxisCol,
            int numOfIndocator = 5)
        {
            int indicatorPosY = (int)Math.Round(screen.GetLength(0) / (float)numOfIndocator, MidpointRounding.AwayFromZero);
            int indicatorPosX = (int)Math.Round(screen.GetLength(1) / (float)numOfIndocator, MidpointRounding.AwayFromZero);

            int xIndicatorRow = xAxisRow + 1;
            int yIndicatorCol = yAxisCol - 1;

            // add xStart, xEnd, yStart, yEnd
            screen[1, yIndicatorCol] = ToChar((int)yEnd);
            screen[xIndicatorRow - 2, yIndicatorCol] = ToChar((int)yStart);
            screen[xIndicatorRow, screen.GetLength(1) - 2] = ToChar((int)xEnd); // last row kept empty
            screen[xIndicatorRow, 3] = ToChar((int)xStart);

            for (int y = 1; y < screen.GetLength(0); y++)
                if (y % indicatorPosY == 0)
                    screen[y, yIndicatorCol] = ToChar((int)ValueOfIndicator(screen.GetLength(0), screen.GetLength(0) - y, yStart, yEnd));

            for (int x = 1; x < screen.GetLength(1); x++)
                if (x % indicatorPosX == 0)
                    screen[xIndicatorRow, x] = ToChar((int)ValueOfIndicator(screen.GetLength(1), x, xStart, xEnd));


            static float ValueOfIndicator(int totalLength, int currentLength, float valueStart, float valueEnd)
            {
                float ratio = currentLength / (float)totalLength;
                return valueStart + (valueEnd - valueStart) * ratio;
            }
        }
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

}