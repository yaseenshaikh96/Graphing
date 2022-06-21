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
    private const char horiChar = '-';
    private const char vertiChar = '|';
    private const char plusChar = '+';
    private const char funcChar = '*';

    private const ConsoleColor yAxisColor = ConsoleColor.DarkRed;
    private const ConsoleColor xAxisColor = ConsoleColor.DarkGreen;
    private const ConsoleColor originColor = ConsoleColor.DarkYellow;
    private const ConsoleColor borderColor = ConsoleColor.Blue;
    private const ConsoleColor funcColor = ConsoleColor.Magenta;

    private static ColorChar xAxisColorCh = new ColorChar(horiChar, xAxisColor);
    private static ColorChar xIndicatorColorCh = new ColorChar(plusChar, xAxisColor);
    private static ColorChar yAxisColorCh = new ColorChar(vertiChar, yAxisColor);
    private static ColorChar yIndicatorColorCh = new ColorChar(plusChar, yAxisColor);
    private static ColorChar originColorCh = new ColorChar(plusChar, originColor);
    private static ColorChar borderHoriColorCh = new ColorChar(horiChar, borderColor);
    private static ColorChar borderVertiColorCh = new ColorChar(vertiChar, borderColor);
    private static ColorChar borderCornerColorCh = new ColorChar(plusChar, borderColor);
    private static ColorChar funcColorCh = new ColorChar(funcChar, funcColor);



    public static void Main(string[] args)
    {
        const int width = 120, height = 30;
        SetWindowSize(width, height);

        (floatFuncFloat, string)[] sampleFunctions = {
            (SampleFunctions.XPow2, "Xpow2"),
            (SampleFunctions.XPow3, "Xpow3"),
            (SampleFunctions.sin, "Sin")
        };
        (floatFuncFloat, string) currentFunc = sampleFunctions[2];

        ColorChar[,] screen = new ColorChar[height - 1, width]; // screen[y,x] == screen[39, 160] // first row in console is taken by magic
        Instantiate(screen);

        Point<float> minValue = new Point<float>(2, -1.5f);
        Point<float> maxValue = new Point<float>(8, 1.5f);
        Point<int> indicatorDensity = new Point<int>(8, 3);

        ColorChar[,] axis = MakeAxis(screen, minValue, maxValue, indicatorDensity);
        ColorChar[,] graphBorder = MakeBorder(screen);
        ColorChar[,] funcBuffer = MakeFunctionCurve(axis, currentFunc.Item1, minValue, maxValue);


        while (true)
        {
            WriteToBuffer(screen, currentFunc.Item2, new Point<int>(0, 0));
            WriteToBuffer(screen, funcBuffer, new Point<int>(GRAPH_PADDING, GRAPH_PADDING));
            WriteToBuffer(screen, axis, new Point<int>(GRAPH_PADDING, GRAPH_PADDING));
            WriteToBuffer(screen, graphBorder, new Point<int>(0, 0));
            Display(screen);
            System.Console.Write(msg);

            string? input = System.Console.ReadLine();
            // change min,maxValue, change func, change color
            // minValue -2 2
            // func 0
            // func new x*x myPow
            // color -xAxis red -yAxis green -func blue -border green
        }
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

    static ColorChar[,] MakeAxis(ColorChar[,] screen, Point<float> minValue, Point<float> maxValue, Point<int> indicatorDensity)
    {
        Point<int> maxScreenPosPad = new Point<int>(screen.GetLength(1) - GRAPH_PADDING, screen.GetLength(0) - GRAPH_PADDING);
        ColorChar[,] axis = new ColorChar[maxScreenPosPad.y, maxScreenPosPad.x];
        Instantiate(axis);
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
            // axis
            Point<int> indicatorRowCol = new Point<int>(origin.x - 1, origin.y + 1);
            for (int y = GRAPH_PADDING - 1; y < maxScreenPosPad.y - GRAPH_PADDING; y++)
                axis[y, origin.x] = yAxisColorCh;
            for (int x = GRAPH_PADDING - 1; x < maxScreenPosPad.x - GRAPH_PADDING; x++)
                axis[origin.y, x] = xAxisColorCh;

            // indicators
            for (int x = 1; x < maxScreenPosPad.x; x++)
            {
                if (x % indicatorDensity.x != 0) continue;
                int xPos = (int)Remap(0, maxScreenPosPad.x, minPos.x, maxPos.x, x);
                float xVal = ScrPosToValue(maxValue.x, minValue.x, maxScreenPosPad.x, xPos);
                string xValstr = ToString(xVal);
                WriteToBuffer(axis, xValstr, new Point<int>(xPos - 2, indicatorRowCol.y));
                axis[indicatorRowCol.y - 1, xPos] = xIndicatorColorCh;
            }

            for (int y = 1; y < maxScreenPosPad.y; y++)
            {
                if (y % indicatorDensity.y != 0) continue;
                int yPos = (int)Remap(0, maxScreenPosPad.y, minPos.y, maxPos.y, y);
                float yVal = ScrPosToValue(maxValue.y, minValue.y, maxScreenPosPad.y, yPos);
                yPos = maxScreenPosPad.y - yPos;
                string yValStr = ToString(yVal);
                WriteToBuffer(axis, yValStr, new Point<int>(indicatorRowCol.x - 3, yPos));
                axis[yPos, indicatorRowCol.x + 1] = yIndicatorColorCh;
            }
            axis[origin.y, origin.x] = originColorCh;
        }

        msg = "origin: " + origin.x + ", " + origin.y + "; originval: " + originVal.x + ", " + originVal.y;

        return axis;
    }

    static ColorChar[,] MakeBorder(ColorChar[,] screen)
    {
        Point<int> maxScreenPos = new Point<int>(screen.GetLength(1) - 1, screen.GetLength(0));
        ColorChar[,] border = new ColorChar[maxScreenPos.y, maxScreenPos.x];
        Instantiate(border);
        for (int x = GRAPH_PADDING; x < maxScreenPos.x - GRAPH_PADDING; x++)
        {
            border[GRAPH_PADDING, x] = borderHoriColorCh;
            border[maxScreenPos.y - GRAPH_PADDING, x] = borderHoriColorCh;
        }
        for (int y = GRAPH_PADDING; y < maxScreenPos.y - GRAPH_PADDING; y++)
        {
            border[y, GRAPH_PADDING] = borderVertiColorCh;
            border[y, maxScreenPos.x - GRAPH_PADDING] = borderVertiColorCh;
        }
        border[GRAPH_PADDING, GRAPH_PADDING] = borderCornerColorCh;
        border[maxScreenPos.y - GRAPH_PADDING, GRAPH_PADDING] = borderCornerColorCh;
        border[GRAPH_PADDING, maxScreenPos.x - GRAPH_PADDING] = borderCornerColorCh;
        border[maxScreenPos.y - GRAPH_PADDING, maxScreenPos.x - GRAPH_PADDING] = borderCornerColorCh;
        return border;
    }

    static ColorChar[,] MakeFunctionCurve(ColorChar[,] axis, floatFuncFloat Func, Point<float> minValue, Point<float> maxValue)
    {
        Point<int> maxAxisPos = new Point<int>(axis.GetLength(1) - GRAPH_PADDING, axis.GetLength(0) - GRAPH_PADDING);
        ColorChar[,] funcBuffer = new ColorChar[maxAxisPos.y, maxAxisPos.x];
        Instantiate(funcBuffer);

        for (int x = 1; x < maxAxisPos.x - 1; x++)
        {
            float xVal = ScrPosToValue(maxValue.x, minValue.x, maxAxisPos.x, x);
            float yVal = Func(xVal);
            if (yVal >= maxValue.y || yVal <= minValue.y) continue;
            int yPosAbs = ValueToScrPos(maxValue.y, minValue.y, yVal, maxAxisPos.y);
            int yPos = maxAxisPos.y - yPosAbs;
            funcBuffer[yPos, x] = funcColorCh;
        }
        return funcBuffer;
    }

    static void WriteToBuffer(ColorChar[,] buffer, string text, Point<int> point, ConsoleColor consoleColor = ConsoleColor.White)
    {
        if (point.y > buffer.GetLength(0)) return;
        for (int i = 0; i < text.Length; i++)
        {
            if (point.x + i > buffer.GetLength(1)) return;
            buffer[point.y, point.x + i].ch = text[i];
            buffer[point.y, point.x + i].consoleColor = consoleColor;
        }
    }
    static void WriteToBuffer(ColorChar[,] buffer, ColorChar[,] value, Point<int> point)
    {
        for (int x = 0; x < value.GetLength(1); x++)
            for (int y = 0; y < value.GetLength(0); y++)
            {
                if ((value[y, x].ch == '\0')) continue;
                buffer[y + point.y, x + point.x] = value[y, x];
            }
    }

    static void Display(ColorChar[,] screen)
    {
        System.Console.Clear();
        for (int y = 0; y < screen.GetLength(0); y++)
        {
            for (int x = 0; x < screen.GetLength(1); x++)
            {
                System.Console.ForegroundColor = screen[y, x].consoleColor;
                System.Console.Write(screen[y, x].ch);
            }
        }
        System.Console.ResetColor();
    }

    static void Instantiate(ColorChar[,] buffer)
    {
        for (int x = 0; x < buffer.GetLength(1); x++)
            for (int y = 0; y < buffer.GetLength(0); y++)
                buffer[y, x] = new ColorChar();
    }

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