using HxAntenna.Controllers.Common;
using HxAntenna.Lib;
using HxAntenna.Models;
using HxAntenna.Models.DAL;
using HxAntenna.Models.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HxAntenna.Controllers
{
    public class TestRecordController : Controller
    {
        List<string> path = new List<string> { };
        private UnitOfWork unitOfWork = new UnitOfWork();
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "TestRecord";
        public string ViewPathBase = "TestRecord";
        public string ViewPath2 = "/";

        public TestRecordController() {
            path.Add("质量管理");
            path.Add("测试记录");
            ViewBag.path = path;
            ViewBag.Name = "测试记录";
            ViewBag.Title = "测试记录";
        }
        public ActionResult Index(int page = 1, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "filter", filter } };
            return View();
        }

        public ActionResult Get(string returnRoot, string actionAjax = "", int page = 1, string filter = null, bool export = false)
        {
            var results = Common<TestResult>.GetQuery(unitOfWork, filter);

            results = results.OrderByDescending(a => a.TestTime);
            //not export
            if (!export)
            {
                var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "filter", filter } };
                return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<TestResult>.Page(this, rv, results));
            }
            else 
            {
                //initailize excel name
                string excelName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                if (results.Count() > 0)
                {
                    //reFormart filter if contains Time filter
                    filter = TimeFormat.TimeFilterConvert("TestTime", "TestTimeStartHour", ">=", filter);
                    filter = TimeFormat.TimeFilterConvert("TestTime", "TestTimeStopHour", "<=", filter);
                    //get testtime filter
                    string testTimeStarStr = GetfilterItemValStr(filter, "TestTime", ">=");
                    string testTimeStopStr = GetfilterItemValStr(filter, "TestTime", "<=");
                    DateTime testTimeStart = new DateTime(1900, 1, 1);
                    DateTime testTimeStop = DateTime.Now;
                    if (testTimeStarStr != "")
                    {
                        if (!DateTime.TryParseExact(testTimeStarStr, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStart))
                        {
                        }
                    }
                    if (testTimeStopStr != "")
                    {
                        if (!DateTime.TryParseExact(testTimeStopStr, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStop))
                        {
                        }
                    }
                    string serialnumberStr = GetfilterItemValStr(filter, "SerialNumber.Name", "%");

                    //call procedure
                    string sql = "exec p_testrecordexcel_Rp @testtimestart, @testtimestop, @serialnumber";
                    SqlParameter[] param = new SqlParameter[3];
                    param[0] = new SqlParameter("@testtimestart", SqlDbType.DateTime2);
                    param[0].Value = testTimeStart;
                    param[1] = new SqlParameter("@testtimestop", SqlDbType.DateTime2);
                    param[1].Value = testTimeStop;
                    param[2] = new SqlParameter("@serialnumber", SqlDbType.NVarChar);
                    param[2].Value = serialnumberStr;
                    //get procedure result
                    DataTable dt = CommonController.GetDateTable(sql, param);
                    List<TestRecordExport> testRecordExportList = new List<TestRecordExport>();
                    if(dt.Rows.Count > 0)
                    {
                        //auto map dt to VnaTotalResult
                        DataTableReader dr = dt.CreateDataReader();
                        testRecordExportList = AutoMapper.Mapper.DynamicMap<IDataReader, List<TestRecordExport>>(dr);
                        var testitemStandardPortList = testRecordExportList
                            .GroupBy(a => new
                            {
                                a.TestItemName,
                                a.Symbol,
                                a.StandardValue,
                                a.Port
                            }).Select(ac => new {
                               TestItemName = ac.Key.TestItemName,
                               Symbol = ac.Key.Symbol,
                               StandardValue = ac.Key.StandardValue,
                               Port = ac.Key.Port
                            }).OrderBy(p => p.TestItemName).ThenBy(p => p.Symbol).ThenBy(p => p.StandardValue).ThenBy(p => p.Port).ToList();
                        MemoryStream stream = new MemoryStream();
                        HSSFWorkbook workbook = new HSSFWorkbook();
                        workbook.CreateSheet("sheet1");
                        ISheet worksheet = workbook.GetSheet("sheet1");
                        //TestItem Title start from row 1
                        IRow testItemRowTitle = worksheet.CreateRow(0);
                        //Standard Title start from row 2
                        IRow standardRowTitle = worksheet.CreateRow(1);
                        //Port Title start from row 3, normal Title also start from row 3
                        IRow portOrNormalRowTitle = worksheet.CreateRow(2);
                        portOrNormalRowTitle.CreateCell(0).SetCellValue("测试时间");
                        portOrNormalRowTitle.CreateCell(1).SetCellValue("天线序列号");
                        portOrNormalRowTitle.CreateCell(2).SetCellValue("度数");
                        //initialize testitemStandardPortGoupList, use TestitemStandardPortGoup ViewModel to add row and cell for each testitemStandardPort
                        List<TestitemStandardPortGoup> testitemStandardPortGoupList = new List<TestitemStandardPortGoup>();
                        //cell start from 4
                        int cellPostionStart = 3;
                        foreach (var testitemStandardPort in testitemStandardPortList)
                        {
                            TestitemStandardPortGoup testitemStandardPortGoup = new TestitemStandardPortGoup {
                                TestItemName = testitemStandardPort.TestItemName,
                                Symbol = testitemStandardPort.Symbol,
                                StandardValue = testitemStandardPort.StandardValue,
                                Port = testitemStandardPort.Port,
                                CellNumber = cellPostionStart
                            };
                            testitemStandardPortGoupList.Add(testitemStandardPortGoup);
                            cellPostionStart = cellPostionStart + 1;
                        }
                        //write testitemStandardPortGoupList to excel
                        foreach (var testitemStandardPortGoup in testitemStandardPortGoupList)
                        {
                            int cellNumber = testitemStandardPortGoup.CellNumber;
                            testItemRowTitle.CreateCell(cellNumber).SetCellValue(testitemStandardPortGoup.TestItemName);
                            //formart standard value
                            string standardVal = "标准";
                            if (testitemStandardPortGoup.Symbol == 0)//<=
                            {
                                standardVal = standardVal + " <= " + testitemStandardPortGoup.StandardValue;
                            }
                            else if (testitemStandardPortGoup.Symbol == 1)//>=
                            {
                                standardVal = standardVal + " >= " + testitemStandardPortGoup.StandardValue;
                            }
                            else if (testitemStandardPortGoup.Symbol == 3)//<
                            {
                                standardVal = standardVal + " < " + testitemStandardPortGoup.StandardValue;
                            }
                            else//>
                            {
                                standardVal = standardVal + " > " + testitemStandardPortGoup.StandardValue;
                            }
                            standardRowTitle.CreateCell(cellNumber).SetCellValue(standardVal);
                            portOrNormalRowTitle.CreateCell(cellNumber).SetCellValue("端口" + testitemStandardPortGoup.Port);
                        }
                        //write data to excel
                        //initialize SerialNuber Degree group
                        var serialNumberDegreeGroupList = new List<string>();
                        //initialzie prevSerialnumber and preSerialCount, used for merge cell
                        string prevSerialnumber = "";
                        int preSerialCount = 0;
                        //start row from 4;
                        int startRow = 3;
                        foreach (var testRecordExport in testRecordExportList.OrderByDescending(a => a.TestTime).ThenBy(a => a.SerialNumber).ThenBy(a => a.Degree).ToList()) 
                        {
                            //merge group
                            if (prevSerialnumber != "" && prevSerialnumber != testRecordExport.SerialNumber)
                            {
                                //TestTime column
                                worksheet.AddMergedRegion(new CellRangeAddress(startRow - preSerialCount, startRow - 1, 0, 0));
                                //SerialNumber column
                                worksheet.AddMergedRegion(new CellRangeAddress(startRow - preSerialCount, startRow - 1, 1, 1));
                                prevSerialnumber = testRecordExport.SerialNumber;
                                preSerialCount = testRecordExport.maxdegree;
                            }
                            else 
                            {
                                prevSerialnumber = testRecordExport.SerialNumber;
                                preSerialCount = testRecordExport.maxdegree;
                            }
                            //get cell from testitemStandardPortGoupList
                            var testitemStandardPortGoup = testitemStandardPortGoupList
                                .Where(a => a.TestItemName == testRecordExport.TestItemName && a.Symbol == testRecordExport.Symbol && a.StandardValue == testRecordExport.StandardValue && a.Port == testRecordExport.Port)
                                .SingleOrDefault();
                            string testDate = testRecordExport.TestTime.ToString();
                            string serialNumber = testRecordExport.SerialNumber;
                            string degree = testRecordExport.Degree + "℃";
                            //new serrialNumberDegreeGroup, store distinct(SerialNumber + Degree)
                            string serialNumberDegreeGroup = testRecordExport.SerialNumber + testRecordExport.Degree;
                            //if not contains in serialNumberDegreeGroupList
                            if (!serialNumberDegreeGroupList.Contains(serialNumberDegreeGroup))
                            {
                                //create new Row
                                IRow newRow = worksheet.CreateRow(startRow);
                                newRow.CreateCell(0).SetCellValue(testDate);
                                newRow.CreateCell(1).SetCellValue(serialNumber);
                                newRow.CreateCell(2).SetCellValue(degree);
                                //write value to selected cell
                                newRow.CreateCell(testitemStandardPortGoup.CellNumber).SetCellValue(testRecordExport.TestData.ToString());
                                //add serialNumberDegreeGroup to serialNumberDegreeGroupList
                                serialNumberDegreeGroupList.Add(serialNumberDegreeGroup);
                                startRow = startRow + 1;
                            }
                            else //if contains in serialNumberDegreeGroup， do not write normal col
                            {
                                //get pre Row because Group is Ordered
                                IRow currentRow = worksheet.GetRow(startRow - 1);
                                //write fail length
                                currentRow.CreateCell(testitemStandardPortGoup.CellNumber).SetCellValue(testRecordExport.TestData.ToString());
                            }
                        }
                        //merge latest group
                        //TestTime column
                        worksheet.AddMergedRegion(new CellRangeAddress(startRow - preSerialCount, startRow - 1, 0, 0));
                        //SerialNumber column
                        worksheet.AddMergedRegion(new CellRangeAddress(startRow - preSerialCount, startRow - 1, 1, 1));
                        if (!workbook.IsWriteProtected)
                        {
                            workbook.Write(stream);
                        }
                        return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                    }
                    else
                    {
                        MemoryStream stream = new MemoryStream();
                        HSSFWorkbook workbook = new HSSFWorkbook();
                        workbook.CreateSheet("sheet1");
                        ISheet worksheet = workbook.GetSheet("sheet1");
                        IRow firstRow = worksheet.CreateRow(0);
                        ICell firstCell = firstRow.CreateCell(0);
                        firstCell.SetCellValue("查询记录为空");
                        if (!workbook.IsWriteProtected)
                        {
                            workbook.Write(stream);
                        }
                        return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                    }
                }
                else
                {
                    MemoryStream stream = new MemoryStream();
                    HSSFWorkbook workbook = new HSSFWorkbook();
                    workbook.CreateSheet("sheet1");
                    ISheet worksheet = workbook.GetSheet("sheet1");
                    IRow firstRow = worksheet.CreateRow(0);
                    ICell firstCell = firstRow.CreateCell(0);
                    firstCell.SetCellValue("查询记录为空");
                    if (!workbook.IsWriteProtected)
                    {
                        workbook.Write(stream);
                    }
                    return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Details(int Id = 0, string returnUrl = "Index") 
        {
            var result = Common<TestResult>.GetQuery(unitOfWork)
                .Where(a => a.Id == Id).SingleOrDefault();
            if (result == null)
            {
                CommonMsg.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Details.cshtml", result);
        }
        //get value in filter, by filter string name, filter string symbol
        private string GetfilterItemValStr(string filter = "", string filterItemName = "", string filterItemSymbol = "")
        {
            string filterItemVal = "";
            string filterStr = filterItemName + "@" + filterItemSymbol + ":";
            int indexOfFilter = filter.IndexOf(filterStr);
            if (indexOfFilter != -1) 
            {
                filterItemVal = filter.Substring(indexOfFilter, filter.Length - indexOfFilter);
                var stopIndex = filterItemVal.IndexOf(";");
                var startIndex = filterStr.Length;
                filterItemVal = filterItemVal.Substring(startIndex, stopIndex - startIndex);
            }
            return filterItemVal;
        }
	}
}