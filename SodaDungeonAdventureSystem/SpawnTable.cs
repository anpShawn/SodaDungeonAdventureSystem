using System.Collections.Generic;

public class SpawnTable
{
    private static Dictionary<DungeonId, List<SpawnTable>> oreTables;

    public static void CreateOreTables()
    {
        oreTables = new Dictionary<DungeonId, List<SpawnTable>>();
        SpawnTable tempTable;

        List<SpawnTable> castleOreTables = new List<SpawnTable>();
        oreTables[DungeonId.CASTLE] = castleOreTables;

        tempTable = AddSpawnTableTo(castleOreTables, 1, 100);
        tempTable.AddSpawnRange(EnemyId.IRON_ORE, 1, 80);
        tempTable.AddSpawnRange(EnemyId.COPPER_ORE, 81, 95);
        tempTable.AddSpawnRange(EnemyId.SILVER_ORE, 96, 99); 
        tempTable.AddSpawnRange(EnemyId.GOLD_ORE, 100, 100);

        tempTable = AddSpawnTableTo(castleOreTables, 101, 499);
        tempTable.AddSpawnRange(EnemyId.IRON_ORE, 1, 70);
        tempTable.AddSpawnRange(EnemyId.COPPER_ORE, 71, 91);
        tempTable.AddSpawnRange(EnemyId.SILVER_ORE, 92, 98); 
        tempTable.AddSpawnRange(EnemyId.GOLD_ORE, 99, 100);

        tempTable = AddSpawnTableTo(castleOreTables, 500, 1000);
        tempTable.AddSpawnRange(EnemyId.IRON_ORE, 1, 100);
        tempTable.AddSpawnRange(EnemyId.COPPER_ORE, 101, 200);
        tempTable.AddSpawnRange(EnemyId.SILVER_ORE, 201, 290); 
        tempTable.AddSpawnRange(EnemyId.GOLD_ORE, 291, 298); 
        tempTable.AddSpawnRange(EnemyId.PLATINUM_ORE, 299, 300);


        tempTable = AddSpawnTableTo(castleOreTables, 1001, 5000);
        tempTable.AddSpawnRange(EnemyId.COPPER_ORE, 1, 200);
        tempTable.AddSpawnRange(EnemyId.SILVER_ORE, 201, 400); 
        tempTable.AddSpawnRange(EnemyId.GOLD_ORE, 401, 489); 
        tempTable.AddSpawnRange(EnemyId.PLATINUM_ORE, 490, 498);
        tempTable.AddSpawnRange(EnemyId.URANIUM_ORE, 499, 500);

        tempTable = AddSpawnTableTo(castleOreTables, 5001, 6000);
        tempTable.AddSpawnRange(EnemyId.COPPER_ORE, 1, 100);
        tempTable.AddSpawnRange(EnemyId.SILVER_ORE, 101, 349); 
        tempTable.AddSpawnRange(EnemyId.GOLD_ORE, 350, 481); 
        tempTable.AddSpawnRange(EnemyId.PLATINUM_ORE, 482, 497);
        tempTable.AddSpawnRange(EnemyId.URANIUM_ORE, 498, 500);
    }

    private static SpawnTable AddSpawnTableTo(List<SpawnTable> inTables, int inMinDungeonLevel, int inMaxDungeonLevel)
    {
        //max must ALWAYS be greater than min
        if (inMaxDungeonLevel <= inMinDungeonLevel)
            throw new System.Exception("tried to add an item table where the max level wasn't greater than the min");

        //min must be EXACTLY one greater than the previous max (if no previous item table, it MUST be 1)
        int requiredMin = 1;
        if (inTables.Count > 0)
            requiredMin = inTables[inTables.Count - 1].dungeonLevelRange.max + 1;

        if (inMinDungeonLevel != requiredMin)
            throw new System.Exception("tried to add an item table that didn't have the required min level of " + requiredMin);

        SpawnTable t = new SpawnTable(inMinDungeonLevel, inMaxDungeonLevel);
        inTables.Add(t);

        return t;
    }

    public static SpawnTable GetOreTable(DungeonId inDungeon, int inCurLevel)
    {
        if (oreTables.ContainsKey(inDungeon))
        {
            List<SpawnTable> tables = oreTables[inDungeon];
            for(int i=0; i<tables.Count; i++)
            {
                if (tables[i].ContainsDungeonLevel(inCurLevel))
                    return tables[i];
            }

            //none of the tables contained our requested level so just return the highest one
            return tables[tables.Count - 1];
        }

        return null;
    }













    private Range dungeonLevelRange;
    private List<Range> spawnRanges;
    private List<string> spawns; //this list runs parallel to item ranges

    public SpawnTable(int inMinLevel, int inMaxLevel)
    {
        dungeonLevelRange = new Range(inMinLevel, inMaxLevel);
        spawnRanges = new List<Range>();
        spawns = new List<string>();
    }

    public bool ContainsDungeonLevel(int inLevel)
    {
        return dungeonLevelRange.Contains(inLevel);
    }

    public void AddSpawnRange(string inSpawnId, int inRollMin, int inRollMax)
    {
        //make sure that max is not less than min (it can be the same though)
        if (inRollMax < inRollMin)
            throw new System.Exception("tried to add a spawn range where the max roll wasn't greater than or equal to the min");

        //ensure that in min is exactly 1 higher than the previous range's max (or 1 if the first one)
        int requiredMin = 1;
        if (spawnRanges.Count > 0)
            requiredMin = spawnRanges[spawnRanges.Count - 1].max + 1;

        if (inRollMin != requiredMin)
            throw new System.Exception("tried to add a spawn range that didn't have the required min level of " + requiredMin);

        spawnRanges.Add(new Range(inRollMin, inRollMax));
        spawns.Add(inSpawnId);
    }

    public string RollSpawn(int inChanceToBumpQuality=0)
    {
        int min = 1;
        int max = spawnRanges[spawnRanges.Count - 1].max;
        int roll = Utils.RandomInRange(min, max+1); //we have to add one to max since the max param is exclusive

        for(int i=0; i<spawnRanges.Count; i++)
        {
            if(spawnRanges[i].Contains(roll))
            {
                //grab the spawn id from the parallel list. roll the chance to bump to the next range (if possible)
                if(inChanceToBumpQuality != 0 && Utils.PercentageChance(inChanceToBumpQuality) && i < (spawns.Count-1))
                    return spawns[i+1];
                else
                    return spawns[i];
            }
        }

        throw new System.Exception("Unable to roll a spawn for roll " + roll + " with range " + min + "-" + max);
    }
}
