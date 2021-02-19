
using System.Collections.Generic;
using System;

public class ObjectPool<T> where T:IPoolable
{
    private int poolIndex;
    private T[] pool;
    private bool overflowIsAllowed = false;
    private string poolTypeName;
    private List<T> overflow;
    private Func<string, T> FuncCreatePoolObject;

	public ObjectPool(bool inOverflowAllowed = false) 
    {
        poolIndex = 0;

        overflowIsAllowed = inOverflowAllowed;
        if(inOverflowAllowed)
            overflow = new List<T>();
    }

    public void CreatePool(int inPoolSize)
    {
        pool = new T[inPoolSize];

        poolTypeName = "";
        FuncCreatePoolObject = (string n) => CreateClassObject();

        for(int i=0; i<pool.Length; i++)
        {
            T newItem = FuncCreatePoolObject(poolTypeName);
            newItem.InitForPool();
            AddToPool(newItem);
        }
    }

    private T CreateClassObject()
    {
        return (T)Activator.CreateInstance<T>();
    }

    public void AddToPool(T inPoolable)
    {
        pool[poolIndex] = inPoolable;
        poolIndex++;
    }

    public void DeactivateAll()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].IsActiveInPool())
            {
                pool[i].DeactivateForPool();
            }
        }
    }

    public T GetNext()
    {
        for(int i=0; i<pool.Length; i++)
        {
            if(!pool[i].IsActiveInPool())
            {
                pool[i].ActivateForPool();
                return pool[i];
            }
        }

        //can we make an overflow object if no others are available?
        if(overflowIsAllowed)
        {
            T overflowObj = FuncCreatePoolObject(poolTypeName + "_overflow_" + overflow.Count);
            overflowObj.InitForPool();
            overflowObj.ActivateForPool();
            overflow.Add(overflowObj);
            return overflowObj;
        }
        else
            throw new Exception("Pool has no inactive members to distribute");
    }

    public T[] GetPool()
    {
        return pool;
    }

    public List<T> GetOverflow()
    {
        return overflow;
    }
}
