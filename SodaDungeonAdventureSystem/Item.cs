using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[Serializable]
public class Item
{
    //static
    protected static Dictionary<string, Item> items;
    private static List<Item> itemCollectionList;
    private static IEnumerable<Item> validItems;
    private static List<string> foodItemIds;
    private static Stats itemLevelBoosts; //set of stats that indicate how much an item can be boosted per level
    private static ItemType itemTypesThatLevelUp;
    private static int numBasicItemsInGame;

    private static Dictionary<StatusEffectType, ItemFlags> statusEffectToPreventionFlagMap;

    public static Item GetItem(string id)
    {
        return items[id];
    }

    public static List<Item> GetAllItemsOfBaseId(string inBaseId)
    {
        List<Item> tempItems = new List<Item>();
        for(int i=0; i<itemCollectionList.Count; i++)
        {
            if (itemCollectionList[i].baseId == inBaseId)
                tempItems.Add(itemCollectionList[i]);
        }
        return tempItems;
    }

    public static string ConvertIdToBaseId(string inFullId)
    {
        return inFullId.Split('_')[0];
    }

    public static Item GetLeveledItem(string inBaseId, int inLevel)
    {
        string idOfLeveledItem = inBaseId + "_" + inLevel;
        return GetItem(idOfLeveledItem);
    }

    public static Item GetLeveledItem(Item inItem, int inLevel)
    {
        string idOfLeveledItem = inItem.baseId + "_" + inLevel;
        return GetItem(idOfLeveledItem);
    }

