public class PoolManager
{
    public static ObjectPool<SkillResult> poolSkillResults;
    public static ObjectPool<LootBundle> poolLootBundles;
    public static ObjectPool<Stats> poolStats;

    public static void CreateObjectPools()
    {
        poolSkillResults = new ObjectPool<SkillResult>();
        poolSkillResults.CreatePool(20);

        poolLootBundles = new ObjectPool<LootBundle>();
        poolLootBundles.CreatePool(20);

        poolStats = new ObjectPool<Stats>();
        poolStats.CreatePool(20);
    }

    public static void Recycle(IPoolable inPoolable)
    {
        if (inPoolable.IsActiveInPool())
        {
            inPoolable.DeactivateForPool();
        }
        else throw new System.Exception("tried to recycle an object that was already deactivated");
    }
}