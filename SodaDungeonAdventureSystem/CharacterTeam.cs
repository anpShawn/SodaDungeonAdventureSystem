using System.Collections.Generic;

public class CharacterTeam
{
    public List<CharacterData> members { get; private set; }
    private List<CharacterData> randomMemberList; //use this to speed up the process of grabbing a random member
    public Faction faction;
    public int id;
    public Stats teamBonusStats;
    public int consecutiveHeals; //used by sodascript to help break out of infinite healing loops
    public int numMembersUsingCustomSodaScript;

    public int teamGoldFindBonus { get { return teamBonusStats.GetAsInt(Stat.gold_find); } }
    public int teamItemFindBonus { get { return teamBonusStats.GetAsInt(Stat.item_find); } }
    public int teamEssenceFindBonus { get { return teamBonusStats.GetAsInt(Stat.essence_find); } }
    public int teamKeyBonus { get { return teamBonusStats.GetAsInt(Stat.chance_for_dungeon_keys); } }

	public CharacterTeam(int inId, Faction inFaction)
    {
        id = inId;
        members = new List<CharacterData>();
        randomMemberList = new List<CharacterData>();
        faction = inFaction;
        teamBonusStats = PoolManager.poolStats.GetNext();
    }

    public void RemoveAllMembers()
    {
        members.Clear();
    }

    public void AddMember(CharacterData inMember)
    {
        members.Add(inMember);
        inMember.UpdateCurFaction(faction);
    }

    public void AddPlayerCharactersFromList(List<PlayerCharacterData> inList)
    {
        for (int i = 0; i < inList.Count; i++)
            AddMember(inList[i]);
    }

    public void SyncCharactersToSpawnPattern(SpawnPattern inPattern)
    {
        //spawn pattern must exactly match the size of our team!
        if (inPattern.availableSpawnPoints != members.Count)
            throw new System.Exception("Spawn patterns with " + inPattern.availableSpawnPoints + " points does match team size of " + members.Count);

        SpawnPoint sp;
        for(int i=0; i<inPattern.spawnPoints.Count; i++)
        {
            sp = inPattern.spawnPoints[i];
            members[i].spawnPosition = sp.position;
            members[i].direction = sp.direction;
        }
    }

    public void PrepareForAdventure()
    {
        //prepare each member for battle
        for(int i=0; i<members.Count; i++)
            members[i].PrepareForAdventure(this);
    }

    public void BoostMaxHPByPercent(int inPercent)
    {
        long curMaxHp, newMaxHp;
        float percentAsFloat = (float)inPercent / 100f;
        for (int i = 0; i < members.Count; i++)
        {
            curMaxHp = members[i].stats.Get(Stat.max_hp);
            newMaxHp = curMaxHp + (long)(curMaxHp * percentAsFloat);
            members[i].stats.Set(Stat.max_hp, newMaxHp);
            members[i].stats.Set(Stat.hp, newMaxHp);
        }
    }

    public void CalculateTeamBonusStats()
    {
        teamBonusStats.Reset();

        //members
        for(int i=0; i<members.Count; i++)
        {
            if(!members[i].dead)
            {
                string curKey;
                for(int j=0; j<Stats.wholePartyKeys.Length; j++)
                {
                    curKey = Stats.wholePartyKeys[j];

                    if(members[i].stats.Has(curKey))
                        teamBonusStats.Augment(curKey, members[i].stats.GetAsDouble(curKey));
                }
            }
        }

        //REMOVED: adding team-based pet and relic bonuses
    }

    public void RecalcStatsForEachMember()
    {
        for (int i = 0; i < members.Count; i++)
            members[i].CalculateModifiedStats();
    }

    public void PrepareForBattle()
    {
        //prepare each member for battle
        for (int i = 0; i < members.Count; i++)
        {
            members[i].teamsId = id;
            members[i].orderInTeam = i;
            members[i].originalOrderInTeam = i;
            members[i].PrepareForBattle();
        }
    }

    //if someone on our team dies we need to update everyone's team order to ensure proper turn priority
    public void RefreshTeamOrder()
    {
        int order = 0;
        for (int i = 0; i < members.Count; i++)
        {
            if(!members[i].dead)
            {
                members[i].orderInTeam = order;
                order++;
            }
        }
    }

    //should only be used for a player team
    public void CacheActiveSodaScriptReferences()
    {
        numMembersUsingCustomSodaScript = 0;

        for (int i = 0; i < members.Count; i++)
        {
             members[i].activeSodaScript = SodaScript.GetPremadeScript(SodaScript.SCRIPT_DEFAULT_PLAYER);
        }
    }

    public void HealAndCureAllMembers()
    {
        CharacterData curMember;
        for(int i=0; i<members.Count; i++)
        {
            curMember = members[i];
            if(!curMember.dead)
            {
                curMember.FullHeal();
                curMember.Cure();
            }
        }
    }

