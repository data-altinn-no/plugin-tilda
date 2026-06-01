using System;
using Newtonsoft.Json;

namespace Dan.Plugin.Tilda.Models;

public class BrregRegnskap
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("journalnr")]
    public string? Journalnr { get; set; }

    [JsonProperty("regnskapstype")]
    public string? Regnskapstype { get; set; }

    [JsonProperty("regnskapDokumenttype")]
    public string? RegnskapDokumenttype { get; set; }

    [JsonProperty("virksomhet")]
    public BrregVirksomhet? Virksomhet { get; set; }

    [JsonProperty("regnskapsperiode")]
    public BrregTidsperiode? Regnskapsperiode { get; set; }

    [JsonProperty("valuta")]
    public string? Valuta { get; set; }

    [JsonProperty("avviklingsregnskap")]
    public bool Avviklingsregnskap { get; set; }

    [JsonProperty("oppstillingsplan")]
    public string? Oppstillingsplan { get; set; }

    [JsonProperty("revisjon")]
    public BrregRevisjon? Revisjon { get; set; }

    [JsonProperty("regnkapsprinsipper")]
    public BrregRegnskapsprinsipper? Regnkapsprinsipper { get; set; }

    [JsonProperty("egenkapitalGjeld")]
    public BrregEgenkapitalGjeld? EgenkapitalGjeld { get; set; }

    [JsonProperty("eiendeler")]
    public BrregEiendeler? Eiendeler { get; set; }

    [JsonProperty("resultatregnskapResultat")]
    public BrregResultatregnskapResultat? ResultatregnskapResultat { get; set; }
}

public class BrregVirksomhet
{
    [JsonProperty("organisasjonsnummer")]
    public string? Organisasjonsnummer { get; set; }

    [JsonProperty("organisasjonsform")]
    public string? Organisasjonsform { get; set; }

    [JsonProperty("morselskap")]
    public bool Morselskap { get; set; }
}

public class BrregTidsperiode
{
    [JsonProperty("fraDato")]
    public DateTime FraDato { get; set; }

    [JsonProperty("tilDato")]
    public DateTime TilDato { get; set; }
}

public class BrregRevisjon
{
    [JsonProperty("ikkeRevidertAarsregnskap")]
    public bool IkkeRevidertAarsregnskap { get; set; }

    [JsonProperty("fravalgRevisjon")]
    public bool FravalgRevisjon { get; set; }
}

public class BrregRegnskapsprinsipper
{
    [JsonProperty("smaaForetak")]
    public bool SmaaForetak { get; set; }

    [JsonProperty("regnskapsregler")]
    public string? Regnskapsregler { get; set; }
}

public class BrregResultatregnskapResultat
{
    [JsonProperty("ordinaertResultatFoerSkattekostnad")]
    public double OrdinaertResultatFoerSkattekostnad { get; set; }

    [JsonProperty("ordinaertResultatSkattekostnad")]
    public double OrdinaertResultatSkattekostnad { get; set; }

    [JsonProperty("ekstraordinaerePoster")]
    public double EkstraordinaerePoster { get; set; }

    [JsonProperty("skattekostnadEkstraordinaertResultat")]
    public double SkattekostnadEkstraordinaertResultat { get; set; }

    [JsonProperty("aarsresultat")]
    public double Aarsresultat { get; set; }

    [JsonProperty("totalresultat")]
    public double Totalresultat { get; set; }

    [JsonProperty("finansresultat")]
    public BrregFinansresultat? Finansresultat { get; set; }

    [JsonProperty("driftsresultat")]
    public BrregDriftsresultat? Driftsresultat { get; set; }
}

public class BrregDriftsresultat
{
    [JsonProperty("driftsresultat")]
    public double Driftsresultat { get; set; }

    [JsonProperty("driftsinntekter")]
    public BrregDriftsinntekter? Driftsinntekter { get; set; }

    [JsonProperty("driftskostnad")]
    public BrregDriftskostnad? Driftskostnad { get; set; }
}

public class BrregDriftsinntekter
{
    [JsonProperty("salgsinntekter")]
    public double Salgsinntekter { get; set; }

    [JsonProperty("sumDriftsinntekter")]
    public double SumDriftsinntekter { get; set; }
}

