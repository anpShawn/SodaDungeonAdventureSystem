
public class Meteor : Skill
{
	public Meteor():base(SkillId.METEOR)
    {
    }

    public override SkillResult GenerateResultToTarget(CharacterData inSource, CharacterData inTarget)
    {
        SkillResult result = PoolManager.poolSkillResults.GetNext();
        result.Init(this, inSource, inTarget);

        result.statChanges.Set(Stat.hp, -CalcHpChange(inSource, inTarget) );

        return result;
    }
}
