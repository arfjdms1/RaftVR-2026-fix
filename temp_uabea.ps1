[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$release = Invoke-RestMethod -Uri 'https://api.github.com/repos/nesrak1/UABEA/releases/latest'
foreach ($a in $release.assets) {
    if ($a.name -like '*tpk*' -or $a.name -like '*dat*') {
        Write-Host $a.name
        Write-Host $a.browser_download_url
    }
}
