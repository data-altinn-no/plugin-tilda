using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dan.Plugin.Tilda.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OperationStatus
    {
        [EnumMember(Value = "ikkeAngitt")]
        Blank = 0,

        [EnumMember(Value = "konkurs")]
        Konkurs = 1,

        [EnumMember(Value = "underAvvikling")]
        UnderAvvikling = 2,

        [EnumMember(Value = "underTvangsavviklingEllerTvangsopploesning")]
        UnderTvangsavviklingEllerTvangsopplosning = 3,

        [EnumMember(Value = "ok")]
        OK = 4,

        [EnumMember(Value = "slettet")]
        Slettet = 5
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MajorAccidentAttributeType
    {
        [EnumMember(Value = "ja")]
        Ja = 4,
        [EnumMember(Value = "nei")]
        Nei = 1,
        [EnumMember(Value = "meldepliktig")]
        Meldepliktig = 2,
        [EnumMember(Value = "rapporteringspliktig")]
        Rapporteringspliktig = 3,
        [EnumMember(Value = "ikkeAngitt")]
        Blank = 0,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SurpriseControlAttributeType
    {
        [EnumMember(Value = "ja")]
        Ja = 1,
        [EnumMember(Value = "nei")]
        Nei = 2,
        [EnumMember(Value = "ikkeAngitt")]
        Blank = 0
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PeriodType
    {
        [EnumMember(Value = "periodisk")]
        Periodisk = 1
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ControlTopic
    {
        [JsonProperty("hendelse")]
        Hendelse = 1,

        [JsonProperty("proevetaking")]
        ProeveTaking = 2,

        [JsonProperty("utslippTilLuft")]
        UtslippTilLuft = 3,

        [JsonProperty("utslippTilVann")]
        UtslippTilVann = 4

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActivityType
    {
        [EnumMember(Value = "fysisk")]
        Fysisk = 1,

        [EnumMember(Value = "brev")]
        Brev = 2,

        [EnumMember(Value = "telefon")]
        Telefon = 3,

        [EnumMember(Value = "virtuelt")]
        Virtuelt = 4
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ControlActivityType
    {
        [EnumMember(Value = "hendelsesbasert")]
        Hendelsesbasert = 1,

        [EnumMember(Value = "nybygg")]
        Nybygg = 2,

        [EnumMember(Value = "oppfoelging")]
        Oppfoelging = 3,

        [EnumMember(Value = "periodisk")]
        Periodisk = 4,

        [EnumMember(Value = "storulykketilsyn")]
        Storulykketilsyn = 5,

        [EnumMember(Value = "tilsynsaksjon")]
        Tilsynsaksjon = 6
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum ControlSelection
    {
        [EnumMember(Value = "groveOvertredelser")]
        GroveOvertredelser = 1,

        [EnumMember(Value = "stikkproeve")]
        StikkProeve = 2,

        [EnumMember(Value = "bekymringsmelding")]
        Bekymringsmelding = 3,

        [EnumMember(Value = "aarlig")]
        Aarlig = 4,

        [EnumMember(Value = "risikovurdering")]
        Risikovurdering = 5
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Status
    {
        [EnumMember(Value = "groenn")]
        Groenn = 1,

        [EnumMember(Value = "gul")]
        Gul = 2,

        [EnumMember(Value = "roed")]
        Roed = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ControlState
    {
        [EnumMember(Value = "planlegging")]
        Planlegging = 4,

        [EnumMember(Value = "aapen")]
        Aapen = 1,

        [EnumMember(Value = "lukket")]
        Lukket = 2,

        [EnumMember(Value = "avbrutt")]
        Avbrutt = 3,

        [EnumMember(Value = "ikkeAngitt")]
        Blank = 0,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FunctionType
    {
        //
        [EnumMember(Value = "dokumenttilsyn")]
         Dokumenttilsyn = 1,

        [EnumMember(Value = "inspeksjon")]
        Inspeksjon = 2,

        [EnumMember(Value = "internrevisjon")]
        Internrevisjon = 3,

        [EnumMember(Value = "markedstilsyn")]
        Markedstilsyn = 4,

        [EnumMember(Value = "revisjon")]
        Revisjon = 5,

        [EnumMember(Value = "aapentTilsyn")]
        AapentTilsyn = 6,

        [EnumMember(Value = "lukketTilsyn")]
        LukketTilsyn = 7
    }
}
