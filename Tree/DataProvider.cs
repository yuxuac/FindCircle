using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tree
{
    public class DataProvider
    {
        public static Random R = new Random();
        public static char[] chs = {'京',
                        '津',
                        '冀',
                        '晋',
                        '蒙',
                        '辽',
                        '吉',
                        '黑',
                        '沪',
                        '苏',
                        '浙',
                        '皖',
                        '闽',
                        '赣',
                        '鲁',
                        '豫',
                        '鄂',
                        '湘',
                        '粤',
                        '桂',
                        '琼',
                        '渝',
                        '川',
                        '贵',
                        '云',
                        '藏',
                        '陕',
                        '甘',
                        '青',
                        '宁',
                        '新',
                        '港',
                        '澳',
                        '台'};

        public static string GetVehicleRegistrationNumber()
        {
            // 48-57
            // 65-90
            StringBuilder sb = new StringBuilder();

            int index = R.Next(0, chs.Length - 1);
            char sheng = chs[index];
            sb.Append(sheng);
            for (int i = 0; i < 6; i++)
            {
                if (R.Next(0, 2) == 0)
                {
                    int t = R.Next(48, 58);
                    sb.Append((char)t);
                }
                else
                {
                    int t = R.Next(65, 91);
                    sb.Append((char)t);
                }
            }

            return sb.ToString();
        }

        public static string GetClaimNumber()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 21; i++)
            {
                sb.Append(R.Next(0, 9));
            }
            return sb.ToString();
        }

        public static List<Claim> GetClaims(int count)
        {
            List<Claim> cls = new List<Claim>();

            for (int i = 0; i < count; i++)
            {
                cls.Add(new Claim()
                {
                    ClaimNumber = GetClaimNumber(),
                    VehicleRegistrationNumber = GetVehicleRegistrationNumber()
                });
            }
            return cls;
        }

        public static Claim GetClaim()
        {
            return new Claim()
            {
                ClaimNumber = GetClaimNumber(),
                VehicleRegistrationNumber = GetVehicleRegistrationNumber()
            };
        }

        public static Node GetNode()
        {
            return new Node()
            {
                ClaimInfo = DataProvider.GetClaim()
            };
        }

        public static List<Node> GetNodes(int count)
        {
            List<Node> list = new List<Node>();
            for (int i = 0; i < count; i++)
            {
                list.Add(GetNode());
            }
            return list;
        }

        public static List<Claim> GetClaims(int count, string claimNumber)
        {
            List<Claim> cls = new List<Claim>();

            for (int i = 0; i < count; i++)
            {
                cls.Add(new Claim()
                {
                    ClaimNumber = claimNumber,
                    VehicleRegistrationNumber = GetVehicleRegistrationNumber()
                });
            }
            return cls;
        }
    }
}
