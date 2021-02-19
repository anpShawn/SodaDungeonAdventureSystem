using System;

[Flags]
public enum CharacterFlags
{
    IMMOBILE =1,
    DEATH_OPTIONAL =2,
    MINABLE =4,
    IN_GROUND =8,
    CANT_STRIKE = 16,
    BLUNT_DMG = 32,
    CANT_DEFEND = 64
}