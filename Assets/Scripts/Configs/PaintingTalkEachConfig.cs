using System;
using System.Collections;
using System.Collections.Generic;
using AutoGenConfig;
using FlatBuffers;
using ResourcesLoad;
using UnityEngine;

public class TalkEachDefine
{
    public int Id;
    public string Name;
    public string Info;
    public int Next;
}
public class PaintingTalkEachConfig : Singleton<PaintingTalkEachConfig>,ISingleton
{
    private Dictionary<int, TalkEachDefine> _allTalkDef;
    
    public void SingletonInit()
    {
        string path = RSPathUtil.Config("TalkEachConfig");
        ResourceManager.Instance.LoadAndProcTextAsset(path,ProcFB);
    }

    private void ProcFB(byte[] bytes)
    {
        ByteBuffer bb = new ByteBuffer(bytes);

        if (!TalkEachConfig.TalkEachConfigBufferHasIdentifier(bb))
        {
            throw new Exception("Identifier text failed,TalkEachConfig");
        }

        TalkEachConfig config = TalkEachConfig.GetRootAsTalkEachConfig(bb);
        Debug.Log("Loaded TalkEachConfig data count"+config.DataLength);

        _allTalkDef = new Dictionary<int, TalkEachDefine>();
        for (int i = 0; i < config.DataLength; i++)
        {
            SingleTalkEachConfigData? data = config.Data(i);
            if (data != null)
            {
                TalkEachDefine def = new TalkEachDefine();
                def.Id = data.Value.ID;
                def.Name = data.Value.Name;
                def.Info = data.Value.Info;
                def.Next = data.Value.Next;

                _allTalkDef.Add(def.Id , def);
            }
        }
    }

    public TalkEachDefine Get(int id)
    {
        TalkEachDefine define;
        _allTalkDef.TryGetValue(id,out define);
        return define;
    }
    public void SingletonDestory()
    {
        _allTalkDef.Clear();
        _allTalkDef = null;
    }
}
