using Avs.StaticSiteHosting.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Models
{
    public class Site : BaseEntity
    {
        public string Description { get; set; }
        public User CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LaunchedOn { get; set; }
        public IDictionary<string, string> Mappings { get; set; }
    }
}
