
public class GameContext
{
    public const float FRAME_TIME_2FPS = 1f / 2;
    public const float FRAME_TIME_4FPS = 1f / 4;
    public const float FRAME_TIME_6FPS = 1f / 6;
    public const float FRAME_TIME_8FPS = 1f / 8;
    public const float FRAME_TIME_10FPS = 1f / 10;
    public const float FRAME_TIME_12FPS = 1f / 12;
    public const float FRAME_TIME_16FPS = 1f / 16;
    public const float FRAME_TIME_18FPS = 1f / 18;
    public const float FRAME_TIME_20FPS = 1f / 20;
    public const float FRAME_TIME_24FPS = 1f / 24;
    public const float FRAME_TIME_30FPS = 1f / 30;
    public const float FRAME_TIME_45FPS = 1f / 45;
    public const float FRAME_TIME_60FPS = 1f / 60;

    public static bool ALLOW_DEBUG;
    public static int DEBUG_DUNGEON_START_LEVEL = 0;
    public static bool DEBUG_INVINCIBLE_CHARACTERS;
    public static bool DEBUG_INFINITE_MP;
    public static bool DEBUG_INVINCIBLE_ENEMIES;
    public static bool DEBUG_WEAK_ENEMIES;
    public static bool DEBUG_MAX_DUNGEON_KEYS;
    public static bool DEBUG_ALWAYS_SPAWN_ESSENCE;
    public static bool DEBUG_CONSTANT_TREASURE;
    public static bool DEBUG_CONSTANT_AMBUSH;
    public static bool DEBUG_USE_TEST_SPAWN;
    public static int DEBUG_TEST_SPAWN_NUM = 1;
    public static bool DEBUG_SHOW_ENEMY_HP;
    public static bool DEBUG_CONSTANT_DOORS;
    public static AltPathResult DEBUG_FORCE_DOOR_RESULT = AltPathResult.NONE;
    public static float DEBUG_BATTLE_SPEED = 1;

    public const int MAX_PARTY_SIZE = 6;
    public const int MAX_NUM_ENEMIES_SUPPPORTED = 500;
    public const int MAX_CAPS = 1000000000;
    public const int MAX_BATTLE_CREDITS = 1440; //minutes in a day
    public const int MAX_EQUIPMENT_QUANTITY = 99;
    public const int MAX_RESOURCE_QUANTITY = 999;
    public const int ESSENCE_GOLD_VALUE = 7000;
    public const int DEFAULT_CREDITS_TO_SPEND = 100;
    public const int MAX_ITEM_LEVEL = 11;
    public const int MAX_ITEM_LEVEL_WITHOUT_ENHANCING = 10;
    public const int MAX_SODA_SCRIPTS = 10;
    public const float BACK_ATTACK_MULTIPLIER = 1.5f;
    public const int SPEED_UNITS_REQUIRED_FOR_TURN = 1000;
    public const int HUNTRESS_DAMAGE_BONUS_LIMIT = 1000;
    public const int KITCHEN_PERCENT_BOOST_PER_LEVEL = 2;

    public const int DARKER_LORD_DIMENSION = 4;
    public const int DARK_LADY_DIMENSION = 6;
    public const int DARK_CEO_DIMENSION = 7;
    public const int LIGHT_LORD_DIMENSION = 8;
    public const int VACATION_LORD_DIMENSION = 9;
    public const int DARKEST_LORD_DIMENSION = 10;
    public const int WARRIORS_DIMENSION = 11;

    //skill-specific values
    public const float SWIFT_METAL_DEFAULT_MULTIPLIER = 2f;
    public const float SWIFT_METAL_POWERED_MULTIPLIER = 4f;
    public const float PICKAXE_AGAINST_STONE_MULTIPLIER = 4f;
    public const float MARK_DAMAGE_MULTIPLIER = 1.5f;
    public const int MARK_TURN_DURATION = 2;
    public const int STUN_TURN_DURATION = 2;
    public const int COOK_MAX_ORE_GAIN = 5;

    //"cache" these string values since they get used so often
    public static string STR_CURRENCY_GOLD = Currency.GOLD.ToString();
    public static string STR_CURRENCY_ESSENCE = Currency.ESSENCE.ToString();
    public static string STR_CURRENCY_CAPS = Currency.CAPS.ToString();

}
