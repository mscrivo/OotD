# Changelog

All notable changes to Outlook on the Desktop are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
For releases prior to 5.1.1, see the [GitHub releases page](https://github.com/mscrivo/OotD/releases).

## [Unreleased]

## [5.5.0] - 2026-09-25

### Added

- Dark mode: OotD now follows the Windows light/dark app setting for its tray menu, header bar, instance manager and dialogs ([#187](https://github.com/mscrivo/OotD/issues/187))
  - Note: the Outlook calendar and mail content is drawn by Outlook itself and stays light

### Changed

- Redesigned the New Email button icon so it's clearly visible on both light and dark header bars
- Slimmed the window border
- The top edge and top corners of a window are now easier to grab for resizing
- Startup errors now explain what went wrong and how to fix it, instead of showing a raw error:
  - If only the new Outlook for Windows is installed, OotD now says that classic Outlook is required ([#255](https://github.com/mscrivo/OotD/issues/255), [#239](https://github.com/mscrivo/OotD/issues/239))
  - If Outlook can't be started (for example, because Outlook or OotD is running as administrator), OotD explains how to resolve it ([#299](https://github.com/mscrivo/OotD/issues/299), [#211](https://github.com/mscrivo/OotD/issues/211), [#179](https://github.com/mscrivo/OotD/issues/179))
  - If Outlook's components are damaged or not registered, OotD explains how to repair Office ([#224](https://github.com/mscrivo/OotD/issues/224), [#166](https://github.com/mscrivo/OotD/issues/166), [#173](https://github.com/mscrivo/OotD/issues/173))
  - If Outlook is switched to the new Outlook, startup errors say to turn off the "New Outlook" toggle
  - These messages are translated into German, French, Italian, Spanish, Japanese, Portuguese (Brazil) and Chinese (Simplified)

### Fixed

- A window whose saved folder no longer exists (for example, after removing an account) now falls back to the default calendar instead of failing to load ([#151](https://github.com/mscrivo/OotD/issues/151))
- The transparency slider in the header bar is no longer cut off at 100% display scaling
- Reviewed all translations and filled in text that was still showing in English

## [5.3.0] - 2026-08-15

### Changed

- Windows are now properly pinned to the desktop so they stay behind other applications always
  - Note: Win-D (Show Desktop) will still hide the windows, but they will reappear when you restore the desktop
- Re-added code signing thanks to signpath.io!
- Updated to latest dotnet SDK & Runtimes

## [5.2.0] - 2026-07-12

### Fixed

- Startup could hang in a busy loop if Outlook's RPC server returned an unexpected COM error; every failure now counts against the ~1 minute retry window.
- Restarting the app (e.g. after changing virtual desktop assignment) could show a false "program is already running" message because the new process raced the old one's shutdown.
- The "Outlook is not running" check now shows its error dialog on the UI thread instead of a timer thread, and no longer misfires during normal shutdown.
- Window opacity is now stored culture-invariantly; on locales that use a comma decimal separator (e.g. German), the saved opacity could be lost when the system locale changed. Legacy values still load.
- The tray icon was rebuilt every second without disposing the old icon, slowly leaking GDI handles; it is now only rebuilt when the day changes and old icons are disposed.
- Opening an instance's context menu could crash in the background "flash" effect if the instance had just been renamed or removed.
- Exiting from an instance's context menu now saves the current view settings, matching the tray menu exit path.
