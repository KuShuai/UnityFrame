using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class ScrollViewBase : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool loop;
    public bool horizontal = true;
    public bool vertical = false;

    public Color32 selectcolor = new Color32(143, 208, 180, 255);
    public Color32 normalecolor = new Color32(178, 178, 178, 255);

    public Button NextBtn;
    public Button UpBtn;

    public int itemCount = 0;
    public int pageCount = 0;
    public int XCount = 0;
    public Vector3 basePos;

    private Vector2 photoPos = Vector2.zero;
    private int currIndex = 0;

    public RectTransform Content;
    public GameObject item;

    public float PageSize = 0.0f;

    public List<RectTransform> AllPage;

    protected Dictionary<int, List<ScrollViewItem>> AllData;

    private bool isDrag = false;
    //偏移量
    private Vector2 offset = Vector2.zero;
    private Vector2 startMousePosition;
    private Vector2 startContentPosition;
    private float startTime;
    private float endTime;
    private int beginIndex = -1;


    public List<int> Data;

    public Image PointImage;
    public List<Image> AllPoint;

    //用于辨识当前是第几页（操作一些当显示在主页面时的操作Event）通过selectcolor点的颜色辨识。
    public int NowPageNo;

    private int chooseAchId;
    private bool hadChoose;

    public void SetChooseAchId(int id)
    {
        chooseAchId = id;
        if (id != 0)
        {
            hadChoose = true;
        }
    }

    private void Awake()
    {
        OnAwake();
    }


    private void Start()
    {
        if (NextBtn != null)
        {
            NextBtn.onClick.AddListener(NextBtnListener);
        }
        if (UpBtn != null)
        {
            UpBtn.onClick.AddListener(UpBtnListener);
        }
        OnStart();

    }

    public void InitItem()
    {
        //if (horizontal)
        //{
        PageSize = Content.sizeDelta.x;
        
        //}
        if (vertical)
        {
            PageSize = Content.sizeDelta.y;
            
        }
        AllPage = new List<RectTransform>();
        AllData = new Dictionary<int, List<ScrollViewItem>>();
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = new GameObject("Page" + i);
            obj.transform.SetParent(Content);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition3D = new Vector3(0, -PageSize * i, 0);
            rect.anchorMin = new Vector2(0, 1f);
            rect.anchorMax = new Vector2(0, 1f);
            rect.pivot = new Vector2(0, 1f);
            rect.sizeDelta = new Vector2(Content.sizeDelta.x, PageSize);
            for (int j = 0; j < pageCount; j++)
            {
                GameObject Obj = GameObject.Instantiate(item, rect);
                Obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(j % XCount * basePos.x, j / XCount * basePos.y, j * basePos.z);
                ScrollViewItem obj_widget = Obj.GetComponent<ScrollViewItem>();
                if (obj_widget != null)
                {
                    if (!AllData.ContainsKey(i))
                    {
                        AllData.Add(i, new List<ScrollViewItem>());
                    }
                    AllData[i].Add(obj_widget);
                }
            }
            AllPage.Add(rect);
        }
        item.gameObject.SetActive(false);
    }

    protected virtual void OnAwake()
    {

    }

    protected virtual void OnStart()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
        startMousePosition = Input.mousePosition;
        startContentPosition = Content.anchoredPosition;
        startTime = Time.time;
        beginIndex = CalculateIndex();
    }

    public void OnDrag(PointerEventData eventData)
    {
        offset = (Vector2)Input.mousePosition - startMousePosition;
        if (horizontal)
        {
            offset.y = 0;
            UpdateContentPosition();
        }
        if (vertical)
        {
            offset.x = 0;
            UpdateContentPosition();
        }
    }

    void UpdateContentPosition()
    {
        Vector2 newPos = startContentPosition + offset;
        //不是前后循环的时候取消边界的
        if (!loop)
        {
            if (horizontal)
            {
                newPos.x = Mathf.Clamp(newPos.x, -PageSize * (itemCount - 1), 0);
            }
            if (vertical)
            {
                newPos.y = Mathf.Clamp(newPos.y, 0, PageSize * (itemCount - 1));
            }
        }
        Content.anchoredPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        endTime = Time.time;
        float duration = endTime - startTime;

        int index = CalculateIndex();
        int nextIndex = -1;

        if (duration < 0.3f)
        {
            offset = (Vector2)Input.mousePosition - startMousePosition;
            if ((horizontal ? -offset.x : offset.y) > 0)
            {
                nextIndex = beginIndex + 1;
            }
            else
            {
                nextIndex = beginIndex - 1;
            }
            if (!loop)
            {
                nextIndex = Mathf.Clamp(nextIndex, 0, itemCount - 1);
            }
            IndexToPhoto(nextIndex);
        }
        else
        {
            IndexToPhoto(index);
        }
    }

    private int CalculateIndex()
    {
        // 计算当前属于第几个
        float posX = Content.anchoredPosition.y;
        if (horizontal)
        {
            posX = -Content.anchoredPosition.x;
        }
        int index = (int)(Mathf.Abs((PageSize / 2) + posX) / PageSize);
        if (loop)
        {
            index = (int)(((PageSize / 2) + posX + (posX < 0 ? -PageSize : 0)) / PageSize);
        }
        return index;
    }

    public void SetToPhoto(int _index)
    {
        photoPos = new Vector2(-_index * PageSize, 0);
        if (vertical)
        {
            photoPos = new Vector2(0, -_index * PageSize);
        }
        Content.anchoredPosition = photoPos;
        if (AllPoint != null && AllPoint.Count > 0)
        {

            if (currIndex >= 0 && currIndex < AllPoint.Count)
            {
                AllPoint[currIndex].color = normalecolor;
            }
            if (_index >= 0 && _index < AllPoint.Count)
            {
                AllPoint[_index].color = selectcolor;

                NowPageNo = _index;
                //EventManager.Instance.SendEvent(EventID.EID_AchieveShowAni, new IntEventParam()
                //{
                //    value = NowPageNo
                //}); 
            }
        }
        currIndex = _index;

        SetItem(_index - 1);
        SetItem(_index);
        SetItem(_index + 1);
        if (NextBtn != null)
        {
            NextBtn.interactable = currIndex < itemCount - 1 || loop;
        }
        if (UpBtn != null)
        {
            UpBtn.interactable = currIndex > 0 || loop;
        }
    }

    private void IndexToPhoto(int _index)
    {
        if (hadChoose)
        {
            if (currIndex - _index > 0)
            {
                SetItem(_index + 1);
            }
            else if (currIndex - _index < 0)
            {
                SetItem(_index - 1);
            }
            hadChoose = false;
        }
        photoPos = new Vector2(0, _index * PageSize);
        if (horizontal)
        {
            photoPos = new Vector2(-_index * PageSize, 0);
        }
        Content.DOAnchorPos(photoPos, 0.3f, true);

        int __index = _index;
        while (__index < 0)
        {
            __index += itemCount;
        }
        while (__index > currIndex)
        {
            __index -= itemCount;
        }
        if (currIndex > __index)
        {
            if (currIndex == __index + 1)
            {
                SetItem(_index - 1);
            }
            else
            {
                SetItem(_index + 1);
            }
        }
        else if (currIndex < __index)
        {
            if (currIndex == __index - 1)
            {
                SetItem(_index + 1);
            }
            else
            {
                SetItem(_index - 1);
            }

        }
        if (AllPoint != null && AllPoint.Count > 0)
        {

            if (currIndex >= 0 && currIndex < AllPoint.Count)
            {
                AllPoint[currIndex].color = normalecolor;
            }
            while (_index < 0)
            {
                _index += AllPoint.Count;
            }
            while (_index > AllPoint.Count - 1)
            {
                _index -= AllPoint.Count;
            }
            if (_index >= 0 && _index < AllPoint.Count)
            {
                AllPoint[_index].color = selectcolor;
                if (NowPageNo != _index)
                {
                    NowPageNo = _index;
                    //EventManager.Instance.SendEvent(EventID.EID_AchieveShowAni, new IntEventParam()
                    //{
                    //    value = NowPageNo
                    //});
                }
            }
        }
        currIndex = _index;

        if (NextBtn != null)
        {
            NextBtn.interactable = currIndex < itemCount - 1 || loop;
        }
        if (UpBtn != null)
        {
            UpBtn.interactable = currIndex > 0 || loop;
        }
    }

    private void NextBtnListener()
    {
        if (currIndex < itemCount - 1 || loop)
        {
            beginIndex = CalculateIndex();
            IndexToPhoto(beginIndex + 1);
        }
    }

    private void UpBtnListener()
    {
        if (0 < currIndex || loop)
        {
            beginIndex = CalculateIndex();
            IndexToPhoto(beginIndex - 1);
        }
    }

    private int GetPracticalIndex(int index)
    {
        while (index < 0)
        {
            index += itemCount;
        }
        while (index > itemCount - 1)
        {
            index -= itemCount;
        }
        return index;
    }

    private int GetShowPageForIndex(int index)
    {
        int result = index;
        while (index < 0)
        {
            index += 3;
        }
        return index;
    }

    /// <summary>
    /// 设置某一页内的所有数据
    /// </summary>
    /// <param name="index">页数索引</param>
    private void SetItem(int index)
    {
        //AllPage[(index+3) % AllPage.Count].anchoredPosition = new Vector2(-PageSize, PageSize);
        //if (index < 0 || index*pageCount >= Data.Count)
        //{
        //    return;
        //}
        if (!horizontal && !vertical)
        {
            return;
        }
        int _index = index;
        index = GetPracticalIndex(index);
        if (horizontal)
        {
            AllPage[GetShowPageForIndex(_index) % 3].anchoredPosition = new Vector2((horizontal ? 1 : -1) * PageSize * _index, 0);
        }
        if (vertical)
        {
            AllPage[GetShowPageForIndex(_index) % 3].anchoredPosition = new Vector2(0, (horizontal ? 1 : -1) * PageSize * _index);
        }

        for (int i = 0; i < pageCount; i++)
        {
            if (index * pageCount + i < Data.Count)
            {
                AllData[GetShowPageForIndex(_index) % 3][i].Data(Data[index * pageCount + i], index, chooseAchId);
            }
            else
            {
                AllData[GetShowPageForIndex(_index) % 3][i].Data(-1, -1, chooseAchId);
            }
        }

    }

    public void ResetAllData()
    {
        if (AllPage != null)
        {
            for (int i = 0; i < AllPage.Count; i++)
            {
                AllPage[i].anchoredPosition = new Vector2(0, PageSize);
            }
        }
        if (AllData != null)
        {
            for (int i = 0; i < AllData.Count; i++)
            {
                for (int j = 0; j < AllData[i].Count; j++)
                {
                    AllData[i][j].Data(-1);
                }
            }
        }
    }
}