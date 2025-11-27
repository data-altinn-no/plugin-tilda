using Dan.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits.Coordination;
using Dan.Tilda.Models.Audits.NPDID;
using Dan.Tilda.Models.Audits.Report;
using Dan.Tilda.Models.Audits.Trend;
using AlertCompact = Dan.Tilda.Models.Entities.AlertCompact;
using AlertFull = Dan.Tilda.Models.Entities.AlertFull;
using AlertMessage = Dan.Tilda.Models.Audits.Alerts.AlertMessage;
using AuditAddress = Dan.Tilda.Models.Entities.AuditAddress;
using Campaign = Dan.Tilda.Models.Entities.Campaign;
using ControlActivity = Dan.Tilda.Models.Entities.ControlActivity;
using ControlAttribute = Dan.Tilda.Models.Entities.ControlAttribute;
using ControlContact = Dan.Tilda.Models.Entities.ControlContact;
using ControlReactionDetails = Dan.Tilda.Models.Entities.ControlReactionDetails;
using CoordinatedControlAgency = Dan.Tilda.Models.Entities.CoordinatedControlAgency;
using PlannedControlActivity = Dan.Tilda.Models.Entities.PlannedControlActivity;
using Reaction = Dan.Tilda.Models.Entities.Reaction;
using Remark = Dan.Tilda.Models.Entities.Remark;

namespace Dan.Plugin.Tilda.Utils
{

    public class Mock
    {
        private string _mockActor;

        private string _organizationNumber;

        private int _digest = 0;

        public List<List<EvidenceValue>> PerhapsAddSomeMockAudits(
            List<List<EvidenceValue>> allResults,
            string mockActor,
            string organizationNumber)
        {
            this._mockActor = mockActor;
            this._organizationNumber = organizationNumber;
            this._digest = Digest();

            if (Rnd() > 80)
            {
                return allResults;
            }

            var mockedList = new List<EvidenceValue>();
            var audit = new Audit() { Audits = new List<AuditElement>() };
            for (var i = 0; i < this.Rnd(1, 4); i++)
            {
                var seed = i.ToString();
                var address = "";
                var locationOrg = "";

                // Generate new address, or use one of the existing locations?
                if (this.Rnd(seed: seed) > 90 || allResults.Count == 0)
                {
                    // Make random
                    address = DummyData.GetRandomAddress(Digest(seed));
                    locationOrg = this.Rnd(800000000, 999999999, seed).ToString();
                }
                else
                {
                    // Use a random already existing
                    var r = allResults.GetRange(this.Rnd(0, allResults.Count-1, seed), 1).First();
                    if (r.Count > 0)
                    {
                        var a = ((Audit) r.First().Value).Audits;

                        if (a.Count > 0)
                        {
                            var ae = a.GetRange(this.Rnd(0, a.Count - 1, seed + "1"), 1).First();
                            address = ae.Location;
                            locationOrg = ae.LocationOrg;
                        }
                        else
                        {
                            address = DummyData.GetRandomAddress(Digest(seed));
                            locationOrg = this.Rnd(800000000, 999999999, seed).ToString();
                        }
                    }

                }

        audit.Audits.Add(
                    new AuditElement()
                        {
                            AuditDate = DummyData.GenerateDummyDateTime(Digest(seed)),
                            AuditResult = Rnd(seed: seed) < 80 ? "Approved" : "ApprovedWithRemarks",
                            Location = address,
                            KnownAudit = true,
                            Auditor = this._mockActor,
                            LocationOrg = locationOrg
                    });
            }

            mockedList.Add(new EvidenceValue() { EvidenceValueName = "Audit", Value = audit });
            allResults.Add(mockedList);

            return allResults;

        }

        public async Task<List<AlertMessage>> GetMockAlertMessages(string orgno, string agency, string agencyname, string identifier)
        {
            var list = new List<AlertMessage>();
            list.Add(CreateMockAlertMessage(agency, orgno, Guid.NewGuid().ToString(), DummyData.GenerateDummyString(Digest())));

            list.Add(CreateMockAlertMessage(agency, orgno, Guid.NewGuid().ToString(), DummyData.GenerateDummyString(Digest())));
            list.Add(CreateMockAlertMessage(agency, orgno, Guid.NewGuid().ToString(), DummyData.GenerateDummyString(Digest())));
            list.Add(CreateMockAlertMessage(agency, orgno, Guid.NewGuid().ToString(), DummyData.GenerateDummyString(Digest())));

            return await Task.FromResult(list);
        }

