TARGET=../../ClassiCube/Server # change me

dotnet publish -c Release -o out --runtime linux-x64
cp out/Bancho.dll $TARGET/plugins
cp out/MongoDB.*.dll $TARGET
cp out/DnsClient.dll $TARGET
cp out/Microsoft.Extensions.Logging.Abstractions.dll $TARGET

RELEASE=out/release

mkdir -p $RELEASE/plugins
cp out/Bancho.dll $RELEASE/plugins
cp out/MongoDB.*.dll $RELEASE
cp out/DnsClient.dll $RELEASE
cp out/Microsoft.Extensions.Logging.Abstractions.dll $RELEASE
