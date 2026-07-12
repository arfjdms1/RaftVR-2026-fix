[Reflection.Assembly]::LoadFrom('C:\Users\Dr. Aarav\raftvr\RaftVRLoader\RaftVRLoader\Assemblies\AssetsTools.NET.dll')
$am = New-Object AssetsTools.NET.Extra.AssetsManager
$afi = $am.LoadAssetsFile('C:\Program Files (x86)\Steam\steamapps\common\Raft\Raft_Data\globalgamemanagers', $true)
if ($afi.file.typeTree.hasTypeTree) { Write-Host 'HAS TYPE TREE' } else { Write-Host 'NO TYPE TREE' }
