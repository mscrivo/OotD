# Outlook on the Desktop

![Latest Version](https://img.shields.io/github/v/release/mscrivo/OotD)
[![Build Status](https://github.com/mscrivo/OotD/actions/workflows/build.yml/badge.svg)](https://github.com/mscrivo/OotD/actions/workflows/build.yml)
[![CodeQL](https://github.com/mscrivo/OotD/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/mscrivo/OotD/actions/workflows/codeql-analysis.yml)
[![GitHub Downloads](https://img.shields.io/github/downloads/mscrivo/OotD/total.svg)](https://github.com/mscrivo/OotD/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Outlook on the Desktop (OotD) is a Windows application that places a live Microsoft Outlook view directly on your desktop. It is designed to keep your calendar and other Outlook content visible all day without needing to keep Outlook in the foreground.

[Official Website](https://outlookonthedesktop.com) | [Download](https://outlookonthedesktop.com/download)

## Screenshots

|                                                                                                                          |                                                                                                                           |
| ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------- |
| ![Outlook on the Desktop calendar view](https://github.com/user-attachments/assets/f32010b5-727e-48f8-9c9f-c3e3dd9e6ea8) | ![Outlook on the Desktop alternate view](https://github.com/user-attachments/assets/24561202-c0e2-471e-8e07-9bedb381339c) |
| ![Outlook on the Desktop toolbar](https://github.com/user-attachments/assets/7d75dcd4-fb53-48c9-afe1-8619a39d828e)       | ![Outlook on the Desktop options dialog](https://github.com/user-attachments/assets/b42989c6-8703-4294-bf20-481ab718cedd) |

## Requirements

- Windows 7 or newer
- Microsoft Outlook 2010 or newer installed

## Supported Languages

Outlook on the Desktop currently includes these UI language resources:

- English (`en-US`)
- German (`de`)
- Spanish (`es`)
- French (`fr`)
- Italian (`it`)
- Portuguese, Brazil (`pt-BR`)
- Japanese (`ja`)
- Chinese, Simplified (`zh-CN`)

---

## Development

### Build Requirements

- .NET 10 SDK for building from source
- Visual Studio 2022 or newer recommended

### What the build produces

- `OotD.Launcher` detects whether Outlook is `x86` or `x64` and starts the matching executable.
- `OotD.Core` contains the main Windows Forms application.
- The installer is built from `Setup Script.iss` during CI.

## Project Layout

| Path               | Purpose                                                                                 |
| ------------------ | --------------------------------------------------------------------------------------- |
| `OotD.Core/`       | Main application code, Windows Forms UI, preferences, controls, and Outlook integration |
| `OotD.Core.Tests/` | Unit and integration-style tests for the core app                                       |
| `OotD.Launcher/`   | Startup launcher that selects the correct executable for Outlook bitness                |
| `OotD.x86/`        | 32-bit build target, files symlinked to OotD.Core                                       |
| `OotD.x64/`        | 64-bit build target, files symlinked to OotD.Core                                       |
| `NetSparkle/`      | Auto-update dependency source                                                           |
| `MACTrackBarLib/`  | Custom trackbar dependency source                                                       |
| `ServerStaging/`   | Staged installer and release assets                                                     |

## Contributing

Issues and pull requests are welcome. If you are changing runtime behavior, UI flows, or installer packaging, validate all of the following before opening a PR:

- The solution builds cleanly in `Release`
- Tests pass locally
- The correct `x86` and `x64` launch behavior still works with Outlook installed
- Installer changes still package successfully

## License

This project is released under the MIT License. See [LICENSE](LICENSE) for details.
