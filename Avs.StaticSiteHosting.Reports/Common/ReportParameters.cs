namespace Avs.StaticSiteHosting.Reports.Common
{
    /// <summary>
    /// Report parameters
    /// </summary>
    public class ReportParameters
    {
        private IDictionary<string, object> _parameters = new Dictionary<string, object>();
        
        /// <summary>
        /// Represents site owner ID
        /// </summary>
        public string? SiteOwnerId { get; set; }

        /// <summary>
        /// Filters
        /// </summary>
        /// <param name="Filter name"></param>
        /// <returns>Filter value</returns>
        public object this[string key] 
        { 
            get => _parameters[key]; 
            set => _parameters[key] = value;
        }

        public void ApplyFilters(IDictionary<string, object> filters)
        {
            foreach (var filter in filters)
            {
                _parameters[filter.Key] = filter.Value;
            }
        }
    }
}