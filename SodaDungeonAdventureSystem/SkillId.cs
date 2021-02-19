using System.Collections.Generic;
using System.Reflection;
using System;

public class SkillId
{
    public const string STRIKE = "STRIKE";
    public const string DEFEND = "DEFEND";

    public const string STRIKE_SWORD_SLASH = "STRIKE_SWORD_SLASH";
    public const string STRIKE_MEDIUM_BLUE = "STRIKE_MEDIUM_BLUE";
    public const string STRIKE_HORIZONTAL_MEDIUM = "STRIKE_HORIZONTAL_MEDIUM";
    public const string STRIKE_HORIZONTAL_MEDIUM_BLUE = "STRIKE_HORIZONTAL_MEDIUM_BLUE";
    public const string STRIKE_LARGE = "STRIKE_LARGE";
    public const string STRIKE_HORIZONTAL_LARGE = "STRIKE_HORIZONTAL_LARGE";
    public const string SURRENDER = "SURRENDER";

    public const string PICKAXE = "PICKAXE";
    public const string FIRST_AID = "FIRST_AID";
    public const string BIOHAZARD = "BIOHAZARD";
    public const string SWIFT_METAL = "SWIFT_METAL";
    public const string TORMENT = "TORMENT";
    public const string PILFER = "PILFER";
    public const string NOXIN = "NOXIN";
    public const string DUAL_STRIKE = "DUAL_STRIKE";
    public const string BURP = "BURP";
    public const string SHARPEN = "SHARPEN";
    public const string GROUP_HEAL = "GROUP_HEAL";
    public const string TRANSMUTE = "TRANSMUTE";
    public const string RECHARGE = "RECHARGE";
    public const string RANSACK = "RANSACK";
    public const string CURSE = "CURSE";
    public const string MARK = "MARK";
    public const string STUN = "STUN";
    public const string COOK = "COOK";
    public const string EVISCERATE = "EVISCERATE";
    public const string FORETELL = "FORETELL";

    public const string HEAL = "HEAL"; //used for healing stone
    public const string PHANTASMAL_CLAW = "PHANTASMAL_CLAW"; //used for phantasmal claw
    public const string SKULL_BASH = "SKULL_BASH"; //used for skull blade
    public const string TECH_LASER = "TECH_LASER"; //used for tech gem
    public const string DARK_FLAME = "DARK_FLAME"; //used for ossein armor
    public const string JUDGEMENT = "JUDGEMENT"; //used for glass warden

    //static skills
    public const string GREEDY = "GREEDY";
    public const string PROSPECTOR = "PROSPECTOR";
    public const string EXCAVATOR = "EXCAVATOR";
    public const string SOUL_BOND = "SOUL_BOND";
    public const string LEADER = "LEADER";
    public const string MAGIC_MASTER = "MAGIC_MASTER";
    public const string MAGIC_INEPT = "MAGIC_INEPT";
    public const string DUAL_WIELD = "DUAL_WIELD";
    public const string DARK_MOMENTUM = "DARK_MOMENTUM";
    public const string DARK_SAVIOR = "DARK_SAVIOR";
    public const string PARTY_PROTECTION = "PARTY_PROTECTION";
    public const string INDEPENDENCE = "INDEPENDENCE";
    public const string PRECOGNITION = "PRECOGNITION";

    //enemy skills
    public const string DREDGE = "DREDGE";
    public const string PYROBLAST = "PYROBLAST";
    public const string INFECT = "INFECT";
    public const string BITE = "BITE";
    public const string CLAW = "CLAW";
    public const string MEGA_CLAW = "MEGA_CLAW";
    public const string CROSSCUT = "CROSSCUT";
    public const string DARK_SLASH = "DARK_SLASH";
    public const string FORK_SLASH = "FORK_SLASH";
    public const string LICK = "LICK";
    public const string WEB_BALL = "WEB_BALL";
    public const string ROCK_TOSS = "ROCK_TOSS";
    public const string BONE_TOSS = "BONE_TOSS";
    public const string DART = "DART";
    public const string KNIFE = "KNIFE";
    public const string WHIRLWIND = "WHIRLWIND";
    public const string DISORIENT = "DISORIENT";
    public const string BOULDER = "BOULDER";
    public const string DOOTS = "DOOTS";
    public const string METEOR = "METEOR";
    public const string DEADLY_GLARE = "DEADLY_GLARE";
    public const string HEAL_2 = "HEAL_2"; //percentage based heal
    public const string STELLAR_BLAST = "STELLAR_BLAST";
    public const string SHADOW_SLICER = "SHADOW_SLICER";
    public const string SHADOW_SLICER_2 = "SHADOW_SLICER_2";
    public const string CAMO = "CAMO";
    public const string BUBBLE_CANNON = "BUBBLE_CANNON";
    public const string LOBSTER_CLAWS = "LOBSTER_CLAWS";
    public const string STOMP_1 = "STOMP_1";
    public const string STOMP_2 = "STOMP_2";
    public const string LASER = "LASER";
    public const string ANIME_BLADE = "ANIME_BLADE";
    public const string FLUSTERSTORM = "FLUSTERSTORM";
    public const string CUSPATE_BLADE = "CUSPATE_BLADE";
    public const string DIAL_IT_UP = "DIAL_IT_UP";
    public const string WRONG_NUMBER = "WRONG_NUMBER";
    public const string SMITE = "SMITE";
    public const string MEDITATE = "MEDITATE";
    public const string DELUGE = "DELUGE";
    public const string GRILL_HEAL = "GRILL_HEAL";

    public const string CRYSTAL_TOSS = "CRYSTAL_TOSS";
    public const string DARK_DREAMS = "DARK_DREAMS";
    public const string DARK_MEGA_CLAW = "DARK_MEGA_CLAW";
    public const string STUN_BOLT = "STUN_BOLT";
    public const string ARMAGEDDON = "ARMAGEDDON";


    private static List<string> validIds;

    public static void CacheIds()
    {
        validIds = new List<string>();

        Type t = typeof(SkillId);
        Type tString = typeof(string);
        FieldInfo[] fields = t.GetFields();
        FieldInfo f;
        for(int i=0; i< fields.Length; i++)
        {
            f = fields[i];
            if(f.IsStatic && f.IsPublic && f.FieldType == tString)
            {
                validIds.Add( (string)f.GetValue(null) );
                //Main.Trace(f.GetValue(null));
            }
        }
    }

    public static bool IsValid(string inId)
    {
        for (int i = 0; i < validIds.Count; i++)
        {
            if (validIds[i] == inId) return true;
        }
            
        return false;
    }

}
