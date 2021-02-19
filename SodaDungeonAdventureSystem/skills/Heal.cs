
public class Heal : Skill
{
	public Heal():base(SkillId.HEAL)
    {
    }

    //this is for classes inheriting, it allows them to pass down their specific skill id
    public Heal(string inId) : base(inId)
    {
    }

    public override SkillResult GenerateResultToTarget(CharacterData inSource, CharacterData inTarget)
    {
        SkillResult result = PoolManager.poolSkillResults.GetNext();
        result.Init(this, inSource, inTarget);

        result.statChanges.Set(Stat.hp, CalcHpChange(inSource, inTarget) );

        return result;
    }
}
