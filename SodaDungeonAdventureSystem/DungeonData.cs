using System;
using System.Collections.Generic;

public class DungeonData
{
    public static Dictionary<DungeonId, DungeonData> dungeonDictionary;

    public DungeonId type;
    public string name { get { return type.ToString(); } }
    public int bossesPerArea;
    public int minibossesPerBoss;
    public int battlesPerArea;
    public int hpIncreasePerArea;
    public int atkIncreasePerArea;
    public int battleLimit;
    public int numBattlesUntilFullLoop;
    public List<AdventureArea> areas;

    private List<long> bossLevels;
    private List<long> minibossLevels;

    private static Dictionary<AdventureArea, string> areaMusicIds;

    public static DungeonData GetDungeon(DungeonId id)
    {
        return dungeonDictionary[id];
    }

    public static void CreateDungeons()
    {
        dungeonDictionary = new Dictionary<DungeonId, DungeonData>();

        //use this line to read from a json file
        //DungeonMap map = Utils.LoadJsonFromPath<DungeonMap>("data/dungeon_data");

        //instead, create the data manually
        var castle = new DungeonRow() {id = "CASTLE", bossesPerArea=2, minibossesPerBoss=1, battlesPerArea=20, hpIncreasePerArea=13, atkIncreasePerArea=2, battleLimit=0, areas = new string[] { "CAVERNS", "JAIL", "KITCHEN", "ARMORY", "THRONE_ROOM" } };
        DungeonMap map = new DungeonMap();
        map.dungeons = new DungeonRow[] { castle };

        DungeonRow row;
        DungeonData tempDungeon;
        for (int i = 0; i < map.dungeons.Length; i++)
        {
            row = map.dungeons[i];

            DungeonId parsedType = (DungeonId)Enum.Parse(typeof(DungeonId), row.id);
            tempDungeon = new DungeonData(parsedType);

            tempDungeon.SetBattleConstraints(row.bossesPerArea, row.minibossesPerBoss, row.battlesPerArea, row.hpIncreasePerArea, row.atkIncreasePerArea, row.battleLimit);
            tempDungeon.SetAreas(row.areas);

            dungeonDictionary.Add(tempDungeon.type, tempDungeon);
        }


        areaMusicIds = new Dictionary<AdventureArea, string>();
        areaMusicIds[AdventureArea.CAVERNS] = "stage_1";
        areaMusicIds[AdventureArea.JAIL] = "jail";
        areaMusicIds[AdventureArea.KITCHEN] = "kitchen";
        areaMusicIds[AdventureArea.ARMORY] = "stage_2";
        areaMusicIds[AdventureArea.THRONE_ROOM] = "throne_room";

        areaMusicIds[AdventureArea.ARENA] = "stage_2";
    }

    public static string GetMusicIdForArea(AdventureArea inArea)
    {
        return areaMusicIds[inArea];
    }






	private DungeonData(DungeonId inType)
    {
        type = inType;
    }

