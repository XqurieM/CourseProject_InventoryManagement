using CourseProject_InventoryManagement.Domain.Entities;
using CourseProject_InventoryManagement.Domain.Enums;
using CourseProject_InventoryManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CourseProject_InventoryManagement.Domain.Services
{
    public class CustomIdGenerator : ICustomIdGenerator
    {
        public string Generate(List<InventoryCustomIdRule> rules, int currentCount)
        {
            var sb = new StringBuilder();

            foreach (var rule in rules.OrderBy(x => x.PartOrder))
            {
                switch (rule.PartType)
                {
                    case CustomIdPartType.StaticText:
                        sb.Append(rule.StaticTextValue);
                        break;

                    case CustomIdPartType.DateTime:
                        sb.Append(DateTime.UtcNow.ToString(rule.Format ?? "yyyyMMdd"));
                        break;

                    case CustomIdPartType.Sequence:
                        sb.Append((currentCount + 1).ToString(rule.Format ?? "D3"));
                        break;

                    case CustomIdPartType.Guid:

                        sb.Append(Guid.NewGuid().ToString(rule.Format ?? "N"));
                        break;

                    case CustomIdPartType.Random6Digit:
                        sb.Append(RandomNumberGenerator.GetInt32(100000, 999999));
                        break;

                    case CustomIdPartType.Random9Digit:
                        sb.Append(RandomNumberGenerator.GetInt32(100000000, 999999999));
                        break;

                    case CustomIdPartType.Random20Bit:
                        sb.Append(GenerateRandomString(20)); 
                        break;

                    case CustomIdPartType.Random32Bit:
                        sb.Append(GenerateRandomString(32)); 
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(rule.PartType), "Tanımlanmamış kural tipi.");
                }
            }

            return sb.ToString();
        }


        private string GenerateRandomString(int bits)
        {
            int byteCount = (int)Math.Ceiling(bits / 8.0);
            byte[] buffer = new byte[byteCount];
            RandomNumberGenerator.Fill(buffer);

            string hex = Convert.ToHexString(buffer);
            int charLength = (int)Math.Ceiling(bits / 4.0); 
            return hex.Substring(0, charLength).ToLower();
        }
    }
}