    public static void CreateItems()
    {
        items = new Dictionary<string, Item>();

        //use this line to load from a json file
        //ItemMap map = Utils.LoadJsonFromPath<ItemMap>("data/item_data");

        /*
         *      {"id":"3:wooden_sword",                                              "type":"WEAPON",         "stats" : ["atk=2", "crit_chance=1","crit_bonus=10"],                                                           "spawn_rate":"15",        "min_spawn_lvl":"5",          "max_spawn_lvl":"0",          "area":"CAVERNS",                                         "attached_skill_ids":"",                                                                        "season":"ALL",                  "slots": [],                           "flags":"",                                               "explicit_rarity":"",                   "explicit_value":0},
                {"id":"4:buckler",                                                   "type":"SHIELD",         "stats" : ["hp=2"],                                                                                             "spawn_rate":"15",        "min_spawn_lvl":"5",          "max_spawn_lvl":"0",          "area":"CAVERNS",                                         "attached_skill_ids":"",                                                                        "season":"ALL",                  "slots": [],                           "flags":"",                                               "explicit_rarity":"",                   "explicit_value":0},
                {"id":"5:leather_armor",                                             "type":"ARMOR",          "stats" : ["hp=1"],                                                                                             "spawn_rate":"20",        "min_spawn_lvl":"5",          "max_spawn_lvl":"0",          "area":"CAVERNS",                                         "attached_skill_ids":"",                                                                        "season":"ALL",                  "slots": [],                           "flags":"",                                               "explicit_rarity":"",                   "explicit_value":0},
         * 
         */

        //instead, create the data manually
        var woodenSwordData = new ItemRow()  { id = "3:" + ItemId.WOODEN_SWORD,    type="WEAPON",   stats = new string[] { "atk=2", "crit_chance=1","crit_bonus=10"},   spawn_rate=15,   min_spawn_lvl=5,  max_spawn_lvl=0, area="CAVERNS", attached_skill_ids="", slots = new string[] { }, flags=""  };
        var woodenShieldData = new ItemRow() { id = "4:" + ItemId.BUCKLER,         type="SHIELD",   stats = new string[] { "hp=2"},                                     spawn_rate=15,   min_spawn_lvl=5,  max_spawn_lvl=0, area="CAVERNS", attached_skill_ids="", slots = new string[] { }, flags=""  };
        var leatherArmorData = new ItemRow() { id = "5:" + ItemId.LEATHER_ARMOR,   type="ARMOR",    stats = new string[] { "hp=1"},                                     spawn_rate=20,   min_spawn_lvl=5,  max_spawn_lvl=0, area="CAVERNS", attached_skill_ids="", slots = new string[] { }, flags=""  };
        var peltData = new ItemRow()         { id = "6:" + ItemId.PELT,            type="RESOURCE", stats = new string[] { },                                           spawn_rate=0,    min_spawn_lvl=0,  max_spawn_lvl=0, area="CAVERNS", attached_skill_ids="", slots = new string[] { }, flags=""  };

        ItemMap map = new ItemMap();
        map.items = new ItemRow[] { woodenSwordData, woodenShieldData, leatherArmorData, peltData };

        ItemRow row;
        Item tempItem;
        string[] idPieces;
        string id;

        for (int i = 0; i < map.items.Length; i++)
        {
            row = map.items[i];

            //item ids are provied in a number:name format. the name is only present in the data file for readability, so we need to discard it
            idPieces = row.id.Split(':');
            id = idPieces[0];

            if (!ItemId.IsValid(id)) throw new Exception("Item id '" + row.id + "' does not exist");

            tempItem = new Item();
            tempItem.Init();

            tempItem.SetId(id);
            tempItem.DeriveBaseAndStorageId();
            ItemType parsedType = (ItemType)Enum.Parse(typeof(ItemType), row.type);
            tempItem.SetType(parsedType);

            Stats.ParseArrayIntoStatObject(row.stats, tempItem.baseStats);

            AdventureArea parsedAreas = Utils.ParseStringToFlagEnum<AdventureArea>(row.area, '|');
            tempItem.SetSpawnInfo(row.spawn_rate, row.min_spawn_lvl, row.max_spawn_lvl, parsedAreas);

            if(string.IsNullOrEmpty(row.explicit_rarity))
                tempItem.DeriveRarity();
            else
            {
                tempItem._rarity = (Rarity)Enum.Parse(typeof(Rarity), row.explicit_rarity);
            }

            char[] separators = new char[',']; //must define a separator array so that we can pass in the stringsplit option param
            tempItem.attachedSkillIds = row.attached_skill_ids.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string curId;
            for(int j=0; j<tempItem.attachedSkillIds.Length; j++)
            {
                curId = tempItem.attachedSkillIds[j];
                if (!SkillId.IsValid(curId))
                    throw new Exception(curId + " is not a valid skill id to attach to an item");
            }

            int numSlots = row.slots.Length;
            tempItem.slotPositions = new Vector2[numSlots];
            string[] coords;
            for (int j = 0; j < numSlots; j++)
            {
                coords = row.slots[j].Split(',');
                tempItem.slotPositions[j] = new Vector2(float.Parse(coords[0]), float.Parse(coords[1]));
            }

            tempItem.flags = Utils.ParseStringToFlagEnum<ItemFlags>(row.flags, ',');

            if (row.explicit_value == 0)
                tempItem.CalculateValue();
            else tempItem.value = row.explicit_value;
           

            items.Add(tempItem.id, tempItem);
        }

        //record the number of un-leveled items
        numBasicItemsInGame = items.Count;

        //create leveled items
        itemLevelBoosts = new Stats();
        itemLevelBoosts.Set(Stat.hp, 3);
        itemLevelBoosts.Set(Stat.mp, 4);
        itemLevelBoosts.Set(Stat.atk, 2);
        itemLevelBoosts.Set(Stat.crit_chance, 1);
        itemLevelBoosts.Set(Stat.crit_bonus, 1);
        itemLevelBoosts.Set(Stat.dmg_reduction, 1);
        itemLevelBoosts.Set(Stat.evade, 1);
        itemLevelBoosts.Set(Stat.gold_find, 3);
        itemLevelBoosts.Set(Stat.item_find, 2);
        itemLevelBoosts.Set(Stat.magic_boost, 2);
        itemLevelBoosts.Set(Stat.phys_boost, 2);
        itemLevelBoosts.Set(Stat.hp_boost, 1);
        itemLevelBoosts.Set(Stat.mp_boost, 1);
        itemLevelBoosts.Set(Stat.atk_boost, 1);
        itemLevelBoosts.Set(Stat.dmg_reflection, 2);
        itemLevelBoosts.Set(Stat.mastery_xp_boost, 2);

        itemTypesThatLevelUp = ItemType.WEAPON | ItemType.ARMOR | ItemType.SHIELD;
        List<Item> existingItems = items.Values.ToList();
        Item newItem;
        int spawnRange = 100;

        for (int i = 0; i < existingItems.Count; i++)
        {
            tempItem = existingItems[i];

            if(tempItem.IsTypeThatLevelsUp())
            {
                //TODO manually exclude some stuff from leveling up
                if (tempItem.id == ItemId.APHOTIC_BLADE || tempItem.id == ItemId.MOP || tempItem.id == ItemId.IRON_SKILLET)
                    continue;

                //force all items to have a max spawn level of min + 100
                tempItem._spawnMaxLevel = tempItem.spawnMinLevel + spawnRange;

                //right now just levels 2 to the "max"
                for(int j=2; j<=GameContext.MAX_ITEM_LEVEL_WITHOUT_ENHANCING; j++)
                {
                    newItem = tempItem.GetCopy();
                    newItem.BoostItemToLevel(j);
                    items.Add(newItem.id, newItem);

                    //once item is level 5 or higher it can ALWAYS spawn, otherwise limit it
                    if (j > 4)
                        newItem._spawnMaxLevel = 0;
                    else
                        newItem._spawnMaxLevel = newItem.spawnMinLevel + spawnRange;

                }
            }
        }

        //copy ALL items into a parallel list for convenience
        itemCollectionList = items.Values.ToList();

        //create specialized lists
        foodItemIds = new List<string>() { ItemId.APPLE, ItemId.BACON, ItemId.CARROT, ItemId.CARAMEL, ItemId.EGGPLANT, ItemId.MUSHROOM, ItemId.RADISH };



        //build this dict to make status effect prevention checks much faster
        statusEffectToPreventionFlagMap = new Dictionary<StatusEffectType, ItemFlags>();
        statusEffectToPreventionFlagMap[StatusEffectType.BURN] = ItemFlags.PREVENTS_BURN;
        statusEffectToPreventionFlagMap[StatusEffectType.STONE] = ItemFlags.PREVENTS_STONE;
        statusEffectToPreventionFlagMap[StatusEffectType.SPEED_DOWN] = ItemFlags.PREVENTS_SPEED_DOWN;
        statusEffectToPreventionFlagMap[StatusEffectType.SLEEP] = ItemFlags.PREVENTS_SLEEP;
    }

