using System.Collections.Generic;
using System.Reflection;
using System;
using System.Text;
using System.Linq;

public class Stats : IPoolable
{
    private static Dictionary<string, long> defaultStats;
    private static List<string> nonbasicKeys; //use this to print stats that aren't hp, mp, atk, etc
    private static List<string> percentageBasedKeys; //stats that boost by a percent factor instead of raw numbers
    public static string[] wholePartyKeys;

    protected Dictionary<string, double> internalStats;
    protected bool isActive;

    public static void BuildDefaultStats()
    {
        //use reflection to build a list of default stats based on what the Stat type contains
        defaultStats = new Dictionary<string, long>();

        Type typeStat = typeof(Stat);
        Type typeString = typeof(string);
        FieldInfo[] statFields = typeStat.GetFields();
        FieldInfo curFieldInfo;
        string curKey;

        nonbasicKeys = new List<string>();

        for(int i = 0; i< statFields.Length; i++)
        {
            curFieldInfo = statFields[i];

            if(curFieldInfo.FieldType == typeString)
            {
                curKey = curFieldInfo.GetValue(null).ToString();
                defaultStats.Add(curKey, 0);

                //also build nonbasic stat keys while here
                if (curKey != Stat.hp && curKey != Stat.mp && curKey != Stat.atk && curKey != Stat.max_hp && curKey != Stat.max_mp)
                    nonbasicKeys.Add(curKey);
            }
        }

        percentageBasedKeys = new List<string>() { Stat.crit_bonus, Stat.crit_chance, Stat.magic_boost, Stat.gold_find, Stat.essence_find, Stat.chance_for_dungeon_keys, Stat.evade,
                                                    Stat.dmg_reduction, Stat.dmg_reflection, Stat.status_resist, Stat.chance_to_psn, Stat.chance_to_burn, Stat.chance_to_stun, Stat.chance_for_food, Stat.mastery_xp_boost,
                                                    Stat.atk_boost, Stat.mp_boost, Stat.hp_boost, Stat.phys_boost};

        wholePartyKeys = new string[] { Stat.gold_find, Stat.item_find, Stat.essence_find, Stat.chance_for_dungeon_keys, Stat.mastery_xp_boost, Stat.ore_find, Stat.gold_multiplier };
    }

    public static bool IsPercentageBasedStat(string inkey)
    {
        if (percentageBasedKeys == null) throw new Exception("Tried to query percentage based stats before created");
        return percentageBasedKeys.Contains(inkey);
    }

    public static bool IsStatKey(string inKey)
    {
        if(defaultStats == null) throw new Exception("Tried to query default stats before created");
        return defaultStats.ContainsKey(inKey);
    }

    public static bool IsWholePartyStat(string inKey)
    {
        if (wholePartyKeys == null) throw new Exception("Tried to query whole team stats before created");
        return wholePartyKeys.Contains(inKey);
    }

    public static void ParseArrayIntoStatObject(string[] inArray, Stats inStats)
    {
        string[] baseStatSplitter;
        string tempKey;
        string tempStrValue;
        int tempValue;

        for (int j = 0; j < inArray.Length; j++)
        {
            baseStatSplitter = inArray[j].Split('=');
            tempKey = baseStatSplitter[0];
            tempStrValue = baseStatSplitter[1];

            if (tempKey == Stat.spd && tempStrValue == "default")
                tempValue = 95;
            else
            {
                int.TryParse(tempStrValue, out tempValue);
            }

            if (Stats.IsStatKey(tempKey))
            {
                inStats.Set(tempKey, tempValue);
            }
            else
            {
                throw new Exception("Tried to parse non-existent stat: " + tempKey);
            }
        }
    }

    public static string PrintSingleStatToString(string inStat, double inVal)
    {
        string statName = Locale.Get("STAT_" + inStat.ToUpper());
        string format = "";
            
        if (inVal % 1 != 0)
            format = "0.0#";

        string strStatVal = inVal.ToString(format);
        string spacer = IsPercentageBasedStat(inStat) ? "% " : " ";
        string sign = inVal > 0 ? "+" : ""; //the negative sign is printed automatically when a negative number is turned into a string

        //overrides
        if(inStat == Stat.chance_to_burn || inStat == Stat.chance_to_psn || inStat == Stat.chance_to_stun || inStat == Stat.chance_for_food)
        {
            spacer = "";
            strStatVal = "";
        }

        return (sign + strStatVal + spacer + statName);
    }

    public static Stats SubtractStatsAFromStatsB(Stats inStatsA, Stats inStatsB)
    {
        Stats diffStats = new Stats();

        long val1, val2;
        foreach (string key in inStatsB.GetKeys())
        {
            if(inStatsA.Has(key))
            {
                val1 = inStatsB.Get(key);
                val2 = inStatsA.Get(key);
                if(val1 > val2)
                {
                    diffStats.Set(key, val1 - val2);
                }
            }
        }

        return diffStats;
    }










	public Stats()
    {
        internalStats = new Dictionary<string, double>();
        isActive = false;
    }

    public void Set(string inKey, long inValue)
    {
        internalStats[inKey] = inValue;
    }

    public void Set(string inKey, double inValue)
    {
        internalStats[inKey] = inValue;
    }

    public long Get(string inKey)
    {
        double val;
        internalStats.TryGetValue(inKey, out val);
        return (long)val;
    }

    public int GetAsInt(string inKey)
    {
        if (Has(inKey))
        {
            if (internalStats[inKey] > int.MaxValue)
                return int.MaxValue;
            else return (int)internalStats[inKey];
        }
        else return 0;
    }

    public double GetAsDouble(string inKey)
    {
        double val;
        internalStats.TryGetValue(inKey, out val);
        return val;
    }