        private AlertMessage CreateMockAlertMessage(string agency, string controlObject, string identifier, string message)
        {
            var a = new AlertMessage()
            {
                Id = identifier,
            };

            return a;
        }

        public async Task<List<NpdidAuditReport>> GetNpdidReports(string orgno, string agency, string agencyname, string npdid)
        {
            var list = new List<NpdidAuditReport>();
            list.Add(CreateMockNPDIDReport(orgno, agency, agencyname, npdid));

            return await Task.FromResult(list);
        }

        private NpdidAuditReport CreateMockNPDIDReport(string orgno, string agency, string agencyname, string npdid)
        {
            _digest = DummyData.GetDigest(DateTime.Now.Second.ToString());
            var a = new NpdidAuditReport()
            {
                Npdid = npdid,
                ControlObject = "974720760",
                ControlAgency = agency,
                ResponsibleAuditor = agency,
                ControlActivities = CreateMockControlActivities(orgno, agency, "12345678"),
                ControlAttributes = CreateMockControlAttributes(),
                AuditNotes = "Notater, notater, notater i lange baner",
                ControlLocations = CreateMockControlLocations(),
                ControlContacts = CreateMockControlContacts(),
                ViolationReactions = CreateMockReactions(),
                NotesAndRemarks = new List<Remark>() { new Remark() { RemarkMessage = "blabla", Remarkreference = 1 } }
            };

            return a;
        }

        public async Task<List<AuditReport>> GetMockAuditReports(string orgno, string agency, string agencyname)
        {
            var list = new List<AuditReport>();
            _digest = DummyData.GetDigest(DateTime.Now.Second.ToString());
            list.Add(CreateMockAuditReport(orgno, agency, agencyname));

            return await Task.FromResult(list);
        }

        public async Task<List<NpdidAuditReport>> GetMockNPDIDAuditReports(string orgno, string agency, string agencyname, string npdid)
        {
            var list = new List<NpdidAuditReport>();

            list.Add(CreateMockNPDIDReport(orgno, agency, agencyname, npdid));

            return await Task.FromResult(list);
        }

        public async Task<List<TrendReport>> GetMockTrendReports(string orgno, string agency, string agencyname)
        {
            var list = new List<TrendReport>();
            list.Add(createMockTrendReport(orgno, agency, agencyname));

            return await Task.FromResult(list);
        }

        public async Task<List<AuditCoordination>> GetMockCoordinationReports(string orgno, string agency, string agencyname)
        {
            var list = new List<AuditCoordination>();
            list.Add(CreateMockCoordinationReport(orgno, agency, agencyname));

            return await Task.FromResult(list);
        }

        public async Task<TildaRegistryEntry> GetMockTildaRegistryEntry()
        {
            var a = new TildaRegistryEntry()
            {
                OrganisationForm = "AS",
                Name = "Organisasjonen AS",
                Accounts = new AccountsInformation(),
                BusinessCode = "47.110",
                OperationalStatus = OperationStatus.OK,
                OrganizationNumber = "123456789",
                PublicLocationAddress = GetMockERAddress(),
                ControlObjectParent = "222222222"
            };

            return await Task.FromResult(a);
        }

        public async Task<List<AuditReport>> GetMockAuditReportsFromFile(string filename)
        {
            var list = new List<AuditReport>();
            AuditReport report = null;
            try
            {
                using (StreamReader file = File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    report = (AuditReport)serializer.Deserialize(file, typeof(AuditReport));
                }

                list.Add(report);
            }
            catch (Exception) {}

            return await Task.FromResult(list);
        }

        private TrendReport createMockTrendReport(string orgno, string agencyorgno, string agencyname)
        {
            var a = new TrendReport()
            {
                ControlObject = "974720760",
                ResponsibleAuditor = agencyorgno,
                AnnualTotals = GetMockAnnuals()
            };

            return a;
        }

