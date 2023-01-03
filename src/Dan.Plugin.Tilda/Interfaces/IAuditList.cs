using System;
using System.Collections.Generic;
using System.Text;
using Dan.Plugin.Tilda.Models.Enums;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface IAuditList
    {
        public void SetStatusAndTextAndOwner(string statusText, StatusEnum status, string owner);

        public string GetOwner();
    }
}
