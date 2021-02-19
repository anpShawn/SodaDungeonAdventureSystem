using System;
using System.Collections.Generic;

public class Skill
{
    public static List<Skill> skills;
    private static List<Action> stepsInAnimation;
    private static int stepIndex;



    public string id;
    public string name { get { return Locale.Get("SKILL_NAME_" + id); } }
    public SkillType type { get; private set; }
    public int mpCost { get; private set; }
    public SkillTarget targetType { get; private set; }
    public bool displaysName { get; private set; }
    public bool canBackAttack { get; private set; }
    public bool primaryEffectIsNotDamage { get; private set; } //their main purpose is not to damage, but inflict a status or change of some kind. curse/transmute/mark/etc. OR stealing
    public bool isFriendly { get; protected set; } //set to protected so that the cook skill can update this value as needed
    public bool isHealing { get; protected set; } //set to protected so that the cook skill can update this value as needed
    public bool allowsSelfTarget { get; private set; }
    public bool allowedForArenaNpcs { get; private set; }
    public StatusEffectCategory statusEffectCategory { get; private set; }
    public Stats stats { get; private set; }
    public int cooldown { get; private set; }

    protected int targetsHit;
    protected float attackMultiplier; //this is only used as an easy way to show the user if this skill can be easily described as multiplying their attack power

    public event Action EAnimationComplete;

    public static void CreateSkills()
    {
        skills = new List<Skill>();
        Skill tempSkill;

        bool DISPLAYS_NAME = true;
        bool DOESNT_DISPLAY_NAME = false;
        bool IS_FRIENDLY = true;
        bool ISNT_FRIENDLY = false;
        bool IS_HEALING = true;
        bool ISNT_HEALING = false;
        bool SELF_TARGETS = true;
        bool DOESNT_SELF_TARGET = false;
        bool ALLOWED_FOR_ARENA_NPCS = true;
        bool BANNED_FOR_ARENA_NPCS = false;


        CreateSkill<Defend>(SkillType.PHYSICAL, 0, SkillTarget.SINGLE, DOESNT_DISPLAY_NAME, IS_FRIENDLY, ISNT_HEALING, SELF_TARGETS);

        tempSkill = CreateSkill<Strike>(SkillType.PHYSICAL, 0, SkillTarget.SINGLE, DOESNT_DISPLAY_NAME);
            tempSkill.MarkAsBackAttackCapable();

        CreateSkill<FirstAid>(SkillType.MAGICAL, 3, SkillTarget.SINGLE, DOESNT_DISPLAY_NAME, IS_FRIENDLY, IS_HEALING);
        tempSkill = CreateSkill<Biohazard>(SkillType.MAGICAL, 3, SkillTarget.SINGLE, DOESNT_DISPLAY_NAME);
            tempSkill.AddStat(Stat.chance_to_psn, 100);
            tempSkill.SetAttackMultiplier(1.2f);
        tempSkill = CreateSkill<SwiftMetal>(SkillType.PHYSICAL, 2, SkillTarget.SINGLE, DOESNT_DISPLAY_NAME);
            tempSkill.SetAttackMultiplier(GameContext.SWIFT_METAL_DEFAULT_MULTIPLIER);

        CreateSkill<Heal>(SkillType.MAGICAL, 1, SkillTarget.SINGLE, DOESNT_DISPLAY_NAME, IS_FRIENDLY, IS_HEALING, SELF_TARGETS);

        CreateSkill<Meteor>(SkillType.PHYSICAL, 3, SkillTarget.SINGLE, DISPLAYS_NAME);

        //create an empty steps list to use later
        stepsInAnimation = new List<Action>();
    }

    private static Skill CreateSkill<T>(SkillType inType, int inMpCost, SkillTarget inTargetType, bool inDisplaysName, bool inIsFriendly=false, bool inIsHealing=false, bool inAllowsSelfTarget = false, StatusEffectCategory inEffectCategory = StatusEffectCategory.NONE, bool inAllowedForArenaNpcs = true)where T:Skill, new()
    {
        Skill s = new T();
        s.SetBaseProperties(inType, inMpCost, inTargetType, inDisplaysName, inIsFriendly, inIsHealing, inAllowsSelfTarget, inEffectCategory, inAllowedForArenaNpcs);
        skills.Add(s);
        return s;
    }

