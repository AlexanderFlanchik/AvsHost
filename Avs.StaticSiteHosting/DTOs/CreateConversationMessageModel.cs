using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    /// <summary>
    /// DTO for user-administration conversation message.
    /// </summary>
    public class CreateConversationMessageModel
    {
        [Required]
        public string ConversationId { get; set; }
        
        [Required]
        public string Content { get; set; }
    }
}