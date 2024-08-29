TARGET=../../ClassiCube/Server # change me

dotnet publish -c Release -o out/win_x64 --runtime win-x64
cp out/win_x64/Bancho.dll $TARGET/plugins
cp out/win_x64/MongoDB.*.dll $TARGET
cp out/win_x64/DnsClient.dll $TARGET
cp out/win_x64/Microsoft.Extensions.Logging.Abstractions.dll $TARGET