    public static List<Item> GetItemsForArea(AdventureArea inAdventureArea)
    {
        //SIMPLIFIED: since we only create a few items for testing purposes, return all of them instead of trying to only find ones for a specified area
        return itemCollectionList.ToList();

        //validItems = itemCollectionList.Where(e => e.CanSpawnInArea(inAdventureArea));
        //return validItems.ToList();
    }

    public static List<Item> GetItemsOfType(ItemType inType, bool inIncludeLeveledItems=true)
    {
        if(inIncludeLeveledItems)
            validItems = itemCollectionList.Where(e => e.IsType(inType));
        else
            validItems = itemCollectionList.Where(e => e.IsType(inType) && e.level == 1);
        return validItems.ToList();
    }

    public static int GetMaxHeldQuantityForType(ItemType inType)
    {
        if (inType == ItemType.RESOURCE) return GameContext.MAX_RESOURCE_QUANTITY;
        return GameContext.MAX_EQUIPMENT_QUANTITY;
    }

    private static void AddItemTo(List<Item> inList, string inItemid, int inQuantity)
    {
        Item item = GetItem(inItemid).GetCopy();
        item.quantity = inQuantity;
        inList.Add(item);
    }

    public static string GetRandomFoodItemId()
    {
        return foodItemIds.GetRandomElement();
    }

    public static bool IsFoodItemId(string inId)
    {
        return foodItemIds.Contains(inId);
    }

