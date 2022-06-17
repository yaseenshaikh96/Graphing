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

    public static void Main(string[] args)
    {
        const int width = 120, height = 30;
        SetWindowSize(width, height);

        (floatFuncFloat, string)[] sampleFunctions = {
            (SampleFunctions.XPow2, "Xpow2")
        };

        char[,] screen = new char[height - 1, width]; // screen[y,x] == screen[39, 160] // first row in console is taken by magic

        WriteToScreenBuffer(screen, "functionName", 0, 0);
        DrawAxis(screen, screen.GetLength(1) / 2, screen.GetLength(0) / 2, 5, 5);

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

    static void DrawAxis(char[,] screen, int originX, int originY, float xEnd, float yEnd)
    {
        for (int y = 1; y < screen.GetLength(0) - 1; y++) // first, last row kept empty. first row for function name
            screen[y, originX] = '|';

        for (int x = 1; x < screen.GetLength(1) - 1; x++) // firat, last col kept empty
            screen[originY, x] = '-';

        screen[originY, originX] = '+';
        AddIndicator(screen, xEnd, yEnd, originY, originX, 5);


        static void AddIndicator(
            char[,] screen,
            float xEnd, float yEnd, // first Quad, other Quads are calculated from this
            int originY, int originX,
            int numOfIndocator = 5)
        {
            int distBetnIndicatorY = screen.GetLength(0) / numOfIndocator;
            int distBetnIndicatorX = screen.GetLength(0) / numOfIndocator;
            distBetnIndicatorX *= 2;
            int yIndicatorCol = originX - 1; // opposite meaning, screen[y, x], screen[y, yIndocatorCol].
            int xIndicatorRow = originY + 1; // yIndicatorCol is a X value. it is fixed so y can vary

            screen[xIndicatorRow, originX + 1] = '0';                           // near origin x
            screen[originY - 1, yIndicatorCol] = '0';                           // near origin y
            screen[xIndicatorRow, screen.GetLength(1) - 2] = ToChar((int)ValueOfIndicator(screen.GetLength(1), screen.GetLength(1) - originX, xEnd)); // x +
            screen[xIndicatorRow, 2] = ToChar((int)ValueOfIndicator(screen.GetLength(1), originX + 1, xEnd));                                         // x -
            screen[1, yIndicatorCol] = ToChar((int)ValueOfIndicator(screen.GetLength(0), originY + 1, yEnd));                                         // y +
            screen[screen.GetLength(0) - 2, yIndicatorCol] = ToChar((int)ValueOfIndicator(screen.GetLength(0), screen.GetLength(0) - originY, yEnd)); // y -
            screen[screen.GetLength(0) - 2, yIndicatorCol - 1] = '-';
            screen[xIndicatorRow, 1] = '-';


            for (int y = 1; y < originY; y++)
                if (y % distBetnIndicatorY == 0)
                    screen[originY - y, yIndicatorCol] = ToChar((int)ValueOfIndicator(screen.GetLength(0), y, yEnd));

            for (int x = 1; x < screen.GetLength(1) - originX; x++)
                if (x % distBetnIndicatorX == 0)
                    screen[xIndicatorRow, originX + x] = ToChar((int)ValueOfIndicator(screen.GetLength(1), x, xEnd));

            for (int x = 1; x < originX; x++)
                if (x % distBetnIndicatorX == 0)
                {
                    screen[xIndicatorRow, originX - x] = ToChar((int)ValueOfIndicator(screen.GetLength(1), x, xEnd));
                    screen[xIndicatorRow, originX - x - 1] = '-';
                }
            for (int y = 1; y < screen.GetLength(0) - originY; y++)
                if (y % distBetnIndicatorY == 0)
                {
                    screen[originY + y, yIndicatorCol] = ToChar((int)ValueOfIndicator(screen.GetLength(0), y, yEnd));
                    screen[originY + y, yIndicatorCol - 1] = '-';
                }


            static float ValueOfIndicator(int totalLength, int currentLength, float valueEnd)
            {

                float ratio = currentLength / (float)totalLength;
                return 2 * valueEnd * ratio; // twice cause origin to one end and also to another end
            }
        }
    }

    // static void WriteFunctionToBuffer(char[,] screen, )
    // {

    // }

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