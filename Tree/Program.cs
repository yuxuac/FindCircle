using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tree
{
    class Program
    {
        // Vehicle registration number: http://baike.baidu.com/view/64583.htm
        public static readonly Regex RegexValidVehicleRegistrationNumber = new Regex(@"^(([\u4e00-\u9fa5]{1}[A-Z]{1})[-]?|([wW][Jj][\u4e00-\u9fa5]{1}[-]?)|([a-zA-Z]{2}))[A-Za-z0-9]{5}$", RegexOptions.Compiled);

        public static bool IsValidVehicleRegistrationNumber(string vehicleRegistrationNumber)
        {
            return RegexValidVehicleRegistrationNumber.IsMatch(vehicleRegistrationNumber.Trim());
        }

        static void Main(string[] args)
        {
            TestMethod1();
        }

        public static void MajorMethod()
        {
            string file1 = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "VehicleClaims.txt");

            // Get only mutiple vehicle accidents.
            var vehicleClaims = Cache.GetVehicleClaims(file1)
                                    .GroupBy(v => v.ClaimNumber)
                                    .Where(g => g.Count() > 1)
                                    .SelectMany(v => v.Where(c => IsValidVehicleRegistrationNumber(c.VehicleRegistionNumber))).ToList();

            var validVehicleRegistrationNumbers = vehicleClaims.Select(r => r.VehicleRegistionNumber)
                                                                .Where(r => IsValidVehicleRegistrationNumber(r))
                                                                .Distinct()
                                                                .ToArray();

            string file = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");

            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.WriteLine("Found a circle");
                for (int i = 0; i < validVehicleRegistrationNumbers.Length; i++)
                {
                    try
                    {
                        decimal percentage = (decimal)(i * 100) / validVehicleRegistrationNumbers.Length;
                        Console.WriteLine(string.Format(@"{0}:{1}/{2}({3}%)", validVehicleRegistrationNumbers[i], i, validVehicleRegistrationNumbers.Length, percentage.ToString("F5")));
                        var root = new Node()
                        {
                            ClaimInfo = new Claim()
                            {
                                ClaimNumber = vehicleClaims.Where(v => v.VehicleRegistionNumber == validVehicleRegistrationNumbers[i]).Select(c => c.ClaimNumber).FirstOrDefault(),
                                VehicleRegistrationNumber = validVehicleRegistrationNumbers[i]
                            },
                            Parent = null
                        };

                        BuildTree2(root, vehicleClaims, new List<string>());

                        if (root.Children.Count <= 0) continue;

                        Tree tr = new Tree() { Root = root };
                        int depth = Tree.GetDepth(tr.Root);

                        if (depth >= 2)
                        {
                            var circles = tr.GetCircles();

                            if (circles.Count > 0)
                            {
                                foreach (var cc in circles)
                                {
                                    sw.WriteLine("Found a circle:" + cc.GetTraceString());
                                    Console.WriteLine("Found a circle:" + cc.GetTraceString());
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine(ex.Message);
                        Console.WriteLine("Exception:" + ex.Message + ex.StackTrace);
                    }
                }
            }
        }

        static void BuildTree2(Node node, List<VehicleClaimModel> vehicleClaims, List<string> claimNumbersAlreadyIncluded)
        {
            if (claimNumbersAlreadyIncluded == null) claimNumbersAlreadyIncluded = new List<string>();
            if (!claimNumbersAlreadyIncluded.Contains(node.ClaimInfo.ClaimNumber))
            {
                claimNumbersAlreadyIncluded.Add(node.ClaimInfo.ClaimNumber);
            }
            string[] historyClaimNumbers = vehicleClaims.Where(c => c.VehicleRegistionNumber == node.ClaimInfo.VehicleRegistrationNumber && 
                                                                    !claimNumbersAlreadyIncluded.Contains(c.ClaimNumber))
                                                        .Select(c => c.ClaimNumber)
                                                        .ToArray();
            var relatingVehicles = vehicleClaims.Where(vc => historyClaimNumbers.Contains(vc.ClaimNumber) && 
                                                             vc.VehicleRegistionNumber != node.ClaimInfo.VehicleRegistrationNumber)
                                                .ToList();
            if (relatingVehicles.Count() > 0)
            {
                foreach (var rv in relatingVehicles)
                {
                    var currentNode = new Node() 
                    { 
                        Parent = node, 
                        ClaimInfo = new Claim() 
                        { 
                            ClaimNumber = rv.ClaimNumber, 
                            VehicleRegistrationNumber = rv.VehicleRegistionNumber 
                        } 
                    };
                    BuildTree2(currentNode, vehicleClaims, claimNumbersAlreadyIncluded);
                    node.Children.Add(currentNode);
                }
            }
        }

        /*
        static void BuildTree(Node node, List<VehicleClaimModel> vehicleClaims)
        {
            var relatingVehicles = vehicleClaims.Where(c => c.VehicleRegistionNumber == node.ClaimInfo.VehicleRegistrationNumber && node.ClaimInfo.ClaimNumber != c.ClaimNumber)
                                                            .GroupBy(c => c.ClaimNumber)
                                                            .Where(c => c.Count() > 1)
                                                            .Select(p => new
                                                            {
                                                                ClaimNumber = p.Key,
                                                                VehicleRegistrationNumbers = p.Where(re => re.VehicleRegistionNumber != node.ClaimInfo.VehicleRegistrationNumber)
                                                                                              .Select(v => v.VehicleRegistionNumber)
                                                            });
            if (relatingVehicles.Count() > 0)
            {
                foreach (var rv in relatingVehicles)
                {
                    foreach (var v in rv.VehicleRegistrationNumbers)
                    {
                        var currentNode = new Node() { Parent = node, ClaimInfo = new Claim() { ClaimNumber = rv.ClaimNumber, VehicleRegistrationNumber = v } };
                        BuildTree(currentNode, vehicleClaims);
                        node.Children.Add(currentNode);
                    }
                }
            }
        }*/

        public static void WriteVehicleClaimsToFile(string file, List<VehicleClaimModel> vcs)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                foreach (var d in vcs)
                {
                    sw.WriteLine(string.Format("{0},{1},{2}", d.ID, d.ClaimNumber, d.VehicleRegistionNumber));
                }
            }
        }

        public static void WriteVehicleRegistrationNumberToFile(string file)
        {
            //string file = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");

            using (StreamWriter sw = new StreamWriter(file))
            {
                using (var context = new ControlExpertEntities())
                {
                    var ds = context.VehicleClaims.Select(vc => vc.VehicleRegistionNumber);
                    // foreach, get tree and try to get circle.
                    foreach (var d in ds)
                    {
                        if (IsValidVehicleRegistrationNumber(d))
                        {
                            sw.WriteLine(d);
                        }
                    }
                }
            }
        }

        public static void TestMethod1()
        {
            var targetClaim = DataProvider.GetClaim();

            var match1 = DataProvider.GetClaim();
            var match2 = DataProvider.GetClaim();
            var match3 = DataProvider.GetClaim();

            match1.VehicleRegistrationNumber = targetClaim.VehicleRegistrationNumber;
            match2.VehicleRegistrationNumber = targetClaim.VehicleRegistrationNumber;
            match3.VehicleRegistrationNumber = targetClaim.VehicleRegistrationNumber;

            var rootNode = new Node
            {
                ClaimInfo = targetClaim,
                Parent = null,
                Children = new List<Node>() 
                { 
                    new Node()
                    { 
                        ClaimInfo = DataProvider.GetClaim(),
                        Children = DataProvider.GetNodes(3)
                    },

                    new Node()
                    { 
                        ClaimInfo = DataProvider.GetClaim(),
                        Children = DataProvider.GetNodes(2)
                    },

                    new Node()
                    { 
                        ClaimInfo = DataProvider.GetClaim(), 
                        Children = new List<Node>()
                        {
                            new Node()
                            {
                                ClaimInfo = DataProvider.GetClaim(),
                                Children = new List<Node>()
                                {
                                    new Node()
                                    {
                                        ClaimInfo = DataProvider.GetClaim(),
                                        Children = new List<Node>()
                                        {
                                            DataProvider.GetNode(),
                                            DataProvider.GetNode(),
                                            DataProvider.GetNode(),
                                            new Node()
                                            {
                                                ClaimInfo = match1,
                                                Children = DataProvider.GetNodes(4)
                                            },
                                        }
                                    },

                                    new Node()
                                    {
                                        ClaimInfo = DataProvider.GetClaim(),
                                        Children = new List<Node>()
                                        {
                                            new Node()
                                            {
                                                ClaimInfo = match2
                                            },
                                            DataProvider.GetNode(),
                                            DataProvider.GetNode()
                                        }
                                    }
                                }
                            },

                            new Node()
                            {
                                 ClaimInfo = DataProvider.GetClaim(),
                                 Children = DataProvider.GetNodes(4)
                            }
                        } 
                    },
                }
            };

         
            var tree = new Tree() { Root = rootNode };
            string treeStr = tree.ToString();

            var circles = tree.GetCircles();
            foreach (var c in circles)
            {
                Console.WriteLine(c.GetTraceString());
            }
        }
    }
}
