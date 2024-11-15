using Dan.Plugin.Tilda.Extensions;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Test.Extensions;

public class BrEntityRegisterEntryExtensionsTests
{
    [Fact]
    public void MatchesFilterParameters_ParametersIsNull_ReturnTrue()
    {
        // Arrange
        TildaParameters? tildaParameters = null;

        var brEntity = new BREntityRegisterEntry();

        // Act
        var actual = brEntity.MatchesFilterParameters(tildaParameters);

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void MatchesFilterParameters_ParametersAreNull_ReturnTrue()
    {
        // Arrange
        var tildaParameters = new TildaParameters(
            fromDate: null,
            toDate: null,
            npdid: null,
            includeSubunits: null,
            sourceFilter: null,
            identifier: null,
            filter: null,
            year: null,
            month: null,
            postcode: null,
            municipalityNumber: null,
            nace: null
        );

        var brEntity = new BREntityRegisterEntry();

        // Act
        var actual = brEntity.MatchesFilterParameters(tildaParameters);

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData("nace",null,null,null,false)]
    [InlineData("nace","nope",null,null,false)]
    [InlineData("nace","nope","nope","nope",false)]
    [InlineData("nace","nace",null,null,true)]
    [InlineData("nace","nace","nope","nope",true)]
    [InlineData("nace","nace","nace","nace",true)]
    [InlineData("nace","nacee","nacee","nacee",true)]
    [InlineData("nacee","nace","nace","nace",false)]
    public void MatchesFilterParameters_Theory(
        string nace, string code1, string code2, string code3, bool expected)
    {
        // Arrange
        var tildaParameters = new TildaParameters(
            fromDate: null,
            toDate: null,
            npdid: null,
            includeSubunits: null,
            sourceFilter: null,
            identifier: null,
            filter: null,
            year: null,
            month: null,
            postcode: null,
            municipalityNumber: null,
            nace: nace
        );

        var brEntity = new BREntityRegisterEntry
        {
            Naeringskode1 = new InstitusjonellSektorkode { Kode = code1 },
            Naeringskode2 = new InstitusjonellSektorkode { Kode = code2 },
            Naeringskode3 = new InstitusjonellSektorkode { Kode = code3 },
        };

        // Act
        var actual = brEntity.MatchesFilterParameters(tildaParameters);

        // Assert
        actual.Should().Be(expected);
    }
}
