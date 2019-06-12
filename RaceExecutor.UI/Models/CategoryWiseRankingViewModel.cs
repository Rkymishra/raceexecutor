using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceExecutor.UI.Models
{
    public class CategoryWiseRankingViewModel
    {
        public string Category { get; set; }
        public string Gender { get; set; }
        public int Distance { get; set; }
        public DateTime Date { get; set; }
        public HttpPostedFileBase FileName { get; set; }
    }
}