# Changelog

All notable changes to Outlook on the Desktop are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
For releases prior to 5.1.1, see the [GitHub releases page](https://github.com/mscrivo/OotD/releases).

## [Unreleased]

## [5.2.0] - 2026-07-12

### Fixed

- Startup could hang in a busy loop if Outlook's RPC server returned an unexpected COM error; every failure now counts against the ~1 minute retry window.
- Restarting the app (e.g. after changing virtual desktop assignment) could show a false "program is already running" message because the new process raced the old one's shutdown.
- The "Outlook is not running" check now shows its error dialog on the UI thread instead of a timer thread, and no longer misfires during normal shutdown.
- Window opacity is now stored culture-invariantly; on locales that use a comma decimal separator (e.g. German), the saved opacity could be lost when the system locale changed. Legacy values still load.
- The tray icon was rebuilt every second without disposing the old icon, slowly leaking GDI handles; it is now only rebuilt when the day changes and old icons are disposed.
- Opening an instance's context menu could crash in the background "flash" effect if the instance had just been renamed or removed.
- Exiting from an instance's context menu now saves the current view settings, matching the tray menu exit path.
