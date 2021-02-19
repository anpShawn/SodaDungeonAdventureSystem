using System;
using System.Text;

[Serializable]public class SodaScriptTrigger
{
    public SodaScriptTarget target;
    public SodaScriptCondition condition;
    public SodaScriptComparison comparison;
    public string compareValue;

    public bool hasSecondaryCondition;
    public SodaScriptCondition secondaryCondition;
    public SodaScriptComparison secondaryComparison;
    public string secondaryCompareValue;

    public SodaScriptSkillCategory skillCategory;
    public string skillId;
    public int percentageChance;

    public SodaScriptTrigger(SodaScriptTarget inTarget, SodaScriptCondition inCondition, SodaScriptComparison inComparison = SodaScriptComparison.EQUALS, string inCompareValue = "")
    {
        target = inTarget;
        condition = inCondition;
        comparison = inComparison;
        compareValue = inCompareValue;
    }

    public void SetSkill(SodaScriptSkillCategory inCategory, string inSkillId="", int inPercentageChance=100)
    {
        skillCategory = inCategory;
        skillId = inSkillId;
        percentageChance = inPercentageChance;
    }

    public void SetSecondaryCondition(SodaScriptCondition inCondition, SodaScriptComparison inComparison = SodaScriptComparison.EQUALS, string inCompareValue = "")
    {
        hasSecondaryCondition = true;
        secondaryCondition = inCondition;
        secondaryComparison = inComparison;
        secondaryCompareValue = inCompareValue;
    }

    public void RemoveSecondaryCondition()
    {
        //we just set this to false, the extra condition data will be ignored on execution
        hasSecondaryCondition = false;
    }

    public SodaScriptTrigger GetCopy()
    {
        SodaScriptTrigger sst = new SodaScriptTrigger(target, condition, comparison, compareValue);

        sst.skillCategory = skillCategory;
        sst.skillId = skillId;
        sst.percentageChance = percentageChance;

        if(hasSecondaryCondition)
        {
            sst.hasSecondaryCondition = hasSecondaryCondition;
            sst.secondaryCondition = secondaryCondition;
            sst.secondaryComparison = secondaryComparison;
            sst.secondaryCompareValue = secondaryCompareValue;
        }

        return sst;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("target: " + target);
        sb.AppendLine("condition: " + condition);
        sb.AppendLine("skillCategory: " + skillCategory);
        return sb.ToString();
    }
}
