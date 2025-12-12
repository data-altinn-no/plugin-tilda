using Dan.Tilda.Models.Enums;

namespace Dan.Tilda.Models.Audits;

public interface IAuditList
{
    public void SetStatusAndTextAndOwner(string statusText, StatusEnum status, string owner);

    public string GetOwner();
}
