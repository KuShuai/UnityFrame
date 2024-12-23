using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLinePanel : UIPanel
{
    private PaintingScrollView R_TimeLine_PaintingScrollView = null;

    public override void OnPreLoad()
    {
        base.OnPreLoad();
        R_TimeLine_PaintingScrollView = GetUIWidget<PaintingScrollView>("R_TimeLine_PaintingScrollView");
    }

    public override void OnLoad()
    {
        base.OnLoad();
        Init();
    }
    
    private  void Init()
    {
        R_TimeLine_PaintingScrollView.SetData("AAA");
    }
}
