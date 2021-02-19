using System;

[Flags]
//default backing type is int32, so 32 flags. 
public enum AdventureArea
{
    CAVERNS = 1,
    JAIL = 2,
    KITCHEN = 4,
    ARMORY = 8,
    THRONE_ROOM = 16,

    ARENA = 32,




    ALL_CASTLE = CAVERNS | JAIL | KITCHEN | ARMORY | THRONE_ROOM
}
