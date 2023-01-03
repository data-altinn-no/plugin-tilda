using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Dan.Plugin.Tilda.Models
{


    [JsonObject("kampanje")]
    public class Campaign
    {
        [JsonProperty("kampanjenavn")]
        public string Name;

        [JsonProperty("kampanjebeskrivelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description;

        [JsonProperty("startdatoForKampanje", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime StartDate;

        [JsonProperty("sluttdatoForKampanje", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime EndDate;
    }

    [JsonObject("meldingTilAnnenMyndighet")]
    public class AlertMessage
    {
        [JsonProperty("meldingFraMyndighet")]
        public string AuditingAgency;

        [JsonProperty("meldingOmTildaenhet")]
        public string ControlObject;

        [JsonProperty("datoForMeldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime AlertDate;

        [JsonProperty("meldingsinnholdTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message;

        [JsonProperty("identifikator", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id;
    }

    [JsonObject("bruddOgReaksjon")]
    public class Reaction
   {
        [JsonProperty("bruddOgReaksjonsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ReactionReference;

        [JsonProperty("tilsynsaktivitetreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ControlActivityReference;

        [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ControlLocationReference;

        [JsonProperty("utredningAvBruddOgReaksjon", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Explanation;

        [JsonProperty("lovparagraf", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Paragraph;

        [JsonProperty("reaksjonsdato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime ReactionDate;

        [JsonProperty("alvorsgrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ControlReactionDetails ControlReactionsDetails;

        [JsonProperty("antallGangerVirkemiddelErTattIBruk", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfEffectuatedReactions;

    }

    [JsonObject("alvorsgrad")]
    public class ControlReactionDetails
    {
        [JsonProperty("utmaaltReaksjonsverdi", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Value;

        [JsonProperty("utmaaltReaksjonstype", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReactionType;

        [JsonProperty("utmaaltReaksjonsklasse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ReactionClass;

        [JsonProperty("lavreaksjonsverdi", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ReactionLow;

        [JsonProperty("hoeyreaksjonsverdi", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ReactionHigh;

        [JsonProperty("lavalvorsgradindeks", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int DegreeLow;

        [JsonProperty("hoeyalvorsgradindeks", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int DegreeHigh;
    }



    [JsonObject("kontaktpunkt")]
    public class ControlContact
    {
        [JsonProperty("kontaktperson", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ResponsibleName;

        [JsonProperty("avdeling", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Department;

        [JsonProperty("telefonnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PhoneNumber;

        [JsonProperty("epost", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Email;

        [JsonProperty("adresse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Address;
    }

    [JsonObject("anmerkning")]
    public class Remark
    {
        [JsonProperty("anmerkningsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Remarkreference;

        [JsonProperty("anmerkning", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string RemarkMessage;
    }

    [JsonObject("tilsynsaktivitet")]
    public class ControlActivity
    {
        [JsonProperty("tilsynsaktivitetreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ActivityReference;

        [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int LocationReference;

        [JsonProperty("internAktivitetsidentifikator", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string InternalControlId;

        [JsonProperty("kontrollobjekt", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlObject;

        [JsonProperty("startdatoForTilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Date;

        [JsonProperty("varighetForTilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Days;

        [JsonProperty("tilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Activity;

        [JsonProperty("aktivitetsutfoerelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ActivityExecutionType;

        [JsonProperty("observasjonFraTilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Observation;

        [JsonProperty("samtidigeKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<CoordinatedControlAgency> CoordinatedControl;

        [JsonProperty("meldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<AlertCompact> AlertMessages;

    }

    [JsonObject("planlagtekontroller")]
    public class PlannedControlActivity
    {
        [JsonProperty("planlagtkontrolldato", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Date;

        [JsonProperty("planlagtkontrollVarighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Days;

        [JsonProperty("tilsynstema", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Topic;

        [JsonProperty("tilsynsaktivitet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Activity;

        [JsonProperty("aktivitetsutfoerelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ActivityExecutionType;

        [JsonProperty("samtidigeKontroller", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<CoordinatedControlAgency> CoordinatedControl;
    }

    [JsonObject("samtidigKontroll")]
    public class CoordinatedControlAgency
    {
        [JsonProperty("samtidigTilsynsmyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlAgency;

        [JsonProperty("tilsynstema", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlTopic;

        [JsonProperty("aktivitetsutfoerelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ActivityExecution;
    }

    [JsonObject("meldingTilAnnenMyndighet")]
    public class AlertCompact
    {
        [JsonProperty("meldingTilmyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReceivingControlAgency;

        [JsonProperty("meldingsinnholdTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message;
    }

    [JsonObject("meldingTilAnnenMyndighet")]
    public class AlertFull
    {
        [JsonProperty("meldingTilMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ReceivingControlAgency;

        [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int LocationReference;

        [JsonProperty("meldingsinnholdTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message;
        
        [JsonProperty("datoForMeldingTilAnnenMyndighet", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime Date;
    }

    [JsonObject("tilsynsegenskap")]
    public class ControlAttribute
    {
        [JsonProperty("internTilsynsid", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string InternalControlId;

        [JsonProperty("storulykketilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MajorAccidentAttributeType Major;

        [JsonProperty("uanmeldttilsyn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SurpriseControlAttributeType NotNotified;

        [JsonProperty("tilsynsutvelgelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SelectionCriteria;

        [JsonProperty("tilsynsstatus", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ControlState ControlStatus;

        [JsonProperty("tilsynstema", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlTopic;

        [JsonProperty("tilsynsnoekkelord", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ControlKeywords;

        [JsonProperty("nettrapport", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string WebReportUrl;
    }

    [JsonObject("tilsynsmyndighet")]
    public class Agency
    {
        [JsonProperty("tilsynsmyndighet")]
        public string AgencyOrganisationNumber;

        [JsonProperty("tema", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Topic;
    }

    [JsonObject("tilsynsadresse")]
    public class AuditAddress
    {
        [JsonProperty("lokalitetsreferanse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int LocationReference;

        [JsonProperty("lokalitetsbeskrivelse", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocationDescription;

        [JsonProperty("lokalitetsnoekkelord", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LocationKeywords;

        [JsonProperty("lengdegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Latitude;

        [JsonProperty("breddegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Longtitude;

        [JsonProperty("bygningsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BuildingNumber;

        [JsonProperty("bruksenhetsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UnitNumber;

        [JsonProperty("adressenavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddressName;

        [JsonProperty("adressenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddressNumber;

        [JsonProperty("postnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PostNumber;

        [JsonProperty("poststedsnavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PostName;

        [JsonProperty("kommunenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MunicipalityNumber;

        [JsonProperty("bydel", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string District;

        [JsonProperty("fylkesnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CountyNumber;
    }

    [JsonObject("tilsynsadresse")]
    public class ERAddress
    {
        [JsonProperty("lengdegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Latitude;

        [JsonProperty("breddegrad", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Longtitude;

        [JsonProperty("bygningsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BuildingNumber;

        [JsonProperty("bruksenhetsnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string UnitNumber;

        [JsonProperty("adressenavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddressName;

        [JsonProperty("adressenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddressNumber;

        [JsonProperty("postnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PostNumber;

        [JsonProperty("poststedsnavn", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PostName;

        [JsonProperty("kommunenummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MunicipalityNumber;

        [JsonProperty("bydel", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string District;

        [JsonProperty("fylkesnummer", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CountyNumber;
    }



}
