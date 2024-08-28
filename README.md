# Bancho Server

Socials plugin for ClassiCube/MCGalaxy.

## Party

Help command: `/party`

![](./img/party.png)

## Chat

Help command: `/chat`

![](./img/chat.png)

## Building

```sh
dotnet publish -c Release -o out
```

For easy of development, do

```sh
dotnet publish -c Release -o out --runtime linux-x64 && cp out/Bancho.dll path/to/plugins
```

## Roadmap

- [x] Party system
- [x] Chat channels
- [ ] Broadcast scope
- [ ] Friend system
- [ ] Game and player stats
