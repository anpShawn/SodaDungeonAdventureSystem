
public class Biohazard : FirstAid
{

	public Biohazard():base(SkillId.BIOHAZARD)
    {
    }

    public override SkillResult GenerateResultToTarget(CharacterData inSource, CharacterData inTarget)
    {
        SkillResult result = PoolManager.poolSkillResults.GetNext();
        result.Init(this, inSource, inTarget);

        result.statChanges.Set(Stat.hp, -CalcHpChange(inSource, inTarget) );
        result.ApplyStatusEffectsBasedOnStats(stats);

        return result;
    }
}