    public static Item GetBestItemForStatAndType(string inStat, ItemType inType, List<Item> availableItems)
    {
        validItems = availableItems.Where(i => i.itemType == inType).Where(i => i.GetAvailableQuantity() > 0);

        if (validItems.Count() == 0) return null;
        return validItems.OrderByDescending(i => i.stats.Get(inStat)).ThenByDescending(i => i.numSlots).First();
    }

    public static Item GetBestItemByValueForType(ItemType inType, List<Item> availableItems)
    {
        validItems = availableItems.Where(i => i.itemType == inType).Where(i => i.GetAvailableQuantity() > 0);

        if (validItems.Count() == 0) return null;
        return validItems.OrderByDescending(i => i.value).First();
    }

    public static Item GetBestItemOfBaseId(string inBaseId, List<Item> availableItems)
    {
        validItems = availableItems.Where(i => i.baseId == inBaseId).Where(i => i.GetAvailableQuantity() > 0);

        if (validItems.Count() == 0) return null;
        return validItems.OrderByDescending(i => i.level).First();
    }

    public static Item GetExactItem(string inId, List<Item> availableItems)
    {
        for(int i=0; i<availableItems.Count; i++)
        {
            if (availableItems[i].id == inId)
            {
                if (availableItems[i].GetAvailableQuantity() > 0)
                    return availableItems[i];
                else
                    return null;
            }
        }
        return null;
    }

    public static int GetTotalNumItemsInGame()
    {
        return numBasicItemsInGame;
    }


















    //serialized
    private string _id;
    public int quantity = 1;

    //non-serialized
    [NonSerialized]public string baseId;
    [NonSerialized]public int storageId;
    [NonSerialized]private ItemType _itemType;
    [NonSerialized]protected Stats _baseStats;
    [NonSerialized]protected Stats _stats; //this exists for our child classes to use and implement. we never use it here because basestats will always = stats
    [NonSerialized]protected int _level;
    [NonSerialized]private int _spawnRate;
    [NonSerialized]private int _spawnMinLevel;
    [NonSerialized]private int _spawnMaxLevel;
    [NonSerialized]private AdventureArea _spawnAreas;
    [NonSerialized]private Rarity _rarity; 
    [NonSerialized]public string[] attachedSkillIds;
    [NonSerialized]public Vector2[] slotPositions;
    [NonSerialized]protected ItemFlags flags;
    [NonSerialized]private long value;

    [NonSerialized]public int equipCount;
    
    //public properties
    public string id { get { return _id; } }
    public virtual string name {
        get
        {
            string fullName = "";
            fullName += "Item #" + baseId;
            if (_level > 1) fullName += " lv" + _level;
            return fullName;
        }
    }

    public string baseName
    {
        get
        {
            return Locale.Get("I" + baseId + "_NAME");
        }
    }

    public string description {  get { return Locale.Get("I" + baseId + "_DESC"); } }
    public ItemType itemType { get { return _itemType; } }
    public Stats baseStats { get { return _baseStats; } }
    public virtual Stats stats { get { return _baseStats; } } //stats just directly returns base stats because the stats for a regular item cannot change. only enchanted items.
    public int level { get { return _level; } }
    public int spawnRate { get { return _spawnRate; } }
    public int spawnMinLevel { get { return _spawnMinLevel; } }
    public int spawnMaxLevel { get { return _spawnMaxLevel; } }
    public AdventureArea spawnAreas { get { return _spawnAreas; } }
    public Rarity rarity { get { return _rarity; } }
    public bool hasMaxSpawnLevel { get { return spawnMaxLevel > spawnMinLevel && spawnMaxLevel != 0; } }
    public int numSlots { get { return slotPositions.Length; } }

    public Item()
    { }

    public virtual void Init()
    {
        _baseStats = new Stats();
        equipCount = 0;
        _level = 1;
    }

    public void SetId(string inId)
    {
        _id = inId;
    }

    public void DeriveBaseAndStorageId()
    {
        if (_id == null)
            throw new Exception("Cannot derive base/storage id when original id is null");

        //this function should only be called on the creation of new, level 1 items.
        //derived or enchanted items may have different ids but the original storage/base id will get copied as-is
        //string[] pieces = _id.Split('_');
        baseId = _id; //pieces[0];
        storageId = int.Parse(baseId);
    }

