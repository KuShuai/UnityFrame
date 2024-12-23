using System;
using System.IO;
using System.Collections.Generic;
using FlatBuffers;
using AutoGenConfig;

public class Sheet1ConfigOperator {

    public class Data {
        public int ID;
        public string data1;
        public int data2;
    }

    public void Save(List<Data> datas, string path) {
        FlatBufferBuilder fbb = new FlatBufferBuilder(1);
        int count = datas.Count;
        Offset<SingleSheet1ConfigData>[] offsets = new Offset<SingleSheet1ConfigData>[count];
        for (int n = 0; n < count; ++n) {
            Data data = datas[n];
            offsets[n] = SingleSheet1ConfigData.CreateSingleSheet1ConfigData(fbb,
            data.ID,
            fbb.CreateString(data.data1),
            data.data2);
        }
        VectorOffset dataOff = Sheet1Config.CreateDataVector(fbb, offsets);
        var configOff = Sheet1Config.CreateSheet1Config(fbb, dataOff);
        Sheet1Config.FinishSheet1ConfigBuffer(fbb, configOff);
        using (var ms = new MemoryStream(fbb.DataBuffer.Data, fbb.DataBuffer.Position, fbb.Offset)) {
            File.WriteAllBytes(path, ms.ToArray());
        }
    }
}
