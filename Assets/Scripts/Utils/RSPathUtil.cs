using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RSPathUtil
{
    public static StringBuilder Builder = new StringBuilder(1024);

    public const string Ext = "";
    public static string UIPath = "Prefabs/UIPrefabs/";
    public static string ConfigsPath = "Configs/";
    public static string LuaScriptsBundlePath = "LuaScripts/";

    public static string Config(string name)
    {
        Builder.Clear();
        return Builder.Append(PathUtil.AssetPath(ConfigsPath)).Append(name).Append(Ext).ToString();
    }

    public static string UI(string name)
    {
        Builder.Clear();
        return Builder.Append(PathUtil.AssetPath(UIPath)).Append(name).Append(Ext).ToString();
    }
    public static string LuaScript(string name)
    {
        Builder.Clear();
        return Builder.Append(PathUtil.AssetPath(LuaScriptsBundlePath)).Append(name).Append(".lua").ToString();
    }

}
