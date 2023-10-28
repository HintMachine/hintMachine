![version](https://img.shields.io/badge/Version-1.1.0-blue)

<p align="center">
  <img src="https://github.com/CalDrac/hintMachine/blob/master/HintMachine/Assets/logo_small.png?raw=true" alt="HintMachine logo"/>
</p>

# HintMachine

HintMachine is a "BK Game" client designed to work for the well-known multiworld ecosystem [Archipelago](https://github.com/ArchipelagoMW/Archipelago).

It connects to a wide variety of games (through RAM peeking, save file watching, etc...) to track progress on specific "quests" which, when completed, award random location hints inside the Archipelago world you are connected to.

Given its high dependency to system calls to do RAM peeking and stuff, only Windows is supported as of now.

## Usage

### Release versions 

- Download a release version's .zip file
- Extract it, and launch HintMachine.exe
- Connect to your Archipelago room by providing the hostname, the slot name and a password if there is one
- You can now connect to any game from HintMachine's library and profit

### Running from source 

- Download the source 
- Open the solution with Visual Studio 2017+
- It should generate & run right away


## Contributing

You are free to contribute to HintMachine's development.

You can check [this document](https://github.com/CalDrac/hintMachine/blob/dev/adding_games.md) for more details on how to add new games.


## Currently supported games

| Game name                        | Platform  |
|----------------------------------|-----------|
| 911 Operator                     | PC        |
| BPM: Bullets Per Minute          | PC        |
| Bust-a-Move 4                    | PS1       |
| Columns                          | Megadrive |
| Dorfromantik                     | PC        |
| Dragon's Crown                   | PS Vita   |
| F-Zero GX                        | GameCube  |
| Geometry Wars Galaxies           | Wii       |
| Geometry Wars : Retro Evolved    | PC        |
| ISLANDERS                        | PC        |
| Katamari Damacy REROLL           | PC        |
| Luck be a Landlord               | PC        |
| Meteos                           | DS        |
| Metroid Prime Pinball            | DS        |
| Minesweeper Classy               | PC        |
| Mini Metro                       | PC        |
| One Finger Death Punch           | PC        |
| PAC-MAN Championship Edition DX+ | PC        |
| Papers, Please                   | PC        |
| Pokémon Pinball: Ruby & Sapphire | GBA       |
| Pokémon Puzzle Challenge         | GBC       |
| Puyo Puyo Tetris                 | PC        |
| Rollcage Stage 2                 | PS1       |
| Sanrio World Smash Ball!         | SNES      |
| Sonic 3 Blue Spheres             | Megadrive |
| Stargunner                       | PC        |
| Super Hexagon                    | PC        |
| Super Mega Baseball 2            | PC        |
| Super Monkey Ball 2              | GameCube  |
| TMNT: Shredder's Revenge         | PC        |
| Tetris                           | NES       |
| Tetris Effect Connected          | PC        |
| Tony Hawk's Pro Skater 1 + 2     | PC        |
| Xenotilt                         | PC        |
| Zachtronics Solitaire Collection | PC        |
