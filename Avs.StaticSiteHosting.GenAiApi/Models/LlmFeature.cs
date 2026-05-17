using System.ComponentModel;

namespace Avs.StaticSiteHosting.GenAiApi.Models;

public enum LlmFeature
{
    [Description("Chat completion")]
    Chat,
    
    [Description("Embedding")]
    Embedding,
    
    [Description("Moderation")]
    Moderation,
    
    [Description("Image")]
    Image,
    
    [Description("Audio")]
    Audio,
    
    [Description("Reasoning")]
    Reasoning,
}