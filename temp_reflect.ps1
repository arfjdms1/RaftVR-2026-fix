$asm = [Reflection.Assembly]::LoadFrom('C:\Program Files (x86)\Steam\steamapps\common\Raft\Raft_Data\Managed\Assembly-CSharp.dll')
$type = $asm.GetType('SweepNet')
if ($type -ne $null) {
    $fields = $type.GetFields([System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static)
    foreach ($f in $fields) {
        Write-Host ($f.Name + ' : ' + $f.FieldType.Name)
    }
} else {
    Write-Host "Type not found"
}
