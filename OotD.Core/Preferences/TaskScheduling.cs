using System;
using System.IO;
using Microsoft.Win32.TaskScheduler;
using NLog;

namespace OotD.Preferences;

internal static class TaskScheduling
{
    private const string OotDSchedTaskDefinitionName = "Outlook on the Desktop";
    private const string OotDSchedTaskDefinitionXMLPath = @"OotDScheduledTaskDefinition.xml";

    internal static ITaskServiceAdapter TaskServiceAdapter { get; set; } = new DefaultTaskServiceAdapter();

    public static bool OotDScheduledTaskExists()
    {
        return TaskServiceAdapter.TaskExists(OotDSchedTaskDefinitionName);
    }

    public static void CreateOotDStartupTask(Logger logger)
    {
        try
        {
            logger.Info($"Creating {OotDSchedTaskDefinitionName} Scheduled Task");
            TaskServiceAdapter.CreateStartupTaskDefinition(
                OotDSchedTaskDefinitionName,
                OotDSchedTaskDefinitionXMLPath,
                Environment.UserName);
        }
        catch (Exception e)
        {
            logger.Error(e, "Error while trying to create startup scheduled task.");
            throw;
        }
    }

    /// <summary>
    ///     Used by the installer (-s) on every install and upgrade. Only (re)creates the task when it's missing
    ///     or its action points at an executable that no longer exists, so a user's customised task survives updates.
    /// </summary>
    public static void EnsureOotDStartupTask(Logger logger)
    {
        if (TaskServiceAdapter.TaskActionTargetExists(OotDSchedTaskDefinitionName))
        {
            logger.Info($"{OotDSchedTaskDefinitionName} Scheduled Task already exists; leaving it unchanged");
            return;
        }

        CreateOotDStartupTask(logger);
    }

    public static void RemoveOotDStartupTask(Logger logger)
    {
        try
        {
            logger.Info($"Removing {OotDSchedTaskDefinitionName} Scheduled Task");
            if (TaskServiceAdapter.TaskExists(OotDSchedTaskDefinitionName))
            {
                TaskServiceAdapter.DeleteTask(OotDSchedTaskDefinitionName);
            }
        }
        catch (Exception e)
        {
            logger.Error(e, "Error while trying to remove startup scheduled task.");
            throw;
        }
    }

    internal interface ITaskServiceAdapter
    {
        bool TaskExists(string taskName);

        /// <summary>False if the task is missing or its program is a full path to a file that doesn't exist.</summary>
        bool TaskActionTargetExists(string taskName);

        void CreateStartupTaskDefinition(string taskName, string xmlPath, string userName);
        void DeleteTask(string taskName);
    }

    private sealed class DefaultTaskServiceAdapter : ITaskServiceAdapter
    {
        public bool TaskExists(string taskName)
        {
            return TaskService.Instance.GetTask(taskName) != null;
        }

        public bool TaskActionTargetExists(string taskName)
        {
            using var task = TaskService.Instance.GetTask(taskName);
            if (task == null)
            {
                return false;
            }

            if (task.Definition.Actions.Count == 0 || task.Definition.Actions[0] is not ExecAction execAction)
            {
                // Not something we'd have created; assume the user set it up deliberately.
                return true;
            }

            var path = Environment.ExpandEnvironmentVariables(execAction.Path.Trim('"'));

            // Bare names (e.g. powershell.exe running a wrapper script) resolve via PATH; trust them.
            return !Path.IsPathRooted(path) || File.Exists(path);
        }

        public void CreateStartupTaskDefinition(string taskName, string xmlPath, string userName)
        {
            using var ts = new TaskService();
            var taskDefinition = ts.NewTaskFromFile(xmlPath);
            var logonTrigger = (LogonTrigger)taskDefinition.Triggers[0];
            logonTrigger.UserId = userName;
            ts.RootFolder.RegisterTaskDefinition(taskName, taskDefinition);
        }

        public void DeleteTask(string taskName)
        {
            TaskService.Instance.RootFolder.DeleteTask(taskName);
        }
    }
}
