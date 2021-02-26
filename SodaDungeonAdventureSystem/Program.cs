using System;
using System.Collections.Generic;

namespace SodaDungeonAdventureSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Soda Dungeon Adventure System");

            //initialize data that the adventure requires to run
            Stats.BuildDefaultStats();
            SkillId.CacheIds();
            Skill.CreateSkills();
            DungeonData.CreateDungeons();
            ItemId.CacheIds();
            Item.CreateItems();
            SpawnTable.CreateOreTables();
            SpawnPattern.CreateSpawnPatterns();
            SodaScript.CreateScripts();
            StatusEffect.CreateAncillaryTypeData();
            CharacterData.InitCharacterCollection();
            PlayerCharacterData.CreatePlayerCharacters();
            EnemyCharacterData.CreateEnemyCharacters();
            PoolManager.CreateObjectPools();

            //create two characters for our party and add them to a list. It's important that we create them as copies of the prototypes from the master character collection
            var character1 = (PlayerCharacterData)CharacterData.GetCopy(CharId.SODA_JUNKIE);
            var character2 = (PlayerCharacterData)CharacterData.GetCopy(CharId.NURSE);
            var character3 = (PlayerCharacterData)CharacterData.GetCopy(CharId.CARPENTER);
            var character4 = (PlayerCharacterData)CharacterData.GetCopy(CharId.NURSE);

            var playerCharacters = new List<PlayerCharacterData>();
            playerCharacters.Add(character1);
            playerCharacters.Add(character2);
            playerCharacters.Add(character3);
            playerCharacters.Add(character4);

            //create a new adventure in the castle, provide characters, and set input mode to auto (otherwise the adventure will stop and wait for player input)
            var adventure = new Adventure(DungeonId.CASTLE, playerCharacters);
            adventure.SetInputMode(AdventureInputMode.AUTO);

            //set this to true if you want the adventure to log a message every time it processes a step
            adventure.showVerboseOutput = false;

            //when input mode is set to auto, combat decisions will handled by soda script. however, we can still allow the player to choose random treasure chests and paths.
            //this is off by default; if you turn it on you must listen for events such as EAltPathsEncountered and respond with OnAltPathClicked(choice)
            adventure.requireInputForRandomChoices = false;

            //listen for various events to receive information about the adventure progress
            adventure.EBattleStarted += OnBattleStarted;
            adventure.ESkillUsed += OnSkillUsed;
            adventure.ESkillResolved += OnSkillResolved;
            adventure.EEnemyLootEarned += OnEnemyLootEarned;
            adventure.ECharactersDied += OnCharactersDied;
            adventure.EAltPathsEncountered += OnAltPathsEncountered;
            adventure.EAltPathConfirmed += OnAltPathConfirmed;
            adventure.ETreasureRoomConfirmed += OnTreasureRoomConfirmed;

            //start the adventure
            adventure.Start();

            //read key so that the output window won't close immediately after the adventure finishes
            Console.ReadKey();
        }

        private static void OnBattleStarted(Adventure inAdventure)
        {
            var numEnemies = inAdventure.enemyTeam.members.Count;
            Console.WriteLine($"\n\nStarting Battle #{inAdventure.battleNum}. Enemies present: {numEnemies}");
        }

        private static void OnSkillUsed(Adventure inAdventure, SkillCommand inCommand)
        {
            var targetId = inCommand.targets[0].id;
            var targetNumber = inCommand.targets[0].orderInTeam;
            Console.WriteLine($"{inCommand.source.id} used skill {inCommand.skill.id} on {targetId} [position {targetNumber}]");
        }

        private static void OnSkillResolved(Adventure inAdventure, List<SkillResult> inResults)
        {
            for(int i=0; i<inResults.Count; i++)
            {
                var result = inResults[i];
                var target = result.target.id;
                var curHp = result.target.stats.Get(Stat.hp);
                var maxHp = result.target.stats.Get(Stat.max_hp);
                Console.WriteLine($"{target} hp : {curHp}/{maxHp}");
            }
        }

        private static void OnEnemyLootEarned(Adventure inAdventure, CharacterData inEnemy, LootBundle inLoot, int inNumKeys)
        {
            Console.WriteLine("Earned " + inLoot.ToString());
            if(inNumKeys != 0)
                Console.WriteLine($"Earned {inNumKeys} keys");
        }

        private static void OnCharactersDied(Adventure inAdventure, List<CharacterData> inDeadCharacters)
        {
            for(int i=0; i<inDeadCharacters.Count; i++)
                Console.WriteLine($"{inDeadCharacters[i].id} died");
        }

        private static void OnAltPathsEncountered(Adventure inAdventure, AltPathPortal[] inChoices)
        {
            var choices = new List<string>();
            for (int i = 0; i < inChoices.Length; i++)
                choices.Add(inChoices[i].destination.ToString());

            Console.Write("\nAlternate paths encountered:");
            Utils.PrintList(choices);
        }

        private static void OnAltPathConfirmed(Adventure inAdventure, AltPathPortal inChoice)
        {
            if (inChoice == null)
                Console.WriteLine("No alternate path chosen");
            else
                Console.WriteLine("Alternate path chosen: " + inChoice.destination);
        }

        private static void OnTreasureRoomConfirmed(Adventure inAdventure, LootBundle inLoot)
        {
            Console.WriteLine("Treasure Room entered, found: " + inLoot.ToString());
        }
    }
}
