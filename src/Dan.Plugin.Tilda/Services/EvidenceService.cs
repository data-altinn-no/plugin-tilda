using System.Collections.Generic;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;

namespace Dan.Plugin.Tilda.Services;

public interface IEvidenceService
{
    List<EvidenceValue> BuildEvidenceValue<T>(string evidenceCodeName, string evidenceValueName, T input,
        string source);
}

public class EvidenceService : IEvidenceService
{
    private readonly IEvidenceSourceMetadata _metadata;

    public EvidenceService(IEvidenceSourceMetadata metadata)
    {
        _metadata = metadata;
    }

    public List<EvidenceValue> BuildEvidenceValue<T>(string evidenceCodeName, string evidenceValueName, T input, string source)
    {
        var ecb = new EvidenceBuilder(_metadata, evidenceCodeName);

        ecb.AddEvidenceValue(evidenceValueName, input, source, false);

        return ecb.GetEvidenceValues();
    }
}
