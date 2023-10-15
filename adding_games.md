# Adding a new game to HintMachine

If you read this document, you're probably considering adding a new game to the HintMachine project.
First, thank you for your interest! We'll try to take a step-by-step approach to give you everything you need to know.

---

## Picking a game to add

This chapter title might look a bit odd, but you read it right: picking the right game is extremely important.
There are many factors that make a game good or bad in this context, and you need to think carefully about all those before rushing into development phase.

1) **Frequently updated games are no good**

HintMachine is a client that connects to external games most of the time against their will, or at least without them knowing.
This means the game you are connecting to doesn't care about you, and if it ever gets updated, things might break.

**Adding a game** to HintMachine also means you will have to **update your connector** whenever the game gets updated.
Depending on the method you will use to track quests, things might completely break on every game update.
This means that a recent game that gets updated often might be very tiring to keep afloat, and it is very frustrating from a user's
perspective to have the game connectors breaking every now and then because things get updated.

This is why, picking games that are rarely updated or never updated anymore is usually the best choice.
Think about it when picking your next game.

2) **Some games are easier to implement than others**

This might sound obvious, but it is something worth considering: depending on your experience in reverse engineering games,
you might want to avoid some complex recent games which can be discouraging.
We advise you to first try implementing simple retro games first to start smoothly, before moving onto bigger targets.

We'll get more into the technical details later in this document, but also know that emulated games tend to be way easier
to implement if a generic connector for this emulator / platform has already been developed.

3) **Not all games are good BK games**

When you're blocked in your multiworld, you want to play a game that's fast, fun, and replayable.
This is why arcade games typically make very good HintMachine material since they are often developed with this in mind in the first place.
On the other hand, linear adventures like RPGs almost always aren't good picks because these are not games anyone can launch to have some quick fun.

Also think about the **quests** that you will track for your game, since it will define the whole experience of playing it.
Some games might sound like good ideas to implement, but don't have any objective that is interesting and easy to track.

4) **Have fun!**

Picking a game that you'd want to play yourself while BK usually is a good starting point. If *you* want it, other people might as well.
Please don't fall into the trap of adding a game because "some people might like it", whereas **you** would not play it.

---

## What you need to know

Now that you have picked your game (or have a few ideas in mind), let's get into the more technical stuff.

### Connectors

A supported game inside HintMachine is actually what we call a "connector", which is a single class inheriting `IGameConnector`.

This `IGameConnector` abstract class defines the interface to provide for everything to go well, including:
- Some **metadata** about the game (game name, description, supported versions, etc...)
- The list of **quests** for that game
- Some methods to implement to define behavior (`Connect` to connect, `Disconnect` to disconnect, `Poll` to define what's being tracked continuously...)

Adding a game mostly consists in adding one file containing your very own game connector which will have the name of your game.
For instance, if I want to add Elden Ring, I'll call it `EldenRingConnector` (but don't get any ideas about adding that specific game, right?)

### How to connect to your game

There are several ways to connect to a game, which we are going to see in details.

-----

**RAM peeking**

This is the most common one, but can be somewhat finnicky to work on all systems. Your connector class will basically "open a backdoor" on the game's process and look into its RAM in real-time to see what happens. 

