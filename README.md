# HintMachine

Archipelago is a program allowing different videogames randomizers to shuffle items between them.

HintMachine is a program giving hints for your Archipelago games.

When you can't play your game anymore because you're waiting for your friend to send you the item to unblock you, launch one of the games supported by HintMachine, complete quests and be rewarded with hints for items locations in your game !

## Supported games

- Bust a Move 4 (PS1)
- Geometry Wars Galaxies (Wii)
- Geometry Wars : Retro Evolved
- ISLANDERS
- One Finger Death Punch
- Puyo Puyo Tetris
- Rollcage Stage 2 (PS1)
- Sonic 3 Blue Spheres
- Stargunner (GOG)
- Tetris Effect Connected
- Xenotilt
- Zachtronics Solitaire Collection

(More are coming)

## Usage

### Release versions 

Download the latest version's zip and extract it. Launch HintMachine.exe, 
connect to your Archipelago server with your username (and password), launch the game you want to play to get hints, select it in the HintMachine dropdown and click connect.
All you have to do now is play the game and complete the quests to get hints.

If you want to play another game, click on "Pick another game".

If you want to change the Archipelago game which will recieve hints, go to "File" ► "Reconnect as ..." and select the corresponding player in your Archipelago session.

### Running from source 

You can also get the source code and open it with Visual Studio 2017+ and .NET Framework 4.8.

It has to be built in x64 configuration.

### How to add a game

The process to add a game is:
- find RAM addresses that contain what you're looking for (using a program like CheatEngine)
- add a new connector class
- add quests that track those RAM addresses
- profit
