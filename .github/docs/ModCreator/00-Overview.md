# ModCreator - Overview

## Introduction

ModCreator is a visual WPF tool for creating and managing GuiGuBaHuang game mods without coding. It provides an intuitive interface for all aspects of mod development.

## Key Features

- **Project Management**: Create, edit, delete, and organize mod projects
- **ModConf Editor**: Manage JSON configs with syntax highlighting, find/replace
- **ModImg Manager**: Import/export images (PNG, JPG, BMP, GIF) with preview
- **Global Variables**: Define variables visually, auto-generate C# code
- **ModEvent Editor**: Create events with GUI or code, drag-and-drop interface
- **Integrated Help**: Built-in documentation and guides

## System Requirements

- Windows 10+, .NET 9 SDK
- Visual Studio 2022+ (for development)
- ~100MB disk space

## Quick Start

**Run:**
```powershell
cd GuiGuBaHuang-ModCreator
.\run.ps1
```

**Debug mode:**
```powershell
.\run.ps1 -Debug
```

## Configuration

Edit `ModCreator\Resources\embeded-settings.json`:
- `rootDir`: Path to GuiGuBaHuang-ModLib
- `steamWorkshopDir`: Path to Steam Workshop
- `WorkplacePath`: Workplace directory (auto-created)

## Workflow

1. Launch ModCreator
2. Create/open project
3. Edit configs (ModConf tab)
4. Manage images (ModImg tab)
5. Define variables (Variables tab)
6. Create events (ModEvents tab)
7. Save and deploy

## Detailed Guides

- [01-Project-Management.md](01-Project-Management.md)
- [02-ModConf-Editor.md](02-ModConf-Editor.md)
- [03-ModImg-Manager.md](03-ModImg-Manager.md)
- [04-Global-Variables.md](04-Global-Variables.md)
- [05-ModEvent-Editor.md](05-ModEvent-Editor.md)

## Support

- GitHub: https://github.com/4azuo/GuiGuBaHuang-ModLib
- Issues: GitHub Issues
- Docs: `.github/docs/`
