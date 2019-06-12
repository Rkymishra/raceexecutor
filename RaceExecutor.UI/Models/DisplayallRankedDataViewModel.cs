using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaceExecutor.UI.Models
{
    public class DisplayallRankedDataViewModel
    {
        
    }

    public class DisplayAllListDataViewModel
    {
       public List<int?> Distance { get; set; }
        public  List<Display8andBelowMaleRankedDataViewModel> Display8andBelowMaleRankedData { get; set; }
        public List<Display8andBelowFemaleRankedDataViewModel> Display8andBelowFemaleRankedData { get; set; }
        public List<Display17andBelowMaleRankedDataViewModel> Display17andBelowMaleRankedData { get; set; }
        public List<Display17andBelowFemaleRankedDataViewModel> Display17andBelowFemaleRankedData { get; set; }
        public List<Display18to39MaleRankedDataViewModel> Display18to39MaleRankedData { get; set; }
        public List<Display18to39FemaleRankedDataViewModel> Display18to39FemaleRankedData { get; set; }
        public List<Display40andAboveMaleRankedDataViewModel> Display40andAboveMaleRankedData { get; set; }
        public List<Display40andAboveFemaleRankedDataViewModel> Display40andAboveFemaleRankedData { get; set; }

        public List<Display12to17MaleRankedDataViewModel> Display12to17MaleRankedDataViewModel { get; set; }
        public List<Display12to17FemaleRankedDataViewModel> Display12to17FemaleRankedDataViewModel { get; set; }

        public List<Display11andBelowMaleRankedDataViewModel> Display11andBelowMaleRankedDataViewModel { get; set; }

        public List<Display11andBelowFemaleRankedDataViewModel> Display11andBelowFemaleRankedDataViewModel { get; set; }
    }

    public class Display12to17MaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display12to17FemaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }

    public class Display11andBelowMaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display11andBelowFemaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }


    public class Display8andBelowMaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display8andBelowFemaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display17andBelowMaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display17andBelowFemaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display18to39MaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display18to39FemaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display40andAboveMaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
    public class Display40andAboveFemaleRankedDataViewModel
    {
        public int Id { get; set; }
        public long? BIBCode { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public Nullable<int> Distance { get; set; }
        public TimeSpan? Interval { get; set; }
        public Nullable<long> RANK { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<long> RaceId { get; set; }
    }
}