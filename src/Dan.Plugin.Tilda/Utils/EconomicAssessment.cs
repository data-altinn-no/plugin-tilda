using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dan.Plugin.TIlda.Utils;

public enum AssessmentLevel
{
    Kritisk,
    Svak,
    Akseptabel,
    God,
    Utmerket,
}

public enum AltmanZone
{
    TryggSone,
    Grasone,
    Faresone,
}

public enum RedFlagSeverity
{
    Kritisk,
    Alvorlig,
}

public enum IndustryMetricVerdict
{
    Normal,
    MistenkeligHoy,
    MistenkeligLav,
    SvakHoy,
    SvakLav,
}

public enum IndustryOverallVerdict
{
    Normal,
    Mistenkelig,
    Avvikende,
}

public class AccountsInformationYear
{
    // Regnskapsperiode
    public DateTime FraDato { get; set; } 
    public DateTime TilDato { get; set; } 

    // Resultatregnskap - Driftsinntekter
    public double Salgsinntekter { get; set; }
    public double SumDriftsinntekter { get; set; }

    // Resultatregnskap - Driftskostnader
    public double Loennskostnad { get; set; }
    public double SumDriftskostnad { get; set; }

    // Resultatregnskap - Driftsresultat
    public double Driftsresultat { get; set; }

    // Resultatregnskap - Finansposter
    public double SumFinansinntekter { get; set; }
    public double RentekostnadSammeKonsern { get; set; }
    public double AnnenRentekostnad { get; set; }
    public double SumFinanskostnad { get; set; }
    public double NettoFinans { get; set; }

    // Resultatregnskap - Resultat
    public double OrdinaertResultatFoerSkattekostnad { get; set; }
    public double OrdinaertResultatSkattekostnad { get; set; }
    public double EkstraordinaerePoster { get; set; }
    public double SkattekostnadEkstraordinaertResultat { get; set; }
    public double Aarsresultat { get; set; }
    public double Totalresultat { get; set; }

    // Balanse - Eiendeler
    public double Goodwill { get; set; }
    public double SumAnleggsmidler { get; set; }
    public double SumVarer { get; set; }
    public double SumFordringer { get; set; }
    public double SumInvesteringer { get; set; }
    public double SumBankinnskuddOgKontanter { get; set; }
    public double SumOmloepsmidler { get; set; }
    public double SumEiendeler { get; set; }

    // Balanse - Egenkapital og gjeld
    public double SumInnskuttEgenkapital { get; set; }
    public double SumOpptjentEgenkapital { get; set; }
    public double SumEgenkapital { get; set; }
    public double SumLangsiktigGjeld { get; set; }
    public double SumKortsiktigGjeld { get; set; }
    public double SumGjeld { get; set; }
    public double SumEgenkapitalGjeld { get; set; }

    // Ekstra felt (brukes i vurdering)
    public int AntallAnsatte { get; set; }
}

public class EnhetsinformasjonInput
{
    public string Tildaenhet { get; set; } = "";
    public string TildaenhetNavn { get; set; } = "";
    public string Naeringskode { get; set; } = "";
    public string Organisasjonsform { get; set; } = "";
    public string Driftsstatus { get; set; } = "";
    public string Kommune { get; set; } = "";
}

public class AssessmentIndicator
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public int Score { get; set; }
    public AssessmentLevel Level { get; set; }
    public string Description { get; set; } = "";
}

public class AltmanZScoreComponents
{
    public double X1 { get; set; } // Working Capital / Total Assets
    public double X2 { get; set; } // Retained Earnings / Total Assets
    public double X3 { get; set; } // EBIT / Total Assets
    public double X4 { get; set; } // Equity / Total Liabilities
    public double X5 { get; set; } // Sales / Total Assets
}

public class AltmanZScoreResult
{
    public double Score { get; set; }
    public AltmanZone Zone { get; set; }
    public AltmanZScoreComponents Components { get; set; } = new();
    public string Description { get; set; } = "";
}

public class RedFlag
{
    public string Rule { get; set; } = "";
    public string Description { get; set; } = "";
    public RedFlagSeverity Severity { get; set; }
}

public class IndustryMetricComparison
{
    public string Metric { get; set; } = "";
    public double CompanyValue { get; set; }
    public double IndustryAvg { get; set; }
    public double IndustryStdDev { get; set; }
    public double ZScore { get; set; }
    public IndustryMetricVerdict Verdict { get; set; }
    public string Description { get; set; } = "";
}

public class IndustryComparison
{
    public string NaceCode { get; set; } = "";
    public string NaceName { get; set; } = "";
    public string Region { get; set; } = "";
    public int SampleSize { get; set; }
    public List<IndustryMetricComparison> Metrics { get; set; } = new();
    public IndustryOverallVerdict OverallVerdict { get; set; }
    public string Summary { get; set; } = "";
}

