using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkPanel : UIPanel
{
    private Image R_UpBg_Image = null;
    private TextEffects R_Up_TextEffects = null;
    private Image R_DownBg_Image = null;
    private TextEffects R_Down_TextEffects = null;

    public override void OnPreLoad()
    {
        base.OnPreLoad();
        R_UpBg_Image = GetUIWidget<Image>("R_UpBg_Image");
        R_Up_TextEffects = GetUIWidget<TextEffects>("R_Up_TextEffects");
        R_DownBg_Image = GetUIWidget<Image>("R_DownBg_Image");
        R_Down_TextEffects = GetUIWidget<TextEffects>("R_Down_TextEffects");
    }

    public override void OnLoad()
    {
        base.OnLoad();
        Init();
        
        //ShowUpInfo(PaintingTalkEachConfig.Instance.Get(1).Info);
    }

    private TalkEachDefine showDef;
    private string downName;
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (Input.anyKeyDown)
        {
            if (showDef == null)
            {
                showDef = PaintingTalkEachConfig.Instance.Get(1);
                downName = showDef.Name;
                ShowDownInfo(showDef.Name+":"+showDef.Info);
            }
            else
            {
                if (showDef.Next == -1)
                    return;
                showDef = PaintingTalkEachConfig.Instance.Get(showDef.Next);
                if (downName.Equals(showDef.Name))
                {
                    ShowDownInfo(showDef.Name+":"+showDef.Info);
                }
                else
                {
                    ShowUpInfo(showDef.Name+":"+showDef.Info);
                }
            }
        }
    }

    private void Init()
    {
        R_UpBg_Image.enabled = false;
        R_DownBg_Image.enabled = false;
        R_Up_TextEffects.SetText(String.Empty);
        R_Down_TextEffects.SetText(String.Empty);
    }

    private void ShowUpInfo(string info)
    {
        R_UpBg_Image.enabled = true;
        R_Up_TextEffects.SetText(info);
    }

    private void CloseUpInfo()
    {
        R_UpBg_Image.enabled = false;
        R_Up_TextEffects.SetText(String.Empty);
    }

    private void ShowDownInfo(string info)
    {
        R_DownBg_Image.enabled = true;
        R_Down_TextEffects.SetText(info);
    }

    private void CloseDownInfo()
    {
        R_DownBg_Image.enabled = false;
        R_Down_TextEffects.SetText(String.Empty);
    }
}
