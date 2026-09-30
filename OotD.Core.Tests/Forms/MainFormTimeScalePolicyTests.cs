namespace OotD.Core.Tests.Forms;

using System.Xml.Linq;
using OotD.Forms;

public class MainFormTimeScalePolicyTests
{
    private const string DayXml = """
                                  <?xml version="1.0"?>
                                  <view type="calendar">
                                      <viewname>Calendar</viewname>
                                      <mode>0</mode>
                                  </view>
                                  """;

    private static string OutlookViewXml(string timeUnits) =>
        $"""
         <?xml version="1.0"?>
         <view type="calendar">
             <viewname>Calendar</viewname>
             <mode>2</mode>
             <displaytimeunits>{timeUnits}</displaytimeunits>
         </view>
         """;

    private static string? TimeUnitsOf(string viewXml) =>
        XDocument.Parse(viewXml).Root?.Element("displaytimeunits")?.Value;

    [Theory]
    [InlineData("15")]
    [InlineData("5")]
    [InlineData("60")]
    public void ApplyOutlookTimeScale_CopiesOutlookTimeScaleIntoBuiltInView(string timeUnits)
    {
        var result = MainFormViewXmlPolicy.ApplyOutlookTimeScale(DayXml, OutlookViewXml(timeUnits));

        TimeUnitsOf(result).Should().Be(timeUnits);
        XDocument.Parse(result).Root!.Element("mode")!.Value.Should().Be("0");
        result.Should().StartWith("<?xml version=\"1.0\"?>");
    }

    [Fact]
    public void ApplyOutlookTimeScale_ReplacesStaleTimeScaleInSavedView()
    {
        var saved = MainFormViewXmlPolicy.ApplyOutlookTimeScale(DayXml, OutlookViewXml("30"));

        var result = MainFormViewXmlPolicy.ApplyOutlookTimeScale(saved, OutlookViewXml("15"));

        TimeUnitsOf(result).Should().Be("15");
        XDocument.Parse(result).Root!.Elements("displaytimeunits").Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not xml")]
    [InlineData("<view type=\"calendar\"><mode>0</mode></view>")]
    [InlineData("<view type=\"table\"><displaytimeunits>15</displaytimeunits></view>")]
    [InlineData("<view type=\"calendar\"><displaytimeunits>abc</displaytimeunits></view>")]
    [InlineData("<view type=\"calendar\"><displaytimeunits>0</displaytimeunits></view>")]
    [InlineData("<view type=\"calendar\"><displaytimeunits>-15</displaytimeunits></view>")]
    public void ApplyOutlookTimeScale_WithoutUsableOutlookTimeScale_LeavesViewUnchanged(string? outlookViewXml)
    {
        MainFormViewXmlPolicy.ApplyOutlookTimeScale(DayXml, outlookViewXml).Should().Be(DayXml);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not xml")]
    [InlineData("<view type=\"table\"><viewname>Messages</viewname></view>")]
    public void ApplyOutlookTimeScale_WhenTargetIsNotACalendarView_LeavesItUnchanged(string viewXml)
    {
        MainFormViewXmlPolicy.ApplyOutlookTimeScale(viewXml, OutlookViewXml("15")).Should().Be(viewXml);
    }
}
