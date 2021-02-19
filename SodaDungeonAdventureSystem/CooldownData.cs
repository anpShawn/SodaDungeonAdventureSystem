public class CooldownData
{
    public string skillId;
    public int turns;

    public CooldownData(string inSkillId, int inTurns)
    {
        skillId = inSkillId;
        turns = inTurns;
    }
}