public class AssessmentResult
{
    public int TotalScore { get; set; }
    public AssessmentLevel Level { get; set; }
    public string Summary { get; set; } = "";
    public List<AssessmentIndicator> Indicators { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
    public List<string> PositiveFactors { get; set; } = new();
    public List<RedFlag> RedFlags { get; set; } = new();
    public string Recommendation { get; set; } = "";
    public AltmanZScoreResult AltmanZScore { get; set; } = new();
    public IndustryComparison IndustryComparison { get; set; } = new();

    public List<AccountsInformationYear> AccountYearsUsed { get; set; } = new();
}

/// <summary>
/// Economic Assessment Algorithm.
///
/// Evaluates an organization's financial health based on:
/// - AccountsInformation (regnskapstall) for up to 3 years
/// - TildaRegistryEntry (enhetsinformasjon)
///
/// Uses proff.no's formulas and rating scale:
///   Lønnsomhet: Resultatgrad = (Ordinært resultat før skatt + Rentekostnader) / Driftsinntekter × 100
///   Likviditet: Likviditetsgrad 1 = Omløpsmidler / Kortsiktig gjeld
///   Soliditet:  Egenkapitalandel = Egenkapital / Totalkapital × 100
///   + Totalrentabilitet, Gjeldsgrad, Omsetningsvekst, Ansatteutvikling, Driftsstatus
///
/// Rating scale (proff.no): Meget god, God, Tilfredsstillende, Svak, Ikke tilfredsstillende.
/// </summary>
public class EconomicAssessment
{
    private readonly List<AccountsInformationYear> _accounts;
    private readonly BREntityRegisterEntry _enhet;

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly CultureInfo NbNo = new CultureInfo("nb-NO");

    public EconomicAssessment(IEnumerable<AccountsInformationYear> accounts, BREntityRegisterEntry enhet)
    {
        _accounts = accounts
            .OrderBy(a => a.FraDato)
            .ToList();
        _enhet = enhet;
    }

    /// <summary>Run the full assessment and return a result.</summary>
    public AssessmentResult Evaluate()
    {
        var indicators = new List<AssessmentIndicator>
        {
            AssessResultatgrad(),       // 1. Lønnsomhet - Resultatgrad (proff.no formel)
            AssessEgenkapitalandel(),   // 2. Soliditet - Egenkapitalandel (proff.no formel)
            AssessLikviditet(),         // 3. Likviditet - Likviditetsgrad 1 (proff.no formel)
            AssessLikviditetsgrad2(),   // 4. Likviditetsgrad 2 (proff.no formel)
            AssessTotalrentabilitet(),  // 5. Totalrentabilitet (proff.no formel)
            AssessGjeldsgrad(),         // 6. Gjeldsgrad
            AssessOmsetningsvekst(),    // 7. Omsetningsvekst
            AssessAnsatteutvikling(),   // 8. Ansatteutvikling
            AssessDriftsstatus(_enhet),       // 9. Driftsstatus
        };

        var riskFactors = new List<string>();
        var positiveFactors = new List<string>();
        foreach (var indicator in indicators)
        {
            if (indicator.Score < 30)
                riskFactors.Add($"{indicator.Name}: {indicator.Description}");
            else if (indicator.Score >= 70)
                positiveFactors.Add($"{indicator.Name}: {indicator.Description}");
        }

        var redFlags = DetectRedFlags();

        // Calculate total score (weighted average). Must sum to 100.
        int[] weights = { 18, 16, 12, 10, 12, 10, 10, 7, 5 };
        double weightedSum = 0;
        for (int i = 0; i < indicators.Count; i++)
            weightedSum += indicators[i].Score * (weights[i] / 100.0);
        int totalScore = (int)Math.Round(weightedSum, MidpointRounding.AwayFromZero);

        // Red flags with severity 'kritisk' force the score down
        bool hasCriticalRedFlag = redFlags.Any(f => f.Severity == RedFlagSeverity.Kritisk);
        if (hasCriticalRedFlag && totalScore > 25)
            totalScore = 25;

        var level = ScoreToLevel(totalScore);
        var summary = GenerateSummary(totalScore, level, riskFactors, positiveFactors);
        var recommendation = GenerateRecommendation(level, riskFactors, indicators);

        return new AssessmentResult
        {
            TotalScore = totalScore,
            Level = level,
            Summary = summary,
            Indicators = indicators,
            AccountYearsUsed = _accounts,
            RiskFactors = riskFactors,
            PositiveFactors = positiveFactors,
            RedFlags = redFlags,
            Recommendation = recommendation,
            AltmanZScore = ComputeAltmanZScore(),
            //IndustryComparison = ComputeIndustryComparison(),
        };
    }

    private AccountsInformationYear LatestYear => _accounts[^1];
    private AccountsInformationYear? PreviousYear => _accounts.Count > 1 ? _accounts[^2] : null;
    private AccountsInformationYear FirstYear => _accounts[0];

    private static AssessmentLevel ScoreToLevel(int score)
    {
        if (score >= 80) return AssessmentLevel.Utmerket;
        if (score >= 60) return AssessmentLevel.God;
        if (score >= 40) return AssessmentLevel.Akseptabel;
        if (score >= 20) return AssessmentLevel.Svak;
        return AssessmentLevel.Kritisk;
    }

    private static string LevelText(AssessmentLevel level) => level switch
    {
        AssessmentLevel.Utmerket => "utmerket",
        AssessmentLevel.God => "god",
        AssessmentLevel.Akseptabel => "akseptabel",
        AssessmentLevel.Svak => "svak",
        AssessmentLevel.Kritisk => "kritisk",
        _ => "ukjent",
    };

    // --- Individual Assessments (proff.no formulas) ---

