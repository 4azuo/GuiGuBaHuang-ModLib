# GuiGuBaHuang-ModLib Installation Guide

Quick installation guide for setting up the GuiGuBaHuang-ModLib development environment.

---

## Prerequisites

### System Requirements

- **Operating System**: Windows 10/11 (64-bit)
- **Administrator Privileges**: Required for installation
- **Disk Space**: At least 1 GB free space
- **Internet Connection**: Required for downloading dependencies

### Tools (will be installed automatically)

- .NET Framework 4.7.2 Developer Pack
- .NET 9 SDK
- Python (latest)
- Git
- GitHub CLI
- Notepad++

---

## Installation Steps

### Step 1: Download Installation Scripts

Download these 3 files to a temporary folder (e.g., `C:\temp\modlib-install`):

- `install.ps1`
- `00-install-dependencies.ps1`
- `01-clone-repository.ps1`

### Step 2: Run as Administrator

Open **PowerShell as Administrator** (Win + X → "Terminal (Admin)"):

```powershell
cd C:\temp\modlib-install
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
.\install.ps1
```

### Step 3: Follow Installation Prompts

The script will automatically:

1. ✅ Create System Restore Point (optional)
2. ✅ Install all dependencies via Chocolatey
3. ✅ Clone repository to `C:\git\GuiGuBaHuang-ModLib`
4. ✅ Initialize Git submodules
5. ✅ Open configuration files in Notepad

### Step 4: Configure Settings

The script opens these files in **Notepad** for manual configuration:

1. **3161035078\tasks\settings.json** - Taoist mod settings
2. **3385996759\tasks\settings.json** - ModLib settings
3. **GuiGuBaHuang-ModCreator\ModCreator\Resources\settings.json** - ModCreator settings

### Step 5: Done!

Installation complete. Repository is at: `C:\git\GuiGuBaHuang-ModLib`

**Optional Parameters:**

```powershell
# Skip restore point, skip starring, auto-open all files
.\install.ps1 -SkipCheckpoint -SkipStar -OpenAll
```

---

## Support

- **Repository:** https://github.com/4azuo/GuiGuBaHuang-ModLib
- **Issues:** https://github.com/4azuo/GuiGuBaHuang-ModLib/issues
- **Author:** 4azuo

---

**Last Updated:** December 4, 2025
