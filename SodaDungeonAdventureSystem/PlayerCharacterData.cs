using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class PlayerCharacterData : CharacterData
{
    //static
    public static Dictionary<string, PlayerCharacterData> playerCollection { get; private set; }
    private static List<string> tempSkillIdList = new List<string>();

    public static void CreatePlayerCharacters()
    {
        playerCollection = new Dictionary<string, PlayerCharacterData>();

        //use this line to load data straight from a json file
        //PlayerCharacterMap map = Utils.LoadJsonFromPath<PlayerCharacterMap>(inPath);
    
        /*
    {"id":"soda_junkie",                   				               "gender":"MALE",      "body":1,    "head":1,    "mouth":1,    "hands":1,    "hair":1,    "outfit":1,    "accessory":0,      "eyes": 1,                 "base_stats" : ["hp=10",     "atk=1",     "mp=5",      "spd=100"],                       "equippable_slots":"WSAI",             "can_hire_in_tavern": "yes",     "can_only_hire_one_per_party":"no",     "hire_cost":0,        "purchase_prereqs" :  ["TAVERN"],                              "skills":""},
    {"id":"tavern_owner",                   			               "gender":"MALE",      "body":1,    "head":1,    "mouth":1,    "hands":1,    "hair":2,    "outfit":2,    "accessory":0,      "eyes": 1,                 "base_stats" : ["hp=10",     "atk=1",     "mp=0",      "spd=100"],                       "equippable_slots":"WSAI",             "can_hire_in_tavern": "no",      "can_only_hire_one_per_party":"no",     "hire_cost":0,        "purchase_prereqs" :  ["SODA_OWNER"],                          "skills":""},
    {"id":"blacksmith",                   			                   "gender":"MALE",      "body":1,    "head":1,    "mouth":1,    "hands":1,    "hair":10,   "outfit":16,   "accessory":0,      "eyes": 1,                 "base_stats" : ["hp=10",     "atk=1",     "mp=0",      "spd=100"],                       "equippable_slots":"WSAI",             "can_hire_in_tavern": "no",      "can_only_hire_one_per_party":"no",     "hire_cost":0,        "purchase_prereqs" :  ["SODA_BLACKSMITH"],                     "skills":""},
    {"id":"wizard",                      			                   "gender":"MALE",      "body":1,    "head":1,    "mouth":1,    "hands":1,    "hair":0,    "outfit":20,   "accessory":40,     "eyes": 100,               "base_stats" : ["hp=25",     "atk=5",     "mp=35",     "spd=100"],                       "equippable_slots":"WSAII",            "can_hire_in_tavern": "no",      "can_only_hire_one_per_party":"yes",    "hire_cost":750,      "purchase_prereqs" :  ["SODA_WIZARD"],                         "skills":"EVISCERATE, PRECOGNITION, MAGIC_MASTER"},
    {"id":"carpenter",                   			                   "gender":"MALE",      "body":1,    "head":1,    "mouth":1,    "hands":1,    "hair":1,    "outfit":3,    "accessory":1,      "eyes": 2,                 "base_stats" : ["hp=13",     "atk=1",     "mp=10",     "spd=100"],                       "equippable_slots":"WSAI",             "can_hire_in_tavern": "yes",     "can_only_hire_one_per_party":"no",     "hire_cost":10,       "purchase_prereqs" :  ["SODA_CARPENTER"],                      "skills":"SWIFT_METAL"},
    {"id":"miner",                   			                       "gender":"MALE",      "body":1,    "head":1,    "mouth":1,    "hands":1,    "hair":1,    "outfit":5,    "accessory":4,      "eyes": 2,                 "base_stats" : ["hp=11",     "atk=2",     "mp=20",     "spd=100"],                       "equippable_slots":"WSAI",             "can_hire_in_tavern": "yes",     "can_only_hire_one_per_party":"no",     "hire_cost":25,       "purchase_prereqs" :  ["SODA_MINER"],                          "skills":"PICKAXE, PROSPECTOR, EXCAVATOR"},
    {"id":"nurse",                   			                       "gender":"FEMALE",    "body":1,    "head":1,    "mouth":2,    "hands":1,    "hair":4,    "outfit":4,    "accessory":2,      "eyes": 1,                 "base_stats" : ["hp=13",     "atk=1",     "mp=30",     "spd=100"],                       "equippable_slots":"WSAI",             "can_hire_in_tavern": "yes",     "can_only_hire_one_per_party":"no",     "hire_cost":40,       "purchase_prereqs" :  ["SODA_NURSE"],                          "skills":"FIRST_AID, BIOHAZARD"},
    */

        //instead, create the data manually
        //the following letters are used to indicate the type of equippable slot:
        /*
         * W = weapon
         * S = shield
         * A = armor
         * I = accessory item
         * */
        var sodaJunkieData = new PlayerCharacterRow { id = CharId.SODA_JUNKIE, gender="MALE",   base_stats = new string[] {"hp=10",     "atk=1",     "mp=5",      "spd=100" }, equippable_slots = "WSAI", skills = "" };
        var carpenterData  = new PlayerCharacterRow { id = CharId.CARPENTER,   gender="MALE",   base_stats = new string[] {"hp=13",     "atk=1",     "mp=10",     "spd=100" }, equippable_slots = "WSAI", skills = SkillId.SWIFT_METAL };
        var nurseData      = new PlayerCharacterRow { id = CharId.NURSE,       gender="FEMALE", base_stats = new string[] {"hp=13",     "atk=1",     "mp=30",     "spd=100" }, equippable_slots = "WSAI", skills = SkillId.FIRST_AID +"," + SkillId.BIOHAZARD };

        PlayerCharacterMap map = new PlayerCharacterMap();
        map.player_characters = new PlayerCharacterRow[] { sodaJunkieData, carpenterData, nurseData };

        PlayerCharacterRow row;
        PlayerCharacterData tempCharacter;

        Gender parsedGender;
        bool parsedCanHire;
        bool parsedCanOnlyHireOnePerParty;

        for (int i = 0; i < map.player_characters.Length; i++)
        {
            row = map.player_characters[i];
            tempCharacter = new PlayerCharacterData();
            tempCharacter.Init();

            tempCharacter.SetId(row.id);
            tempCharacter.SetSpecies(Species.HUMAN);
            tempCharacter.SetBaseFaction(Faction.PLAYER);

            Stats.ParseArrayIntoStatObject(row.base_stats, tempCharacter.baseStats);

            parsedGender = (Gender)Enum.Parse(typeof(Gender), row.gender);
            parsedCanHire = (row.can_hire_in_tavern == "yes");
            parsedCanOnlyHireOnePerParty = (row.can_only_hire_one_per_party == "yes");

            tempCharacter.SetBaseAppearance(parsedGender, row.body, row.head, row.mouth, row.hands, row.hair, row.outfit, row.accessory, row.eyes);
            tempCharacter.SetHireInfo(parsedCanHire, row.hire_cost, parsedCanOnlyHireOnePerParty);

            //parse out equip slots
            ParseStringIntoEquipSlotsForCharacter(row.equippable_slots, tempCharacter);

            //purchase prereqs
            tempCharacter.purchasePrerequisites = row.purchase_prereqs;

            //skills
            if(row.skills.Length > 0)
                tempCharacter.SetBaseSkills(row.skills.Split(','));

            //set defend flags for certain classes
            if (tempCharacter.id == CharId.SODA_JUNKIE || tempCharacter.id == CharId.DUAL_WIELD)
                tempCharacter.SetFlags(CharacterFlags.CANT_DEFEND);

            playerCollection.Add(tempCharacter.id, tempCharacter);
            characterCollection.Add(tempCharacter.id, tempCharacter);
        }
    }









    //serialized
    protected List<EquipmentSlot> _equipmentSlots;

    [OptionalField(VersionAdded = 4)]
    protected List<string> _chefMenuChoices;

    //non-serialized
    [NonSerialized]public Gender gender;
    [NonSerialized]public int body;
    [NonSerialized]public int head;
    [NonSerialized]public int mouth;
    [NonSerialized]public int hands;
    [NonSerialized]public int hair;
    [NonSerialized]public int outfit;
    [NonSerialized]public int accessory;
    [NonSerialized]public int eyes;
    [NonSerialized]public ulong hireCost;
    [NonSerialized]public bool canBeHiredInTavern;
    [NonSerialized]public bool canOnlyHireOnePerParty;
    [NonSerialized]public float tavernSpawnChance; //set by outside classes for temp spawn calculations
    [NonSerialized]public int numSpawned; //set by outside classes for temp spawn calculations
    [NonSerialized]private string[] purchasePrerequisites;

    //public properties
    public List<EquipmentSlot> equipmentSlots { get{return _equipmentSlots;}}
    public List<string> chefMenuChoices  { get { return _chefMenuChoices; } set { _chefMenuChoices = value; } }

    public PlayerCharacterData() { }

    public override void Init()
    {
        base.Init();
        if(equipmentSlots == null )_equipmentSlots = new List<EquipmentSlot>();
    }

    private static void ParseStringIntoEquipSlotsForCharacter(string inSlotStr, PlayerCharacterData inData)
    {
        char[] slotChars = inSlotStr.ToUpper().ToCharArray();
        char curChar;

        ItemType slotType;
        int slotId;

        int numWeapons, numShields, numArmors, numItems;
        numWeapons = numShields = numArmors = numItems = 0;

        for (int i = 0; i < slotChars.Length; i++)
        {
            curChar = slotChars[i];

            if (curChar == 'W')
            {
                numWeapons++;
                slotType = ItemType.WEAPON;
                slotId = numWeapons;
            }
            else if (curChar == 'S')
            {
                numShields++;
                slotType = ItemType.SHIELD;
                slotId = numShields;
            }
            else if (curChar == 'A')
            {
                numArmors++;
                slotType = ItemType.ARMOR;
                slotId = numArmors;
            }
            else if (curChar == 'I')
            {
                numItems++;
                slotType = ItemType.ACCESSORY;
                slotId = numItems;
            }
            else throw new Exception("Tried to parse an equippable slot type that doesn't exist: " + curChar);

            inData.equipmentSlots.Add(new EquipmentSlot(slotType, slotId));
        }
    }

    public void SetBaseAppearance(Gender inGender, int inBody, int inHead, int inMouth, int inHands, int inHair, int inOutfit, int inAccessory, int inEyes)
    {
        gender = inGender;
        body = inBody;
        head = inHead;
        mouth = inMouth;
        hands = inHands;
        hair = inHair;
        outfit = inOutfit;
        accessory = inAccessory;
        eyes = inEyes;
    }

    public void SetHireInfo(bool inCanBeHiredInTavern, ulong inCost, bool inCanOnlyHireOnePerParty)
    {
        canBeHiredInTavern = inCanBeHiredInTavern;
        hireCost = inCost;
        canOnlyHireOnePerParty = inCanOnlyHireOnePerParty;
    }

    public void EquipSlot(string inSlotId, string inEquipmentId)
    {
        EquipmentSlot targetSlot = GetEquipmentSlot(inSlotId);
        if (targetSlot == null){
            throw new Exception("Cannot equip: " + name + " does not have slot '" + inSlotId + "'");
        }
        else targetSlot.SetEquipment(inEquipmentId);
    }

    public void UnequipSlot(string inSlotId)
    {
        EquipmentSlot targetSlot = GetEquipmentSlot(inSlotId);
        if (targetSlot == null){
            throw new Exception("Cannot equip: " + name + " does not have slot '" + inSlotId + "'");
        }
        else targetSlot.RemoveEquipment();
    }

    public string GetEquipmentIdInSlot(string inSlotId)
    {
        EquipmentSlot targetSlot = GetEquipmentSlot(inSlotId);
        if (targetSlot == null){
            throw new Exception("Cannot get equipment id: " + name + " does not have slot '" + inSlotId + "'");
        }
        else return targetSlot.equipmentId;
    }

    public EquipmentSlot GetSlotWithItemEquipped(string inId)
    {
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            if (equipmentSlots[i].equipmentId == inId)
                return equipmentSlots[i];
        }

        return null;
    }

    public override void RebuildSkillList()
    {
        base.RebuildSkillList();

        //add any skills that are attached to our items
        Skill skill;
        EquipmentSlot tempSlot;
        Item tempEquipment;
        Item tempGem;

        //this is a static list that gets re-used by ALL character data
        tempSkillIdList.Clear();

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            tempSlot = equipmentSlots[i];
            if (!tempSlot.IsEmpty())
            {
                tempEquipment = GetItem(tempSlot.equipmentId);
                tempSkillIdList.AddRange(tempEquipment.attachedSkillIds);

                //add skills from any gems
                if(tempSlot.HasGemSlots())
                {
                    string gemId;
                    for(int j=0; j<tempSlot.gemIds.Length; j++)
                    {
                        gemId = tempSlot.gemIds[j];
                        if(gemId != null)
                        {
                            tempGem = GetItem(gemId);
                            tempSkillIdList.AddRange(tempGem.attachedSkillIds);
                        }
                    }
                }

                for(int j=0; j<tempSkillIdList.Count; j++)
                {
                    skill = Skill.GetSkill(tempSkillIdList[j]);

                    //prevent dupe skills from equipping the same item twice
                    if (_skills.Contains(skill))
                        continue;

                    _skills.Add(skill);

                    if (!skill.IsType(SkillType.STATIC))
                        _nonStaticSkills.Add(skill);
                }
            }
        }

        //add any skills that come from mastery levels. make sure the mastery exists at all first (we may be examining a character the player doesn't own yet)
        //REMOVED: adding any skills derived from character mastery levels

        //sort the list so that static skills always come after regular skills
        _skills.Sort(SortSkill);
    }

    private int SortSkill(Skill inSkill1, Skill inSkill2)
    {
        if (inSkill1.IsType(SkillType.STATIC) && inSkill2.IsType(SkillType.STATIC))
            return 0;
        else if (inSkill1.IsType(SkillType.STATIC))
            return 1;
        else
            return -1;
    }

    public override void CalculateModifiedStats()
    {
        ResetStatsToBase();

        //add equipment stats to our own
        Item tempEquipment;
        Item tempGem;
        EquipmentSlot tempSlot;

        int numWeaponsExamined = 0;

        for (int i=0; i<equipmentSlots.Count; i++)
        {
            tempSlot = equipmentSlots[i];
            if(!tempSlot.IsEmpty())
            {
                tempEquipment = GetItem(tempSlot.equipmentId);
                stats.Add(tempEquipment.stats);

                //also add any gems that this slot contains
                if(tempSlot.HasGemSlots())
                {
                    string gemId;
                    for(int j=0; j<tempSlot.gemIds.Length; j++)
                    {
                        gemId = tempSlot.gemIds[j];
                        if(gemId != null)
                        {
                            tempGem = GetItem(gemId);
                            stats.Add(tempGem.stats);
                        }
                    }
                }

                //if a weapon, store their crit stats separately for other use
                if(tempSlot.itemType == ItemType.WEAPON)
                {
                    if(numWeaponsExamined == 0)
                    {
                        primaryWeaponDamage = tempEquipment.stats.Get(Stat.atk);
                        primaryWeaponCritChance = tempEquipment.stats.Get(Stat.crit_chance);
                        primaryWeaponCritBonus = tempEquipment.stats.Get(Stat.crit_bonus);
                    }
                    else
                    {
                        secondaryWeaponDamage = tempEquipment.stats.Get(Stat.atk);
                        secondaryWeaponCritChance = tempEquipment.stats.Get(Stat.crit_chance);
                        secondaryWeaponCritBonus = tempEquipment.stats.Get(Stat.crit_bonus);
                    }

                    numWeaponsExamined++;
                }
            }    
        }

        //add any static skill stats
        Skill tempSkill;
        for(int i=0; i<_skills.Count; i++)
        {
            tempSkill = _skills[i];
            if (tempSkill.IsType(SkillType.STATIC) && tempSkill.stats != null)
                stats.Add(tempSkill.stats);
        }

        //include relics+pet+mastery if in the player faction
        if(curFaction == Faction.PLAYER)
        {
            //REMOVED: stat bonuses that come from relics, pets, and mastery levels
        }

        //now apply any stats that %-boost other stats
        if(stats.Has(Stat.atk_boost))
        {
            stats.BoostStatByPercent(Stat.atk, stats.Get(Stat.atk_boost));
        }
        if(stats.Has(Stat.mp_boost))
        {
            stats.BoostStatByPercent(Stat.mp, stats.Get(Stat.mp_boost));
        }
        if(stats.Has(Stat.hp_boost))
        {
            stats.BoostStatByPercent(Stat.hp, stats.Get(Stat.hp_boost));
        }

        //if magic inept, cancel any MP
        if (HasSkill(SkillId.MAGIC_INEPT))
            stats.Set(Stat.mp, 0);

        stats.EnforceLimits();
    }

    public bool HasSlottedItemEquipped()
    {
        Item item;
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            if (!equipmentSlots[i].IsEmpty())
            {
                item = GetItem(equipmentSlots[i].equipmentId);
                if (item.HasAGemSlot())
                    return true;
            }
        }

        return false;
    }

    public override bool IsImmuneToBackAttackBonus()
    {
        Item item;
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            if (!equipmentSlots[i].IsEmpty())
            {
                item = GetItem(equipmentSlots[i].equipmentId);
                if (item.FlagIsSet(ItemFlags.PREVENTS_BACK_ATK_BONUS))
                    return true;
            }
        }

        return false;
    }

    public override bool HasItemToPreventStatusEffect(StatusEffectType inType)
    {
        Item item;
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            if (!equipmentSlots[i].IsEmpty())
            {
                item = GetItem(equipmentSlots[i].equipmentId);
                if (item.PreventsStatusEffect(inType))
                    return true;
            }
        }

        return false;
    }

    //we have a local getItem function because we might need to defer to different lists depending on our faction
    public Item GetItem(string inId)
    {
        //SIMPLIFIED: in the real game we would pull item data from the save file in case it had custom enhancements; instead just always pull from the main item list since enhanced (plated) items are disabled
        /*
        if (curFaction == Faction.PLAYER)
            return Main.services.saveManager.currentFile.GetItem(inId);
        else
            return Item.GetItem(inId);
        */

        return Item.GetItem(inId);
    }

    public EquipmentSlot GetEquipmentSlot(string inId)
    {
        for(int i=0; i<equipmentSlots.Count; i++)
        {
            if (equipmentSlots[i].id == inId) return equipmentSlots[i];
        }
        return null;
    }

    public int NumItemOrVariantEquipped(string inId)
    {
        int num = 0;
        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            if (!equipmentSlots[i].IsEmpty())
            {
                if (equipmentSlots[i].equipmentId == inId)
                    num++;
                else
                {
                    //the ids don't exactly match, so now just see if current equipment slot's BASE ID matches
                    if (ItemId.ExtractBaseId(equipmentSlots[i].equipmentId) == inId)
                        num++;
                }
            }
        }

        return num;
    }

    public bool ItemOrVariantIsEquipped(string inId)
    {
        return NumItemOrVariantEquipped(inId) > 0;
    }

    public bool SlotIsEquipped(string inSlotName)
    {
        EquipmentSlot slot = GetEquipmentSlot(inSlotName);
        return slot != null && !slot.IsEmpty();
    }

    public bool CanEquipTwoAccessories()
    {
        int numCanEquip = 0;

        for (int i = 0; i < equipmentSlots.Count; i++)
            if (equipmentSlots[i].itemType == ItemType.ACCESSORY)
                numCanEquip++;

        return numCanEquip > 1;
    }

    public int GetNumWeaponsEquipped()
    {
        int numEquipped = 0;
        for (int i = 0; i < equipmentSlots.Count; i++)
            if (equipmentSlots[i].itemType == ItemType.WEAPON && !equipmentSlots[i].IsEmpty())
                numEquipped++;

        return numEquipped;
    }

    public string GetGenderString()
    {
        return gender.ToString().ToLower();
    }

    public override CharacterData GetCopy()
    {
        PlayerCharacterData copy = new PlayerCharacterData();
        copy.Init();
        copy.SetId(id);
        copy._skinColor = _skinColor; //it would be more proper to use SetSkinColor() but that requires changing from Color back to hex.
        copy.isDinnerBoy = isDinnerBoy; //this value belongs to the base class but it's more fitting to only copy it here

        CopyPrototypeValuesToInstance(copy);

        for (int i = 0; i < equipmentSlots.Count; i++){
            copy.equipmentSlots.Add(new EquipmentSlot(equipmentSlots[i].itemType, equipmentSlots[i].slotNumber));
        }

        return copy;
    }

    public override void CopyPrototypeValuesToInstance(CharacterData inInstance)
    {
        base.CopyPrototypeValuesToInstance(inInstance);

        PlayerCharacterData pcd = (PlayerCharacterData)inInstance;
        pcd.SetBaseAppearance(gender, body, head, mouth, hands, hair, outfit, accessory, eyes);
        pcd.SetHireInfo(canBeHiredInTavern, hireCost, canOnlyHireOnePerParty);
    }

    protected override void ExpandFromPrototype()
    {
        Init();
        PlayerCharacterData prototype = (PlayerCharacterData)Get(id);
        prototype.CopyPrototypeValuesToInstance(this);
    }

    public void PrintEquipmentSlots()
    {
        for(int i=0; i<equipmentSlots.Count; i++)
        {
            Console.WriteLine("slot " + equipmentSlots[i].itemType + " contains item id " + equipmentSlots[i].equipmentId);
        }
    }

    //INNER STRUCTS TO USE FOR JSON MAPPING
    [System.Serializable]
    public struct PlayerCharacterRow
    {
        public string id;

        public string gender;
        public int body;
        public int head;
        public int mouth;
        public int hands;
        public int hair;
        public int outfit;
        public int accessory;
        public int eyes;

        public string[] base_stats;
        public string equippable_slots;
        public string can_hire_in_tavern;
        public string can_only_hire_one_per_party;
        public ulong hire_cost;
        public string[] purchase_prereqs;
        public string skills;
    }

    [System.Serializable]
    public struct PlayerCharacterMap
    {
        public PlayerCharacterRow[] player_characters;
    }
}
