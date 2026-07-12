[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$tree = Invoke-RestMethod -Uri 'https://api.github.com/repos/nesrak1/AssetsTools.NET/git/trees/master?recursive=1'
foreach ($f in $tree.tree) {
    if ($f.path -like '*.tpk' -or $f.path -like '*.dat') {
        Write-Host $f.path
    }
}
