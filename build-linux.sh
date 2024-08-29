TARGET=../../ClassiCube/Server # change me

dotnet publish -c Release -o out/linux_x64 --runtime linux-x64
cp out/linux_x64/Bancho.dll $TARGET/plugins
cp out/linux_x64/MongoDB.*.dll $TARGET
cp out/linux_x64/DnsClient.dll $TARGET
cp out/linux_x64/Microsoft.Extensions.Logging.Abstractions.dll $TARGET