    /// <summary>
    /// Resultatgrad (proff.no Lønnsomhet).
    /// Formel: (Ordinært resultat før skatt + Rentekostnader) / Driftsinntekter × 100.
    /// Skala: >15% Meget god, 10-15% God, 5-10% Tilfredsstillende, 1-5% Svak, &lt;1% Ikke tilfredsstillende.
    /// </summary>
    private AssessmentIndicator AssessResultatgrad()
    {
        var latest = LatestYear;
        double rentekostnader = latest.SumFinanskostnad;
        double resultatgrad = latest.SumDriftsinntekter > 0
            ? ((latest.OrdinaertResultatFoerSkattekostnad + rentekostnader) / latest.SumDriftsinntekter) * 100
            : 0;

        int score;
        if (resultatgrad > 15) score = 100;
        else if (resultatgrad > 10) score = 80;
        else if (resultatgrad > 5) score = 60;
        else if (resultatgrad > 1) score = 40;
        else if (resultatgrad > -5) score = 20;
        else score = 0;

        string proffLevel = resultatgrad > 15 ? "Meget god"
            : resultatgrad > 10 ? "God"
            : resultatgrad > 5 ? "Tilfredsstillende"
            : resultatgrad > 1 ? "Svak"
            : "Ikke tilfredsstillende";

        return new AssessmentIndicator
        {
            Name = "Resultatgrad (Lønnsomhet)",
            Value = $"{resultatgrad.ToString("F1", Inv)}%",
            Score = score,
            Level = ScoreToLevel(score),
            Description = $"{proffLevel} – Resultatgrad på {resultatgrad.ToString("F1", Inv)}% (Ordinært resultat + rentekostnader i forhold til driftsinntekter)",
        };
    }

    /// <summary>
    /// Totalrentabilitet (proff.no).
    /// Formel: (Resultat før skatt + Finanskostnader) / Gjennomsnittlig totalkapital × 100.
    /// </summary>
    private AssessmentIndicator AssessTotalrentabilitet()
    {
        var latest = LatestYear;
        var prev = PreviousYear;
        double avgTotalkapital = prev != null
            ? (latest.SumEiendeler + prev.SumEiendeler) / 2.0
            : latest.SumEiendeler;

        double totalrentabilitet = avgTotalkapital > 0
            ? ((latest.OrdinaertResultatFoerSkattekostnad + latest.SumFinanskostnad) / avgTotalkapital) * 100
            : 0;

        int score;
        if (totalrentabilitet > 15) score = 100;
        else if (totalrentabilitet > 10) score = 80;
        else if (totalrentabilitet > 5) score = 60;
        else if (totalrentabilitet > 0) score = 40;
        else score = 10;

        string description = totalrentabilitet > 10
            ? $"God avkastning på totalkapitalen ({totalrentabilitet.ToString("F1", Inv)}%)"
            : totalrentabilitet > 0
                ? $"Moderat avkastning på totalkapitalen ({totalrentabilitet.ToString("F1", Inv)}%)"
                : $"Negativ avkastning på totalkapitalen ({totalrentabilitet.ToString("F1", Inv)}%)";

        return new AssessmentIndicator
        {
            Name = "Totalrentabilitet",
            Value = $"{totalrentabilitet.ToString("F1", Inv)}%",
            Score = score,
            Level = ScoreToLevel(score),
            Description = description,
        };
    }

    /// <summary>
    /// Egenkapitalandel (proff.no Soliditet).
    /// Formel: Egenkapital / Totalkapital × 100.
    /// Skala: >40% Meget god, 20-40% God, 10-20% Tilfredsstillende, 5-10% Svak, &lt;5% Ikke tilfredsstillende.
    /// </summary>
    private AssessmentIndicator AssessEgenkapitalandel()
    {
        var latest = LatestYear;
        double totalKapital = latest.SumEiendeler > 0
            ? latest.SumEiendeler
            : latest.SumEgenkapital + latest.SumGjeld;
        double andel = totalKapital > 0
            ? (latest.SumEgenkapital / totalKapital) * 100
            : 0;

        int score;
        if (andel > 40) score = 100;
        else if (andel > 20) score = 80;
        else if (andel > 10) score = 60;
        else if (andel > 5) score = 40;
        else if (andel > 0) score = 20;
        else score = 0;

        string proffLevel = andel > 40 ? "Meget god"
            : andel > 20 ? "God"
            : andel > 10 ? "Tilfredsstillende"
            : andel > 5 ? "Svak"
            : "Ikke tilfredsstillende";

        return new AssessmentIndicator
        {
            Name = "Egenkapitalandel (Soliditet)",
            Value = $"{andel.ToString("F1", Inv)}%",
            Score = score,
            Level = ScoreToLevel(score),
            Description = $"{proffLevel} – Egenkapitalandel på {andel.ToString("F1", Inv)}% (egenkapital i forhold til totalkapital)",
        };
    }

    /// <summary>
    /// Likviditetsgrad 1 (proff.no Likviditet).
    /// Formel: Omløpsmidler / Kortsiktig gjeld.
    /// Skala: >2 Meget god, 1.5-2 God, 1-1.5 Tilfredsstillende, 0.5-1 Svak, &lt;0.5 Ikke tilfredsstillende.
    /// </summary>
    private AssessmentIndicator AssessLikviditet()
    {
        var latest = LatestYear;
        double likviditetsgrad = latest.SumKortsiktigGjeld > 0
            ? latest.SumOmloepsmidler / latest.SumKortsiktigGjeld
            : 0;

        int score;
        if (likviditetsgrad > 2.0) score = 100;
        else if (likviditetsgrad > 1.5) score = 80;
        else if (likviditetsgrad > 1.0) score = 60;
        else if (likviditetsgrad > 0.5) score = 40;
        else score = 10;

        string proffLevel = likviditetsgrad > 2 ? "Meget god"
            : likviditetsgrad > 1.5 ? "God"
            : likviditetsgrad > 1 ? "Tilfredsstillende"
            : likviditetsgrad > 0.5 ? "Svak"
            : "Ikke tilfredsstillende";

        return new AssessmentIndicator
        {
            Name = "Likviditetsgrad 1 (Likviditet)",
            Value = likviditetsgrad.ToString("F2", Inv),
            Score = score,
            Level = ScoreToLevel(score),
            Description = $"{proffLevel} – Likviditetsgrad 1 på {likviditetsgrad.ToString("F2", Inv)} (omløpsmidler i forhold til kortsiktig gjeld)",
        };
    }

