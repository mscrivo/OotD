// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
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
        void CreateStartupTaskDefinition(string taskName, string xmlPath, string userName);
        void DeleteTask(string taskName);
    }

    private sealed class DefaultTaskServiceAdapter : ITaskServiceAdapter
    {
        public bool TaskExists(string taskName)
        {
            return TaskService.Instance.GetTask(taskName) != null;
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
