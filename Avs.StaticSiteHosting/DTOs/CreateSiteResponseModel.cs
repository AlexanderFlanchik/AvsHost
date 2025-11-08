using System;
using System.Collections.Generic;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class CreateSiteResponseModel
    {
        /// <summary>
        /// Site name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Site description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Site Id
        /// </summary>
        public string SiteId { get; set; }

        /// <summary>
        /// Check if the site is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Site creator's name
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets site launch date time
        /// </summary>
        public DateTime? LaunchedOn { get; set; }

        /// <summary>
        /// Gets or sets a collection of resource mappings
        /// </summary>
        public IDictionary<string, string> Mappings { get; set; }

        /// <summary>
        /// Site landing page
        /// </summary>
        public string LandingPage { get; set; }
        
        /// <summary>
        /// Site database (MongoDb)
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets a collection with site tag Ids
        /// </summary>
        public string[] TagIds { get; set; }

        /// <summary>
        /// Gets or sets a collection with site uploaded files
        /// </summary>
        public ContentItemModel[] Uploaded { get; set; }
    }

    public record ContentFileModel(
        string Id,
        string Name, 
        string ContentType, 
        DateTime UploadedAt, 
        string FullName, 
        decimal Size, 
        DateTime? UpdateDate);
}