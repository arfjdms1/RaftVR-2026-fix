$asm = [Reflection.Assembly]::LoadFrom('C:\Users\Dr. Aarav\raftvr\RaftVRLoader\RaftVRLoader\Assemblies\AssetsTools.NET.dll')
$type = $asm.GetType('AssetsTools.NET.Extra.AssetsManager')
if ($type -ne $null) {
    $methods = $type.GetMethods()
    foreach ($m in $methods) {
        if ($m.Name -like 'LoadClass*') {
            Write-Host $m.Name
        }
    }
} else {
    Write-Host "Type not found"
}
