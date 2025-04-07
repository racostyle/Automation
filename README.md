# Automation

## Installation
When installing on a new machine, delete `appsettings.json` and `EasyScriptLauncher_Settings.json` if they are present.

## Development Options
For development options, click the 'Scripts Location' label 5 times. In the development options, updates will:
- Copy/replace `TaskMonitor`.
- Create an `EasyScriptLauncher` shortcut in the startup folder.
- Create/Update `EasyScriptLauncher` settings.

## Overview of Components

### Automation.exe
Designed to simplify setup and configuration editing.

### EasyScriptLauncher.exe
Addresses issues where Windows tasks sometimes fail to trigger on machines with identical environments. This launcher starts all `.ps1` scripts in the `..\Scripts` folder. This should generally only be `TaskMonitor`. This will happen when user logs in the machine.

### TaskMonitor
On activation, `TaskMonitor` checks for all configuration files ending with `_Config.json` in the root folder (the folder where `TaskMonitor` is deployed). It reads these configurations and periodically checks if the executables defined in each configuration are running. It also offers the option to execute shell scripts periodically, which should be placed in `..\Scripts\RecurringScripts`.

## Configuration Options
Use `Automation.exe` for easier creation or editing of configurations:
- **Base Folder**: The root location where the executable should be found. This is usually the folder where the executable resides, but if you have a dynamic folder that is regularly updated, `TaskMonitor` can search in top-level folders for the executable.
- **Executable Name**: Name of the executable file. `Automation.exe` can autofill this and the Base Folder field when creating or editing configurations.
- **Arguments**: Arguments to pass to the executable. Leave blank if unsure.
- **Priority**: The lower the priority number, the lower in the sequence the executable will be checked. Use the default value unless explicitly needed.
- **Interval**: Time in seconds between each check. Use the default value unless a change is necessary, but do not set it below 60 seconds.

### Startup Delay
- When `TaskMonitor` launches, it will sleep for 30 seconds to allow the environment to initialize before beginning monitoring.
