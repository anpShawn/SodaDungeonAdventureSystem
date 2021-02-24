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

            //create two characters for our party and add them to a list
            var character1 = (PlayerCharacterData)CharacterData.Get(CharId.SODA_JUNKIE).GetCopy();
            var character2 = (PlayerCharacterData)CharacterData.Get(CharId.NURSE).GetCopy();

            var playerCharacters = new List<PlayerCharacterData>();
            playerCharacters.Add(character1);
            playerCharacters.Add(character2);

            //create a new adventure in the castle, provide characters, and set input mode to auto (otherwise the adventure will stop and wait for player input)
            var adventure = new Adventure(DungeonId.CASTLE, playerCharacters);
            adventure.SetInputMode(AdventureInputMode.AUTO);

            //set this to true if you want the adventure to log a message every time it processes a step
            adventure.showVerboseOutput = false;

            //listen for various events to receive information about the adventure progress
            adventure.EBattleStarted += OnBattleStarted;
            adventure.ESkillUsed += OnSkillUsed;
            adventure.ESkillResolved += OnSkillResolved;
            adventure.EEnemyLootEarned += OnEnemyLootEarned;
            adventure.ECharactersDied += OnCharactersDied;

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
    }
}
