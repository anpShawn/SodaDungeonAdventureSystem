using System.Collections.Generic;

public class SkillResult : IPoolable
{
    public Skill skill;
    public CharacterData source;
    public CharacterData target;
    public Stats statChanges;
    public bool skillAttemptedToInflictDamage; //some skills don't attempt at all, some do but get their result set to 0 depending on dmg absorb and defend state
    public List<StatusEffect> statusEffects;
    public string itemIdEarnedOnHit;
    public LootBundle stolenLoot;
    public int turnsExhaustedSourceFor;
    public int autocritsGiven;
    public CharacterData transmuteResult;

    //bonuses/stats that apply to the whole team
    public int teamBonusXpAwarded;
    public int teamNextOreBonusDrops;
    public int teamNextEnemyGoldBoostPercent;
    public int teamNextEnemyEssenceBoostPercent;
    public int teamNextCritDamageBoostPercent;

    public bool wasEvaded;
    public bool resistedAllNegativeStatuses;
    public bool wasCrit;
    public bool wasDoubleCrit;
    public bool wasBackAttack;
    public bool wasHuntressCapture;
    public bool turnedTargetAround;
    public bool transmutesTarget;
    public bool failedATransmute;
    public bool invokedDarkSavior;
    public bool forcesWarpAtNextPaths;

	public SkillResult()
    {
        statusEffects = new List<StatusEffect>();
    }

    public void Init(Skill inSkill, CharacterData inSource, CharacterData inTarget)
    {
        skill = inSkill;
        source = inSource;
        target = inTarget;

        statChanges = PoolManager.poolStats.GetNext();
        skillAttemptedToInflictDamage = false;

        turnsExhaustedSourceFor = 0;
        autocritsGiven = 0;
        transmuteResult = null;
        teamBonusXpAwarded = 0;
        teamNextOreBonusDrops = 0;
        teamNextEnemyGoldBoostPercent = 0;
        teamNextEnemyEssenceBoostPercent = 0;
        teamNextCritDamageBoostPercent = 0;

        wasEvaded = false;
        resistedAllNegativeStatuses = false;
        wasCrit = false;
        wasDoubleCrit = false;
        wasBackAttack = false;
        wasHuntressCapture = false;
        turnedTargetAround = false;
        transmutesTarget = false;
        failedATransmute = false;
        invokedDarkSavior = false;
        forcesWarpAtNextPaths = false;
    }

    public void ApplyStatusEffectsBasedOnStats(Stats inStats)
    {
        long chancePoison = inStats.Get(Stat.chance_to_psn);
        long chanceBurn = inStats.Get(Stat.chance_to_burn);
        long chanceStun = inStats.Get(Stat.chance_to_stun);
        long chanceFood = inStats.Get(Stat.chance_for_food);
        
        if(chancePoison > 0)
        {
            if(Utils.PercentageChance( (int)chancePoison ))
                statusEffects.Add(StatusEffect.CreateNew(StatusEffectType.POISON, 3));
        }

        if(chanceBurn > 0)
        {
            if(Utils.PercentageChance( (int)chanceBurn ))
                statusEffects.Add(StatusEffect.CreateNew(StatusEffectType.BURN, 3));
        }

        if(chanceStun > 0)
        {
            if(Utils.PercentageChance( (int)chanceStun ))
                statusEffects.Add(StatusEffect.CreateNew(StatusEffectType.STUNNED, GameContext.STUN_TURN_DURATION));
        }

        if(chanceFood > 0)
        {
            if (Utils.PercentageChance((int)chanceFood))
                itemIdEarnedOnHit = Item.GetRandomFoodItemId();
        }
    }

    public void SetAsEvaded()
    {
        wasEvaded = true;

        wasCrit = false;
        wasDoubleCrit = false;
        wasBackAttack = false;
        wasHuntressCapture = false;
        turnedTargetAround = false;
        transmutesTarget = false;
        failedATransmute = false;
        invokedDarkSavior = false;
        forcesWarpAtNextPaths = false;
    }

