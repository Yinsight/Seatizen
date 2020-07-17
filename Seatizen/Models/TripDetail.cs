using System;
using System.Collections.Generic;
using System.Text;

namespace Seatizen.Models
{
    public class TripDetail
    {
        public int BranchID { get; set; }
        public int TrainID { get; set; }
        public int StationID { get; set; }
        public int CarID { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
    }
}
