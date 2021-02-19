using System.Collections;
using System.Collections.Generic;

public class StatusEffect
{
    private static Dictionary<string, bool> effectsAreNegative;

    public static StatusEffect CreateNew(StatusEffectType inType, int inTurnDuration)
    {
        return new StatusEffect(inType, inTurnDuration);
    }

    public static void CreateAncillaryTypeData()
    {
        effectsAreNegative = new Dictionary<string, bool>();
        effectsAreNegative[StatusEffectType.STONE.ToString()] = true;
        effectsAreNegative[StatusEffectType.BURN.ToString()] = true;
        effectsAreNegative[StatusEffectType.POISON.ToString()] = true;
        effectsAreNegative[StatusEffectType.SLEEP.ToString()] = true;
        effectsAreNegative[StatusEffectType.CONFUSION.ToString()] = true;
        effectsAreNegative[StatusEffectType.SLEEP.ToString()] = true;
        effectsAreNegative[StatusEffectType.ATK_DOWN.ToString()] = true;
        effectsAreNegative[StatusEffectType.SPEED_DOWN.ToString()] = true;
        effectsAreNegative[StatusEffectType.MARKED.ToString()] = true;
        effectsAreNegative[StatusEffectType.STUNNED.ToString()] = true;

        effectsAreNegative[StatusEffectType.ATK_UP.ToString()] = false;
        effectsAreNegative[StatusEffectType.SPEED_UP.ToString()] = false;
        effectsAreNegative[StatusEffectType.EVADE_UP.ToString()] = false;
        effectsAreNegative[StatusEffectType.MP_REGEN.ToString()] = false;
    }


    public static bool StatusEffectTypeIsNegative(StatusEffectType inType)
    {
        string key = inType.ToString();

        if (effectsAreNegative.ContainsKey(key))
            return effectsAreNegative[key];

        throw new System.Exception("Status effect type '" + inType + "' has no definition for it being negative or positive");
    }





    public StatusEffectType type { get; private set; }
    public int turnDuration { get; private set; }
    public int turnsActive { get; private set; }
    public bool onlyProcsOnOwnTurn { get; private set; }
    public bool wasResisted { get; private set; }
    public bool hasExpired { get { return turnDuration == 0; } }
    private string statAffected;
    private long statChangeAmt;

	public StatusEffect(StatusEffectType inType, int inTurnDuration)
    {
        type = inType;
        turnDuration = inTurnDuration;

        onlyProcsOnOwnTurn = (inType == StatusEffectType.MARKED);

        turnsActive = 0;
        statChangeAmt = 0;
    }

    //right now all we do is adopt the new effect's turn duration
    public void CombineWith(StatusEffect inEffect)
    {
        if(inEffect.type == type)
        {
            turnDuration = inEffect.turnDuration;
            turnsActive = 0;
        }
    }

    public void MarkAsResisted()
    {
        wasResisted = true;
    }

    public void ApplyToTarget(CharacterData inTarget)
    {
        if(type == StatusEffectType.SPEED_DOWN)
        {
            statAffected = Stat.spd;
            statChangeAmt = inTarget.stats.GetAllowableAugmentAmountForStat(Stat.spd, -20);
        }
        else if(type == StatusEffectType.EVADE_UP)
        {
            statAffected = Stat.evade;
            statChangeAmt = inTarget.stats.GetAllowableAugmentAmountForStat(Stat.evade, 60); //used to be 45
        }
        else if(type == StatusEffectType.MP_REGEN)
        {
            statAffected = Stat.mp_regen;
            statChangeAmt = 8;
        }

        if (statAffected != null)
            inTarget.stats.Augment(statAffected, statChangeAmt);
    }

    public void ProcTarget(CharacterData inTarget)
    {
        turnDuration--;
        turnsActive++;

        long procDmg = GetProcDamage(inTarget);
        if(procDmg != 0)
        {
            inTarget.stats.Augment(Stat.hp, -procDmg);

            inTarget.UpdateMostRecentSourceOfDamage(Locale.Get(type.ToString()), null, true, false);
        }
    }

    public long GetProcDamage(CharacterData inTarget)
    {
        //debug invincible characters
        if ((GameContext.DEBUG_INVINCIBLE_CHARACTERS && inTarget.curFaction == Faction.PLAYER)
            || (GameContext.DEBUG_INVINCIBLE_ENEMIES && inTarget.curFaction == Faction.ENEMY))
        {
            return 0;
        }

        if(type == StatusEffectType.BURN)
        {
            //burn deals 6% of max hp per turn active
            return Utils.CeilToInt(inTarget.stats.Get(Stat.max_hp) * (.06f * turnsActive));
        }
        else if(type == StatusEffectType.POISON)
        {
            //constant 10% of max hp
            return Utils.CeilToInt(inTarget.stats.Get(Stat.max_hp) * .1f);
        }

        return 0;
    }

    public void ForceExpire()
    {
        turnDuration = 0;
    }

    public void RemoveFromTarget(CharacterData inTarget)
    {
        if(statAffected != null)
        {
            inTarget.stats.Augment(statAffected, -statChangeAmt);
        }
    }

    public bool IsNegative()
    {
        return StatusEffectTypeIsNegative(type);
    }

    public bool IsPositive()
    {
        return !StatusEffectTypeIsNegative(type);
    }

    //certain status effects don't stack, they simply refresh the turn count (burn, poison)
    public bool CombinesWithOwnType()
    {
        return (type == StatusEffectType.BURN || type == StatusEffectType.POISON || type == StatusEffectType.SLEEP || 
                type == StatusEffectType.STONE || type == StatusEffectType.CONFUSION || type == StatusEffectType.MP_REGEN 
                || type == StatusEffectType.MARKED || type == StatusEffectType.STUNNED);
    }
}
