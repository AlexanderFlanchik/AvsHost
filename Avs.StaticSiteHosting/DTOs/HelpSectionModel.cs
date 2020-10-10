using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.DTOs
{
    public class HelpSectionModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<HelpSectionModel> Sections { get; set; } 

        public HelpSectionModel()
        {
            Sections = new List<HelpSectionModel>();
        }

        public string[] RolesAllowed { get; set; }
    }
}
