# Restart the script with elevated privileges and hidden window if necessary
function Restart-Script {
    param (
        [bool]$runAsAdmin = $false
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "powershell.exe"
    $psi.Arguments = "-ExecutionPolicy Bypass -File `"$PSCommandPath`""

    if ($runAsAdmin) {
        $psi.Verb = "RunAs"
    }

    [System.Diagnostics.Process]::Start($psi)
    exit
}

# Check if running as administrator
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Restart-Script -runAsAdmin $true
    exit
}

$host.UI.RawUI.WindowTitle = "Monitoring Script"

# Is process running
function IsProcess {
    param (
        [string]$ProgramName
    )
    $process = Get-Process -Name $ProgramName -ErrorAction SilentlyContinue
    if ($process) {
        return $true
    }
    else {
        return $false
    }
}

# Logging
$MaxLogLines = 200
function Log-Message {
    param (
        [string]$message,
        [string]$logLevel = "INFO"
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "$timestamp [$logLevel] $message"

    Add-Content -Path $LogFilePath -Value $logEntry
    Write-Host $logEntry

    # Check if log file needs trimming
    $logLines = Get-Content -Path $LogFilePath
    if ($logLines.Count -gt $MaxLogLines) {
        # Keep only the last $MaxLogLines entries
        $logLines = $logLines[ - $MaxLogLines..-1]
        Set-Content -Path $LogFilePath -Value $logLines
    }
}

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$configFiles = Get-ChildItem -Path $scriptDirectory -Filter "*_Config.json" -File

$scriptName = [System.IO.Path]::GetFileNameWithoutExtension($scriptDirectory)
$LogFileName = "$scriptName" + "_Log.txt"
$LogFilePath = Join-Path -Path $scriptDirectory -ChildPath $LogFileName

$programsList = @()
foreach ($configFile in $configFiles) {

    $configPath = Join-Path -Path $scriptDirectory -ChildPath $configFile
    $config = Get-Content $configPath -Force | ConvertFrom-Json

    $BaseFolder = $config.BaseFolder
    $ExecutableName = $config.ExecutableName
    $Arguments = $config.Arguments
    [int]$Priority = 0

    if ($config.PSObject.Properties.Name -contains 'Priority') {
        $success = [int]::TryParse($config.Priority, [ref]$Priority)
    }

    $expectedExecutablePath = Join-Path -Path $BaseFolder -ChildPath $ExecutableName

    # Check if the executable exists at the expected location
    if (-not (Test-Path -Path $expectedExecutablePath)) {
        Log-Message "The expected executable $expectedExecutablePath does not exist, starting search..." "INFO"
        # If not found, proceed with deep search
        $ProgramPath = Get-ChildItem -LiteralPath $BaseFolder -Filter $ExecutableName -Recurse -File -ErrorAction SilentlyContinue |
                       Sort-Object LastWriteTime -Descending |
                       Select-Object -First 1 -ExpandProperty FullName
    } else {
        $ProgramPath = $expectedExecutablePath
        Log-Message "Executable file found at expected location: $ProgramPath"
    }

    if (Test-Path -Path $ProgramPath) {
        Log-Message "Executable file confirmed: $ProgramPath"
    }
    else {
        Log-Message "Executable file does not exist after search: $ProgramPath"
        continue
    }

    $ProgramName = [System.IO.Path]::GetFileNameWithoutExtension($ProgramPath)
    $WorkingDirectory = Split-Path -Path $ProgramPath

    $programInfo = [PSCustomObject]@{
        ProgramPath      = $ProgramPath
        ProgramName      = $ProgramName
        ExecutableName   = $ExecutableName
        WorkingDirectory = $WorkingDirectory
        Arguments        = $Arguments
        Priority         = $Priority
    }
    $programsList += $programInfo
}

# Sort the programs list by Priority from lowest to highest
$programsList = $programsList | Sort-Object -Property Priority

#Minimize the window
Add-Type @"
using System;
using System.Runtime.InteropServices;

public class Win32 {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetForegroundWindow();
}
"@
$SW_MINIMIZE = 6
$hWnd = [Win32]::GetForegroundWindow()
$null = [Win32]::ShowWindow($hWnd, $SW_MINIMIZE) # null will prevent true being printed in console when window is minimized

$criticalOperationFile = "C:\WORKING.txt"
function IsCriticalOperationRunning {
    if (Test-Path $criticalOperationFile -PathType Leaf) {
        $result = $true
    }
    else {
        $result = $false
    }
    $result
}

#Safetycheck: delete leftover operation file at startup just in case
if (IsCriticalOperationRunning){
    Remove-Item $criticalOperationFile -Force
}

Write-Host "--------------------------------------------------------------------------------------------" 

Log-Message "About to sleep for 30 seconds to ensure environment is set"
Start-Sleep -Seconds 30 #Delay before loop
Log-Message "Resumed after sleeping for 30 seconds"


#InfoCheck. This is outside of the loop so it does not get spammed every interval
foreach ($program in $programsList) {
    $IsProcessAlreadyRunning = Get-Process -Name $program.ProgramName -ErrorAction SilentlyContinue
    if ($IsProcessAlreadyRunning) {
        Log-Message "Process '$($program.ProgramName)' is already running." "INFO"
    }
}

$runspacePool = [runspacefactory]::CreateRunspacePool(1, [Environment]::ProcessorCount)
$runspacePool.Open()
# An array to store the runspaces
$runspaces = @()

$CheckInterval = 60

#Execution
while ($true) {
    $delay = 0

    if (IsCriticalOperationRunning) {
        "Execution of critical operation detected. Sleeping for 30 seconds"
        Start-Sleep -Seconds 30
        continue
    } 

    foreach ($program in $programsList) {
        try {
            $name = $program.ProgramName
            $args = $program.Arguments
            $workingDir = $program.WorkingDirectory
            $executable = $program.ExecutableName
            
            if (-not (IsProcess $name)) {

                if (IsCriticalOperationRunning) {
                    break
                }

                Set-Location $workingDir

                if ($executable.EndsWith(".ps1", [StringComparison]::OrdinalIgnoreCase)) {
                    $command = "PowerShell.exe -NoProfile -ExecutionPolicy Bypass -File `"$executable`""
                    if (![string]::IsNullOrEmpty($args)) {
                        $command += " $args"
                    }
                    Invoke-Expression $command
                    Log-Message "${name} Executed." "INFO"
                } else {
                    Log-Message "${name} is not running" "WARNING"
                    Log-Message "Attempting to start: ${$executable}" "INFO"

                    if (-not [string]::IsNullOrEmpty($args)) {
                        Start-Process "${executable}" -ArgumentList "$args" -ErrorAction Stop
                    }
                    else {
                        Start-Process "${executable}" -ErrorAction Stop
                    }
                    Log-Message "Attempted to start ${name} using Start-Process." "INFO"
                    Start-Sleep -Seconds 5
                    $delay = $delay + 5

                    if (IsProcess $name) {
                        Log-Message "${name} started successfully." "INFO"
                    }
                    else {
                        Log-Message "Failed to start ${name}: $_" "ERROR"
                    }
                }
            }
        }
        catch {
            Log-Message "An error occurred while monitoring the program: $_" "ERROR"
        }
    }

    $timeToSleep = $CheckInterval - $delay
    if ($timeToSleep -lt 5) {
        $timeToSleep = 5
    }
    Start-Sleep -Seconds $timeToSleep
}

Log-Message "The monitoring script has exited." "INFO"
