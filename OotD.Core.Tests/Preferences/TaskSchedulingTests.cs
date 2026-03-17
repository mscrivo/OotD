using NLog;
using OotD.Preferences;

namespace OotD.Core.Tests.Preferences;

public class TaskSchedulingTests : IDisposable
{
    private readonly FakeTaskServiceAdapter _adapter;
    private readonly TaskScheduling.ITaskServiceAdapter _originalAdapter;
    private readonly Logger _logger;

    public TaskSchedulingTests()
    {
        _adapter = new FakeTaskServiceAdapter();
        _logger = LogManager.GetCurrentClassLogger();
        _originalAdapter = TaskScheduling.TaskServiceAdapter;
        TaskScheduling.TaskServiceAdapter = _adapter;
    }

    [Fact]
    public void OotDScheduledTaskExists_WhenTaskPresent_ShouldReturnTrue()
    {
        _adapter.TaskExistsResult = _ => true;

        var exists = TaskScheduling.OotDScheduledTaskExists();

        exists.Should().BeTrue();
        _adapter.TaskExistsCallCount.Should().Be(1);
        _adapter.LastTaskName.Should().Be("Outlook on the Desktop");
    }

    [Fact]
    public void OotDScheduledTaskExists_WhenTaskMissing_ShouldReturnFalse()
    {
        _adapter.TaskExistsResult = _ => false;

        var exists = TaskScheduling.OotDScheduledTaskExists();

        exists.Should().BeFalse();
        _adapter.TaskExistsCallCount.Should().Be(1);
        _adapter.LastTaskName.Should().Be("Outlook on the Desktop");
    }

    [Fact]
    public void RemoveOotDStartupTask_WhenTaskExists_ShouldDeleteTask()
    {
        _adapter.TaskExistsResult = _ => true;

        TaskScheduling.RemoveOotDStartupTask(_logger);

        _adapter.DeleteTaskCallCount.Should().Be(1);
        _adapter.LastDeletedTaskName.Should().Be("Outlook on the Desktop");
    }

    [Fact]
    public void RemoveOotDStartupTask_WhenTaskDoesNotExist_ShouldNotDeleteTask()
    {
        _adapter.TaskExistsResult = _ => false;

        TaskScheduling.RemoveOotDStartupTask(_logger);

        _adapter.DeleteTaskCallCount.Should().Be(0);
    }

    [Fact]
    public void RemoveOotDStartupTask_WhenAdapterThrows_ShouldRethrow()
    {
        _adapter.TaskExistsResult = _ => throw new InvalidOperationException("boom");

        var action = () => TaskScheduling.RemoveOotDStartupTask(_logger);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateOotDStartupTask_WhenAdapterThrows_ShouldRethrow()
    {
        _adapter.CreateStartupTaskDefinitionAction = (_, _, _) => throw new InvalidOperationException("boom");

        var action = () => TaskScheduling.CreateOotDStartupTask(_logger);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CreateOotDStartupTask_WhenSuccessful_ShouldCreateStartupDefinitionWithExpectedValues()
    {
        TaskScheduling.CreateOotDStartupTask(_logger);

        _adapter.CreateStartupTaskDefinitionCallCount.Should().Be(1);
        _adapter.LastCreatedTaskName.Should().Be("Outlook on the Desktop");
        _adapter.LastCreatedXmlPath.Should().Be("OotDScheduledTaskDefinition.xml");
        _adapter.LastCreatedUserName.Should().Be(Environment.UserName);
    }

    public void Dispose()
    {
        TaskScheduling.TaskServiceAdapter = _originalAdapter;
    }

    private sealed class FakeTaskServiceAdapter : TaskScheduling.ITaskServiceAdapter
    {
        public Func<string, bool> TaskExistsResult { get; set; } = _ => false;
        public Action<string, string, string> CreateStartupTaskDefinitionAction { get; set; } = (_, _, _) => { };

        public int TaskExistsCallCount { get; private set; }
        public string? LastTaskName { get; private set; }

        public int CreateStartupTaskDefinitionCallCount { get; private set; }
        public string? LastCreatedTaskName { get; private set; }
        public string? LastCreatedXmlPath { get; private set; }
        public string? LastCreatedUserName { get; private set; }

        public int DeleteTaskCallCount { get; private set; }
        public string? LastDeletedTaskName { get; private set; }

        public bool TaskExists(string taskName)
        {
            TaskExistsCallCount++;
            LastTaskName = taskName;
            return TaskExistsResult(taskName);
        }

        public void CreateStartupTaskDefinition(string taskName, string xmlPath, string userName)
        {
            CreateStartupTaskDefinitionCallCount++;
            LastCreatedTaskName = taskName;
            LastCreatedXmlPath = xmlPath;
            LastCreatedUserName = userName;
            CreateStartupTaskDefinitionAction(taskName, xmlPath, userName);
        }

        public void DeleteTask(string taskName)
        {
            DeleteTaskCallCount++;
            LastDeletedTaskName = taskName;
        }
    }
}
