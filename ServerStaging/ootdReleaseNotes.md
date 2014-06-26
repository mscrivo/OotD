# Outlook on the Desktop Release Notes

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