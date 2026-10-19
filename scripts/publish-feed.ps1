<#
Uploads a BobsComponents .nupkg to the private feed: Azure Blob Storage account bobswebworksnugetsa,
container "feed" (RG bobsbillets-rg, Central US, no public access). Needs `az login`.
Consumers (KFit CI) download the nupkg with a read-only SAS into a local folder feed; see KFit scripts/fetch-bobscomponents.ps1.
Prints the read-only base URL (contains a SAS secret) to the console only. Never commit it.

  .\scripts\publish-feed.ps1 -Package ..\pack\BobsComponents.0.1.0.nupkg
  .\scripts\publish-feed.ps1 -Rotate          # only issue a fresh read SAS
#>
param([string]$Package, [switch]$Rotate, [string]$Account = 'bobswebworksnugetsa', [string]$Container = 'feed', [int]$SasDays = 365)
$ErrorActionPreference = 'Stop'
$key = az storage account keys list -g bobsbillets-rg -n $Account --query '[0].value' -o tsv
$auth = @('--account-name', $Account, '--account-key', $key)
if ($Package) {
    $p = Resolve-Path $Package
    az storage blob upload @auth -c $Container -f $p -n (Split-Path $p -Leaf) --overwrite -o none
}
$expiry = (Get-Date).ToUniversalTime().AddDays($SasDays).ToString('yyyy-MM-ddTHH:mmZ')
$sas = az storage container generate-sas @auth -n $Container --permissions rl --expiry $expiry -o tsv
Write-Host "Read-only feed URL (secret, expires $expiry). Store it as the GitHub secret BOBS_FEED_URL:"
Write-Host "https://$Account.blob.core.windows.net/$Container`?$sas"
