if (-not $script:azdCmd) {
    $script:azdCmd = Get-Command azd -ErrorAction SilentlyContinue
}

# Check if 'azd' command is available
if (-not $script:azdCmd) {
    throw "Error: 'azd' command is not found. Please ensure you have 'azd' installed. For installation instructions, visit: https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd"
}

if (-not $script:dotnetCmd) {
    $script:dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
}

# Check if 'dotnet' command is available
if (-not $script:dotnetCmd) {
    throw "Error: 'dotnet' command is not found. Please ensure you have 'dotnet' installed. For installation instructions, visit: https://learn.microsoft.com/en-us/dotnet/core/tools/"
}

function Invoke-ExternalCommand {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Command,
        
        [Parameter(Mandatory = $false)]
        [string]$Arguments
    )

    Start-Process -FilePath $Command -ArgumentList $Arguments -Wait -NoNewWindow
}

if ([string]::IsNullOrEmpty($env:AZD_PREPENV_RAN) -or $env:AZD_PREPENV_RAN -eq "false") {
    Write-Host 'Running "PrepareEnvironment.dll"'

    Get-Location | Select-Object -ExpandProperty Path

    $dotnetArguments = @"
    run --project "src/PrepareEnvironment/PrepareEnvironment.csproj" "./data/hotels.json" --openaihost "$env:OPENAI_HOST" --openaikey "$env:OPENAI_API_KEY" --openaiservice "$env:AZURE_OPENAI_SERVICE" --openaimodelname "$env:AZURE_OPENAI_EMB_DEPLOYMENT" --searchservice "$env:AZURE_SEARCH_SERVICE" --index "$env:AZURE_SEARCH_INDEX"
"@
    
    Invoke-ExternalCommand -Command "dotnet" -Arguments $dotnetArguments

    Invoke-ExternalCommand -Command ($azdCmd).Source -Arguments @"
    env set AZD_PREPENV_RAN "true"
"@

}
else {
    Write-Host "AZD_PREPENV_RAN is set to true. Skipping the run."
}