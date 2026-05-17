using Avs.StaticSiteHosting.GenAiApi.Models;

namespace Avs.StaticSiteHosting.GenAiApi.Common;

public static class LlmFeatureMapper
{
    public static string[] MapFromModelFeatures(LlmFeature[] features)
    {
        return features.Select(f => f.ToString().ToLowerInvariant()).ToArray();
    }

    public static LlmFeature[] MapToModelLlmFeatures(string[] features)
    {
        return features.Where(f => Enum.TryParse<LlmFeature>(f, ignoreCase: true, out _))
            .Select(f => Enum.Parse<LlmFeature>(f, true))
            .ToArray();
    }
}