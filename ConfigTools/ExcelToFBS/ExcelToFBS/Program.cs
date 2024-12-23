using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Threading;
using System.Diagnostics;
using NPOI;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace ExcelToFBS
{
    class Program
    {
        static string _excelHeader = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 XML;HDR=NO;IMEX=1;'";
        static string _excelPath = "";
        static List<ExcelData> _excelDatas = new List<ExcelData>();

        static void Main(string[] args)
        {
            _excelPath = AppConfigs.Instance.ExcelPath;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("\n----------生成数据文件----------");
            try
            {
#if USE_NPOI
                ReadExcelNPOI();
#else
                ReadExcelOle();
#endif
                EGenType genType = EGenType.BOTH;
                if (args.Length != 0)
                {
                    if (args[0] == "client")
                    {
                        genType = EGenType.CLIENT;
                    }
                    else if (args[0] == "server")
                    {
                        genType = EGenType.SERVER;
                    }
                }
                else
                {
                    genType = EGenType.CLIENT;
                }
                Convertor convertor = new Convertor(genType, 1, _excelDatas);
                convertor.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            sw.Stop();
            Console.WriteLine("\n处理完毕，用时：: " + sw.ElapsedMilliseconds / 1000.0f);
            Console.ReadKey();
        }

        private static void ReadExcelNPOI()
        {
            string[] files = Directory.GetFiles(_excelPath, "*.xlsx");
            foreach (string file in files)
            {
                string excelName = Path.GetFileNameWithoutExtension(file);

                // 正在编辑的Excel文件会生成一个临时文件，并以~$开头
                if (excelName.StartsWith("~$"))
                    continue;

                XSSFWorkbook wk = new XSSFWorkbook(file);
                int count = wk.Count;
                for (int n = 0; n < count; ++n)
                {
                    ISheet sheet = wk[n];

                    string sheetName = sheet.SheetName;

                    // 忽略描述页(带中括号的视为描述页)
                    if (sheetName.Contains("【"))
                        continue;

                    if (!sheetName.EndsWith("Config"))
                    {
                        Console.WriteLine(string.Format("\n{0}.xlsx中的{1}标签必须以Config结尾！", excelName, sheetName));
                        continue;
                    }

                    ExcelData data = new ExcelData();
                    data.excelName = excelName;
                    data.sheetName = sheetName;
#if USE_NPOI
                    data.sheet = sheet;
#endif
                    _excelDatas.Add(data);
                }
            }
        }

        private static void ReadExcelOle()
        {
            string[] files = Directory.GetFiles(_excelPath, "*.xlsx");
            foreach (string file in files)
            {
                string excelName = Path.GetFileNameWithoutExtension(file);
                // 正在编辑的Excel文件会生成一个临时文件，并以~$开头
                if (excelName.StartsWith("~$"))
                    continue;

                string connectionString = string.Format(_excelHeader, file);
                OleDbConnection ODC = new OleDbConnection(connectionString);

                try
                {
                    ODC.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                DataTable sheets = ODC.GetSchema("Tables");
                foreach (DataRow sheet in sheets.Rows)
                {
                    if (sheet["Table_Type"].ToString() == "TABLE")
                    {
                        string sheetName = sheet["Table_Name"].ToString();

                        // 忽略描述页
                        if (sheetName == "description$")
                            continue;

                        if (!sheetName.EndsWith("Config$"))
                        {
                            Console.WriteLine(string.Format("\n{0}.xlsx中的{1}标签必须以Config结尾！", excelName, sheetName));
                            continue;
                        }

                        OleDbDataAdapter adapter = new OleDbDataAdapter("select * from [" + sheetName + "]", ODC);
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        if (table.Rows.Count < 1)
                            continue;

                        sheetName = sheetName.Replace("$", "");

                        ExcelData data = new ExcelData();
                        data.excelName = excelName;
                        data.sheetName = sheetName;
#if !USE_NPOI
                        data.table = table;
#endif
                        _excelDatas.Add(data);
                    }
                }

                ODC.Close();
            }
        }
    }
}
