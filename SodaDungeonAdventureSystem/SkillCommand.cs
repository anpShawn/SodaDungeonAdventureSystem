
using System.Collections.Generic;

public class SkillCommand : IAdventureCommand
{
    public Skill skill;
    public CharacterData source;
    public List<CharacterData> targets;

    public SkillCommand()
    {
        targets = new List<CharacterData>();
    }

    public void Reset(Skill inSkill, CharacterData inSource)
    {
        skill = inSkill;
        source = inSource;
        targets.Clear();
    }

    public void AddTarget(CharacterData inTarget)
    {
        targets.Add(inTarget);
    }
}
