using System.Collections.Generic;

public class SpawnPattern
{
    public static List<SpawnPattern> spawnPatterns;

    //privately cache lists of player spawn patterns for quick access
    private static SpawnPattern[] playerStandardSpawnPatterns;
    private static SpawnPattern[] playerAmbushedSpawnPatterns;

    public static void CreateSpawnPatterns()
    {
        spawnPatterns = new List<SpawnPattern>();
        SpawnPattern tempPattern;

        //player spawn patterns
        tempPattern = new SpawnPattern(SpawnPatternId.ONE_PLAYER);
            tempPattern.AddPlayerSpawnPoint(new Vector2(-2, -.75f), Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.TWO_PLAYER);
            tempPattern.AddPlayerSpawnPoint(new Vector2(-2, -.25f), Direction.RIGHT);
            tempPattern.AddPlayerSpawnPoint(new Vector2(-2, -1f), Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.THREE_PLAYER);
            tempPattern.AddPlayerSpawnPoint(new Vector2(-1.8f, 0), Direction.RIGHT);
            tempPattern.AddPlayerSpawnPoint(new Vector2(-2.5f, -.75f), Direction.RIGHT);
            tempPattern.AddPlayerSpawnPoint(new Vector2(-1.8f, -1.5f), Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.FOUR_PLAYER);
            tempPattern.CopySpawnPointsFrom(SpawnPattern.GetSpawnPattern(SpawnPatternId.THREE_PLAYER));
            tempPattern.AddPlayerSpawnPoint(new Vector2(2, -.75f), Direction.LEFT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.FIVE_PLAYER);
            tempPattern.CopySpawnPointsFrom(SpawnPattern.GetSpawnPattern(SpawnPatternId.THREE_PLAYER));
            tempPattern.AddPlayerSpawnPoint(new Vector2(2, -.25f), Direction.LEFT);
            tempPattern.AddPlayerSpawnPoint(new Vector2(2, -1f), Direction.LEFT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.SIX_PLAYER);
            tempPattern.CopySpawnPointsFrom(SpawnPattern.GetSpawnPattern(SpawnPatternId.THREE_PLAYER));
            tempPattern.AddPlayerSpawnPoint(new Vector2(1.8f, 0), Direction.LEFT);
            tempPattern.AddPlayerSpawnPoint(new Vector2(2.5f, -.75f), Direction.LEFT);
            tempPattern.AddPlayerSpawnPoint(new Vector2(1.8f, -1.5f), Direction.LEFT);
        spawnPatterns.Add(tempPattern);



        //enemy spawn patterns
        tempPattern = new SpawnPattern(SpawnPatternId.ONE_ENEMY);
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(0, 0));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.TWO_ENEMY);
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(-.5f, .13f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(.5f, -.13f));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.THREE_ENEMY);
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(.5f, .5f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(-.5f, 0));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(.5f, -.5f));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.FOUR_ENEMY);
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(.5f, .75f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(-.5f, .25f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(.5f, -.25f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(-.5f, -.75f));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.FIVE_ENEMY);
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(-.5f, 1f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(.5f, .5f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(-.5f, 0));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(.5f, -.5f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(-.5f, -1f));
        spawnPatterns.Add(tempPattern);

        /*
         * NO SIX ENEMY SPAWN RIGHT NOW
         * */

        tempPattern = new SpawnPattern(SpawnPatternId.SEVEN_ENEMY);
            tempPattern.CopySpawnPointsFrom(SpawnPattern.GetSpawnPattern(SpawnPatternId.FIVE_ENEMY));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(-1.5f, 0));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(1.5f, 0));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.MINIBOSS);
            tempPattern.AddEnemySpawnPoint(EnemyRank.MBOS, EnemySize.ALL, new Vector2(0, -.3f));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.BOSS);
            tempPattern.AddEnemySpawnPoint(EnemyRank.BOSS, EnemySize.ALL, new Vector2(0, -.4f));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.TEMPERED_BOSS);
            float yPos = -.5f;
            tempPattern.CopySpawnPointsFrom(SpawnPattern.GetSpawnPattern(SpawnPatternId.BOSS));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.SM, new Vector2(-.4f, yPos));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.SM, new Vector2(.4f, yPos));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.SM, new Vector2(1.2f, yPos));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.SM, new Vector2(-1.2f, yPos));
        spawnPatterns.Add(tempPattern);


        
        //SHIFT THE Y POSITION OF ALL ENEMY SPAWNS DOWN BY .75f
        List<SpawnPoint> points;
        Vector2 augment = new Vector2(0, -.75f);
        for(int i=0; i<spawnPatterns.Count; i++)
        {
            if(spawnPatterns[i].isEnemySpawnPattern)
            {
                points = spawnPatterns[i].spawnPoints;
                for(int j=0; j<points.Count; j++)
                {
                    points[j].AugmentPosition(augment);
                }
            }
        }



        /*
         * AMBUSH SPAWNS
         * */

         tempPattern = new SpawnPattern(SpawnPatternId.TWO_ENEMY_AMBUSH);
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(-2f, -.7f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.ALL, new Vector2(2f, -.7f));
            tempPattern.spawnPoints[0].SetDirection(Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.FOUR_ENEMY_AMBUSH);
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(-2f, -.2f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.MED_OR_SMALLER, new Vector2(2f, -.2f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.SM, new Vector2(-2f, -1.2f));
            tempPattern.AddEnemySpawnPoint(EnemyRank.NORM, EnemySize.SM, new Vector2(2f, -1.2f));
            tempPattern.spawnPoints[0].SetDirection(Direction.RIGHT);
            tempPattern.spawnPoints[2].SetDirection(Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.SIX_ENEMY_AMBUSH);
            tempPattern.CopySpawnPointsFrom(SpawnPattern.GetSpawnPattern(SpawnPatternId.SIX_PLAYER));
            tempPattern.SetEnemyConstraintsOnAllPoints(EnemyRank.NORM, EnemySize.MED_OR_SMALLER);
        spawnPatterns.Add(tempPattern);




        tempPattern = new SpawnPattern(SpawnPatternId.ONE_PLAYER_AMBUSHED);
            tempPattern.CopySpawnPointsFrom(GetSpawnPattern(SpawnPatternId.ONE_ENEMY));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.TWO_PLAYER_AMBUSHED);
            tempPattern.CopySpawnPointsFrom(GetSpawnPattern(SpawnPatternId.TWO_ENEMY));
            tempPattern.spawnPoints[0].SetDirection(Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.THREE_PLAYER_AMBUSHED);
            tempPattern.CopySpawnPointsFrom(GetSpawnPattern(SpawnPatternId.THREE_ENEMY));
            tempPattern.spawnPoints[1].SetDirection(Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.FOUR_PLAYER_AMBUSHED);
            tempPattern.CopySpawnPointsFrom(GetSpawnPattern(SpawnPatternId.FOUR_ENEMY));
            tempPattern.spawnPoints[0].SetDirection(Direction.RIGHT);
            tempPattern.spawnPoints[1].SetDirection(Direction.RIGHT);
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.FIVE_PLAYER_AMBUSHED);
            tempPattern.CopySpawnPointsFrom(GetSpawnPattern(SpawnPatternId.FIVE_ENEMY));
            tempPattern.spawnPoints[0].SetDirection(Direction.RIGHT);
            tempPattern.spawnPoints[0].AugmentPosition(new Vector2(0, -.2f));
            tempPattern.spawnPoints[1].SetDirection(Direction.RIGHT);
            tempPattern.spawnPoints[4].AugmentPosition(new Vector2(0, .2f));
        spawnPatterns.Add(tempPattern);

        tempPattern = new SpawnPattern(SpawnPatternId.SIX_PLAYER_AMBUSHED);
            tempPattern.CopySpawnPointsFrom(GetSpawnPattern(SpawnPatternId.FIVE_PLAYER_AMBUSHED));
            tempPattern.AddPlayerSpawnPoint(new Vector2(.5f, -.75f), Direction.LEFT);
        spawnPatterns.Add(tempPattern);


        //create cached player spawn pattern arrays
        playerStandardSpawnPatterns = new SpawnPattern[6];
        playerStandardSpawnPatterns[0] = GetSpawnPattern(SpawnPatternId.ONE_PLAYER);
        playerStandardSpawnPatterns[1] = GetSpawnPattern(SpawnPatternId.TWO_PLAYER);
        playerStandardSpawnPatterns[2] = GetSpawnPattern(SpawnPatternId.THREE_PLAYER);
        playerStandardSpawnPatterns[3] = GetSpawnPattern(SpawnPatternId.FOUR_PLAYER);
        playerStandardSpawnPatterns[4] = GetSpawnPattern(SpawnPatternId.FIVE_PLAYER);
        playerStandardSpawnPatterns[5] = GetSpawnPattern(SpawnPatternId.SIX_PLAYER);

        playerAmbushedSpawnPatterns = new SpawnPattern[6];
        playerAmbushedSpawnPatterns[0] = GetSpawnPattern(SpawnPatternId.ONE_PLAYER_AMBUSHED);
        playerAmbushedSpawnPatterns[1] = GetSpawnPattern(SpawnPatternId.TWO_PLAYER_AMBUSHED);
        playerAmbushedSpawnPatterns[2] = GetSpawnPattern(SpawnPatternId.THREE_PLAYER_AMBUSHED);
        playerAmbushedSpawnPatterns[3] = GetSpawnPattern(SpawnPatternId.FOUR_PLAYER_AMBUSHED);
        playerAmbushedSpawnPatterns[4] = GetSpawnPattern(SpawnPatternId.FIVE_PLAYER_AMBUSHED);
        playerAmbushedSpawnPatterns[5] = GetSpawnPattern(SpawnPatternId.SIX_PLAYER_AMBUSHED);
    }

    public static SpawnPattern GetSpawnPattern(SpawnPatternId inId)
    {
        for (int i = 0; i < spawnPatterns.Count; i++)
            if (spawnPatterns[i].id == inId) return spawnPatterns[i];

        throw new System.Exception("No Spawn pattern with id '" + inId + "' exists");
    }

    public static SpawnPattern GetTestSpawnPattern(int inNum)
    {
        SpawnPattern tempPattern;
        string enemyType;

        if(inNum ==1)
        {
            enemyType = "toad";

            tempPattern = GetSpawnPattern(SpawnPatternId.TWO_ENEMY);
            tempPattern.FillManually(enemyType, enemyType);
        }
        else if(inNum == 2)
        {
            tempPattern = GetSpawnPattern(SpawnPatternId.SEVEN_ENEMY);
            enemyType = EnemyId.FLY; 
            tempPattern.FillManually(enemyType, enemyType, enemyType, enemyType, enemyType, enemyType, enemyType);
        }
        else if(inNum == 3)
        {
            tempPattern = GetSpawnPattern(SpawnPatternId.FOUR_ENEMY);
            tempPattern.FillManually(EnemyId.IRON_ORE, EnemyId.IRON_ORE, EnemyId.IRON_ORE, EnemyId.RAT);
        }
        else
        {
            throw new System.Exception("requested invalid test spawn pattern: " + inNum);
        }
        
        return tempPattern;
    }

    public static SpawnPattern GetStandardPlayerSpawnPatternForTeamSize(int inSize)
    {
        return playerStandardSpawnPatterns[inSize - 1];
    }

     public static SpawnPattern GetAmbushedPlayerSpawnPatternForTeamSize(int inSize)
    {
        return playerAmbushedSpawnPatterns[inSize - 1];
    }

    public SpawnPatternId id { get; private set; }
    public List<SpawnPoint> spawnPoints { get; private set; }
    public int availableSpawnPoints { get { return spawnPoints.Count; } }
    public bool isEnemySpawnPattern { get; private set; }

    private SpawnPattern(SpawnPatternId inId)
    {
        id = inId;
        spawnPoints = new List<SpawnPoint>();
    }

    private void AddEnemySpawnPoint(EnemyRank inRank, EnemySize inSize, Vector2 inPosition)
    {
        isEnemySpawnPattern = true;

        //enemies always face left
        SpawnPoint s = new SpawnPoint(inPosition, Direction.LEFT);
        s.SetEnemyConstraints(inRank, inSize);
        spawnPoints.Add(s);
    }

    private void AddPlayerSpawnPoint(Vector2 inPosition, int inDirection)
    {
        spawnPoints.Add(new SpawnPoint(inPosition, inDirection));
    }

    private void SetEnemyConstraintsOnAllPoints(EnemyRank inRank, EnemySize inSize)
    {
        for (int i = 0; i < spawnPoints.Count; i++)
            spawnPoints[i].SetEnemyConstraints(inRank, inSize);
    }

    //use this method to quickly fill a spawn pattern manually, instead of letting the dungeon figure it out
    public void FillManually(params string[] inIds)
    {
        if(inIds.Length > availableSpawnPoints) throw new System.Exception("Requested too many spawns (" + inIds.Length + ") from pattern: " + id);

        for(int i=0; i<inIds.Length; i++)
        {
            spawnPoints[i].characterId = inIds[i];
        }
    }

    public void ChangeRandomSpawnTo(EnemyCharacterData inCharacter)
    {
        spawnPoints.GetRandomElement().characterId = inCharacter.id;
    }

    public void CopySpawnPointsFrom(SpawnPattern inPattern)
    {
        SpawnPoint originalSp;
        for(int i=0; i<inPattern.spawnPoints.Count; i++)
        {
            originalSp = inPattern.spawnPoints[i];
            spawnPoints.Add(originalSp.Clone());
        }
    }

    public void ClearSpawns()
    {
        for (int i = 0; i < spawnPoints.Count; i++) spawnPoints[i].Clear();
    }

    public bool IsFull()
    {
        int numFilled = 0;
        for (int i = 0; i < spawnPoints.Count; i++)
            if (!spawnPoints[i].isEmpty) numFilled++;

        return numFilled == spawnPoints.Count;
    }
}
