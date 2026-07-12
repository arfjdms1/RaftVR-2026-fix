[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$releases = Invoke-RestMethod -Uri 'https://api.github.com/repos/nesrak1/AssetsTools.NET/releases'
foreach ($r in $releases) {
    Write-Host "Release: $($r.tag_name)"
    foreach ($a in $r.assets) {
        if ($a.name -like '*.tpk' -or $a.name -like '*.dat') {
            Write-Host "  Asset: $($a.name)"
            Write-Host "  URL: $($a.browser_download_url)"
        }
    }
}
