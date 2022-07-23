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

    public static bool OotDScheduledTaskExists()
    {
        return TaskService.Instance.GetTask(OotDSchedTaskDefinitionName) != null;
    }

    public static void CreateOotDStartupTask(Logger logger)
    {
        try
        {
            using var ts = new TaskService();
            logger.Info($"Creating {OotDSchedTaskDefinitionName} Scheduled Task");
            var td = ts.NewTaskFromFile(OotDSchedTaskDefinitionXMLPath);
            var logonTrigger = (LogonTrigger)td.Triggers[0];
            logonTrigger.UserId = Environment.UserName;
            ts.RootFolder.RegisterTaskDefinition(OotDSchedTaskDefinitionName, td);
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
            var td = TaskService.Instance.GetTask(OotDSchedTaskDefinitionName);
            if (td != null)
            {
                TaskService.Instance.RootFolder.DeleteTask(OotDSchedTaskDefinitionName);
            }

        }
        catch (Exception e)
        {
            logger.Error(e, "Error while trying to remove startup scheduled task.");
            throw;
        }
    }
}