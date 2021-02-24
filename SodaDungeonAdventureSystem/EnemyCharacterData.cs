using System;
using System.Collections.Generic;
using System.Linq;

public class EnemyCharacterData : CharacterData
{
    protected static Dictionary<string, EnemyCharacterData> enemyCollection;
    private static List<EnemyCharacterData> enemyCollectionList;
    private static IEnumerable<EnemyCharacterData> validSpawns;
    private static List<EnemyCharacterData> validSpawnSubset;

    //enemy stuff
    [NonSerialized]public int storageId; //this id is a compressed way to store the enemy's id in the bestiary
    [NonSerialized]public AdventureArea spawnAreas;
    [NonSerialized]public EnemyRank rank;
    [NonSerialized]public EnemySize size;
    [NonSerialized]public int spawnRate;
    [NonSerialized]public Range areaSpawnRange;
    [NonSerialized]public int dropRate;
    [NonSerialized]public string[] itemDrops;
    [NonSerialized]public int fps;
    [NonSerialized]public int frames;
    [NonSerialized]public int shadowSize;
    [NonSerialized]public int shadowOffset;
    [NonSerialized]public int flyHeight;
    [NonSerialized]public int flySpeed;
    [NonSerialized]public Vector2 projectileSpawnOffset;
    [NonSerialized]public int fadeAmt;

