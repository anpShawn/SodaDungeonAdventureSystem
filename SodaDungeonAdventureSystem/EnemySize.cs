using System;
[Flags]

public enum EnemySize
{
    SM = 1,
    MD = 2,
    LG = 4,
    XL = 8,






    MED_OR_SMALLER = SM | MD,
    LG_OR_SMALLER = SM | MD | LG,
    ALL = SM | MD | LG | XL
}
