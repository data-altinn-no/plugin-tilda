using Dan.Common.Models;
using System.Linq;

namespace Dan.Plugin.Tilda.Extensions
{
    public static class EvidenceHarvesterRequestExtension
    {
        /// <summary>
        /// Gets a parameter value as the requested type if set 
        /// </summary>
        /// <typeparam name="T">The requested type to attempt to cast the value to</typeparam>
        /// <param name="ehr">The evidence harvester request</param>
        /// <param name="paramName">The parameter name</param>
        /// <returns>The parameter value</returns>
        public static T GetOptionalParameterValue<T>(this EvidenceHarvesterRequest ehr, string paramName)
        {
            var value = ehr.GetOptionalParameter(paramName);
            if (value == null) return default;
            return (T) value.Value;

        }

        /// <summary>
        /// Gets a parameter if set
        /// </summary>
        /// <param name="ehr">The evidence harvester request</param>
        /// <param name="paramName">The parameter name</param>
        /// <returns>The parameter</returns>
        public static EvidenceParameter GetOptionalParameter(this EvidenceHarvesterRequest ehr, string paramName)
        {
            return ehr.Parameters?.FirstOrDefault(x => x.EvidenceParamName == paramName);
        }
    }
}
