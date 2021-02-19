
public class Strike : Skill
{

    //this constructor is so the game can instantiate a base version of strike with no constructor params
	public Strike():base(SkillId.STRIKE)
    {
    }

    //this is for classes inheriting from strike, it allows them to pass down their specific skill id
    public Strike(string inId):base(inId)
    {
    }

    public override SkillResult GenerateResultToTarget(CharacterData inSource, CharacterData inTarget)
    {
        SkillResult result = PoolManager.poolSkillResults.GetNext();
        result.Init(this, inSource, inTarget);

        //strike deals attack damage directly
        result.statChanges.Set(Stat.hp, -CalcHpChange(inSource, inTarget) );
        result.ApplyStatusEffectsBasedOnStats(inSource.stats);

        return result;
    }
}