    public void ResolveEffects(Adventure inAdventure)
    {
        if(!wasEvaded)
        {
            //update most recent source of damage to target
            target.UpdateMostRecentSourceOfDamage(source.name, skill.id, false, wasBackAttack);

            target.stats.Add(statChanges);

            StatusEffect curEffect;
            int numStatusNegative = 0;
            int numStatusResisted = 0;

            for(int i=0; i<statusEffects.Count; i++)
            {
                curEffect = statusEffects[i];

                if(curEffect.IsNegative())
                {
                    numStatusNegative++;
                    if(target.HasItemToPreventStatusEffect(curEffect.type) || target.PassesStatusResistCheck(curEffect.type))
                    {
                        numStatusResisted++;
                        curEffect.MarkAsResisted();
                    }
                    else
                         target.ApplyStatusEffect(curEffect);
                }
                else
                    target.ApplyStatusEffect(curEffect);
            }

            int numStatusNegativeReceived = numStatusNegative - numStatusResisted;

            //REMOVED: tracking/recording stats related to status effects

            if (numStatusNegative > 0 && numStatusNegative == numStatusResisted)
                resistedAllNegativeStatuses = true;

            //clear the resisted ones
            statusEffects.RemoveAll(e => e.wasResisted);
                
            target.stats.EnforceNonzeroForKeys(statChanges.GetKeys());
            target.stats.EnforceLimits();

            //turn target around
            if (turnedTargetAround)
                target.FlipDirection();

            //item earned on hit?
            if (itemIdEarnedOnHit != null)
                inAdventure.AddLoot(new Loot(LootType.ITEM, itemIdEarnedOnHit, 1));

            //stolen loot?
            if (stolenLoot != null)
                inAdventure.AddLootBundle(stolenLoot);

            //exhaustion
            if (turnsExhaustedSourceFor > 0)
                source.SetExhaustedTurns(turnsExhaustedSourceFor);

            //autocrits?
            if (autocritsGiven > 0)
                target.SetAutoCrits(autocritsGiven);

            //huntress capture?
            if (!target.isOre && source.id == CharId.HUNTRESS && target.IsInFaction(Faction.ENEMY) && target.species == Species.CREATURE)
            {
                if(target.stats.Get(Stat.hp) == 0)
                {
                    wasHuntressCapture = true;
                    EnemyCharacterData ecd = (EnemyCharacterData)target;
                    //Main.services.saveManager.currentFile.IncrementHuntressCaptures(ecd.storageId);
                }
            } 
        }
    }

    public void ResolveTeamEffects(Adventure inAdventure)
    {
        //player team xp
        if(teamBonusXpAwarded != 0)
        {
            //REMOVED: add mastery xp to player team
        }

        //extra ore drops
        inAdventure.nextOreBonusDrops += teamNextOreBonusDrops;
        if (inAdventure.nextOreBonusDrops > GameContext.COOK_MAX_ORE_GAIN) inAdventure.nextOreBonusDrops = GameContext.COOK_MAX_ORE_GAIN;

        //next enemy gold boost
        inAdventure.nextEnemyGoldBoostPercent += teamNextEnemyGoldBoostPercent;

        //next enemy essence boost
        inAdventure.nextEnemyEssenceBoostPercent += teamNextEnemyEssenceBoostPercent;

        //next crit boost
        inAdventure.nextCritDamageBoostPercent += teamNextCritDamageBoostPercent;

        //forces a warp
        if (forcesWarpAtNextPaths)
            inAdventure.forceWarpAtNextPaths = true;
    }

    public bool WasStealAttempt()
    {
        return skill.id == SkillId.PILFER || skill.id == SkillId.RANSACK;
    }

    /************************************************************************************************************************************************************
     * IPOOLABLE INTERFACE
     * 
     * *********************************************************************************************************************************************************/

    public bool IsActiveInPool()
    {
        return (skill != null);
    }

    public void InitForPool()
    {
    }

    public void ActivateForPool()
    {
    }

    public void DeactivateForPool()
    {
        skill = null;
        source = null;
        target = null;
        statusEffects.Clear();
        itemIdEarnedOnHit = null;
        transmuteResult = null;

        if(stolenLoot != null)
        {
            PoolManager.Recycle(stolenLoot);
            stolenLoot = null;
        }
            
        PoolManager.Recycle(statChanges);
        statChanges = null;
    }
}
