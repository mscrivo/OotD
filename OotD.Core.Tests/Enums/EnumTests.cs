using OotD.Enums;

namespace OotD.Core.Tests.Enums;

public class EnumTests
{
    [Fact]
    public void CurrentCalendarView_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<CurrentCalendarView>().Should().Contain([
            CurrentCalendarView.Day,
            CurrentCalendarView.Week,
            CurrentCalendarView.WorkWeek,
            CurrentCalendarView.Month
        ]);
    }

    [Theory]
    [InlineData(CurrentCalendarView.Day, 0)]
    [InlineData(CurrentCalendarView.Week, 1)]
    [InlineData(CurrentCalendarView.Month, 2)]
    [InlineData(CurrentCalendarView.WorkWeek, 4)]
    public void CurrentCalendarView_ShouldHaveCorrectIntegerValues(CurrentCalendarView view, int expectedValue)
    {
        // Assert
        ((int)view).Should().Be(expectedValue);
    }

    [Fact]
    public void FolderViewType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<FolderViewType>().Should().Contain([
            FolderViewType.Calendar,
            FolderViewType.Contacts,
            FolderViewType.Inbox,
            FolderViewType.Notes,
            FolderViewType.Tasks,
            FolderViewType.Todo
        ]);
    }

    [Theory]
    [InlineData(FolderViewType.Calendar)]
    [InlineData(FolderViewType.Contacts)]
    [InlineData(FolderViewType.Inbox)]
    [InlineData(FolderViewType.Notes)]
    [InlineData(FolderViewType.Tasks)]
    [InlineData(FolderViewType.Todo)]
    public void FolderViewType_AllValuesShouldBeDefinedAndValid(FolderViewType folderType)
    {
        // Assert
        Enum.IsDefined(typeof(FolderViewType), folderType).Should().BeTrue();
        folderType.ToString().Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(CurrentCalendarView.Day)]
    [InlineData(CurrentCalendarView.Week)]
    [InlineData(CurrentCalendarView.WorkWeek)]
    [InlineData(CurrentCalendarView.Month)]
    public void CurrentCalendarView_AllValuesShouldBeDefinedAndValid(CurrentCalendarView view)
    {
        // Assert
        Enum.IsDefined(typeof(CurrentCalendarView), view).Should().BeTrue();
        view.ToString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CurrentCalendarView_ShouldNotHaveInvalidValues()
    {
        // Arrange
        const CurrentCalendarView invalidView = (CurrentCalendarView)999;

        // Assert
        Enum.IsDefined(typeof(CurrentCalendarView), invalidView).Should().BeFalse();
    }

    [Fact]
    public void FolderViewType_ShouldNotHaveInvalidValues()
    {
        // Arrange
        const FolderViewType invalidType = (FolderViewType)999;

        // Assert
        Enum.IsDefined(typeof(FolderViewType), invalidType).Should().BeFalse();
    }

    [Fact]
    public void EnumValues_ShouldBeStable()
    {
        // This test ensures enum values don't accidentally change
        // which could break serialization or configuration
        
        // Assert CurrentCalendarView stability
        CurrentCalendarView.Day.Should().Be((CurrentCalendarView)0);
        CurrentCalendarView.Week.Should().Be((CurrentCalendarView)1);
        CurrentCalendarView.Month.Should().Be((CurrentCalendarView)2);
        CurrentCalendarView.WorkWeek.Should().Be((CurrentCalendarView)4);
        
        // Assert FolderViewType names are meaningful
        FolderViewType.Calendar.ToString().Should().Be("Calendar");
        FolderViewType.Contacts.ToString().Should().Be("Contacts");
        FolderViewType.Inbox.ToString().Should().Be("Inbox");
        FolderViewType.Notes.ToString().Should().Be("Notes");
        FolderViewType.Tasks.ToString().Should().Be("Tasks");
        FolderViewType.Todo.ToString().Should().Be("Todo");
    }
}
