## Project Context

Two main projects:
- **3385996759** - ModLib framework (core library)
- **3161035078** - Taoist mod (uses ModLib)

## Working Principles

### Structure
- **ModConf/** - JSON configs (name as `*_<original>.json`, reference SampleConfs)
- **ModCode/** - C# source (compiles to ModCode/dll/)
- **SampleConfs/** - Templates for all config types (project 3385996759)

### Guidelines
- Read instruction files before modifying matching code
- Use existing Helper utilities before creating new ones
- Check SampleConfs when working with configs
- Search for existing patterns before implementing new features
- Read docs from `.github/docs/` for context

### Workflow
- Use `rebuild-and-deploy.ps1` in `tasks/` for build/deploy
- Output: `debug/Mod_xxxxx/ModCode/dll/`
- Batch independent operations when possible
- Use localText.json for all user-facing text

## Documentation

Files in `.github/docs/`:
- **00-Getting-Started.md** - Setup and structure
- **01-ModMain.md** - ModMain class and initialization
- **02-Events.md** - Event system
- **03-Configs.md** - Configuration structure
- **04-LocalText.md** - Localization
- **05-Helpers.md** - Helper functions
- **06-Debug.md** - Debugging
- **07-Build-Deploy.md** - Build scripts
- **details/SampleConfs-List.md** - Config samples list

Reference appropriate docs when answering questions.