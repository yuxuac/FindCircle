using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Tree
{
    public static class Cache
    {
        public static List<VehicleClaimModel> VehicleClaims
        {
            get
            {
                return UsingCache<List<VehicleClaimModel>>("VehicleClaims", () =>
                {
                    ControlExpertEntities context = new ControlExpertEntities();
                    return context.VehicleClaims.Select(v => new VehicleClaimModel() { ClaimNumber = v.ClaimNumber, ID = v.ID, VehicleRegistionNumber = v.VehicleRegistionNumber }).ToList();
                });
            }
            private set { }
        }

        public static string[] VehicleRegistrationNumbers
        {
            get
            {
                List<string> vs = new List<string>();
                return UsingCache<string[]>("VehicleRegistrationNumbers", () =>
                {
                    using (StreamReader sr = new StreamReader(@".\AllVehicles.txt"))
                    {
                        while (!sr.EndOfStream) 
                        {
                            vs.Add(sr.ReadLine().Trim());
                        }
                    }
                    return vs.ToArray();
                });
            }
            private set { }
        }

        public static List<VehicleClaimModel> GetVehicleClaims(string file)
        {
            List<VehicleClaimModel> vcs = new List<VehicleClaimModel>();
            List<VehicleClaimModel> result = new List<VehicleClaimModel>();

            using (StreamReader sr = new StreamReader(file))
            {
                while (!sr.EndOfStream) 
                { 
                    string line = sr.ReadLine();
                    if(line.Contains(","))
                    {
                        string[] ls = line.Split(',');
                        VehicleClaimModel m = new VehicleClaimModel() 
                        { 
                            ID = int.Parse(ls[0]),
                            ClaimNumber = ls[1], //ls[2] + "_" + ls[3], , 
                            VehicleRegistionNumber = ls[2], 
                            EventDate = ls[3]
                        };
                        vcs.Add(m);
                    }
                }
            }

            // Only keep valid Vehicle RegistrationNumber
            vcs = vcs.Where(v => Program.IsValidVehicleRegistrationNumber(v.VehicleRegistionNumber))
                     .ToList();

            var claims = vcs.GroupBy(c => c.VehicleRegistionNumber + "#" + c.EventDate)
                .Select(o => new { VehicleEvent = o.Key, CNT = o.Count() })
                .Where(c => c.CNT > 1)
                .ToList().Distinct();
                                                                              
            //var vrPair = claims.Select(c =>
            //{
            //    var a = c.VehicleEvent.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            //    return new { VehicleRegistionNumber = a[0], EventDate = a[1], VE = c.VehicleEvent};
            //}).Distinct();


            /*
            // Duplicated claim numbers

            int count = 0;

            // Step1
            foreach (var item in vcs)
            {
                Console.WriteLine((count++) + "/" + vcs.Count());
                if (vrPair.Where(vp => vp.VehicleRegistionNumber == item.VehicleRegistionNumber && vp.EventDate == item.EventDate).Count() == 0)
                {
                    result.Add(item);
                }
            }

            count = 0;
            foreach (var pair in vrPair)
            {
                Console.WriteLine((count++) + "/" + vrPair.Count());

                string chosenClaimNumber = null;

                List<VehicleClaimModel> tempList = new List<VehicleClaimModel>();

                vcs.Where(v => v.VehicleRegistionNumber == pair.VehicleRegistionNumber &&
                               v.EventDate == pair.EventDate)
                    .ToList()
                    .ForEach(i => 
                    {
                        if (chosenClaimNumber == null)
                        {
                            chosenClaimNumber = i.ClaimNumber;
                            tempList.Add(i);
                        }
                        else 
                        {
                            i.ClaimNumber = chosenClaimNumber;
                            tempList.Add(i);
                        }
                    });
            }

            var claims2 = result.GroupBy(c => c.VehicleRegistionNumber + "#" + c.EventDate)
                             .Select(o => new { VehicleEvent = o.Key, CNT = o.Count() })
                             .Where(c => c.CNT > 1)
                             .ToList();
             */
            return vcs;
        }

        public static string[] GetVehicleRegistrationNumbers(string file)
        {
            List<string> vcs = new List<string>();
            using (StreamReader sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    vcs.Add(line.Trim());
                }
            }
            return vcs.ToArray();
        }

        public static T UsingCache<T>(string cacheKey, Func<T> function)
        {
            object cachedObject = null;
            if (HttpContext.Current != null)
            {
                cachedObject = HttpContext.Current.Cache.Get(cacheKey);
            }
            else
            {
                cachedObject = MemoryCache.Default.Get(cacheKey);
            }

            if (cachedObject != null)
            {
                if (cachedObject == DBNull.Value)
                {
                    return default(T);
                }

                try
                {
                    return (T)cachedObject;
                }
                catch
                {
                    // In this case continue and get result from function
                }
            }

            T result = function();

            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(cacheKey, (object)result ?? DBNull.Value, new CacheDependency(HttpContext.Current.Server.MapPath("~/bin")), DateTime.Now.AddDays(1), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Default, null);
            }
            else
            {
                MemoryCache.Default.Add(
                    cacheKey,
                    (object)result ?? DBNull.Value,
                    new CacheItemPolicy()
                    {
                        AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1))
                    });
            }

            return result;
        }

        public static MemoryStream SerializeStream<T>(T obj)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return stream;
        }

        public static T DeSerializeStream<T>(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            T o = (T)formatter.Deserialize(stream);
            return o;
        }

        public static void SaveStream(string fileName, MemoryStream ms)
        {
            // Write to memory file without encoding.
            using (FileStream file = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[ms.Capacity];
                ms.Read(bytes, 0, (int)ms.Length);
                file.Write(bytes, 0, bytes.Length);
                ms.Close();
            }
        }

        public static MemoryStream ReadFileToMemoryStream(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                MemoryStream ms = new MemoryStream(bytes);
                return ms;
            }
        }
    }
}
