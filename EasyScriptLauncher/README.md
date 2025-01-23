# EasyScriptLauncher

EasyScriptLauncher is a simple utility designed to automatically execute PowerShell scripts located in a specified folder. This application allows you to configure and control the execution of these scripts through a JSON settings file.

> [!IMPORTANT]
> If using this for autostart, do not run this as an admin. It won't start! You will need to use Task Scheduler. Elevated privileges are not required since this app will run scripts with admin rights.

> [!NOTE]
> How to open the autostart folder: Press `Windows logo key + R`, then enter `shell:startup` (for the current user's Startup folder) or `shell:common startup` (for the all-users Startup folder).

## How to Use

**Create and configure the settings**:
   - The application requires a settings file named `EasyScriptLauncher_Settings.json` to be in the same directory as the executable.
   - If this file doesn't exist, the application will create a default one for you.
   - Open the `EasyScriptLauncher_Settings.json` file and adjust the configurations as needed (see the Settings section below).

### EasyScriptLauncher_Settings

* The `EasyScriptLauncher_Settings.json` file controls how the application behaves. Below are the configurable options:
```json
{
    "ScriptsFolder": "A:\\path\\to\\scripts",
    "SearchForScriptsRecursively": false,
    "RunInSameWindow": false,   
    "HideWindow": false,
    "TestBehaviour": false
}
```

## How to Use

**Create and configure the settings**:
   - The application requires a settings file named `EasyScriptLauncher_Settings.json` to be in the same directory as the executable.
   - If this file doesn't exist, the application will create a default one for you.
   - Open the `EasyScriptLauncher_Settings.json` file and adjust the configurations as needed (see the Settings section below).

### EasyScriptLauncher_Settings

* The `EasyScriptLauncher_Settings.json` file controls how the application behaves. Below are the configurable options:
```json
{
    "ScriptsFolder": "A:\\path\\to\\scripts",
    "SearchForScriptsRecursively": false,
    "RunInSameWindow": false,   
    "HideWindow": false,
    "TestBehaviour": false
}
```

* Settings Explanation:
  * **`ScriptsFolder`**: The directory where your PowerShell scripts (`.ps1` files) are located. The application will look for scripts in this folder to execute.
  * **`SearchForScriptsRecursively`**: If set to `true`, the application will search for scripts in all subdirectories within `ScriptsFolder`. If `false`, it only looks in the top-level directory.
  * **`RunInSameWindow`**: If set to `true`, scripts will run in the same console window. If `false`, scripts will run in a separate window.
  * **`HideWindow`**: If `true`, the window where the script runs will be hidden. This is useful if you want the scripts to run in the background without displaying a console window.
  * **`TestBehaviour`**: If `true`, the application will run a dummy command that exits immediately instead of executing the actual scripts. This can be useful for testing purposes. Basically app will do every safetcyheck, will search for scripts but at the end will just fire script with exit code

## How It Works

### Script Execution

- The application reads the configuration from `EasyScriptLauncher_Settings.json`.
- It verifies that the `ScriptsFolder` exists. If the folder is not found, an error message is logged, and the application exits.
- Depending on the `SearchForScriptsRecursively` setting, it either retrieves all PowerShell scripts from the specified folder or all its subfolders.
- For each script found, the application logs the start of execution and then attempts to run the script using `powershell.exe`.
- If `TestBehaviour` is enabled, it runs a simple command that exits immediately instead of the actual script.
- After each script's execution, the application logs whether the script started successfully, completed successfully, or failed to load.

### Logging

- The application logs all its operations to a file named `EasyScriptLauncher_Log.txt`.
- This log file is automatically managed to keep only the latest entries, ensuring it doesn't grow too large.
