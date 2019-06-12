using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RaceExecutor.UI.Models
{
    public class TrailRace
    {
        public List<UltraTrailRaceViewModel> DisplayList { get; set; }
        public DateTime Date { get; set; }

    }
    public class UltraTrailRaceViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Interval { get; set; }
        public int? Distance { get; set; }
        public int Rank { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public long? RaceId { get; set; }
        public List<SelectListItem> listStartTime { get; set; }
        public string Nationaity { get; set; }
        public string SeasonPass { get; set; }
        public string TeamName { get; set; }
    }
}