    public static void CreateEnemyCharacters()
    {
        enemyCollection = new Dictionary<string, EnemyCharacterData>();

        //use this line to load from a json file
        //EnemyCharacterMap map = Utils.LoadJsonFromPath<EnemyCharacterMap>(inPath);

        /*
         *     {"id":"1:rat",                   				               "area": "CAVERNS|JAIL",     "rank":"NORM",        "size": "MD",               "base_stats" : ["hp=2",     "atk=1",      "spd=default"],                "spawn_rate": 1,        "area_spawn_range": "0-50",       "drop_rate":"3",                "drops": "6",                                                                   "fps":4,        "frames":3,           "shadow_size": 7,      "shadow_offset":0,         "fly_height": 0,        "fly_speed": 0,          "projectile_spawn_offset":"0,0",          "fade_amt":0,              "flags": "CANT_STRIKE",                                                      "variable_target_preference":1,     "skills":"CLAW"},
               {"id":"11:tunneler",           				                 "area": "CAVERNS",          "rank":"MBOS",        "size": "LG",               "base_stats" : ["hp=8",     "atk=2",      "spd=default"],                "spawn_rate": 1,        "area_spawn_range": "0-50",       "drop_rate":"1",                "drops": "3",                                                                   "fps":4,        "frames":3,           "shadow_size": 0,      "shadow_offset":0,         "fly_height": 0,        "fly_speed": 0,          "projectile_spawn_offset":"0,0",          "fade_amt":0,              "flags": "IN_GROUND",                                                        "variable_target_preference":1,     "skills":""},
               {"id":"13:dragon",              				               "area": "CAVERNS",          "rank":"BOSS",        "size": "XL",               "base_stats" : ["hp=28",    "atk=3",      "spd=default"],                "spawn_rate": 1,        "area_spawn_range": "0-50",       "drop_rate":"50",               "drops": "55",                                                                  "fps":4,        "frames":3,           "shadow_size": 23,     "shadow_offset":0,         "fly_height": 30,       "fly_speed": 10,         "projectile_spawn_offset":"0,0",          "fade_amt":0,              "flags": "",                                                                 "variable_target_preference":1,     "skills":"METEOR"},
        */

        //instead, create the data manually
        var ratData = new EnemyCharacterRow()      { id="1:" + EnemyId.RAT,       area="CAVERNS|JAIL",   rank="NORM", size="MD", base_stats = new string[] {"hp=2",     "atk=1",      "spd=default" }, spawn_rate=1, area_spawn_range = "0-50", drop_rate = "3",  drops = "6", flags="", variable_target_preference=1, skills="" };
        var tunnelerData = new EnemyCharacterRow() { id="11:" + EnemyId.TUNNELER, area="CAVERNS",  rank="MBOS", size="MD", base_stats = new string[] {"hp=8",     "atk=2",      "spd=default" },       spawn_rate=1, area_spawn_range = "0-50", drop_rate = "1",  drops = "",  flags="IN_GROUND", variable_target_preference=1, skills="" };
        var dragonData = new EnemyCharacterRow()   { id="13:" + EnemyId.DRAGON,   area="CAVERNS",    rank="BOSS", size="MD", base_stats = new string[] {"hp=28",    "atk=3",      "spd=default" },     spawn_rate=1, area_spawn_range = "0-50", drop_rate = "50", drops = "",  flags="", variable_target_preference=1, skills="METEOR" };

        EnemyCharacterMap map = new EnemyCharacterMap();
        map.enemy_characters = new EnemyCharacterRow[] { ratData, tunnelerData, dragonData };

        EnemyCharacterRow row;
        EnemyCharacterData tempCharacter;
        string[] idPieces;

        for (int i = 0; i < map.enemy_characters.Length; i++)
        {
            row = map.enemy_characters[i];
            tempCharacter = new EnemyCharacterData();
            tempCharacter.Init();

            idPieces = row.id.Split(':');
            int storageId = int.Parse(idPieces[0]);

            tempCharacter.SetId(idPieces[1]);
            tempCharacter.SetStorageId(storageId);
            tempCharacter.SetSpecies(Species.CREATURE);
            tempCharacter.SetBaseFaction(Faction.ENEMY);

            Stats.ParseArrayIntoStatObject(row.base_stats, tempCharacter.baseStats);

            tempCharacter.spawnAreas = Utils.ParseStringToFlagEnum<AdventureArea>(row.area, '|');

            EnemyRank parsedRank = (EnemyRank)Enum.Parse(typeof(EnemyRank), row.rank);
            EnemySize parsedSize = (EnemySize)Enum.Parse(typeof(EnemySize), row.size);
            tempCharacter.SetRankAndSize(parsedRank, parsedSize);

            int spawnRate = row.spawn_rate;
            string[] rangeInfo = row.area_spawn_range.Split('-');
            if (rangeInfo.Length != 2) throw new Exception("Spawn range for enemy " + row.id + " is not exactly two elements");
            int minSpawnRange = int.Parse(rangeInfo[0]);
            int maxSpawnRange = int.Parse(rangeInfo[1]);
            tempCharacter.SetSpawnValues(spawnRate, minSpawnRange, maxSpawnRange);

            string[] drops = row.drops == "" ? new string[] { } : row.drops.Split('|');
            for (int j = 0; j < drops.Length; j++)
                if (!ItemId.IsValid(drops[j])) throw new Exception("Item drop id '" + drops[j] + "' is not valid");

            int dropRate = 9;
            if (row.drop_rate != "default") dropRate = int.Parse(row.drop_rate);
            tempCharacter.SetItemDrops(dropRate, drops);

            tempCharacter.SetVisualValues(row.fps, row.frames, row.shadow_size, row.shadow_offset, row.fade_amt);
            tempCharacter.SetFlightValue(row.fly_height, row.fly_speed);

            CharacterFlags flags = Utils.ParseStringToFlagEnum<CharacterFlags>(row.flags, ',');
            flags |= CharacterFlags.CANT_DEFEND;
            tempCharacter.SetFlags(flags);
            tempCharacter.SetVariableTargetPreference(row.variable_target_preference);

            if(row.skills.Length > 0)
                tempCharacter.SetBaseSkills(row.skills.Split(','));

            //was the cant strike flag set? only allow this if the enemy has at least one other skill that costs 0 mp
            if(tempCharacter.FlagIsSet(CharacterFlags.CANT_STRIKE))
            {
                if(tempCharacter.baseSkills[0].mpCost != 0)
                    throw new Exception("0 mp Default skill not provided for enemy " + row.id + " (cant_strike flag set)");
            }

            enemyCollection.Add(tempCharacter.id, tempCharacter);
            characterCollection.Add(tempCharacter.id, tempCharacter);
        }

        enemyCollectionList = enemyCollection.Values.ToList();

        if(enemyCollection.Count > GameContext.MAX_NUM_ENEMIES_SUPPPORTED)
        {
            throw new Exception("Loaded " + enemyCollection.Count + " enemy types but game only allows a max of " + GameContext.MAX_NUM_ENEMIES_SUPPPORTED);
        }
    }