    private static Skill CreateStaticSkill(string inId)
    {
        Skill s = new Skill(inId);
        s.type = SkillType.STATIC;
        skills.Add(s);
        return s;
    }

    public static Skill GetSkill(string inId)
    {
        for(int i = 0; i<skills.Count; i++)
        {
            if (skills[i].id == inId) return skills[i];
        }

        throw new System.Exception("Tried to retrieve non-existant skill '"+inId+"'");
    }

    public static void AugmentAllSkillsBasedOnSaveFile(SaveFile inFile)
    {
        for(int i = 0; i<skills.Count; i++)
        {
            skills[i].AugmentPropertiesBasedOnSaveFile(inFile);
        }
    }
















    public Skill(string inId)
    {
        id = inId;
        mpCost = 0;
    }

    public void SetBaseProperties(SkillType inType, int inMpCost, SkillTarget inTargetType, bool inDisplaysName, bool inIsFriendly, bool inIsHealing, bool inAllowsSelfTarget, StatusEffectCategory inEffectCategory, bool inAllowedForArenaNpcs)
    {
        type = inType;
        mpCost = inMpCost;
        targetType = inTargetType;
        displaysName = inDisplaysName;
        isFriendly = inIsFriendly;
        isHealing = inIsHealing;
        allowsSelfTarget = inAllowsSelfTarget;
        statusEffectCategory = inEffectCategory;
        allowedForArenaNpcs = inAllowedForArenaNpcs;
    }

    private void AddStat(string inStatId, long inValue)
    {
        //if (type != SkillType.STATIC)
            //throw new Exception("Cannot add stats to non-static skills");

        if (stats == null)
            stats = new Stats();

        stats.Set(inStatId, inValue);
    }

    private void SetCooldown(int inCooldown)
    {
        cooldown = inCooldown;
    }

    private void MarkAsBackAttackCapable()
    {
        canBackAttack = true;
    }

    //utility skills are generally not chosen by autocombat due to variety of effects they are involved with
    private void MarkAsPrimaryEffectIsNotDamage()
    {
        primaryEffectIsNotDamage = true;
    }

    private void SetAttackMultiplier(float inMul)
    {
        attackMultiplier = inMul;
    }

    public bool IsType(SkillType inType)
    {
        return (type & inType) == inType;
    }

    public bool IsTargetType(SkillTarget inType)
    {
        return targetType == inType;
    }

    public bool IncludesStatusBuff()
    {
        return statusEffectCategory == StatusEffectCategory.POSITIVE;
    }

    public bool IncludesStatusDebuff()
    {
        return statusEffectCategory == StatusEffectCategory.NEGATIVE;
    }

    //public abstract Stats GenerateStatChangesFromSourceToTarget(CharacterData inSource, CharacterData inTarget);
    public virtual void PerformSetupTasks() { }
    public virtual SkillResult GenerateResultToTarget(CharacterData inSource, CharacterData inTarget) { throw new Exception("This method should not be called on base class 'Skill'"); }

    public string GetNameAndCost()
    {
        string txtResult = name + ".";
        if (mpCost > 0)
            txtResult += " " + Locale.Get("MP_COST", mpCost.ToString()) + ".";

        //special text addition for displaying the defend skill
        if(id == SkillId.DEFEND)
        {
            txtResult += " " + Locale.Get("INTERACT_AGAIN_TO_DEFEND");
        }

        return txtResult;
    }

