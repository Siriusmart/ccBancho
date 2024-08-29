# Bancho Server

Socials plugin for ClassiCube/MCGalaxy.

## Party

Help command: `/party`

![](./img/party.png)

## Chat

Help command: `/chat`

![](./img/chat.png)

## Usage

Download a [release](https://github.com/Siriusmart/ccBancho/releases) and copy all files to your server folder. Overwrite any old files if needed.

Load the plugin once to generate the .properties file.

### MongoDB

A MongoDB instance should be running without any authentication, edit properties/bancho.properties such that mongodb-address is correct.

specs/\* contains the format of MongoDB documents.

> If you have a use case where you need to run MongoDB with authentication, open a pull request or let me know.

## Building

1. Git clone MCGalaxy such that the repo is available at ../
2. Edit `build.sh` such that the first line points to your server folder, and run it.

Note that only the generate content in out/releases are critical.

## Roadmap

- [x] Party system
- [x] Chat channels
- [x] Broadcast scope
- [ ] Friend system
- [ ] Guilds
- [ ] Games framework
