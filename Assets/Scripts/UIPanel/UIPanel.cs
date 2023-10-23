using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UIPanel : UIWidget
{
    public int ID;
    public UIConfig _uiConfig;
    
    private Canvas _canvas;

    public void SetCanvas(Canvas canvas)
    {
        _canvas = canvas;
    }

    public Canvas GetCanvas()
    {
        return _canvas;
    }

    /// <summary>
    /// 设置面板是否可见
    /// </summary>
    /// <param name="enabled">是否可见</param>
    public void SetCanvasStatus(bool enabled)
    {
        if (_canvas != null && _canvas.enabled != enabled)
        {
            _canvas.enabled = enabled;
        }
    }

    public void SetOrder(int order)
    {
        _canvas.sortingOrder = order;
    }
    
    

    public void SetUp(int id ,UIConfig uiConfig,object load_paramter)
    {
        ID = id;
        _uiConfig = uiConfig;

        loadParamter = load_paramter;

    }
}
