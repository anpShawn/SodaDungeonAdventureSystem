using System;

[Serializable]public class Loot
{

    //serialized
    private LootType _lootType;
    private string _id;
    public BigInteger quantity;
    [NonSerialized] private int _maxQuantity; //don't save this, it's only being set/used while an adventure is taking place

    //public properties
    public LootType lootType { get { return _lootType; } }
    public string id { get { return _id; } }
    public string name { get { return Locale.Get(_id); } }

    public Loot(LootType inType, string inId, BigInteger inQuantity, int inMaxQuantity = -1)
    {
        _lootType = inType;
        _id = inId;

        //we have to impose a max quantity for certain item types
        _maxQuantity = inMaxQuantity;

        quantity = inQuantity;
    }

    //to save some redundant processing overhead
    public void EnforceMaxQuantity()
    {
        if (_maxQuantity > 0 && quantity > _maxQuantity)
            quantity = _maxQuantity;
    }

    public override string ToString()
    {
        if(_lootType == LootType.CURRENCY)
            return (Utils.FormatBigInt(quantity) + " " + Locale.Get(id.ToUpper()));
        else if(_lootType == LootType.ITEM)
        {
            Item i = Item.GetItem(id);
            return i.name;
        }
        else
        {
            throw new Exception("no tostring method specified for loot type: " + _lootType);
        }
    }
}
