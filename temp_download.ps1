[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest -Uri 'https://github.com/nesrak1/AssetsTools.NET/releases/latest/download/classdata.tpk' -OutFile 'c:\Users\Dr. Aarav\raftvr\RaftVRMod\RaftVRMod\Resources\cldb'
