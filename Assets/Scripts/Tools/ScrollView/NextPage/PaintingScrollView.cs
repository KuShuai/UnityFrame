using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.IO;

public class PaintingScrollView : ScrollViewBase
{
    protected override void OnAwake()
    {
        base.OnAwake();
        if (Content != null)
        {
            InitItem();

        }
    }

    protected override void OnStart()
    {
        base.OnStart();
    }

    public void SetData(string name)
    {
        foreach (var item in AllData)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                item.Value[i].fileName = name;
            }
        }
        SetData(100);
    }

    private void SetData(int tabPage, string fileName, bool IsRecrived = false, int ShowAchId = 0)
    {
        string filePath = Application.streamingAssetsPath + "/" + fileName;
        DirectoryInfo info = new DirectoryInfo(filePath);
        Data.Clear();
        for (int i = 0; i < info.GetFiles("*.png",SearchOption.AllDirectories).Length; i++)
        {
            Data.Add(i);
        }
        int page = 0;
        Debug.Log("Data count is "+Data.Count+"tabpage is "+tabPage);
        itemCount = Data.Count;

        if (AllPoint == null)
        {
            AllPoint = new List<Image>();
        }
        if (AllPoint.Count < itemCount && PointImage != null)
        {
            PointImage.gameObject.SetActive(true);
            for (int i = AllPoint.Count; i < itemCount; i++)
            {
                GameObject PointImg = GameObject.Instantiate(PointImage.gameObject, PointImage.transform.parent);
                if (PointImg != null)
                {
                    PointImg.GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 1);
                    AllPoint.Add(PointImg.GetComponent<Image>());
                }
            }
            PointImage.gameObject.SetActive(false);
        }
        else if (AllPoint.Count > itemCount)
        {
            while (AllPoint.Count > itemCount)
            {
                Image img = AllPoint[0];
                AllPoint.RemoveAt(0);
                Destroy(img.gameObject);
            }
        }

        for (int i = 0; i < AllPoint.Count; i++)
        {
            AllPoint[i].color = normalecolor;
        }

        SetChooseAchId(ShowAchId);
        SetToPhoto(page);
        SetChooseAchId(0);
    }
    
    
    private void SetData(int count, int ShowAchId = 0)
    {
        
        Data.Clear();
        for (int i = 0; i < count; i++)
        {
            Data.Add(i);
        }
        int page = 0;
        Debug.Log("Data count is "+Data.Count+"tabpage is "+count);
        itemCount = Data.Count;

        if (AllPoint == null)
        {
            AllPoint = new List<Image>();
        }
        if (AllPoint.Count < itemCount && PointImage != null)
        {
            PointImage.gameObject.SetActive(true);
            for (int i = AllPoint.Count; i < itemCount; i++)
            {
                GameObject PointImg = GameObject.Instantiate(PointImage.gameObject, PointImage.transform.parent);
                if (PointImg != null)
                {
                    PointImg.GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 1);
                    AllPoint.Add(PointImg.GetComponent<Image>());
                }
            }
            PointImage.gameObject.SetActive(false);
        }
        else if (AllPoint.Count > itemCount)
        {
            while (AllPoint.Count > itemCount)
            {
                Image img = AllPoint[0];
                AllPoint.RemoveAt(0);
                Destroy(img.gameObject);
            }
        }

        for (int i = 0; i < AllPoint.Count; i++)
        {
            AllPoint[i].color = normalecolor;
        }

        SetChooseAchId(ShowAchId);
        SetToPhoto(page);
        SetChooseAchId(0);
    }


}
