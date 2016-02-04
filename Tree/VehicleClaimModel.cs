using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree
{
    [Serializable]
    public class VehicleClaimModel
    {
        public int ID { get; set; }
        public string ClaimNumber { get; set; }
        public string VehicleRegistionNumber { get; set; }
        public string EventDate { get; set; }

        public string VE { get { return VehicleRegistionNumber + "#" + EventDate; } }
    }
}