This requires analyzing the game beforehand using an external tool (such as the excellent [CheatEngine](https://www.cheatengine.org/downloads.php)) to know where to look. The challenge usually is to find a robust **pointer path** because dynamically allocated memory never ends up at the same place depending on where the system tells the program to put its stuff, so we cannot use a fixed address. There are lots of tutorials that cover that subject on the net so we won't cover it here.

The `ProcessRamWatcher` helper class provides you all the tools you need to perform RAM peeking painlessly.

Please note that RAM peeking is extremely sensible to game updates and will most likely break if your game `exe` gets updated. 

Good example to start from: `GeometryWarsConnector`

-----

**Savefile watching**

This one is also not always possible, but can be a very interesting option when it is. Your connector class puts a filesystem watcher to look for changes on the game's savefile, and when it changes, analyzes what changed. 

From there, it reads the interesting values and updates quests accordingly. 

This is extremely resilient to game updates, since savefiles are usually untouched to keep compatibility between game versions.

Good example to start from: `ZachtronicsSolitaireConnector`

-----

**Using a generic emulator connector**

This is not really a "way" of connecting, but if you are trying to add an emulated game you most likely won't have to care about handling the connection stuff. Indeed, there are chances that a generic connector already exists and does that for you.

In that case, all you need to find are offsets in the console's RAM (using an emulator like [Bizhawk](https://tasvideos.org/BizHawk/ReleaseHistory) is very efficient for that kind of work) and add them to the base address the connector found for you.

Good example to start from: `ColumnsConnector`

-----

**Network communication**

This one is rarely possible, but when it is, it can be a very good solution. Your connector class opens a socket to communicate via network with the game process, which needs to have some networking API available (which is why it's rarely possible). Please note we dislike requiring the user to mod their games to make them work on HintMachine: it must work right away.

No example so far in the codebase.

---

### Quests

Quests are the way to track objectives and reward hint points in HintMachine.
Currently, there are two kind of quests:
- `HintQuestCounter`: a basic counter which is incremented manually when a certain action is detected
- `HintQuestCumulative`: a cumulative counter to which you provide values update every tick, and which detects **movement** for that value. It means its value will increase by 5 when the tracked value increases by 5, but will not decrease when it decreases.

These two types of quests are pretty simple to manipulate, and the examples given in the previous chapter should feature quests usage.

---

There is a **global guideline** for quests balance, to ensure all games provide hints roughly at the same rate. You should aim for your game (with all of its quests) to provide a hint every ~2-3 minutes.

In games featuring a single quest, it's pretty easy to balance since you just have to ensure that specific quest roughly goes at that rate.
On the other end, for multi-quest games, you need to ensure that gameplay in pretty much every gamemode provides that rough hint output.

---

There is also good practice regarding **quest security**, because a connector can fail and will fail (reads at the wrong address on very specific systems, etc...).

The quest classes provide safety measures to ensure not everything breaks when that happens:
    
- `MaxIncrease`: This attribute defines a threshold and if the value attempts to increase by more than this threshold, the change is rejected. This is typically useful when reading abnormally high values at a wrong spot in memory to prevent the game from awarding millions of hint tokens at once.
By default, `MaxIncrease = GoalValue x 2`, but specifying at a value closer to what's possible in your game might reinforce that security. Don't put it too low though, because it could be irritating for players who are good at the game you're trying to implement

- `CooldownBetweenIncrements`: This attributes defines a cooldown between two increases in value for that quest. This not always relevant, but let's take the simple example of a racing game where you have a "races completed" quest. You know for a fact that a race is never going to be faster than 1m30s, so putting a cooldown of 1m30s on increases ensures nothing bad can happen in the meantime. On the other hand, quests that are updated often usually are not adapted at all to use this safety mechanism.

Testing is very important (both for technical reasons and balance reasons), so don't neglect it.

---

## Getting your game ready for submission

So, you developed your game and everything looks like it's working. Nice, good job!

Now, we need to get your game ready for submission.

First, let's check if all of this work as it should:

- Quitting the game while being connected to it should automatically disconnect HintMachine. If that's not the case, add the following code snippet at the beginning of you `Poll` method:

```csharp
    if(!_ram.TestProcess())
        return false;
```

- Attempting to connect to the game while it's not launched should fail. This is especially true on emulators, where you must not be able to connect if the emulator is on but the ROM is not loaded, or if another ROM is put.

- Have you tried saving & loading games, does it make your values go bonkers? In that case, you might want to try to detect breaks in the "game flow" to spot when the user comes back to main menu, etc... This can be accomplished by finding values such as current turn count, maybe a in-game timer of sorts which would
change in a very unusual way when such an action is performed by the user.

- Add a description for your game, the list of supported versions, a game cover in the same format as the ones already present (which is the same format as [IGDB](https://www.igdb.com/games/toy-commander))

- Add the game to the table inside the README.md file at the project's root

- Create a pull request on GitHub to ask for your game to be merged
