using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace ExcelToFBS {
    public abstract class Validator {

        public static Dictionary<string, List<int>> IDValues = new Dictionary<string, List<int>>();
        public static Dictionary<string, List<float>> RangeValues = new Dictionary<string, List<float>>();

        private static List<Validator> _validators = new List<Validator>();

        public static bool Success = true;

        public static void Error(string msg) {
            Success = false;
            Console.WriteLine(msg);
        }

        public static void GenerateDatas(List<ExcelData> datas) {
            for(int d = 0; d < datas.Count; ++d) {
                ExcelData data = datas[d];
                //Console.WriteLine(string.Format("Validate过程中 表{0}中 {1}页", data.excelName, data.sheetName));

                foreach(var info in data.fieldInfos.Values) {
                    if(string.IsNullOrEmpty(info.constraint) && info.index != 0) {
                        continue;
                    }

                    for(int n = 1; n <= data.sheet.LastRowNum; ++n) {
                        IRow dataRow = data.sheet.GetRow(n);
                        if(dataRow == null) {
                            Error(string.Format("Validate过程中 表{0}中 {1}页 找不到{2}行", data.excelName, data.sheetName, n));
                            continue;
                        }

                        string infoValue = "";
                        ICell cell = dataRow.GetCell(info.index);
                        if(cell == null) {
                            continue;
                        }
                        else {
                            if(cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell)) {
                                infoValue = cell.DateCellValue.ToString();
                            }
                            else {
                                infoValue = cell.ToString();
                            }
                        }

                        //if(string.IsNullOrEmpty(infoValue)) {
                        //    continue;
                        //}

                        Object valueObj = Utils.GetFieldValue(data, info, infoValue);

                        // IDS
                        if(info.index == 0) {
                            AddIDConstrain(data, info, valueObj);
                        }

                        if(info.constraint.Contains("ID") || info.constraint.Contains("Id")) {
                            AddIDConstrain(data, info, valueObj);
                        }
                        // Ranges
                        else if(info.constraint.Contains("Range")) {
                            AddRangeConstrain(data, info, valueObj);
                        }
                    }
                }
            }
        }

        private static string GetConstrainKey(ExcelData data, ExcelFieldInfo info) {
            return data.sheetName + info.name;
        }

        private static void AddIDConstrain(ExcelData data, ExcelFieldInfo info, Object valueObj) {
            int value = 0;
            if(valueObj is int) {
                value = (int)(valueObj);
            }
            else {
                Console.WriteLine(string.Format("Validate过程中 表{0}中 {1}页 ID约束数据{1}类型错误", data.excelName, data.sheetName, valueObj));
            }

            string key = GetConstrainKey(data, info);
            List<int> ids;
            if(IDValues.TryGetValue(key, out ids)) {
                ids.Add(value);
            }
            else {
                ids = new List<int>();
                ids.Add(value);
                IDValues.Add(key, ids);
            }
        }

        private static void AddRangeConstrain(ExcelData data, ExcelFieldInfo info, Object valueObj) {
            float value = 0;
            if(valueObj is int) {
                value = (int)(valueObj);
            }
            else if(valueObj is float) {
                value = (float)(valueObj);
            }
            else {
                Error(string.Format("Validate过程中 表{0}中 {1}页 范围约束数据{1}类型错误", data.excelName, data.sheetName, valueObj));
            }

            string key = GetConstrainKey(data, info);
            List<float> values;
            if(RangeValues.TryGetValue(key, out values)) {
                values.Add(value);
            }
            else {
                values = new List<float>();
                values.Add(value);
                RangeValues.Add(key, values);
            }
        }

        public static void GenerateValidators(List<ExcelData> datas) {
            for(int d = 0; d < datas.Count; ++d) {
                ExcelData data = datas[d];

                foreach(var info in data.fieldInfos.Values) {
                    if(string.IsNullOrEmpty(info.constraint)) {
                        continue;
                    }
                    _validators.Add(CreateValidator(data, info));
                }
            }
        }

        public static void MakeValidate() {
            try {
                for(int n = 0; n < _validators.Count; ++n) {
                    _validators[n].Validate();
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        private static Validator CreateValidator(ExcelData data, ExcelFieldInfo info) {
            string constrain = info.constraint;

            int index0 = constrain.IndexOf("(");
            int index1 = constrain.IndexOf(")");
            if(index0 == -1) {
                Error(string.Format("Create Validator 约束格式不正确 表{0} {1}中 约束信息:{2} {3}", data.excelName, data.sheetName, info.desc, info.constraint));
            }

            string typeName = constrain.Substring(0, index0);
            string param = constrain.Substring(index0 + 2, index1 - index0 - 3);     // 去掉""

            Validator validator = null;
            if(typeName == "ID") {
                List<int> ids;
                List<int> constrainIds;
                string key = GetConstrainKey(data, info);
                if(!IDValues.TryGetValue(key, out ids)) {
                    Error(string.Format("ID Validator 表{0}:{1}找不到被约束的ID，检查字段名是不是没有大写", data.excelName, data.sheetName));
                }

                key = param + "ID";
                if(!IDValues.TryGetValue(key, out constrainIds)) {
                    Error(string.Format("ID Validator 表{0}找不到约束的ID，检查字段名是不是没有大写", param));
                }

                validator = new IDValidator(data.excelName, data.sheetName, param, info.name, ids, constrainIds);
            }
            else if(typeName == "Range") {
                string key = GetConstrainKey(data, info);
                List<float> values;
                if(!RangeValues.TryGetValue(key, out values)) {
                    Error(string.Format("Range Validator 表{0}:{1}中找不到约束的字段{2}", data.excelName, data.sheetName, info.name));
                }

                validator = new RangeValidator(data.excelName, data.sheetName, info.name, param, values);
            }

            return validator;
        }

        public abstract bool Validate();
    }

    public class RangeValidator : Validator {
        private string _excelName;
        private string _sheetName;
        private string _name;
        private float _min;
        private float _max;
        private List<float> _values;

        public RangeValidator(string excelName, string sheetName, string name, string param, List<float> values) {
            _excelName = excelName;
            _sheetName = sheetName;
            _name = name;

            string[] mm = param.Split(',');
            if(mm == null) {
                Error(string.Format("Range Validator 参数为空"));
            }
            else if(mm.Length != 2) {
                Error(string.Format("Range Validator 参数格式化错误 {0}", param));
            }

            _min = float.Parse(mm[0]);
            _max = float.Parse(mm[1]);

            _values = values;
        }

        public override bool Validate() {
            for(int n = 0; n < _values.Count; ++n) {
                float value = _values[n];
                if(value < _min || value > _max) {
                    Error(string.Format("Range Validator {0}表{1}中的{2}字段的值{3}超出约定范围{4}~{5}", _excelName, _sheetName, _name, value, _min, _max));
                }
            }

            return true;
        }
    }

    public class IDValidator : Validator {
        private string _excelName;
        private string _sheetName;
        private string _name;
        private string _constrainedSheetName;

        private List<int> _ids;
        private List<int> _constrainIds;

        public IDValidator(string excelName, string sheetName, string constrainedSheetName, string name, List<int> ids, List<int> constrainIds) {
            _excelName = excelName;
            _sheetName = sheetName;
            _constrainedSheetName = constrainedSheetName;
            _name = name;
            _ids = ids;
            _constrainIds = constrainIds;
        }

        public override bool Validate() {
            for(int n = 0; n < _ids.Count; ++n) {
                int id = _ids[n];

                // 默认允许为空
                if(id == 0) {
                    continue;
                }

                if(!_constrainIds.Contains(id)) {
                    Error(string.Format("{0}表中不存在表{1}的{2}字段中需要的ID:{3}", _constrainedSheetName, _sheetName, _name, id));
                }
            }

            return true;
        }
    }

}