        private List<Annual> GetMockAnnuals()
        {
            return new List<Annual>()
            {
                new Annual()
                {
                    Year = DateTime.Now.Year,
                    MonthsOfData = 11,
                    NumberOfAudits = 2,
                    NumberOfAuditsWithReactions = 1,
                    NumberOfAuditsWithoutReactions = 1,
                    NumberOfControls = 2,
                    NumberOfControlsWithReactions = 1,
                    NumberOfControlsWithoutReactions = 1,
                    NumberofAlerts = 1,
                    PoliceReactions = 1,
                    NumberOfRemarks = 2323

                },
                new Annual()
                {
                    Year = DateTime.Now.Year - 1,
                    MonthsOfData = 12,
                    NumberOfAudits = 3,
                    NumberOfAuditsWithReactions = 1,
                    NumberOfAuditsWithoutReactions = 1,
                    NumberOfControls = 2,
                    NumberOfControlsWithReactions = 1,
                    NumberOfControlsWithoutReactions = 1,
                    NumberofAlerts = 1,
                    PoliceReactions = 1,
                    NumberOfRemarks = 23
                },
                new Annual()
                {
                    Year = DateTime.Now.Year - 2,
                    MonthsOfData = 12,
                    NumberOfAudits = 1,
                    NumberOfAuditsWithReactions = 1,
                    NumberOfAuditsWithoutReactions = 1,
                    NumberOfControls = 2,
                    NumberOfControlsWithReactions = 1,
                    NumberOfControlsWithoutReactions = 1,
                    NumberofAlerts = 1,
                    PoliceReactions = 1,
                    NumberOfRemarks = 1
                },
                new Annual()
                {
                    Year = DateTime.Now.Year - 3,
                    MonthsOfData = 12,
                    NumberOfAudits = 12,
                    NumberOfAuditsWithReactions = 1,
                    NumberOfAuditsWithoutReactions = 1,
                    NumberOfControls = 2,
                    NumberOfControlsWithReactions = 1,
                    NumberOfControlsWithoutReactions = 1,
                    NumberofAlerts = 1,
                    PoliceReactions = 1,
                    NumberOfRemarks = 0
                },
                new Annual()
                {
                    Year = DateTime.Now.Year - 4,
                    MonthsOfData = 12,
                    NumberOfAudits = 21,
                    NumberOfAuditsWithReactions = 1,
                    NumberOfAuditsWithoutReactions = 1,
                    NumberOfControls = 2,
                    NumberOfControlsWithReactions = 1,
                    NumberOfControlsWithoutReactions = 1,
                    NumberofAlerts = 1,
                    PoliceReactions = 1,
                    NumberOfRemarks = 0
                }
            };
        }

        private AuditAddress GetMockAuditAddress()
        {
            return new AuditAddress()
            {
                AddressName = "Objektadresseveien 2",
                AddressNumber = "2",
                District = "Dal",
                BuildingNumber = "2",
                CountyNumber = "020",
                Latitude = "59.913868",
                Longtitude = "10.752245",
                LocationDescription = "Enda ei rønne",
                MunicipalityNumber = "0301",
                PostName = "Poststedsnavn",
                PostNumber = "Postnummer",
                UnitNumber = "Enhetsnummer",
                LocationKeywords = "key, word",
                LocationReference = 0
            };
        }

        private ERAddress GetMockERAddress()
        {
            return new ERAddress()
            {
                AddressName = "Objektadresseveien 2",
                AddressNumber = "2",
                District = "Dal",
                BuildingNumber = "2",
                CountyNumber = "020",
                Latitude = "59.913868",
                Longtitude = "10.752245",
                MunicipalityNumber = "0301",
                PostName = "Poststedsnavn",
                PostNumber = "Postnummer",
                UnitNumber = "Enhetsnummer",
            };
        }

        private AuditCoordination CreateMockCoordinationReport(string orgno, string agencyorgno, string agencyname)
        {
            var a = new AuditCoordination()
            {
                ControlObject = orgno,
                ControlAgency = agencyorgno,
                ResponsibleAuditor = "223344556",
                ControlLocations = CreateMockControlLocations(),
                PlannedControlContact = CreateMockControlContacts(),
                PlannedControlActivities = new List<PlannedControlActivity>()
                {
                    new PlannedControlActivity()
                    {
                        Date = DateTime.Now.AddDays(22),
                        Days = 1,
                        Activity = "aktivitet",
                        ActivityExecutionType = "aktivitetsutførelsestype",
                        CoordinatedControl = new List<CoordinatedControlAgency>()
                        {
                            new CoordinatedControlAgency()
                            {
                                ActivityExecution = "aktivitetseksekvering",
                                ControlAgency = "123456788",
                                ControlTopic = "tema"
                            }
                        },
                        Topic = "tema"

                    }
                },
                ControlCampaigns = new List<Campaign>()
                {
                    new Campaign()
                    {
                        Name = "Laktosesjekk",
                        Description = "Tilsyn av iskremselgere",
                        StartDate = DateTime.Now.AddDays(20),
                        EndDate = DateTime.Now.AddDays(21)
                    }
                },
                Alerts = CreateMockAlertFullMessages("12345678"),
                CurrentControls = 12

            };

            return a;
        }