    public void Augment(string inKey, long inAmt)
    {
        Set(inKey, Get(inKey) + inAmt);
    }

    public void Augment(string inKey, double inAmt)
    {
        Set(inKey, GetAsDouble(inKey) + inAmt);
    }

    //this function used by status effects that want to attempt to inflict a buff or debuff of a set amount
    public long GetAllowableAugmentAmountForStat(string inStat, long inAttemptedAugment)
    {
        long curStatAmt = Get(inStat);
        if(inAttemptedAugment < 0)
        {
            long minAllowed = 0;
            if (inStat == Stat.spd) minAllowed = 10;
            if (curStatAmt + inAttemptedAugment < minAllowed)
            {
                return -(curStatAmt - minAllowed);
            }
            else return inAttemptedAugment;
        }
        else
        {
            return inAttemptedAugment;
        }
    }

    public long GetCumulativeValue()
    {
        double amt = 0;
        foreach (string key in internalStats.Keys)
        {
            if (key == Stat.crit_bonus || key == Stat.crit_chance) continue;
            amt += internalStats[key];
        }

        return (long)amt;
    }

    public IEnumerable<string> GetKeys()
    {
        return internalStats.Keys;
    }

    public void CopyFrom(Stats inStats)
    {
        foreach (string key in inStats.GetKeys())
        {
            Set(key,
                inStats.GetAsDouble(key)
                );
        }
    }

    public void Add(Stats inStats)
    {
        foreach (string key in inStats.GetKeys() )
        {
            Set(key,
                GetAsDouble(key) + inStats.GetAsDouble(key)
                );
        }
    }

    public void Subtract(Stats inStats)
    {
        foreach (string key in inStats.GetKeys() )
        {
            Set(key,
                GetAsDouble(key) - inStats.GetAsDouble(key)
                );
        }
    }

    public void SubtractCritStatsFrom(Stats inStats)
    {
        if(Has(Stat.crit_chance) && inStats.Has(Stat.crit_chance))
        {
            Augment(Stat.crit_chance, -inStats.Get(Stat.crit_chance));
        }

        if(Has(Stat.crit_bonus) && inStats.Has(Stat.crit_bonus))
        {
            Augment(Stat.crit_bonus, -inStats.Get(Stat.crit_bonus));
        }
    }

    public bool Has(string inKey)
    {
        return internalStats.ContainsKey(inKey);
    }

    public void BoostBy(Stats inBoosts, int inMultiplier)
    {
        List<string> keys = new List<string> (internalStats.Keys);
        foreach (string key in keys)
        {
            if( inBoosts.internalStats.ContainsKey(key) )
            {
                Augment(key, inBoosts.Get(key) * inMultiplier);
            }
        }
    }

    public void BoostStatByPercent(string inKey, long inPercent)
    {
        double multiplier = 1 + (inPercent / 100d);
        long amt = (long)(Get(inKey) * multiplier);
        if (amt < 0) amt = long.MaxValue;
        Set(inKey, amt);
    }

    public void EnforceNonzeroForKeys(IEnumerable<String> inKeys)
    {
        foreach (string key in inKeys)
        {
            if( Get(key) < 0)
            {
                Set(key, 0);
            }
        }
    }

    public void EnforceLimits()
    {
        double curHp = internalStats[Stat.hp];
        double curMp = internalStats[Stat.mp];

        if(Has(Stat.max_hp))
        {
            if (curHp > internalStats[Stat.max_hp])
                Set(Stat.hp, Get(Stat.max_hp));
            else if (curHp < 0)
                Set(Stat.hp, 0);
        }
        
        if(Has(Stat.max_mp))
        {
            if (curMp > internalStats[Stat.max_mp])
                Set(Stat.mp, Get(Stat.max_mp));
            else if (curMp < 0)
                Set(Stat.mp, 0);
        }
        
        if (Has(Stat.evade) && internalStats[Stat.evade] > 90)
            internalStats[Stat.evade] = 90;

        if (Has(Stat.dmg_reduction) && internalStats[Stat.dmg_reduction] > 90)
            internalStats[Stat.dmg_reduction] = 90;
    }

    public void Reset()
    {
        internalStats.Clear();
    }

    public bool IsEmpty()
    {
        return internalStats.Count == 0;
    }

    public void Print()
    {
        foreach (string key in internalStats.Keys)
            Console.WriteLine(key + " : " + internalStats[key]);
    }

    public string ToStringFormatted(bool onlyIncludeNonbasicStats = false, bool printSpeedBasedOn100 = false, bool excludeCrit=false)
    {
        StringBuilder sb = new StringBuilder();

        foreach(string key in internalStats.Keys)
        {
            if (onlyIncludeNonbasicStats && !nonbasicKeys.Contains(key))
                continue;

            //exclude crit? usually for weapons that do a custom crit readout
            if (excludeCrit && (key == Stat.crit_bonus || key == Stat.crit_chance))
                continue;
                
            double statValue = internalStats[key];

            if(key == Stat.spd && printSpeedBasedOn100)
            {
                statValue -= 100;
                if (statValue == 0)
                    continue;
            }

            sb.Append(PrintSingleStatToString(key, statValue));
            sb.Append(", ");
        }

        if(sb.Length > 0)
        {
            sb.Remove(sb.Length-2, 2);
            sb.Append(".");
        }
        
        return sb.ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        int numPrinted = 0;
        foreach (string key in internalStats.Keys)
        {
            numPrinted++;
            sb.Append(key + ":" + internalStats[key]);
            if (numPrinted != internalStats.Keys.Count) sb.Append(", ");
        }

        return sb.ToString();
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
    }

    public void DeactivateForPool()
    {
        Reset();
        isActive = false;
    }
}