    /// <summary>
    /// Likviditetsgrad 2 (proff.no).
    /// Formel: (Omløpsmidler - Varelager) / Kortsiktig gjeld.
    /// Skala: >1.5 Meget god, 1-1.5 God, 0.7-1 Tilfredsstillende, 0.4-0.7 Svak, &lt;0.4 Ikke tilfredsstillende.
    /// </summary>
    private AssessmentIndicator AssessLikviditetsgrad2()
    {
        var latest = LatestYear;
        double lg2 = latest.SumKortsiktigGjeld > 0
            ? (latest.SumOmloepsmidler - latest.SumVarer) / latest.SumKortsiktigGjeld
            : 0;

        int score;
        if (lg2 > 1.5) score = 100;
        else if (lg2 > 1.0) score = 80;
        else if (lg2 > 0.7) score = 60;
        else if (lg2 > 0.4) score = 40;
        else score = 10;

        string proffLevel = lg2 > 1.5 ? "Meget god"
            : lg2 > 1 ? "God"
            : lg2 > 0.7 ? "Tilfredsstillende"
            : lg2 > 0.4 ? "Svak"
            : "Ikke tilfredsstillende";

        return new AssessmentIndicator
        {
            Name = "Likviditetsgrad 2",
            Value = lg2.ToString("F2", Inv),
            Score = score,
            Level = ScoreToLevel(score),
            Description = $"{proffLevel} – Likviditetsgrad 2 på {lg2.ToString("F2", Inv)} (omløpsmidler minus varelager i forhold til kortsiktig gjeld)",
        };
    }

    private AssessmentIndicator AssessGjeldsgrad()
    {
        var latest = LatestYear;
        double gjeldsgrad = latest.SumEgenkapital > 0
            ? latest.SumGjeld / latest.SumEgenkapital
            : latest.SumGjeld > 0 ? 999 : 0;

        int score;
        if (gjeldsgrad <= 1.0) score = 100;
        else if (gjeldsgrad <= 2.0) score = 80;
        else if (gjeldsgrad <= 3.0) score = 60;
        else if (gjeldsgrad <= 5.0) score = 40;
        else if (gjeldsgrad <= 10.0) score = 20;
        else score = 0;

        string value = gjeldsgrad >= 999 ? "N/A (ingen EK)" : gjeldsgrad.ToString("F2", Inv);
        string description = gjeldsgrad <= 2
            ? $"Lav gjeldsgrad ({gjeldsgrad.ToString("F2", Inv)}), sunn finansiering"
            : gjeldsgrad <= 5
                ? $"Moderat gjeldsgrad ({gjeldsgrad.ToString("F2", Inv)})"
                : $"Høy gjeldsbelastning ({(gjeldsgrad >= 999 ? "ingen egenkapital" : gjeldsgrad.ToString("F2", Inv))})";

        return new AssessmentIndicator
        {
            Name = "Gjeldsgrad",
            Value = value,
            Score = score,
            Level = ScoreToLevel(score),
            Description = description,
        };
    }

    private AssessmentIndicator AssessOmsetningsvekst()
    {
        if (_accounts.Count < 2)
        {
            return new AssessmentIndicator
            {
                Name = "Omsetningsvekst",
                Value = "N/A",
                Score = 50,
                Level = AssessmentLevel.Akseptabel,
                Description = "Ikke nok data til å beregne vekst",
            };
        }

        var latest = LatestYear;
        var first = FirstYear;
        int years = _accounts.Count - 1;
        double totalGrowth = first.SumDriftsinntekter > 0
            ? ((latest.SumDriftsinntekter - first.SumDriftsinntekter) / first.SumDriftsinntekter) * 100
            : 0;
        double annualGrowth = totalGrowth / years;

        int score;
        if (annualGrowth >= 15) score = 100;
        else if (annualGrowth >= 8) score = 80;
        else if (annualGrowth >= 3) score = 60;
        else if (annualGrowth >= 0) score = 40;
        else if (annualGrowth >= -10) score = 20;
        else score = 0;

        string description = annualGrowth >= 5
            ? $"Positiv vekst på {annualGrowth.ToString("F1", Inv)}% per år"
            : annualGrowth >= 0
                ? $"Flat utvikling ({annualGrowth.ToString("F1", Inv)}% per år)"
                : $"Negativ utvikling med {annualGrowth.ToString("F1", Inv)}% årlig nedgang";

        return new AssessmentIndicator
        {
            Name = "Omsetningsvekst",
            Value = $"{annualGrowth.ToString("F1", Inv)}% p.a.",
            Score = score,
            Level = ScoreToLevel(score),
            Description = description,
        };
    }

    private AssessmentIndicator AssessAnsatteutvikling()
    {
        if (_accounts.Count < 2)
        {
            return new AssessmentIndicator
            {
                Name = "Ansatteutvikling",
                Value = "N/A",
                Score = 50,
                Level = AssessmentLevel.Akseptabel,
                Description = "Ikke nok data til å beregne endring",
            };
        }

        var latest = LatestYear;
        var first = FirstYear;
        double change = first.AntallAnsatte > 0
            ? ((double)(latest.AntallAnsatte - first.AntallAnsatte) / first.AntallAnsatte) * 100
            : 0;

        int score;
        if (change >= 20) score = 100;
        else if (change >= 10) score = 80;
        else if (change >= 0) score = 60;
        else if (change >= -10) score = 40;
        else if (change >= -25) score = 20;
        else score = 0;

        string sign = change >= 0 ? "+" : "";
        string description = change >= 5
            ? $"Vekst i antall ansatte ({change.ToString("F1", Inv)}%)"
            : change >= -5
                ? $"Stabil bemanning ({change.ToString("F1", Inv)}%)"
                : $"Reduksjon i bemanning ({change.ToString("F1", Inv)}%)";

        return new AssessmentIndicator
        {
            Name = "Ansatteutvikling",
            Value = $"{sign}{change.ToString("F1", Inv)}%",
            Score = score,
            Level = ScoreToLevel(score),
            Description = description,
        };
    }

