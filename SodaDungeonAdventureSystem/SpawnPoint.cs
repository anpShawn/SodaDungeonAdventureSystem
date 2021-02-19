public class SpawnPoint
{
    public Vector2 position { get; private set; }
    public int direction { get; private set; }
    public EnemyRank allowedRank { get; private set; }
    public EnemySize allowedSizes { get; private set; }
    public bool isEmpty { get { return characterId == ""; } }
    public string characterId = "";

    public SpawnPoint(Vector2 inPosition, int inDirection)
    {
        position = inPosition;
        direction = inDirection;
    }

    public void SetEnemyConstraints(EnemyRank inRank, EnemySize inSizes)
    {
        allowedRank = inRank;
        allowedSizes = inSizes;
    }

    public void SetDirection(int inDirection)
    {
        direction = inDirection;
    }

    public void AugmentPosition(Vector2 inPos)
    {
        position += inPos;
    }

    public void Clear()
    {
        characterId = "";
    }

    public SpawnPoint Clone()
    {
        SpawnPoint sp = new SpawnPoint(position, direction);
        sp.SetEnemyConstraints(allowedRank, allowedSizes);
        return sp;
    }

    public bool EnemyFitsSpawnPointRequirements(EnemyCharacterData e)
    {
        bool isAllowedRank = e.rank == allowedRank;
        bool isAllowedSize = (allowedSizes & e.size) == e.size;
        return isAllowedRank && isAllowedSize;
    }
}
