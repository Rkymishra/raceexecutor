using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceExecutor.UI.Models
{
    public class ImportCheckPointDataViewModel
    {
       public long BIBCode { get; set; }
        public DateTime? EndTime { get; set; }
        public string checkpostname { get; set; }
    }
}