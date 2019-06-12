using Newtonsoft.Json;
using RaceExecutor.UI.Models;
using RaceExecutorModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace RaceExecutor.UI.Controllers
{
    public class RaceApiController : ApiController
    {

        TrailRaceNepalEntities db = new TrailRaceNepalEntities();

        [HttpPost]
        [Route("api/RaceApi/TimeIntervalIns")]
        public object TimeIntervalIns(long? BIBCode)
        {           
            try
            {
                var BIBCodeCheck = db.Registers.Where(x => x.BIBCode == BIBCode).ToList();
                var raceid = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
                var registeredData = db.Registers.Where(x => x.RaceId == raceid).ToList();
                ReturnMessageModel result = new ReturnMessageModel();
                ObjectParameter returnMessage = new ObjectParameter("ReturnMessage", typeof(String));
                foreach (var item in registeredData)
                {

                    if (item.BIBCode == BIBCode)
                    {
                        var time = new TimeInterval()
                        {
                            BIBCode = BIBCode,
                            StartTime = item.StartTime,
                            EndTime = DateTime.UtcNow.AddHours(5).AddMinutes(45),
                            Interval = DateTime.UtcNow.AddHours(5).AddMinutes(45) - item.StartTime,
                            RaceId = raceid
                        };
                        try
                        {
                            db.TimeIntervals.Add(time);
                            db.SaveChanges();
                        }
                        catch (SqlException)
                        {
                            return Ok("BIB Already inserted." + Environment.NewLine + "Or something went wrong.");
                        }

                    }
                }
                    return Ok("Successfully inserted!");
            }
            catch (Exception excep)
            {
                return Ok("BIB Already inserted \n Or something went wrong.");
            }
        }


        [ResponseType(typeof(TimeInterval))]
        public IHttpActionResult PostTimeInterval(UltraTrailRaceViewModel viewmodelobj)
        {            
            var BIBCodeCheck = db.Registers.Where(x => x.BIBCode == viewmodelobj.BIBCode).ToList();
            var raceid = db.Races.OrderByDescending(x => x.Id).Take(1).Select(x => x.Id).FirstOrDefault();
            var registeredData = db.Registers.Where(x => x.RaceId == raceid).ToList();
            var End_Time = viewmodelobj.EndTime;
            try
            {
                    foreach (var item in registeredData)
                    {
                        if (item.BIBCode == viewmodelobj.BIBCode)
                        {

                            var day = (DateTime.UtcNow.AddHours(5).AddMinutes(45).Day - item.StartTime.Value.Day);
                            var hours = (DateTime.UtcNow.AddHours(5).AddMinutes(45).TimeOfDay - item.StartTime.Value.TimeOfDay);
                            var time = new TimeInterval()
                            {
                                BIBCode = viewmodelobj.BIBCode,
                                StartTime = item.StartTime,
                                EndTime = End_Time,
                                Interval = DateTime.UtcNow.AddHours(5).AddMinutes(45) - item.StartTime,
                                RaceId = raceid
                            };
                            db.TimeIntervals.Add(time);
                            db.SaveChanges();
                            return Ok(" Added and Checked Submitted Successfully!");
                        }

                    }
                    return Ok(" Added and Checked Submitted Successfully!");
            }
            catch
            {
                return Ok("Something went wrong");
            }
        }
    }
}
