using System;
using System.Data;
using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace ExcelToFBS {
    public class ExcelFieldInfo {
        public EPeerType peer;
        public int index;
        public string name;
        public string type;
        public string desc;
        public string constraint;
    }

    public class ExcelData {
        public string excelName;
        public string sheetName;
#if USE_NPOI
        public ISheet sheet;
#else
        public DataTable table;
#endif
        public Dictionary<string, ExcelFieldInfo> fieldInfos = new Dictionary<string, ExcelFieldInfo>();
        public Dictionary<string, ExcelFieldInfo> clientFieldInfos = new Dictionary<string, ExcelFieldInfo>();
        public Dictionary<string, ExcelFieldInfo> serverFieldInfos = new Dictionary<string, ExcelFieldInfo>();
    }
}