    public static EnemyCharacterData GetEnemyData(string id)
    {
        return enemyCollection[id];
    }

    public static List<EnemyCharacterData> GetEnemiesForArea(AdventureArea inArea)
    {
        List<EnemyCharacterData> enemiesForArea = new List<EnemyCharacterData>();

        for(int i=0; i<enemyCollectionList.Count; i++)
        {
            if (enemyCollectionList[i].SpawnsInArea(inArea) && !enemyCollectionList[i].isOre)
                enemiesForArea.Add(enemyCollectionList[i]);
        }

        return enemiesForArea;
    }

    public static List<EnemyCharacterData> GetEnemiesOfRank(EnemyRank inRank)
    {
        List<EnemyCharacterData> enemiesOfRank = new List<EnemyCharacterData>();

        for(int i=0; i<enemyCollectionList.Count; i++)
        {
            if (enemyCollectionList[i].rank == inRank && !enemyCollectionList[i].isOre)
                enemiesOfRank.Add(enemyCollectionList[i]);
        }

        return enemiesOfRank;
    }

    public static List<EnemyCharacterData> GetEnemiesOfSize(EnemySize inSize)
    {
        List<EnemyCharacterData> enemiesOfSize = new List<EnemyCharacterData>();

        for(int i=0; i<enemyCollectionList.Count; i++)
        {
            if (enemyCollectionList[i].size == inSize && !enemyCollectionList[i].isOre)
                enemiesOfSize.Add(enemyCollectionList[i]);
        }

        return enemiesOfSize;
    }

    public static string GetRandomBossEnemyId()
    {
        return GetEnemiesOfRank(EnemyRank.BOSS).GetRandomElement().id;
    }

    public static string GetRandomSmallEnemyId()
    {
        return GetEnemiesOfSize(EnemySize.SM).GetRandomElement().id;
    }

    public static void FillSpawnPattern(SpawnPattern inPattern, long inBattleNum, AdventureArea inAdventureArea, int inAreaProgress)
    {
        //HIDDEN: This is the normal routine for filling a spawn pattern from a large collection of enemies
        /*
        //this will fill a spawn pattern. it WILL NOT, however, apply any boosts that would be needed as the dungeon gets harder
        //first pick out all valid enemies for this pattern
        validSpawns = enemyCollectionList.Where(e => e.SpawnsInArea(inAdventureArea) && e.CanSpawnForAreaCompletion(inAreaProgress) && e.spawnRate > 0);

        //next, pick a default enemy to use in case our spawn selection times out
        EnemyCharacterData defaultEnemy = validSpawns.OrderBy(e => e.spawnRate).First();

        //fill each spawn position indivdually
        EnemyCharacterData potentialEnemy;
        int tryCounter;
        int roll;
        for(int i=0; i<inPattern.spawnPoints.Count; i++)
        {
            tryCounter = 0;
            SpawnPoint tempPoint = inPattern.spawnPoints[i];
            validSpawnSubset = validSpawns.Where( e => tempPoint.EnemyFitsSpawnPointRequirements(e) ).ToList();

            while (tempPoint.isEmpty)
            {
                potentialEnemy = validSpawnSubset.GetRandomElement();
                roll = Utils.RandomInRange(1, potentialEnemy.spawnRate);

                if(roll == 1)
                {
                    tempPoint.characterId = potentialEnemy.id;
                    break;
                }

                tryCounter++;
                if(tryCounter > 100)
                {
                    tempPoint.characterId = defaultEnemy.id;
                    break;
                }
            }
        }
        */

        //SIMPLIFIED: for demo purposes I've only created one enemy for each enemy rank, which will always be returned here
        for (int i = 0; i < inPattern.spawnPoints.Count; i++)
        {
            SpawnPoint tempPoint = inPattern.spawnPoints[i];

            if (tempPoint.allowedRank == EnemyRank.NORM)
                tempPoint.characterId = EnemyId.RAT;
            else if (tempPoint.allowedRank == EnemyRank.MBOS)
                tempPoint.characterId = EnemyId.TUNNELER;
            else if (tempPoint.allowedRank == EnemyRank.BOSS)
                tempPoint.characterId = EnemyId.DRAGON;
        }
    }

    public static int GetTotalNumEnemiesInGame()
    {
        return enemyCollectionList.Count;
    }







