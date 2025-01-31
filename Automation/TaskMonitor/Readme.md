## Features

- **Automated Monitoring**: Continuously monitors the specified executable to ensure it is running.
- **Auto-Restart**: If the executable stops running, Task Monitor V2 automatically attempts to restart it.
- **Customizable Configuration**: The executable, its location, arguments, and the check interval can all be configured via a JSON file.
- **Detailed Logging**: Provides logs for process start attempts, errors, and overall monitoring status.
- **Supports Multiple Startup Methods**: Uses `Start-Process` or dot-sourcing to start the executable.

## Prerequisites

- Administrator privileges to start the script.
- **Startup folder will not be triggered until user logs in. SO server MUST be configured to auto login**. Otherwise use task scheduler or services

1. Ensure PowerShell is set to allow script execution. You may need to set the execution policy to `RemoteSigned`:

   ```powershell
   Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

2. Modify the `ExampleApp_Config.json` file to suit your needs. You can rename the file to anything you want but it MUST end with **\_Config.json**

3. Each application you want to run needs own config. You can run multiple applications

## Configuration

All configuration settings are managed in the `ExampleApp_Config.json` file.

### Example Configuration:

```json
{
  "BaseFolder": "C:\\some\\location",
  "ExecutableName": "ExampleApp.exe",
  "CheckInterval": 60,
  "Arguments": "Arg1,Arg2"
}
```

### Configuration Options:

- **BaseFolder**: Path to the folder containing the executable.
- **ExecutableName**: The name of the executable file to monitor.
- **CheckInterval**: The interval, in seconds, between checks to see if the process is running.
- **Arguments**: Any command-line arguments to pass to the executable when it starts.

## Usage

1. Update the `ExampleApp_Config.json` file with the appropriate details for your executable.
2. Run the PowerShell script:

   ```powershell
   .\TaskMonitorV2_ExampleApp.ps1
   ```

3. The script will start monitoring the executable as per the configuration. It will:
   - Check if the executable is running every `CheckInterval` seconds.
   - Attempt to start the executable if it is not running.
   - Log the actions performed, including successes and failures.

### Example Log Output:

```
2024-09-11 15:41:56 [INFO] Working directory: C:\some\location
2024-09-11 15:42:00 [INFO] ExampleApp started successfully.
2024-09-11 15:50:00 [ERROR] ExampleApp not running. Attempting to restart...
```

### Starting with Arguments

If the executable requires arguments, ensure the `Arguments` field in the config is set. The script will pass the arguments to `Start-Process` when launching the executable.

### Methods to Start the Executable

- **Start-Process**: This is the primary method to start the executable. It supports passing arguments.
- **Dot-Sourcing**: A fallback method if `Start-Process` fails. The script runs the executable using PowerShell's dot-sourcing mechanism.

### Pausing the script

If there is a file named `C:\WORKING.txt` the script will not start any program or detect if it is running. Delete the .txt file to continue operating

## Troubleshooting

- **Path Not Found Error**: Ensure that the `BaseFolder` path exists and the `ExecutableName` matches the exact name of the executable (including file extension).
- **Executable Not Starting**: Check the arguments and ensure that they are correct in the `ExampleApp_Config.json`. Also, check the script log for detailed error messages.

---
