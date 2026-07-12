[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$release = Invoke-RestMethod -Uri 'https://api.github.com/repos/AssetRipper/Tpk/releases/tags/1.2.1'
foreach ($a in $release.assets) {
    Write-Host $a.name
}
