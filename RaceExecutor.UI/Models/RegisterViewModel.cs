using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceExecutor.UI.Models
{
    public partial class RegisterViewModel
    {
        public int Id { get; set; }
        public long BIBCode { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Nullable<int> Distance { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<long> RaceId { get; set; }
        public HttpPostedFileBase FileName { get; set; }
    }
}