    private void SetBattleConstraints(int inBossesPerArea, int inMinibossesPerBoss, int inBattlesPerArea, int inHpIncrease, int inAtkIncrease, int inBattleLimit)
    {
        bossesPerArea = inBossesPerArea;
        minibossesPerBoss = inMinibossesPerBoss;

        battlesPerArea = inBattlesPerArea;
        hpIncreasePerArea = inHpIncrease;
        atkIncreasePerArea = inAtkIncrease;
        battleLimit = inBattleLimit;

        bossLevels = new List<long>();
        minibossLevels = new List<long>();

        if (bossesPerArea == 0) return;
        if (bossesPerArea > battlesPerArea) throw new Exception("Number of bosses in an area cannot exceed the number of battles in an area");

        //we add all but the last boss level to the list
        int distanceBetweenBosses = battlesPerArea / bossesPerArea;
        int bossLevel = distanceBetweenBosses;
        int bossLevelsToAdd = bossesPerArea - 1;
        for(int i=0; i<bossLevelsToAdd; i++)
        {
            bossLevels.Add(bossLevel);
            bossLevel += distanceBetweenBosses;
        }

        //then we ensure the last boss level is always at the very end of the area
        bossLevels.Add(battlesPerArea);

        if (minibossesPerBoss == 0) return;

        /*For minibosses we always take the distance between bosses and divide by the number of minibosses we want. The number of minibosses requested must be less than half the distance between bosses.

        20 levels per area, two bosses. Bosses are at level 10 and 20. Distance between bosses: 10.

        Max allowed minibosses is 4.
        10/5 = 2

        We place one boss at start + 2, and end -2

        If we requested two minibosses per area. We’d want them to appear at 3 and 7
        Roughly 3 between each. How do we get that number?
        10 / (2+1) = 3.33

        Add one to the num requested. Then make sure to start one iteration in. so start at 3, not 0. 
        */
        if (minibossesPerBoss >= distanceBetweenBosses / 2) throw new Exception("Too many minibosses requested");

        int distanceBetweenMinibosses = distanceBetweenBosses / (minibossesPerBoss + 1);
        long minibossLevel = distanceBetweenMinibosses;
        for (int i = 0; i < bossLevels.Count; i++)
        {
            for (int j = 0; j < minibossesPerBoss; j++)
            {
                if (!IsBossLevel(minibossLevel)) minibossLevels.Add(minibossLevel);
                minibossLevel += distanceBetweenMinibosses;
            }

            //after each "boss" set the miniboss level equal to the current boss level + distance between minibosses
            minibossLevel = bossLevels[i] + distanceBetweenMinibosses;
        }
    }

    private void SetAreas(string[] inAreaIds)
    {
        areas = new List<AdventureArea>();
        string enumVal;
        for(int i=0; i<inAreaIds.Length; i++)
        {
            enumVal = inAreaIds[i].ToUpper();
            AdventureArea parsedArea = (AdventureArea)Enum.Parse(typeof(AdventureArea), enumVal);
            areas.Add(parsedArea);
        }

        numBattlesUntilFullLoop = areas.Count * battlesPerArea;
    }

    public AdventureArea GetAreaForBattleNum(long inBattleNum)
    {
        long areaNum = GetAreaNumForBattleNum(inBattleNum);
        int numAreas = areas.Count;

        int loopedArea = (int)(areaNum % numAreas);
        if (loopedArea == 0) loopedArea = numAreas;

        return areas[loopedArea - 1];
    }

    public long GetAreaNumForBattleNum(long inBattleNum)
    {
        double val = (double)inBattleNum / battlesPerArea;
        return (long)Math.Ceiling(val);
    }

    public bool HasBattleLimit()
    {
        return battleLimit > 0;
    }

    public bool IsBossLevel(long inLevelNum)
    {
        for (int i = 0; i < bossLevels.Count; i++)
            if (bossLevels[i] == inLevelNum) return true;
        return false;
    }

    public bool IsMinibossLevel(long inLevelNum)
    {
        for (int i = 0; i < minibossLevels.Count; i++)
            if (minibossLevels[i] == inLevelNum) return true;
        return false;
    }

    public bool IsTreasureLevel(long inLevelNum)
    {
        //right now, we are considering treasure levels to be equal to boss levels
        if (GameContext.DEBUG_CONSTANT_TREASURE) return true;
        return IsBossLevel(inLevelNum);
    }

    public bool IsAnArena()
    {
        //normally we check the dungeon id here but since arena functionality has been disabled, always return false
        return false;
    }

    //INNER STRUCTS TO USE FOR JSON MAPPING
    [System.Serializable]
    public struct DungeonRow
    {
        public string id;

        public int bossesPerArea;
        public int minibossesPerBoss;
        public int battlesPerArea;
        public int hpIncreasePerArea;
        public int atkIncreasePerArea;
        public int battleLimit;

        public string[] areas;
    }

    [System.Serializable]
    public struct DungeonMap
    {
        public DungeonRow[] dungeons;
    }
}