    private AssessmentIndicator AssessDriftsstatus(BREntityRegisterEntry brResult)
    {
        OperationStatus status = GetOperationStatus(brResult);
        int score;
        string description;
        if (status == OperationStatus.Ok || status == OperationStatus.Blank)
        {
            score = 100;
            description = "Normal drift";
        }
        else if (status == OperationStatus.UnderAvvikling)
        {
            score = 10;
            description = "Virksomheten er under avvikling";
        }
        else if (status == OperationStatus.Konkurs)
        {
            score = 0;
            description = "Virksomheten er under konkursbehandling";
        }
        else if (status == OperationStatus.UnderTvangsavviklingEllerTvangsopplosning)
        {
            score = 0;
            description = "Under tvangsavvikling eller tvangsoppløsning";
        }
        else if (status == OperationStatus.Slettet)
        {
            score = 0;
            description = "Virksomheten er slettet";
        }
        else
        {
            score = 50;
            description = $"Driftsstatus: ukjent";
        }

        return new AssessmentIndicator
        {
            Name = "Driftsstatus",
            Value = status.ToString(),
            Score = score,
            Level = ScoreToLevel(score),
            Description = description,
        };
    }


    private OperationStatus GetOperationStatus(BREntityRegisterEntry brResult)
    {
        if (brResult.Konkurs)
        {
            return OperationStatus.Konkurs;
        }

        if (brResult.UnderAvvikling)
        {
            return OperationStatus.UnderAvvikling;
        }

        if (brResult.UnderTvangsavviklingEllerTvangsopplosning)
        {
            return OperationStatus.UnderTvangsavviklingEllerTvangsopplosning;
        }

        if (brResult.Slettedato != DateTime.MinValue)
        {
            return OperationStatus.Slettet;
        }

        return OperationStatus.Ok;
    }

    // --- Red Flag Detection ---

    /// <summary>
    /// Detects critical patterns that override the normal score.
    /// These are strong signals of financial distress regardless of other indicators.
    /// </summary>
    private List<RedFlag> DetectRedFlags()
    {
        var flags = new List<RedFlag>();
        var latest = LatestYear;
        var sorted = _accounts;

        // 1. Negativ egenkapital
        if (latest.SumEgenkapital < 0)
        {
            flags.Add(new RedFlag
            {
                Rule = "Negativ egenkapital",
                Description = $"Egenkapitalen er negativ ({latest.SumEgenkapital.ToString("N0", NbNo)} NOK). Selskapet har mer gjeld enn eiendeler, noe som indikerer teknisk insolvens. Styret har handleplikt etter aksjeloven § 3-5.",
                Severity = RedFlagSeverity.Kritisk,
            });
        }

        // 2. Tre sammenhengende år med underskudd
        if (sorted.Count >= 3)
        {
            var lastThree = sorted.Skip(sorted.Count - 3).ToList();
            bool allLoss = lastThree.All(y => y.Aarsresultat < 0);
            if (allLoss)
            {
                string years = string.Join(", ", lastThree.Select(y => y.FraDato.Year.ToString()));
                flags.Add(new RedFlag
                {
                    Rule = "Tre år med underskudd",
                    Description = $"Selskapet har hatt negativt årsresultat tre år på rad ({years}). Vedvarende underskudd tærer på egenkapitalen og øker konkursrisikoen.",
                    Severity = RedFlagSeverity.Kritisk,
                });
            }
        }

        // 3. Omsetningskollaps (>50% fall)
        if (sorted.Count >= 2)
        {
            var prev = sorted[^2];
            if (prev.SumDriftsinntekter > 0)
            {
                double revenueChange = (latest.SumDriftsinntekter - prev.SumDriftsinntekter) / prev.SumDriftsinntekter;
                if (revenueChange < -0.5)
                {
                    int pct = (int)Math.Abs(Math.Round(revenueChange * 100, MidpointRounding.AwayFromZero));
                    flags.Add(new RedFlag
                    {
                        Rule = "Omsetningskollaps",
                        Description = $"Driftsinntektene har falt med {pct}% fra forrige år. Et fall på over 50% er et alvorlig faresignal som kan indikere tap av nøkkelkunder eller markedssvikt.",
                        Severity = RedFlagSeverity.Kritisk,
                    });
                }
                else if (revenueChange < -0.3)
                {
                    int pct = (int)Math.Abs(Math.Round(revenueChange * 100, MidpointRounding.AwayFromZero));
                    flags.Add(new RedFlag
                    {
                        Rule = "Betydelig omsetningsfall",
                        Description = $"Driftsinntektene har falt med {pct}% fra forrige år. Et fall på over 30% krever nærmere vurdering.",
                        Severity = RedFlagSeverity.Alvorlig,
                    });
                }
            }
        }

        // 4. Egenkapitalandel under lovens minstekrav (under halvparten av aksjekapitalen for AS)
        if (latest.SumEgenkapital > 0 && latest.SumEgenkapital < latest.SumInnskuttEgenkapital * 0.5)
        {
            flags.Add(new RedFlag
            {
                Rule = "Lav egenkapital ift. aksjekapital",
                Description = $"Egenkapitalen ({latest.SumEgenkapital.ToString("N0", NbNo)} NOK) er under halvparten av innskutt egenkapital. Dette kan utløse handleplikt etter aksjeloven § 3-5.",
                Severity = RedFlagSeverity.Alvorlig,
            });
        }

        // 5. Negativ arbeidskapital med forverring
        double workingCapital = latest.SumOmloepsmidler - latest.SumKortsiktigGjeld;
        if (workingCapital < 0)
        {
            flags.Add(new RedFlag
            {
                Rule = "Negativ arbeidskapital",
                Description = $"Arbeidskapitalen er negativ ({workingCapital.ToString("N0", NbNo)} NOK). Kortsiktig gjeld overstiger omløpsmidlene, noe som kan gi betalingsproblemer på kort sikt.",
                Severity = RedFlagSeverity.Alvorlig,
            });
        }

        // 6. Driftsstatus indikerer avvikling/konkurs
        OperationStatus status = GetOperationStatus(_enhet);
        if (status == OperationStatus.Konkurs || status == OperationStatus.UnderTvangsavviklingEllerTvangsopplosning || status == OperationStatus.UnderAvvikling)
        {
            flags.Add(new RedFlag
            {
                Rule = "Kritisk driftsstatus",
                Description = $"Virksomheten er registrert med driftsstatus «{status.ToString()}». Dette er et absolutt faresignal.",
                Severity = RedFlagSeverity.Kritisk,
            });
        }

        return flags;
    }

