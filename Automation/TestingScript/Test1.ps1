# Define the log file path
$logFile = "C:\Temp\OperationTest.log"
$scriptName = Split-Path -Leaf $MyInvocation.MyCommand.Path

# Write initial log entry
"$scriptName started at $(Get-Date)" | Out-File -FilePath $logFile -Append