    public void SetType(ItemType inType)
    {
        _itemType = inType;
    }

    public bool IsType(ItemType inType)
    {
        return (_itemType & inType) == _itemType;
    }

    public bool IsTypeThatLevelsUp()
    {
        return IsType(itemTypesThatLevelUp);
    }

    public bool IsAtMaxLevel()
    {
        return level == GameContext.MAX_ITEM_LEVEL_WITHOUT_ENHANCING;
    }

    public void SetSpawnInfo(int inSpawnRate, int inSpawnMinLvl, int inSpawnMaxLvl, AdventureArea inAreas)
    {
        _spawnRate = inSpawnRate;
        _spawnMinLevel = inSpawnMinLvl;
        _spawnMaxLevel = inSpawnMaxLvl;
        _spawnAreas = inAreas;
    }

    public void DeriveRarity()
    {
        _rarity = Rarity.COMMON;
        if (_spawnRate > 5000) _rarity = Rarity.MYTHIC;
        else if (_spawnRate > 500) _rarity = Rarity.LEGENDARY;
        else if (_spawnRate > 50) _rarity = Rarity.RARE;
        else if (_spawnRate > 20) _rarity = Rarity.UNCOMMON;
        else if(_spawnRate == 0)
        {
            //spawn rate = 0 (no spawn) means the rarity must be at LEAST uncommon
            _rarity = Rarity.UNCOMMON;
        }
    }

    public void BoostItemToLevel(int inLevel)
    {
        SetId(baseId + "_" + inLevel);
        _level = inLevel;

        //update spawn area based on existing rarity
        int levelBoostModifier = 100;
		if (rarity == Rarity.RARE || rarity == Rarity.LEGENDARY || rarity == Rarity.MYTHIC) levelBoostModifier = 200;
		_spawnMinLevel += (_level - 1) * levelBoostModifier;

        //higher level items become more rare
        if (_spawnRate != 0 && _level > 2)
            _spawnRate = _spawnRate * Utils.CeilToInt(_level * .5f);

        //update stats
        baseStats.BoostBy(itemLevelBoosts, _level - 1);

        //after stats are boosted, re-calc value
        CalculateValue();
    }

    public void CalculateValue()
    {
        //calc value. (increase value for num slots?)
        //also increase value for crit stats? (high crit weapons are very desirable)
        //also increase value for base item level? (technically already happens because these items have higher stats, meaning higher value)
        value = 10 + stats.GetCumulativeValue();
        float rarityBoost = 1.5f * ((int)_rarity);
        if(rarityBoost > 1)
            value *= (long)rarityBoost;
    }

    public int GetAvailableQuantity()
    {
        return quantity - equipCount;
    }

    public void AddQuantity(int inAmt)
    {
        quantity = Utils.AddWithOverflowCheck(quantity, inAmt);
        int maxQuantity = Item.GetMaxHeldQuantityForType(itemType);
        if (quantity > maxQuantity) quantity = maxQuantity;
    }

    public bool IsAtMaxQuantity()
    {
        return quantity == Item.GetMaxHeldQuantityForType(itemType);
    }

    public int GetNumUntilMaxQuantity()
    {
        return Item.GetMaxHeldQuantityForType(itemType) - quantity;
    }

    public void RemoveQuantity(int inAmt)
    {
        quantity -= inAmt;
        if (quantity < 0) quantity = 0;
    }

    public bool DealsBluntDamage()
    {
        return stats.Get(Stat.crit_chance) == 0;
    }

    public string GetNonbasicStats()
    {
        string result = "";

        //start with crit
        bool excludeCritFromNormalReadout = false;
        if(HasCrit() && IsType(ItemType.WEAPON))
        {
            excludeCritFromNormalReadout = true;
            long critChance = stats.Get(Stat.crit_chance);
            long critBonus = stats.Get(Stat.crit_bonus);
            result += Locale.Get("CRIT_FULL_DESC");
        }

        //add non-basic stats
        result += stats.ToStringFormatted(true, false, excludeCritFromNormalReadout);

        return result;
    }

