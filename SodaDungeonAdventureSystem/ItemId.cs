using System.Collections.Generic;
using System.Reflection;
using System;

public class ItemId
{
    public const string BOTTLE                  = "1";
    public const string STOOL                   = "2";
    public const string WOODEN_SWORD            = "3";
    public const string BUCKLER                 = "4";
    public const string LEATHER_ARMOR           = "5";
    public const string PELT                    = "6";
    public const string LEATHER                 = "7";
    public const string IRON_ORE                = "8";
    public const string IRON_BAR                = "9";
    public const string GOLDEN_EYE              = "10";
    public const string RED_GEM                 = "11";
    public const string IRON_SWORD              = "12";
    public const string IRON_SHIELD             = "13";
    public const string IRON_ARMOR              = "14";

    public const string COPPER_ORE              = "15";
    public const string SILVER_ORE              = "16";
    public const string GOLD_ORE                = "17";
    public const string PLATINUM_ORE            = "18";
    public const string URANIUM_ORE             = "19";
    public const string JADE_SHARD              = "20";
    public const string FIRESTONE_SHARD         = "21";
    public const string LABRADORITE_SHARD       = "22";
    public const string MALACHITE_SHARD         = "23";
    public const string OPAL_SHARD              = "24";
    public const string AQUAMARINE_SHARD        = "25";
    public const string DIAMOND_SHARD           = "26";
    public const string EMERALD_SHARD           = "27";
    public const string RAINBOWSTONE_SHARD      = "28";
    public const string RUBY_SHARD              = "29";

    public const string COPPER_BAR              = "30";
    public const string SILVER_BAR              = "31";
    public const string GOLD_BAR                = "32";
    public const string PLATINUM_BAR            = "33";
    public const string URANIUM_BAR             = "34";
    public const string JADE                    = "35";
    public const string FIRESTONE               = "36";
    public const string LABRADORITE             = "37";
    public const string MALACHITE               = "38";
    public const string OPAL                    = "39";
    public const string AQUAMARINE              = "40";
    public const string DIAMOND                 = "41";
    public const string EMERALD                 = "42";
    public const string RAINBOWSTONE            = "43";
    public const string RUBY                    = "44";

    public const string WOOD                    = "45";
    public const string STONE                   = "46";
    public const string STEEL                   = "47";
    public const string COAL                    = "48";

    public const string BONE                    = "49";
    public const string LIGHT_GLAND             = "50";
    public const string MUSHROOM                = "51";
    public const string SLIME                   = "52";
    public const string TEETH                   = "53";
    public const string SCALE                   = "54";
    public const string DRAGON_SCALE            = "55";

    public const string GOBLIN_SHIELD           = "56";
    public const string RUSTY_SWORD             = "57";

    public const string INSECT_WING             = "58";
    public const string CANDLE                  = "59";
    public const string KEY                     = "60";
    public const string MEAT                    = "61";
    public const string SILK                    = "62";
    public const string SPECTRAL_DRAUGHT        = "63";

    public const string BONE_CLUB               = "64";
    public const string MACE                    = "65";
    public const string CHAINMAIL               = "66";

    public const string BLUE_GEM                = "67";
    public const string FROG_SHIELD             = "68";

    public const string APPLE                   = "69";
    public const string BACON                   = "70";
    public const string BLUEBERRY_JUICE         = "71";
    public const string CARAMEL                 = "72";
    public const string CARROT                  = "73";
    public const string COG                     = "74";
    public const string EGGPLANT                = "75";
    public const string GLASS_SHARD             = "76";
    public const string RADISH                  = "77";
    public const string DEMONIC_FRAGMENT        = "78";
    public const string FEATHER                 = "79";
    public const string GLUE                    = "80";
    public const string MAGIC_POWDER            = "81";
    public const string METAL_SHARD             = "82";
    public const string SKULL                   = "83";
    public const string STARDUST                = "84";

    public const string MEAT_SUIT               = "85";
    public const string LEATHER_SHIELD          = "86";
    public const string PIG_SHIELD              = "87";
    public const string MEATY_CLUB              = "88";
    public const string BUTCHER_KNIFE           = "89";
    public const string IRON_FIST               = "90";
    public const string MINI_GLAIVE             = "91";

    public const string BLOOD                   = "92";
    public const string LOVE_DUST               = "93";
    public const string DEMON_HORN              = "94";
    public const string POISON                  = "95";

    public const string STONE_OF_SIGHT          = "96";
    public const string HEAL_STONE              = "97";
    public const string ROCK                    = "98";

    public const string APHOTIC_BLADE           = "99";

    public const string PHANTASMAL_CLAW         = "100";
    public const string BONE_MAIL               = "101";
    public const string FRYING_PAN              = "102";
    public const string SABER                   = "103";
    public const string KNUCKLE_BLADE           = "104";
    public const string BULLSEYE_SHIELD         = "105";
    public const string INFECTED_EDGE           = "106";
    public const string VAMPIRE_SUIT            = "107";