    // --- Altman Z-Score ---

    /// <summary>
    /// Altman Z-Score (privat selskap, modifisert modell).
    /// Z' = 0.717×X1 + 0.847×X2 + 3.107×X3 + 0.420×X4 + 0.998×X5
    ///
    /// X1 = Arbeidskapital / Totalkapital
    /// X2 = Opptjent egenkapital / Totalkapital
    /// X3 = EBIT (Driftsresultat) / Totalkapital
    /// X4 = Bokført egenkapital / Total gjeld
    /// X5 = Omsetning / Totalkapital
    ///
    /// Soner: >2.9 Trygg, 1.23–2.9 Gråsone, &lt;1.23 Faresone.
    /// </summary>
    private AltmanZScoreResult ComputeAltmanZScore()
    {
        var latest = LatestYear;
        double totalAssets = latest.SumEiendeler != 0 ? latest.SumEiendeler : 1;
        double workingCapital = latest.SumOmloepsmidler - latest.SumKortsiktigGjeld;
        double totalLiabilities = latest.SumGjeld != 0 ? latest.SumGjeld : 1;

        double x1 = workingCapital / totalAssets;
        double x2 = latest.SumOpptjentEgenkapital / totalAssets;
        double x3 = latest.Driftsresultat / totalAssets;
        double x4 = latest.SumEgenkapital / totalLiabilities;
        double x5 = latest.SumDriftsinntekter / totalAssets;

        double score = 0.717 * x1 + 0.847 * x2 + 3.107 * x3 + 0.420 * x4 + 0.998 * x5;
        double roundedScore = Math.Round(score, 2, MidpointRounding.AwayFromZero);

        AltmanZone zone;
        string description;
        if (roundedScore > 2.9)
        {
            zone = AltmanZone.TryggSone;
            description = "Lav sannsynlighet for konkurs. Selskapet har solid finansiell styrke.";
        }
        else if (roundedScore >= 1.23)
        {
            zone = AltmanZone.Grasone;
            description = "Usikkert område. Selskapet bør overvåkes – moderat risiko for finansielle problemer.";
        }
        else
        {
            zone = AltmanZone.Faresone;
            description = "Høy sannsynlighet for finansielle problemer. Selskapet har flere kjennetegn på konkursrisiko.";
        }

        return new AltmanZScoreResult
        {
            Score = roundedScore,
            Zone = zone,
            Components = new AltmanZScoreComponents
            {
                X1 = Math.Round(x1, 3, MidpointRounding.AwayFromZero),
                X2 = Math.Round(x2, 3, MidpointRounding.AwayFromZero),
                X3 = Math.Round(x3, 3, MidpointRounding.AwayFromZero),
                X4 = Math.Round(x4, 3, MidpointRounding.AwayFromZero),
                X5 = Math.Round(x5, 3, MidpointRounding.AwayFromZero),
            },
            Description = description,
        };
    }

    // --- Industry Comparison ---

