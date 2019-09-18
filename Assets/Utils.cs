using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

public abstract class Utils
{
    public static string CharNativeArrayToString(NativeArray<char> na)
    {
        string ret = "";
        foreach (char c in na)
        {
            ret += c;
        }
        return ret;
    }

    public static string ExtensionFromFileName(string fileName)
    {
        int dotIndex = fileName.LastIndexOf('.');
        if (dotIndex != -1 && dotIndex != 0)
            return fileName.Substring(dotIndex + 1);
        return "";
    }
}
