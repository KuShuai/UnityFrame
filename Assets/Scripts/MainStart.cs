using System;
using System.Collections;
using System.Collections.Generic;
using ResourcesLoad;
using UnityEngine;
using XLua;

public class MainStart : MonoBehaviour
{
    private void Awake()
    {
        UIManager.Instance.Init();
        //ResourceManager.Instance.Init();
        LuaScriptManager.Instance.Init();
        //UIManager.OpenUI("", null);
    }
}
