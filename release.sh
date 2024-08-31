GREY='\e[37m'
YELLOW='\e[33m'
WHITE='\e[37m'
GREEN='\e[32m'
RED='\e[31m'
CYAN='\e[36m'

if ! command -v dotnet &> /dev/null
then
    echo -e "${RED}Command 'dotnet' could not be found, build aborted."
    exit 1
fi

if ! command -v date &> /dev/null
then
    echo -e "${RED}Command 'date' could not be found, build aborted."
    exit 1
fi

if ! command -v zip &> /dev/null
then
    echo -e "${RED}Command 'date' could not be found, build aborted."
    exit 1
fi

if ! command -v tar &> /dev/null
then
    echo -e "${RED}Command 'date' could not be found, build aborted."
    exit 1
fi

if ! command -v printf &> /dev/null
then
    echo -e "${RED}Command 'printf' could not be found, build aborted."
    exit 1
fi

if [ ! -d "../MCGalaxy" ]; then
  echo -e "${RED}../MCGalaxy does not exist, build aborted."
  exit 1
fi

begin=$(date +%s%3N)

rm -rf release/*

echo -en "\033[0K\r${GREY}[${YELLOW}1${WHITE}/${YELLOW}3${GREY}] ${GREEN}Building for linux_x64"
dotnet publish -c Release -o out/linux_x64 --runtime linux-x64 > /dev/null
echo -en "\033[0K\r${GREY}[${YELLOW}2${WHITE}/${YELLOW}3${GREY}] ${GREEN}Building for osx_x64  "
dotnet publish -c Release -o out/osx_x64 --runtime osx-x64 > /dev/null
echo -en "\033[0K\r${GREY}[${YELLOW}3${WHITE}/${YELLOW}3${GREY}] ${GREEN}Building for win_x64  "
dotnet publish -c Release -o out/win_x64 --runtime win-x64 > /dev/null

mkdir -p release/linux_x64/plugins
mkdir -p release/osx_x64/plugins
mkdir -p release/win_x64/plugins


echo -en "\033[0K\r${GREY}[${YELLOW}1${WHITE}/${YELLOW}6${GREY}] ${GREEN}Compressing releases ${CYAN}linux_x64.zip"
cp out/linux_x64/Bancho.dll release/linux_x64/plugins
cp out/linux_x64/MongoDB.*.dll release/linux_x64
cp out/linux_x64/DnsClient.dll release/linux_x64
cp out/linux_x64/Microsoft.Extensions.Logging.Abstractions.dll release/linux_x64
cd release/linux_x64 && zip -r ../linux_x64.zip . -q && cd ../..

echo -en "\033[0K\r${GREY}[${YELLOW}2${WHITE}/${YELLOW}6${GREY}] ${GREEN}Compressing releases ${CYAN}osx_x64.zip  "
cp out/osx_x64/Bancho.dll release/osx_x64/plugins
cp out/osx_x64/MongoDB.*.dll release/osx_x64
cp out/osx_x64/DnsClient.dll release/osx_x64
cp out/osx_x64/Microsoft.Extensions.Logging.Abstractions.dll release/osx_x64
cd release/osx_x64 && zip -r ../osx_x64.zip . -q && cd ../..

echo -en "\033[0K\r${GREY}[${YELLOW}3${WHITE}/${YELLOW}6${GREY}] ${GREEN}Compressing releases ${CYAN}win_x64.zip  "
cp out/win_x64/Bancho.dll release/win_x64/plugins
cp out/win_x64/MongoDB.*.dll release/win_x64
cp out/win_x64/DnsClient.dll release/win_x64
cp out/win_x64/Microsoft.Extensions.Logging.Abstractions.dll release/win_x64
cd release/win_x64 && zip -r ../win_x64.zip . -q && cd ../..

echo -en "\033[0K\r${GREY}[${YELLOW}4${WHITE}/${YELLOW}6${GREY}] ${GREEN}Compressing releases ${CYAN}linux_x64.tar.gz  "
cd release/linux_x64 && tar -czf ../linux_x64.tar.gz . && cd ../..
echo -en "\033[0K\r${GREY}[${YELLOW}5${WHITE}/${YELLOW}6${GREY}] ${GREEN}Compressing releases ${CYAN}win_x64.tar.gz  "
cd release/osx_x64 && tar -czf ../osx_x64.tar.gz . && cd ../..
echo -en "\033[0K\r${GREY}[${YELLOW}6${WHITE}/${YELLOW}6${GREY}] ${GREEN}Compressing releases ${CYAN}win_x64.tar.gz  "
cd release/win_x64 && tar -czf ../win_x64.tar.gz . && cd ../..

echo -en "\033[0K\r${GREEN}Cleaning up...                           "

rm -rf release/linux_x64
rm -rf release/osx_x64
rm -rf release/win_x64

end=$(date +%s%3N)
elapsed=$((end - begin))
formatted=$(printf "%.3f" "$(echo "scale=3; $elapsed / 1000" | bc)")

echo -en "\033[0K\rBuild completed in ${formatted}s\n"