    public EnemyCharacterData() { }

    public void SetStorageId(int inId)
    {
        storageId = inId;
    }

    public void SetRankAndSize(EnemyRank inRank, EnemySize inSize)
    {
        rank = inRank;
        size = inSize;
    }

    public void SetSpawnValues(int inRate, int inMinAreaPercent, int inMaxAreaPercent)
    {
        spawnRate = inRate;
        areaSpawnRange = new Range(inMinAreaPercent, inMaxAreaPercent);
    }

    public void SetItemDrops(int inRate, string[] inItems)
    {
        dropRate = inRate;
        itemDrops = inItems;
    }

    public void SetVisualValues(int inFps, int inFrames, int inShadowSize, int inShadowOffset, int inFadeAmt)
    {
        fps = inFps;
        frames = inFrames;
        shadowSize = inShadowSize;
        shadowOffset = inShadowOffset;
        fadeAmt = inFadeAmt;
    }

    public void SetFlightValue(int inHeight, int inSpeed)
    {
        flyHeight = inHeight;
        flySpeed = inSpeed;
    }

    public void SetProjectileOffset(Vector2 inOffset)
    {
        projectileSpawnOffset = inOffset;
    }

    public void SetVariableTargetPreference(int inPref)
    {
        variableTargetPreference = inPref;
    }

    public override void CalculateModifiedStats()
    {
        ResetStatsToBase();
        stats.Set(Stat.mp, 5000);
    }

    public bool HasItemDrops()
    {
        return itemDrops.Length > 0;
    }

    public string GetRandomItemDrop()
    {
        return itemDrops.GetRandomElement();
    }

    public long GetGoldValue()
    {
        if (id == EnemyId.THE_DARKEST_ONE)
            return 0;

        long amt = stats.Get(Stat.max_hp);
        amt += stats.Get(Stat.atk);
        amt /= 2;

        return amt;
    }

    public bool CanSpawnForAreaCompletion(int inCompletionPercent)
    {
        return (inCompletionPercent >= areaSpawnRange.min && inCompletionPercent <= areaSpawnRange.max);
    }

    public bool SpawnsInArea(AdventureArea inArea)
    {
        return (spawnAreas & inArea) == inArea;
    }

    public override CharacterData GetCopy()
    {
        EnemyCharacterData copy = new EnemyCharacterData();
        copy.Init();
        copy.SetId(id);
        CopyPrototypeValuesToInstance(copy);
        return copy;
    }
    
    public override void CopyPrototypeValuesToInstance(CharacterData inInstance)
    {
        base.CopyPrototypeValuesToInstance(inInstance);

        EnemyCharacterData ecd = (EnemyCharacterData)inInstance;
        ecd.SetStorageId(storageId);
        ecd.SetRankAndSize(rank, size);
        ecd.SetSpawnValues(spawnRate, areaSpawnRange.min, areaSpawnRange.max);
        ecd.SetItemDrops(dropRate, itemDrops);
        ecd.SetVisualValues(fps, frames, shadowSize, shadowOffset, fadeAmt);
        ecd.SetFlightValue(flyHeight, flySpeed);
        ecd.SetProjectileOffset(projectileSpawnOffset);
        ecd.SetVariableTargetPreference(variableTargetPreference);

        ecd.spawnAreas = spawnAreas;
    }

    protected override void ExpandFromPrototype()
    {
        Init();
        EnemyCharacterData prototype = (EnemyCharacterData)Get(id);
        prototype.CopyPrototypeValuesToInstance(this);
    }

    //INNER STRUCTS TO USE FOR JSON MAPPING
    [System.Serializable]
    public struct EnemyCharacterRow
    {
        public string id;

        public string area;
        public string rank;
        public string size;

        public string[] base_stats;

        public int spawn_rate;
        public string area_spawn_range;

        public string drop_rate;
        public string drops;

        public int fps;
        public int frames;
        public int shadow_size;
        public int shadow_offset;
        public int fly_height;
        public int fly_speed;
        public string projectile_spawn_offset;
        public int fade_amt;

        public string flags;
        public int variable_target_preference;
        public string skills;
    }

    [System.Serializable]
    public struct EnemyCharacterMap
    {
        public EnemyCharacterRow[] enemy_characters;
    }
}
