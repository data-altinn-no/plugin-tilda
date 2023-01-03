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
using Microsoft.Extensions.Options;
using JsonSchema = NJsonSchema.JsonSchema;

namespace Dan.Plugin.Tilda
{
    public class Metadata
    {

        const string mpScope = "brreg:tilda";
        private const string enhetsinfo_schema = "enhetsinformasjon_schema.json";

        private const string TILDA_SCOPE = "altinn:dataaltinnno/tilda";
        private Settings _settings;
        public static List<string> belongsToTilda = new List<string>() { "Tilda" };

        public Metadata(Settings settings)
        {
            _settings = settings;
        }

        [Function(Constants.EvidenceSourceMetadataFunctionName)]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var response = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(await GetEvidenceCodes(), typeof(List<EvidenceCode>), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore
                })),
                RequestMessage = req
            };


            return response;
        }

        public async Task<List<EvidenceCode>> GetEvidenceCodes()
        {
            var a = new List<EvidenceCode>();

            a.AddRange(await GetTilsynsdataRapportMetadata());
            a.AddRange(await GetTilsynsdataRapportAllMetadata());
            a.AddRange(await GetTilsynskoordineringMetadata());
            a.AddRange(await GetTilsynskoordineringAllMetadata());
            a.AddRange(await GetTilsynsTrendMetadata());
            a.AddRange(await GetTilsynsTrendMetadataAll());
            a.AddRange(await GetNPDIDMetadata());
            a.AddRange(await GetTildaMetadataMetadata());
            a.AddRange(await GetTildaStorulykkeMetadata());
            a.AddRange(await GetTildaStorulykkeMetadataAlle());
            // *** TEMPORARY TILDA FILTERING ***
            if (_settings.IsTest)
            {
                a.AddRange(await GetTildaMeldingTilAnnenMyndighetMetadata());
            }

            return a;
        }

        private static async Task<IEnumerable<EvidenceCode>> GetTildaStorulykkeMetadataAlle()
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

        private static async Task<IEnumerable<EvidenceCode>> GetTildaStorulykkeMetadata()
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

        private static async Task<List<EvidenceCode>> GetTildaMeldingTilAnnenMyndighetMetadata()
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
                        EvidenceParamName = "identifikator",
                        ParamType = EvidenceParamType.String,
                        Required = false
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

        private static async Task<IEnumerable<EvidenceCode>> GetTildaMetadataMetadata()
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
                        EvidenceValueName = "AlleKilder",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue()
                    {
                        EvidenceValueName = "TildaNPDIDv1",
                        Source = "Tilda",
                        ValueType = EvidenceValueType.String
                    }
                }
            };

            return await Task.FromResult(new List<EvidenceCode>
            {
                a
            });

        }

        private static async Task<IEnumerable<EvidenceCode>> GetTilsynsTrendMetadataAll()
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
                        EvidenceParamName = "filter",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    }
                },
            };

            return new List<EvidenceCode>
            {
                a
            };
        }

        private static async Task<IEnumerable<EvidenceCode>> GetTilsynskoordineringAllMetadata()
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
                        EvidenceParamName = "filter",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    }
                },
            };

            return new List<EvidenceCode>
            {
                a
            };
        }

        private static async Task<IEnumerable<EvidenceCode>> GetNPDIDMetadata()
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

        private static async Task<IEnumerable<EvidenceCode>> GetTilsynsTrendMetadata()
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

        public static async Task<List<EvidenceCode>> GetTilsynskoordineringMetadata()
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

        private static async Task<JSchema> GetJSchema<T>(string name, string fileName = "")
        {
            if (fileName.Equals(string.Empty))
            {
                JSchemaGenerator generator = new JSchemaGenerator();
                generator.GenerationProviders.Add(new StringEnumGenerationProvider());
                generator.SchemaIdGenerationHandling = SchemaIdGenerationHandling.TypeName;
                JSchema schema = generator.Generate(typeof(T));
                schema.SchemaVersion = new Uri("http://json-schema.org/draft-07/schema#");
                schema.Title = name;

                return schema;
            }
            else
            {
                return await LoadSchemaFromFile(fileName);
            }
        }

        private static async Task<JSchema> LoadSchemaFromFile(string name)
        {
            var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));
            return JSchema.Parse(await File.ReadAllTextAsync(rootDirectory + $@"\JsonSchemas\{name}"));
        }

        public static async Task<List<EvidenceCode>> GetTilsynsdataRapportAllMetadata()
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
                        EvidenceParamName = "filter",
                        ParamType = EvidenceParamType.String,
                        Required = false
                    }
                },
            };

            a.AuthorizationRequirements.AddRange(GetTildaAuthRequirements<ITildaAuditReportsAll>());

            return new List<EvidenceCode>(){a};
        }

        public static async Task<List<EvidenceCode>> GetTilsynsdataRapportMetadata()
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

    public class EvidenceSourceMetadata : IEvidenceSourceMetadata
    {
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

        private Settings _settings;

        public EvidenceSourceMetadata(Settings settings)
        {
            _settings = settings;
        }

        public List<EvidenceCode> GetEvidenceCodes()
        {
            return (new Metadata(_settings)).GetEvidenceCodes().Result;
        }
    }

}
