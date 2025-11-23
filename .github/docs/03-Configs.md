# Create Config Files

JSON files in `ModConf/` customize game data. ModLib auto-loads on start.

## Using SampleConfs

The `SampleConfs/` folder contains template files for all game config types. These are original game config files that show the correct structure and properties.

**Process:**
1. Find the config type you need in `SampleConfs/` (e.g., `taskBase.json`, `roleCreateFeature.json`)
2. Copy the structure and properties from the sample file
3. Create your config file in `ModConf/` with naming: `<prefix>_<original_name>.json`
4. Add your custom data following the same structure

**Example:**
- Sample: `SampleConfs/taskBase.json`
- Your file: `ModConf/mymod_taskBase.json`

**Important:** Always reference SampleConfs to ensure correct property names, data types, and file structure.

For a complete list of all available config types, see [SampleConfs List](./details/SampleConfs-List.md).
