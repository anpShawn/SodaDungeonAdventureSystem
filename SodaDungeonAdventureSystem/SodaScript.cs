using System.Collections.Generic;
using System;
using System.Linq;


public class SodaScript
{
    private static List<SodaScript> scripts;
    private static SodaScriptTrigger defaultTrigger;
    public const short MAX_VAR_NAME_LENGTH = 10;
    public const short MAX_NUM_VARIABLES = 20;
    public const short MAX_INSTRUCTIONS = 50;
    public const short MAX_EXECUTION_TIME_IN_MS = 5;

    public const string SCRIPT_DEFAULT_ENEMY = "Default Enemy";
    public const string SCRIPT_DEFAULT_PLAYER = "Default Player";
    public const string SCRIPT_DARK_LADY = "Dark Lady";

    public static SodaScriptTarget[] USER_ALLOWED_TARGETS;
    
    private static Dictionary<SodaScriptTarget, SodaScriptCondition[]> allowedConditionsForTarget;
    private static SodaScriptCondition[] conditionsThatRequireAComparison;

    private static SodaScriptComparison[] allComparisons;
    private static SodaScriptComparison[] justEqualsComparison;
    private static SodaScriptComparison[] greaterOrLessThanComparison;

    private static Dictionary<SodaScriptCondition, string[]> comparisonValuesForCondition;

    public static SodaScriptSkillCategory[] USER_ALLOWED_SKILL_CATEGORIES;

    private static Dictionary<SodaScriptTarget, string[]> allowedSkillIdsForTarget;

    public static SodaScript GetPremadeScript(string inName)
    {
        for (int i = 0; i < scripts.Count; i++)
            if (scripts[i].name == inName)
                return scripts[i];

        return null;
    }