    public const string STEEL_SHIELD            = "108";
    public const string GOLD_SHIELD             = "109";
    public const string ASSASSINS_DAGGER        = "110";
    public const string DARKWING_SHIELD         = "111";
    public const string DEMON_ARMOR             = "112";

    public const string LUCKY_CLOVER            = "113";
    public const string MAGIC_RING              = "114";
    public const string POWER_GLOVE             = "115";

    public const string BLUE_CRYSTAL            = "116";
    public const string RED_CRYSTAL             = "117";
    public const string YELLOW_CRYSTAL          = "118";
    public const string GREEN_CRYSTAL           = "119";

    public const string SKULL_BLADE             = "120";
    public const string PURPLE_GEM              = "121";
    public const string PRISMASHELL_SHIELD      = "122";
    public const string BLADE_OF_VIRTUE         = "123";
    public const string HALLOWED_PLATE          = "124";

    public const string BUTTER                  = "125";
    public const string COPPER_BRACELET         = "126";
    public const string DARK_AMULET             = "127";
    public const string IRON_RING               = "128";
    public const string SILVER_NECKLACE         = "129";

    public const string BACK_PROTECTOR          = "130";
    public const string MOP                     = "131";

    public const string WOODEN_STAFF            = "132";

    public const string AQUA_GEM                = "133";
    public const string GEODE                   = "134";
    public const string ANCIENT_AXE             = "135";
    public const string SPLENDID_STAFF          = "136";

    public const string DEMON_GEM               = "137";
    public const string STAR_GEM                = "138";
    public const string GANTUS_BLADE            = "139";
    public const string STAFF_OF_AWARENESS      = "140";
    public const string OSSEIN_ARMOR            = "141";
    public const string POWER_SHIELD            = "142";

    public const string KARUTA                  = "143";
    public const string EYE_GEM                 = "144";
    public const string STAFF_OF_CHOOSING       = "145";
    public const string ROYAL_SWORD             = "146";

    public const string GOLD_ARMOR              = "147";
    public const string TARGET_GEM              = "148";
    public const string ESSENCE_MAGNET          = "149";
    public const string STOPLIGHT_SHIELD        = "150";
    public const string STAFF_OF_GREED          = "151";
    public const string BUZZBLADE               = "152";

    public const string PEARL_GEM               = "153";
    public const string COSMIC_SHIELD           = "154";
    public const string KEY_SWORD               = "155";

    public const string GRAND_SCEPTER           = "156";
    public const string SOUL_GUARDIAN           = "157";

    public const string OBSIDIAN                = "158";
    public const string SULFUR                  = "159";

    public const string MAGNIFICENT_ROBE        = "160";
    public const string RADIOACTIVE_SUIT        = "161";
    public const string PLATINUM_ARMOR          = "162";
    public const string GOLD_GEM                = "163";
    public const string SPACE_GEM               = "164";
    public const string TRIANGLE_GEM            = "165";
    public const string TECH_GEM                = "166";
    public const string SHIELD_OF_THE_DIVINE    = "167";
    public const string GLASS_WARDEN            = "168";
    public const string GOLD_SWORD              = "169";
    public const string ESSENCE_REAVER          = "170";

    public const string BIB                     = "171";
    public const string FORK                    = "172";
    public const string SPOON                   = "173";
    public const string ROOSTER_HEAD            = "174";
    public const string ENIGMATIC_SHARD         = "175";
    public const string FANG                    = "176";
    public const string EYE                     = "177";

    public const string IRON_SKILLET            = "178";
    public const string SLOW_STONE              = "179";
    public const string GANALAR                 = "180";




    private static List<string> validIds;

    public static void CacheIds()
    {
        validIds = new List<string>();

        Type t = typeof(ItemId);
        Type tString = typeof(string);
        FieldInfo[] fields = t.GetFields();
        FieldInfo f;
        for(int i=0; i< fields.Length; i++)
        {
            f = fields[i];
            if(f.IsStatic && f.IsPublic && f.FieldType == tString)
            {
                validIds.Add( (string)f.GetValue(null) );
            }
        }
    }

    public static bool IsValid(string inId)
    {
        for (int i = 0; i < validIds.Count; i++)
            if (validIds[i] == inId) return true;
        return false;
    }

    public static string ExtractBaseId(string inId)
    {
        //leveled item ids are id_level: 180_1
        //plated item ids are id_e_platingtype_uniqueid: 180_e_32_999

        if (inId.Contains("_"))
            return inId.Split('_')[0];
        else return inId;
    }

    public static bool IsEnhancedItemId(string inId)
    {
        return inId.Contains("_e_");
    }

    public static string ExtractPlatingTypeFromEnhancedItemId(string inId)
    {
        return inId.Split('_')[2];
    }
}
