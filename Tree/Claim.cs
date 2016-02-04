using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree
{
    [Serializable]
    public class Claim
    {
        public string ClaimNumber { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string EventDate { get; set; }
        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}", ClaimNumber, VehicleRegistrationNumber, EventDate.Replace("-", "/"));
        }
    }
}
