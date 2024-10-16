using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using JsonSchema = NJsonSchema.JsonSchema;
using Azure.Core.Serialization;
using Dan.Plugin.Tilda.Models.AlertMessages;

namespace Dan.Plugin.Tilda
{
    public class Metadata : IEvidenceSourceMetadata
    {

        const string mpScope = "brreg:tilda";
        private const string enhetsinfo_schema = "enhetsinformasjon_schema.json";

        private const string TILDA_SCOPE = "altinn:dataaltinnno/tilda";
        private Settings _settings;
        public static List<string> belongsToTilda = new List<string>() { "Tilda" };

        public const string SourceEnhetsregisteret = "Tilsynsdata";

        public const int ERROR_ORGANIZATION_NOT_FOUND = 1;

        public const int ERROR_CCR_UPSTREAM_ERROR = 2;

        public const int ERROR_NO_REPORT_AVAILABLE = 3;

        public const int ERROR_ASYNC_REQUIRED_PARAMS_MISSING = 4;

        public const int ERROR_ASYNC_ALREADY_INITIALIZED = 5;

        public const int ERROR_ASYNC_NOT_INITIALIZED = 6;

        public const int ERROR_AYNC_STATE_STORAGE = 7;

        public const int ERROR_ASYNC_HARVEST_NOT_AVAILABLE = 8;

        public const int ERROR_CERTIFICATE_OF_REGISTRATION_NOT_AVAILABLE = 9;

        public const int TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS = 40;

        public Metadata(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req, FunctionContext context)
        {
            var logger = context.GetLogger(context.FunctionDefinition.Name);
            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(GetEvidenceCodes(), new NewtonsoftJsonObjectSerializer(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }));
            return response;
        }

        public List<EvidenceCode> GetEvidenceCodes()
        {
            var a = new List<EvidenceCode>();

            a.AddRange(GetTilsynsdataRapportMetadata());
            a.AddRange(GetTilsynsdataRapportAllMetadata());
            a.AddRange(GetTilsynskoordineringMetadata());
            a.AddRange(GetTilsynskoordineringAllMetadata());
            a.AddRange(GetTilsynsTrendMetadata());
            a.AddRange(GetTilsynsTrendMetadataAll());
            a.AddRange(GetNPDIDMetadata());
            a.AddRange(GetTildaMetadataMetadata());
            a.AddRange(GetTildaStorulykkeMetadata());
            a.AddRange(GetTildaStorulykkeMetadataAlle());
            a.AddRange(GetPdfReportMetadata());
            // *** TEMPORARY TILDA FILTERING ***
            if (_settings.IsTest)
            {
                a.AddRange(GetTildaMeldingTilAnnenMyndighetMetadata());
            }

            return a;
        }

