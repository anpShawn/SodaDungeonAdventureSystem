
public class Defend : Skill
{

	public Defend():base(SkillId.DEFEND)
    {
    }

    public override SkillResult GenerateResultToTarget(CharacterData inSource, CharacterData inTarget)
    {
        return null;
    }
}
