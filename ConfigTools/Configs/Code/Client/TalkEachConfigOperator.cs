using System;
using System.IO;
using System.Collections.Generic;
using FlatBuffers;
using AutoGenConfig;

public class TalkEachConfigOperator {

    public class Data {
        public int ID;
        public string Name;
        public string Info;
        public int next;
    }

    public void Save(List<Data> datas, string path) {
        FlatBufferBuilder fbb = new FlatBufferBuilder(1);
        int count = datas.Count;
        Offset<SingleTalkEachConfigData>[] offsets = new Offset<SingleTalkEachConfigData>[count];
        for (int n = 0; n < count; ++n) {
            Data data = datas[n];
            offsets[n] = SingleTalkEachConfigData.CreateSingleTalkEachConfigData(fbb,
            data.ID,
            fbb.CreateString(data.Name),
            fbb.CreateString(data.Info),
            data.next);
        }
        VectorOffset dataOff = TalkEachConfig.CreateDataVector(fbb, offsets);
        var configOff = TalkEachConfig.CreateTalkEachConfig(fbb, dataOff);
        TalkEachConfig.FinishTalkEachConfigBuffer(fbb, configOff);
        using (var ms = new MemoryStream(fbb.DataBuffer.Data, fbb.DataBuffer.Position, fbb.Offset)) {
            File.WriteAllBytes(path, ms.ToArray());
        }
    }
}