        private TildaRegistryEntry CreateMockBRData(string name, string orgNumber, string orgForm)
        {
            return new TildaRegistryEntry()
            {
                Name = name,
                OrganizationNumber = orgNumber,
                OrganisationForm = orgForm,
                PublicLocationAddress = new ERAddress()
                {
                    AddressName = "Objektadresseveien 2",
                    AddressNumber = "2",
                    District = "Dal",
                    BuildingNumber = "x",
                    CountyNumber = "020",
                    Latitude = "59.913868",
                    Longtitude = "10.752245",
                    MunicipalityNumber = "0301",
                    PostName = "Poststedsnavn",
                    PostNumber = "Postnummer",
                    UnitNumber = "Enhetsnummer"
                },
                BusinessCode = "NACE",
                Accounts = new AccountsInformation(),
                OperationalStatus = OperationStatus.OK,
            };
        }

        private AuditReport CreateMockAuditReport(string orgno, string agencyorgno, string agencyname)
        {
            var a = new AuditReport()
            {
                ResponsibleAuditor = agencyorgno,
                ControlAgency = agencyorgno,
                ControlObject = orgno,
                ControlActivities = CreateMockControlActivities(orgno,agencyorgno,"12345678"),
                ControlAttributes = CreateMockControlAttributes(),
                AuditNotes = "Notater, notater, notater i lange baner",
                ControlLocations = CreateMockControlLocations(),
                ControlContacts = CreateMockControlContacts(),
                ViolationReactions = CreateMockReactions(),
                NotesAndRemarks = new List<Remark>() { new Remark() { RemarkMessage = "blabla", Remarkreference = 1 } }
            };


            return a;
        }

        private List<Reaction> CreateMockReactions()
        {
            return new List<Reaction>()
            {
                CreateMockReaction()
            };
        }

        private Reaction CreateMockReaction()
        {
            return new Reaction()
            {
                ControlActivityReference = (int)DummyData.GenerateDummyNumber(_digest),
                ControlLocationReference = (int)DummyData.GenerateDummyNumber(_digest),
                ControlReactionsDetails = CreateMockReactionDetail(),
                Explanation = "Forklaring",
                Paragraph = DummyData.GenerateDummyNumber(_digest).ToString(),
                ReactionDate = DummyData.GenerateDummyDateTime(_digest),
                NumberOfEffectuatedReactions = (int)DummyData.GenerateDummyNumber(_digest),
                ReactionReference = 1
            };
        }

        private ControlReactionDetails CreateMockReactionDetail()
        {
            return new ControlReactionDetails()
            {
                DegreeHigh = (int)DummyData.GenerateDummyNumber(_digest),
                DegreeLow = (int)DummyData.GenerateDummyNumber(_digest),
                ReactionClass = (int)DummyData.GenerateDummyNumber(_digest),
                ReactionHigh = (int)DummyData.GenerateDummyNumber(_digest),
                ReactionLow = (int)DummyData.GenerateDummyNumber(_digest),
                ReactionType = "Spesifisert type reaksjon",
                Value = (int)DummyData.GenerateDummyNumber(_digest)
            };
        }

        private List<ControlContact> CreateMockControlContacts()
        {
            return new List<ControlContact>()
           {
               CreateMockControlContact()
           };
        }

        private ControlContact CreateMockControlContact()
        {
            return new ControlContact()
            {
                Address = DummyData.GetRandomAddress(_digest),
                Department = DummyData.GenerateDummyString(_digest),
                Email = DummyData.GetDummyWord(_digest) + "@" + DummyData.GetDummyWord(_digest) + DummyData.GetDummyTld(_digest),
                PhoneNumber = "12345678",
                ResponsibleName = "Ansvarligheten selv"
            };
        }