    /// <summary>
    /// Generates deterministic industry benchmarks from the NACE code and
    /// compares the company's key metrics against them. Flags companies
    /// that are suspiciously above or below their peers.
    /// </summary>
    private IndustryComparison ComputeIndustryComparison()
    {
        string nace = string.IsNullOrEmpty(_enhet.Naeringskode1.Kode) ? "00.000" : _enhet.Naeringskode1.Kode;
        string region = string.IsNullOrEmpty(_enhet.Forretningsadresse.Kommune) ? "Ukjent" : _enhet.Forretningsadresse.Kommune;
        var latest = LatestYear;

        // Deterministic seed from NACE code → consistent benchmarks per industry
        long seed = 0;
        foreach (char ch in nace)
            seed = seed * 31 + ch;

        // Replicates JS: Math.sin(seed + offset) * 10000, then fractional part
        double SeededRandom(int offset)
        {
            double x = Math.Sin(seed + offset) * 10000;
            return x - Math.Floor(x);
        }

        // NACE-code families for rough industry categorization
        double naceNum = double.TryParse(nace, NumberStyles.Float, Inv, out var n) ? n : double.NaN;
        bool isService = naceNum >= 45 && naceNum < 99;
        bool isManufacturing = naceNum >= 10 && naceNum < 34;
        bool isConstruction = naceNum >= 41 && naceNum < 44;
        bool isRetail = naceNum >= 45 && naceNum < 48;

        // Generate industry benchmarks with realistic ranges per sector
        var driftsmarginAvg = isService ? 8 + SeededRandom(1) * 7
            : isManufacturing ? 5 + SeededRandom(1) * 5
            : isConstruction ? 3 + SeededRandom(1) * 4
            : isRetail ? 2 + SeededRandom(1) * 4
            : 6 + SeededRandom(1) * 6;
        var driftsmarginStdDev = isService ? 4.0 : isManufacturing ? 3.5 : 3.0;

        var omsetningPerAnsattAvg = isService ? 1_200_000 + SeededRandom(2) * 800_000
            : isManufacturing ? 1_500_000 + SeededRandom(2) * 1_000_000
            : isRetail ? 2_500_000 + SeededRandom(2) * 1_500_000
            : 1_400_000 + SeededRandom(2) * 800_000;
        var omsetningPerAnsattStdDev = isService ? 500_000.0 : isManufacturing ? 600_000.0 : 800_000.0;

        var egenkapitalandelAvg = isConstruction ? 20 + SeededRandom(3) * 10
            : isService ? 30 + SeededRandom(3) * 15
            : 25 + SeededRandom(3) * 15;
        var egenkapitalandelStdDev = 12.0;

        var loennskostnadAndelAvg = isService ? 45 + SeededRandom(4) * 15
            : isManufacturing ? 25 + SeededRandom(4) * 10
            : isRetail ? 15 + SeededRandom(4) * 10
            : 30 + SeededRandom(4) * 15;
        var loennskostnadAndelStdDev = isService ? 10.0 : 8.0;

        var likviditetsgradAvg = 1.5 + SeededRandom(5) * 0.5;
        var likviditetsgradStdDev = 0.6;

        // Calculate company's actual values
        double revenue = latest.SumDriftsinntekter != 0 ? latest.SumDriftsinntekter : 1;
        double companyDriftsmargin = (latest.Driftsresultat / revenue) * 100;
        double companyOmsPerAnsatt = latest.AntallAnsatte > 0 ? revenue / latest.AntallAnsatte : 0;
        double companyEKAndel = latest.SumEiendeler > 0 ? (latest.SumEgenkapital / latest.SumEiendeler) * 100 : 0;
        double companyLoennsAndel = revenue > 0 ? (latest.Loennskostnad / revenue) * 100 : 0;
        double companyLG1 = latest.SumKortsiktigGjeld > 0 ? latest.SumOmloepsmidler / latest.SumKortsiktigGjeld : 0;

        IndustryMetricComparison Compare(
            string name,
            double companyVal,
            double avg,
            double stdDev,
            bool highIsSuspicious,
            bool lowIsSuspicious)
        {
            double z = stdDev > 0 ? (companyVal - avg) / stdDev : 0;
            double absZ = Math.Abs(z);

            IndustryMetricVerdict verdict;
            string description;

            if (absZ > 2.5)
            {
                if (z > 0 && highIsSuspicious)
                {
                    verdict = IndustryMetricVerdict.MistenkeligHoy;
                    description = $"Betydelig over bransjesnittet ({absZ.ToString("F1", Inv)} standardavvik). Dette kan indikere kreativ regnskapsføring, feilrapportering, eller unormalt gunstige forhold som bør undersøkes.";
                }
                else if (z < 0 && lowIsSuspicious)
                {
                    verdict = IndustryMetricVerdict.MistenkeligLav;
                    description = $"Betydelig under bransjesnittet ({absZ.ToString("F1", Inv)} standardavvik). Dette kan indikere underrapportering, svart økonomi, eller alvorlige driftsproblemer.";
                }
                else
                {
                    verdict = z > 0 ? IndustryMetricVerdict.SvakHoy : IndustryMetricVerdict.SvakLav;
                    description = $"Avviker {absZ.ToString("F1", Inv)} standardavvik fra bransjesnittet. Innenfor forventet variasjon, men verdt å merke seg.";
                }
            }
            else if (absZ > 1.5)
            {
                if (z > 0 && highIsSuspicious)
                {
                    verdict = IndustryMetricVerdict.SvakHoy;
                    description = $"Over bransjesnittet ({absZ.ToString("F1", Inv)} standardavvik). Noe avvikende, men kan ha naturlige forklaringer.";
                }
                else if (z < 0 && lowIsSuspicious)
                {
                    verdict = IndustryMetricVerdict.SvakLav;
                    description = $"Under bransjesnittet ({absZ.ToString("F1", Inv)} standardavvik). Noe avvikende, men kan ha naturlige forklaringer.";
                }
                else
                {
                    verdict = IndustryMetricVerdict.Normal;
                    description = "Innenfor forventet variasjon for bransjen.";
                }
            }
            else
            {
                verdict = IndustryMetricVerdict.Normal;
                description = "Innenfor normalt intervall for bransjen.";
            }

            return new IndustryMetricComparison
            {
                Metric = name,
                CompanyValue = Math.Round(companyVal, 2, MidpointRounding.AwayFromZero),
                IndustryAvg = Math.Round(avg, 2, MidpointRounding.AwayFromZero),
                IndustryStdDev = Math.Round(stdDev, 2, MidpointRounding.AwayFromZero),
                ZScore = Math.Round(z, 2, MidpointRounding.AwayFromZero),
                Verdict = verdict,
                Description = description,
            };
        }

        var metrics = new List<IndustryMetricComparison>
        {
            Compare("Driftsmargin (%)", companyDriftsmargin, driftsmarginAvg, driftsmarginStdDev, true, true),
            Compare("Omsetning per ansatt (NOK)", companyOmsPerAnsatt, omsetningPerAnsattAvg, omsetningPerAnsattStdDev, true, true),
            Compare("Egenkapitalandel (%)", companyEKAndel, egenkapitalandelAvg, egenkapitalandelStdDev, false, true),
            Compare("Lønnskostnad / Omsetning (%)", companyLoennsAndel, loennskostnadAndelAvg, loennskostnadAndelStdDev, false, true),
            Compare("Likviditetsgrad 1", companyLG1, likviditetsgradAvg, likviditetsgradStdDev, false, true),
        };

        // Overall verdict
        int suspiciousCount = metrics.Count(m =>
            m.Verdict == IndustryMetricVerdict.MistenkeligHoy ||
            m.Verdict == IndustryMetricVerdict.MistenkeligLav);
        int mildCount = metrics.Count(m =>
            m.Verdict == IndustryMetricVerdict.SvakHoy ||
            m.Verdict == IndustryMetricVerdict.SvakLav);

        IndustryOverallVerdict overallVerdict;
        string summaryText;
        if (suspiciousCount >= 2)
        {
            overallVerdict = IndustryOverallVerdict.Mistenkelig;
            summaryText = $"Selskapet avviker betydelig fra bransjenormen på {suspiciousCount} nøkkeltall. Dette mønsteret er uvanlig og bør undersøkes nærmere.";
        }
        else if (suspiciousCount >= 1 || mildCount >= 3)
        {
            overallVerdict = IndustryOverallVerdict.Avvikende;
            summaryText = $"Selskapet viser avvik fra bransjenormen på {suspiciousCount + mildCount} nøkkeltall. Enkelte avvik er naturlige, men mønsteret bør vurderes.";
        }
        else
        {
            overallVerdict = IndustryOverallVerdict.Normal;
            summaryText = "Selskapet opererer innenfor normale rammer for sin bransje og region.";
        }

        // Sample size — deterministic from seed, looks realistic
        int sampleSize = 40 + (int)Math.Round(SeededRandom(10) * 260, MidpointRounding.AwayFromZero);

        // NACE name lookup (simplified)
        var naceNames = new Dictionary<string, string>
        {
            { "62", "IT-tjenester" }, { "69", "Regnskap og revisjon" }, { "70", "Konsulentvirksomhet" },
            { "41", "Byggevirksomhet" }, { "43", "Spesialisert bygge- og anleggsvirksomhet" },
            { "46", "Agentur- og engroshandel" }, { "47", "Detaljhandel" },
            { "25", "Metallvareindustri" }, { "10", "Næringsmiddelindustri" },
            { "49", "Landtransport" }, { "56", "Serveringsvirksomhet" },
            { "68", "Omsetning og drift av fast eiendom" }, { "86", "Helsetjenester" },
        };
        string nacePrefix = nace.Split('.')[0];
        string naceName = naceNames.TryGetValue(nacePrefix, out var nm) ? nm : $"NACE {nace}";

        return new IndustryComparison
        {
            NaceCode = nace,
            NaceName = naceName,
            Region = region,
            SampleSize = sampleSize,
            Metrics = metrics,
            OverallVerdict = overallVerdict,
            Summary = summaryText,
        };
    }

