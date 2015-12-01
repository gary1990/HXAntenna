using HxAntenna.Common;
using HxAntenna.Models;
using HxAntenna.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using HxAntenna.Models.ViewModels;
using System.Text;
using Ionic.Zip;
using System.Transactions;
using System.IO;
using System.Globalization;
using HxAntenna.Models.Constant;
using HxAntenna.Lib;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace HxAntenna.Controllers
{
    public class ApiController : Controller
    {
        private UnitOfWork unitOfWork = new UnitOfWork();
        public ActionResult ClientLogin(string jobNumber = null, string passWord = null)
        {
            SingleResultXml result = new SingleResultXml() { Message = "true"};
            if (String.IsNullOrWhiteSpace(jobNumber) || String.IsNullOrWhiteSpace(passWord))
            {
                result.Message = "jobnumer or password can not be null.";
            }
            else 
            {
                try 
                {
                    var user = unitOfWork.AntennaUserRepository.Get(a => a.JobNumber.ToUpper() == (jobNumber.ToUpper()) && a.AntennaRole.Name == "测试员" && a.IsDeleted == false).SingleOrDefault();
                    if (user == null)
                    {
                        result.Message = "jobnumber or password incorrect.";
                    }
                    else 
                    {
                        var userName = user.UserName;
                        user = unitOfWork.AntennaUserRepository.context.UserManager.Find(userName, passWord);
                        if(user == null)
                        {
                            result.Message = "jobnumber or password incorrect.";
                        }
                    }
                }
                catch(Exception e)
                {
                    result.Message = e.Message;
                }
            }
            return new XmlResult<SingleResultXml>() { Data = result };
        }

        public ActionResult UploadFile() 
        {
            SingleResultXml result = new SingleResultXml() { Message = "true" };
            HttpPostedFileBase file = Request.Files["file"];
            string fileFullName;
            string fileEx;
            string fileNameWithoutEx;
            string slash = "/";
            string uploadTime = DateTime.Now.ToString("yyyyMMdd");
            string uploadPath = AppDomain.CurrentDomain.BaseDirectory + "/UploadedFolder/" + uploadTime;;
            string savePath;
            string saveFolderPath;
            

            if (file == null || file.ContentLength <= 0)
            {
                result.Message = "file can not be null";
                return new XmlResult<SingleResultXml>() { Data = result }; 
            }
            fileFullName = System.IO.Path.GetFileName(file.FileName);
            fileEx = System.IO.Path.GetExtension(fileFullName);
            fileNameWithoutEx = System.IO.Path.GetFileNameWithoutExtension(fileFullName);
            if (fileEx != ".zip")
            {
                result.Message = "incorrect file type";
                return new XmlResult<SingleResultXml>() { Data = result }; 
            }
            var fileNameSplit = fileFullName.Split('_');
            if (fileNameSplit[0] == "" || fileNameSplit[1] == "")
            {
                result.Message = "incorrect file name";
                return new XmlResult<SingleResultXml>() { Data = result }; 
            }
            if(!System.IO.Directory.Exists(uploadPath))
            {
                try 
                {
                    System.IO.Directory.CreateDirectory(uploadPath);
                }
                catch(Exception /*e*/)
                {
                    result.Message = "can not create upalod directory";
                    return new XmlResult<SingleResultXml>() { Data = result }; 
                }
            }
            savePath = System.IO.Path.Combine(uploadPath, fileFullName);
            try 
            {
                file.SaveAs(savePath);
            }
            catch(Exception /*e*/)
            {
                result.Message = "can not save uploaded file";
                return new XmlResult<SingleResultXml>() { Data = result }; 
            }

            ZipFile zip = ZipFile.Read(savePath, new ReadOptions { Encoding = Encoding.Default });
            try
            {
                //unzip file
                zip.AlternateEncoding = Encoding.Default;
                zip.ExtractAll(uploadPath, ExtractExistingFileAction.OverwriteSilently);
            }
            catch (Exception) 
            {
                result.Message = "can not unzip file";
                return new XmlResult<SingleResultXml>() { Data = result }; 
            }
            zip.Dispose();
            using (var scope = new TransactionScope())
            {
                saveFolderPath = uploadPath + slash + fileNameWithoutEx;
                //read general.csv
                string generalCsvPath = saveFolderPath + slash + "general.csv";
                if (!System.IO.File.Exists(generalCsvPath))
                {
                    result.Message = "can not find general.csv";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                StreamReader srGeneralCsv = new StreamReader(saveFolderPath + slash + "general.csv");
                string testTimeStr;
                string jobnumberStr;
                string serialnumberStr;
                string testitemStr;
                string testitemNuberStr;
                string line = string.Empty;
                bool isEsc = false;//是否电调
                string[] lineArr = null;
                line = srGeneralCsv.ReadLine();
                if(line == null)
                {
                    srGeneralCsv.Close();
                    result.Message = "general.csv first line is null";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                lineArr = line.Split(',');
                if (lineArr.Count() < 5)
                {
                    srGeneralCsv.Close();
                    result.Message = "general.csv first line content error";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                testTimeStr = lineArr[0];
                jobnumberStr = lineArr[1];
                serialnumberStr = lineArr[2];
                testitemStr = lineArr[3];
                testitemNuberStr = lineArr[4];
                srGeneralCsv.Close();
                //get all .csv file except general.csv, and check degree csv file exists
                string[] degreeFiles = Directory.GetFiles(saveFolderPath, "*.csv");
                if (degreeFiles.Count() <= 1)
                {
                    result.Message = "test data is null";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                else if (degreeFiles.Count() > 2)//csv file number > 2, is ESC
                {
                    isEsc = true;
                }
                //convert testTimeStr to DateTime
                DateTime testTime;
                if (!DateTime.TryParseExact(testTimeStr, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTime))
                {
                    result.Message = "general.csv testtime convert failed";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                //get Tester(AntennaUser)
                AntennaUser testerDb = unitOfWork.AntennaUserRepository.Get(a => a.JobNumber.ToUpper() == jobnumberStr.ToUpper() && a.IsDeleted == false && a.AntennaRole.Name == "测试员").SingleOrDefault();
                if(testerDb == null)
                {
                    result.Message = "general.csv tester can not find";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                //convert testitemStr to testItemInt
                int testItemInt = Int32.Parse(testitemStr);
                bool isPIMTest = false;
                if(testItemInt == 1)//驻波 or 隔离
                {
                    isPIMTest = false;
                }
                else if(testItemInt == 2)//PIM
                {
                    isPIMTest = true;
                }
                else
                {
                    result.Message = "general.csv testitem can not recognized";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                //convert testitemNumberStr, justify if 隔离 is test
                int testItemNumer = Int32.Parse(testitemNuberStr);
                bool segregationTested = false;
                if (testItemNumer == 1)
                {
                    segregationTested = false;
                }
                else if (testItemNumer == 2)
                {
                    segregationTested = true;
                }
                else 
                {
                    result.Message = "general.csv testitem number can not recognized";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                //get SerialNumber in db or add new SerialNumber record to db
                var serialNumberDb = unitOfWork.SerialNumberRepository.Get(a => a.Name == serialnumberStr).SingleOrDefault();
                int serialNumberDbId;
                if (serialNumberDb != null)
                {
                    serialNumberDbId = serialNumberDb.Id;
                }
                else 
                {
                    try 
                    {
                        SerialNumber serialNumberAdd = new SerialNumber { Name = serialnumberStr};
                        unitOfWork.SerialNumberRepository.Insert(serialNumberAdd);
                        unitOfWork.DbSaveChanges();
                        serialNumberDbId = serialNumberAdd.Id;
                    }
                    catch(Exception /*e*/)
                    {
                        result.Message = "Insert SerialNumber failed";
                        return new XmlResult<SingleResultXml>() { Data = result }; 
                    }
                }
                //get TestResult in db or add new TestResult record to db
                var testResultDb = unitOfWork.TestResultRepository.Get(a => a.SerialNumberId == serialNumberDbId).SingleOrDefault();
                bool totalResult = true;
                if (testResultDb != null)
                {
                    totalResult = testResultDb.Result;
                    try
                    {
                        testResultDb.TestTime = testTime;
                        unitOfWork.TestResultRepository.Update(testResultDb);
                        unitOfWork.DbSaveChanges();
                    }
                    catch (Exception /*e*/)
                    {
                        result.Message = "Update TestResult TestTime failed";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                }
                else 
                {
                    try 
                    {
                        testResultDb = new TestResult { SerialNumberId = serialNumberDbId, TestTime = testTime, Result = totalResult };
                        unitOfWork.TestResultRepository.Insert(testResultDb);
                        unitOfWork.DbSaveChanges();
                    }
                    catch(Exception /*e*/)
                    {
                        result.Message = "Add TestResult record failed";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                }
                //get testitemId
                int testItemSwId;
                int testItemSegId;
                int testItemPimId;
                try 
                {
                    testItemSwId = unitOfWork.TestItemRepository.Get(a => a.Name == "驻波").SingleOrDefault().Id;
                    testItemSegId = unitOfWork.TestItemRepository.Get(a => a.Name == "隔离").SingleOrDefault().Id;
                    testItemPimId = unitOfWork.TestItemRepository.Get(a => a.Name == "交调").SingleOrDefault().Id;
                }
                catch(Exception /*e*/)
                {
                    result.Message = "can not get Testitem Ids";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                //add TestResultItem and read per .csv file expect general.csv
                if ((!isPIMTest) && segregationTested)//is not PIM test and 隔离 is tested
                {
                    //UPDATE old TestResultItem in db where TestItem is 驻波 or 隔离 and IsLatestTest is true, set IsLatestTest to false
                    var testResultItemsOld = testResultDb.TestResultItems.Where(a => (a.TestItemId == testItemSwId || a.TestItemId == testItemSegId) && a.IsLatestTest == true).ToList();
                    foreach (var testResultItemOld in testResultItemsOld)
                    {
                        testResultItemOld.IsLatestTest = false;
                        try
                        {
                            unitOfWork.TestResultItemRepository.Update(testResultItemOld);
                            unitOfWork.DbSaveChanges();
                        }
                        catch (Exception /*e*/)
                        {
                            result.Message = "SW and SEQ:add TestResultItem failed";
                            return new XmlResult<SingleResultXml>() { Data = result };
                        }
                    }
                    bool resultItemSw = true;//驻波项结果
                    bool resultItemSeg = true;//隔离项结果
                    //驻波
                    TestResultItem testResultItemSwAdd = new TestResultItem { 
                        TestResultId = testResultDb.Id, 
                        ResultItem = resultItemSw,
                        TestItemId = testItemSwId, 
                        AntennaUserId = testerDb.Id,
                        TestTimeItem = testTime,
                        IsLatestTest = true,
                        IsEsc = isEsc
                    };
                    //隔离
                    TestResultItem testResultItemSegAdd = new TestResultItem
                    {
                        TestResultId = testResultDb.Id,
                        ResultItem = resultItemSeg,
                        TestItemId = testItemSegId,
                        AntennaUserId = testerDb.Id,
                        TestTimeItem = testTime,
                        IsLatestTest = true,
                        IsEsc = isEsc
                    };
                    try 
                    {
                        unitOfWork.TestResultItemRepository.Insert(testResultItemSwAdd);
                        unitOfWork.TestResultItemRepository.Insert(testResultItemSegAdd);
                        unitOfWork.DbSaveChanges();
                    }
                    catch(Exception /*e*/)
                    {
                        result.Message = "SW and SEQ:add TestResultItem failed";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                    //read per degree csv file
                    foreach (var degreefilePath in degreeFiles)
                    {
                        if (!degreefilePath.ToUpper().Contains("GENERAL"))
                        {
                            //add TestResultItemDegree and TestResultItemDegreeVal
                            TestResultItemDegree testResultItemDegree;
                            decimal degree;
                            string degreeStr = System.IO.Path.GetFileNameWithoutExtension(degreefilePath);
                            bool resultItemDegree = true;//度数结果
                            int testResultItemId = 0;
                            if (!Decimal.TryParse(degreeStr, out degree))
                            {
                                result.Message = "SW and SEQ:can not Parse " + degreeStr + "  to decimal";   
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                            string degreeImgPath = saveFolderPath + slash + degreeStr + ".jpg";
                            if (!System.IO.File.Exists(degreeImgPath)) 
                            {
                                result.Message = "SW and SEQ:can not find" + degreeStr + ".jpg file";
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                            degreeImgPath = uploadTime + slash + fileNameWithoutEx + slash + degreeStr + ".jpg";
                            StreamReader degreeCsvSr = new StreamReader(degreefilePath);
                            string degreeItemLine = string.Empty;
                            string[] degreeItemPortArr = null;
                            int lineNumber = 0;
                            //read every line in csv file, line > 2 will be ignore
                            do
                            {
                                lineNumber++;
                                if (lineNumber <= 2)//read first two numbers
                                {
                                    degreeItemLine = degreeCsvSr.ReadLine();
                                    //reset ResultItemDegree for each line, because line 1 is SW, line 2 is SEQ
                                    resultItemDegree = true;
                                    if (lineNumber == 1)//驻波
                                    {
                                        if (degreeItemLine == null)
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:" + degreeStr + ".csv 驻波 test data is null";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        else
                                        {
                                            testResultItemId = testResultItemSwAdd.Id;
                                        }
                                    }
                                    if (lineNumber == 2)//隔离
                                    {
                                        if (degreeItemLine == null)
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:" + degreeStr + ".csv 隔离 test data is null";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        else
                                        {
                                            testResultItemId = testResultItemSegAdd.Id;
                                        }
                                    }
                                    testResultItemDegree = new TestResultItemDegree { TestResultItemId = testResultItemId, Degree = degree, Img = degreeImgPath, ResultItemDegree = resultItemDegree };
                                    try
                                    {
                                        unitOfWork.TestResultItemDegreeRepository.Insert(testResultItemDegree);
                                        unitOfWork.DbSaveChanges();
                                    }
                                    catch (Exception /*e*/)
                                    {
                                        degreeCsvSr.Close();
                                        result.Message = "SW and SEQ:Add to TestResultItemDegree failed";
                                        return new XmlResult<SingleResultXml>() { Data = result };
                                    }
                                    //split each line with ;
                                    degreeItemPortArr = degreeItemLine.Split(';');
                                    //port in TestResultItemDegreeVal model
                                    int portNumber = 0;
                                    foreach (var degreeItemPort in degreeItemPortArr)
                                    {
                                        portNumber++;
                                        //splite each data with ,
                                        string[] degreeItemPortData = degreeItemPort.Split(',');
                                        //number not 4, return error
                                        if (degreeItemPortData.Count() != 4)// 4 colums
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:" + degreeStr + ".csv test data number is incorrect";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        //4 columes string
                                        string testDataStr = degreeItemPortData[0];
                                        string standardValueStr = degreeItemPortData[1];
                                        string standardSymbolStr = degreeItemPortData[2];
                                        string resultItemDegreeValStr = degreeItemPortData[3];
                                        //4 columes in db
                                        decimal testData;
                                        decimal standardValue;
                                        Symbol standardSymbol;
                                        bool resultItemDegreeVal;
                                        //convert and check 4 colums
                                        if (!Decimal.TryParse(testDataStr, out testData))
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:can not Parse " + testDataStr + " in " + degreeStr + ".csv line " + lineNumber + " to decimal";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        if (!Decimal.TryParse(standardValueStr, out standardValue))
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:can not Parse " + standardValueStr + " in " + degreeStr + ".csv " + lineNumber + " to decimal";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        if (standardSymbolStr == "0")//0 represent <=
                                        {
                                            standardSymbol = Symbol.LessOrEqual;
                                        }
                                        else if (standardSymbolStr == "1")//1 represent >=
                                        {
                                            standardSymbol = Symbol.GreatOrEqual;
                                        }
                                        else
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:can not Parse " + standardSymbolStr + " in " + degreeStr + ".csv line " + lineNumber + " to ligal Compare Symbol";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        if (resultItemDegreeValStr == "0")//0 represent fail
                                        {
                                            resultItemDegreeVal = false;
                                        }
                                        else if (resultItemDegreeValStr == "1")//1 represent pass
                                        {
                                            resultItemDegreeVal = true;
                                        }
                                        else
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:can not Parse " + standardSymbolStr + " in " + degreeStr + ".csv line " + lineNumber + " to ligal test result";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        //update resultItemDegree with this ResultItemDegreeVal
                                        resultItemDegree = resultItemDegree && resultItemDegreeVal;
                                        //get TestStandardId
                                        var testStadardDb = unitOfWork.TestStandardRepository.Get(a => a.StandardValue == standardValue && a.Symbol == standardSymbol).SingleOrDefault();
                                        if (testStadardDb == null)
                                        {
                                            testStadardDb = new TestStandard { StandardValue = standardValue, Symbol = standardSymbol };
                                            try
                                            {
                                                unitOfWork.TestStandardRepository.Insert(testStadardDb);
                                                unitOfWork.DbSaveChanges();
                                            }
                                            catch (Exception /*e*/)
                                            {
                                                degreeCsvSr.Close();
                                                result.Message = "SW and SEQ:can not ADD " + standardValueStr + ":" + standardSymbolStr + " in " + degreeStr + ".csv line " + lineNumber + " to TestStandard";
                                                return new XmlResult<SingleResultXml>() { Data = result };
                                            }
                                        }
                                        //add TestResultItemDegreeVal
                                        TestResultItemDegreeVal testResultItemDegreeVal = new TestResultItemDegreeVal
                                        {
                                            TestResultItemDegreeId = testResultItemDegree.Id,
                                            TestStandardId = testStadardDb.Id,
                                            Port = portNumber,
                                            TestData = testData,
                                            ResultItemDegreeVal = resultItemDegreeVal
                                        };
                                        try
                                        {
                                            unitOfWork.TestResultItemDegreeValRepository.Insert(testResultItemDegreeVal);
                                            unitOfWork.DbSaveChanges();
                                        }
                                        catch (Exception /*e*/)
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW and SEQ:add TestResultItemDegreeVal failed";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                    }
                                    //update TestResultItemDegree ResultItemDegree
                                    testResultItemDegree.ResultItemDegree = resultItemDegree;
                                    try
                                    {
                                        unitOfWork.TestResultItemDegreeRepository.Update(testResultItemDegree);
                                        unitOfWork.DbSaveChanges();
                                    }
                                    catch (Exception /*e*/)
                                    {
                                        result.Message = "SW and SEQ:unable UPDATE TestResultItemDegree model's ResultItemDegree";
                                        return new XmlResult<SingleResultXml>() { Data = result };
                                    }
                                    //juticfy current TestResultItemDegree belongs to 驻波 or 隔离
                                    if (testResultItemDegree.TestResultItemId == testResultItemSwAdd.Id)
                                    {
                                        resultItemSw = resultItemSw && testResultItemDegree.ResultItemDegree;
                                    }
                                    if (testResultItemDegree.TestResultItemId == testResultItemSegAdd.Id)
                                    {
                                        resultItemSeg = resultItemSeg && testResultItemDegree.ResultItemDegree;
                                    }
                                }
                                else 
                                {
                                    //lineNumber > 2, set degreeItemLine to null, stop read file
                                    degreeItemLine = null;
                                }
                            }
                            while (degreeItemLine != null);
                            //close current degree csv file   
                            degreeCsvSr.Close();
                        }
                    }
                    //update TestResutItem ResultItem
                    testResultItemSwAdd.ResultItem = resultItemSw;
                    testResultItemSegAdd.ResultItem = resultItemSeg;
                    testResultDb.Result = resultItemSw && resultItemSeg;
                    try 
                    {
                        unitOfWork.TestResultItemRepository.Update(testResultItemSwAdd);
                        unitOfWork.TestResultItemRepository.Update(testResultItemSegAdd);
                        unitOfWork.DbSaveChanges();
                    }
                    catch(Exception /*e*/)
                    {
                        result.Message = "SW and SEQ:unable UPDATE TestResultItem model's ResultItem";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                    try 
                    {
                        unitOfWork.TestResultRepository.Update(testResultDb);
                        unitOfWork.DbSaveChanges();
                    }
                    catch(Exception /*e*/)
                    {
                        result.Message = "SW and SEQ:unable UPDATE TestResult model's Result";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                }
                else if (((!isPIMTest) && (!segregationTested)) || (isPIMTest && (!segregationTested)))//is not PIM test and 隔离 is not tested || PIM test
                {
                    //current TestItem Id
                    int testItemIdCurr = 0;
                    if (!isPIMTest)//TestItem is SW
                    {
                        testItemIdCurr = testItemSwId;
                    }
                    else //TestItem is PIM
                    {
                        testItemIdCurr = testItemPimId;
                    }
                    //UPDATE old TestResultItem in db where TestItem is current TestItem and IsLatestTest is true, then set IsLatestTest to false
                    var testResultItemsOld = testResultDb.TestResultItems.Where(a => a.TestItemId == testItemIdCurr && a.IsLatestTest == true).ToList();
                    foreach (var testResultItemOld in testResultItemsOld)
                    {
                        testResultItemOld.IsLatestTest = false;
                        try
                        {
                            unitOfWork.TestResultItemRepository.Update(testResultItemOld);
                            unitOfWork.DbSaveChanges();
                        }
                        catch (Exception /*e*/)
                        {
                            result.Message = "SW or PIM:add TestResultItem failed";
                            return new XmlResult<SingleResultXml>() { Data = result };
                        }
                    }
                    bool resultItemSwOrPim = true;//驻波/PIM项结果
                    bool resultItemOther = true;//ResultItem of other TestItem（except current TestItem）
                    //GET TestResultItem in db where TestItem is Not current TestItem and IsLatestTest is true
                    var testResultItemsNotCurrOld = testResultDb.TestResultItems.Where(a => a.TestItemId != testItemIdCurr && a.IsLatestTest == true).ToList();
                    foreach(var testResultItemNotCurrOld in testResultItemsNotCurrOld)
                    {
                        resultItemOther = resultItemOther && testResultItemNotCurrOld.ResultItem;
                    }
                    //驻波 or PIM
                    TestResultItem testResultItemSwOrPimAdd = new TestResultItem
                    {
                        TestResultId = testResultDb.Id,
                        ResultItem = resultItemSwOrPim,
                        TestItemId = testItemIdCurr,
                        AntennaUserId = testerDb.Id,
                        TestTimeItem = testTime,
                        IsLatestTest = true,
                        IsEsc = isEsc
                    };
                    try
                    {
                        unitOfWork.TestResultItemRepository.Insert(testResultItemSwOrPimAdd);
                        unitOfWork.DbSaveChanges();
                    }
                    catch (Exception /*e*/)
                    {
                        result.Message = "SW or PIM:add TestResultItem failed";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                    //read per degree csv file
                    foreach (var degreefilePath in degreeFiles)
                    {
                        if (!degreefilePath.ToUpper().Contains("GENERAL"))
                        {
                            //add TestResultItemDegree and TestResultItemDegreeVal
                            TestResultItemDegree testResultItemDegree;
                            decimal degree;
                            string degreeStr = System.IO.Path.GetFileNameWithoutExtension(degreefilePath);
                            bool resultItemDegree = true;//度数结果
                            int testResultItemId = 0;
                            if (!Decimal.TryParse(degreeStr, out degree))
                            {
                                result.Message = "SW:can not Parse " + degreeStr + "  to decimal";
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                            string degreeImgPath = saveFolderPath + slash + degreeStr + ".jpg";
                            if (!System.IO.File.Exists(degreeImgPath))
                            {
                                result.Message = "SW:can not find" + degreeStr + ".jpg file";
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                            degreeImgPath = uploadTime + slash + fileNameWithoutEx + slash + degreeStr + ".jpg";
                            StreamReader degreeCsvSr = new StreamReader(degreefilePath);
                            string degreeItemLine = string.Empty;
                            string[] degreeItemPortArr = null;
                            int lineNumber = 0;
                            //read every line in csv file, line > 1 will be ignore
                            do
                            {
                                lineNumber++;
                                if (lineNumber <= 1)//read first line, because only SW/PIM
                                {
                                    degreeItemLine = degreeCsvSr.ReadLine();
                                    //check line content not null
                                    if (degreeItemLine == null)
                                    {
                                        degreeCsvSr.Close();
                                        result.Message = "SW or PIM:" + degreeStr + ".csv test data is null";
                                        return new XmlResult<SingleResultXml>() { Data = result };
                                    }
                                    else
                                    {
                                        testResultItemId = testResultItemSwOrPimAdd.Id;
                                    }
                                    
                                    testResultItemDegree = new TestResultItemDegree { TestResultItemId = testResultItemId, Degree = degree, Img = degreeImgPath, ResultItemDegree = resultItemDegree };
                                    try
                                    {
                                        unitOfWork.TestResultItemDegreeRepository.Insert(testResultItemDegree);
                                        unitOfWork.DbSaveChanges();
                                    }
                                    catch (Exception /*e*/)
                                    {
                                        degreeCsvSr.Close();
                                        result.Message = "SW or PIM:Add to TestResultItemDegree failed";
                                        return new XmlResult<SingleResultXml>() { Data = result };
                                    }
                                    //split each line with ;
                                    degreeItemPortArr = degreeItemLine.Split(';');
                                    //port in TestResultItemDegreeVal model
                                    int portNumber = 0;
                                    foreach (var degreeItemPort in degreeItemPortArr)
                                    {
                                        portNumber++;
                                        //splite each data with ,
                                        string[] degreeItemPortData = degreeItemPort.Split(',');
                                        //number not 4, return error
                                        if (degreeItemPortData.Count() != 4)// 4 colums
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW or PIM:" + degreeStr + ".csv test data number is incorrect";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        //4 columes string
                                        string testDataStr = degreeItemPortData[0];
                                        string standardValueStr = degreeItemPortData[1];
                                        string standardSymbolStr = degreeItemPortData[2];
                                        string resultItemDegreeValStr = degreeItemPortData[3];
                                        //4 columes in db
                                        decimal testData;
                                        decimal standardValue;
                                        Symbol standardSymbol;
                                        bool resultItemDegreeVal;
                                        //convert and check 4 colums
                                        if (!Decimal.TryParse(testDataStr, out testData))
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW or PIM: can not Parse " + testDataStr + " in " + degreeStr + ".csv line " + lineNumber + " to decimal";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        if (!Decimal.TryParse(standardValueStr, out standardValue))
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW or PIM:can not Parse " + standardValueStr + " in " + degreeStr + ".csv " + lineNumber + " to decimal";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        if (standardSymbolStr == "0")//0 represent <=
                                        {
                                            standardSymbol = Symbol.LessOrEqual;
                                        }
                                        else if (standardSymbolStr == "1")//1 represent >=
                                        {
                                            standardSymbol = Symbol.GreatOrEqual;
                                        }
                                        else
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW or PIM:can not Parse " + standardSymbolStr + " in " + degreeStr + ".csv line " + lineNumber + " to ligal Compare Symbol";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        if (resultItemDegreeValStr == "0")//0 represent fail
                                        {
                                            resultItemDegreeVal = false;
                                        }
                                        else if (resultItemDegreeValStr == "1")//1 represent pass
                                        {
                                            resultItemDegreeVal = true;
                                        }
                                        else
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW or PIM:can not Parse " + standardSymbolStr + " in " + degreeStr + ".csv line " + lineNumber + " to ligal test result";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                        //update resultItemDegree with this ResultItemDegreeVal
                                        resultItemDegree = resultItemDegree && resultItemDegreeVal;
                                        //get TestStandardId
                                        var testStadardDb = unitOfWork.TestStandardRepository.Get(a => a.StandardValue == standardValue && a.Symbol == standardSymbol).SingleOrDefault();
                                        if (testStadardDb == null)
                                        {
                                            testStadardDb = new TestStandard { StandardValue = standardValue, Symbol = standardSymbol };
                                            try
                                            {
                                                unitOfWork.TestStandardRepository.Insert(testStadardDb);
                                                unitOfWork.DbSaveChanges();
                                            }
                                            catch (Exception /*e*/)
                                            {
                                                degreeCsvSr.Close();
                                                result.Message = "SW or PIM:can not ADD " + standardValueStr + ":" + standardSymbolStr + " in " + degreeStr + ".csv line " + lineNumber + " to TestStandard";
                                                return new XmlResult<SingleResultXml>() { Data = result };
                                            }
                                        }
                                        //add TestResultItemDegreeVal
                                        TestResultItemDegreeVal testResultItemDegreeVal = new TestResultItemDegreeVal
                                        {
                                            TestResultItemDegreeId = testResultItemDegree.Id,
                                            TestStandardId = testStadardDb.Id,
                                            Port = portNumber,
                                            TestData = testData,
                                            ResultItemDegreeVal = resultItemDegreeVal
                                        };
                                        try
                                        {
                                            unitOfWork.TestResultItemDegreeValRepository.Insert(testResultItemDegreeVal);
                                            unitOfWork.DbSaveChanges();
                                        }
                                        catch (Exception /*e*/)
                                        {
                                            degreeCsvSr.Close();
                                            result.Message = "SW or PIM:add TestResultItemDegreeVal failed";
                                            return new XmlResult<SingleResultXml>() { Data = result };
                                        }
                                    }
                                    //update TestResultItemDegree ResultItemDegree
                                    testResultItemDegree.ResultItemDegree = resultItemDegree;
                                    try
                                    {
                                        unitOfWork.TestResultItemDegreeRepository.Update(testResultItemDegree);
                                        unitOfWork.DbSaveChanges();
                                    }
                                    catch (Exception /*e*/)
                                    {
                                        result.Message = "SW or PIM:unable UPDATE TestResultItemDegree model's ResultItemDegree";
                                        return new XmlResult<SingleResultXml>() { Data = result };
                                    }
                                    //UPDATE current TestResultItem Model's ResultItem with current TestResultItemDegree ResultItemDegree
                                    resultItemSwOrPim = resultItemSwOrPim && testResultItemDegree.ResultItemDegree;
                                }
                                else
                                {
                                    //lineNumber > 2, set degreeItemLine to null, stop read file
                                    degreeItemLine = null;
                                }
                            }
                            while (degreeItemLine != null);
                            //close current degree csv file   
                            degreeCsvSr.Close();
                        }
                    }
                    //update TestResutItem ResulItem
                    testResultItemSwOrPimAdd.ResultItem = resultItemSwOrPim;
                    testResultDb.Result = resultItemSwOrPim && resultItemOther;
                    try
                    {
                        unitOfWork.TestResultItemRepository.Update(testResultItemSwOrPimAdd);
                        unitOfWork.DbSaveChanges();
                    }
                    catch (Exception /*e*/)
                    {
                        result.Message = "SW or PIM:unable UPDATE TestResultItem model's ResultItem";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                    try
                    {
                        unitOfWork.TestResultRepository.Update(testResultDb);
                        unitOfWork.DbSaveChanges();
                    }
                    catch (Exception /*e*/)
                    {
                        result.Message = "SW OR PIM:unable UPDATE TestResult model's Result";
                        return new XmlResult<SingleResultXml>() { Data = result };
                    }
                }
                else
                {
                    result.Message = "impossibility test manner";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                scope.Complete();
            }
            
            return new XmlResult<SingleResultXml>() { Data = result }; 
        }

        public ActionResult UploadPimFileRonsenberger()
        {
            SingleResultXml result = new SingleResultXml() { Message = "true" };
            HttpPostedFileBase file = Request.Files["file"];
            string fileFullName;
            string fileEx;
            string fileNameWithoutEx;
            string slash = "/";
            string uploadTime = DateTime.Now.ToString("yyyyMMdd");
            string uploadPath = AppDomain.CurrentDomain.BaseDirectory + "/UploadedFolder/PIM/Ronsenberger/" + uploadTime;
            string savePath;
            string saveFolderPath;

            if (file == null || file.ContentLength <= 0)
            {
                result.Message = "file can not be null";
                return new XmlResult<SingleResultXml>() { Data = result };
            }

            fileFullName = System.IO.Path.GetFileName(file.FileName);
            fileEx = System.IO.Path.GetExtension(fileFullName);
            fileNameWithoutEx = System.IO.Path.GetFileNameWithoutExtension(fileFullName);
            if (fileEx.ToLower() != ".zip")
            {
                result.Message = "incorrect file type";
                return new XmlResult<SingleResultXml>() { Data = result };
            }
            if (!System.IO.Directory.Exists(uploadPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(uploadPath);
                }
                catch (Exception /*e*/)
                {
                    result.Message = "can not create upalod directory";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
            }
            savePath = System.IO.Path.Combine(uploadPath, fileFullName);
            try
            {
                file.SaveAs(savePath);
            }
            catch (Exception /*e*/)
            {
                result.Message = "can not save uploaded file";
                return new XmlResult<SingleResultXml>() { Data = result };
            }

            ZipFile zip = ZipFile.Read(savePath, new ReadOptions { Encoding = Encoding.Default });
            try
            {
                //unzip file
                zip.AlternateEncoding = Encoding.Default;
                zip.ExtractAll(uploadPath + slash + fileNameWithoutEx, ExtractExistingFileAction.OverwriteSilently);
            }
            catch (Exception /*e*/)
            {
                result.Message = "can not unzip file";
                zip.Dispose();
                return new XmlResult<SingleResultXml>() { Data = result };
            }
            zip.Dispose();

            using (var scope = new TransactionScope())
            {
                saveFolderPath = uploadPath + slash + fileNameWithoutEx;
                //read xls File
                string serialNumber = "";
                string testImage = "";
                string excelFilePath = "";
                string txtFilePath = "";
                string pdfFilePath = "";
                string[] xlsFiles = Directory.GetFiles(saveFolderPath, "*.xls");
                if (xlsFiles.Count() != 1)
                {
                    result.Message = "The number of excel incorrect";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                else
                {
                    //get serial number, excel file without extension
                    serialNumber = System.IO.Path.GetFileNameWithoutExtension(xlsFiles[0]);
                    excelFilePath = saveFolderPath + slash + serialNumber + ".xls";
                    txtFilePath = saveFolderPath + slash + serialNumber + ".txt";
                    pdfFilePath = saveFolderPath + slash + serialNumber + ".pdf";
                }
                if (!System.IO.File.Exists(pdfFilePath))
                {
                    result.Message = "can not find " + serialNumber + ".pdf file";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }
                if (!System.IO.File.Exists(txtFilePath))
                {
                    result.Message = "can not find " + serialNumber + ".txt file";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }

                try
                {
                    PdfHandler.ExtractImagesFromPDF(pdfFilePath, saveFolderPath);
                    testImage = "/Ronsenberger/" + uploadTime + slash + fileNameWithoutEx + slash + "result.jpg";
                }
                catch (Exception /*e*/)
                {
                    result.Message = "can not generate image from " + serialNumber + ".pdf file";
                    return new XmlResult<SingleResultXml>() { Data = result };
                }

                //get TestResultPim records in DB
                var testResultPimDbs = unitOfWork.TestResultPimRepository.Get(a => a.SerialNumber == serialNumber).ToList();
                if (testResultPimDbs.Count != 0)
                {
                    foreach (var testResultPimDb in testResultPimDbs)
                    {
                        try
                        {
                            testResultPimDb.IsLatest = false;
                            unitOfWork.DbSaveChanges();
                        }
                        catch (Exception /*e*/)
                        {
                            result.Message = "update TestResultPim Old record failed";
                            return new XmlResult<SingleResultXml>() { Data = result };
                        }
                    }
                }
                //initialize new TestResultPim
                TestResultPim testResultPim = new TestResultPim();
                testResultPim.SerialNumber = serialNumber;
                testResultPim.TestImage = testImage;

                //read txt file
                StreamReader srTxt = new StreamReader(txtFilePath);
                string txtLine = string.Empty;
                string[] txtLineArr = null;
                int txtLineNumber = 0;
                while (txtLineNumber < 25)//maxLineNumber = 25
                {
                    txtLine = srTxt.ReadLine();
                    txtLineNumber++;
                    if (txtLineNumber <= 2)//first two line, TestResultPim.Carrier
                    {
                        txtLineArr = txtLine.Trim().Split(' ');
                        //SetFreq
                        string setFreqStr = txtLineArr[1];
                        decimal setFreq;
                        if (!Decimal.TryParse(setFreqStr, out setFreq))
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " SetFreq: " + setFreqStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //StartFreq
                        string startFreqStr = txtLineArr[4];
                        decimal startFreq;
                        if (!Decimal.TryParse(startFreqStr, out startFreq))
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " StartFreq: " + startFreqStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //StopFreq
                        string stopFreqStr = txtLineArr[7];
                        decimal stopFreq;
                        if (!Decimal.TryParse(stopFreqStr, out stopFreq))
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " StopFreq: " + stopFreqStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //Freq unit
                        string freqUnitStr = txtLineArr[2];
                        Unit freqUnit;
                        if (freqUnitStr.ToUpper() == "MHZ")
                        {
                            freqUnit = Unit.M;
                        }
                        else
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " Freq unit: " + freqUnitStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //Power
                        string powerStr = txtLineArr[10];
                        decimal power;
                        if (!Decimal.TryParse(powerStr, out power))
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " Power: " + powerStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //Power Unit
                        string imUnitStr = txtLineArr[11];
                        ImUnit imUnit;
                        if (imUnitStr.ToUpper() == "DBC")
                        {
                            imUnit = ImUnit.dBc;
                        }
                        else if (imUnitStr.ToUpper() == "DBM")
                        {
                            imUnit = ImUnit.dBm;
                        }
                        else
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " Power unit: " + imUnitStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        Carrier carrier = new Carrier()
                        {
                            SetFreq = setFreq,
                            StartFreq = startFreq,
                            StopFreq = stopFreq,
                            Power = power,
                            FreqUnit = freqUnit,
                            ImUnit = imUnit
                        };
                        var carrierDb = unitOfWork.CarrierRepository.Get(a => a.SetFreq == setFreq && a.StartFreq == startFreq && a.StopFreq == stopFreq && a.Power == power && a.FreqUnit == freqUnit && a.ImUnit == imUnit).SingleOrDefault();
                        if (carrierDb == null)
                        {
                            try
                            {
                                unitOfWork.CarrierRepository.Insert(carrier);
                                unitOfWork.DbSaveChanges();
                                testResultPim.Carriers.Add(carrier);
                            }
                            catch (Exception /*e*/)
                            {
                                srTxt.Close();
                                result.Message = "insert Carrier failed";
                                return new XmlResult<SingleResultXml>()
                                {
                                    Data = result
                                };
                            }
                        }
                        else
                        {
                            testResultPim.Carriers.Add(carrierDb);
                        }
                    }
                    else if (txtLineNumber == 3)//ImOrder, TestResultPim.TestMeas in line 3
                    {
                        txtLineArr = txtLine.Trim().Split(' ');
                        //ImOrder.OrderNumber
                        string orderNumberStr = txtLineArr[1];
                        int orderNumber;
                        if (!int.TryParse(orderNumberStr.Substring(orderNumberStr.Length - 1), out orderNumber))
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " " + orderNumberStr + " parse failed";
                            return new XmlResult<SingleResultXml>() { Data = result };
                        }
                        //ImOrder.ImUnit
                        string imUnitStr = txtLineArr[3];
                        ImUnit imUnit;
                        if (imUnitStr.ToUpper() == "DBC")
                        {
                            imUnit = ImUnit.dBc;
                        }
                        else if (imUnitStr.ToUpper() == "DBM")
                        {
                            imUnit = ImUnit.dBm;
                        }
                        else
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " Power unit: " + imUnitStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //set TestResultPim.TestMeans
                        string testMeansStr = txtLineArr[4];
                        TestMeans testMeans;
                        if (testMeansStr.ToUpper() == "SWEEP")
                        {
                            testMeans = TestMeans.Sweep;
                        }
                        else if (testMeansStr.ToUpper() == "SINGLE")
                        {
                            testMeans = TestMeans.Single;
                        }
                        else
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + "  Meas: " + testMeansStr + " parse failed";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //set TestResultPim.TestMeans
                        testResultPim.TestMeans = testMeans;
                        //set TestResultPim.ImOrderId
                        ImOrder imOrder = new ImOrder() { OrderNumber = orderNumber, ImUnit = imUnit };
                        //get ImOrder from db
                        var imOrderDb = unitOfWork.ImOrderRepository.Get(a => a.OrderNumber == orderNumber && a.ImUnit == imUnit).SingleOrDefault();
                        if (imOrderDb == null)
                        {
                            try
                            {
                                unitOfWork.ImOrderRepository.Insert(imOrder);
                                unitOfWork.DbSaveChanges();
                                testResultPim.ImOrderId = imOrder.Id;
                            }
                            catch (Exception /*e*/)
                            {
                                srTxt.Close();
                                result.Message = "line " + txtLineNumber + ", Insert IMOrder failed";
                                return new XmlResult<SingleResultXml>()
                                {
                                    Data = result
                                };
                            }
                        }
                        else
                        {
                            testResultPim.ImOrderId = imOrderDb.Id;
                        }
                    }
                    else if (txtLineNumber == 4)//TestEquipment in line 4
                    {
                        txtLineArr = txtLine.Trim().Split(' ');
                        string name = txtLineArr[2];
                        string serailNumber = txtLineArr[txtLineArr.Count() - 1];
                        //set TestResultPim.TestEquipmentId
                        TestEquipment testEquipment = new TestEquipment() { Name = name.Substring(0, name.Length - 1), SerialNumber = serailNumber, isVna = false, IsDelete = false };
                        var testEquipmentDb = unitOfWork.TestEquipmentRepository.Get(a => a.Name == name.Substring(0, name.Length - 1) && a.SerialNumber == serailNumber).FirstOrDefault();
                        if (testEquipmentDb == null)
                        {
                            try
                            {
                                unitOfWork.TestEquipmentRepository.Insert(testEquipment);
                                unitOfWork.DbSaveChanges();
                                testResultPim.TestEquipmentId = testEquipment.Id;
                            }
                            catch (Exception /*e*/)
                            {
                                srTxt.Close();
                                result.Message = "line " + txtLineNumber + ", Insert TestEquipment failed";
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                        }
                        else
                        {
                            testResultPim.TestEquipmentId = testEquipmentDb.Id;
                        }
                    }
                    else if (txtLineNumber == 5)//TestTime in line 5
                    {
                        string testTimeStr = txtLine.Trim().Replace(":", "") + DateTime.Now.ToString("ss");//add ss to Test Time string 
                        DateTime testTime;
                        if (!DateTime.TryParseExact(testTimeStr, "yyyyMMdd HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTime))
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " Test Time : " + testTimeStr + " parse failed";
                            return new XmlResult<SingleResultXml>() { Data = result };
                        }
                        //set TestResultPim.TestTime
                        testResultPim.TestTime = testTime;
                    }
                    else if (txtLineNumber == 23)//TestResultPim.LimitLine
                    {
                        txtLineArr = txtLine.Trim().Split(' ');
                        string limitLineStr = txtLineArr[txtLineArr.Count() - 1];
                        decimal limitLine;
                        if (!Decimal.TryParse(limitLineStr, out limitLine))
                        {
                            srTxt.Close();
                            result.Message = "line " + txtLineNumber + " Limitline: " + limitLineStr + " parse failed";
                            return new XmlResult<SingleResultXml>() { Data = result };
                        }
                        //set TestResultPim.LimitLine
                        testResultPim.LimitLine = limitLine;
                    }
                    else
                    {
                        //other line, useless ignore
                    }
                }
                //close txt file
                srTxt.Close();

                //read excel file
                HSSFWorkbook workbook = new HSSFWorkbook();
                using (FileStream fsExcelFile = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
                {
                    workbook = new HSSFWorkbook(fsExcelFile);
                }
                ISheet sheet = workbook.GetSheetAt(0);
                List<TestResultPimPoint> testResultPimPoints = new List<TestResultPimPoint> { };
                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    if (row == 0)
                    {
                        //skip line 1, title line
                    }
                    else
                    {
                        if (sheet.GetRow(row) != null)
                        {
                            string carrierOneFreqStr = sheet.GetRow(row).GetCell(0).StringCellValue;
                            decimal carrierOneFreq;
                            if (!Decimal.TryParse(carrierOneFreqStr, out carrierOneFreq))
                            {
                                result.Message = "excel line " + (row + 1) + " Carrier 1 Freq " + carrierOneFreqStr + " parse failed";
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                            string carrierTwoFreqStr = sheet.GetRow(row).GetCell(1).StringCellValue;
                            decimal carrierTwoFreq;
                            if (!Decimal.TryParse(carrierTwoFreqStr, out carrierTwoFreq))
                            {
                                result.Message = "excel line " + (row + 1) + " Carrier 2 Freq " + carrierOneFreqStr + " parse failed";
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                            string imFreqStr = sheet.GetRow(row).GetCell(6).StringCellValue;
                            decimal imFreq;
                            if (!Decimal.TryParse(imFreqStr, out imFreq))
                            {
                                result.Message = "excel line " + (row + 1) + " IM Freq " + imFreqStr + " parse failed";
                                return new XmlResult<SingleResultXml>() { Data = result };
                            }
                            string imPowerStr = sheet.GetRow(row).GetCell(7).StringCellValue;
                            decimal imPower;
                            if (!Decimal.TryParse(imPowerStr, out imPower))
                            {
                                result.Message = "excel line " + (row + 1) + " IM Power " + imFreqStr + " parse failed";
                                return new XmlResult<SingleResultXml>()
                                {
                                    Data = result
                                };
                            }
                            TestResultPimPoint testResultPimPoint = new TestResultPimPoint
                            {
                                CarrierOneFreq = carrierOneFreq,
                                CarrierTwoFreq = carrierTwoFreq,
                                ImFreq = imFreq,
                                ImPower = imPower
                            };
                            testResultPimPoints.Add(testResultPimPoint);
                        }
                    }
                }

                var maxImPower = testResultPimPoints.Max(a => a.ImPower);
                var maxImPowerPoint = testResultPimPoints.Where(a => a.ImPower == maxImPower).First();
                maxImPowerPoint.isWorst = true;
                //set TestResultPim.TestResultPimPoints
                testResultPim.TestResultPimPoints = testResultPimPoints;
                //set TestResultPim.TestResult
                if (maxImPower < testResultPim.LimitLine)
                {
                    testResultPim.TestResult = true;
                }

                try
                {
                    unitOfWork.TestResultPimRepository.Insert(testResultPim);
                    unitOfWork.DbSaveChanges();
                }
                catch (Exception /*e*/)
                {
                    result.Message = "Insert TestResultPim failed";
                    return new XmlResult<SingleResultXml>()
                    {
                        Data = result
                    };
                }
                scope.Complete();
            }

            return new XmlResult<SingleResultXml>() { Data = result };
        }
	}
}