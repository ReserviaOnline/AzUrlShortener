using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cloud5mins.domain
{
    public static class Utility
    {
        private const string ConversionCode = @"y<3c$9^hv%,%s5FHC]0VU6xs!hoT5K?/#KdS)X}*]@Kb(]z)b5O:6SF5\KkVxard";
        private static readonly int Base = ConversionCode.Length;
        //sets the length of the unique code to add to vanity
        private const int MinVanityCodeLength = 5;

        public static async Task<string> GetValidEndUrl(string vanity, StorageTableHelper stgHelper)
        {
            if (string.IsNullOrEmpty(vanity))
            {
                var newKey = await stgHelper.GetNextTableId();
                string getCode() => Encode(newKey);
                if (await stgHelper.IfShortUrlEntityExistByVanity(getCode()))
                    return await GetValidEndUrl(vanity, stgHelper);
              
                return string.Join(string.Empty, getCode());
            }
            else
            {
                return string.Join(string.Empty, vanity);
            }
        }

        public static string Encode(int i)
        {
            if (i == 0)
                return ConversionCode[0].ToString();

            return GenerateUniqueRandomToken(i);
        }

        public static string GetShortUrl(string host, string vanity)
        {
            return host + "/" + vanity;
        }

        // generates a unique, random, and alphanumeric token for the use as a url 
        //(not entirely secure but not sequential so generally not guessable)
        public static string GenerateUniqueRandomToken(int uniqueId)
        {
            using (var generator = new RNGCryptoServiceProvider())
            {
                //minimum size I would suggest is 5, longer the better but we want short URLs!
                var bytes = new byte[MinVanityCodeLength];
                generator.GetBytes(bytes);
                var chars = bytes
                    .Select(b => ConversionCode[b % ConversionCode.Length]);
                var token = new string(chars.ToArray());
                var reversedToken = string.Join(string.Empty, token.Reverse());
                return uniqueId + reversedToken;
            }
        }

        public static IActionResult CatchUnauthorize(ClaimsPrincipal principal, ILogger log)
        {
            if (principal == null)
            {
                log.LogWarning("No principal.");
                return new UnauthorizedResult();
            }

            if (principal.Identity == null)
            {
                log.LogWarning("No identity.");
                return new UnauthorizedResult();
            }

            if (!principal.Identity.IsAuthenticated)
            {
                log.LogWarning("Request was not authenticated.");
                return new UnauthorizedResult();
            }
            return null;
        }
    }
}