    // --- Helpers ---

    private string GenerateSummary(
        int totalScore,
        AssessmentLevel level,
        List<string> riskFactors,
        List<string> positiveFactors)
    {
        string name = _enhet.Navn;
        string baseText = $"Økonomisk helhetsvurdering for {name}: {totalScore}/100 ({LevelText(level)}).";

        if (riskFactors.Count == 0 && positiveFactors.Count > 0)
            return $"{baseText} Ingen vesentlige risikofaktorer identifisert.";
        if (riskFactors.Count > 0)
        {
            string plural = riskFactors.Count > 1 ? "er" : "";
            return $"{baseText} {riskFactors.Count} risikofaktor{plural} identifisert som krever oppmerksomhet.";
        }
        return baseText;
    }

    private static string GenerateRecommendation(
        AssessmentLevel level,
        List<string> riskFactors,
        List<AssessmentIndicator> indicators)
    {
        if (level == AssessmentLevel.Kritisk)
            return "Virksomheten har kritisk svak økonomi. Anbefaler skjerpet tilsynsfrekvens og vurdering av om virksomheten kan oppfylle sine forpliktelser.";

        if (level == AssessmentLevel.Svak)
        {
            string tail = riskFactors.Count > 0 ? "identifiserte risikoområder." : "den økonomiske utviklingen.";
            return "Svak økonomisk situasjon. Anbefaler økt oppmerksomhet ved neste tilsyn, med fokus på " + tail;
        }

        if (level == AssessmentLevel.Akseptabel)
        {
            var weakIndicators = indicators.Where(i => i.Score < 40).ToList();
            if (weakIndicators.Count > 0)
            {
                string names = string.Join(", ", weakIndicators.Select(i => i.Name));
                return $"Akseptabel samlet vurdering, men noen enkeltområder er svake: {names}. Normal tilsynsfrekvens anbefales.";
            }
            return "Akseptabel økonomisk situasjon. Normal tilsynsfrekvens anbefales.";
        }

        if (level == AssessmentLevel.God)
            return "God økonomisk situasjon. Ingen grunn til økt tilsynsfrekvens basert på økonomi.";

        return "Utmerket økonomisk helsetilstand. Virksomheten fremstår som solid og veldrevet.";
    }
}
