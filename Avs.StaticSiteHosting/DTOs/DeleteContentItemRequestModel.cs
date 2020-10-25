using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class DeleteContentItemRequestModel
    {
        public string ContentItemId { get; set; }   // for existed files
        public string ContentItemName { get; set; } // for just uploaded files
    }
}
