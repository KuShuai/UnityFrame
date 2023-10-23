using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class UIWidget : MonoBehaviour
{
    public string LuaFile = string.Empty;
    
    [System.Serializable]
    public class LinkItem
    {
        public string Name;
        public GameObject UIWidget;
    }
    
    /// <summary>
    /// 所有以R_开头的GameObject
    /// </summary>
    [HideInInspector]
    public List<LinkItem> Links = new List<LinkItem>();

    /// <summary>
    /// 所有以R_开头的GameObject字典
    /// </summary>
    private Dictionary<int, LinkItem> _linkItemMap = null;

    private LuaTable scriptEnv;
    
    //所有lua方法
    private Action _Awake = null;
    private Action _Enable = null;
    public LuaFunction _Start = null;
    private Action _Update = null;
    private Action _LateUpdate = null;
    private Action _Disable = null;
    private Action _Destroy = null;

    //panel的虚拟方法
    public virtual void OnPreLoad() { }
    public virtual void OnLoad() { }
    public virtual void OnShow(object obj) { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnDisShow() { }
    public virtual void OnClose() { }

    private bool _inited = false;
    
    public object loadParamter = null;
    
    private void Awake()
    {
        InitWidgets();
    }

    private void InitWidgets()
    {
        var widgets = GetComponentsInChildren<UIWidget>();
        for (int i = 0; i < widgets.Length; i++)
        {
            if (widgets[i].transform == transform)
                continue;
            widgets[i].InitWidgets();
        }

        if (_inited)
            return;
        _inited = true;
        
        //遍历Links——初始化_linkItemMap
        _linkItemMap = new Dictionary<int, LinkItem>();
        for (int i = 0; i < Links.Count; i++)
        {
            int key = Links[i].GetHashCode();
            if (!_linkItemMap.ContainsKey(key))
            {
                _linkItemMap.Add(key,Links[i]);
            }
        }

        LoadLuaFile(LuaFile);
        
        OnPreLoad();
        _Awake?.Invoke();   
    }

    private void LoadLuaFile(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            scriptEnv = LuaScriptManager.Instance.LoadScript(fileName);
            if (scriptEnv != null)
            {
                scriptEnv.Set("self", this);
                
                scriptEnv.Get("Awake",out _Awake);
                scriptEnv.Get("Enable",out _Enable);
                scriptEnv.Get("Start",out _Start);
                scriptEnv.Get("Update",out _Update);
                scriptEnv.Get("LateUpdate",out _LateUpdate);
                scriptEnv.Get("Disable",out _Disable);
                scriptEnv.Get("Destroy",out _Destroy);
            }
        }
    }

    private void Start()
    {
        OnLoad();
        _Start?.Action(loadParamter);
    }

    private void OnEnable()
    {
        //OnShow();
        _Enable?.Invoke();
    }

    private void Update()
    {
        OnUpdate();
        _Update?.Invoke();
    }

    private void LateUpdate()
    {
        OnLateUpdate();
        _LateUpdate?.Invoke();
    }

    public void Close()
    {
        OnDisShow();
        _Disable?.Invoke();
    }

    private void OnDestroy()
    {
        OnClose();
        _Destroy?.Invoke();
    }

}