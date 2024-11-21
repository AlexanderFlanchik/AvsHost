namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class UpdateSiteResponseModel
    {
        /// <summary>
        /// Returns a collection of the files related to site
        /// </summary>
        public ContentItemModel[] Uploaded { get; set; }
    }
}