using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.CodeDom.Compiler;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace ExcelToFBS
{

    public enum EGenType
    {
        UNKOWN = -1,
        CLIENT,
        SERVER,
        BOTH,
        MAX
    }

    public enum EPeerType
    {
        UNKOWN = -1,
        CLIENT,
        SERVER,
        BOTH,
        MAX
    }

    public class Convertor
    {
        public static string NewLine = Environment.NewLine;
        public static string Tab = "    ";
        public static string ConfigSaveFileEx = "Operator";

        private static Regex _regex = new Regex(@"\A(?<desc>.+?)(?<flag>\|{1,3})(?<name>.+?)\((?<type>.+?)(:(?<constraint>.*?))?\)$");

        private List<ExcelData> _excels;

        private EGenType _genType;

        /// <summary>
        /// flat生成的C#对应的编译结果
        /// </summary>
        private Assembly _clientAssmbly;
        private Assembly _serverAssmbly;
        //private Assembly _commonAssmbly;

        private Dictionary<string, string> _types = new Dictionary<string, string>();

        public Convertor(EGenType genType, int procNum, List<ExcelData> excels)
        {
            _genType = genType;

            _excels = excels;

            _types.Add("float", "float");
            _types.Add("int32", "int");
            _types.Add("bool", "bool");
            _types.Add("string", "string");
        }

        // 删除文件夹及其内容
        private void DeleteFolder(string dir)
        {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }

        private void ReCreateFolder(string dir)
        {
            DeleteFolder(dir);
            Directory.CreateDirectory(dir);
        }

        public void Run()
        {
            GenField();

            if(!MakeValidate()) {
                return;
            }

            switch(_genType) {
                case EGenType.BOTH:
                    GenFBS();
                    GenCode();
                    CompileCSharp();
                    GenDataFiles();
                    CopyFiles();
                    break;
                case EGenType.CLIENT:
                    GenFBS(EPeerType.CLIENT);
                    GenCode(EPeerType.CLIENT);
                    CompileCSharp(EPeerType.CLIENT);
                    GenDataFiles(EPeerType.CLIENT);
                    CopyFiles(EPeerType.CLIENT);
                    break;
                case EGenType.SERVER:
                    GenFBS(EPeerType.SERVER);
                    GenCode(EPeerType.SERVER);
                    CompileCSharp(EPeerType.SERVER);
                    GenDataFiles(EPeerType.SERVER);
                    CopyFiles(EPeerType.SERVER);
                    break;
            }
        }

        /// <summary>
        /// 获取表头信息
        /// </summary>
        private void GenField()
        {
            Console.WriteLine("\n----------生成配置表头信息----------");
            foreach (ExcelData data in _excels)
            {
                GenFieldInfos(data);
            }
        }

        private bool MakeValidate() {
            Console.WriteLine("\n----------检查数据----------");
            try {
                Validator.GenerateDatas(_excels);
                Validator.GenerateValidators(_excels);
                Validator.MakeValidate();

                return Validator.Success;
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        /// <summary>
        /// 生成FBS文件
        /// </summary>
        private void GenFBS()
        {
            GenFBS(EPeerType.CLIENT);
            GenFBS(EPeerType.SERVER);
        }

        private void GenFBS(EPeerType peer)
        {
            Console.WriteLine(string.Format("\n----------生成{0}FBS文件----------", peer));
            string FBSPath = GetFBSPath(peer);
            ReCreateFolder(FBSPath);
            foreach (ExcelData data in _excels)
            {
                var infos = GetInfos(data, peer);
                if (infos.Count == 0)
                {
                    continue;
                }

                string fileName = FBSPath + "/" + data.sheetName + ".fbs";
                EnsureDirectory(FBSPath);
                StringBuilder sb = new StringBuilder();
                string dataClass = "Single" + data.sheetName + "Data";
                sb.Append("namespace " + AppConfigs.Instance.FbsNameSpace + ";" + NewLine);
                sb.Append(NewLine);
                sb.Append("table " + data.sheetName + "{" + NewLine);
                sb.Append(Tab + "data:[" + dataClass + "];" + NewLine);
                sb.Append("}" + NewLine);
                sb.Append(NewLine);
                sb.Append("table " + dataClass + "{" + NewLine);
                foreach (ExcelFieldInfo info in infos.Values)
                {
                    sb.Append(Tab + info.name + ":" + FieldToType(info.type) + ";" + NewLine);
                }
                sb.Append("}" + NewLine);
                sb.Append(NewLine);
                sb.Append("root_type " + data.sheetName + ";" + NewLine);
                sb.Append("file_identifier \"WHAT\";");

                FileStream fs = new FileStream(fileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// 用flatbuffer生成对应的代码
        /// </summary>
        private void GenCode()
        {
            GenCode(EPeerType.CLIENT);
            GenCode(EPeerType.SERVER);
        }

        private void GenCode(EPeerType peer)
        {
            Console.WriteLine(string.Format("\n----------生成{0}代码----------", peer));
            string FBSPath = GetFBSPath(peer);
            string codePath = GetGenCodePath(peer);
            ReCreateFolder(codePath);
            foreach (ExcelData data in _excels)
            {
                string fbsName = FBSPath + "/" + data.sheetName + ".fbs";

                EnsureDirectory(codePath);

                var infos = GetInfos(data, peer);
                if (infos.Count == 0)
                {
                    continue;
                }

                // 配置操作配置文件的代码

                string arguments = string.Format("--csharp -o {0} {1} --gen-onefile", codePath, fbsName);
                ProcCmd(AppConfigs.Instance.FlatC, arguments, FBSPath, false);
                if (peer == EPeerType.SERVER)
                {
                    arguments = string.Format("--go -o {0} {1} ", codePath, fbsName);
                    ProcCmd(AppConfigs.Instance.FlatC, arguments, FBSPath, false);
                }

                // 生成将配置文件存储成文件的代码
                string dataClass = "Single" + data.sheetName + "Data";
                string className = data.sheetName + ConfigSaveFileEx;
                StringBuilder sb = new StringBuilder();
                sb.Append("using System;" + NewLine);
                sb.Append("using System.IO;" + NewLine);
                sb.Append("using System.Collections.Generic;" + NewLine);
                sb.Append("using FlatBuffers;" + NewLine);
                sb.Append("using AutoGenConfig;" + NewLine);
                sb.Append(NewLine);
                sb.Append("public class " + className + " {" + NewLine);
                sb.Append(NewLine);

                sb.Append(Tab + "public class Data {" + NewLine);
                foreach (ExcelFieldInfo info in infos.Values)
                {
                    sb.Append(Tab + Tab + "public " + FieldToType(info.type) + " " + info.name + ";" + NewLine);
                }
                sb.Append(Tab + "}" + NewLine);
                sb.Append(NewLine);

                sb.Append(Tab + "public void Save(List<Data> datas, string path) {" + NewLine);
                sb.Append(Tab + Tab + "FlatBufferBuilder fbb = new FlatBufferBuilder(1);" + NewLine);
                sb.Append(Tab + Tab + "int count = datas.Count;" + NewLine);
                sb.Append(string.Format(Tab + Tab + "Offset<{0}>[] offsets = new Offset<{1}>[count];", dataClass, dataClass) + NewLine);

                sb.Append(Tab + Tab + "for (int n = 0; n < count; ++n) {" + NewLine);
                sb.Append(Tab + Tab + Tab + "Data data = datas[n];" + NewLine);
                sb.Append(string.Format(Tab + Tab + Tab + "offsets[n] = {0}.Create{1}(fbb,", dataClass, dataClass) + NewLine);

                int index = 0;
                foreach (ExcelFieldInfo info in infos.Values)
                {
                    string type = FieldToType(info.type);
                    string end = ",";
                    if (index == infos.Count - 1)
                    {
                        end = ");";
                    }
                    if (type == "string")
                    {
                        sb.Append(string.Format(Tab + Tab + Tab + "fbb.CreateString(data.{0})", info.name) + end + NewLine);
                    }
                    else
                    {
                        sb.Append(Tab + Tab + Tab + "data." + info.name + end + NewLine);
                    }

                    index++;
                }
                sb.Append(Tab + Tab + "}" + NewLine);

                sb.Append(Tab + Tab + string.Format("VectorOffset dataOff = {0}.CreateDataVector(fbb, offsets);", data.sheetName) + NewLine);
                sb.Append(string.Format(Tab + Tab + "var configOff = {0}.Create{1}(fbb, dataOff);", data.sheetName, data.sheetName) + NewLine);
                sb.Append(string.Format(Tab + Tab + "{0}.Finish{1}Buffer(fbb, configOff);", data.sheetName, data.sheetName) + NewLine);

                sb.Append(Tab + Tab + "using (var ms = new MemoryStream(fbb.DataBuffer.Data, fbb.DataBuffer.Position, fbb.Offset)) {" + NewLine);
                sb.Append(Tab + Tab + Tab + "File.WriteAllBytes(path, ms.ToArray());" + NewLine);
                sb.Append(Tab + Tab + "}" + NewLine);

                sb.Append(Tab + "}" + NewLine);
                sb.Append("}" + NewLine);

                string fileName = codePath + className + ".cs";
                FileStream fs = new FileStream(fileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
                fs.Close();
            }

        }

        private void CompileCSharp()
        {
            CompileCSharp(EPeerType.CLIENT);
            CompileCSharp(EPeerType.SERVER);
        }

        private void CompileCSharp(EPeerType peer)
        {
            Console.WriteLine(string.Format("\n----------编译{0}C#代码----------", peer));
            string codePath = GetGenCodePath(peer);
            string[] files = Directory.GetFiles(codePath, "*.cs");

            string flatRefPath = AppConfigs.Instance.CompileRefPath + "FlatBuffers.dll";
            if (peer == EPeerType.CLIENT)
            {
                _clientAssmbly = CompileCS(flatRefPath, null, null, files);
            }
            else if (peer == EPeerType.SERVER)
            {
                _serverAssmbly = CompileCS(flatRefPath, null, null, files);
            }

            //_commonAssmbly = CompileCS(flatRefPath, null, null, AppConfigs.Instance.ClientTypePath);
        }

        /// <summary>
        /// 生成数据文件
        /// </summary>
        private void GenDataFiles()
        {
            GenDataFiles(EPeerType.CLIENT);
            GenDataFiles(EPeerType.SERVER);
        }

        private void GenDataFiles(EPeerType peer)
        {
            Console.WriteLine(string.Format("\n----------生成{0}数据文件----------", peer));
            if (peer == EPeerType.SERVER)
            {
                // 先把现有的全部删掉
                DeleteAllFiles(AppConfigs.Instance.ServerDataPath);
            }

            foreach (ExcelData data in _excels)
            { 
                var infos = GetInfos(data, peer);
                if (infos.Count == 0)
                {
                    continue;
                }

                string classTypeName = data.sheetName + ConfigSaveFileEx;

                // 反射生成类中类要用+连接
                string dataTypeName = data.sheetName + ConfigSaveFileEx + "+Data";

                Assembly assembly = GetAssembly(peer);
                object datas = Utils.CreateGeneric(typeof(List<>), assembly.GetType(dataTypeName));

#if USE_NPOI
                for (int n = 1; n <= data.sheet.LastRowNum; ++n)
                {
                    IRow dataRow = data.sheet.GetRow(n);
                    if (dataRow == null)
                    {
                        Console.WriteLine(string.Format("表{0}中 {1}页 找不到{2}行", data.excelName, data.sheetName, n));
                        continue;
                    }

                    ICell first = dataRow.GetCell(0);
                    // 第一个字段为空，则认为整个数据表已经结束
                    if (first == null || "" == first.ToString().Trim())
                    {
                        break;
                    }
#else
                for (int n = 1; n < data.table.Rows.Count; ++n) {
                    DataRow dataRow = data.table.Rows[n];
                    // 第一个字段为空，则认为整个数据表已经结束
                    if ("" == dataRow[0].ToString().Trim()) {
                        break;
                    }
#endif

                    object dataInst = assembly.CreateInstance(dataTypeName);
                    System.Reflection.FieldInfo[] fields = dataInst.GetType().GetFields();
                    foreach (System.Reflection.FieldInfo pi in fields)
                    {
                        ExcelFieldInfo fieldInfo = null;
                        string infoValue = null;
                        object value = null;

                        try
                        {
                            fieldInfo = data.fieldInfos[pi.Name];
#if USE_NPOI
                            ICell cell = dataRow.GetCell(fieldInfo.index);
                            if (cell == null)
                            {
                                infoValue = "";
                            }
                            else
                            {
                                if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                                {
                                    infoValue = cell.DateCellValue.ToString();
                                }
                                else
                                {
                                    infoValue = cell.ToString();
                                }
                            }
#else
                            infoValue = dataRow[fieldInfo.index].ToString();
#endif

                            value = Utils.GetFieldValue(data, fieldInfo, infoValue);

                            pi.SetValue(dataInst, value);
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            if (fieldInfo == null)
                            {
                                Console.WriteLine(string.Format("表{0}中 {1}页, 列名错误: {2}, 可能是页重名了,{3}", data.excelName, data.sheetName, pi.Name, e.Message));
                            } else
                            {
                                Console.WriteLine(string.Format("表{0}中 {1}页 {2}, 行: {3}, 列: {4}", data.excelName, data.sheetName, e.Message, n + 1, fieldInfo.desc));
                            }
                            Console.ResetColor();
                        }
                    }

                    datas.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, datas, new object[] { dataInst });
                }

                object classInst = assembly.CreateInstance(classTypeName);
                object[] parameters = new object[2];
                parameters[0] = datas;
                parameters[1] = GetDataPath(peer) + data.sheetName + ".bytes";
                Utils.CallMethod(classInst, "Save", parameters);
            }
        }

        /// <summary>
        /// 把生成的文件拷贝到对应的目录
        /// </summary>
        private void CopyFiles()
        {
            CopyFiles(EPeerType.CLIENT);
            CopyFiles(EPeerType.SERVER);
        }

        private void CopyFiles(EPeerType peer)
        {
            string genCodePath = GetGenCodePath(peer);
            string codePath = GetCodePath(peer);
            ReCreateFolder(codePath);
            string filter = "*.cs";
            if (peer == EPeerType.SERVER)
            {
                filter = "*.go";
                genCodePath += AppConfigs.Instance.FbsNameSpace;
                // 先把现有的全部删掉
                DeleteAllFiles(codePath);
            }

            string[] files = Directory.GetFiles(genCodePath, filter);
            string fileName = null;
            for (int n = 0; n < files.Length; ++n)
            {
                // 保存文件的代码不拷贝
                if (files[n].Contains(ConfigSaveFileEx))
                    continue;

                fileName = codePath + Path.GetFileName(files[n]);
                File.Copy(files[n], fileName, true);
            }
        }

        /// <summary>
        /// 动态编译C#
        /// </summary>
        /// <param name="refer"></param>
        /// <param name="output"></param>
        /// <param name="options"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private Assembly CompileCS(string refer, string output, string options, params string[] code)
        {
            CodeDomProvider domProvider = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters compileParams = new CompilerParameters();
            compileParams.GenerateExecutable = false;
            compileParams.GenerateInMemory = true;
            compileParams.ReferencedAssemblies.Add(AppConfigs.Instance.CompileRefPath + "System.dll");
            if (!string.IsNullOrEmpty(refer))
            {
                compileParams.ReferencedAssemblies.Add(refer);
            }
            if (!string.IsNullOrEmpty(output))
            {
                compileParams.OutputAssembly = output;
            }
            if (!string.IsNullOrEmpty(options))
            {
                compileParams.CompilerOptions = options;
            }

            CompilerResults compileResults = domProvider.CompileAssemblyFromFile(compileParams, code);

            if (compileResults.Errors.Count > 0)
            {
                Console.WriteLine("compile error!");
                foreach (CompilerError error in compileResults.Errors)
                {
                    Console.WriteLine(string.Format("  {0}", error.ToString()));
                    Console.WriteLine("");
                }
            }

            return compileResults.CompiledAssembly;
        }

        private void GenFieldInfos(ExcelData data)
        {

#if USE_NPOI
            IRow header = data.sheet.GetRow(0);
            if (header == null)
            {
                return;
            }
            for (int n = 0; n < header.LastCellNum; ++n)
            {
                string field = "";
                ICell cell = header.GetCell(n);
                if (cell == null)
                {
                    Console.WriteLine(data.excelName + "文件中的" + data.sheetName + "中有空列：" + n);
                    return;
                }
                field = cell.ToString();
#else
            DataRow header = data.table.Rows[0];
            for (int n = 0; n < header.ItemArray.Length; ++n) {
                string field = header.ItemArray[n].ToString();
#endif

                if (!field.Contains("|"))
                    continue;

                ExcelFieldInfo info = GetFieldInfo(data, field);
                if (null == info)
                {
                    continue;
                }
                if (info.desc.Contains("#"))
                {
                    continue;
                }

                info.index = n;
                try
                {
                    data.fieldInfos.Add(info.name, info);
                    if (info.peer == EPeerType.CLIENT || info.peer == EPeerType.BOTH)
                    {
                        data.clientFieldInfos.Add(info.name, info);
                    }
                    if (info.peer == EPeerType.SERVER || info.peer == EPeerType.BOTH)
                    {
                        data.serverFieldInfos.Add(info.name, info);
                    }
                }
                catch (ArgumentException)
                {
                    Console.Write(data.excelName + "文件中的" + data.sheetName + "表中的：" + info.name + " 键重复");
                    throw new Exception("键重复");
                }
            }
        }

        private ExcelFieldInfo GetFieldInfo(ExcelData data, string content)
        {
            content = content.Replace("\r\n", "").Replace("\n", "");
            Match match = _regex.Match(content);
            if (match.Success)
            {
                string flag = match.Groups["flag"].Value;

                // 过滤字段，｜表示客户端字段，||标识服务器端字段，|||表示客户端和服务器共用字段
                EPeerType peer = EPeerType.UNKOWN;
                if (flag == "|")
                {
                    peer = EPeerType.CLIENT;
                }
                else if (flag == "||")
                {
                    peer = EPeerType.SERVER;
                }
                else if (flag == "|||")
                {
                    peer = EPeerType.BOTH;
                }

                string name = match.Groups["name"].Value;
                string type = match.Groups["type"].Value;
                string desc = match.Groups["desc"].Value;
                string constraint = match.Groups["constraint"].Value;

                if (FieldTypeValid(type))
                {
                    ExcelFieldInfo info = new ExcelFieldInfo();
                    info.peer = peer;
                    info.name = name;
                    info.type = type;
                    info.desc = desc;
                    info.constraint = constraint;

                    return info;
                }
                else
                {
                    Console.WriteLine(string.Format("\"{0}\"表中的\"{1}\"页中的{2}字段类型不正确:{3}", data.excelName, data.sheetName, name, type));
                    return null;
                }
            }
            else
            {
                throw new Exception(string.Format("{0}表中{1}标题格式错误:{2}", data.excelName, data.sheetName, content));
            }
        }

        private Dictionary<string, ExcelFieldInfo> GetInfos(ExcelData data, EPeerType peer)
        {
            if (peer == EPeerType.CLIENT)
            {
                return data.clientFieldInfos;
            }
            if (peer == EPeerType.SERVER)
            {
                return data.serverFieldInfos;
            }

            return null;
        }

        private bool FieldMatch(EPeerType peer, EPeerType fieldPeer)
        {
            if (peer == EPeerType.CLIENT)
            {
                if (fieldPeer != EPeerType.CLIENT && fieldPeer != EPeerType.BOTH)
                {
                    return false;
                }
            }
            else if (peer == EPeerType.SERVER)
            {
                if (fieldPeer != EPeerType.SERVER && fieldPeer != EPeerType.BOTH)
                {
                    return false;
                }
            }

            return true;
        }

        private string GetFBSPath(EPeerType peer)
        {
            string path = AppConfigs.Instance.FBSClientPath;
            if (peer == EPeerType.SERVER)
            {
                path = AppConfigs.Instance.FBSServerPath;
            }

            return path;
        }

        private string GetGenCodePath(EPeerType peer)
        {
            string path = AppConfigs.Instance.GenClientCodePath;
            if (peer == EPeerType.SERVER)
            {
                path = AppConfigs.Instance.GenServerCodePath;
            }

            return path;
        }

        private string GetCodePath(EPeerType peer)
        {
            string path = AppConfigs.Instance.ClientCodePath;
            if (peer == EPeerType.SERVER)
            {
                path = AppConfigs.Instance.ServerCodePath;
            }

            return path;
        }

        private string GetDataPath(EPeerType peer)
        {
            string path = AppConfigs.Instance.ClientDataPath;
            if (peer == EPeerType.SERVER)
            {
                path = AppConfigs.Instance.ServerDataPath;
            }

            return path;
        }

        private string GetPeerName(EPeerType peer)
        {
            string name = "Client";
            if (peer == EPeerType.SERVER)
            {
                name = "Server";
            }

            return name;
        }

        private Assembly GetAssembly(EPeerType peer)
        {
            if (peer == EPeerType.CLIENT)
            {
                return _clientAssmbly;
            }
            else if (peer == EPeerType.SERVER)
            {
                return _serverAssmbly;
            }

            return null;
        }

        /// <summary>
        /// 字段类型是否正确
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool FieldTypeValid(string field)
        {
            if (_types.ContainsKey(field))
            {
                return true;
            }
            else if (field.Contains("enum"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 通过field获取C#数据类型
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private string FieldToType(string field)
        {
            string type = "";
            if (_types.TryGetValue(field, out type))
            {
                return type;
            }
            else if (field.Contains("enum"))
            {
                type = "int";
            }
            return type;
        }

        /// <summary>
        /// 删除目录下的所有文件
        /// </summary>
        /// <param name="path"></param>
        private void DeleteAllFiles(string path)
        {
            string[] files = Directory.GetFiles(AppConfigs.Instance.ServerCodePath);
            for (int n = 0; n < files.Length; ++n)
            {
                File.Delete(files[n]);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="exe"></param>
        /// <param name="arguments"></param>
        /// <param name="workDir"></param>
        /// <param name="useShell"></param>
        /// <returns></returns>
        private void ProcCmd(string exe, string arguments, string workDir, bool useShell)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exe;
            psi.Arguments = arguments;
            psi.WorkingDirectory = workDir;
            psi.UseShellExecute = useShell;
            Process.Start(psi).WaitForExit();
        }

        /// <summary>
        /// 阻塞等待进程
        /// </summary>
        /// <param name="procs"></param>
        /// <param name="remainCount"></param>
        private void WaitProcess(List<Process> procs, int remainCount)
        {
            while (GetActiveProcessNum(procs) > remainCount)
            {
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 获取当前正在运行进程的数量
        /// </summary>
        /// <param name="procs"></param>
        /// <returns></returns>
        private int GetActiveProcessNum(List<Process> procs)
        {
            for (int n = 0; n < procs.Count; ++n)
            {
                if (procs[n].HasExited)
                {
                    procs.RemoveAt(n);
                    --n;
                }
            }
            return procs.Count;
        }

        private void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}