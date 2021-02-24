using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public abstract class CharacterData
{
    //static
    protected static Dictionary<string, CharacterData> characterCollection;





    //serialized
    protected string _id;
    protected string _skinColor;

    //non-serialized
    [NonSerialized]protected Stats _baseStats;
    [NonSerialized]protected Stats _stats;
    [NonSerialized]protected List<Skill> _skills;
    [NonSerialized]protected List<Skill> _nonStaticSkills;
    [NonSerialized]protected Species _species;
    [NonSerialized]protected Faction _baseFaction;
    [NonSerialized]protected Faction _curFaction;
    [NonSerialized]protected List<StatusEffect> _statusEffects;
    [NonSerialized]protected List<CooldownData> cooldowns;
    [NonSerialized]protected bool _dead;
    [NonSerialized]protected bool _tempered;

    [NonSerialized]public int direction;
    [NonSerialized]public bool isDefending;
    [NonSerialized]public int orderInTeam;
    [NonSerialized]public int originalOrderInTeam; //store original order separately for soda script comparisons. normal "orderInTeam" updates as other team members die
    [NonSerialized]public int teamsId;
    [NonSerialized]public Vector2 spawnPosition;
    [NonSerialized]public bool hasBeenStolenFrom;
    [NonSerialized]public int darkSaviorUseCount;
    [NonSerialized]public SodaScript activeSodaScript;
    [NonSerialized]protected CharacterFlags flags;
    [NonSerialized]protected Skill[] baseSkills;
    [NonSerialized]private CharacterTeam team;
    [NonSerialized]private long speedUnitsUntilReadyForTurn;
    [NonSerialized]private int exhaustionTurnCount;
    [NonSerialized]private long heldEssence;
    [NonSerialized]public int variableTargetPreference = 1; //if the char has a variable target skill, it will attempt to target this many opponents
    [NonSerialized]public bool isArenaNpc;
    [NonSerialized]public bool isDinnerBoy;
    [NonSerialized]public string mostRecentSourceOfDamage;
    [NonSerialized]public string mostRecentSkillIdToCauseDamage;
    [NonSerialized]public bool mostRecentSourceOfDamageWasStatusEffect;
    [NonSerialized]public bool mostRecentSourceOfDamageWasBackAttack;

    [NonSerialized] protected long primaryWeaponDamage;
    [NonSerialized] protected long secondaryWeaponDamage;
    [NonSerialized] protected long primaryWeaponCritChance;
    [NonSerialized] protected long secondaryWeaponCritChance;
    [NonSerialized] protected long primaryWeaponCritBonus;
    [NonSerialized] protected long secondaryWeaponCritBonus;
    [NonSerialized] protected int autoCritsRemaining; //a skill like sharpen might force us to autocrit the next X attacks

    
    //public properties
    public string id { get { return _id; } }
    public string name {
        get {
            return id;
        }
    }
    public Stats baseStats { get { return _baseStats; } }
    public Stats stats {get {return _stats;}}
    public List<Skill> skills { get { return _skills; } }
    public List<Skill> nonStaticSkills { get { return _nonStaticSkills; } }
    public Species species { get { return _species; } }
    public Faction baseFaction { get { return _baseFaction; } }
    public Faction curFaction { get { return _curFaction; } }
    public List<StatusEffect> statusEffects { get { return _statusEffects; } }
    public bool dead { get { return _dead; } }
    public bool tempered { get { return _tempered; } }
    public bool isOre { get { return FlagIsSet(CharacterFlags.MINABLE); } }
    public bool hasEssence { get { return heldEssence > 0; } }
    public long curHp { get { return _stats.Get(Stat.hp); } }
    public long curMp { get { return _stats.Get(Stat.mp); } }
    
    public static void InitCharacterCollection()
    {
        characterCollection = new Dictionary<string, CharacterData>();
    }

    public static CharacterData Get(string id)
    {
        return characterCollection[id];
    }

    public static CharacterData GetCopy(string id)
    {
        return Get(id).GetCopy();
    }








    //we use this in place of a constructor because of serialization
    public virtual void Init()
    {
        _stats = new Stats();
        _baseStats = new Stats();
    }

    public void SetId(string inId)
    {
        _id = inId;
    }

    public void SetSpecies(Species inSpecies)
    {
        _species = inSpecies;
    }

    public void SetBaseFaction(Faction inFaction)
    {
        _baseFaction = inFaction;
    }

    public void SetFlags(CharacterFlags inFlags){
        flags |= inFlags;
    }

    public void UnsetFlags(CharacterFlags inFlags){
        flags &= ~inFlags;
    }

    public bool FlagIsSet(CharacterFlags inFlag){
        return (flags & inFlag) == inFlag;
    }

    public void SetBaseSkills(params string[] inSkillIds)
    {
        string trimmedId;
        baseSkills = new Skill[inSkillIds.Length];
        for(int i=0; i<inSkillIds.Length; i++)
        {
            trimmedId = inSkillIds[i].Trim();
            if (!SkillId.IsValid(trimmedId))
                throw new Exception(String.Format("Could not parse skill id {0} for character {1}", inSkillIds[i], id));

            baseSkills[i] = Skill.GetSkill(trimmedId);
        }
    }

    public abstract CharacterData GetCopy();
    protected abstract void ExpandFromPrototype();

    public virtual void CopyPrototypeValuesToInstance(CharacterData inInstance)
    {
        inInstance.SetSpecies(species);
        inInstance.SetBaseFaction(baseFaction);
        inInstance.baseStats.CopyFrom(baseStats);
        inInstance.SetFlags(flags);

        if(baseSkills != null)
            inInstance.baseSkills = (Skill[])baseSkills.Clone();
    }

    /****************************************************************************************************************************
    BATTLE/ ADVENTURE
    ****************************************************************************************************************************/

    public abstract void CalculateModifiedStats();

    //we may want to eventually make this abstract, forcing players/enemies to implement a more robust skill build. but for now:
    public virtual void RebuildSkillList()
    {
        if (_skills == null) _skills = new List<Skill>();
        else _skills.Clear();

        if(!FlagIsSet(CharacterFlags.CANT_STRIKE))
            _skills.Add(Skill.GetSkill(SkillId.STRIKE));

        if(!FlagIsSet(CharacterFlags.CANT_DEFEND))
            _skills.Add(Skill.GetSkill(SkillId.DEFEND));

        if(baseSkills != null)
            _skills.AddRange(baseSkills);

        //copy non-static skills into a separate array
        if (_nonStaticSkills == null) _nonStaticSkills = new List<Skill>();
        else _nonStaticSkills.Clear();

        for(int i=0; i<_skills.Count; i++)
        {
            if (!_skills[i].IsType(SkillType.STATIC))
                _nonStaticSkills.Add(_skills[i]);
        }

    }

    public void PrepareForAdventure(CharacterTeam inTeam)
    {
        team = inTeam;
        UpdateCurFaction(team.faction);
        RebuildSkillList();
        CalculateModifiedStats();
        
        _statusEffects = new List<StatusEffect>();
        stats.Set(Stat.max_hp, stats.Get(Stat.hp));
        stats.Set(Stat.max_mp, stats.Get(Stat.mp));

        cooldowns = new List<CooldownData>();

        _dead = false;
        isDefending = false;
        exhaustionTurnCount = 0;
    }

    public void BoostStartingHp(long inAmt)
    {
        stats.Augment(Stat.hp, inAmt);
        stats.Set(Stat.max_hp, stats.Get(Stat.hp));
    }

    public void BoostStartingAtk(long inAmt)
    {
        stats.Augment(Stat.atk, inAmt);
    }

    protected void ResetStatsToBase()
    {
        stats.Reset();
        stats.CopyFrom(baseStats);
    }

    public void MakeTempered()
    {
        _tempered = true;

        //boost hpx2, atk x1.2, speed+50
        stats.Augment(Stat.spd, 50);
        stats.Set(Stat.hp, stats.Get(Stat.hp) * 2);
        stats.Set(Stat.atk,  (long)(stats.Get(Stat.atk) * 1.2f) );
    }

    public void GiveEssence(long inAmt)
    {
        heldEssence = inAmt;
    }

    public void FullHeal()
    {
        stats.Set(Stat.hp, stats.Get(Stat.max_hp));
        stats.Set(Stat.mp, stats.Get(Stat.max_mp));
    }

    public void Cure()
    {
        //negative status effects are expired but not removed yet, we need to wait for battle manager to do this so it can properly remove and broadcast events
        for(int i=0; i<statusEffects.Count; i++)
        {
            if (statusEffects[i].IsNegative())
            {
                statusEffects[i].ForceExpire();
            }
        }
    }

    public void UpdateCurFaction(Faction inFaction)
    {
        _curFaction = inFaction;
    }

    public void SetDirection(int inDirection)
    {
        direction = inDirection;
    }

    public void FlipDirection()
    {
        SetDirection(-direction);
    }

    public bool BackIsTurnedTo(CharacterData inOtherCharacter)
    {
        return ((spawnPosition.x < inOtherCharacter.spawnPosition.x && direction == Direction.LEFT) ||
                 (spawnPosition.x > inOtherCharacter.spawnPosition.x && direction == Direction.RIGHT));
    }

    public bool IsFacing(CharacterData inOtherCharacter)
    {
        return !BackIsTurnedTo(inOtherCharacter);
    }

    public void PrepareForBattle()
    {
        darkSaviorUseCount = 0;
        ResetSpeedUnits();
    }

    public void TickSpeedUnits()
    {
        speedUnitsUntilReadyForTurn -= stats.Get(Stat.spd);
    }

    public void ResetSpeedUnits()
    {
        speedUnitsUntilReadyForTurn = GameContext.SPEED_UNITS_REQUIRED_FOR_TURN;
    }

    public bool IsReadyForTurn()
    {
        return speedUnitsUntilReadyForTurn <= 0;
    }

    public bool IsAbleToTakeTurn()
    {
        if (FlagIsSet(CharacterFlags.IMMOBILE)) return false;
        if (IsExhausted()) return false;
        return !HasImmobileStatusEffect();
    }

    public void SetExhaustedTurns(int inTurns)
    {
        exhaustionTurnCount = inTurns;
    }

    public void ProcExhaustion()
    {
        exhaustionTurnCount--;
    }

    public bool IsExhausted()
    {
        return exhaustionTurnCount > 0;
    }

    public void ProcEffectsThatOnlyOccurOnOwnTurn()
    {
        if (IsExhausted())
            ProcExhaustion();

        StatusEffect sf;
        for(int i=0; i<_statusEffects.Count; i++)
        {
            sf = _statusEffects[i];
            if(sf.onlyProcsOnOwnTurn)
            {
                sf.ProcTarget(this);
            }
        }
    }

    public bool IsAbleToUseDarkSavior()
    {
        return HasSkill(SkillId.DARK_SAVIOR) && darkSaviorUseCount == 0;
    }

    public void IncrementDarkSaviorUseCount()
    {
        darkSaviorUseCount++;
    }

    public void SetAutoCrits(int inCrits)
    {
        autoCritsRemaining = inCrits;
    }

    public bool HasAutoCritRemaining()
    {
        return autoCritsRemaining > 0;
    }

    public void ApplyStatusEffect(StatusEffect inEffect)
    {
        bool addAsNew = true;

        if(inEffect.CombinesWithOwnType())
        {
            StatusEffect curEffect = GetStatusEffect(inEffect.type);
            if (curEffect != null)
            {
                addAsNew = false;
                curEffect.CombineWith(inEffect);
            }
        }

        if(addAsNew)
        {
            _statusEffects.Add(inEffect);
            inEffect.ApplyToTarget(this);
        }

        if (HasImmobileStatusEffect())
            ClearCooldowns();
    }

    public void RemoveExpiredStatusEffects()
    {
        statusEffects.RemoveAll(e => e.hasExpired);
    }

    public StatusEffect GetStatusEffect(StatusEffectType inType)
    {
        for (int i = 0; i < statusEffects.Count; i++)
            if (statusEffects[i].type == inType) return statusEffects[i];

        return null;
    }

    public bool HasStatusEffect(StatusEffectType inType)
    {
        if (statusEffects == null) return false;
        for (int i = 0; i < statusEffects.Count; i++)
            if (statusEffects[i].type == inType) return true;
        return false;
    }

    public bool HasAnyStatusEffect()
    {
        return _statusEffects.Count > 0;
    }

    public bool HasPostiveStatusEffect()
    {
        if (autoCritsRemaining > 0) return true;

        for (int i = 0; i < _statusEffects.Count; i++)
            if (_statusEffects[i].IsPositive())
                return true;
        return false;
    }

    public bool HasNegativeStatusEffect()
    {
        for (int i = 0; i < _statusEffects.Count; i++)
            if (_statusEffects[i].IsNegative())
                return true;
        return false;
    }

    public bool HasImmobileStatusEffect()
    {
        return ( HasStatusEffect(StatusEffectType.SLEEP) || HasStatusEffect(StatusEffectType.STONE) || HasStatusEffect(StatusEffectType.STUNNED) );
    }

    public bool HasSkill(string inSkillId)
    {
        for (int i = 0; i < _skills.Count; i++)
            if (_skills[i].id == inSkillId)
                return true;

        return false;
    }

    public void SpendMp(int inAmt)
    {
        stats.Augment(Stat.mp, -inAmt);
    }

    //basically anything except strike and defend
    public List<Skill> GetNonbasicSkills()
    {
        List<Skill> tempList = new List<Skill>();
        for (int i = 0; i < _skills.Count; i++)
        {
            if (_skills[i].id != SkillId.STRIKE && _skills[i].id != SkillId.DEFEND)
                tempList.Add(_skills[i]);
        }
        return tempList;
    }

    public void AddCooldown(string inSkillId, int inNumTurns)
    {
        //check existing cooldowns, but this should never find one (two cooldowns for the same skill means the skill never should have been used again)
        for(int i=0; i<cooldowns.Count; i++)
        {
            if(cooldowns[i].skillId == inSkillId)
            {
                cooldowns[i].turns += inNumTurns;
                return;
            }
        }

        //existing cooldown not found
        cooldowns.Add(new CooldownData(inSkillId, inNumTurns));
    }

    public void ProcCooldowns()
    {
        for(int i=cooldowns.Count-1; i>=0; i--)
        {
            cooldowns[i].turns--;
            if (cooldowns[i].turns <= 0)
                cooldowns.RemoveAt(i);
        }
    }

    public void ClearCooldowns()
    {
        cooldowns.Clear();
    }

    public bool CanUseSkill(string inId)
    {
        for (int i = 0; i < _skills.Count; i++)
        {
            if (_skills[i].id == inId)
            {
                //check cooldowns
                for (int j = 0; j < cooldowns.Count; j++)
                    if (cooldowns[j].skillId == inId)
                        return false;

                //special requirement for stun
                if(inId == SkillId.STUN)
                {
                    if (this is PlayerCharacterData)
                    {
                        return ((PlayerCharacterData)this).GetNumWeaponsEquipped() > 0;
                    }
                    else return false;
                }

                //lastly just check mp
                long mp = stats.Get(Stat.mp);
                return (mp >= _skills[i].mpCost);
            }
        }

        return false;
    }

    public Skill GetRandomUsableSkill()
    {
        int numTries = 0;
        Skill targetSkill;
        while(numTries < 50)
        {
            targetSkill = _nonStaticSkills.GetRandomElement();
            if (CanUseSkill(targetSkill.id) && targetSkill.id != SkillId.DEFEND)
                return targetSkill;
        }

        //no skill found, default to our default
        return GetDefaultSkill();
    }

    //THIS WILL NOT CHECK IF THE SKILL CAN BE USED, ONLY THAT IT CONSUMES MP. CAN RETURN NULL.
    public Skill GetAnyNonFriendlySkillThatUsesMP()
    {
        //if we only have one skill it is the default one and does not consume mp, so exit without even trying
        if (skills.Count == 1)
            return null;

        int numTries = 0;
        Skill tempSkill;
        while(numTries < 50)
        {
            tempSkill = _nonStaticSkills.GetRandomElement();
            if (tempSkill.mpCost > 0 && !tempSkill.isFriendly)
                return tempSkill;

            numTries++;
        }

        return null;
    }

    public Skill GetAnyNonFriendlySkillThatDoesntUseMP()
    {
        int numTries = 0;
        Skill tempSkill;
        while(numTries < 50)
        {
            tempSkill = _nonStaticSkills.GetRandomElement();
            if (tempSkill.mpCost == 0 && !tempSkill.isFriendly && tempSkill.id != SkillId.DEFEND)
                return tempSkill;

            numTries++;
        }

        return null;
    }

    public Skill GetAnySkillThatBuffs()
    {
        Skill tempSkill = null;
        for (int i = 0; i < _nonStaticSkills.Count; i++)
        {
            tempSkill = _nonStaticSkills[i];
            if (tempSkill.IncludesStatusBuff())
                return tempSkill;
        }
        return null;
    }

    public Skill GetAnySkillThatSelfBuffs()
    {
        Skill tempSkill = null;
        for (int i = 0; i < _nonStaticSkills.Count; i++)
        {
            tempSkill = _nonStaticSkills[i];
            if (tempSkill.allowsSelfTarget && tempSkill.IncludesStatusBuff())
                return tempSkill;
        }
        return null;
    }

    public Skill GetAnySkillThatDebuffs()
    {
        Skill tempSkill = null;
        for (int i = 0; i < _nonStaticSkills.Count; i++)
        {
            tempSkill = _nonStaticSkills[i];
            if (tempSkill.IncludesStatusDebuff())
                return tempSkill;
        }
        return null;
    }

    public Skill GetStrongestUsableHealSkill(bool inMustTargetGroup=false, bool inMustAllowSelfTarget=false)
    {
        //right now just find the best heal skill based on how much mp it consumes
        int highestCost = int.MinValue;
        Skill tempSkill = null;
        Skill strongestHealSkill = null;
        bool skillIsOk = false;
        for(int i=0; i<_nonStaticSkills.Count; i++)
        {
            tempSkill = _nonStaticSkills[i];
            if(tempSkill.isHealing)
            {
                skillIsOk = true;

                if (inMustTargetGroup && !tempSkill.IsTargetType(SkillTarget.ALL))
                    skillIsOk = false;

                if (inMustAllowSelfTarget && !tempSkill.allowsSelfTarget)
                    skillIsOk = false;

                if(skillIsOk && tempSkill.mpCost > highestCost && CanUseSkill(tempSkill.id))
                {
                    highestCost = tempSkill.mpCost;
                    strongestHealSkill = tempSkill;
                }
            }
        }

        return strongestHealSkill;
    }

    public Skill GetStrongestUsableDamageSkill(bool inMustTargetGroup=false, bool inMustTargetSingle=false)
    {
        //right now just find the best damage skill based on how much mp it consumes
        int highestCost = int.MinValue;
        Skill tempSkill = null;
        Skill strongestDamageSkill = null;
        bool skillIsOk = false;
        for(int i=0; i<_nonStaticSkills.Count; i++)
        {
            tempSkill = _nonStaticSkills[i];

            //skip any skill whose primary purpose is not damage
            if (tempSkill.primaryEffectIsNotDamage)
                continue;

            if(!tempSkill.isFriendly)
            {
                skillIsOk = true;

                if (inMustTargetGroup && !tempSkill.IsTargetType(SkillTarget.ALL))
                    skillIsOk = false;

                if (inMustTargetSingle && !tempSkill.IsTargetType(SkillTarget.SINGLE))
                    skillIsOk = false;

                if(skillIsOk && CanUseSkill(tempSkill.id))
                {
                    //force some skills to be used immediately (might not be based on mp)
                    if (tempSkill.id == SkillId.DUAL_STRIKE)
                        return tempSkill;

                    if(tempSkill.mpCost > highestCost)
                    {
                        highestCost = tempSkill.mpCost;
                        strongestDamageSkill = tempSkill;
                    }
                }
            }
        }

        return strongestDamageSkill;
    }

    public Skill GetDefaultSkill()
    {
        return _nonStaticSkills[0];
    }

    public int GetSideOfScreen()
    {
        if (spawnPosition.x < 0) return Direction.LEFT;
        else return Direction.RIGHT;
    }

    public long GetEssence()
    {
        return heldEssence;
    }

    public CharacterTeam GetCurTeam()
    {
        return team;
    }

    public void Activate()
    {
        isDefending = false;
    }

    public SkillCommand PopulateAISkillCommand(SkillCommand inCommand, CharacterTeam opposingTeam)
    {
        //right now always choose a random target
        CharacterData target = opposingTeam.GetRandomLivingMember();

        Skill skillToUse = GetDefaultSkill();
        inCommand.Reset(skillToUse, this);
        inCommand.AddTarget(target);
        return inCommand;
    }

    public bool PassesStatusResistCheck(StatusEffectType inType)
    {
        //can never be turned to stone if asleep
        if (inType == StatusEffectType.STONE && HasStatusEffect(StatusEffectType.SLEEP))
            return true;

        //ore can't receive any status
        if (isOre) return true;

        //"marked" can't be resisted
        if (inType == StatusEffectType.MARKED)
            return false;

        //roll a check based on our status resist stat
        long resistChance = stats.Get(Stat.status_resist);

        return Utils.PercentageChance((int)resistChance);
    }

    public virtual bool IsImmuneToBackAttackBonus()
    {
        //character data doesn't include items, enemy/playercharacterdata must override this
        return false;
    }

    public virtual bool HasItemToPreventStatusEffect(StatusEffectType inType)
    {
        //character data doesn't include items, enemy/playercharacterdata must override this
        return false;
    }

    public bool PassesEvasionCheck()
    {
        long evadeChance = stats.Get(Stat.evade);
        if (HasImmobileStatusEffect() || evadeChance == 0)
            return false;

        return Utils.PercentageChance((int)evadeChance);
    }

    public void Deactivate()
    {

    }

    public void Die()
    {
        _dead = true;
    }

    public bool IsInFaction(Faction inFaction)
    {
        return curFaction == inFaction;
    }

    public int GetHpPercent()
    {
        double max = stats.Get(Stat.max_hp);
        double cur = stats.Get(Stat.hp);
        return (int)(cur/max * 100);
    }

    public int GetMpPercent()
    {
        double max = stats.Get(Stat.max_mp);
        double cur = stats.Get(Stat.mp);
        if (max == 0) return 100;
        return (int)(cur/max * 100);
    }

    public bool RollPrimaryCritChance()
    {
        return RollCritChance(true);
    }

    public bool RollSecondaryCritChance()
    {
        return RollCritChance(false);
    }

    private bool RollCritChance(bool inForPrimaryWeapon)
    {
        bool canCrit = true;

        if (inForPrimaryWeapon && primaryWeaponCritChance == 0)
            canCrit = false;

        if (!inForPrimaryWeapon && secondaryWeaponCritChance == 0)
            canCrit = false;

        long critChance = stats.Get(Stat.crit_chance) - (inForPrimaryWeapon ? secondaryWeaponCritChance : primaryWeaponCritChance);

        if(autoCritsRemaining > 0)
        {
            autoCritsRemaining--;
            critChance = 100;
        }

        if (!canCrit)
            return false;

        return Utils.RandomInRange(0, 100) <= critChance;
    }

    public float GetPrimaryCritBonusAppliedToDamage(float inDamage)
    {
        return GetCritBonusAppliedToDamage(inDamage, true);
    }

    public float GetSecondaryCritBonusAppliedToDamage(float inDamage)
    {
        return GetCritBonusAppliedToDamage(inDamage, false);
    }

    public long GetPrimaryWeaponDamage()
    {
        return primaryWeaponDamage;
    }

    public long GetSecondaryWeaponDamage()
    {
        return secondaryWeaponDamage;
    }

    private float GetCritBonusAppliedToDamage(float inDamage, bool inForPrimaryWeapon)
    {
        float critBonus = stats.Get(Stat.crit_bonus) - (inForPrimaryWeapon ? secondaryWeaponCritBonus : primaryWeaponCritBonus);
        float critMul = 1 + (critBonus/100);
        return Utils.Floor(inDamage * critMul); //we floor here because damage is expressed as negative
    }

    public void UpdateMostRecentSourceOfDamage(string inSource, string inSkillId, bool inWasStatusProc, bool inWasBackAttack)
    {
        mostRecentSourceOfDamage = inSource;
        mostRecentSkillIdToCauseDamage = inSkillId;
        mostRecentSourceOfDamageWasStatusEffect = inWasStatusProc;
        mostRecentSourceOfDamageWasBackAttack = inWasBackAttack;
    }

    //ON SERIALIZE/DESERIALIZE
    [OnDeserialized]
    protected virtual void OnDeserialized(StreamingContext sc) //called immediately after everything is deserialized from disk
    {
        //after the char data is deserialized, we have to fill back in the base values that we don't store
        ExpandFromPrototype();
    }
}
