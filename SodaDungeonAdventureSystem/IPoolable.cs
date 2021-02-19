
public interface IPoolable
{
    bool IsActiveInPool();
    void InitForPool();
    void ActivateForPool();
    void DeactivateForPool();
}
