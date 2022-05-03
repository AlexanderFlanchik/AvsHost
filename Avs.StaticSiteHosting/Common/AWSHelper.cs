using Amazon;
using System.Collections.Generic;
using System.Linq;

namespace Avs.StaticSiteHosting.Web.Common
{
    public static class AWSHelper
    {
        public static IDictionary<string, string> GetAWSRegions()
        {
            var regions = RegionEndpoint.EnumerableAllRegions
                
                .ToDictionary(k => k.SystemName, v => v.DisplayName);

            return regions;
        }
    }
}