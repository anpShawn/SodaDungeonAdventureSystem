# SodaDungeonAdventureSystem



SodaDungeonAdventureSystem is the core code responsible for managing a player’s progress through a turn-based dungeon in Soda Dungeon 2. This version can be run as a standalone console program in C#.

You can play Soda Dungeon 2 for free on the following platforms:
* Steam https://store.steampowered.com/app/946050/Soda_Dungeon_2/
* iOS https://apps.apple.com/us/app/soda-dungeon-2/id1454882086
* Android https://play.google.com/store/apps/details?id=com.armorgames.sodadungeon2
* Web https://armorgames.com/soda-dungeon-2-game/19058?fp=ng

Follow on Twitter for Updates:
* https://twitter.com/SodaDungeon
* https://twitter.com/anpShawn


## A QUICK NOTE
This code has not been peer-reviewed, and I’ve never worked on a software development team. I try to adhere to general principles of readability and modular design, but this degrades over time as requirements change and deadlines approach. Take everything with a grain of salt, your mileage may vary, etc.

This is not meant to be an empirical example of how to write a turn-based combat system. This is just my version of it which can hopefully serve as a reference point if you wish to write your own. This code could function as a starting point but I wouldn’t recommend copying it verbatim, as it carries a lot of baggage (discussed in “removed”)

## FEATURES
* Turn-based combat dictated by speed units
* 1-6 member party, customizable with Equipment, Active Skills, and Passive Skills
* Endless progression with linear enemy scaling
* Loot Drops and Treasure Rooms
* Optional “Alternate” paths (healing fairy, traps, etc) 
* Status Effect management
* Skill Cooldowns
* Scriptable character behavior (see “Soda Scripts”)
* Object pooling of key data types to reduce memory churn

## GETTING STARTED
Program.cs is the entry point for setting up and executing an “Adventure”
The first step is to initialize a chunk of data relating to characters, items, skills and more.
Afterward we subscribe to a number of listeners that the Adventure class offers.
Lastly we start the adventure and watch its output in the console.

The core of this system resides in Adventure.cs and BattleManager.cs. They are two tightly-coupled classes that communicate back and forth to produce the results seen on screen. In the real game I use what I call a “Visual Adventure Observer” to listen to Adventure’s events, pause it, play animations, then resume it.

The CharacterData class (and child classes PlayerCharacterData and EnemyCharacterData) also do some heavy lifting. Most of the other classes serve as ways to model the data that the adventure system needs to work properly.

I tried to find a balance between streamlining the core classes for understandability but also leaving in code that demonstrates extended functionality. I also wanted to show what my version of “shipped” code looks like without hiding poor choices.

## WHAT HAS BEEN REMOVED
Whenever I removed or altered original code I tried to tag it with one of the following:
* REMOVED: code outside the scope of this project, removed entirely.
* HIDDEN: code outside of the scope but may still be of interest, such as how to pass player input to the Adventure class.
* SIMPLIFIED: code within scope but simplified to match the needs of the project.

More specifically, the removed portions generally fall into one of these categories:
* Backend services, including ads and analytics
* Relics, Pets, and other upgrades present in the full game
* Arena Challenges, Blacksmith Inventory, Mastery progression, Achievements, and other features also present in the full game
* The “Chef”, a highly specialized character class that required a lot of extra code to function correctly
* Miscellaneous debug code

That being said, there is still a lot of extraneous code present. Much of it deals with logic for specific skills, classes, and items that have been removed from this version. 


## EXTENDING FUNCTIONALITY
In order to keep things simple I have recreated only a small portion of the characters, items, skills, and enemies found in the real game. If you wish to add more of any of these to the game it should be pretty straightforward to do so if you use the existing code as a template.

## SODA SCRIPTS
“Soda Scripts” are an optional portion of the codebase that allow players to construct a basic list of priorities (triggers) for each character to follow when it is their turn. It can be used to attack certain types of enemies with certain skills, heal the team at certain thresholds, and more. Right now each character is assigned the default script which prioritizes healing teammates (if possible), then attacking the weakest enemy. 

You can also examine the AutoKeyAndPathSettings.cs file to see this same type of logic applied to the team as a whole when it approaches a set of optional side paths. Again, default settings are provided but feel free to experiment with your own.

## SHORTCOMINGS
I think the biggest issue by far with this codebase is dependency management. In the production version of the game I use a very crude version of the Service Locator pattern to mitigate this. In this version I swap some of this out with static calls to classes such as SaveFile, ObjectPool, Locale, and more. Regardless, a lot of tight coupling between the other classes exists.

There is some ambiguity in certain places when determining whose “responsibility” it is to cleanup and recycle/de-pool objects. Several classes will grab the next pooled object available, return it as part of a function, and assume that the caller will take proper care of it.

Adventure.cs and BattleManager.cs are also quite big, coming in at about 2k and 1k lines of code, respectively. They aren’t quite “god” classes, but could probably stand to be broken up more.

## COLLABORATION
I welcome any feedback on this repository but please understand that unless a critical error is found I don’t intend to merge any part of this codebase back into the production version of Soda Dungeon 2.
