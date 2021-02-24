using System;

public class EquipmentSlot
{
    //serialized
    private string _id;
    private ItemType _itemType;
    private string _preferredStat;
    private int _slotNumber; 
    private string _equipmentId;
    private string[] _gemIds;

    //public properties
    public string id { get { return _id; } }
    public ItemType itemType { get { return _itemType; } }
    public string preferredStat { get { return _preferredStat; } }
    public int slotNumber { get { return _slotNumber; } }
    public string equipmentId { get { return _equipmentId; } }
    public string[] gemIds { get { return _gemIds; } }

	public EquipmentSlot(ItemType inEquipmentType, int inSlotNumber)
    {
        _itemType = inEquipmentType;
        _slotNumber = inSlotNumber;
        _id = itemType.ToString() + "_" + slotNumber;
        _equipmentId = "";
        if(itemType.CanHaveGemSlots() )_gemIds = new string[4];

        //set preferred stat based on item type
        switch (itemType)
        {
            case ItemType.ARMOR: _preferredStat = Stat.hp; break;
            case ItemType.WEAPON: _preferredStat = Stat.atk; break;
            case ItemType.SHIELD: _preferredStat = Stat.hp; break;
            case ItemType.ACCESSORY: _preferredStat = Stat.atk; break;
        }
    }

    public void SetEquipment(string inId)
    {
        _equipmentId = inId;
    }

    public void SetGem(int inSlotId, string inGemId)
    {
        if (IsEmpty())
            throw new Exception("Cannot set a gem for a slot with no item");

        Item i = Item.GetItem(_equipmentId);
        int numSlots = i.numSlots;

        if (numSlots == 0)
            throw new Exception("The item type in this slot does not have gem slots");

        int maxSlotId = numSlots - 1;

        if (inSlotId < 0 || inSlotId > maxSlotId)
            throw new Exception("Tried to slot a gem into invalid slot ("+inSlotId+") for item " + _equipmentId);

        gemIds[inSlotId] = inGemId;
    }

    public void RemoveGem(int inSlotId)
    {
        SetGem(inSlotId, null);
    }

    public void ClearGemSlots()
    {
        if(_gemIds != null)
        {
            for (int i = 0; i < _gemIds.Length; i++)
                _gemIds[i] = null;
        }
    }

    public void RemoveGemsAfterSlotNumber(int inNum)
    {
        if (!HasGemSlots()) return;

        int slotNum;
        for(int i=0; i<_gemIds.Length; i++)
        {
            slotNum = i + 1;
            if (slotNum > inNum)
                _gemIds[i] = null;
        }
    }

    public bool HasGemSlots()
    {
        return _gemIds != null;
    }

    public void RemoveEquipment()
    {
        _equipmentId = "";
        ClearGemSlots();
    }

    public bool IsEmpty()
    {
        return equipmentId == "";
    }
}
