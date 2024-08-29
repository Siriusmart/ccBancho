TARGET=../../ClassiCube/Server # change me

dotnet publish -c Release -o out/osx_x64 --runtime osx-x64
cp out/osx_x64/Bancho.dll $TARGET/plugins
cp out/osx_x64/MongoDB.*.dll $TARGET
cp out/osx_x64/DnsClient.dll $TARGET
cp out/osx_x64/Microsoft.Extensions.Logging.Abstractions.dll $TARGET
