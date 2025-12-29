using System.Collections.Generic;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class UpdateSiteResponseModel
    {
        /// <summary>
        /// Returns a collection of the files related to site
        /// </summary>
        public ContentItemModel[] Uploaded { get; set; }

        /// <summary>
        /// Returns a collection of custom route handlers associated with the site
        /// </summary>
        public List<CustomRouteHandlerModel> CustomRouteHandlers { get; set; } = [];
    }
}