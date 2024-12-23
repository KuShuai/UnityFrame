using System;
using System.Reflection;

namespace ExcelToFBS {
    enum EConvertType {
        UNKOWN = -1,
        CLIENT,
        SERVER,
        VALIDATE,
        MAX
    }

    public class Utils {

        /// <summary>
        /// 动态创建泛型类型
        /// </summary>
        /// <param name="generic"></param>
        /// <param name="innerType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object CreateGeneric(Type generic, Type innerType, params object[] args) {
            Type specificType = generic.MakeGenericType(new System.Type[] { innerType });
            return Activator.CreateInstance(specificType, args);
        }

        /// <summary>
        /// 动态调用方法
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        public static void CallMethod(object obj, string method, object[] parameters) {
            MethodInfo methodInfo = obj.GetType().GetMethod(method);
            methodInfo.Invoke(obj, parameters);
        }

        public static object GetFieldValue(ExcelData data, ExcelFieldInfo info, string value) {
            object ret = null;
            try {
                switch(info.type) {
                    case "float":
                        if(string.IsNullOrEmpty(value)) {
                            ret = 0f;
                        }
                        else {
                            ret = float.Parse(value);
                        }
                        break;
                    case "int32":
                        if(string.IsNullOrEmpty(value)) {
                            ret = 0;
                        }
                        else {
                            ret = int.Parse(value);
                        }
                        break;
                    case "bool":
                        if(string.IsNullOrEmpty(value)) {
                            ret = false;
                        }
                        else {
                            ret = (value.ToLower() == "true" || value == "1");
                        }
                        break;
                    case "string":
                        if(string.IsNullOrEmpty(value)) {
                            ret = "";
                        }
                        else {
                            ret = value;
                        }
                        break;
                    //default:            // 枚举,转换成int类型
                    //    Type t = _commonAssmbly.GetType("msg." + info.constraint);
                    //    ret = Convert.ToInt32(Enum.Parse(t, value));
                    //    break;
                }
            }
            catch(Exception e) {
                Console.WriteLine(data.excelName + "中的" + data.sheetName + "中的：" + info.name + " " + info.type + " " + value + " " + e.Message);
            }

            return ret;
        }
    }
}
