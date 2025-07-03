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

# Get the current process ID
$currentProcessId = $PID
$scriptName = Split-Path -Leaf $MyInvocation.MyCommand.Path
# Check if any other process is running the same script
$existingProcess = Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" | Where-Object {
    $_.CommandLine -like "*$scriptName*" -and $_.ProcessId -ne $currentProcessId
}
# If such a process exists, exit the script
if ($existingProcess) {
    Write-Host "Another instance of '$scriptName' is already running. Exiting..."
    exit
}

# Is process running
function IsProcessByPath {
    param (
        [string]$ProgramPath
    )
    
    $exeName = [System.IO.Path]::GetFileNameWithoutExtension($ProgramPath)
    return Get-Process -Name $exeName -ErrorAction SilentlyContinue |
        Where-Object {
            $_.Path -eq $ProgramPath -or $_.MainModule.FileName -eq $ProgramPath
        }
}

$LOG_TAG_INFO = "INFO"
$LOG_TAG_WARNING = "WARNING"
$LOG_TAG_ERROR = "ERROR"
$LOG_TAG_EXECUTION = "EXECUTION"
$LOG_TAG_OK = "OK"

# Logging
$MaxLogLines = 200
function Log-Message {
    param (
        [string]$message,
        [string]$logLevel
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "$timestamp [$logLevel] $message"

    Add-Content -Path $LogFilePath -Value $logEntry

    switch ($logLevel) {
        $LOG_TAG_WARNING   { Write-Host $logEntry -ForegroundColor Yellow; break }
        $LOG_TAG_ERROR     { Write-Host $logEntry -ForegroundColor Red; break }
        $LOG_TAG_EXECUTION { Write-Host $logEntry -ForegroundColor Cyan; break }
        $LOG_TAG_OK        { Write-Host $logEntry -ForegroundColor Green; break }
        default            { Write-Host $logEntry -ForegroundColor White }
    }

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

$logPrefix = [System.IO.Path]::GetFileNameWithoutExtension($MyInvocation.MyCommand.Name)
$LogFileName = "$logPrefix" + "_Log.txt"
$LogFilePath = Join-Path -Path $scriptDirectory -ChildPath $LogFileName

$programsList = @()
foreach ($configFile in $configFiles) {

    $configPath = Join-Path -Path $scriptDirectory -ChildPath $configFile
    $config = Get-Content $configPath -Force | ConvertFrom-Json

    try {
        $BaseFolder = $config.BaseFolder
        $ExecutableName = $config.ExecutableName
        $Arguments = $config.Arguments
        [int]$Priority = if ($config.PSObject.Properties.Name -contains 'Priority') { [int]$config.Priority } else { 100 }
        [int]$Interval = if ($config.PSObject.Properties.Name -contains 'Interval') { [int]$config.Interval } else { 1 }
    } catch {
        Log-Message "Problem with reading $configFile. Config will be skipped" $LOG_TAG_WARNING
        Write-Host "Error: $_"
        continue
    }

    $expectedExecutablePath = Join-Path -Path $BaseFolder -ChildPath $ExecutableName

    # Check if the executable exists at the expected location
    if (-not (Test-Path -Path $expectedExecutablePath)) {
        Log-Message "The expected executable $expectedExecutablePath does not exist, starting search..." $LOG_TAG_WARNING
        $ProgramPath = Get-ChildItem -LiteralPath $BaseFolder -Filter $ExecutableName -Recurse -File -ErrorAction Stop |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1 -ExpandProperty FullName
    } else {
        $ProgramPath = $expectedExecutablePath
        Log-Message "Executable file found at expected location: $ProgramPath" $LOG_TAG_INFO 
    }

    if (-not $ProgramPath -or -not (Test-Path -Path $ProgramPath)) {
        Log-Message "Executable file does not exist after search: $ProgramPath" $LOG_TAG_ERROR
        Write-Host ""
        continue
    }
    else {
        Log-Message "Executable file confirmed: $ProgramPath" $LOG_TAG_INFO 
        Unblock-File -Path $ProgramPath
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
        BaseInterval     = $Interval
        ModInterval      = 0
    }
    $programsList += $programInfo
    Write-Host ""
}

if ($programsList.Count -gt 1) {
    # Sort the programs list by Priority from lowest to highest. If only one element skip it because it will
    # returned back as astring not an array
    $programsList = $programsList | Sort-Object -Property Priority
    Write-Host ("Program Execution order:")
    foreach ($item in $programsList)
    {
         Write-Host ("Priority: " + $item.Priority + "   | Program: " + $item.ProgramName)
    }
}

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

Write-Host ""
Log-Message "About to sleep for 30 seconds to ensure environment is set" $LOG_TAG_INFO 
Start-Sleep -Seconds 30 #Delay before loop
Log-Message "Resumed after sleeping for 30 seconds" $LOG_TAG_INFO 

#InfoCheck. This is outside of the loop so it does not get spammed every interval
Write-Host ""
foreach ($program in $programsList) {
    if (IsProcessByPath $program.ProgramPath) {
        Log-Message "Process '$($program.ProgramName)' is already running." $LOG_TAG_INFO 
    }
}

Write-Host ""
Write-Host "Main Loop Started"
Write-Host ""

$CheckInterval = 60

#Execution
while ($true) {
    $delay = 0

    for ($i = 0; $i -lt $programsList.Count; $i++) {

        $programsList[$i].ModInterval--
        if ($programsList[$i].ModInterval -gt 0) { continue }
        $programsList[$i].ModInterval = $programsList[$i].BaseInterval

        try {
            $name = $programsList[$i].ProgramName
            $args = $programsList[$i].Arguments
            $workingDir = $programsList[$i].WorkingDirectory
            $executable = $programsList[$i].ExecutableName
            $programPath = $programsList[$i].ProgramPath

            if (-not (IsProcessByPath $ProgramPath)) {

                if ($executable.EndsWith(".ps1", [StringComparison]::OrdinalIgnoreCase)) {
                    $command = "PowerShell.exe -NoProfile -ExecutionPolicy Bypass -File `"$programPath`" -Verb RunAs"
                    if (![string]::IsNullOrEmpty($args)) {
                        $command += " $args"
                    }
                    Invoke-Expression $command
                    # Log-Message "${name} Executed." "INFO" 
                } else {
                    Log-Message "${name} is not running" $LOG_TAG_WARNING
                    Log-Message $executable $LOG_TAG_INFO 

                    if (-not [string]::IsNullOrWhiteSpace($args)) {
                        Start-Process -FilePath $programPath `
                                      -ArgumentList $args `
                                      -WorkingDirectory $workingDir `
                                      -Verb RunAs `
                                      -ErrorAction Stop
                    } else {
                        Start-Process -FilePath $programPath `
                                      -WorkingDirectory $workingDir `
                                      -Verb RunAs `
                                      -ErrorAction Stop
                    }

                    Log-Message "Attempted to start ${name} using Start-Process." $LOG_TAG_EXECUTION 

                    Start-Sleep -Seconds 1
                    $delay = $delay + 1

                    if (IsProcessByPath $ProgramPath) {
                        Log-Message "${name} started successfully." $LOG_TAG_OK
                    }
                    else {
                        Log-Message "Failed to start ${name}: $_" $LOG_TAG_ERROR
                    }
                }
                Start-Sleep -Seconds 5
                $delay = $delay + 5
            }
        }
        catch {
            Log-Message "An error occurred while monitoring the program: $_" $LOG_TAG_ERROR
        }
    }

    $timeToSleep = $CheckInterval - $delay
    if ($timeToSleep -lt 5) {
        $timeToSleep = 5
    }

    Start-Sleep -Seconds $timeToSleep
}

Log-Message "The monitoring script has exited." $LOG_TAG_INFO 
