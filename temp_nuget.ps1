[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest -Uri 'https://www.nuget.org/api/v2/package/AssetsTools.NET/3.0.0' -OutFile 'c:\Users\Dr. Aarav\raftvr\AssetsTools.NET.3.0.0.nupkg'
Expand-Archive -Path 'c:\Users\Dr. Aarav\raftvr\AssetsTools.NET.3.0.0.nupkg' -DestinationPath 'c:\Users\Dr. Aarav\raftvr\AssetsTools.NET.3.0.0' -Force
