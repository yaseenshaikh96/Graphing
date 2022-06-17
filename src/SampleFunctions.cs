public static class SampleFunctions
{
    public delegate float floatFuncFloatOutString(float x, out string functionName);

    public static float Xpow2(float x, out string methodName)
    {
        string? name = System.Reflection.MethodBase.GetCurrentMethod()?.Name;
        if (name == null)
            methodName = "error";
        else
            methodName = name;
        return x * x;
    }


}