public class BrregDriftskostnad
{
    [JsonProperty("loennskostnad")]
    public double Loennskostnad { get; set; }

    [JsonProperty("sumDriftskostnad")]
    public double SumDriftskostnad { get; set; }
}

public class BrregFinansresultat
{
    [JsonProperty("nettoFinans")]
    public double NettoFinans { get; set; }

    [JsonProperty("finansinntekt")]
    public BrregFinansinntekt? Finansinntekt { get; set; }

    [JsonProperty("finanskostnad")]
    public BrregFinanskostnad? Finanskostnad { get; set; }
}

public class BrregFinansinntekt
{
    [JsonProperty("sumFinansinntekter")]
    public double SumFinansinntekter { get; set; }
}

public class BrregFinanskostnad
{
    [JsonProperty("rentekostnadSammeKonsern")]
    public double RentekostnadSammeKonsern { get; set; }

    [JsonProperty("annenRentekostnad")]
    public double AnnenRentekostnad { get; set; }

    [JsonProperty("sumFinanskostnad")]
    public double SumFinanskostnad { get; set; }
}

public class BrregEgenkapitalGjeld
{
    [JsonProperty("sumEgenkapitalGjeld")]
    public double SumEgenkapitalGjeld { get; set; }

    [JsonProperty("egenkapital")]
    public BrregEgenkapital? Egenkapital { get; set; }

    [JsonProperty("gjeldOversikt")]
    public BrregGjeld? GjeldOversikt { get; set; }
}

public class BrregEgenkapital
{
    [JsonProperty("sumEgenkapital")]
    public double SumEgenkapital { get; set; }

    [JsonProperty("opptjentEgenkapital")]
    public BrregOpptjentEgenkapital? OpptjentEgenkapital { get; set; }

    [JsonProperty("innskuttEgenkapital")]
    public BrregInnskuttEgenkapital? InnskuttEgenkapital { get; set; }
}

public class BrregOpptjentEgenkapital
{
    [JsonProperty("sumOpptjentEgenkapital")]
    public double SumOpptjentEgenkapital { get; set; }
}

public class BrregInnskuttEgenkapital
{
    [JsonProperty("sumInnskuttEgenkaptial")]
    public double SumInnskuttEgenkaptial { get; set; }
}

public class BrregGjeld
{
    [JsonProperty("sumGjeld")]
    public double SumGjeld { get; set; }

    [JsonProperty("kortsiktigGjeld")]
    public BrregKortsiktigGjeld? KortsiktigGjeld { get; set; }

    [JsonProperty("langsiktigGjeld")]
    public BrregLangsiktigGjeld? LangsiktigGjeld { get; set; }
}

public class BrregKortsiktigGjeld
{
    [JsonProperty("sumKortsiktigGjeld")]
    public double SumKortsiktigGjeld { get; set; }
}

public class BrregLangsiktigGjeld
{
    [JsonProperty("sumLangsiktigGjeld")]
    public double SumLangsiktigGjeld { get; set; }
}

public class BrregEiendeler
{
    [JsonProperty("goodwill")]
    public double Goodwill { get; set; }

    [JsonProperty("sumVarer")]
    public double SumVarer { get; set; }

    [JsonProperty("sumFordringer")]
    public double SumFordringer { get; set; }

    [JsonProperty("sumInvesteringer")]
    public double SumInvesteringer { get; set; }

    [JsonProperty("sumBankinnskuddOgKontanter")]
    public double SumBankinnskuddOgKontanter { get; set; }

    [JsonProperty("sumEiendeler")]
    public double SumEiendeler { get; set; }

    [JsonProperty("omloepsmidler")]
    public BrregOmloepsmidler? Omloepsmidler { get; set; }

    [JsonProperty("anleggsmidler")]
    public BrregAnleggsmidler? Anleggsmidler { get; set; }
}

public class BrregOmloepsmidler
{
    [JsonProperty("sumOmloepsmidler")]
    public double SumOmloepsmidler { get; set; }
}

public class BrregAnleggsmidler
{
    [JsonProperty("sumAnleggsmidler")]
    public double SumAnleggsmidler { get; set; }
}
