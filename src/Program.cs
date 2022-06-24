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

    // private static string continuePrompt = "press any key to continue...";
    private static string prompt = "type 'help' to get started";
    private static string defaultPrompt = "Command: ";
    private static string nullInputPrompt = "null Input. ";
    private static string firstInvalidPrompt = "first input invalid";
    private static string secondInvalidPrompt = "second input invalid";
    private static string invalidNumOfInput = "invalid number of arguments";
    public static ConsoleColor promptColor = ConsoleColor.Green;

    public const int BORDER_PADDING = 2;
    public const int AXIS_PADDING = BORDER_PADDING + 2; // +2 => 1 space gap
    public const int NUM_PRECISION = 4;
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

    private static Point<float> minValue = new Point<float>(2, -1.5f);
    private static Point<float> maxValue = new Point<float>(8, 1.5f);
    private static Point<int> indicatorDensity = new Point<int>(8, 3);

    public static void Main(string[] args)
    {
        const int width = 150, height = 40;
        SetWindowSize(width, height);

        (floatFuncFloat, string)[] sampleFunctions = {
            (SampleFunctions.XPow2, "Xpow2"),
            (SampleFunctions.XPow3, "Xpow3"),
            (SampleFunctions.sin, "Sin")
        };
        (floatFuncFloat, string) currentFunc = sampleFunctions[2];

        ColorChar[,] screen = new ColorChar[height - 1, width]; // screen[y,x] == screen[39, 160] // first row in console is taken by magic
        Instantiate(screen);




        while (true)
        {
            ColorChar[,] axis = MakeAxis(screen);
            ColorChar[,] graphBorder = MakeBorder(screen);
            ColorChar[,] funcBuffer = MakeFunctionCurve(axis, currentFunc.Item1);

            WriteToBuffer(screen, currentFunc.Item2, new Point<int>(0, 0));
            WriteToBuffer(screen, funcBuffer, new Point<int>(AXIS_PADDING, AXIS_PADDING));
            WriteToBuffer(screen, axis, new Point<int>(AXIS_PADDING, AXIS_PADDING));
            WriteToBuffer(screen, graphBorder, new Point<int>(BORDER_PADDING, BORDER_PADDING));
            Display(screen);
            Instantiate(screen);

            PrintPrompt();
            prompt = defaultPrompt;
            string? input = System.Console.ReadLine();
            HandleCommand(input);

            // change min,maxValue, change func, change color
            // minValue -2 2
            // func 0
            // func new x*x myPow
            // color -xAxis red -yAxis green -func blue -border green
        }
    }

    public static void PrintPrompt()
    {
        System.Console.ForegroundColor = promptColor;
        System.Console.Write(prompt);
        System.Console.ResetColor();
        System.Console.Write(", " + defaultPrompt);
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

    static ColorChar[,] MakeAxis(ColorChar[,] screen)
    {
        Point<int> maxAxisPos = new Point<int>(screen.GetLength(1) - 2 * AXIS_PADDING, screen.GetLength(0) - 2 * AXIS_PADDING);
        ColorChar[,] axis = new ColorChar[maxAxisPos.y, maxAxisPos.x];
        Instantiate(axis);
        Point<int> origin;
        Point<float> originVal;
        {
            Point<int> originAbs;
            originAbs.x = ValueToScrPos(maxValue.x, minValue.x, 0, maxAxisPos.x);
            originAbs.y = ValueToScrPos(maxValue.y, minValue.y, 0, maxAxisPos.y);

            if (originAbs.x > maxAxisPos.x - NUM_PRECISION) // xAxis shuldnt go till edge
                originAbs.x = maxAxisPos.x - NUM_PRECISION;
            else if (originAbs.x < NUM_PRECISION)
                originAbs.x = NUM_PRECISION;
            if (originAbs.y > maxAxisPos.y - (NUM_PRECISION + 1)) // above + 1 for yAxis line
                originAbs.y = maxAxisPos.y - (NUM_PRECISION + 1);
            else if (originAbs.y < (NUM_PRECISION + 1))
                originAbs.y = (NUM_PRECISION + 1);

            originVal.x = ScrPosToValue(maxValue.x, minValue.x, maxAxisPos.x, originAbs.x);
            originVal.y = ScrPosToValue(maxValue.y, minValue.y, maxAxisPos.y, originAbs.y);

            origin = new Point<int>(originAbs.x, maxAxisPos.y - originAbs.y);
        }

        Point<int> minPos;
        minPos.x = ValueToScrPos(maxValue.x, minValue.x, minValue.x, maxAxisPos.x);
        minPos.y = ValueToScrPos(maxValue.y, minValue.y, minValue.y, maxAxisPos.y);
        Point<int> maxPos;
        maxPos.x = ValueToScrPos(maxValue.x, minValue.x, maxValue.x, maxAxisPos.x);
        maxPos.y = ValueToScrPos(maxValue.y, minValue.y, maxValue.y, maxAxisPos.y);

        {
            Point<int> indicatorRowCol = new Point<int>(origin.x, origin.y + 1);

            // axis
            for (int y = 0; y < maxAxisPos.y; y++)
                axis[y, origin.x] = yAxisColorCh;
            for (int x = 0; x < maxAxisPos.x; x++)
                axis[origin.y, x] = xAxisColorCh;
            axis[origin.y, origin.x] = originColorCh;

            // indicators
            for (int x = 0; x < maxAxisPos.x; x++)
            {
                if (x % indicatorDensity.x != 0) continue;
                int xPos = (int)Remap(0, maxAxisPos.x, minPos.x, maxPos.x, x);
                float xVal = ScrPosToValue(maxValue.x, minValue.x, maxAxisPos.x, xPos);
                string xValstr = ToString(xVal);
                WriteToBuffer(axis, xValstr, new Point<int>(xPos, indicatorRowCol.y));
                axis[indicatorRowCol.y - 1, xPos] = xIndicatorColorCh;
            }
            for (int y = 0; y < maxAxisPos.y; y++)
            {
                if (y % indicatorDensity.y != 0) continue;
                int yPos = (int)Remap(0, maxAxisPos.y, minPos.y, maxPos.y, y);
                float yVal = ScrPosToValue(maxValue.y, minValue.y, maxAxisPos.y, yPos);
                yPos = maxAxisPos.y - 1 - yPos;
                if (yPos == origin.y) continue;
                string yValStr = ToString(yVal);
                WriteToBuffer(axis, yValStr, new Point<int>(indicatorRowCol.x - NUM_PRECISION, yPos));
                axis[yPos, indicatorRowCol.x] = yIndicatorColorCh;
            }

        }
        return axis;
    }


    static ColorChar[,] MakeBorder(ColorChar[,] screen)
    {
        ColorChar[,] border = new ColorChar[screen.GetLength(0) - 2 * BORDER_PADDING, screen.GetLength(1) - 2 * BORDER_PADDING];
        Point<int> maxBorderPos = new Point<int>(border.GetLength(1), border.GetLength(0));
        Instantiate(border);

        for (int x = 0; x < maxBorderPos.x; x++)
        {
            border[0, x] = borderHoriColorCh;
            border[maxBorderPos.y - 1, x] = borderHoriColorCh;
        }

        for (int y = 0; y < maxBorderPos.y; y++)
        {
            border[y, 0] = borderVertiColorCh;
            border[y, maxBorderPos.x - 1] = borderVertiColorCh;
        }

        border[0, 0] = borderCornerColorCh;
        border[maxBorderPos.y - 1, maxBorderPos.x - 1] = borderCornerColorCh;
        border[0, maxBorderPos.x - 1] = borderCornerColorCh;
        border[maxBorderPos.y - 1, 0] = borderCornerColorCh;

        return border;
    }

    static ColorChar[,] MakeFunctionCurve(ColorChar[,] axis, floatFuncFloat Func)
    {
        Point<int> maxFuncPos = new Point<int>(axis.GetLength(1), axis.GetLength(0));
        ColorChar[,] funcBuffer = new ColorChar[maxFuncPos.y, maxFuncPos.x];
        Instantiate(funcBuffer);

        for (int x = 1; x < maxFuncPos.x - 1; x++)
        {
            float xVal = ScrPosToValue(maxValue.x, minValue.x, maxFuncPos.x, x);
            float yVal = Func(xVal);
            // if (yVal >= maxValue.y || yVal <= minValue.y) continue;
            int yPosAbs = ValueToScrPos(maxValue.y, minValue.y, yVal, maxFuncPos.y);
            int yPos = maxFuncPos.y - yPosAbs;
            if (yPos > maxFuncPos.y - 1 || yPos < 1) continue;
            funcBuffer[yPos, x] = funcColorCh;
        }
        return funcBuffer;
    }

    static void HandleCommand(string? input)
    {
        if (input == null)
        {
            prompt = nullInputPrompt;
            promptColor = ConsoleColor.DarkRed;
            return;
        }
        string[] args = input.Split(" ");

        switch (args[0])
        {
            case "change":
                Change(args);
                break;
            case "help":
                Help(args);
                break;
            case "exit":
                Exit();
                break;
            default:
                prompt = firstInvalidPrompt;
                promptColor = ConsoleColor.DarkRed;
                break;
        }
    }

    static void Help(string[] args)
    {
        if (args.Length < 2)
        {
            prompt = "list of commands: change, exit, help";
            promptColor = ConsoleColor.DarkGreen;
            return;
        }
        if (args[1] == "change")
        {
            prompt = "change [scalex/scaley] num1 num2";
            promptColor = ConsoleColor.DarkGreen;
            return;
        }
        if (args[1] == "exit")
        {
            prompt = "exits the application with success";
            promptColor = ConsoleColor.DarkGreen;
            return;
        }
        if (args[1] == "help")
        {
            prompt = "???";
            promptColor = ConsoleColor.DarkGreen;
            return;
        }
    }

    static void Change(string[] args)
    {
        if (args.Length < 4)
        {
            prompt = invalidNumOfInput;
            promptColor = ConsoleColor.DarkRed;
            return;
        }
        bool success = false;
        int[] nums = new int[2];
        for (int i = 0; i < nums.Length; i++)
        {
            success = System.Int32.TryParse(args[2 + i], out nums[i]);
            if (!success)
            {
                prompt = "failed parse: " + args[2 + i];
                promptColor = ConsoleColor.DarkRed;
                return;
            }
        }

        if (args[1] == "scalex")
        {
            minValue = new Point<float>(nums[0], minValue.y);
            maxValue = new Point<float>(nums[1], maxValue.y);
            prompt = "Changed scale for X";
            promptColor = ConsoleColor.DarkGreen;
        }
        else if (args[1] == "scaley")
        {
            minValue = new Point<float>(minValue.x, nums[0]);
            maxValue = new Point<float>(maxValue.x, nums[1]);
            prompt = "Changed scale for Y";
            promptColor = ConsoleColor.DarkGreen;
        }
        else
        {
            prompt = secondInvalidPrompt;
            promptColor = ConsoleColor.DarkRed;
        }
    }

    public static void Exit()
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.Clear();
        System.Console.Write("Thank You for using the application!\n");
        System.Console.Write("press any key to exit");
        System.Console.Read();
        System.Environment.Exit(0);
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
        if (output.Length > NUM_PRECISION)
            index = NUM_PRECISION;
        else
            index = output.Length;

        output = output.Substring(0, index);
        return output.PadRight(NUM_PRECISION, ' ');
    } //  most significant digit


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