    public string GetFullDesc()
    {
        string desc = GetNameAndCost() + " ";

        string skillType = "";
        if (type == SkillType.MAGICAL)
            skillType = Locale.Get("MAGICAL");
        else if (type == SkillType.PHYSICAL)
            skillType = Locale.Get("PHYSICAL");

        if (!string.IsNullOrEmpty(skillType))
            desc += skillType + ". ";

        //assume we want to describe the skill in terms of attack power if it exists
        if(attackMultiplier > 0)
        {
            string strMultiplier = "";
            if (attackMultiplier < 1) strMultiplier = attackMultiplier.ToString(".00");
            else strMultiplier = attackMultiplier.ToString();

            if (strMultiplier[strMultiplier.Length - 1] == '0')
                strMultiplier = strMultiplier.Substring(0, strMultiplier.Length - 1);

            strMultiplier += "<size=-5>x</size>";

            string strTargetType = Locale.Get( targetType == SkillTarget.ALL ? "GROUP" : "TARGET");
            desc += Locale.Get("DEALS_ATK_X", strMultiplier, strTargetType) + ". ";
        }

        string flavorDesc = GetFlavorText();
        if(flavorDesc != null)
        {
            desc += flavorDesc + " ";
        }

        if(cooldown > 0)
        {
            desc += Locale.Get("COOLDOWN_X", cooldown.ToString()) + ". ";
        }
            
        if (stats != null)
        {
            //no stats, end with a period
            desc += " " + stats.ToStringFormatted();
        }

        return desc;
    }

    //other skills can override this to provide more specific skill details
    protected virtual string GetFlavorText()
    {
        return Locale.Get("SKILL_DESC_" + id);
    }

    //some skills can be powered up through various save file properties
    protected virtual void AugmentPropertiesBasedOnSaveFile(SaveFile inFile)
    {

    }

