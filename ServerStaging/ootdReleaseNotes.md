# Outlook on the Desktop Release Notes

## 3.5.2
* Fixed additional HiDPI scaling issues (courtesy of .NET 4.7 Framework).
* Fixed an issue where update release notes did not render properly.
* Added additional logging

## 3.5.1
* Fixed issue where instances would not show up on monitors that had difference scaling factor than main monitor.
* Fixed issue where calendar buttons were not showing up on custom calendars.

## 3.5.0
* Updated to use .NET 4.6.2 for even better HiDPI support
* Fixed laggy resizing of windows
* Fixed flickering when moving/resizing of windows
* Misc fixes and performance enhancements to installer.

## 3.4.0
* Added "new email" icon in Inbox view that allows you to compose a new email without having to go into Outlook.
* Fixed issue where toolbar icons did not appear when using a custom folder view.
* Fixed issue where Outlook app running in background was not being closed when shutting down OotD and you are using multiple instances.
* Fixed issue where OotD would crash if you chose a view type that your version of Outlook does not support.
* Upgraded to use .NET 4.6.1 Framework
* Misc performance enhancements.

## 3.3.0

* Upgraded to use .NET 4.6 Framework
* Added ability to view To-Do List

## 3.2.6

* Fixed an issue where OotD would not start up in for particular Office configurations.  Office 365 being one of them.
* Fixed an issue where an instance window could not be moved if configured to be anything other than the calendar view.

## 3.2.5

* Fixed issue where OotD wouldn't start on systems with older versions of Office (notably 2007 and 2003)

## 3.2.4

* Fixed issue introduced in 3.2.1 where having OotD running would make Outlook unresponsive after some time.
* Fixed various usability issues with top header bar.
* Fixed issue where last instance position was not always remembered when starting up (2nd and final attempt).
* Fixed issue where download window on HiDPI displays was cut off at the bottom.

## 3.2.3

* Made instances sticky to make it easier to line them up and organize.
* Fixed issue where last instance position was not always remembered when starting up.

## 3.2.2

* Fixed issue where top control bar would sometimes not be uniform in color.
* Fixed multiple issues with tray icon context menus showing incorrect items or duplicate items.
* Performance improvements in resizing/moving windows.
* Fixed crash when selecting a folder than clicking cancel.
* Other minor fixes and internal component upgrades.

## 3.2.1

* Fixed issue where HighDPI improvements in .NET 4.5.2 were not enabled.
* Streamlined installation/upgrade process.  Will now offer to close app automatically if it is running.

## 3.2.0

* Updated from .NET Framework 4.0 to 4.5.2.  This not only fixes several HiDPI issues, but also gives a nice boost in performance.  However, the minimum supported versions of Windows for .NET 4.5.2 is Vista SP2, so this release and going forward, OotD will only work on Windows Vista SP2 and higher.

## 3.1.0

* Added HiDPI support. Requires that you are using a version of Microsoft Office that supports HiDPI as well.

## 3.0.0

* Added buttons for: Go To Today, Previous and Next to the calendar view.  They are context 
sensitive depending on what view you are looking at.
* Replaced text on calendar buttons with images to make it more visually appealing and compact.
* Added currently selected date to the header bar in the calendar view.
* Made transparency slider bar much easier to use.
* Added automatic check for updates functionality.
 * Checks automatically every 20 days, or can be triggered via icon in system tray.
* Added new logging framework to capture failures/program flow to help diagnose issues.
* Fixed several bugs:
 * "Calendar Could Not be Read" no longer pops up when switch calendar views in Outlook 2013
 * Fixed issue where month view would not be the default calendar view for new instances.
 * Fixed issue where moving cursor down from top of the window onto the calendar buttons would continue to show the resize cursor.
 * Fixed issue where "Remove Instance" dialog would pop up behind instance windows.
 * When adding a new instance, its position is now properly saved.