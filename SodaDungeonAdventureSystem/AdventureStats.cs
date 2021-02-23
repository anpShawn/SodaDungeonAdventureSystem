public class AdventureStats
{
    public DungeonId dungeonId;
    public long highestLevelComplete;
    public long essenceFromGnomes;
    public CharacterTeam playerTeam;
    public int soulOrbsCaptured; 
    private int[] enemyKillCounts;

    public AdventureStats()
    {
        enemyKillCounts = new int[GameContext.MAX_NUM_ENEMIES_SUPPPORTED];
        essenceFromGnomes = 0;
    }

    public void IncrementEnemyKillCount(int inId)
    {
        int index = inId - 1;
        if (enemyKillCounts[index] < int.MaxValue)
            enemyKillCounts[index]++;
    }

    public int GetEnemyKillCount(int inId)
    {
        return enemyKillCounts[inId - 1];
    }
}