    //it would be more proper to keep this logic inside of each concrete skill class but having it here makes it way easier to see the damage spread across all skills
    protected long CalcHpChange(CharacterData inSource, CharacterData inTarget)
    {
        long change = 0;
        long atk = inSource.stats.Get(Stat.atk);

        //all values here are calculated as positive for reference/comparison. Concrete skill subclasses may change them as needed
        switch(id)
        {
            case SkillId.STRIKE: change = atk; break;
            case SkillId.STRIKE_SWORD_SLASH: change = atk; break;
            case SkillId.STRIKE_MEDIUM_BLUE: change = atk; break;
            case SkillId.STRIKE_HORIZONTAL_MEDIUM: change = atk; break;
            case SkillId.STRIKE_HORIZONTAL_MEDIUM_BLUE: change = atk; break;
            case SkillId.STRIKE_LARGE: change = atk; break;
            case SkillId.STRIKE_HORIZONTAL_LARGE: change = atk; break;
            case SkillId.PICKAXE: change = ApplyMultiplierWithMinimumResult(atk, .75f, 1); break;
            case SkillId.FIRST_AID: change = inTarget.stats.Get(Stat.max_hp); break;
            case SkillId.GROUP_HEAL: change = ApplyMultiplierWithMinimumResult(inTarget.stats.Get(Stat.max_hp), .2f, 100); break;
            case SkillId.BIOHAZARD: change = ApplyMultiplierWithMinimumResult(atk, attackMultiplier, 1); break;
            case SkillId.SWIFT_METAL: change = ApplyMultiplierWithMinimumResult(atk, attackMultiplier, 6); break;
            case SkillId.TORMENT: change = ApplyMultiplierWithMinimumResult(atk, attackMultiplier, 17); break;
            case SkillId.NOXIN: change = ApplyMultiplierWithMinimumResult(atk, attackMultiplier, 25); break;
            case SkillId.BURP: change = Floor(atk, attackMultiplier); break;
            case SkillId.RANSACK: change = Floor(atk, attackMultiplier); break;
            case SkillId.EVISCERATE: change = ApplyMultiplierWithMinimumResult(atk, attackMultiplier, 35); break;
            case SkillId.FORETELL: change = 0; break;

            case SkillId.HEAL: change = 20; break;
            case SkillId.PHANTASMAL_CLAW: change = Floor(atk, attackMultiplier); break;
            case SkillId.SKULL_BASH: change = Floor(atk, attackMultiplier); break;
            case SkillId.TECH_LASER: change = Floor(atk, attackMultiplier); break;
            case SkillId.DARK_FLAME: change = Floor(atk, attackMultiplier); break;
            case SkillId.JUDGEMENT: throw new System.Exception("Judgement is a special skill and should not derive its damage number from the main Skill class");

            case SkillId.DREDGE: change = atk; break;
            case SkillId.PYROBLAST: change = atk; break;
            case SkillId.INFECT: change = atk; break;
            case SkillId.DOOTS: change = 0; break;
            case SkillId.WEB_BALL: change = atk; break;
            case SkillId.BITE: change = ApplyMultiplierWithMinimumGain(atk, 1.1f, 2); break;
            case SkillId.ROCK_TOSS: change = ApplyMultiplierWithMinimumGain(atk, attackMultiplier, 2); break;
            case SkillId.BONE_TOSS: change = atk; break;
            case SkillId.DART: change = atk; break;
            case SkillId.KNIFE: change = atk; break;
            case SkillId.WHIRLWIND: change = ApplyMultiplierWithMinimumGain(atk, 1.1f, 2); break;
            case SkillId.DISORIENT: change = ApplyMultiplierWithMinimumGain(atk, 1.1f, 2); break;
            case SkillId.CLAW: change = atk; break;
            case SkillId.FORK_SLASH: change = atk; break;
            case SkillId.DARK_SLASH: change = ApplyMultiplierWithMinimumGain(atk, 1.3f, 2); break;
            case SkillId.MEGA_CLAW: change = Floor(atk, .8f); break;
            case SkillId.CROSSCUT: change = atk; break;
            case SkillId.LICK: change = atk; break;
            case SkillId.METEOR: change = ApplyMultiplierWithMinimumGain(atk, 1.5f, 1); break;
            case SkillId.BOULDER: change = ApplyMultiplierWithMinimumGain(atk, 1.3f, 2); break;
            case SkillId.DEADLY_GLARE: change = atk; break;
            case SkillId.HEAL_2: change = inTarget.stats.Get(Stat.max_hp)/5; break;
            case SkillId.STELLAR_BLAST: change = Floor(atk, .5f); break;
            case SkillId.SHADOW_SLICER: change = Floor(atk, .5f); break;
            case SkillId.SHADOW_SLICER_2: change = Floor(atk, attackMultiplier * inSource.GetCurTeam().GetNumLivingMembers() ); break;
            case SkillId.BUBBLE_CANNON: change = Floor(atk, .7f); break;
            case SkillId.LOBSTER_CLAWS: change = Floor(atk, .9f); break;
            case SkillId.STOMP_1: change = Floor(atk, 1.3f); break;
            case SkillId.STOMP_2: change = Floor(atk, 1.5f); break;
            case SkillId.LASER: change = Floor(atk, 2.5f); break;
            case SkillId.ANIME_BLADE: change = Floor(atk, 1.5f); break;
            case SkillId.FLUSTERSTORM: change = Floor(atk, .75f); break;
            case SkillId.CUSPATE_BLADE: change = atk; break;
            case SkillId.DIAL_IT_UP: change = atk; break;
            case SkillId.WRONG_NUMBER: change = atk; break;
            case SkillId.SMITE: change =  Floor(atk, 1.9f); break;
            case SkillId.MEDITATE: change = inTarget.stats.Get(Stat.max_hp)/4; break;
            case SkillId.GRILL_HEAL: change = inTarget.stats.Get(Stat.max_hp)/3; break;
            case SkillId.DELUGE: change = atk; break;
            case SkillId.CRYSTAL_TOSS: change = atk; break;
            case SkillId.DARK_DREAMS: change = Floor(atk, .5f); break;
            case SkillId.DARK_MEGA_CLAW: change = atk; break;
            case SkillId.STUN_BOLT: change =  Floor(atk, 2f); break;
            case SkillId.ARMAGEDDON: change =  Floor(atk, 2f); break;
        }

        return change;
    }

    private long Floor(long inAmt, float inMul)
    {
        return (long)(inAmt * inMul);
    }

    private long Max(long inA, long inB)
    {
        return inA > inB ? inA : inB;
    }

    private long ApplyMultiplierWithMinimumResult(long inBase, float inMul, long inMin)
    {
        long newAmt = (long)(inBase * inMul);
        if (newAmt < inMin) return inMin;
        else return newAmt;
    }

    private long ApplyMultiplierAndConstrainToRange(long inBase, float inMul, long inMin, long inMax)
    {
        long newAmt = (long)(inBase * inMul);

        if (newAmt < inMin) return inMin;
        else if (newAmt > inMax) return inMax;
        else return newAmt; 
    }

    private long ApplyMultiplierWithMinimumGain(long inBase, float inMul, long inMin)
    {
        return ApplyMultiplierWithMinimumResult(inBase, inMul, (inBase + inMin));
    }
}