        private static IEnumerable<EvidenceCode> GetPdfReportMetadata()
        {
            var a = new EvidenceCode()
            {
                Description = "TildaTilsynsrapportpdfv1",
                EvidenceCodeName = "TildaTilsynsrapportpdfv1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                BelongsToServiceContexts = belongsToTilda,
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaPdfReport>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "pdfrapport",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.Attachment
                    }
                },
                Parameters = new List<EvidenceParameter>()
                {
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "internTilsynsId",
                        ParamType = EvidenceParamType.String,
                        Required = true
                    }
                }
            };

            return new List<EvidenceCode>()
            {
                a
            };
        }

        private static List<EvidenceParameter> GetTildaAllParameters()
        {
            return new List<EvidenceParameter>()
            {
                new EvidenceParameter()
                {
                    EvidenceParamName = "maaned",
                    ParamType = EvidenceParamType.String,
                    Required = true
                },
                new EvidenceParameter()
                {
                    EvidenceParamName = "aar",
                    ParamType = EvidenceParamType.String,
                    Required = true
                },
                new EvidenceParameter()
                {
                    EvidenceParamName = "filter",
                    ParamType = EvidenceParamType.String,
                    Required = false
                }
            };
        }

        private static List<EvidenceParameter> GetTildaAllParametersWithGeoSearch()
        {
            var parameters = GetTildaAllParameters();
            var geoSearchParameters = new List<EvidenceParameter>
            {
                new()
                {
                    EvidenceParamName = "postnummer",
                    ParamType = EvidenceParamType.Number,
                    Required = false
                },
                new()
                {
                    EvidenceParamName = "kommunenummer",
                    ParamType = EvidenceParamType.Number,
                    Required = false
                },
                new()
                {
                    EvidenceParamName = "naeringskode",
                    ParamType = EvidenceParamType.String,
                    Required = false
                }
            };
            parameters.AddRange(geoSearchParameters);
            return parameters;
        }

        private static IEnumerable<EvidenceCode> GetTildaStorulykkeMetadataAlle()
        {
            var schema = JsonSchema.FromType<StorulykkevirksomhetListe>().ToJson(Formatting.Indented);

            var a = new EvidenceCode()
            {
                Description = "TildaStorulykkevirksomhetAlle",
                EvidenceCodeName = "TildaStorulykkevirksomhetAlle",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                BelongsToServiceContexts = belongsToTilda,
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaAlertMessage>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "StorulykkevirksomheterParagraf6",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "StorulykkevirksomheterParagraf9",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    }
                },
            };

            return new List<EvidenceCode>()
            {
                a
            };
        }

        private static IEnumerable<EvidenceCode> GetTildaStorulykkeMetadata()
        {
            var schema = JsonSchema.FromType<StorulykkevirksomhetKontroll>().ToJson(Formatting.Indented);

            var a = new EvidenceCode()
            {
                Description = "TildaStorulykkevirksomhet",
                EvidenceCodeName = "TildaStorulykkevirksomhet",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                BelongsToServiceContexts = belongsToTilda,
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaAlertMessage>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "Storulykkevirksomhet",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    }
                },
            };

            return new List<EvidenceCode>()
            {
                a
            };
        }

        private static List<EvidenceCode> GetTildaMeldingTilAnnenMyndighetMetadata()
        {
            var schema = JsonSchema.FromType<AlertMessageList>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var a = new EvidenceCode()
            {
                Description = "TildaMeldingTilAnnenMyndighet v1",
                EvidenceCodeName = "TildaMeldingTilAnnenMyndighetv1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                BelongsToServiceContexts = belongsToTilda,
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaAlertMessage>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "meldingTilAnnenMyndighet",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    }
                },
                Parameters = new List<EvidenceParameter>()
                {
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "identifikator",
                        ParamType = EvidenceParamType.String,
                        Required = true
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "tilsynskilder",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    }
                },
            };

            return new List<EvidenceCode>()
            {
                a
            };
        }

        private static IEnumerable<EvidenceCode> GetTildaMetadataMetadata()
        {

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "Informasjon om datakilder i Tilda",
                EvidenceCodeName = "TildaMetadatav1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "",
                ServiceContext = "Tilda",
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaAuditCoordinationAll>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaTrendrapportv1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaTrendrapportAllev1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaTilsynsrapportv1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaTilsynsrapportAllev1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaTilsynskoordineringv1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaTilsynskoordineringAllev1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaTilsynsrapportpdfv1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "AlleKilder",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaNPDIDv1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaMeldingTilAnnenMyndighetv1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    }
                }
            };

            return new List<EvidenceCode>()
            {
                a
            };

        }

        private static IEnumerable<EvidenceCode> GetTilsynsTrendMetadataAll()
        {
            var schema = JsonSchema.FromType<AuditCoordinationList>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var schemaER = JsonSchema.FromType<TildaRegistryEntry>().ToJson(Newtonsoft.Json.Formatting.Indented);

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "Tilsynstrender v1",
                EvidenceCodeName = "TildaTrendrapportAllev1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                ServiceContext = "Tilda",
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaTrendReportsAll>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "tilsynstrendrapporter",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    }
                },
                Parameters = GetTildaAllParametersWithGeoSearch(),
            };

            return new List<EvidenceCode>
            {
                a
            };
        }

        private static IEnumerable<EvidenceCode> GetTilsynskoordineringAllMetadata()
        {
            var schema = JsonSchema.FromType<AuditCoordination>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var schemaER = JsonSchema.FromType<TildaRegistryEntry>().ToJson(Newtonsoft.Json.Formatting.Indented);

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "Tilsynskoordinering v1",
                EvidenceCodeName = "TildaTilsynskoordineringAllev1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                ServiceContext = "Tilda",
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaAuditCoordinationAll>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "tilsynskoordineringer",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    }
                },
                Parameters = GetTildaAllParametersWithGeoSearch()
            };

            return new List<EvidenceCode>
            {
                a
            };
        }

        private static IEnumerable<EvidenceCode> GetNPDIDMetadata()
        {
            var schema = JsonSchema.FromType<NPDIDAuditReportList>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var schemaER = JsonSchema.FromType<TildaRegistryEntry>().ToJson(Newtonsoft.Json.Formatting.Indented);

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "NPDID-rapporter v1",
                EvidenceCodeName = "TildaNPDIDv1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                ServiceContext = "Tilda",
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaNPDIDAuditReports>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "tilsynsrapporter",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "enhetsinformasjon",
                        Source = "Enhetsregisteret",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schemaER
                    }
                },
                Parameters = new List<EvidenceParameter>()
                {
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "startdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "sluttdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "tilsynskilder",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "npdid",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "inkluderUnderenheter",
                        ParamType = EvidenceParamType.Boolean,
                        Required = false
                    }
                },
            };

            return new List<EvidenceCode>
            {
                a
            };
        }

        private static IEnumerable<EvidenceCode> GetTilsynsTrendMetadata()
        {
            var schema = JsonSchema.FromType<AuditCoordinationList>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var schemaER = JsonSchema.FromType<TildaRegistryEntry>().ToJson(Newtonsoft.Json.Formatting.Indented);

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "Tilsynstrender v1",
                EvidenceCodeName = "TildaTrendrapportv1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                ServiceContext = "Tilda",
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaTrendReports>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "tilsynstrendrapporter",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "enhetsinformasjon",
                        Source = "Enhetsregisteret",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schemaER
                    }
                },
                Parameters = new List<EvidenceParameter>()
                {
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "startdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "sluttdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "tilsynskilder",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "inkluderUnderenheter",
                        ParamType = EvidenceParamType.Boolean,
                        Required = false
                    }
                },
            };

            return new List<EvidenceCode>
            {
                a
            };
        }

        private static List<Requirement> GetTildaAuthRequirements<T>()
        {
            return new List<Requirement>()
            {
                new MaskinportenScopeRequirement()
                {
                    RequiredScopes = new List<string> { TILDA_SCOPE }
                },
                new AccreditationPartyRequirement()
                {
                    PartyRequirements = new List<AccreditationPartyRequirementType>()
                    {
                        AccreditationPartyRequirementType.RequestorAndOwnerAreEqual
                    }
                }
            };
        }

        public static List<EvidenceCode> GetTilsynskoordineringMetadata()
        {
            var schema = JsonSchema.FromType<AuditCoordination>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var schemaER = JsonSchema.FromType<TildaRegistryEntry>().ToJson(Newtonsoft.Json.Formatting.Indented);

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "Tilsynskoordinering v1",
                EvidenceCodeName = "TildaTilsynskoordineringv1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                ServiceContext = "Tilda",
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaAuditCoordination>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "tilsynskoordineringer",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    },
                    new EvidenceValue()
                    {
                    EvidenceValueName = "enhetsinformasjon",
                    Source = "Enhetsregisteret",
                    ValueType = EvidenceValueType.JsonSchema,
                    JsonSchemaDefintion = schemaER
                    }
                },
                Parameters = new List<EvidenceParameter>()
                {
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "startdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "sluttdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "tilsynskilder",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "inkluderUnderenheter",
                        ParamType = EvidenceParamType.Boolean,
                        Required = false
                    }
                },
            };

            return new List<EvidenceCode>
            {
                a
            };
        }

        public static List<EvidenceCode> GetTilsynsdataRapportAllMetadata()
        {
            var schema = JsonSchema.FromType<AuditReportList>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var schemaER = JsonSchema.FromType<TildaRegistryEntry>().ToJson(Newtonsoft.Json.Formatting.Indented);

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "Tilsynsdata fra valgt kilde innenfor gitt datospenn",
                EvidenceCodeName = "TildaTilsynsrapportAllev1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                ServiceContext = "Tilda",
                AuthorizationRequirements = new List<Requirement>(),
                MaxValidDays = 365,
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "tilsynsrapporter",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    }
                },
                Parameters = GetTildaAllParametersWithGeoSearch()
            };

            a.AuthorizationRequirements.AddRange(GetTildaAuthRequirements<ITildaAuditReportsAll>());

            return new List<EvidenceCode>(){a};
        }

        public static List<EvidenceCode> GetTilsynsdataRapportMetadata()
        {
            var schema = JsonSchema.FromType<AuditReportList>().ToJson(Newtonsoft.Json.Formatting.Indented);
            var schemaER = JsonSchema.FromType<TildaRegistryEntry>().ToJson(Newtonsoft.Json.Formatting.Indented);

            var a = new EvidenceCode()
            {
                BelongsToServiceContexts = belongsToTilda,
                Description = "Tilsynsdata fra valgte kilder innenfor gitt datospenn, tilsynskilder er kommaseparert liste med orgnr for tilsynsmyndigheter",
                EvidenceCodeName = "TildaTilsynsrapportv1",
                EvidenceSource = "Tilda",
                IsAsynchronous = false,
                RequiredScopes = "brreg:tilda",
                ServiceContext = "Tilda",
                MaxValidDays = 365,
                AuthorizationRequirements = GetTildaAuthRequirements<ITildaAuditReports>(),
                Timeout = TILDA_CANCELLATION_TOKEN_TIMEOUT_SECONDS,
                Values = new List<EvidenceValue>()
                {
                    new EvidenceValue()
                    {
                        EvidenceValueName = "tilsynsrapporter",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schema
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "enhetsinformasjon",
                        Source = "Enhetsregisteret",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = schemaER
                    }

                },
                Parameters = new List<EvidenceParameter>()
                {
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "startdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "sluttdato",
                        ParamType = EvidenceParamType.DateTime,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "tilsynskilder",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    },
                    new EvidenceParameter()
                    {
                        EvidenceParamName = "inkluderUnderenheter",
                        ParamType = EvidenceParamType.Boolean,
                        Required = false
                    }
                },
            };

            return new List<EvidenceCode>
            {
                a
            };
        }
    }
}
