using System;

[Flags]
public enum ItemType
{
	WEAPON=1,
    SHIELD=2,
    ARMOR=4,
    ACCESSORY=8,
    ORE=16,
    GEM=32,
    RESOURCE=64,



    ALL= ~0,
    NONE = 0
}