        private List<AuditAddress> CreateMockControlLocations()
        {
            return new List<AuditAddress>()
           {
                CreateMockAuditAddress(), CreateMockAuditAddress()

           };
        }

        private AuditAddress CreateMockAuditAddress()
        {
            _digest = Digest();
            return new AuditAddress()
            {
                AddressName = DummyData.GetRandomAddress(_digest),
                AddressNumber = DummyData.GenerateDummyNumber(_digest).ToString(),
                District = "Dal",
                BuildingNumber = DummyData.GenerateDummyNumber(_digest).ToString(),
                CountyNumber = "020",
                Latitude = DummyData.GenerateDummyNumber(_digest).ToString(),
                Longtitude = DummyData.GenerateDummyNumber(_digest).ToString(),
                LocationDescription = "Enda ei rønne",
                MunicipalityNumber = "0301",
                PostName = DummyData.GenerateDummyString(_digest),
                PostNumber = "Postnummer",
                UnitNumber = "Enhetsnummer",
                LocationKeywords = DummyData.GenerateDummyString(_digest),
                LocationReference = (int)DummyData.GenerateDummyNumber(_digest)
            };
        }

        private ControlAttribute CreateMockControlAttributes()
        {
            return new ControlAttribute()
            {
                ControlKeywords = "key, word",
                ControlStatus = Dan.Tilda.Models.Enums.ControlState.Aapen,
                InternalControlId = Guid.NewGuid().ToString(),
                Major = Dan.Tilda.Models.Enums.MajorAccidentAttributeType.Nei,
                NotNotified = Dan.Tilda.Models.Enums.SurpriseControlAttributeType.Ja,
                ControlTopic = "Fem tema om dagen gjør godt for magen",
                SelectionCriteria = "Veldig suspekte folk",
                WebReportUrl = "https://www.vg.no"
            };
        }

        private List<ControlActivity> CreateMockControlActivities(string controlObject, string agency, string receivingAgency)
        {
            var a = new ControlActivity()
            {
                LocationReference = 1,
                ControlObject = controlObject,
                Activity = "aktivitet",
                ActivityExecutionType = "fysisk",
                ActivityReference = 1,
                Date = DateTime.Now,
                Days = 1,
                InternalControlId = Guid.NewGuid().ToString(),
                Observation = "Vi kikket på en god stund",
                AlertMessages = CreateMockAlertCompactMessages(receivingAgency),
                CoordinatedControl = CreateMockCoordinatedControl(agency)

            };

            return new List<ControlActivity>() {a};
        }

        private List<CoordinatedControlAgency> CreateMockCoordinatedControl(string agency)
        {
            var a = new CoordinatedControlAgency()
            {
                ActivityExecution = "",
                ControlAgency = agency,
                ControlTopic = "tema"
            };

            return new List<CoordinatedControlAgency>() {a};
        }

        private List<AlertCompact> CreateMockAlertCompactMessages(string receivingAgency)
        {
            var a = new AlertCompact()
            {
                Message = "VIKTIG BESKJED",
                ReceivingControlAgency = receivingAgency

            };

            var b = new AlertCompact()
            {
                Message = "VIKTIG BESKJED",
                ReceivingControlAgency = receivingAgency

            };

            return new List<AlertCompact>(){a, b};
        }

        private List<AlertFull> CreateMockAlertFullMessages(string receivingAgency)
        {
            var a = new AlertFull()
            {
                Message = "VIKTIG BESKJED NR 1",
                ReceivingControlAgency = receivingAgency,
                Date = DummyData.GenerateDummyDateTime(_digest),
                LocationReference = 1

            };

            var b = new AlertFull()
            {
                Message = "VIKTIG BESKJED NR 2",
                ReceivingControlAgency = receivingAgency,
                Date = DummyData.GenerateDummyDateTime(Digest()),
                LocationReference = 1

            };

            return new List<AlertFull>() { a, b };
        }

        private int Digest(string seed = null)
        {
            return seed == null && this._digest != 0 ? this._digest : DummyData.GetDigest(_mockActor + _organizationNumber + seed);
        }

        private int Rnd(int min = 0, int max = 100, string seed = null)
        {
            return DummyData.Clamp(Digest(seed), min, max);
        }
    }


}
