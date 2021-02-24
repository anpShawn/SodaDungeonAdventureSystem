using System.Collections.Generic;

public class LootBundle : IPoolable
{
    //serialized
    private List<Loot> _loot;
    private bool isActive;
    public bool condenseLootTypes;
    public bool enforceMaxQuantities;

    //public properties
    public List<Loot> loot { get { return _loot; } }
    public bool isEmpty { get { return loot.Count == 0; } }

    public LootBundle()
    {
        _loot = new List<Loot>();
        isActive = false;
        condenseLootTypes = true;
        enforceMaxQuantities = false;
    }

    public void AddGold(long inAmt)
    {
        if (inAmt < 0) return;
        AddLoot(LootType.CURRENCY, GameContext.STR_CURRENCY_GOLD, inAmt);
    }

    public void AddEssence(long inAmt)
    {
        if (inAmt < 0) return;
        AddLoot(LootType.CURRENCY, GameContext.STR_CURRENCY_ESSENCE, inAmt);
    }

    public void AddCaps(long inAmt)
    {
        if (inAmt < 0) return;
        AddLoot(LootType.CURRENCY, GameContext.STR_CURRENCY_CAPS, inAmt);
    }

    public void AddLoot(LootType inType, string inId, long inQuantity)
    {
        if (condenseLootTypes)
        {
            for (int i = 0; i < loot.Count; i++)
            {
                if (loot[i].lootType == inType && loot[i].id == inId)
                {
                    loot[i].quantity += inQuantity;
                    if(enforceMaxQuantities)loot[i].EnforceMaxQuantity();
                    return;
                }
            }
        }

        //if we made it here it means the loot doesn't exist yet in this bundle
        Loot newLoot = new Loot(inType, inId, inQuantity);
        if(enforceMaxQuantities)newLoot.EnforceMaxQuantity();
        loot.Add(newLoot);
    }

    public void RemoveItem(string inItemId, long inQuantity)
    {
        for (int i = loot.Count-1; i >= 0; i--)
        {
            if (loot[i].lootType == LootType.ITEM && loot[i].id == inItemId)
            {
                loot[i].quantity -= inQuantity;
                if (loot[i].quantity <= 0)
                    loot.RemoveAt(i);
                return;
            }
        }
    }

    public void AddLoot(Loot inLoot)
    {
        AddLoot(inLoot.lootType, inLoot.id, inLoot.quantity);
    }

    public void Empty()
    {
        loot.Clear();
    }

    public void EmptyInto(LootBundle otherBundle)
    {
        CopyInto(otherBundle);
        Empty();
    }

    public void CopyInto(LootBundle otherBundle)
    {
        Loot curLoot;
        for (int i = 0; i < loot.Count; i++)
        {
            curLoot = loot[i];
            otherBundle.AddLoot(curLoot.lootType, curLoot.id, curLoot.quantity);
        }
    }

    public List<Item> ToItemList()
    {
        List<Item> list = new List<Item>();

        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.ITEM)
            {
                Item item = Item.GetItem(loot[i].id);
                item.quantity = (int)(loot[i].quantity);
                list.Add(item);
            }

        return list;
    }

    public bool ContainsAnItem()
    {
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.ITEM) return true;
        return false;
    }

    public bool ContainsARareItem()
    {
        Item tempItem;
        for (int i = 0; i < loot.Count; i++)
        {
            if (loot[i].lootType == LootType.ITEM)
            {
                tempItem = Item.GetItem(loot[i].id);
                if (tempItem.rarity == Rarity.RARE)
                    return true;
            }
        }
            
        return false;
    }

    public bool ContainsACraftingRecipe()
    {
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.CRAFTING_RECIPE) return true;
        return false;
    }

    public bool ContainsItem(string inId)
    {
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.ITEM && loot[i].id == inId) return true;
        return false;
    }

    public bool ContainsGold() {
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.CURRENCY && loot[i].id == GameContext.STR_CURRENCY_GOLD) return true;
        return false;
    }

    public long GetGold()
    {
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.CURRENCY && loot[i].id == GameContext.STR_CURRENCY_GOLD) return loot[i].quantity;
        return 0;
    }

    public long GetCaps()
    {
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.CURRENCY && loot[i].id == GameContext.STR_CURRENCY_CAPS) return loot[i].quantity;
        return 0;
    }

    public long GetEssence()
    {
        for (int i = 0; i < loot.Count; i++)
            if (loot[i].lootType == LootType.CURRENCY && loot[i].id == GameContext.STR_CURRENCY_ESSENCE) return loot[i].quantity;
        return 0;
    }

    public void ShuffleContents()
    {
        loot.Shuffle();
    }

    public string ToString()
    {
        string output = "";

        for(int i=0; i<loot.Count; i++)
        {
            output += loot[i].ToString() + ", ";
        }

        if (output.Length > 2)
            output = output.Substring(0, output.Length - 2);

        return output;
    }

    /************************************************************************************************************************************************************
     * IPOOLABLE INTERFACE
     * 
     * *********************************************************************************************************************************************************/

    public bool IsActiveInPool()
    {
        return isActive;
    }

    public void InitForPool()
    {
    }

    public void ActivateForPool()
    {
        isActive = true;
        condenseLootTypes = true;
        enforceMaxQuantities = false;
    }

    public void DeactivateForPool()
    {
        isActive = false;
        Empty();
    }
}