    public void KillAllMembers()
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].stats.Set(Stat.hp, 0);
        }
    }

    public void TurnMemberInto(CharacterData inOldMember, CharacterData inNewMember)
    {
        for(int i=0; i<members.Count; i++)
        {
            if(members[i] == inOldMember)
            {
                members[i] = inNewMember;
                inNewMember.PrepareForAdventure(this);

                inNewMember.teamsId = inOldMember.teamsId;
                inNewMember.orderInTeam = inOldMember.orderInTeam;
                inNewMember.originalOrderInTeam = inOldMember.originalOrderInTeam;
                inNewMember.PrepareForBattle();
            }
        }
    }

    public void ChangeFlagForAllMembers(CharacterFlags inFlag, bool inSet)
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (inSet)
                members[i].SetFlags(inFlag);
            else
                members[i].UnsetFlags(inFlag);
        }
    }

    public CharacterData GetWeakestLivingMember()
    {
        CharacterData weakestMember = null;
        int curHpPercent;
        int lowestHpPercent = 999;

        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead)
            {
                curHpPercent = members[i].GetHpPercent();
                if(curHpPercent < lowestHpPercent)
                {
                    weakestMember = members[i];
                    lowestHpPercent = curHpPercent;
                }
            }
        }

        return weakestMember;
    }

    public CharacterData GetRandomLivingMember(CharacterData inExcludedMember = null)
    {
        randomMemberList.Clear();
        for(int i=0; i<members.Count; i++)
        {
            if (inExcludedMember != null && members[i] == inExcludedMember)
                continue;

            if (!members[i].dead)
                randomMemberList.Add(members[i]);
        }
        randomMemberList.Shuffle();

        return randomMemberList[0];
    }

    public CharacterData GetMemberWhoCanUseDarkSaviorThatIsnt(CharacterData inCharacter)
    {
        for (int i = 0; i < members.Count; i++)
            if (!members[i].dead && members[i] != inCharacter && members[i].IsAbleToUseDarkSavior())
                return members[i];

        return null;
    }

    public bool ContainsOre()
    {
        for (int i = 0; i < members.Count; i++)
            if (members[i].isOre) return true;

        return false;
    }

    public bool ContainsEssence()
    {
        for (int i = 0; i < members.Count; i++)
            if (members[i].hasEssence) return true;

        return false;
    }

    public bool AllMembersDefeated()
    {
        return GetNumMembersThatNeedDefeated() == 0;
    }

    public float GetGoldFindAsFloatMultiplier()
    {
        return 1 + teamBonusStats.Get(Stat.gold_find) / 100f;
    }

    public double GetBigGoldMultiplier()
    {
        return 1 + teamBonusStats.GetAsDouble(Stat.gold_multiplier);
    }

    public double GetMasteryXpBoostAsMultiplier()
    {
        return 1 + teamBonusStats.GetAsDouble(Stat.mastery_xp_boost) / 100d;
    }

    public int GetNumLivingMembers()
    {
        int num = 0;
        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead ) num++;
        }
        return num;
    }

    public int GetNumMembersThatNeedDefeated()
    {
        int num = 0;
        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead && !members[i].FlagIsSet(CharacterFlags.DEATH_OPTIONAL) ) num++;
        }
        return num;
    }

    public bool LivingMemberHasItemEquipped(string inId)
    {
        if (faction != Faction.PLAYER)
            throw new System.Exception("This function cannot be called on an enemy team");

        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead)
            {
                PlayerCharacterData pcd = (PlayerCharacterData)members[i];
                if(pcd.ItemOrVariantIsEquipped(inId)) return true;
            }
        }

        return false;
    }

    public bool LivingMemberHasStatusEffect(StatusEffectType inType)
    {
        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead)
            {
                if (members[i].HasStatusEffect(inType))
                    return true;
            }
        }

        return false;
    }

    public bool LivingMemberHasSkill(string inSkillId)
    {
        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead)
            {
                if (members[i].HasSkill(inSkillId))
                    return true;
            }
        }

        return false;
    }

    public bool LivingMemberIsFromClass(string inClassId)
    {
        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead)
            {
                if (members[i].id == inClassId)
                    return true;
            }
        }

        return false;
    }

    public CharacterData GetFirstLivingMemberOfClass(string inClassId)
    {
        for(int i=0; i<members.Count; i++)
        {
            if (!members[i].dead)
            {
                if (members[i].id == inClassId)
                    return members[i];
            }
        }

        return null;
    }

    public bool HasReachedConsecutiveHealLimit()
    {
        return consecutiveHeals >= GetNumLivingMembers() * 2;
    }

    public int GetTeamHPPercent()
    {
        double totalCur = 0;
        double totalMax = 0;
        for(int i=0; i<members.Count; i++)
        {
            if(!members[i].dead) //use this to filter by dead/not dead. 
            {
                totalCur += members[i].stats.Get(Stat.hp);
                totalMax += members[i].stats.Get(Stat.max_hp);
            }
        }

        if (totalMax == 0) return 0; //if total max is 0 it means all members are dead
        return (int)(totalCur/totalMax * 100);
    }

    public int GetTeamMPPercent()
    {
        double totalCur = 0;
        double totalMax = 0;
        for(int i=0; i<members.Count; i++)
        {
            if(!members[i].dead) //use this to filter by dead/not dead. 
            {
                totalCur += members[i].stats.Get(Stat.mp);
                totalMax += members[i].stats.Get(Stat.max_mp);
            }
        }

        if (totalMax == 0) return 100; //if total max is 0 it means nobody has any mp at all, so they are "full"
        return (int)(totalCur/totalMax * 100);
    }

    public int GetNumMembersWithBackTurnedTo(CharacterData inTarget)
    {
        int num = 0;

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].BackIsTurnedTo(inTarget))
                num++;
        }

        return num;
    }

    public void Cleanup()
    {
        RemoveAllMembers();
        members = null;
        PoolManager.Recycle(teamBonusStats);
    }
}
