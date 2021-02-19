



/*
* THIS IS A STUBBED-OUT SAVE FILE CLASS THAT ALLOWS THE ADVENTURE SYSTEM TO WORK CORRRECTLY
* NORMALLY THIS CLASS WOULD PROVIDE MUCH MORE EXTENSIVE FUNCTIONALITY, NAMELY THE SHARING OF PERSISTENT DATA BETWEEN ALL AREAS OF THE GAME
* 
*/



public class SaveFile
{
    //poor man's static singleton
    private static SaveFile saveFile;
    public static SaveFile GetSaveFile()
    {
        if (saveFile == null)
            saveFile = new SaveFile();

        return saveFile;
    }
    
    





    public SaveFile()
    {

    }

    //always assume dimension 1
    public int GetCurDimension()
    {
        return 1;
    }

    //dark lord never in dungeon
    public bool DarkLordIsInDungeon(DungeonId inId)
    {
        return false;
    }

    //even though dark lord is never in dungeon, these functions need to exist. return their dimension 1 values
    public int GetDarkLordChallengeBattleNum()
    {
        return 81;
    }

    public int GetDarkLordFightBattleNum()
    {
        return 100;
    }

    //highest level completed? let's make it 100
    public int GetHighestLevelCompleted(DungeonId inId)
    {
        return 100;
    }

    //accept data but do nothing with it
    public void AddLoot(Loot inLoot)
    {
    }

    public void AddLootBundle(LootBundle inBundle)
    {
    }

    public void IncrementEnemyKillCount(int inId)
    {
    }

    public void UpdateHighestLevelCompletedIfHigher(DungeonId inId, long inLevel)
    {
    }

    //assume any flag is set to true
    public bool FlagIsSet(int inFlag)
    {
        return true;
    }
}