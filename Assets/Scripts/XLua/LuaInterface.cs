using System.Collections.Generic;
using UnityEngine;
using XLua;


[LuaCallCSharp]
public class LuaInterface
{
    public static void DebugLog(string param,params object[] others)
    {
        Debug.LogFormat(param, others);
    }

    public static string[] Split(string param, char ch)
    {
        string[] rt = param.Split(ch);
        return rt;
    }
}