    public static void CreateScripts()
    {
        scripts = new List<SodaScript>();

        SodaScript script;
        SodaScriptTrigger trigger;

        //ENEMY
        script = new SodaScript(SCRIPT_DEFAULT_ENEMY);
        script._isReadonly = true;

        //testing trigger, use this to force a skill
        //trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.ANY);
        //trigger.SetSkill(SodaScriptSkillCategory.SPECIFIC, SkillId.STUN);

        trigger = script.AddTrigger(SodaScriptTarget.SELF, SodaScriptCondition.HP, SodaScriptComparison.LESS_THAN, "50");
        trigger.SetSkill(SodaScriptSkillCategory.STRONGEST_HEAL);
        trigger.percentageChance = 70;

        trigger = script.AddTrigger(SodaScriptTarget.ALLY, SodaScriptCondition.HP, SodaScriptComparison.LESS_THAN, "60");
        trigger.SetSkill(SodaScriptSkillCategory.STRONGEST_HEAL);
        trigger.percentageChance = 90;

        trigger = script.AddTrigger(SodaScriptTarget.SELF, SodaScriptCondition.STATUS, SodaScriptComparison.NOT_EQUALS, StatusEffectCategory.POSITIVE.ToString());
        trigger.SetSkill(SodaScriptSkillCategory.ANY_SELF_BUFF);
        trigger.percentageChance = 70;

        //we can use this to attempt debuffs specifically, but any debuff is included in "any non friendly that consumes mp" trigger
        //trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.STATUS, SodaScriptComparison.NOT_EQUALS, StatusEffectCategory.NEGATIVE.ToString());
        //trigger.SetSkill(SodaScriptSkillCategory.ANY_DEBUFF);

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.ANY);
        trigger.SetSkill(SodaScriptSkillCategory.ANY_NON_FRIENDLY_THAT_CONSUMES_MP);
        trigger.percentageChance = 25;

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.ANY);
        trigger.SetSkill(SodaScriptSkillCategory.ANY_NON_FRIENDLY_THAT_DOESNT_CONSUME_MP);

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.ANY);
        trigger.SetSkill(SodaScriptSkillCategory.DEFAULT);
        scripts.Add(script);

        //DARK LADY
        script = new SodaScript(SCRIPT_DARK_LADY);
        script._isReadonly = true;

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.TEAM_NUM_BACK_TURNED, SodaScriptComparison.GREATER_THAN, "0");
        trigger.SetSkill(SodaScriptSkillCategory.SPECIFIC, SkillId.CUSPATE_BLADE);

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.TEAM_NUM_BACK_TURNED, SodaScriptComparison.LESS_THAN, "3");
        trigger.SetSkill(SodaScriptSkillCategory.SPECIFIC, SkillId.FLUSTERSTORM);

        scripts.Add(script);

        //PLAYER
        script = new SodaScript(SCRIPT_DEFAULT_PLAYER);
        script._isReadonly = true;

        //testing trigger, use this to force a skill
        //trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.ANY);
        //trigger.SetSkill(SodaScriptSkillCategory.SPECIFIC, SkillId.STUN);

        trigger = script.AddTrigger(SodaScriptTarget.SELF, SodaScriptCondition.HP, SodaScriptComparison.LESS_THAN, "60");
        trigger.SetSkill(SodaScriptSkillCategory.STRONGEST_HEAL);

        trigger = script.AddTrigger(SodaScriptTarget.ALLY, SodaScriptCondition.HP, SodaScriptComparison.LESS_THAN, "60");
        trigger.SetSkill(SodaScriptSkillCategory.STRONGEST_HEAL);

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.TEAM_ALIVE, SodaScriptComparison.GREATER_THAN, "2");
        trigger.SetSkill(SodaScriptSkillCategory.STRONGEST_GROUP_ATTACK);

        //50% chance to use a debuff skill on character with no debuffs
        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.STATUS, SodaScriptComparison.NOT_EQUALS, StatusEffectCategory.NEGATIVE.ToString());
        trigger.SetSkill(SodaScriptSkillCategory.ANY_DEBUFF);
        trigger.percentageChance = 50;

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.RANK, SodaScriptComparison.GREATER_THAN, EnemyRank.MBOS.ToString());
        trigger.SetSecondaryCondition(SodaScriptCondition.HP, SodaScriptComparison.GREATER_THAN, "50");
        trigger.SetSkill(SodaScriptSkillCategory.STRONGEST_SINGLE_TARGET_ATTACK);

        trigger = script.AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.ISNT_ORE);
        trigger.SetSecondaryCondition(SodaScriptCondition.WEAKEST);
        trigger.SetSkill(SodaScriptSkillCategory.DEFAULT);
        scripts.Add(script);

        //default trigger that will mostly be used by player scripts
        defaultTrigger = new SodaScriptTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.WEAKEST);
        defaultTrigger.SetSkill(SodaScriptSkillCategory.DEFAULT);

        //create some lists to limit user input to logically correct values
        USER_ALLOWED_TARGETS = new SodaScriptTarget[]
        {
            SodaScriptTarget.ENEMY,
            SodaScriptTarget.ALLY,
            SodaScriptTarget.SELF
        };

        allowedConditionsForTarget = new Dictionary<SodaScriptTarget, SodaScriptCondition[]>();
        allowedConditionsForTarget[SodaScriptTarget.ENEMY] = new SodaScriptCondition[]
        {
            SodaScriptCondition.WEAKEST,
            SodaScriptCondition.HP,
            SodaScriptCondition.MP,
            SodaScriptCondition.TEAM_ALIVE,
            SodaScriptCondition.TEAM_HP,
            SodaScriptCondition.RANK,
            SodaScriptCondition.IS_ORE,
            SodaScriptCondition.ISNT_ORE,
            SodaScriptCondition.BACK_IS_TURNED,
            SodaScriptCondition.BACK_ISNT_TURNED,
            SodaScriptCondition.STATUS,
            SodaScriptCondition.HAS_ESSENCE,
            SodaScriptCondition.NO_ESSENCE,
            SodaScriptCondition.IS_JANITOR,
            SodaScriptCondition.ISNT_JANITOR,
            SodaScriptCondition.ANY
        };

        allowedConditionsForTarget[SodaScriptTarget.ALLY] = new SodaScriptCondition[]
        {
            SodaScriptCondition.WEAKEST,
            SodaScriptCondition.HP,
            SodaScriptCondition.MP,
            SodaScriptCondition.TEAM_ALIVE,
            SodaScriptCondition.TEAM_HP,
            SodaScriptCondition.STATUS,
            SodaScriptCondition.POSITION,
            SodaScriptCondition.ANY
        };

        allowedConditionsForTarget[SodaScriptTarget.SELF] = new SodaScriptCondition[]
        {
            SodaScriptCondition.HP,
            SodaScriptCondition.MP,
            SodaScriptCondition.TEAM_ALIVE,
            SodaScriptCondition.TEAM_HP,
            SodaScriptCondition.STATUS
        };

        conditionsThatRequireAComparison = new SodaScriptCondition[]{
            SodaScriptCondition.HP,
            SodaScriptCondition.MP,
            SodaScriptCondition.TEAM_ALIVE,
            SodaScriptCondition.TEAM_HP,
            SodaScriptCondition.RANK,
            SodaScriptCondition.STATUS,
            SodaScriptCondition.POSITION
        };

        allComparisons = new SodaScriptComparison[] { SodaScriptComparison.GREATER_THAN, SodaScriptComparison.LESS_THAN, SodaScriptComparison.EQUALS, SodaScriptComparison.NOT_EQUALS };
        justEqualsComparison = new SodaScriptComparison[] { SodaScriptComparison.EQUALS, SodaScriptComparison.NOT_EQUALS };
        greaterOrLessThanComparison = new SodaScriptComparison[] { SodaScriptComparison.LESS_THAN, SodaScriptComparison.GREATER_THAN };


        /**********************************************************************************************************************
         * DATA AND LABELS FOR COMPARISON VALUES THAT BELONG TO SPECIFIC CONDITIONS
         * ******************************************************************************************************************** */
        string[] zeroToOneHundred = GenerateNumericDataList(0, 100, 5);
        string[] zeroToSix = GenerateNumericDataList(0, 6, 1);
        string[] oneToSix = GenerateNumericDataList(1, 6, 1);

        comparisonValuesForCondition = new Dictionary<SodaScriptCondition, string[]>();

        comparisonValuesForCondition[SodaScriptCondition.HP] = zeroToOneHundred;
        comparisonValuesForCondition[SodaScriptCondition.MP] = zeroToOneHundred;
        comparisonValuesForCondition[SodaScriptCondition.TEAM_ALIVE] = zeroToSix;
        comparisonValuesForCondition[SodaScriptCondition.POSITION] = oneToSix;
        comparisonValuesForCondition[SodaScriptCondition.TEAM_HP] = zeroToOneHundred;

        EnemyRank[] ranks = new EnemyRank[] { EnemyRank.NORM, EnemyRank.MBOS, EnemyRank.BOSS, EnemyRank.DBOS };
        comparisonValuesForCondition[SodaScriptCondition.RANK] = ranks.Select(d => d.ToString()).ToArray<string>();

        StatusEffectCategory[] effectCategories = new StatusEffectCategory[] { StatusEffectCategory.POSITIVE, StatusEffectCategory.NEGATIVE, StatusEffectCategory.NONE };
        comparisonValuesForCondition[SodaScriptCondition.STATUS] = effectCategories.Select(d => d.ToString()).ToArray<string>();

        /**********************************************************************************************************************
         * SKILL CATEGORIES AND IDS
         * ******************************************************************************************************************** */
        USER_ALLOWED_SKILL_CATEGORIES = new SodaScriptSkillCategory[]
        {
            SodaScriptSkillCategory.DEFAULT,
            SodaScriptSkillCategory.STRONGEST_ATTACK,
            SodaScriptSkillCategory.STRONGEST_GROUP_ATTACK,
            SodaScriptSkillCategory.STRONGEST_SINGLE_TARGET_ATTACK,
            SodaScriptSkillCategory.STRONGEST_HEAL,
            SodaScriptSkillCategory.STRONGEST_GROUP_HEAL,
            SodaScriptSkillCategory.ANY_BUFF,
            SodaScriptSkillCategory.ANY_DEBUFF,
            SodaScriptSkillCategory.ANY,
            SodaScriptSkillCategory.SPECIFIC
        };

        //this array is locally scoped, we only use it to derive more specific lists
        //defend is not included here, it is a special case added later
        string[] userAllowedSkillIds = new string[]
        {
            SkillId.SWIFT_METAL,
            SkillId.BIOHAZARD,
            SkillId.FIRST_AID,
            SkillId.HEAL
        };

        allowedSkillIdsForTarget = new Dictionary<SodaScriptTarget, string[]>();

        //build out the lists/arrays
        Skill curSkill;
        List<string> allowedEnemySkillIds = new List<string>();
        List<string> allowedAllySkillIds = new List<string>();
        List<string> allowedSelfSkillIds = new List<string>();
        for(int i=0; i<userAllowedSkillIds.Length; i++)
        {
            curSkill = Skill.GetSkill(userAllowedSkillIds[i]);

            if (curSkill.isFriendly)
            {
                allowedAllySkillIds.Add(curSkill.id);

                if (curSkill.allowsSelfTarget)
                    allowedSelfSkillIds.Add(curSkill.id);
            }
            else
                allowedEnemySkillIds.Add(curSkill.id);
        }

        //make sure to add "defend" for all categories
        allowedEnemySkillIds.Add(SkillId.DEFEND);
        allowedAllySkillIds.Add(SkillId.DEFEND);
        allowedSelfSkillIds.Add(SkillId.DEFEND);

        allowedSkillIdsForTarget[SodaScriptTarget.ENEMY] = allowedEnemySkillIds.ToArray<string>();
        allowedSkillIdsForTarget[SodaScriptTarget.ALLY] = allowedAllySkillIds.ToArray<string>();
        allowedSkillIdsForTarget[SodaScriptTarget.SELF] = allowedSelfSkillIds.ToArray<string>();
    }

    //CONDITIONS
    public static SodaScriptCondition[] GetAllowedConditionsForTarget(SodaScriptTarget inTarget)
    {
        return allowedConditionsForTarget[inTarget];
    }

    public static bool IsAllowedConditionForTarget(SodaScriptCondition inCondition, SodaScriptTarget inTarget)
    {
        SodaScriptCondition[] allowedConditions = GetAllowedConditionsForTarget(inTarget);
        return allowedConditions.Contains(inCondition);
    }

    public static SodaScriptCondition GetDefaultConditionForTarget(SodaScriptTarget inTarget)
    {
        return allowedConditionsForTarget[inTarget][0];
    }

    public static bool ConditionUsesPercentageCompareValues(SodaScriptCondition inCondition)
    {
        return inCondition == SodaScriptCondition.HP || inCondition == SodaScriptCondition.MP || inCondition == SodaScriptCondition.TEAM_HP;
    }

    public static bool ConditionRequiresAComparison(SodaScriptCondition inCondition)
    {
        for (int i = 0; i < conditionsThatRequireAComparison.Length; i++)
            if (conditionsThatRequireAComparison[i] == inCondition)
                return true;

        return false;
    }

    //COMPARISONS
    public static SodaScriptComparison[] GetAllowedComparisonsForCondition(SodaScriptCondition inCondition)
    {
        if (inCondition == SodaScriptCondition.STATUS)
            return justEqualsComparison;
        else if (ConditionUsesPercentageCompareValues(inCondition))
            return greaterOrLessThanComparison;
        else
            return allComparisons;
    }

    public static bool IsAllowedComparisonForCondition(SodaScriptComparison inComparison, SodaScriptCondition inCondition)
    {
        SodaScriptComparison[] allowedComparisons = GetAllowedComparisonsForCondition(inCondition);
        return allowedComparisons.Contains(inComparison);
    }

    public static SodaScriptComparison GetDefaultComparisonForCondition(SodaScriptCondition inCondition)
    {
        return GetAllowedComparisonsForCondition(inCondition)[0];
    }

    //COMPARE VALUES
    public static string[] GetComparisonValuesForCondition(SodaScriptCondition inCondition)
    {
        return comparisonValuesForCondition[inCondition];
    }

    public static bool IsAllowedComparisonValueForCondition(string inValue, SodaScriptCondition inCondition)
    {
        string[] allowedValues = GetComparisonValuesForCondition(inCondition);
        return allowedValues.Contains(inValue);
    }

    public static string GetDefaultCompareValueForCondition(SodaScriptCondition inCondition)
    {
        return GetComparisonValuesForCondition(inCondition)[0];
    }

    //SKILL IDS
    public static string[] GetAllowedSkillIdsForTarget(SodaScriptTarget inTarget)
    {
        return allowedSkillIdsForTarget[inTarget];
    }

    public static bool IsAllowedSkillIdForTarget(string inSkillId, SodaScriptTarget inTarget)
    {
        string[] allowedSkills = GetAllowedSkillIdsForTarget(inTarget);
        return allowedSkills.Contains(inSkillId);
    }

    public static string GetDefaultSkillIdForTarget(SodaScriptTarget inTarget)
    {
        return GetAllowedSkillIdsForTarget(inTarget)[0];
    }

    //HELPER
    private static string[] GenerateNumericDataList(int inStart, int inEnd, int inStep)
    {
        List<string> nums = new List<string>();
        for(int i= inStart; i<= inEnd; i+=inStep)
            nums.Add(i.ToString());

        return nums.ToArray<string>();
    }




    public string name;
    public int id;
    public List<SodaScriptTrigger> triggers;
    public bool isReadonly { get { return _isReadonly; } }

    //cached/resued data for the execution of the script itself; these are wiped out when execution finishes.
    [NonSerialized]private Adventure adventure;
    [NonSerialized]private CharacterData activeCharacter;
    [NonSerialized]private CharacterTeam ownTeam;
    [NonSerialized]private CharacterTeam opposingTeam;
    [NonSerialized]private List<CharacterData> potentialTargets;
    [NonSerialized]private List<CharacterData> culledTargets;
    [NonSerialized]private Skill potentialSkill;

    [NonSerialized]private bool _isReadonly; //this is just so the user can't edit premade scripts

    public SodaScript(string inName)
    {
        name = inName;
        triggers = new List<SodaScriptTrigger>();
    }

    //Validate ensures that the chosen combination of target, condition, comparison, value, and skill all make sense.
    //In the case of an invalid combination, default values are chosen
    public void ValidateTrigger(SodaScriptTrigger inTrigger)
    {
        if (!triggers.Contains(inTrigger))
            throw new Exception("Soda script was asked to validate a trigger it doesn't contain");

        //VALIDATE PRIMARY CONDITION
        if (!IsAllowedConditionForTarget(inTrigger.condition, inTrigger.target))
            inTrigger.condition = GetDefaultConditionForTarget(inTrigger.target);

        if(ConditionRequiresAComparison(inTrigger.condition))
        {
            if (!IsAllowedComparisonForCondition(inTrigger.comparison, inTrigger.condition))
                inTrigger.comparison = GetDefaultComparisonForCondition(inTrigger.condition);

            if (!IsAllowedComparisonValueForCondition(inTrigger.compareValue, inTrigger.condition))
                inTrigger.compareValue = GetDefaultCompareValueForCondition(inTrigger.condition);
        }

        //VALIDATE SECONDARY CONDITION
        if (!IsAllowedConditionForTarget(inTrigger.secondaryCondition, inTrigger.target))
            inTrigger.secondaryCondition = GetDefaultConditionForTarget(inTrigger.target);

        if (ConditionRequiresAComparison(inTrigger.secondaryCondition))
        {
            if (!IsAllowedComparisonForCondition(inTrigger.secondaryComparison, inTrigger.secondaryCondition))
                inTrigger.secondaryComparison = GetDefaultComparisonForCondition(inTrigger.secondaryCondition);

            if (!IsAllowedComparisonValueForCondition(inTrigger.secondaryCompareValue, inTrigger.secondaryCondition))
                inTrigger.secondaryCompareValue = GetDefaultCompareValueForCondition(inTrigger.secondaryCondition);
        }

        //VALIDATE CHOSEN SKILL ID (IF THERE IS ONE)
        if(inTrigger.skillCategory == SodaScriptSkillCategory.SPECIFIC)
        {
            if (!IsAllowedSkillIdForTarget(inTrigger.skillId, inTrigger.target))
                inTrigger.skillId = GetDefaultSkillIdForTarget(inTrigger.target);
        }
    }

    public void CopyTriggersFrom(SodaScript inOtherScript)
    {
        triggers.Clear();

        for(int i=0; i<inOtherScript.triggers.Count; i++)
        {
            triggers.Add(inOtherScript.triggers[i].GetCopy());
        }
    }

    public SodaScriptTrigger AddTrigger(SodaScriptTarget inTarget, SodaScriptCondition inCondition, SodaScriptComparison inComparison = SodaScriptComparison.EQUALS, string inCompareValue = "")
    {
        SodaScriptTrigger t = new SodaScriptTrigger(inTarget, inCondition, inComparison, inCompareValue);
        triggers.Add(t);
        return t;
    }

    public SodaScriptTrigger AddDefaultTrigger()
    {
        SodaScriptTrigger dt = AddTrigger(SodaScriptTarget.ENEMY, SodaScriptCondition.WEAKEST);
        dt.SetSkill(SodaScriptSkillCategory.DEFAULT);
        return dt;
    }

    public void MoveTriggerUp(SodaScriptTrigger inTrigger)
    {
        if (!triggers.Contains(inTrigger))
            throw new Exception("Soda script does not contain requested trigger");

        if (triggers.Count == 1)
            return;

        int triggerIndex = GetTriggerIndex(inTrigger);
        if (triggerIndex == 0)
            return;

        triggers.Remove(inTrigger);
        triggers.Insert(triggerIndex - 1, inTrigger);
    }

    public void MoveTriggerDown(SodaScriptTrigger inTrigger)
    {
        if(!triggers.Contains(inTrigger))
            throw new Exception("Soda script does not contain requested trigger");

        if (triggers.Count == 1)
            return;

        int triggerIndex = GetTriggerIndex(inTrigger);
        if (triggerIndex == triggers.Count-1)
            return;

        triggers.Remove(inTrigger);
        triggers.Insert(triggerIndex + 1, inTrigger);
    }

    public float GetNormalizedListPositionForTrigger(SodaScriptTrigger inTrigger)
    {
        float triggerIndex = GetTriggerIndex(inTrigger);
        if (triggerIndex == 0) return 0;

        return (triggerIndex + 1) / triggers.Count;
    }

    private int GetTriggerIndex(SodaScriptTrigger inTrigger)
    {
        for(int i=0; i<triggers.Count; i++)
        {
            if (triggers[i] == inTrigger)
                return i;
        }

        throw new Exception("Soda script does not contain requested trigger");
    }

    public void DeleteTrigger(SodaScriptTrigger inTrigger)
    {
        if (!triggers.Contains(inTrigger))
            throw new Exception("Soda script does not contain requested trigger");

        triggers.Remove(inTrigger);
    }

    public void ExecuteTriggers(CharacterData inActiveCharacter, Adventure inAdventure, SkillCommand inCommandToPopulate)
    {
        //store some data up front that will probably be useful
        activeCharacter = inActiveCharacter;
        adventure = inAdventure;
        opposingTeam = inAdventure.GetTeamOppositeOfFaction(activeCharacter.curFaction);
        ownTeam = inAdventure.GetTeamForFaction(activeCharacter.curFaction);

        if (potentialTargets == null)
            potentialTargets = new List<CharacterData>();
        if (culledTargets == null)
            culledTargets = new List<CharacterData>();

        potentialSkill = null;

        //for a trigger to eval it needs a valid target, and the ability to cast the requested spell
        bool atLeastOneTriggerEvaluated = false;
        for(int i=0; i<triggers.Count; i++)
        {
            if (EvalTrigger(triggers[i], inCommandToPopulate))
            {
                atLeastOneTriggerEvaluated = true;
                break;
            }
        }

        //if we made it this far, fall back to the default trigger that will always fire
        if(!atLeastOneTriggerEvaluated)
            EvalTrigger(defaultTrigger, inCommandToPopulate);


        //cleanup
        potentialTargets.Clear();
        culledTargets.Clear();
        potentialSkill = null;
    }

    private bool EvalTrigger(SodaScriptTrigger inTrigger, SkillCommand inCommandToPopulate)
    {
        //is this a trigger with a percentage chance? if so start with that since it can save us processing time
        if(inTrigger.percentageChance < 100)
        {
            if (!Utils.PercentageChance(inTrigger.percentageChance))
                return false;
        }

        potentialTargets.Clear();
        culledTargets.Clear();

        CharacterData curTarget = null;
        if(inTrigger.target == SodaScriptTarget.SELF)
        {
            potentialTargets.Add(activeCharacter);
        }
        else if(inTrigger.target == SodaScriptTarget.ALLY)
        {
            for(int i=0; i<ownTeam.members.Count; i++)
            {
                curTarget = ownTeam.members[i];
                if (!curTarget.dead && curTarget != activeCharacter)
                    potentialTargets.Add(curTarget);
            }
        }
        else if(inTrigger.target == SodaScriptTarget.ENEMY)
        {
            for(int i=0; i<opposingTeam.members.Count; i++)
            {
                curTarget = opposingTeam.members[i];
                if (!curTarget.dead)
                    potentialTargets.Add(curTarget);
            }
        }

        //we might not even have a list of *potential* targets! (this would happen in a case when we try target an ally but we're the only member on the team)
        if (potentialTargets.Count == 0)
            return false;

        //eval the condition on our list
        if(inTrigger.condition == SodaScriptCondition.ANY)
        {
            curTarget = potentialTargets.GetRandomElement();
            potentialTargets.Clear();
            potentialTargets.Add(curTarget);
        }
        else
        {
            //default to a normal eval
            EvalCondition(inTrigger.condition, inTrigger.comparison, inTrigger.compareValue, potentialTargets);
        }

        //exit if no potentials found yet
        if (potentialTargets.Count == 0)
            return false;
        
        //do we have to check a secondary condition?
        if(inTrigger.hasSecondaryCondition)
        {
            EvalCondition(inTrigger.secondaryCondition, inTrigger.secondaryComparison, inTrigger.secondaryCompareValue, potentialTargets);
        }

        //did we find a valid target? if not, return false
        if (potentialTargets.Count == 0)
            return false;

        //fix some issues when targetting self and requesting certain skill categories, turn into strongest *self* heal (avoids issues with first aid)
        if (inTrigger.target == SodaScriptTarget.SELF)
        {
            if (inTrigger.skillCategory == SodaScriptSkillCategory.STRONGEST_HEAL)
                inTrigger.skillCategory = SodaScriptSkillCategory.STRONGEST_SELF_HEAL;

            if (inTrigger.skillCategory == SodaScriptSkillCategory.ANY_BUFF)
                inTrigger.skillCategory = SodaScriptSkillCategory.ANY_SELF_BUFF;
        }

        //is the skill also valid? if not, return false
        if(inTrigger.skillCategory == SodaScriptSkillCategory.DEFAULT)
        {
            potentialSkill = activeCharacter.GetDefaultSkill();
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.SPECIFIC)
        {
            potentialSkill = Skill.GetSkill(inTrigger.skillId);
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.ANY_NON_FRIENDLY_THAT_CONSUMES_MP)
        {
            potentialSkill = activeCharacter.GetAnyNonFriendlySkillThatUsesMP();
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.ANY_NON_FRIENDLY_THAT_DOESNT_CONSUME_MP)
        {
            potentialSkill = activeCharacter.GetAnyNonFriendlySkillThatDoesntUseMP();
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.ANY)
        {
            potentialSkill = activeCharacter.GetRandomUsableSkill();
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.STRONGEST_SELF_HEAL)
        {
            potentialSkill = activeCharacter.GetStrongestUsableHealSkill(false, true);
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.STRONGEST_HEAL)
        {
            potentialSkill = activeCharacter.GetStrongestUsableHealSkill(false, false);
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.STRONGEST_GROUP_HEAL)
        {
            potentialSkill = activeCharacter.GetStrongestUsableHealSkill(true, false);
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.STRONGEST_ATTACK)
        {
            potentialSkill = activeCharacter.GetStrongestUsableDamageSkill(false);
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.STRONGEST_GROUP_ATTACK)
        {
            potentialSkill = activeCharacter.GetStrongestUsableDamageSkill(true);
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.STRONGEST_SINGLE_TARGET_ATTACK)
        {
            potentialSkill = activeCharacter.GetStrongestUsableDamageSkill(false, true);
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.ANY_BUFF)
        {
            potentialSkill = activeCharacter.GetAnySkillThatBuffs();
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.ANY_SELF_BUFF)
        {
            potentialSkill = activeCharacter.GetAnySkillThatSelfBuffs();
        }
        else if(inTrigger.skillCategory == SodaScriptSkillCategory.ANY_DEBUFF)
        {
            potentialSkill = activeCharacter.GetAnySkillThatDebuffs();
        }
        else
        {
            throw new Exception("Skill category " + inTrigger.skillCategory + " is not supported");
        }

        if(potentialSkill == null)
            return false;

        if (!activeCharacter.CanUseSkill(potentialSkill.id))
            return false;


        //make sure we're not using a friendly skill on an enemy (unless it is DEFEND)
        if (inTrigger.target == SodaScriptTarget.ENEMY && potentialSkill.isFriendly && potentialSkill.id != SkillId.DEFEND)
            return false;

        //make sure we're not using a damage skill on self or ally
        if ((inTrigger.target == SodaScriptTarget.ALLY || inTrigger.target == SodaScriptTarget.SELF) && !potentialSkill.isFriendly)
            return false;

        //make sure if targeting self, chosen skill is allowed to self target
        if (inTrigger.target == SodaScriptTarget.SELF && !potentialSkill.allowsSelfTarget)
            return false;

        //make sure not stealing from someone who has already been stolen from
        if ( (potentialSkill.id == SkillId.PILFER || potentialSkill.id == SkillId.RANSACK) && potentialTargets[0].hasBeenStolenFrom)
            return false;

        //don't try to transmute a target after we've already hit the limit of 1 successful transmute per battle (or if not a common enemy)
        if(potentialSkill.id == SkillId.TRANSMUTE)
        {
            bool battleIsBossOrMiniboss = (adventure.battleNumBasedOn10 == 5 || adventure.battleNumBasedOn10 == 10);
            if (adventure.battleManager.CurBattleTransmuteLimitReached() || battleIsBossOrMiniboss)
                return false;
        }
        else if(potentialSkill.id == SkillId.FORETELL)
        {
            //don't try to foretell if the "force warp" flag is active for the current adventure
            if (adventure.forceWarpAtNextPaths)
                return false;
        }

        //make sure an arena npc isn't using a banned skill
        if (activeCharacter.isArenaNpc && !potentialSkill.allowedForArenaNpcs)
            return false;

        //make sure this team hasn't tried to heal itself too many times in a row
        if (potentialSkill.isHealing && ownTeam.HasReachedConsecutiveHealLimit())
            return false;

        //if we found a valid target and skill, populate the commmand and return true
        inCommandToPopulate.Reset(potentialSkill, activeCharacter);
        inCommandToPopulate.AddTarget(potentialTargets[0]);
        return true;
    }

    //return true if this evals true for any target provided
    private void EvalCondition(SodaScriptCondition inCondition, SodaScriptComparison inComparison, string inCompareValue, List<CharacterData> inTargets )
    {
        CharacterData targ;
        EnemyCharacterData ecd;
        PlayerCharacterData pcd;
        culledTargets.Clear();

        if(inCondition == SodaScriptCondition.WEAKEST)
        {
            culledTargets.Add(GetLowestHpPercentTarget(inTargets));
        }
        else if(inCondition == SodaScriptCondition.TEAM_ALIVE)
        {
            targ = inTargets[0];
            CharacterTeam team = targ.IsInFaction(Faction.PLAYER) ? adventure.playerTeam : adventure.enemyTeam;
            if (EvalNumericComparison(team.GetNumLivingMembers(), int.Parse(inCompareValue), inComparison))
                culledTargets.Add(targ);
        }
        else if(inCondition == SodaScriptCondition.TEAM_HP)
        {
            targ = inTargets[0];
            CharacterTeam team = targ.IsInFaction(Faction.PLAYER) ? adventure.playerTeam : adventure.enemyTeam;
            if (EvalNumericComparison(team.GetTeamHPPercent(), int.Parse(inCompareValue), inComparison))
                culledTargets.Add(targ);
        }
        else if(inCondition == SodaScriptCondition.TEAM_NUM_BACK_TURNED)
        {
            targ = inTargets[0];
            CharacterTeam team = targ.IsInFaction(Faction.PLAYER) ? adventure.playerTeam : adventure.enemyTeam;
            int numBackTurned = team.GetNumMembersWithBackTurnedTo(activeCharacter);
            if (EvalNumericComparison(numBackTurned, int.Parse(inCompareValue), inComparison))
                culledTargets.Add(targ);
        }
        else
        {
            for(int i=0; i<inTargets.Count; i++)
            {
                targ = inTargets[i];
                if(inCondition == SodaScriptCondition.HP)
                {
                    if (EvalNumericComparison(targ.GetHpPercent(), int.Parse(inCompareValue), inComparison))
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.MP)
                {
                    if (EvalNumericComparison(targ.GetMpPercent(), int.Parse(inCompareValue), inComparison))
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.POSITION)
                {
                    if (EvalNumericComparison(targ.originalOrderInTeam+1, int.Parse(inCompareValue), inComparison))
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.RANK)
                {
                    bool foundRank = false;
                    EnemyRank enemyRank = EnemyRank.NORM;
                    EnemyRank parsedRank = (EnemyRank)Enum.Parse(typeof(EnemyRank), inCompareValue);

                    if(targ is EnemyCharacterData)
                    {
                        foundRank = true;
                        ecd = (EnemyCharacterData)targ;
                        enemyRank = ecd.rank;
                    }
                    else if(targ.id == CharId.JANITOR)
                    {
                        foundRank = true;
                        enemyRank = EnemyRank.BOSS;
                    }

                    if(foundRank)
                    {
                        if (EvalNumericComparison((int)enemyRank, (int)parsedRank, inComparison))
                            culledTargets.Add(targ);
                    }
                }
                else if(inCondition == SodaScriptCondition.IS_ORE)
                {
                    if (targ.isOre)
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.ISNT_ORE)
                {
                    if (!targ.isOre)
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.IS_JANITOR)
                {
                    if (targ.id == CharId.JANITOR)
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.ISNT_JANITOR)
                {
                    if (targ.id != CharId.JANITOR)
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.BACK_IS_TURNED)
                {
                    if(!targ.isOre && targ.BackIsTurnedTo(activeCharacter))
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.BACK_ISNT_TURNED)
                {
                    if(!targ.isOre && !targ.BackIsTurnedTo(activeCharacter))
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.HAS_ESSENCE)
                {
                    if (targ.hasEssence)
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.NO_ESSENCE)
                {
                    if (!targ.hasEssence)
                        culledTargets.Add(targ);
                }
                else if(inCondition == SodaScriptCondition.STATUS)
                {
                    StatusEffectCategory parsedCategory = (StatusEffectCategory)Enum.Parse(typeof(StatusEffectCategory), inCompareValue);
                    if (parsedCategory == StatusEffectCategory.POSITIVE)
                    {
                        bool hasPos = targ.HasPostiveStatusEffect();
                        if ((inComparison == SodaScriptComparison.EQUALS && hasPos) ||
                            (inComparison == SodaScriptComparison.NOT_EQUALS && !hasPos))
                            culledTargets.Add(targ);
                    }
                    else if(parsedCategory == StatusEffectCategory.NEGATIVE)
                    {
                        bool hasNeg = targ.HasNegativeStatusEffect();
                        if ((inComparison == SodaScriptComparison.EQUALS && hasNeg) ||
                            (inComparison == SodaScriptComparison.NOT_EQUALS && !hasNeg))
                            culledTargets.Add(targ);
                    }
                    else if(parsedCategory == StatusEffectCategory.NONE)
                    {
                        bool hasAny = targ.HasAnyStatusEffect();
                        if ((inComparison == SodaScriptComparison.EQUALS && !hasAny) ||
                            (inComparison == SodaScriptComparison.NOT_EQUALS && hasAny))
                            culledTargets.Add(targ);
                    }
                }
                else
                {
                    throw new Exception("Trigger condition " + inCondition + " is not supported as a condition");
                }
            }

            //do a final list sort based on the condition?
            if(inCondition == SodaScriptCondition.HP)
            {
                if (inComparison == SodaScriptComparison.GREATER_THAN)
                    culledTargets = culledTargets.OrderByDescending(t => t.GetHpPercent()).ToList<CharacterData>();
                else if (inComparison == SodaScriptComparison.LESS_THAN)
                    culledTargets = culledTargets.OrderBy(t => t.GetHpPercent()).ToList<CharacterData>();
            }
            else if(inCondition == SodaScriptCondition.MP)
            {
                if (inComparison == SodaScriptComparison.GREATER_THAN)
                    culledTargets = culledTargets.OrderByDescending(t => t.GetMpPercent()).ToList<CharacterData>();
                else if (inComparison == SodaScriptComparison.LESS_THAN)
                    culledTargets = culledTargets.OrderBy(t => t.GetMpPercent()).ToList<CharacterData>();
            }
        }

        potentialTargets.Clear();
        potentialTargets.AddRange(culledTargets);
    }

    private CharacterData GetLowestHpPercentTarget(List<CharacterData> inTargets)
    {
        return inTargets.OrderBy(t => t.GetHpPercent()).First();
    }

    private CharacterData GetHighestHpPercentTarget(List<CharacterData> inTargets)
    {
        return inTargets.OrderByDescending(t => t.GetHpPercent()).First();
    }

    private CharacterData GetTargetHpPercentEquals(List<CharacterData> inTargets, int targetPercent)
    {
        for (int i = 0; i < inTargets.Count; i++)
            if (inTargets[i].GetHpPercent() == targetPercent)
                return inTargets[i];

        return null;
    }

    private CharacterData GetLowestMpPercentTarget(List<CharacterData> inTargets)
    {
        return inTargets.OrderBy(t => t.GetMpPercent()).First();
    }

    private CharacterData GetHighestMpPercentTarget(List<CharacterData> inTargets)
    {
        return inTargets.OrderByDescending(t => t.GetMpPercent()).First();
    }

    private CharacterData GetTargetMpPercentEquals(List<CharacterData> inTargets, int targetPercent)
    {
        for (int i = 0; i < inTargets.Count; i++)
            if (inTargets[i].GetMpPercent() == targetPercent)
                return inTargets[i];

        return null;
    }

    private bool EvalNumericComparison(int inA, int inB, SodaScriptComparison inCompare)
    {
        if (inCompare == SodaScriptComparison.LESS_THAN)
            return inA < inB;
        else if (inCompare == SodaScriptComparison.GREATER_THAN)
            return inA > inB;
        else if (inCompare == SodaScriptComparison.EQUALS)
            return inA == inB;
        else if (inCompare == SodaScriptComparison.NOT_EQUALS)
            return inA != inB;

        throw new Exception("Invalid numeric comparison: " + inCompare);
    }

    private bool EvalStringComparison(string inA, string inB, SodaScriptComparison inCompare)
    {
        if (inCompare == SodaScriptComparison.EQUALS)
            return inA == inB;
        else if (inCompare == SodaScriptComparison.NOT_EQUALS)
            return inA != inB;

        throw new Exception("Invalid numeric comparison: " + inCompare);
    }

    public void Execute(CharacterData inActiveCharacter, Adventure inAdventure, SkillCommand inCommandToPopulate)
    {
        //use triggers if they exist instead of regular psuedocode commands
        if(triggers.Count > 0)
        {
            ExecuteTriggers(inActiveCharacter, inAdventure, inCommandToPopulate);
            return;
        }
    }

    public string GetDisplayName()
    {
        if (_isReadonly && name == SCRIPT_DEFAULT_PLAYER)
            return Locale.Get("DEFAULT");
        else
            return name;
    }
}
