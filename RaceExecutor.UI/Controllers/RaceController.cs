using Excel;
using LinqToExcel;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Formula.Functions;
using RaceExecutor.UI.Externals.Helper;
using RaceExecutor.UI.Models;
using RaceExecutorModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace RaceExecutor.UI.Controllers
{
    public class RaceController : Controller
    {
        TrailRaceNepalEntities db = new TrailRaceNepalEntities();
        //
        // GET: /TrailRace/
        public ActionResult Registered()
        {
            var RaceId = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var data = db.Registers.Where(x => x.RaceId == RaceId).ToList();
            return View(data);
        }
        [HttpPost]
        public ActionResult AddRunners(FormCollection fc)
        {
            var BIBCode = fc["BIBCode"];
            var StartTime = DateTime.Parse(fc["StartTime"]);
            var data = new Register();
            data.BIBCode = Convert.ToInt32(BIBCode);
            data.StartTime = StartTime;
            db.Registers.Add(data);
            db.SaveChanges();
            return RedirectToAction("Create", "Race");
        }

        public ActionResult Index()
        {
            var Object = new TrailRace();
            var RaceId = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var registerdata = db.Registers.Where(x => x.RaceId == RaceId).ToList();
            var intervaldata = db.TimeIntervals.Where(x => x.RaceId == RaceId).ToList();
            var data = (from a in registerdata
                        join c in intervaldata on a.BIBCode equals c.BIBCode

                        //To Find the list of current Date
                        //where (a.StartTime.Value.Year == DateTime.Now.Year)
                        //&& (a.StartTime.Value.Month == DateTime.Now.Month)
                        //&& (a.StartTime.Value.Day == DateTime.Now.Day)
                        select new UltraTrailRaceViewModel()
                        {
                            Id = c.Id,
                            BIBCode = c.BIBCode,
                            Name = a.Name,
                            DateOfBirth = a.DateOfBirth,
                            Gender = a.Gender,
                            Email = a.Email,
                            PhoneNumber = a.PhoneNumber,
                            Distance = a.Distance,
                            StartTime = c.StartTime,
                            EndTime = c.EndTime,
                            Interval = c.Interval,
                            RaceId = c.RaceId
                        }).Distinct().ToList();
            //var rank = data.GroupBy(d => new { d.Distance, d.StartTime.Value.Date, d.Age, d.Gender })

            var rank = data.GroupBy(d => new { d.Distance, d.StartTime.Value.Date, d.Gender })
               .SelectMany(g => g.OrderBy(y => y.Interval)
                                 .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
            var list = new List<UltraTrailRaceViewModel>();

            foreach (var item in rank)
            {
                var a = new UltraTrailRaceViewModel()
                {
                    Id = item.Item.Id,
                    BIBCode = item.Item.BIBCode,
                    Name = item.Item.Name,
                    DateOfBirth = item.Item.DateOfBirth,
                    Gender = item.Item.Gender,
                    Email = item.Item.Email,
                    PhoneNumber = item.Item.PhoneNumber,
                    Distance = item.Item.Distance,
                    StartTime = item.Item.StartTime,
                    EndTime = item.Item.EndTime,
                    Interval = item.Item.Interval,
                    Rank = item.Rank
                };
                list.Add(a);
            }

            Object.DisplayList = list;
            ViewBag.model = new CategoryWiseRankingViewModel();
            return View(Object);
        }

        public ActionResult Create()
        {
            var date = DateTime.Now.ToLocalTime();
            var time = new UltraTrailRaceViewModel();
            var allsupportuser = db.Registers.Where(x => x.StartTime.Value.Year == DateTime.Now.Year && x.StartTime.Value.Month == DateTime.Now.Month)
                .GroupBy(x => x.StartTime).ToList();

            List<SelectListItem> startlist = new List<SelectListItem>();
            bool IdSelected = false;
            startlist.Add(new SelectListItem { Text = "......Select start-time......", Value = "" });
            foreach (var item in allsupportuser)
            {

                startlist.Add(new SelectListItem
                {
                    Text = item.Key.ToString(),
                    Value = item.Key.ToString(),
                    Selected = IdSelected
                });

            }
            time.listStartTime = startlist;
            return View(time);
        }

        [HttpPost]
        public object Create(UltraTrailRaceViewModel viewmodelobj)
        {
            var BIBCodeCheck = db.Registers.Where(x => x.BIBCode == viewmodelobj.BIBCode).ToList();
            var raceid = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var registeredData = db.Registers.Where(x => x.RaceId == raceid).ToList();
            ReturnMessageModel result = new ReturnMessageModel();
            ObjectParameter returnMessage = new ObjectParameter("ReturnMessage", typeof(String));
            foreach (var item in registeredData)
            {

                if (item.BIBCode == viewmodelobj.BIBCode)
                {
                    var time = new TimeInterval()
                    {
                        BIBCode = viewmodelobj.BIBCode,
                        StartTime = item.StartTime,
                        EndTime = DateTime.UtcNow.AddHours(5).AddMinutes(45),
                        Interval = DateTime.UtcNow.AddHours(5).AddMinutes(45) - item.StartTime,
                        RaceId = raceid
                    };
                    db.TimeIntervals.Add(time);
                    db.SaveChanges();
                    //var time = new TimeInterval()
                    //{
                    //    BIBCode = viewmodelobj.BIBCode,
                    //    StartTime = item.StartTime,
                    //    EndTime = DateTime.UtcNow.AddHours(5).AddMinutes(45),
                    //    Interval = DateTime.UtcNow.AddHours(5).AddMinutes(45) - item.StartTime,
                    //    RaceId = raceid
                    //};
                    //db.SpTimeIntervalIns(raceid, time.StartTime, time.EndTime, time.Interval.GetValueOrDefault(), time.BIBCode, returnMessage);

                }
            }
            return result.ReturnMessage;
        }
        //[HttpPost]
        //public ActionResult Create(UltraTrailRaceViewModel viewmodelobj)
        //{
        //    var BIBCodeCheck = db.Registers.Where(x => x.BIBCode == viewmodelobj.BIBCode).ToList();
        //    var raceid = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
        //    var registeredData = db.Registers.Where(x => x.RaceId == raceid).ToList();
        //    if (BIBCodeCheck == null || BIBCodeCheck.Count == 0)
        //    {
        //        var reg = new Register();
        //        reg.BIBCode = viewmodelobj.BIBCode;
        //        db.Registers.Add(reg);
        //        db.SaveChanges();
        //        var time = new TimeInterval()
        //        {
        //            BIBCode = viewmodelobj.BIBCode,
        //            StartTime = (from a in db.Races where a.Id == raceid select a.CreateDate).FirstOrDefault(),
        //            EndTime = DateTime.UtcNow.AddHours(5).AddMinutes(45),
        //            Interval = DateTime.UtcNow.AddHours(5).AddMinutes(45) - (from a in db.Races where a.Id == raceid select a.CreateDate).FirstOrDefault(),
        //            RaceId = raceid
        //        };
        //        db.TimeIntervals.Add(time);
        //        db.SaveChanges();
        //        TempData["Success"] = "BIB Submitted Successfully!";
        //    }
        //    else
        //    {
        //        foreach (var item in registeredData)
        //        {

        //            if (item.BIBCode == viewmodelobj.BIBCode)
        //            {
        //                /*var day = (DateTime.UtcNow.AddHours(5).AddMinutes(45).Day - item.StartTime.Value.Day);
        //                var hours = (DateTime.UtcNow.AddHours(5).AddMinutes(45).TimeOfDay - item.StartTime.Value.TimeOfDay);
        //                var testdata = new TestTimeInterval()
        //                {
        //                    StartTime = item.StartTime,
        //                    EndTime = DateTime.UtcNow.AddHours(5).AddMinutes(45),
        //                    Interval = ((hours.Hours + day * 24) + ":" + hours.Minutes + ":" +hours.Seconds).ToString()
        //                };
        //                db.TestTimeIntervals.Add(testdata);
        //                db.SaveChanges();*/
        //                var day = (DateTime.UtcNow.AddHours(5).AddMinutes(45).Day - item.StartTime.Value.Day);
        //                var hours = (DateTime.UtcNow.AddHours(5).AddMinutes(45).TimeOfDay - item.StartTime.Value.TimeOfDay);
        //                var time = new TimeInterval()
        //                {
        //                    BIBCode = viewmodelobj.BIBCode,
        //                    StartTime = item.StartTime,
        //                    EndTime = DateTime.UtcNow.AddHours(5).AddMinutes(45),
        //                    // Interval = ((hours.Hours + day * 24) + ":" + hours.Minutes + ":" + hours.Seconds).ToString(),
        //                    Interval = DateTime.UtcNow.AddHours(5).AddMinutes(45) - item.StartTime,
        //                    RaceId = raceid
        //                };
        //                db.TimeIntervals.Add(time);
        //                db.SaveChanges();
        //                TempData["Success"] = "BIB Added and Checked Submitted Successfully!";

        //                //Email Sending Part

        //                //var value = db.Registers.ToList();
        //                //var email = value.Where(x => x.BIBCode == viewmodelobj.BIBCode).Select(y => y.Email).FirstOrDefault();
        //                //DateTime Date = DateTime.UtcNow.AddHours(5).AddMinutes(45);
        //                //ViewBag.Name = "Jimi Oostrum and Mahesh Dahal";
        //                //ViewBag.Runnername = value.Where(x => x.BIBCode == viewmodelobj.BIBCode).Select(y => y.Name).FirstOrDefault();
        //                //ViewBag.StartTime = value.Where(x => x.BIBCode == viewmodelobj.BIBCode).Select(y => y.StartTime).FirstOrDefault();
        //                //ViewBag.EndTime = data.EndTime;
        //                //ViewBag.Age = value.Where(x => x.BIBCode == viewmodelobj.BIBCode).Select(y => y.Age).FirstOrDefault();
        //                //ViewBag.interval = time.Interval;
        //                //ViewBag.Gender = value.Where(x => x.BIBCode == viewmodelobj.BIBCode).Select(y => y.Gender).FirstOrDefault();
        //                //ViewBag.Distance = value.Where(x => x.BIBCode == viewmodelobj.BIBCode).Select(y => y.Distance).FirstOrDefault();
        //                //ViewBag.Date = Date.ToShortDateString();
        //                //string messageto = email;
        //                //var subject = "Timings from Trail Running Nepal";
        //                //// var Heading = "Trail Running Nepal";
        //                //var body = PartialViewForEmail("PartialViewForEmail", viewmodelobj);
        //                //using (var message = new MailMessage())
        //                //{
        //                //    message.From = new MailAddress("sapana.chaudhary@incessantrain.com");
        //                //    message.To.Add(messageto);
        //                //    message.CC.Add("sapana.chaudhary@incessantrain.com");
        //                //    message.Subject = subject;
        //                //    message.Body = body;
        //                //    message.IsBodyHtml = true;
        //                //    try
        //                //    {
        //                //        SmtpClient smtp = new SmtpClient();
        //                //        smtp.Send(message);
        //                //        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
        //                //    }
        //                //    catch (Exception ex)
        //                //    {
        //                //        Response.Write("Could not send the e-mail - error: " + ex.Message);
        //                //        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
        //                //    }
        //                //}
        //                return View();
        //            }

        //        }

        //    }
        //    return View();
        //}

        //partial page for email send
        public string PartialViewForEmail(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
        //Rank Runner categry wise
        public ActionResult RankCategoryWiseForm()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RankCategoryWiseFormExcel(FormCollection fc)
        {
            var raceid = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var listdata = new TrailRace();
            int year;
            int month;
            int day;
            var list = new List<UltraTrailRaceViewModel>();
            var selecteddate = DateTime.Parse(fc["Date"]);
            year = selecteddate.Year;
            month = selecteddate.Month;
            day = selecteddate.Day;

            var category = fc["Category"];
            var gender = fc["Gender"];
            var distance = fc["Distance"];
            var DIS = Int32.Parse(distance);
            if (category != "0" && gender != "0" && DIS != 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            // && (a.Gender.Contains(gender)) && (a.Age.Contains(category)))
                            // where ((a.Gender.ToLower() == gender.ToLower()) && (a.Distance == DIS) &&
                            where ((a.Gender.ToLower() == gender.ToLower()) && (a.Age == category) && (a.Distance == DIS) && (c.RaceId == raceid) &&
                            ((a.StartTime.Value.Month == month)
                            && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)))
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                                Nationaity = a.Nationality,
                                SeasonPass = a.SeasonPass,
                                TeamName = a.TeamName
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank,
                        Nationaity = item.Item.Nationaity,
                        SeasonPass = item.Item.SeasonPass,
                        TeamName = item.Item.TeamName
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;
            }

            if (category == "0" && gender == "0" && DIS != 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            // && (a.Gender.Contains(gender)) && (a.Age.Contains(category)))
                            // where ((a.Gender.ToLower() == gender.ToLower()) && (a.Distance == DIS) &&
                            where ((a.Distance == DIS) && c.RaceId == raceid && ((a.StartTime.Value.Month == month)
                            && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)))
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                                Nationaity = a.Nationality,
                                SeasonPass = a.SeasonPass,
                                TeamName = a.TeamName
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank,
                        Nationaity = item.Item.Nationaity,
                        SeasonPass = item.Item.SeasonPass,
                        TeamName = item.Item.TeamName
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;

            }

            if (DIS == 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            where (((a.StartTime.Value.Month == month)
                             && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)) && a.Distance == null && c.RaceId == raceid)
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                                Nationaity = a.Nationality,
                                SeasonPass = a.SeasonPass,
                                TeamName = a.TeamName
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank,
                        Nationaity = item.Item.Nationaity,
                        SeasonPass = item.Item.SeasonPass,
                        TeamName = item.Item.TeamName
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;


            }

            if (category == "0" && (gender != null || gender != "0") && DIS != 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            // && (a.Gender.Contains(gender)) && (a.Age.Contains(category)))
                            // where ((a.Gender.ToLower() == gender.ToLower()) && (a.Distance == DIS) &&
                            where ((a.Distance == DIS) && (c.RaceId == raceid) && (a.Gender.ToLower() == gender.ToLower()) && ((a.StartTime.Value.Month == month)
                            && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)))
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                                Nationaity = a.Nationality,
                                SeasonPass = a.SeasonPass,
                                TeamName = a.TeamName
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank,
                        Nationaity = item.Item.Nationaity,
                        SeasonPass = item.Item.SeasonPass,
                        TeamName = item.Item.TeamName
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;
            }

            var Object = listdata.DisplayList.Distinct();

            //Install-Package NPOI -Version 2.3.0
            /***********************************************************************************/
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet();
            //(Optional) set the width of the columns
            sheet.SetColumnWidth(0, 5 * 256);
            sheet.SetColumnWidth(1, 30 * 256);
            sheet.SetColumnWidth(2, 20 * 256);
            sheet.SetColumnWidth(3, 20 * 256);
            sheet.SetColumnWidth(4, 20 * 256);
            //sheet.SetColumnWidth(5, 16 * 256);
            sheet.SetColumnWidth(5, 20 * 256);
            sheet.SetColumnWidth(6, 20 * 256);
            sheet.SetColumnWidth(7, 10 * 256);
            sheet.SetColumnWidth(8, 30 * 256);
            sheet.SetColumnWidth(9, 30 * 256);
            sheet.SetColumnWidth(10, 20 * 256);
            sheet.SetColumnWidth(11, 20 * 256);
            sheet.SetColumnWidth(12, 20 * 256);

            sheet.DefaultRowHeightInPoints = 22;
            sheet.SetZoom(8, 8);
            sheet.FitToPage = true;

            //sheet.AutoSizeColumn(40, true);
            var headerfont = (HSSFFont)workbook.CreateFont();
            headerfont.FontHeightInPoints = 11;
            headerfont.FontName = "Times New Roman";
            headerfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            var headerCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
            headerCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

            headerCellStyle.FillForegroundColor = HSSFColor.LightOrange.Index;
            headerCellStyle.SetFont(headerfont);
            headerCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Medium;

            var bodyfont = (HSSFFont)workbook.CreateFont();
            bodyfont.FontHeightInPoints = 10;
            bodyfont.FontName = "Times New Roman";
            bodyfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            var bodyCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
            bodyCellStyle.FillForegroundColor = HSSFColor.LightOrange.Index;
            bodyCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            bodyCellStyle.SetFont(bodyfont);
            bodyCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            bodyCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            bodyCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;

            //sample for border
            var titlefont = (HSSFFont)workbook.CreateFont();
            titlefont.FontHeightInPoints = 10;
            titlefont.FontName = "Times New Roman";
            titlefont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            var cellarrow = (HSSFCellStyle)workbook.CreateCellStyle();
            cellarrow.FillForegroundColor = HSSFColor.LightOrange.Index;
            cellarrow.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            cellarrow.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            cellarrow.SetFont(bodyfont);

            var headerRowMain = sheet.CreateRow(0);
            var dataCellHead = headerRowMain.CreateCell(0);
            dataCellHead.CellStyle = cellarrow;
            //dataCellHead.CellStyle = headerCellStyle;
            dataCellHead.SetCellValue("Trail Running runner's List" + String.Concat("     ") + DateTime.Now.ToLocalTime());


            var cra = new NPOI.SS.Util.CellRangeAddress(0, 2, 0, 12);
            sheet.AddMergedRegion(cra);
            sheet.CreateDrawingPatriarch();

            var headerRow = sheet.CreateRow(3);
            var dataCell = headerRow.CreateCell(1);
            dataCell.CellStyle = headerCellStyle;
            dataCell.SetCellValue("SN");

            string[] rowArr = {"Rank", "Name", "BIBCode","Interval", "Gender","StartTime","EndTime",
                               "Distance", "Category","Date Of Birth","Nationality","Team Name","Season Pass"};
            // string[] rowArr = {"SN", "Name", "BIBCode", "Gender","DateOfBirth",
            //                   "StartTime", "EndTime","Interval","Distance","Rank"};

            int j = 0;
            foreach (var item in rowArr)
            {
                dataCell = headerRow.CreateCell(j);
                dataCell.CellStyle = headerCellStyle;
                dataCell.SetCellValue(item);
                j++;
            }


            if (Object.Count() != 0)
            {
                var ObjectCount = Object.FirstOrDefault().GetType().GetProperties().Count();//+1 to add S.N. on excel sheet
                string[,] testArr = new string[(listdata.DisplayList.Count) + 12, ObjectCount];
                for (int arr1 = 0; arr1 < (listdata.DisplayList.Count); arr1++)
                {
                    // testArr[arr1, 0] = String.Concat((arr1 + 1).ToString(), ".");
                    testArr[arr1, 0] = listdata.DisplayList[arr1].Rank.ToString();
                    testArr[arr1, 1] = listdata.DisplayList[arr1].Name;
                    testArr[arr1, 2] = listdata.DisplayList[arr1].BIBCode.ToString();
                    testArr[arr1, 3] = listdata.DisplayList[arr1].Interval.ToString();
                    testArr[arr1, 4] = listdata.DisplayList[arr1].Gender.ToString();
                    testArr[arr1, 5] = listdata.DisplayList[arr1].StartTime.Value.ToLongTimeString();
                    testArr[arr1, 6] = listdata.DisplayList[arr1].EndTime.Value.ToLongTimeString();
                    testArr[arr1, 7] = listdata.DisplayList[arr1].Distance.ToString();
                    testArr[arr1, 8] = listdata.DisplayList[arr1].Age.ToString();
                    testArr[arr1, 9] = listdata.DisplayList[arr1].DateOfBirth.ToString();
                    testArr[arr1, 10] = listdata.DisplayList[arr1].Nationaity.ToString();
                    testArr[arr1, 11] = listdata.DisplayList[arr1].TeamName.ToString();
                    testArr[arr1, 12] = listdata.DisplayList[arr1].SeasonPass.ToString();

                }

                // Get the upper bound.
                var rowPosition = 4;
                for (int inc1 = 0; inc1 < testArr.GetUpperBound(0); inc1++)
                {
                    var contentRow = sheet.CreateRow(rowPosition);
                    for (int inc2 = 0; inc2 < (ObjectCount - 4); inc2++)
                    {
                        string s1 = testArr[inc1, inc2];
                        dataCell = contentRow.CreateCell(inc2);
                        dataCell.CellStyle = bodyCellStyle;
                        dataCell.SetCellValue(s1);
                    }
                    rowPosition++;
                }

                sheet.CreateFreezePane(0, 1, 0, 1);
                var output = new MemoryStream();
                workbook.Write(output);
                return File(
                    output.ToArray(),
                   "application/vnd.ms-excel",
                    distance + "_" + category + ".xls");
            }

            else
            {
                return RedirectToAction("RankCategoryWiseForm");
            }

        }


        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CategoryListView(FormCollection fc)
        {
            var raceid = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var listdata = new TrailRace();
            int year;
            int month;
            int day;
            var list = new List<UltraTrailRaceViewModel>();
            var selecteddate = DateTime.Parse(fc["Date"]);
            year = selecteddate.Year;
            month = selecteddate.Month;
            day = selecteddate.Day;

            var category = fc["Category"];
            var gender = fc["Gender"];
            var distance = fc["Distance"];
            var DIS = Int32.Parse(distance);
            if (category != "0" && gender != "0" && DIS != 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            // && (a.Gender.Contains(gender)) && (a.Age.Contains(category)))
                            // where ((a.Gender.ToLower() == gender.ToLower()) && (a.Distance == DIS) &&
                            where ((a.Gender.ToLower() == gender.ToLower()) && (a.Age == category) && (a.Distance == DIS) && (c.RaceId == raceid) &&
                            ((a.StartTime.Value.Month == month)
                            && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)))
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;
            }

            if (category == "0" && gender == "0" && DIS != 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            // && (a.Gender.Contains(gender)) && (a.Age.Contains(category)))
                            // where ((a.Gender.ToLower() == gender.ToLower()) && (a.Distance == DIS) &&
                            where ((a.Distance == DIS) && (c.RaceId == raceid) && ((a.StartTime.Value.Month == month)
                            && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)))
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;

            }
            if (category == "0" && (gender != null || gender != "0") && DIS != 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            // && (a.Gender.Contains(gender)) && (a.Age.Contains(category)))
                            // where ((a.Gender.ToLower() == gender.ToLower()) && (a.Distance == DIS) &&
                            where ((a.Distance == DIS) && (c.RaceId == raceid) && (a.Gender.ToLower() == gender.ToLower()) && ((a.StartTime.Value.Month == month)
                            && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)))
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;
            }

            if (DIS == 0)
            {
                var data = (from a in db.Registers
                            join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                            where (((a.StartTime.Value.Month == month)
                             && (a.StartTime.Value.Year == year) && (a.StartTime.Value.Day == day)) && a.Distance == null && (c.RaceId == raceid))
                            select new UltraTrailRaceViewModel()
                            {
                                BIBCode = c.BIBCode,
                                Name = a.Name,
                                DateOfBirth = a.DateOfBirth,
                                Gender = a.Gender,
                                Email = a.Email,
                                Age = a.Age,
                                PhoneNumber = a.PhoneNumber,
                                Distance = a.Distance,
                                StartTime = c.StartTime,
                                EndTime = c.EndTime,
                                Interval = c.Interval,
                            }).ToList();
                var rank = data.GroupBy(d => d.Distance)
                   .SelectMany(g => g.OrderBy(y => y.Interval)
                                     .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
                foreach (var item in rank)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        BIBCode = item.Item.BIBCode,
                        Name = item.Item.Name,
                        Age = item.Item.Age,
                        DateOfBirth = item.Item.DateOfBirth,
                        Gender = item.Item.Gender,
                        Email = item.Item.Email,
                        PhoneNumber = item.Item.PhoneNumber,
                        Distance = item.Item.Distance,
                        StartTime = item.Item.StartTime,
                        EndTime = item.Item.EndTime,
                        Interval = item.Item.Interval,
                        Rank = item.Rank
                    };
                    list.Add(a);
                }
                listdata.DisplayList = list;


            }

            var Object = listdata.DisplayList;
            return View(listdata);
        }

        public ActionResult DateWiseListFilter()
        {
            return PartialView();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DateWiseListFilterShowingView(FormCollection fc)
        {

            var selecteddate = DateTime.Parse(fc["Date"]);
            var Object = new TrailRace();
            var data = (from a in db.Registers
                        join c in db.TimeIntervals on a.BIBCode equals c.BIBCode
                        //To Find the list of current Date
                        where (a.StartTime.Value.Year == selecteddate.Year)
                        && (a.StartTime.Value.Month == selecteddate.Month)
                        && (a.StartTime.Value.Day == selecteddate.Day)
                        select new UltraTrailRaceViewModel()
                        {
                            Id = c.Id,
                            BIBCode = c.BIBCode,
                            Name = a.Name,
                            DateOfBirth = a.DateOfBirth,
                            Gender = a.Gender,
                            Email = a.Email,
                            PhoneNumber = a.PhoneNumber,
                            Distance = a.Distance,
                            StartTime = c.StartTime,
                            EndTime = c.EndTime,
                            Interval = c.Interval,
                        }).ToList();

            //, d.StartTime.Value.Date 
            var rank = data.GroupBy(d => new { d.Distance })
               .SelectMany(g => g.OrderBy(y => y.Interval)
                                 .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }));
            var list = new List<UltraTrailRaceViewModel>();

            foreach (var item in rank)
            {
                var a = new UltraTrailRaceViewModel()
                {
                    Id = item.Item.Id,
                    BIBCode = item.Item.BIBCode,
                    Name = item.Item.Name,
                    DateOfBirth = item.Item.DateOfBirth,
                    Gender = item.Item.Gender,
                    Email = item.Item.Email,
                    PhoneNumber = item.Item.PhoneNumber,
                    Distance = item.Item.Distance,
                    StartTime = item.Item.StartTime,
                    EndTime = item.Item.EndTime,
                    Interval = item.Item.Interval,
                    Rank = item.Rank
                };
                list.Add(a);
            }

            Object.DisplayList = list;
            return PartialView(Object);
        }

        public ActionResult AddRace()
        {
            return View();
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddRace(Race race)
        {
            if (race != null)
            {
                db.Races.Add(race);
                db.SaveChanges();
            }
            return View();
        }
        public ActionResult ExportSample()
        {

            //    /**********************************************************************************/
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet();
            sheet.SetColumnWidth(0, 10 * 256);
            sheet.SetColumnWidth(1, 30 * 256); //bib
            sheet.SetColumnWidth(2, 30 * 256);  // end time
            sheet.SetColumnWidth(3, 30 * 256);  // check post name
            sheet.DefaultRowHeightInPoints = 22;
            sheet.SetZoom(8, 8);
            sheet.FitToPage = true;
            //sheet.AutoSizeColumn(40, true);
            var headerfont = (HSSFFont)workbook.CreateFont();
            headerfont.FontHeightInPoints = 11;
            headerfont.FontName = "Arial";
            headerfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            var headerCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
            headerCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

            headerCellStyle.FillForegroundColor = HSSFColor.LightOrange.Index;
            headerCellStyle.SetFont(headerfont);
            headerCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Medium;

            var bodyfont = (HSSFFont)workbook.CreateFont();
            bodyfont.FontHeightInPoints = 10;
            bodyfont.FontName = "Arial";
            bodyfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            var bodyCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
            bodyCellStyle.FillForegroundColor = HSSFColor.LightOrange.Index;
            bodyCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            bodyCellStyle.SetFont(bodyfont);
            bodyCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            bodyCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            bodyCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;

            //sample for border
            var titlefont = (HSSFFont)workbook.CreateFont();
            titlefont.FontHeightInPoints = 10;
            titlefont.FontName = "Arial";
            titlefont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            var cellarrow = (HSSFCellStyle)workbook.CreateCellStyle();
            cellarrow.FillForegroundColor = HSSFColor.LightOrange.Index;
            cellarrow.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            cellarrow.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            cellarrow.SetFont(bodyfont);

            var headerRow = sheet.CreateRow(0);
            var dataCell = headerRow.CreateCell(0);
            dataCell.CellStyle = headerCellStyle;
            //  dataCell.SetCellValue("SN");


            string[] rowArr = { "SN", "BIBCode", "EndTime", "Check-Post Name" };


            int j = 0;
            foreach (var item in rowArr)
            {
                dataCell = headerRow.CreateCell(j);
                dataCell.CellStyle = headerCellStyle;
                dataCell.SetCellValue(item);
                j++;
            }

            string[,] ArrayData = new string[4, 4] { { "1", "201", "2018-10-02 13:13:51.000", "Irkhu" }, { "2", "202", "2018-10-02 13:13:51.000", "birkhu" }, { "3", "203", "2018-10-02 13:13:51.000", "sirkhu" }, { "4", "204", "2018-10-02 13:13:51.000", "mirku" } };


            var rowPosition = 1;
            for (int inc1 = 0; inc1 <= ArrayData.GetUpperBound(0); inc1++)
            {
                var contentRow = sheet.CreateRow(rowPosition);
                for (int inc2 = 0; inc2 < 4; inc2++)
                {
                    string s1 = ArrayData[inc1, inc2];
                    dataCell = contentRow.CreateCell(inc2);
                    dataCell.CellStyle = bodyCellStyle;
                    dataCell.SetCellValue(s1);
                }
                rowPosition++;
            }
            sheet.CreateFreezePane(0, 1, 0, 1);
            var output = new MemoryStream();
            workbook.Write(output);
            return File(
                        output.ToArray(),
                       "application/vnd.ms-excel",
                         "CheckPostList" + ".xls");
        }
        [HttpPost]
        public ActionResult ImportExcelCheckPointData(FormCollection fc, CategoryWiseRankingViewModel cat, HttpPostedFileBase file)
        {

            AdoHelper ado = new AdoHelper();
            DataTable dt;
            DataSet result;
            int rowCount = 0;
            IExcelDataReader reader = null;
            if (cat.FileName != null && cat.FileName.ContentLength > 0)
            {
                // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                // to get started. This is how we avoid dependencies on ACE or Interop:
                Stream stream = cat.FileName.InputStream;

                if (cat.FileName.FileName.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (cat.FileName.FileName.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else
                {
                    ModelState.AddModelError("File", "This file format is not supported");
                    return View();
                }

                //4. DataSet - Create column names from first row
                reader.IsFirstRowAsColumnNames = true;
                result = reader.AsDataSet();
                rowCount = result.Tables[0].Rows.Count;
                dt = result.Tables[0];
                int currentRow = 0;

                //Validating the data 
                try
                {
                    while (reader.Read())
                    {

                        if (currentRow > 1 && currentRow < (rowCount))
                        {

                            if (reader.GetString(0) != null)
                            {
                                var SN = reader.GetString(0).Trim();

                            }
                            else
                            {
                                TempData["Fail"] = "Please Provide the SN at" + (currentRow + 1) + "th" + " row";
                                return Content("0");
                            }


                            if (reader.GetString(1) != null)
                            {
                                var BIBCode = reader.GetString(1).Trim();
                            }
                            else
                            {
                                TempData["Fail"] = "Please Provide the BIBCode at" + (currentRow + 1) + "th" + "row.";
                                return Content("0");

                            }
                            //var FullName = (reader.GetString(1) != null) ? reader.GetString(1).Trim() : "";

                            if (reader.GetString(2) != null)
                            {
                                var EndTime = reader.GetString(2).Trim();

                            }
                            else
                            {
                                TempData["Fail"] = "Please provide the proper EndTime" + (currentRow + 1) + "th " + "row.";
                                return Content("0");
                            }

                            //var SubDeptName = (reader.GetString(3) != null) ? reader.GetString(3).Trim() : null;

                            if (reader.GetString(3) != null)
                            {
                                var CheckPostName = reader.GetString(3).Trim();


                            }
                            else
                            {
                                TempData["Fail"] = "Please provide the valid Check point Name" + (currentRow + 1) + "th " + " row.";

                                return Content("0");
                            }

                        }
                        currentRow++;
                    }

                }
                catch (Exception ex)
                {
                    TempData["Fail"] = "Sorry, but something went wrong while processing excel import, please try again Later";
                    return View("Error", new HandleErrorInfo(ex, "RankCategoryWiseForm", "Race"));
                }
                var column = dt.Columns.Count;
                var rows = dt.Rows.Count;
                //get the distinct data
                DataTable disT = dt.DefaultView.ToTable( /*distinct*/ true);
                var station = new CheckStation();
                for (int i = 0; i < (disT.Rows.Count); i++)
                {
                    station.BIBCode = Convert.ToInt32(disT.Rows[i].ItemArray[1].ToString().Trim());
                    station.EndTime = DateTime.Parse(disT.Rows[i].ItemArray[2].ToString().Trim());
                    station.StationName = disT.Rows[i].ItemArray[3].ToString().Trim();
                    db.CheckStations.Add(station);
                    db.SaveChanges();
                }
                TempData["Success"] = "Successfully imported excel data.";
                return RedirectToAction("RankCategoryWiseForm", "Race");
            }

            else
            {
                TempData["Fail"] = "Sorry, but something went wrong while processing excel import, please try again Later";
                return RedirectToAction("RankCategoryWiseForm", "Race");
            }
        }

        [HttpPost]
        public ActionResult UpdateRegistered(int? rowId, string UserName, string UserBIBCode, string UserAge, string UserGender, string UserDateOfBirth, string UserPhoneNumber, string UserEmail, string UserStartTime, string UserDistance)
        {
            var data = db.Registers.Find(rowId);
            if (UserName != null && UserName != "")
            {
                data.Name = UserName.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserBIBCode != null && UserBIBCode != "")
            {
                data.BIBCode = Convert.ToInt32(UserBIBCode);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserAge != null && UserAge != "")
            {
                data.Age = UserAge.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserGender != null && UserGender != "")
            {
                data.Gender = UserGender.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserDateOfBirth != null && UserDateOfBirth != "")
            {
                data.DateOfBirth = DateTime.Parse(UserDateOfBirth);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserPhoneNumber != null && UserPhoneNumber != "")
            {
                data.PhoneNumber = UserPhoneNumber.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserEmail != null && UserEmail != "")
            {
                data.Email = UserEmail.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserStartTime != null && UserStartTime != "")
            {
                data.StartTime = DateTime.Parse(UserStartTime);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserDistance != null && UserDistance != "")
            {
                data.Distance = Convert.ToInt32(UserDistance);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            return View();
        }
        [HttpPost]
        public ActionResult UpdateFinalData(int? rowId, string UserName, string UserBIBCode, string UserAge, string UserGender, string UserInterval, string UserEndTime, string UserStartTime, string UserDistance)
        {
            var data = db.Registers.Find(rowId);
            var IntervalId = db.TimeIntervals.Where(x => x.BIBCode == data.BIBCode).FirstOrDefault();

            if (UserName != null && UserName != "")
            {
                data.Name = UserName.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserBIBCode != null && UserBIBCode != "")
            {
                data.BIBCode = Convert.ToInt32(UserBIBCode);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserAge != null && UserAge != "")
            {
                data.Age = UserAge.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserGender != null && UserGender != "")
            {
                data.Gender = UserGender.Trim();
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserEndTime != null && UserEndTime != "")
            {

                IntervalId.EndTime = DateTime.Parse(UserEndTime);
                db.Entry(IntervalId).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserInterval != null && UserInterval != "")
            {
                IntervalId.Interval = TimeSpan.Parse(UserInterval);
                db.Entry(IntervalId).State = EntityState.Modified;
                db.SaveChanges();
            }

            if (UserStartTime != null && UserStartTime != "")
            {
                data.StartTime = DateTime.Parse(UserStartTime);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();

                IntervalId.StartTime = DateTime.Parse(UserStartTime);
                db.Entry(IntervalId).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (UserDistance != null && UserDistance != "")
            {
                data.Distance = Convert.ToInt32(UserDistance);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            return View();
        }

        public ActionResult RegisterRunner()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RegisterRunner(RegisterViewModel register)
        {
            var RaceId = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var data = new Register()
            {
                Age = register.Age,
                BIBCode = register.BIBCode,
                DateOfBirth = register.DateOfBirth,
                Distance = register.Distance,
                Email = register.Email,
                FirstName = register.FirstName,
                Gender = register.Gender,
                LastName = register.LastName,
                MiddleName = register.MiddleName,
                Name = register.Name,
                PhoneNumber = register.PhoneNumber,
                StartTime = register.StartTime,
                RaceId = RaceId
            };
            db.Registers.Add(data);
            db.SaveChanges();
            return RedirectToAction("RegisterRunner", "Race");

        }

        public ActionResult Export()
        {

            //    /**********************************************************************************/
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet();
            sheet.SetColumnWidth(0, 10 * 256);
            sheet.SetColumnWidth(1, 20 * 256); //bib
            sheet.SetColumnWidth(2, 20 * 256);  //Name
            sheet.SetColumnWidth(3, 20 * 256);//Fistname
            sheet.SetColumnWidth(4, 20 * 256);//middlename
            sheet.SetColumnWidth(5, 20 * 256);//LastName
            sheet.SetColumnWidth(6, 20 * 256);//Age
            sheet.SetColumnWidth(7, 20 * 256);//Gender
            sheet.SetColumnWidth(8, 20 * 256);//Date of Birth
            sheet.SetColumnWidth(9, 20 * 256); //Phone Number
            sheet.SetColumnWidth(10, 20 * 256); //Email
            sheet.SetColumnWidth(11, 20 * 256); //Distance
            sheet.SetColumnWidth(12, 20 * 256);  //starttime
            sheet.DefaultRowHeightInPoints = 22;
            sheet.SetZoom(8, 8);
            sheet.FitToPage = true;
            //sheet.AutoSizeColumn(40, true);
            var headerfont = (HSSFFont)workbook.CreateFont();
            headerfont.FontHeightInPoints = 11;
            headerfont.FontName = "Arial";
            headerfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            var headerCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
            headerCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

            headerCellStyle.FillForegroundColor = HSSFColor.LightOrange.Index;
            headerCellStyle.SetFont(headerfont);
            headerCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Medium;
            headerCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Medium;

            var bodyfont = (HSSFFont)workbook.CreateFont();
            bodyfont.FontHeightInPoints = 10;
            bodyfont.FontName = "Arial";
            bodyfont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Normal;
            var bodyCellStyle = (HSSFCellStyle)workbook.CreateCellStyle();
            bodyCellStyle.FillForegroundColor = HSSFColor.LightOrange.Index;
            bodyCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            bodyCellStyle.SetFont(bodyfont);
            bodyCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            bodyCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            bodyCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;

            //sample for border
            var titlefont = (HSSFFont)workbook.CreateFont();
            titlefont.FontHeightInPoints = 10;
            titlefont.FontName = "Arial";
            titlefont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            var cellarrow = (HSSFCellStyle)workbook.CreateCellStyle();
            cellarrow.FillForegroundColor = HSSFColor.LightOrange.Index;
            cellarrow.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            cellarrow.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            cellarrow.SetFont(bodyfont);

            var headerRow = sheet.CreateRow(0);
            var dataCell = headerRow.CreateCell(0);
            dataCell.CellStyle = headerCellStyle;
            //  dataCell.SetCellValue("SN");


            string[] rowArr = { "SN", "BIBCode", "Name", "FirstName", "MiddleName", "LastName", "Age", "Gender", "DateOfBirth", "PhoneNumber", "Email", "Distance", "StartTime" };


            int j = 0;
            foreach (var item in rowArr)
            {
                dataCell = headerRow.CreateCell(j);
                dataCell.CellStyle = headerCellStyle;
                dataCell.SetCellValue(item);
                j++;
            }

            string[,] ArrayData = new string[1, 13] { { "1", "201", "Sapana Chaudhary", "Sapana", "", "Chaudhary", "18 to 39", "Female", "2018-01-01 12:00:00.000", "837283728", "sapnaa@gmail.com", "12", "2018-01-01 22:00:22.000" } };


            var rowPosition = 1;
            for (int inc1 = 0; inc1 <= ArrayData.GetUpperBound(0); inc1++)
            {
                var contentRow = sheet.CreateRow(rowPosition);
                for (int inc2 = 0; inc2 < 13; inc2++)
                {
                    string s1 = ArrayData[inc1, inc2];
                    dataCell = contentRow.CreateCell(inc2);
                    dataCell.CellStyle = bodyCellStyle;
                    dataCell.SetCellValue(s1);
                }
                rowPosition++;
            }
            sheet.CreateFreezePane(0, 1, 0, 1);
            var output = new MemoryStream();
            workbook.Write(output);
            return File(
                        output.ToArray(),
                       "application/vnd.ms-excel",
                         "UploadExcelList" + ".xls");
        }

        public ActionResult ExcelImportForRegister(FormCollection fc, RegisterViewModel cat, HttpPostedFileBase file)
        {

            AdoHelper ado = new AdoHelper();
            DataTable dt;
            DataSet result;
            int rowCount = 0;
            IExcelDataReader reader = null;
            var RaceId = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            if (cat.FileName != null && cat.FileName.ContentLength > 0)
            {
                // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                // to get started. This is how we avoid dependencies on ACE or Interop:
                Stream stream = cat.FileName.InputStream;

                if (cat.FileName.FileName.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (cat.FileName.FileName.EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else
                {
                    ModelState.AddModelError("File", "This file format is not supported");
                    return View();
                }

                //4. DataSet - Create column names from first row
                reader.IsFirstRowAsColumnNames = true;
                result = reader.AsDataSet();
                rowCount = result.Tables[0].Rows.Count;
                dt = result.Tables[0];
                int currentRow = 0;

                //Validating the data 
                try
                {
                    while (reader.Read())
                    {

                        if (currentRow > 1 && currentRow < (rowCount))
                        {

                            var SN = reader.GetString(0).Trim();
                            var BIBCode = reader.GetString(1).Trim();
                            var Name = reader.GetString(2).Trim();
                            var FistName = reader.GetString(3).Trim();
                            var MiddleName = reader.GetString(4).Trim();
                            var LastName = reader.GetString(5).Trim();
                            var Age = reader.GetString(6).Trim();
                            var Gender = reader.GetString(7).Trim();
                            var DateOfBirth = reader.GetString(8).Trim();
                            var PhoneNumber = reader.GetString(9).Trim();
                            var Email = reader.GetString(10).Trim();
                            var Distance = reader.GetString(11).Trim();
                            var StartTime = reader.GetString(12).Trim();


                        }
                        currentRow++;
                    }

                }
                catch (Exception ex)
                {
                    TempData["Fail"] = "Sorry, but something went wrong while processing excel import, please try again Later";
                    return View("Error", new HandleErrorInfo(ex, "Register", "Race"));
                }
                var column = dt.Columns.Count;
                var rows = dt.Rows.Count;
                //get the distinct data
                DataTable disT = dt.DefaultView.ToTable( /*distinct*/ true);
                var reg = new Register();
                for (int i = 0; i < (disT.Rows.Count); i++)
                {
                    reg.BIBCode = Convert.ToInt32(disT.Rows[i].ItemArray[1]);
                    reg.Name = disT.Rows[i].ItemArray[2].ToString().Trim();
                    reg.FirstName = disT.Rows[i].ItemArray[3].ToString().Trim();
                    reg.MiddleName = disT.Rows[i].ItemArray[4].ToString().Trim();
                    reg.LastName = disT.Rows[i].ItemArray[5].ToString().Trim();
                    reg.Age = disT.Rows[i].ItemArray[6].ToString().Trim();
                    reg.Gender = disT.Rows[i].ItemArray[7].ToString().Trim();
                    reg.DateOfBirth = DateTime.Parse(disT.Rows[i].ItemArray[8].ToString().Trim());
                    reg.PhoneNumber = disT.Rows[i].ItemArray[9].ToString().Trim();
                    reg.Email = disT.Rows[i].ItemArray[10].ToString().Trim();
                    reg.Distance = Convert.ToInt32(disT.Rows[i].ItemArray[11]);
                    reg.StartTime = DateTime.Parse(disT.Rows[i].ItemArray[12].ToString());
                    reg.RaceId = RaceId;
                    db.Registers.Add(reg);
                    db.SaveChanges();
                }
                TempData["Success"] = "Successfully imported excel data.";
                return RedirectToAction("RegisterRunner", "Race");
            }

            else
            {
                TempData["Fail"] = "Sorry, but something went wrong while processing excel import, please try again Later";
                return RedirectToAction("RegisterRunner", "Race");
            }
        }

        public ActionResult ShowResult()
        {
            var RaceId = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var allDistance = db.Registers.Where(x => x.RaceId == RaceId).GroupBy(x => x.Distance).OrderByDescending(x => x.Key).ToList();
            var FinalListData = new DisplayAllListDataViewModel();
            List<int?> distance = new List<int?>();
            foreach (var itm in allDistance)
            {
                distance.Add(itm.Key);
            }
            FinalListData.Distance = distance;
            return View(FinalListData);
        }
        public ActionResult ShowResultPartial(int Distance)
        {

            var FinalListData = new DisplayAllListDataViewModel();
            var RaceId = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            if (Distance != 0)
            {

                FinalListData.Display12to17MaleRankedDataViewModel = (from a in db.View12to17MaleRankedList
                                                                      where a.RaceId == RaceId && a.Distance == Distance
                                                                      select new Display12to17MaleRankedDataViewModel()
                                                                      {
                                                                          Id = a.Id,
                                                                          Age = a.Age,
                                                                          BIBCode = a.BIBCode,
                                                                          Distance = a.Distance,
                                                                          Email = a.Email,
                                                                          EndTime = a.EndTime,
                                                                          Gender = a.Gender,
                                                                          Interval = a.Interval,
                                                                          Name = a.Name,
                                                                          RaceId = a.RaceId,
                                                                          RANK = a.RANK,
                                                                          StartTime = a.StartTime
                                                                      }).Take(3).OrderBy(x => x.RANK).ToList();

                FinalListData.Display12to17FemaleRankedDataViewModel = (from a in db.View12to17FemaleRankedList
                                                                        where a.RaceId == RaceId && a.Distance == Distance
                                                                        select new Display12to17FemaleRankedDataViewModel()
                                                                        {
                                                                            Id = a.Id,
                                                                            Age = a.Age,
                                                                            BIBCode = a.BIBCode,
                                                                            Distance = a.Distance,
                                                                            Email = a.Email,
                                                                            EndTime = a.EndTime,
                                                                            Gender = a.Gender,
                                                                            Interval = a.Interval,
                                                                            Name = a.Name,
                                                                            RaceId = a.RaceId,
                                                                            RANK = a.RANK,
                                                                            StartTime = a.StartTime
                                                                        }).Take(3).OrderBy(x => x.RANK).ToList();


                FinalListData.Display11andBelowMaleRankedDataViewModel = (from a in db.View11andBelowMaleRankedList
                                                                          where a.RaceId == RaceId && a.Distance == Distance
                                                                          select new Display11andBelowMaleRankedDataViewModel()
                                                                          {
                                                                              Id = a.Id,
                                                                              Age = a.Age,
                                                                              BIBCode = a.BIBCode,
                                                                              Distance = a.Distance,
                                                                              Email = a.Email,
                                                                              EndTime = a.EndTime,
                                                                              Gender = a.Gender,
                                                                              Interval = a.Interval,
                                                                              Name = a.Name,
                                                                              RaceId = a.RaceId,
                                                                              RANK = a.RANK,
                                                                              StartTime = a.StartTime
                                                                          }).Take(3).OrderBy(x => x.RANK).ToList();

                FinalListData.Display11andBelowFemaleRankedDataViewModel = (from a in db.View11andBelowFemaleRankedList
                                                                            where a.RaceId == RaceId && a.Distance == Distance
                                                                            select new Display11andBelowFemaleRankedDataViewModel()
                                                                            {
                                                                                Id = a.Id,
                                                                                Age = a.Age,
                                                                                BIBCode = a.BIBCode,
                                                                                Distance = a.Distance,
                                                                                Email = a.Email,
                                                                                EndTime = a.EndTime,
                                                                                Gender = a.Gender,
                                                                                Interval = a.Interval,
                                                                                Name = a.Name,
                                                                                RaceId = a.RaceId,
                                                                                RANK = a.RANK,
                                                                                StartTime = a.StartTime
                                                                            }).Take(3).OrderBy(x => x.RANK).ToList();



                FinalListData.Display8andBelowMaleRankedData = (from a in db.View8andBelowMaleRankedList
                                                                where a.RaceId == RaceId && a.Distance == Distance
                                                                select new Display8andBelowMaleRankedDataViewModel()
                                                                {
                                                                    Id = a.Id,
                                                                    Age = a.Age,
                                                                    BIBCode = a.BIBCode,
                                                                    Distance = a.Distance,
                                                                    Email = a.Email,
                                                                    EndTime = a.EndTime,
                                                                    Gender = a.Gender,
                                                                    Interval = a.Interval,
                                                                    Name = a.Name,
                                                                    RaceId = a.RaceId,
                                                                    RANK = a.RANK,
                                                                    StartTime = a.StartTime
                                                                }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display8andBelowFemaleRankedData = (from a in db.View8andBelowFemaleRankedList
                                                                  where a.RaceId == RaceId && a.Distance == Distance
                                                                  select new Display8andBelowFemaleRankedDataViewModel()
                                                                  {
                                                                      Id = a.Id,
                                                                      Age = a.Age,
                                                                      BIBCode = a.BIBCode,
                                                                      Distance = a.Distance,
                                                                      Email = a.Email,
                                                                      EndTime = a.EndTime,
                                                                      Gender = a.Gender,
                                                                      Interval = a.Interval,
                                                                      Name = a.Name,
                                                                      RaceId = a.RaceId,
                                                                      RANK = a.RANK,
                                                                      StartTime = a.StartTime
                                                                  }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display17andBelowFemaleRankedData = (from a in db.View17andBelowFemaleRankedList
                                                                   where a.RaceId == RaceId && a.Distance == Distance
                                                                   select new Display17andBelowFemaleRankedDataViewModel()
                                                                   {
                                                                       Id = a.Id,
                                                                       Age = a.Age,
                                                                       BIBCode = a.BIBCode,
                                                                       Distance = a.Distance,
                                                                       Email = a.Email,
                                                                       EndTime = a.EndTime,
                                                                       Gender = a.Gender,
                                                                       Interval = a.Interval,
                                                                       Name = a.Name,
                                                                       RaceId = a.RaceId,
                                                                       RANK = a.RANK,
                                                                       StartTime = a.StartTime
                                                                   }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display17andBelowMaleRankedData = (from a in db.View17andBelowMaleRankedList
                                                                 where a.RaceId == RaceId && a.Distance == Distance
                                                                 select new Display17andBelowMaleRankedDataViewModel()
                                                                 {
                                                                     Id = a.Id,
                                                                     Age = a.Age,
                                                                     BIBCode = a.BIBCode,
                                                                     Distance = a.Distance,
                                                                     Email = a.Email,
                                                                     EndTime = a.EndTime,
                                                                     Gender = a.Gender,
                                                                     Interval = a.Interval,
                                                                     Name = a.Name,
                                                                     RaceId = a.RaceId,
                                                                     RANK = a.RANK,
                                                                     StartTime = a.StartTime
                                                                 }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display18to39MaleRankedData = (from a in db.View18to39MaleRankedList
                                                             where a.RaceId == RaceId && a.Distance == Distance
                                                             select new Display18to39MaleRankedDataViewModel()
                                                             {
                                                                 Id = a.Id,
                                                                 Age = a.Age,
                                                                 BIBCode = a.BIBCode,
                                                                 Distance = a.Distance,
                                                                 Email = a.Email,
                                                                 EndTime = a.EndTime,
                                                                 Gender = a.Gender,
                                                                 Interval = a.Interval,
                                                                 Name = a.Name,
                                                                 RaceId = a.RaceId,
                                                                 RANK = a.RANK,
                                                                 StartTime = a.StartTime
                                                             }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display18to39FemaleRankedData = (from a in db.View18to39FemaleRankedList
                                                               where a.RaceId == RaceId && a.Distance == Distance
                                                               select new Display18to39FemaleRankedDataViewModel()
                                                               {
                                                                   Id = a.Id,
                                                                   Age = a.Age,
                                                                   BIBCode = a.BIBCode,
                                                                   Distance = a.Distance,
                                                                   Email = a.Email,
                                                                   EndTime = a.EndTime,
                                                                   Gender = a.Gender,
                                                                   Interval = a.Interval,
                                                                   Name = a.Name,
                                                                   RaceId = a.RaceId,
                                                                   RANK = a.RANK,
                                                                   StartTime = a.StartTime
                                                               }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display40andAboveMaleRankedData = (from a in db.View40andAboveMaleRankedList
                                                                 where a.RaceId == RaceId && a.Distance == Distance
                                                                 select new Display40andAboveMaleRankedDataViewModel()
                                                                 {
                                                                     Id = a.Id,
                                                                     Age = a.Age,
                                                                     BIBCode = a.BIBCode,
                                                                     Distance = a.Distance,
                                                                     Email = a.Email,
                                                                     EndTime = a.EndTime,
                                                                     Gender = a.Gender,
                                                                     Interval = a.Interval,
                                                                     Name = a.Name,
                                                                     RaceId = a.RaceId,
                                                                     RANK = a.RANK,
                                                                     StartTime = a.StartTime
                                                                 }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display40andAboveFemaleRankedData = (from a in db.View40andAboveFemaleRankedList
                                                                   where a.RaceId == RaceId && a.Distance == Distance
                                                                   select new Display40andAboveFemaleRankedDataViewModel()
                                                                   {
                                                                       Id = a.Id,
                                                                       Age = a.Age,
                                                                       BIBCode = a.BIBCode,
                                                                       Distance = a.Distance,
                                                                       Email = a.Email,
                                                                       EndTime = a.EndTime,
                                                                       Gender = a.Gender,
                                                                       Interval = a.Interval,
                                                                       Name = a.Name,
                                                                       RaceId = a.RaceId,
                                                                       RANK = a.RANK,
                                                                       StartTime = a.StartTime
                                                                   }).Take(3).OrderBy(x => x.RANK).ToList();
            }
            else
            {


                FinalListData.Display8andBelowMaleRankedData = (from a in db.View8andBelowMaleRankedList
                                                                where a.RaceId == RaceId && a.Distance == null
                                                                select new Display8andBelowMaleRankedDataViewModel()
                                                                {
                                                                    Id = a.Id,
                                                                    Age = a.Age,
                                                                    BIBCode = a.BIBCode,
                                                                    Distance = a.Distance,
                                                                    Email = a.Email,
                                                                    EndTime = a.EndTime,
                                                                    Gender = a.Gender,
                                                                    Interval = a.Interval,
                                                                    Name = a.Name,
                                                                    RaceId = a.RaceId,
                                                                    RANK = a.RANK,
                                                                    StartTime = a.StartTime
                                                                }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display8andBelowFemaleRankedData = (from a in db.View8andBelowFemaleRankedList
                                                                  where a.RaceId == RaceId && a.Distance == null
                                                                  select new Display8andBelowFemaleRankedDataViewModel()
                                                                  {
                                                                      Id = a.Id,
                                                                      Age = a.Age,
                                                                      BIBCode = a.BIBCode,
                                                                      Distance = a.Distance,
                                                                      Email = a.Email,
                                                                      EndTime = a.EndTime,
                                                                      Gender = a.Gender,
                                                                      Interval = a.Interval,
                                                                      Name = a.Name,
                                                                      RaceId = a.RaceId,
                                                                      RANK = a.RANK,
                                                                      StartTime = a.StartTime
                                                                  }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display17andBelowFemaleRankedData = (from a in db.View17andBelowFemaleRankedList
                                                                   where a.RaceId == RaceId && a.Distance == null
                                                                   select new Display17andBelowFemaleRankedDataViewModel()
                                                                   {
                                                                       Id = a.Id,
                                                                       Age = a.Age,
                                                                       BIBCode = a.BIBCode,
                                                                       Distance = a.Distance,
                                                                       Email = a.Email,
                                                                       EndTime = a.EndTime,
                                                                       Gender = a.Gender,
                                                                       Interval = a.Interval,
                                                                       Name = a.Name,
                                                                       RaceId = a.RaceId,
                                                                       RANK = a.RANK,
                                                                       StartTime = a.StartTime
                                                                   }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display17andBelowMaleRankedData = (from a in db.View17andBelowMaleRankedList
                                                                 where a.RaceId == RaceId && a.Distance == null
                                                                 select new Display17andBelowMaleRankedDataViewModel()
                                                                 {
                                                                     Id = a.Id,
                                                                     Age = a.Age,
                                                                     BIBCode = a.BIBCode,
                                                                     Distance = a.Distance,
                                                                     Email = a.Email,
                                                                     EndTime = a.EndTime,
                                                                     Gender = a.Gender,
                                                                     Interval = a.Interval,
                                                                     Name = a.Name,
                                                                     RaceId = a.RaceId,
                                                                     RANK = a.RANK,
                                                                     StartTime = a.StartTime
                                                                 }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display18to39MaleRankedData = (from a in db.View18to39MaleRankedList
                                                             where a.RaceId == RaceId && a.Distance == null
                                                             select new Display18to39MaleRankedDataViewModel()
                                                             {
                                                                 Id = a.Id,
                                                                 Age = a.Age,
                                                                 BIBCode = a.BIBCode,
                                                                 Distance = a.Distance,
                                                                 Email = a.Email,
                                                                 EndTime = a.EndTime,
                                                                 Gender = a.Gender,
                                                                 Interval = a.Interval,
                                                                 Name = a.Name,
                                                                 RaceId = a.RaceId,
                                                                 RANK = a.RANK,
                                                                 StartTime = a.StartTime
                                                             }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display18to39FemaleRankedData = (from a in db.View18to39FemaleRankedList
                                                               where a.RaceId == RaceId && a.Distance == null
                                                               select new Display18to39FemaleRankedDataViewModel()
                                                               {
                                                                   Id = a.Id,
                                                                   Age = a.Age,
                                                                   BIBCode = a.BIBCode,
                                                                   Distance = a.Distance,
                                                                   Email = a.Email,
                                                                   EndTime = a.EndTime,
                                                                   Gender = a.Gender,
                                                                   Interval = a.Interval,
                                                                   Name = a.Name,
                                                                   RaceId = a.RaceId,
                                                                   RANK = a.RANK,
                                                                   StartTime = a.StartTime
                                                               }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display40andAboveMaleRankedData = (from a in db.View40andAboveMaleRankedList
                                                                 where a.RaceId == RaceId && a.Distance == null
                                                                 select new Display40andAboveMaleRankedDataViewModel()
                                                                 {
                                                                     Id = a.Id,
                                                                     Age = a.Age,
                                                                     BIBCode = a.BIBCode,
                                                                     Distance = a.Distance,
                                                                     Email = a.Email,
                                                                     EndTime = a.EndTime,
                                                                     Gender = a.Gender,
                                                                     Interval = a.Interval,
                                                                     Name = a.Name,
                                                                     RaceId = a.RaceId,
                                                                     RANK = a.RANK,
                                                                     StartTime = a.StartTime
                                                                 }).Take(3).OrderBy(x => x.RANK).ToList();
                FinalListData.Display40andAboveFemaleRankedData = (from a in db.View40andAboveFemaleRankedList
                                                                   where a.RaceId == RaceId && a.Distance == null
                                                                   select new Display40andAboveFemaleRankedDataViewModel()
                                                                   {
                                                                       Id = a.Id,
                                                                       Age = a.Age,
                                                                       BIBCode = a.BIBCode,
                                                                       Distance = a.Distance,
                                                                       Email = a.Email,
                                                                       EndTime = a.EndTime,
                                                                       Gender = a.Gender,
                                                                       Interval = a.Interval,
                                                                       Name = a.Name,
                                                                       RaceId = a.RaceId,
                                                                       RANK = a.RANK,
                                                                       StartTime = a.StartTime
                                                                   }).Take(3).OrderBy(x => x.RANK).ToList();
            }
            return PartialView(FinalListData);
        }
        public ActionResult MissedRunnerCheckSel()
        {
            var RaceId = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var jsonResult = "";
            var Object = new TrailRace();
            var jsonlist = db.SpMissedRunnersCheckinSel(RaceId).ToList();
            jsonResult = String.Join("", jsonlist.ToArray());
            if (jsonResult == "")
            {
                return View(Object.DisplayList = null);
            }
            else
            {
                var model = JsonConvert.DeserializeObject<List<UltraTrailRaceViewModel>>(jsonResult);
                var list = new List<UltraTrailRaceViewModel>();

                foreach (var item in model)
                {
                    var a = new UltraTrailRaceViewModel()
                    {
                        Id = item.Id,
                        BIBCode = item.BIBCode,
                        Name = item.Name,
                        DateOfBirth = item.DateOfBirth,
                        Gender = item.Gender,
                        Email = item.Email,
                        PhoneNumber = item.PhoneNumber,
                        Distance = item.Distance,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        Interval = item.Interval,
                        RaceId = item.RaceId,
                        Age = item.Age
                    };
                    list.Add(a);
                }

                Object.DisplayList = list;
                return View(Object);
            }

        }





        /*API PORTION*/



    }
}