    public string GetRarityDescription()
    {
        return (Locale.Get("RARITY_" + rarity.ToString() ) + " " + Locale.Get("ITEM_TYPE_" +itemType.ToString() ));
    }

    public bool HasAttachedSkills()
    {
        return attachedSkillIds.Length > 0;
    }

    public bool AllowsMultibuy()
    {
        return id == ItemId.BLUE_CRYSTAL;
    }

    public bool HasAGemSlot()
    {
        return numSlots > 0;
    }

    public bool HasCrit()
    {
        return stats.Get(Stat.crit_chance) > 0 && stats.Get(Stat.crit_bonus) > 0;
    }

    public bool CanSpawnInArea(AdventureArea inArea)
    {
        return (spawnRate > 0) && (spawnAreas & inArea) == inArea;
    }

    public bool PreventsStatusEffect(StatusEffectType inType)
    {
        ItemFlags checkFlag;
        bool success = statusEffectToPreventionFlagMap.TryGetValue(inType, out checkFlag);
        if (success) return FlagIsSet(statusEffectToPreventionFlagMap[inType]);
        else return false;
    }

    public void SetFlags(ItemFlags inFlags){
        flags |= inFlags;
    }

    public void UnsetFlags(ItemFlags inFlags){
        flags &= ~inFlags;
    }

    public bool FlagIsSet(ItemFlags inFlag){
        return (flags & inFlag) == inFlag;
    }

    public virtual bool IsPlated()
    {
        return false;
    }

    public Item GetCopy()
    {
        Item copy = new Item();
        copy.Init();
        copy.SetId(id);
        CopyPrototypeValuesToInstance(copy);
        return copy;
    }

    private void CopyPrototypeValuesToInstance(Item inInstance)
    {
        inInstance.baseId = baseId;
        inInstance.storageId = storageId;

        inInstance.SetType(_itemType);
        inInstance.SetSpawnInfo(_spawnRate, _spawnMinLevel, _spawnMaxLevel, _spawnAreas);
        inInstance.baseStats.CopyFrom(_baseStats);
        inInstance._rarity = _rarity;
        inInstance._level = _level;
        inInstance.flags = flags;
        inInstance.value = value;

        inInstance.attachedSkillIds = new string[attachedSkillIds.Length];
        for (int i = 0; i < attachedSkillIds.Length; i++)
            inInstance.attachedSkillIds[i] = attachedSkillIds[i];

        inInstance.slotPositions = new Vector2[slotPositions.Length];
        for (int i = 0; i < slotPositions.Length; i++)
            inInstance.slotPositions[i] = slotPositions[i];
    }

    protected virtual void ExpandFromPrototype()
    {
        Init();
        string prototypeId = _id;

        //level 11+ are enhanced items with a unique id, their prototype values are equal to a level 10 version of itself 
        bool isEnhancedItem = ItemId.IsEnhancedItemId(_id);
        if(isEnhancedItem) prototypeId = _id.Split('_')[0] + "_10";

        Item prototype = GetItem(prototypeId);
        prototype.CopyPrototypeValuesToInstance(this);
    }

    //ON SERIALIZE/DESERIALIZE
    [OnDeserialized]
    private void OnDeserialized(StreamingContext sc) //called immediately after everything is deserialized from disk
    {
        //after the item is deserialized, we have to fill back in the base values that we don't store
        ExpandFromPrototype();
    }

    //INNER STRUCTS TO USE FOR JSON MAPPING
    [System.Serializable]
    public struct ItemRow
    {
        public string id;
        public string type;

        public string[] stats;

        public int spawn_rate;
        public int min_spawn_lvl;
        public int max_spawn_lvl;
        public string area;

        public string attached_skill_ids;
        public string season;
        public string[] slots;
        public string flags;
        public string explicit_rarity;
        public int explicit_value;
    }

    [System.Serializable]
    public struct ItemMap
    {
        public ItemRow[] items;
    }
}
