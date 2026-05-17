namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public interface IPayloadMapper
{
    Stream MapPayload(Stream inputPayload, string mappingId);
    Stream MapPayload(string inputJson, string mappingId);
}