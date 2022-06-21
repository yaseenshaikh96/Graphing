public struct ColorChar
{
    public char ch;
    public System.ConsoleColor consoleColor;

    public ColorChar(char ch = default(char), System.ConsoleColor consoleColor = ConsoleColor.White)
    {
        this.ch = ch;
        this.consoleColor = consoleColor;
    }
}