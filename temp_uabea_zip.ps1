[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$release = Invoke-RestMethod -Uri 'https://api.github.com/repos/nesrak1/UABEA/releases/latest'
foreach ($a in $release.assets) {
    if ($a.name -like '*.zip') {
        Invoke-WebRequest -Uri $a.browser_download_url -OutFile 'c:\Users\Dr. Aarav\raftvr\uabea.zip'
        break
    }
}
Expand-Archive -Path 'c:\Users\Dr. Aarav\raftvr\uabea.zip' -DestinationPath 'c:\Users\Dr. Aarav\raftvr\uabea' -Force
Get-ChildItem -Path 'c:\Users\Dr. Aarav\raftvr\uabea' -Recurse -Filter "*.tpk" | Select-Object FullName
Get-ChildItem -Path 'c:\Users\Dr. Aarav\raftvr\uabea' -Recurse -Filter "*.dat" | Select-Object FullName
