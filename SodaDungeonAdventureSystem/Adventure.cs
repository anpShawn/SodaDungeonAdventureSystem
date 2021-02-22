using System.Linq;
using System.Collections.Generic;
using System;

public class Adventure
{
    public DungeonData dungeonData { get; private set; }
    public AdventureArea curArea { get; private set; }
    public CharacterTeam playerTeam { get; private set; }
    public CharacterTeam enemyTeam { get; private set; }
    public long battleNum { get; private set; }
    public long battleNumBasedOn10 { get; private set; }
    public long battleNumBasedOn100 { get; private set; }
    public long battleNumInArea { get; private set; }
    public long dungeonLoops { get; private set; }
    public BigInteger battleLimit { get; private set; }
    public bool battleLimitReached { get; private set; }
    public AdventureInputMode inputMode { get; private set; }
    public long battlesCompleted { get; private set; }
    public long enemiesKilled { get; private set; }
    public DateTime startTime { get; private set; }
    public TimeSpan elapsedTime { get; private set; }
    public BattleManager battleManager { get; private set; }
    public bool isComplete { get; private set; }
    public LootBundle adventureLoot { get; private set; }
    public int keys { get; private set; } //keys used to unlock alt paths doors and special chests
    public AdventureStats adventureStats { get; private set; }
    private SkillCommand cachedSkillCommand;
    private bool initialAreaSet;
    private bool oreCanSpawnForArea;
    private bool oreIsDisabled;
    private long areaEnemyAtkBoost;
    private long areaEnemyHpBoost;
    private long areaMinbossHpBoost;
    private long areaBossHpBoost;
    private bool isAcceptingManualInput;
    private bool isProcessing;
    private bool isPausedExternally;
    private bool insideBattle;
    private int numEnemiesHoldingEssence;
    private int cooldownNegation; //a number to reduce all cooldowns by. calculated at start by finding out how many characters have a static skill that reduces cooldowns
    private SaveFile curFile;
    private AutoKeyAndPathSettings pathSettings;

    private int areaAmbushLimit;
    private int areaAmbushes;
    public bool curBattleIsAmbush { get; private set; }
    private int areaAltPathsEncountered;

    private AltPathResult[] goodPathResults;
    private AltPathResult[] badPathResults;
    private AltPathPortal[] altPathPortals;
    private AltPathPortal altPathSelection;
    public bool curBattleIsAltPath { get; private set; }
    private bool turnNextBattleIntoMineshaft;
    public bool curBattleIsMineshaft { get; private set; }
    public bool justWarped { get; private set; }
    private List<int> warpGates; //levels that cannot be warped past (but can be warped TO)
    private List<CharacterData> charactersAffectedByPathStatusEffect;

    private bool isInsideTreasureRoom;
    private LootBundle treasureLoot;
    private TreasureRoomChoice treasureRoomSelection;
    
    private int stepIndex;
    private List<Action> stepsInAnAdventure;

    public SpawnTable curOreTable { get; private set; }
    private List<Item> areaItemPool;
    private List<Item> battleItemPool;
    private List<Item> megaChestItemPool;

    private List<Item> mythicItemPool;
    private List<Item> legendaryItemPool;
    private List<Item> rareItemPool;
    private List<Item> commonUncommonItemPool;

    public bool partyPetLeveledUp { get; private set; }
    public List<string> classesThatLeveledUp { get; private set; }
    public Dictionary<string, int> skillUseCounts { get; private set; }

    public bool isUsingBattleCredits { get; private set; }
    private int battleCreditsRemaining;
    private int battleCreditsSpent;
    public bool showVerboseOutput;
    private bool AS_INTERNAL_PAUSE = false; //used to show the pause came from inside this class/battle

    public bool isInteractiveCutscene { get; private set; }
    public bool playerSurrendered { get; private set; }

    public List<Item> itemsFoundByBlacksmith;

    //vars to aid with special skill effects, usually from cooking meals
    public int nextOreBonusDrops;
    public int nextEnemyGoldBoostPercent;
    public int nextEnemyEssenceBoostPercent;
    public int nextCritDamageBoostPercent;
    public bool forceWarpAtNextPaths; //can be set to "true" by the foretell skill

    //flags/vars to track special achievements
    public bool goldEarnedNewRecord;
    public bool essenceEarnedNewRecord;
    public bool enemiesKilledNewRecord;
    public bool numBattlesNewRecord;

    //fph
    private int battleStartNum;
    public double fph;
    public double fphIgnoringWarpedLevels;
    
    public event Action<Adventure, AdventureArea> EAreaSet;
    public event Action<Adventure> EInputRequested;
    public event Action<Adventure> EAdventureStarted;
    public event Action<Adventure, AltPathPortal[]> EAltPathsEncountered;
    public event Action<Adventure, AltPathPortal> EAltPathConfirmed;
    public event Action<Adventure> EAdventureKeyUsed;
    public event Action<Adventure, StatusEffectType, List<CharacterData>> EAltPathStatusInflicted;
    public event Action<Adventure, LootBundle> EAltPathTreasure;
    public event Action<Adventure> EBattleStarted;
    public event Action<Adventure, CharacterData, CharacterData> ECharacterTransformed;
    public event Action<Adventure, CharacterData> ECharacterActivated;
    public event Action<Adventure, SkillCommand> ESkillUsed;
    public event Action<Adventure, List<SkillResult>> ESkillResolved;
    //public event Action<Adventure> ETurnEnded;
    public event Action<Adventure, CharacterData, StatusEffect> EStatusProc;
    public event Action<Adventure, CharacterData, StatusEffect> EStatusExpired;
    public event Action<Adventure, CharacterData, long, long> ERegenProc;
    public event Action<Adventure, CharacterData> EFinishCharacterProcs;
    public event Action<Adventure, List<CharacterData>> ECharactersDied;
    public event Action<Adventure, CharacterData, LootBundle, int> EEnemyLootEarned;
    public event Action<Adventure, CharacterTeam> EBattleEnded;
    public event Action<Adventure, int> ETreasureRoomEncountered;
    public event Action<Adventure, LootBundle> ETreasureRoomConfirmed;
    public event Action<Adventure> EAdventureBattleLimitReached;
    public event Action<Adventure> EOfferPlayerTeamRevive;
    public event Action<Adventure> EPlayerTeamRevived;
    public event Action<Adventure, AdventureStats, LootBundle> EAdventureEnded;

    public event Action<Adventure, BattleCreditEvent, string> EBattleCreditEventOccured;

    public Adventure(DungeonId inDungeonId, List<PlayerCharacterData> inPlayerTeamMembers, int inBattleStartNum=1)
    {
        dungeonData = DungeonData.GetDungeon(inDungeonId);
        battleStartNum = inBattleStartNum;
        elapsedTime = TimeSpan.Zero;

        isProcessing = false;
        insideBattle = false;
        isComplete = false;
        isAcceptingManualInput = false;
        cachedSkillCommand = new SkillCommand();

        //cache cur file reference for convenience
        curFile = SaveFile.GetSaveFile();

        pathSettings = new AutoKeyAndPathSettings();

        //create teams. enemy team will be re-populated at each new battle
        playerTeam = new CharacterTeam(1, Faction.PLAYER);
        enemyTeam = new CharacterTeam(2, Faction.ENEMY);

        playerTeam.AddPlayerCharactersFromList(inPlayerTeamMembers);
        playerTeam.PrepareForAdventure();
        playerTeam.CacheActiveSodaScriptReferences();

        //kitchen boost 
        ApplyKitchenBoostToPlayerTeam();

        battleManager = new BattleManager(this);
        battleManager.ECharacterActivated += OnCharacterActivated;
        battleManager.EBattleStarted += OnBattleStarted;
        battleManager.ESkillResolved += OnSkillResolved;
        battleManager.EStatusProc += OnStatusProc;
        battleManager.EStatusExpired += OnStatusExpired;
        battleManager.ERegenProc += OnRegenProc;
        battleManager.EFinishCharacterProcs += OnFinishCharacterProcs;
        battleManager.ECharacterTransformed += OnCharacterTransformed;
        battleManager.ECharactersDied += OnCharactersDied;
        battleManager.EEnemyLootEarned += OnEnemyLootEarned;
        battleManager.EOfferPlayerTeamRevive += OnPlayerTeamReviveOffered;
        battleManager.EPlayerTeamRevived += OnPlayerTeamRevived;
        battleManager.EBattleEnded += OnBattleEnded;
        battleNum = battleStartNum;
        UpdateBattleNumCalculations();

        inputMode = AdventureInputMode.MANUAL;

        battleItemPool = new List<Item>();
        megaChestItemPool = new List<Item>();

        mythicItemPool = new List<Item>();
        legendaryItemPool = new List<Item>();
        rareItemPool = new List<Item>();
        commonUncommonItemPool = new List<Item>();

        adventureLoot = PoolManager.poolLootBundles.GetNext();
        adventureLoot.enforceMaxQuantities = true;

        adventureStats = new AdventureStats();
        adventureStats.dungeonId = inDungeonId;
        adventureStats.playerTeam = playerTeam;

        stepsInAnAdventure = new List<Action>();
        stepsInAnAdventure.Add(GotoBattleOrAltPath);
        stepsInAnAdventure.Add(ConfirmAltPathSelectionIfApplicable);
        stepsInAnAdventure.Add(CheckForAdventureEnd);
        stepsInAnAdventure.Add(CheckForTreasureRoom);
        stepsInAnAdventure.Add(ConfirmTreasureRoomSelectionIfApplicable);
        stepsInAnAdventure.Add(IncrementBattleNumber);
        stepsInAnAdventure.Add(Repeat);
        stepIndex = 0;

        //This type of dungeon may have a hard limit, OR, we have to limit due to story/other progression
        battleLimit = 0;

        if (dungeonData.IsAnArena())
        {
            //REMOVED: setup code for arena challenges
        }
        else if (dungeonData.HasBattleLimit())
        {
            battleLimit = dungeonData.battleLimit;
        }
        else
        {
            if (curFile.DarkLordIsInDungeon(dungeonData.type))
            {
                battleLimit = curFile.GetCurDimension() * 100;
            }
        }

        areaAmbushLimit = 1;
        if (GameContext.DEBUG_CONSTANT_AMBUSH) areaAmbushLimit = 999;

        altPathPortals = new AltPathPortal[3];
        for (int i = 0; i < altPathPortals.Length; i++)
            altPathPortals[i] = new AltPathPortal();

        goodPathResults = new AltPathResult[]{ AltPathResult.BONUS_TREASURE, AltPathResult.HEAL, AltPathResult.WARP, AltPathResult.MINE_SHAFT };
        badPathResults = new AltPathResult[]{ AltPathResult.FLAME_TRAP, AltPathResult.POISON_TRAP, AltPathResult.DROWSY_TRAP }; //need to add drowsy trap
        charactersAffectedByPathStatusEffect = new List<CharacterData>();

        classesThatLeveledUp = new List<string>();
        skillUseCounts = new Dictionary<string, int>();

        if (GameContext.DEBUG_MAX_DUNGEON_KEYS)
            keys = 999;

        itemsFoundByBlacksmith = new List<Item>();

        //set up warp gates
        warpGates = new List<int>();

        if (curFile.DarkLordIsInDungeon(dungeonData.type))
        {
            warpGates.Add(curFile.GetDarkLordChallengeBattleNum());
            warpGates.Add(curFile.GetDarkLordFightBattleNum());
        }

        warpGates.Add(1000000);
        warpGates.Sort(); //just a vanilla low to high sort, no params needed

        //REMOVED: code that would cache/handle various external upgrades, such as locksmithing and the ore blocker oddity
    }

    public void ApplyKitchenBoostToPlayerTeam()
    {
        //HIDDEN: kitchen is an external upgrade
        /*
        int kitchenBoostPercent = GameContext.KITCHEN_PERCENT_BOOST_PER_LEVEL * curFile.GetPurchaseLevel(PurchaseId.KITCHEN);
        if(kitchenBoostPercent > 0)
            playerTeam.BoostMaxHPByPercent(kitchenBoostPercent);
            */
    }

    public void InsertBattleCredits(int inCredits)
    {
        isUsingBattleCredits = true;
        battleCreditsRemaining = inCredits;
        SetInputMode(AdventureInputMode.AUTO);

        //when running on credits, the battle limit is the highest level we've reached without credits
        battleLimit = curFile.GetHighestLevelCompleted(dungeonData.type);
    }

    public void SetInputMode(AdventureInputMode inMode)
    {
        inputMode = inMode;

        //if autocombat is on and we are waiting for manual input, push the adventure back into auto mode by forcing the active player character to attack
        if (inputMode == AdventureInputMode.AUTO && isAcceptingManualInput && battleManager.activeCharacter.IsInFaction(Faction.PLAYER))
        {
            battleManager.RunAutocombatForActiveCharacter();
        }
    }

    public void Process()
    {
        while (isProcessing)
        {
            StepThroughAdventure();
        }
    }

    private void StepThroughAdventure()
    {
        if(showVerboseOutput)
            Console.WriteLine("calling adventure step: " + stepsInAnAdventure[stepIndex].Method.Name );
        stepsInAnAdventure[stepIndex++]();
    }

    private void Repeat()
    {
        stepIndex = 0;
    }

    //battle events that we re-broadcast:
    private void OnBattleStarted()
    {
        elapsedTime = DateTime.Now.Subtract(startTime);
        double hours = elapsedTime.TotalHours;
        if(hours > 0)
        {
            fph = Math.Round((battleNum - battleStartNum) / hours, 0); //the absolute number of floors we have moved including warps
            fphIgnoringWarpedLevels = Math.Round(battlesCompleted / hours, 0); //the number of battles completed, excluding warps
        }

        cooldownNegation = 0;
        for(int i=0; i<playerTeam.members.Count; i++)
        {
            if (!playerTeam.members[i].dead && playerTeam.members[i].HasSkill(SkillId.LEADER))
                cooldownNegation++;
        }
            
        if (EBattleStarted != null)
        {
            EBattleStarted(this);
        }
    }

    private void OnCharacterActivated(CharacterData inCharacter)
    {
        inCharacter.ProcCooldowns();
        if (ECharacterActivated != null) ECharacterActivated(this, inCharacter);
    }

    private void OnSkillResolved(List<SkillResult> inResults)
    {
        SkillResult curResult;
        for(int i=0; i<inResults.Count; i++)
        {
            curResult = inResults[i];

            //use the first entry in the list to mark skill usage counts
            if(i == 0)
            {
                if(curResult.source.IsInFaction(Faction.PLAYER))
                {
                    string skillId = curResult.skill.id;
                    if (skillUseCounts.ContainsKey(skillId))
                    {
                        if (skillUseCounts[skillId] < int.MaxValue)
                            skillUseCounts[skillId]++;
                    }
                    else
                        skillUseCounts.Add(skillId, 1);
                }
            }
        }

        if (ESkillResolved != null) ESkillResolved(this, inResults);
    }

    private void OnStatusProc(CharacterData inData, StatusEffect inEffect)
    {
        if (EStatusProc != null) EStatusProc(this, inData, inEffect);
    }

    private void OnStatusExpired(CharacterData inData, StatusEffect inEffect)
    {
        if (EStatusExpired != null) EStatusExpired(this, inData, inEffect);
    }

    private void OnRegenProc(CharacterData inData, long inHpRegen, long inMpRegen)
    {
        //REMOVED: some stat reporting for regen effects

        if (ERegenProc != null) ERegenProc(this, inData, inHpRegen, inMpRegen);
    }

    private void OnFinishCharacterProcs(CharacterData inData)
    {
        if (EFinishCharacterProcs != null) EFinishCharacterProcs(this, inData);
    }

    private void OnCharacterTransformed(CharacterData inOriginalCharacter, CharacterData inNewCharacter)
    {
        if (ECharacterTransformed != null) ECharacterTransformed(this, inOriginalCharacter, inNewCharacter);
    }

    private void OnCharactersDied(List<CharacterData> inDead)
    {
        int numEnemiesDead = 0;
        for (int i = 0; i < inDead.Count; i++)
        {
            if (inDead[i].curFaction == Faction.ENEMY)
            {
                //track enemy death stats
                numEnemiesDead++;

                if(inDead[i] is EnemyCharacterData)
                {
                    EnemyCharacterData ecd = (EnemyCharacterData)inDead[i];
                    curFile.IncrementEnemyKillCount( ecd.storageId );
                    adventureStats.IncrementEnemyKillCount( ecd.storageId );
                }
            }
            else if(inDead[i].curFaction == Faction.PLAYER)
            {
                //log their death
                CharacterData pcd = inDead[i];
                int teamPos = 1;
                for(int j=0; j<playerTeam.members.Count; j++)
                {
                    if(playerTeam.members[j] == pcd)
                    {
                        teamPos = j + 1;
                        break;
                    }
                }

                LogAdventureDeath(pcd, teamPos, battleNum, pcd.mostRecentSourceOfDamage, pcd.mostRecentSourceOfDamageWasStatusEffect, pcd.mostRecentSkillIdToCauseDamage, pcd.mostRecentSourceOfDamageWasBackAttack, curBattleIsAmbush, pcd.statusEffects);
            }
        }
                
        enemiesKilled += numEnemiesDead;
        if (ECharactersDied != null) ECharactersDied(this, inDead);
    }

    private void OnEnemyLootEarned(CharacterData inCharacter, LootBundle inLoot, int inKeys)
    {
        AddLootBundle(inLoot);
        keys += inKeys;
        if (EEnemyLootEarned != null) EEnemyLootEarned(this, inCharacter, inLoot, inKeys);
    }

    private void OnPlayerTeamReviveOffered()
    {
        //if nobody subscribed to the adventure's revive event, skip. otherwise, broadcast
        if(EOfferPlayerTeamRevive == null)
        {
            battleManager.ReceivePlayerTeamReviveResponse(false);
        }
        else
        {
            //we broadcast the offer and effectively must receive some kind of response for the game to continue
            EOfferPlayerTeamRevive(this);
        }
    }

    private void OnPlayerTeamRevived()
    {
        if (EPlayerTeamRevived != null)
            EPlayerTeamRevived(this);
    }

    public void OnBattleEnded(CharacterTeam inWinningTeam)
    {
        curBattleIsMineshaft = false;
        curBattleIsAmbush = false;
        curBattleIsAltPath = false;

        //REMOVED: end of battle stat reporting

        //when a battle ends we need to take back over from the battlemanager. we broadcast an event that might make us pause
        insideBattle = false;
        if (EBattleEnded != null) EBattleEnded(this, inWinningTeam);

        //if no pause occurred, force resume our progress through the adventure steps
        if(!isPausedExternally)
        {
            isProcessing = true;
            Process();
        }
    }

    //this is a public, global pause that is intended to be used by outside callers
    public void Pause(bool inExternally=true)
    {
        isPausedExternally = inExternally;

        if (insideBattle)
        {
            battleManager.Pause();
        }
        else
        {
            isProcessing = false;
        }
    }

    public void Resume()
    {
        //don't try to resume a completed adventure, the game will lock up
        if (isComplete) return;

        isPausedExternally = false;

        if (insideBattle)
        {
            battleManager.Resume();
        }
        else
        {
            if (!isProcessing)
            {
                isProcessing = true;
                Process();
            }
        }
    }

    public void WaitForManualInput()
    {
        //broadcast event that says we would like some input. although the hud already displays skills at the request of the visual adventure observer
        Pause(AS_INTERNAL_PAUSE);
        if (EInputRequested != null) EInputRequested(this);
        isAcceptingManualInput = true;
    }

    //HIDDEN: this function informs us that the player has manually chosen a skill and then a target to use it on. Requires mouse/touch input.
    /*
    public void OnCharacterClicked(Character inTarget, Skill inActiveSkill)
    {
        if (isAcceptingManualInput)
        {
            //if it's the player's turn, force their active character to attack the enemy that was clicked
            if (battleManager.activeCharacter.IsInFaction(Faction.PLAYER))
            {
                CharacterData clickTarget = inTarget.data;

                if(inActiveSkill.id == SkillId.DEFEND)
                {
                    //the defend skill gets a pass no matter what- it can be activated by clicking on any target
                    //FORCE defend to always target the person who used it
                    clickTarget = battleManager.activeCharacter;
                }
                else
                {
                    //if not defending, make sure the player can use on the character they are targeting
                    CharacterData source = battleManager.activeCharacter;

                    //can't use non-friendly skill on target in own faction
                    if (source.IsInFaction(inTarget.data.curFaction) && !inActiveSkill.isFriendly)
                        return;

                    //can't use friendly skill on target in opposing faction
                    if (!source.IsInFaction(inTarget.data.curFaction) && inActiveSkill.isFriendly)
                        return;

                    //can't use single target skills on self unless allowed
                    if (inActiveSkill.IsTargetType(SkillTarget.SINGLE) && inTarget.data == source && !inActiveSkill.allowsSelfTarget)
                        return;
                }

                cachedSkillCommand.Reset(inActiveSkill, battleManager.activeCharacter);
                cachedSkillCommand.AddTarget(clickTarget);
                ReceiveCommand(cachedSkillCommand);
            }
        }
    }*/

    public void OnAltPathClicked(AltPathPortal inPortal)
    {
        altPathSelection = inPortal;
    }

    public void OnTreasureChestClicked(TreasureRoomChoice inChoice)
    {
        treasureRoomSelection = inChoice;
    }

    public void OnTeamReviveAccepted()
    {
        LogAdventureMessage(Locale.Get("LOG_REVIVE", Utils.CommaFormatStringNumber(battleNum.ToString())), "icon_log_start");
        battleManager.ReceivePlayerTeamReviveResponse(true);
    }

    public void OnTeamReviveDeclined()
    {
        battleManager.ReceivePlayerTeamReviveResponse(false);
    }

    //this command can be user or ai-generated
    public void ReceiveCommand(IAdventureCommand inCommand)
    {
        isAcceptingManualInput = false;

        SkillCommand skillCommand = inCommand as SkillCommand;
        Skill skill = skillCommand.skill;

        if (skillCommand != null)
        {
            CharacterData primaryTarget = null;
            CharacterTeam primaryTargetTeam = null;

            if(skillCommand.targets.Count > 0)
            {
                primaryTarget = skillCommand.targets[0];
                primaryTargetTeam = GetTeamForFaction(primaryTarget.curFaction);
            }

            //fix the targets. the command comes with a primary target. but if we target group or otherwise we have to do some work to reflect it
            if(skill.targetType == SkillTarget.ALL)
            {
                //if all, add all other targets that share the primary target's faction
                for(int i=0; i<primaryTargetTeam.members.Count; i++)
                {
                    var mem = primaryTargetTeam.members[i];
                    if (!mem.dead && mem != primaryTarget)
                        skillCommand.AddTarget(mem);
                }
            }
            else if(skill.targetType == SkillTarget.VARIABLE)
            {
                int variableTargetPreference = skillCommand.source.variableTargetPreference;

                if(variableTargetPreference > 1)
                {
                    int maxAdditionalTargets = primaryTargetTeam.GetNumLivingMembers() - 1;
                    int requestedAdditionalTargets = variableTargetPreference - 1;
                    int additionalTargets = requestedAdditionalTargets > maxAdditionalTargets ? maxAdditionalTargets : requestedAdditionalTargets;
                    int additionalTargetsChosen = 0;

                    for(int i=0; i<primaryTargetTeam.members.Count; i++)
                    {
                        var mem = primaryTargetTeam.members[i];
                        if (!mem.dead && mem != primaryTarget)
                        {
                            skillCommand.AddTarget(mem);
                            additionalTargetsChosen++;

                            if (additionalTargetsChosen == additionalTargets)
                                break;
                        }
                    }
                }
            }
            else if(skill.targetType == SkillTarget.SPECIAL)
            {
                //we must make some special targeting decisions for certain skills
                if(skill.id == SkillId.MEGA_CLAW || skill.id == SkillId.DARK_MEGA_CLAW || skill.id == SkillId.ANIME_BLADE || skill.id == SkillId.CUSPATE_BLADE)
                {
                    //add everyone on the same side of the screen as the original target
                    int screenSide = primaryTarget.GetSideOfScreen();
                    for(int i=0; i<primaryTargetTeam.members.Count; i++)
                    {
                        var mem = primaryTargetTeam.members[i];
                        if (!mem.dead && mem != primaryTarget && mem.GetSideOfScreen() == screenSide)
                            skillCommand.AddTarget(mem);
                    }
                }
            }

            //a few skills (like 'wrong number') require special up-front work before playing their animations (like choosing a random number)
            skill.PerformSetupTasks();

            //cooldown? add 1 to each cooldown to negate the fact that they proc at the start of their turn
            if (skill.cooldown > 0)
            {
                int actualCooldown = skill.cooldown - cooldownNegation;
                if(actualCooldown > 0)
                    skillCommand.source.AddCooldown(skill.id, actualCooldown + 1);
            }
                
            battleManager.ReceiveSkillCommand(skillCommand);
            if (ESkillUsed != null) ESkillUsed(this, skillCommand);
        }
    }

    public void Start()
    {
        isProcessing = true;
        if (EAdventureStarted != null) EAdventureStarted(this);
        startTime = DateTime.Now;

        //REMOVED: increment the use count of each character in the party for the game's records (requires save file)

        LogAdventureMessage(Locale.Get("LOG_ADVENTURE_START", Utils.CommaFormatStringNumber(battleNum.ToString())), "icon_log_start");

        Process();
    }

    private void GotoBattleOrAltPath()
    {
        if (isUsingBattleCredits)
        {
            battleCreditsRemaining--;
            battleCreditsSpent++;
        }
            
        curBattleIsAltPath = false;
        bool forcePath = (GameContext.DEBUG_CONSTANT_DOORS && battleNumBasedOn10 > 1 && (battleNumBasedOn10 % 2 != 0));
        bool pathAppearsNaturally = (battleNum > 10 && areaAltPathsEncountered < 2 && (battleNumBasedOn10 == 3 || battleNumBasedOn10 == 6 || battleNumBasedOn10 == 8) && Utils.PercentageChance(35));

        //hard override: we cannot encounter an alt path if we *just* came from a warp
        if(justWarped)
        {
            pathAppearsNaturally = false;
            justWarped = false;
        }

        //also hard override for interactive cutscene and arena
        if (isInteractiveCutscene || dungeonData.IsAnArena())
            pathAppearsNaturally = false;

        if (forcePath || pathAppearsNaturally)
        {
            areaAltPathsEncountered++;
            curBattleIsAltPath = true;
            OfferAltPaths();
        }
        else
            StartNewBattle();
    }

    private void OfferAltPaths()
    {
        altPathSelection = null; //no path by default

        //select a bad path. if under level 100 don't allow the drowsy trap
        AltPathResult badPath = badPathResults.GetRandomElement();
        if (badPath == AltPathResult.DROWSY_TRAP && battleNum < 100)
            badPath = AltPathResult.POISON_TRAP;

        //generate the path options
        altPathPortals[0].UpdateInfo(goodPathResults.GetRandomElement(), false);
        altPathPortals[1].UpdateInfo(badPath, false);
        altPathPortals[2].UpdateInfo(goodPathResults.GetRandomElement(), true);

        //if someone has the excavator skill, % chance to turn the locked good path into the mineshaft
        if(playerTeam.LivingMemberHasSkill(SkillId.EXCAVATOR))
        {
            if (Utils.PercentageChance(30))
                altPathPortals[2].UpdateInfo(AltPathResult.MINE_SHAFT, true);
        }

        //if we're forcing a warp path (foretell has been used) turn the locked good path into it
        if(forceWarpAtNextPaths)
        {
            forceWarpAtNextPaths = false;
            altPathPortals[2].UpdateInfo(AltPathResult.WARP, true);
        }

        //randomize them
        altPathPortals.Shuffle();

        //force debug path? change unlocked portals to the forced result, leave locked as-is
        if(GameContext.DEBUG_FORCE_DOOR_RESULT != AltPathResult.NONE)
        {
            for(int i=0; i<altPathPortals.Length; i++)
            {
                if (!altPathPortals[i].isLocked)
                    altPathPortals[i].UpdateInfo(GameContext.DEBUG_FORCE_DOOR_RESULT, false);
            }
        }

        //reset spawn positions
        SpawnPattern playerSpawnPattern = SpawnPattern.GetStandardPlayerSpawnPatternForTeamSize(playerTeam.members.Count);
        playerTeam.SyncCharactersToSpawnPattern(playerSpawnPattern);

        //broadcast events
        if (EAltPathsEncountered != null)
            EAltPathsEncountered(this, altPathPortals);

        //if nothing is "listening" to the adventure, proceed immediately
        if(isUsingBattleCredits)
        {
            altPathSelection = ChoosePortalBasedOnKeySettings(altPathPortals);

            //cancel selection if warp was chosen, this option is pointless for battle credits
            if (altPathSelection != null && altPathSelection.destination == AltPathResult.WARP)
                altPathSelection = null;

            EnactAltPathSelection();
        }
    }

    private void ConfirmAltPathSelectionIfApplicable()
    {
        if (curBattleIsAltPath)
            EnactAltPathSelection();
    }

    private void EnactAltPathSelection()
    {
        AltPathResult pathResult = AltPathResult.NONE;
        int keysUsed = 0;
        bool choseAnUnknownPath = false;

        if(altPathSelection != null)
        {
            pathResult = altPathSelection.destination;

            //spend key?
            if (altPathSelection.isLocked)
            {
                int keysRequired = 1;
                keys -= keysRequired;
                keysUsed += keysRequired;

                if (EAdventureKeyUsed != null) EAdventureKeyUsed(this);
            }
            else
                choseAnUnknownPath = true;

            //was it a bad path? and does someone have the leader skill? chance to turn it into a good path
            if(badPathResults.Contains(pathResult) && playerTeam.LivingMemberHasSkill(SkillId.LEADER))
            {
                if (Utils.PercentageChance(75))
                {
                    altPathSelection.UpdateInfo(goodPathResults.GetRandomElement(), false);
                    pathResult = altPathSelection.destination;
                } 
            }
        }

        //broadcast
        //BE CAREFUL WITH BROADCASTING THIS EVENT- path-related logic still occurs below here.
        if (EAltPathConfirmed != null)
            EAltPathConfirmed(this, altPathSelection);

        //calc additional things based on the path and broadcast it
        if(pathResult == AltPathResult.FLAME_TRAP)
        {
            AttemptToInflictPathBasedStatusEffect(StatusEffectType.BURN, 3);
        }
        else if(pathResult == AltPathResult.POISON_TRAP)
        {
            AttemptToInflictPathBasedStatusEffect(StatusEffectType.POISON, 3);
        }
        else if(pathResult == AltPathResult.DROWSY_TRAP)
        {
            AttemptToInflictPathBasedStatusEffect(StatusEffectType.SLEEP, 3);
        }
        else if(pathResult == AltPathResult.WARP)
        {
            //increase cur battle level but no need to broadcast an extra event
            int warpAmt = 3 + Utils.RandomInRange(1, 5);

            if(playerTeam.LivingMemberHasSkill(SkillId.PRECOGNITION))
            {
                warpAmt *= 2;
            }

            long newLevel = battleNum + warpAmt;

            //check the warp gates (dark lord appearance, etc)
            int curGate;
            for(int i=0; i<warpGates.Count; i++)
            {
                curGate = warpGates[i];
                if(battleNum < curGate && newLevel > curGate)
                {
                    newLevel = curGate;
                    break;
                }
            }

            justWarped = true;
            battleNum = newLevel;
            UpdateBattleNumCalculations();

            //when warping, force the progress to "repeat" so we don't attempt any treasure or battle increment calculations
            Repeat();
        }
        else if(pathResult == AltPathResult.HEAL)
        {
            //heal team. broadcast any negative status effects removed
            playerTeam.HealAndCureAllMembers();
            battleManager.RemoveExpiredStatusEffects();
        }
        else if(pathResult == AltPathResult.BONUS_TREASURE)
        {
            playerTeam.CalculateTeamBonusStats();

            LootBundle treasureLoot = PoolManager.poolLootBundles.GetNext();
            treasureLoot.condenseLootTypes = false;
            RollTreasureForArea(treasureLoot);

            if (EAltPathTreasure != null)
                EAltPathTreasure(this, treasureLoot);

            AddLootBundle(treasureLoot);
            PoolManager.Recycle(treasureLoot);
        }
        else if(pathResult == AltPathResult.MINE_SHAFT)
        {
            //force next battle to spawn a mine shaft pattern
            turnNextBattleIntoMineshaft = true;
        }

        //REMOVED: stat reporting for alt path choices

        curBattleIsAltPath = false;
    }

    private void AttemptToInflictPathBasedStatusEffect(StatusEffectType inType, int inDuration)
    {
        charactersAffectedByPathStatusEffect.Clear();
        int maxNumCanAfflict = Utils.CeilToInt( (float)playerTeam.GetNumLivingMembers() /2f  );
        int numAfflicted = 0;

        //copy the player team into an array that we can shuffle so that the statuses get inflicted in a truly random order
        CharacterData[] characters = playerTeam.members.ToArray();
        characters.Shuffle();

        CharacterData curMember;
        for(int i=0; i<characters.Length; i++)
        {
            curMember = characters[i];
            if(!curMember.dead && numAfflicted < maxNumCanAfflict)
            {
                if(!curMember.HasItemToPreventStatusEffect(inType) && !curMember.PassesStatusResistCheck(inType) && !curMember.HasSkill(SkillId.PARTY_PROTECTION))
                {
                    numAfflicted++;
                    charactersAffectedByPathStatusEffect.Add(curMember);
                    curMember.ApplyStatusEffect( new StatusEffect(inType, inDuration) );
                }
            }
        }

        if (EAltPathStatusInflicted != null)
            EAltPathStatusInflicted(this, inType, charactersAffectedByPathStatusEffect);
    }

    public void StartNewBattle()
    {
        EnemyCharacterData ecd; //this gets used in various places
        playerTeam.CalculateTeamBonusStats();

        //move to a new area?
        AdventureArea areaForCurBattleNum = dungeonData.GetAreaForBattleNum(battleNum);
        if(!initialAreaSet || curArea != areaForCurBattleNum)
        {
            initialAreaSet = true;
            SetArea(areaForCurBattleNum);
        }

        //update our battle item pool
        battleItemPool.Clear();

        //SIMPLIFIED: normally we search for specific items that meet our current conditions for area and spawn level. 
        //since we only have a few for testing purposes, just always include all of them
        battleItemPool.AddRange(areaItemPool);

        /*
        Item item;
        for(int i=0; i<areaItemPool.Count; i++)
        {
            item = areaItemPool[i];
            if (battleNum >= item.spawnMinLevel)
            {
                //there is always a min, but there may not be a max
                if (item.hasMaxSpawnLevel)
                {
                    if (battleNum <= item.spawnMaxLevel)
                        battleItemPool.Add(item);
                }
                else battleItemPool.Add(item);
            }
        }

        //no items for pool? add stool as a failsafe :/
        if (battleItemPool.Count == 0 && !dungeonData.IsAnArena())
        {
            Console.WriteLine("WARNING: NO ITEM POOL AVAILABLE FOR LEVEL " + battleNum + " IN AREA " + curArea.GetLocalizedName());
            battleItemPool.Add(Item.GetItem(ItemId.STOOL));
        }
        */
            
        SpawnPattern enemySpawnPattern = null;
        curBattleIsAmbush = false;

        //SIMPLIFIED: these bools are all set to false now but in the real game some would be set based on data in the save file
        bool oreCanSpawnForLevel = false;
        bool curBattleIsDungeonBoss = false;
        bool curBattleIsJanitor = false;
        bool curBattleIsJanitorElite = false;
        bool curBattleIsTemperedBoss = false;
        bool curBattleIsDarkestLordIntroCutscene = battleNum == 100 && isInteractiveCutscene;

        /*
        if(battleNum <= 10)
        {
            //REMOVED: code that would select specific "tutorial" spawn patterns for the first ten battles
        }
        */
        if (dungeonData.IsAnArena())
        {
            //REMOVED: code that would create a special spawn pattern for arena levels
        }
        else if(turnNextBattleIntoMineshaft)
        {
            curBattleIsMineshaft = true;
            turnNextBattleIntoMineshaft = false;

            enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.FOUR_ENEMY);
            enemySpawnPattern.ClearSpawns();

            long oreFind = playerTeam.teamBonusStats.Get(Stat.ore_find);
            int qualityBumpChance = oreFind.AsInt() / 15;

            string[] oreIds = new string[4];
            for(int i=0; i<oreIds.Length; i++)
                oreIds[i] = curOreTable.RollSpawn(qualityBumpChance);
            enemySpawnPattern.FillManually(oreIds);
        }
        else if(GameContext.DEBUG_USE_TEST_SPAWN)
        {
            enemySpawnPattern = SpawnPattern.GetTestSpawnPattern(GameContext.DEBUG_TEST_SPAWN_NUM);
        }
        else if(curBattleIsDarkestLordIntroCutscene)
        {
            enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.BOSS);
            enemySpawnPattern.ClearSpawns();
            enemySpawnPattern.FillManually(EnemyId.DARKEST_LORD_LARGE);
        }
        else if(curBattleIsDungeonBoss)
        {
            string charId = GetDungeonBossIdForLevel(battleNum);
            if(charId == CharId.DARK_LORD)enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.ONE_ENEMY);
            else if(charId == EnemyId.VACATION_LORD_LARGE)enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.TWO_ENEMY);
            else enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.BOSS);

            enemySpawnPattern.ClearSpawns();

            //special fill for the vacation lord + grill
            if(charId == EnemyId.VACATION_LORD_LARGE)
                enemySpawnPattern.FillManually(charId, EnemyId.GRILL);
            else
                enemySpawnPattern.FillManually(charId);
        }
        else if(curBattleIsTemperedBoss) //tempered bosses replace janitors so it's important we check for it first
        {
            enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.TEMPERED_BOSS);
            enemySpawnPattern.ClearSpawns();

            string boss = EnemyCharacterData.GetRandomBossEnemyId();
            string minion = EnemyCharacterData.GetRandomSmallEnemyId();

            enemySpawnPattern.FillManually(boss, minion, minion, minion, minion);
        }
        else if(curBattleIsJanitor)
        {
            enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.ONE_ENEMY);
            enemySpawnPattern.ClearSpawns();
            enemySpawnPattern.FillManually(CharId.JANITOR);
        }
        else
        {
            if (dungeonData.IsMinibossLevel(battleNumInArea)) enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.MINIBOSS);
            else if (dungeonData.IsBossLevel(battleNumInArea)) enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.BOSS);
            else
            {
                if (battleNumBasedOn10 == 1) enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.ONE_ENEMY);
                else if (battleNumBasedOn10 == 2 || battleNumBasedOn10 == 3 || battleNumBasedOn10 == 7)enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.TWO_ENEMY);
                else if(battleNumBasedOn10 == 4 || battleNumBasedOn10 == 6 || battleNumBasedOn10 == 8)enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.THREE_ENEMY);
                else enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.FOUR_ENEMY);

                //trigger an ambush?
                /*
                15% ambush chance. Can’t happen on the 1st battle of any set of ten.
                Can only be ambushed once per area
                Can only be ambushed on a level with randomized enemies. This means no bosses, minibosses, or preset levels (1-10)
                Cannot be ambushed if character has the leader skill
                */

                if(battleNumBasedOn10 != 1 && areaAmbushes < areaAmbushLimit && !isInteractiveCutscene)
                {
                    int chance = GameContext.DEBUG_CONSTANT_AMBUSH ? 100 : 15;
                    if(Utils.RandomInRange(0, 100) < chance)
                    {
                        areaAmbushes++;
                        curBattleIsAmbush = true;

                        if (battleNumBasedOn10 <= 5)
                            enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.FOUR_ENEMY_AMBUSH);
                        else
                            enemySpawnPattern = SpawnPattern.GetSpawnPattern(SpawnPatternId.TWO_ENEMY_AMBUSH);
                    }
                }

                //SIMPLIFIED: allowing ore spawns requires additional items and enemies to work correctly, so it has been disabled
                oreCanSpawnForLevel = false;
                //oreCanSpawnForLevel = !curBattleIsAmbush && !isInteractiveCutscene;
            }

            enemySpawnPattern.ClearSpawns();

            //fill the pattern
            EnemyCharacterData.FillSpawnPattern(enemySpawnPattern, battleNum, curArea, GetCurAreaProgressPercent());

            //random chance to have an ore appear
            if(oreCanSpawnForLevel && oreCanSpawnForArea && enemySpawnPattern.spawnPoints.Count > 1)
            {
                int chance = 10;
                //gain up to an additional 10% chance with 100% ore find. for the purpose of this calculation ore find gets capped to 100
                float oreFind = Math.Min(playerTeam.teamBonusStats.Get(Stat.ore_find), 100);
                float additionalChance = (oreFind / 100f) * 10;
                chance += (int)additionalChance;

                if (Utils.PercentageChance(chance))
                {
                    //EnemyCharacterData ore = EnemyCharacterData.RollOreForArea(curArea);
                    EnemyCharacterData ore = EnemyCharacterData.GetEnemyData(curOreTable.RollSpawn());
                    enemySpawnPattern.ChangeRandomSpawnTo(ore);
                }
            }
        }

        //essence spawn?
        bool spawnEssence = false;
        int essenceSpawnChance = 0;
        if (dungeonData.type == DungeonId.CASTLE && curFile.GetCurDimension() >= 2)
        {
            if (!curFile.FlagIsSet(Flag.SEEN_ESSENCE))
                spawnEssence = true;
            else
            {
                if(!curBattleIsDungeonBoss && !curBattleIsJanitor && !curBattleIsJanitorElite && !curBattleIsTemperedBoss)
                {
                    int essenceBaseSpawnChance = 10;
                    if (battleNum > 1000)
                        essenceBaseSpawnChance = 5;
                    else if (battleNum > 500)
                        essenceBaseSpawnChance = 7;

                    essenceSpawnChance = essenceBaseSpawnChance + playerTeam.teamEssenceFindBonus;
                    spawnEssence = Utils.PercentageChance(essenceSpawnChance);
                }
            }
        }
        
        if (GameContext.DEBUG_ALWAYS_SPAWN_ESSENCE)
            spawnEssence = true;

        int maxNumEnemiesHoldingEssence = 1;
        numEnemiesHoldingEssence = 0;

        //TODO temp fix to make the level 100 boss "tempered" (after dark lord leaves d1)
        //bool temperBosses = (battleNum % 100 == 0 && !curFile.DarkLordIsInDungeon(dungeonData.type));

        enemyTeam.RemoveAllMembers();
        CharacterData enemyData;
        List<SpawnPoint> spawns = enemySpawnPattern.spawnPoints;

        for (int i = 0; i < spawns.Count; i++)
        {
            if(!spawns[i].isEmpty)
            {
                enemyData = CharacterData.Get(spawns[i].characterId).GetCopy();
                enemyTeam.AddMember(enemyData);

                if(spawnEssence && numEnemiesHoldingEssence < maxNumEnemiesHoldingEssence && !enemyData.isOre)
                {
                    numEnemiesHoldingEssence++;
                    spawnEssence = false;

                    //give essence based on dungeon level and enemy rank
                    long essenceAmount = Utils.FloorToInt(dungeonLoops / 2);
                    if(enemyData is EnemyCharacterData)
                    {
                        EnemyRank rank = ((EnemyCharacterData)enemyData).rank;
                        if (rank == EnemyRank.MBOS)
                            essenceAmount += 1;
                        else if (rank == EnemyRank.BOSS)
                            essenceAmount += 2;
                    }

                    //add additional essence if our essence find stat is high enough. +1 per each 50 levels after 100
                    if(essenceSpawnChance > 100)
                    {
                        //every 50 extra levels of EF gives +1 essence
                        essenceAmount += (essenceSpawnChance - 100) / 50;
                    }

                    enemyData.GiveEssence(essenceAmount + 1);
                }

                //arena? auto equip them, modify speed, tag as an arena npc
                if(dungeonData.IsAnArena() && enemyData.species == Species.HUMAN)
                {
                    //REMOVED: auto equip and setup arena characters
                }

                //did the dark lord spawn on the enemy team? give him his blade + boost stats
                if(enemyData.id == CharId.DARK_LORD)
                {
                    PlayerCharacterData pcd = (PlayerCharacterData)enemyData;
                    pcd.EquipSlot(EquipmentSlotId.WEAPON_1, ItemId.APHOTIC_BLADE);

                    pcd.baseStats.Set(Stat.hp, 300);
                    pcd.baseStats.Set(Stat.atk, 5);
                    pcd.baseStats.Set(Stat.spd, 110);

                    pcd.SetBaseSkills(SkillId.DISORIENT, SkillId.SHADOW_SLICER);
                }
                else if(enemyData.id == CharId.JANITOR)
                {
                    //match the janitor with whatever boss would normally be here, but ensure he goes first
                    MatchJanitorToDungeonBoss(enemyData, GetDungeonBossIdForLevel(battleNum));
                    enemyData.baseStats.Set(Stat.spd, 110);

                    //give the mop
                    PlayerCharacterData pcd = (PlayerCharacterData)enemyData;
                    pcd.EquipSlot(EquipmentSlotId.WEAPON_1, ItemId.MOP);
                }
                else if(enemyData.id == EnemyId.DARKEST_LORD_LARGE)
                {
                    if(curBattleIsDarkestLordIntroCutscene)
                    {
                        //when spawned into the intro cutscene the darkest lord has increased speed and only one attack
                        enemyData.baseStats.Set(Stat.spd, 200);
                        enemyData.SetBaseSkills(SkillId.ARMAGEDDON);
                    }
                }
            }
        }

        enemyTeam.PrepareForAdventure();

        //if mineshaft, unset all death_optional flags for enemy team
        if(curBattleIsMineshaft)
        {
            enemyTeam.ChangeFlagForAllMembers(CharacterFlags.DEATH_OPTIONAL, false);
        }

        //tempering?
        if(curBattleIsTemperedBoss)
        {
            for(int i=0; i<enemyTeam.members.Count; i++)
            {
                if(enemyTeam.members[i] is EnemyCharacterData)
                {
                    ecd = (EnemyCharacterData)enemyTeam.members[i];
                    if (ecd.rank == EnemyRank.BOSS)
                        ecd.MakeTempered();
                }
            }
        }

        //select player spawn pattern. apply each spawn pattern to their respective teams
        SpawnPattern playerSpawnPattern;
        if(curBattleIsAmbush)
        {
            playerSpawnPattern = SpawnPattern.GetAmbushedPlayerSpawnPatternForTeamSize(playerTeam.members.Count);
        }
        else
        {
            playerSpawnPattern = SpawnPattern.GetStandardPlayerSpawnPatternForTeamSize(playerTeam.members.Count);
        }

        playerTeam.SyncCharactersToSpawnPattern(playerSpawnPattern);
        enemyTeam.SyncCharactersToSpawnPattern(enemySpawnPattern);

        //boost enemy stats here
        for(int i=0; i<enemyTeam.members.Count; i++)
        {
            enemyData = enemyTeam.members[i];

            if(!enemyData.FlagIsSet(CharacterFlags.MINABLE))
            {
                if(dungeonData.IsAnArena())
                {
                    //REMOVED: special scaling for arena enemies
                }
                else
                {
                    enemyData.BoostStartingAtk(areaEnemyAtkBoost);

                    if(enemyData is EnemyCharacterData)
                    {
                        ecd = (EnemyCharacterData)enemyData;
                        if (ecd.rank == EnemyRank.BOSS)
                        {
                            enemyData.BoostStartingHp(areaBossHpBoost);
                            enemyData.stats.Augment(Stat.status_resist, 75);
                        }    
                        else if(ecd.rank == EnemyRank.MBOS)
                        {
                            enemyData.BoostStartingHp(areaMinbossHpBoost);
                            enemyData.stats.Augment(Stat.status_resist, 25);
                        }    
                        else if(ecd.rank == EnemyRank.DBOS)
                        {
                            enemyData.stats.Augment(Stat.status_resist, 90);
                        }
                        else
                            enemyData.BoostStartingHp(areaEnemyHpBoost);
                    }
                }
                
            }
            
            if(GameContext.DEBUG_WEAK_ENEMIES || (isInteractiveCutscene && enemyData.id != EnemyId.DARKEST_LORD_LARGE))
                enemyData.stats.Set(Stat.hp, 1);
        }

        //if ambush, speed up a random enemy
        if(curBattleIsAmbush)
        {
            enemyTeam.members.GetRandomElement().stats.Set(Stat.spd, 155);
        }

        //this is where we hand control over to the battle manager. not a full 'pause,' but we simply stop processing the outside adventure loop for now
        Pause(AS_INTERNAL_PAUSE);
        insideBattle = true;
        battleManager.SetupNewBattle(playerTeam, enemyTeam);
        battleManager.Start();
    }

    private string GetDungeonBossIdForLevel(long inLevel)
    {
        if(inLevel % 100 == 0 && dungeonData.type == DungeonId.CASTLE)
        {
            string charId = EnemyId.RAT;

            //how many times does 100 divide evenly into the level?
            long num = (inLevel % 1000)/100;
            switch(num)
            {
                case 1: charId = CharId.DARK_LORD; break;
                case 2: charId = EnemyId.CHAMELEON; break;
                case 3: charId = EnemyId.LOBSTER; break;
                case 4: charId = EnemyId.DARKER_LORD_LARGE; break;
                case 5: charId = EnemyId.DARK_LORD_MECH; break;
                case 6: charId = EnemyId.DARK_LADY_LARGE; break;
                case 7: charId = EnemyId.PHONE; break;
                case 8: charId = EnemyId.LIGHT_LORD_LARGE; break;
                case 9: charId = EnemyId.VACATION_LORD_LARGE; break;
                case 0: charId = EnemyId.DARKEST_LORD_LARGE; break;
            }

            return charId;
        }
        throw new System.Exception("boss id cannot be retrieved for dungeons that aren't the castle");
    }

    private void MatchJanitorToDungeonBoss(CharacterData inJanitor, string inBossId)
    {
        CharacterData boss = CharacterData.Get(inBossId);
        
        List<string> janitorSkills = new List<string>();
        janitorSkills.Add(SkillId.STRIKE);

        if (inBossId == EnemyId.CHAMELEON)
            janitorSkills.Add(SkillId.MEGA_CLAW);
        else if (inBossId == EnemyId.LOBSTER)
            janitorSkills.Add(SkillId.BUBBLE_CANNON);
        else if (inBossId == EnemyId.DARKER_LORD_LARGE)
            janitorSkills.Add(SkillId.DARK_SLASH);
        else if (inBossId == EnemyId.DARK_LORD_MECH)
        {
            janitorSkills.Add(SkillId.LASER);
            janitorSkills.Add(SkillId.ANIME_BLADE);
        }
        else if (inBossId == EnemyId.DARK_LADY_LARGE)
        {
            janitorSkills.Add(SkillId.FLUSTERSTORM);
            janitorSkills.Add(SkillId.CUSPATE_BLADE);
        }
        else if (inBossId == EnemyId.PHONE)
        {
            janitorSkills.Add(SkillId.DIAL_IT_UP);
            janitorSkills.Add(SkillId.WRONG_NUMBER);
        }
        else if (inBossId == EnemyId.LIGHT_LORD_LARGE)
        {
            janitorSkills.Add(SkillId.MEDITATE);
            janitorSkills.Add(SkillId.SMITE);
        }
        else if (inBossId == EnemyId.VACATION_LORD_LARGE)
            janitorSkills.Add(SkillId.DELUGE);
        else if(inBossId == EnemyId.DARKEST_LORD_LARGE)
        {
            janitorSkills.Add(SkillId.CRYSTAL_TOSS);
            janitorSkills.Add(SkillId.DARK_DREAMS);
        }


        inJanitor.baseStats.Set(Stat.atk, boss.baseStats.Get(Stat.atk));
        inJanitor.baseStats.Set(Stat.hp, boss.baseStats.Get(Stat.hp));

        inJanitor.SetBaseSkills(janitorSkills.ToArray());
    }

    public void CheckForAdventureEnd()
    {
        battleLimitReached = false;
        if(battleLimit > 0)
        {
            //battle limit only counts as being reached if the player team is still alive
            if (!playerTeam.AllMembersDefeated() && battleNum >= battleLimit)
            {
                battleLimitReached = true;

                //is this the arena? if so, give arena rewards
                if(dungeonData.IsAnArena())
                {
                    //REMOVED: arena reward
                }

                if (EAdventureBattleLimitReached != null) EAdventureBattleLimitReached(this);
            }
        }

        //check for end scenarios involving battle credits, and/or broadcast events about normal end scenarios
        if(isUsingBattleCredits && EBattleCreditEventOccured != null)
        {
            if (playerTeam.AllMembersDefeated())
            {
                EBattleCreditEventOccured(this, BattleCreditEvent.PLAYER_TEAM_DEFEATED, "");
            }
            else if(battleLimitReached)
            {
                EBattleCreditEventOccured(this, BattleCreditEvent.BATTLE_LIMIT_REACHED, "");
            }
            else
            {
                //we can check for story events but generally speaking, story events only happen on levels already completed. and if we haven't completed the level, we'll get caught by a battle limit
                //this can be used if story events suddenly appear at a lower level of completion, but right now that doesn't happen
                /*
                long nextLevel = battleNum + 1;
                if(file.DarkLordFightsAtBattle(nextLevel, dungeonData.type) || file.DarkLordIssuesChallengeAtBattle(nextLevel, dungeonData.type))
                {
                    battleLimitReached = true;
                    EBattleCreditEventOccured(this, BattleCreditEvent.STORY_EVENT_REACHED, "");
                }
                */

                //if we made it this far and no event has ended the adventure, also check to see if there are no more battle credits
                if(!battleLimitReached && battleCreditsRemaining<=0)
                {
                    Pause(AS_INTERNAL_PAUSE);
                    EBattleCreditEventOccured(this, BattleCreditEvent.INSERT_CREDITS_TO_CONTINUE, "");
                }
            }
        }

        if (playerTeam.AllMembersDefeated() || battleLimitReached)
        {
            //there are various win/loss conditions but for now, if the player team is dead, the adventure is over
            EndAdventure();
        } 
    }

    private void EndAdventure()
    {
        isComplete = true;
        Pause(AS_INTERNAL_PAUSE);

        LogAdventureMessage(Locale.Get("LOG_ADVENTURE_END", Utils.CommaFormatStringNumber(battleNum.ToString()) ), "icon_log_stop");

        //they completed the battle they are on. but if the enemy team did not die, they must have only completed the battle before
        //NOTE this helps to account for situations in which both teams die at the same time
        long highestCompleteThisAdventure = battleNum;
        if (!enemyTeam.AllMembersDefeated()) highestCompleteThisAdventure--;

        adventureStats.highestLevelComplete = highestCompleteThisAdventure;

        //REMOVED: make sure to wipe any party stored in the save file

        //REMOVED: boost essence gain if the player has unlocked the essence gnome upgrade

        //REMOVED: see if the party pet leveled up

        //REMOVED: check for classes that have leveled up and award them relics if applicable

        //NEW BLACKSMITH ITEMS
        if(!dungeonData.IsAnArena() && !isUsingBattleCredits)
        {
            itemsFoundByBlacksmith.Clear();
            string itemId;
            Item newItem;
            for(int i=0; i<5; i++)
            {
                itemId = RollItemForArea();
                newItem = Item.GetItem(itemId);

                if (newItem.rarity > Rarity.RARE)
                    continue;

                itemsFoundByBlacksmith.Add(newItem.GetCopy());
            }

            //red crystals?
            if(battleNum > 500)
            {
                newItem = Item.GetItem(ItemId.RED_CRYSTAL).GetCopy();
                newItem.AddQuantity(Utils.RandomInRange(1, 11));
                itemsFoundByBlacksmith.Add(newItem);
            }
        }
        
        //BATTLE CREDIT REFUND
        if (isUsingBattleCredits && battleCreditsRemaining > 0 && EBattleCreditEventOccured != null)
            EBattleCreditEventOccured(this, BattleCreditEvent.CREDITS_REFUNDED, battleCreditsRemaining.ToString());

        //REMOVED: report anonymous dungeon stats to server
        
        //REMOVED: record gameplay stats to save file

        //REMOVED: send game file to cloud server for backup
            
        //broadcast
        if (EAdventureEnded != null) EAdventureEnded(this, adventureStats, adventureLoot);
    }

    public int GetMegaChestKeyCost()
    {
        //TODO eventually maybe change this depending how far in the dungeon we are. for now, always 3
        return 3;
    }

    private void CheckForTreasureRoom()
    {
        if(dungeonData.IsTreasureLevel(battleNumInArea))
        {
            isInsideTreasureRoom = true;

            //record the highest level complete every time we hit a treasure room so that autosave properly captures it in case of a game crash
            if(!isUsingBattleCredits)
                curFile.UpdateHighestLevelCompletedIfHigher(dungeonData.type, battleNum);

            OfferTreasureRoom();
        }
    }

    private void OfferTreasureRoom()
    {
        playerTeam.CalculateTeamBonusStats();

        int numChests = 3;
        int numTreasures = 1;

        if (playerTeam.LivingMemberHasSkill(SkillId.GREEDY))
            numTreasures = numChests;

        treasureLoot = PoolManager.poolLootBundles.GetNext();
        treasureLoot.condenseLootTypes = false;

        for(int i=0; i<numTreasures; i++)
            RollTreasureForArea(treasureLoot);

        //don't broadcast the loot here, just the encounter
        if (ETreasureRoomEncountered != null)
            ETreasureRoomEncountered(this, numChests);

        //TODO: if nothing is "listening" to the adventure, force a choice
        if(isUsingBattleCredits)
        {
            //unlike alt paths, there MUST be a treasure choice. in the case of alt paths, "no path" is a valid choice
            treasureRoomSelection = TreasureRoomChoice.NORMAL_CHEST;
            EnactTreasureRoomSelection();
        }
    }

    private void ConfirmTreasureRoomSelectionIfApplicable()
    {
        if (isInsideTreasureRoom) //curbattle is treasure room
            EnactTreasureRoomSelection();
    }

    private void EnactTreasureRoomSelection()
    {
        //the regular treasure loot was already calculated up above, we only stop here to see if we need to swap for a mega chest
        if(treasureRoomSelection == TreasureRoomChoice.MEGA_CHEST)
        {
            //turn the existing loot into mega chest loot
            treasureLoot.Empty();
            string itemId = RollItemForArea(true);
            treasureLoot.AddLoot(LootType.ITEM, itemId, 1);

            //spend the keys
            keys -= GetMegaChestKeyCost();
            if (EAdventureKeyUsed != null)
                EAdventureKeyUsed(this);
        }

        if (ETreasureRoomConfirmed != null)
            ETreasureRoomConfirmed(this, treasureLoot);

        AddLootBundle(treasureLoot);
        PoolManager.Recycle(treasureLoot);

        isInsideTreasureRoom = false;
    }

    private void IncrementBattleNumber()
    {
        battleNum++;
        battlesCompleted++;
        UpdateBattleNumCalculations();
    }

    private void UpdateBattleNumCalculations()
    {
        battleNumBasedOn10 = battleNum % 10;
        battleNumBasedOn100 = battleNum % 100;

        dungeonLoops = battleNum / dungeonData.numBattlesUntilFullLoop;

        battleNumInArea = battleNum % dungeonData.battlesPerArea;
        if (battleNumInArea == 0) battleNumInArea = dungeonData.battlesPerArea; //if the modulus is 0 that means this IS the last battle in the area
    }

    public long BoostGoldByActiveMultipliers(long inAmt, bool inGoldIsComingFromEnemy = false)
    {
        float multiplier = playerTeam.GetGoldFindAsFloatMultiplier();
        if (curBattleIsAmbush) multiplier += .5f;

        //REMOVED: if player has banner ads active, boost gold by an additional amount

        long amt = (long)(inAmt * multiplier);

        //enemy gold boost from chef is multiplicative
        if(inGoldIsComingFromEnemy && nextEnemyGoldBoostPercent > 0)
        {
            multiplier = (nextEnemyGoldBoostPercent/100f);
            nextEnemyGoldBoostPercent = 0;
            amt += (long)(amt * multiplier);
        }

        //lastly, factor in the big gold multiplier
        amt = (long)(amt * playerTeam.GetBigGoldMultiplier());

        if (amt < 0) amt = long.MaxValue;

        return amt;
    }

    private void RollTreasureForArea(LootBundle inBundle)
    {
        if(Utils.PercentageChance(33))
        {
            long battleGoldMultiplier = battleNum / 10;
		    if (battleGoldMultiplier <= 0) battleGoldMultiplier = 1;
		    long gold = 40 * battleGoldMultiplier;

            gold = BoostGoldByActiveMultipliers(gold);
            inBundle.AddGold(gold);
        }
        else
        {
            string itemId = RollItemForArea();
            inBundle.AddLoot(LootType.ITEM, itemId, 1);
        }
    }

    public string RollItemForArea(bool inForMegaChest=false)
    {
        Item itemFound = null;
        Item potentialItem;
        int spawnRate;
        int roll;
        int loops = 0;

        int itemFindBonus = playerTeam.teamItemFindBonus + (curBattleIsAmbush ? 10 : 0);
        if (inForMegaChest) itemFindBonus += 5;

        List<Item> itemPool = inForMegaChest ? megaChestItemPool : battleItemPool;

        //return ItemId.BULLSEYE_SHIELD;
        //Main.Trace(itemPool[0].name + ", " + itemPool[1].name);

        bool rollForRarity = false;
        if(rollForRarity)
        {
            //roll for a specific item pool first, THEN the item
            if (mythicItemPool.Count() > 0 && Utils.OneIn(Math.Max(500 - itemFindBonus, 1)))
            {
                itemPool = mythicItemPool;
            }
            else if (legendaryItemPool.Count() > 0 && Utils.OneIn(Math.Max(300 - itemFindBonus, 1)))
            {
                itemPool = legendaryItemPool;
            }
            else if(rareItemPool.Count() > 0 && Utils.OneIn(Math.Max(100 - itemFindBonus, 1)))
            {
                itemPool = rareItemPool;
            }
            else
            {
                itemPool = commonUncommonItemPool;
            }
        }

        Rarity minRarityToUseItemFindFor = Rarity.RARE;
        if (playerTeam.teamItemFindBonus >= 200) minRarityToUseItemFindFor = Rarity.MYTHIC;
        else if (playerTeam.teamItemFindBonus >= 100) minRarityToUseItemFindFor = Rarity.LEGENDARY;

        while (itemFound == null)
        {
            potentialItem = itemPool.GetRandomElement();
            spawnRate = potentialItem.spawnRate;

            //if using the "roll for rarity" method, we don't need to make any further adjustments to spawn rate since we're specifying an exact rarity group to choose from
            if (rollForRarity)
            {
                spawnRate -= itemFindBonus;
            }
            else
            {
                if (potentialItem.rarity >= minRarityToUseItemFindFor)
                {
                    spawnRate -= itemFindBonus;
                }
            }
                
            if (spawnRate < 1) spawnRate = 1;
            roll = Utils.RandomInRange(1, spawnRate);

            if (roll == 1)
            {
                itemFound = potentialItem;
                break;
            }

            loops++;
            if (loops > 500)
            {
                //too many loops? give them the most common item we can find in the current list
                itemFound = itemPool.OrderBy(i => i.spawnRate).First();
                break;
            }
        }

        return itemFound.id;
    }

    //we filter all loot bundles in through this function so we can both add them to the adventure loot and also the save file directly
    public void AddLootBundle(LootBundle inLoot)
    {
        inLoot.CopyInto(adventureLoot);
        curFile.AddLootBundle(inLoot);
    }

    public void AddLoot(Loot inLoot)
    {
        adventureLoot.AddLoot(inLoot);
        curFile.AddLoot(inLoot);
    }

    private void SetArea(AdventureArea inArea)
    {
        curArea = inArea;
        areaItemPool = Item.GetItemsForArea(curArea);
            
        curOreTable = SpawnTable.GetOreTable(dungeonData.type, battleNum.AsInt());
        oreCanSpawnForArea = (curOreTable != null && !oreIsDisabled && inArea == AdventureArea.CAVERNS); //ore can only spawn natively in the caverns (other areas require a mineshaft)

        long areaNum = dungeonData.GetAreaNumForBattleNum(battleNum);
        areaEnemyHpBoost = (areaNum - 1) * dungeonData.hpIncreasePerArea;
        areaEnemyAtkBoost = (areaNum - 1) * dungeonData.atkIncreasePerArea;

        if(battleNum > 500)
        {
            areaEnemyHpBoost = (long) (areaEnemyHpBoost * 1.2f);
            areaEnemyAtkBoost = (long) (areaEnemyAtkBoost * 1.2f);
        }

        areaMinbossHpBoost = areaEnemyHpBoost * (battleNum > 100 ? 2 : 1);
        areaBossHpBoost = areaEnemyHpBoost * (battleNum > 100 ? 3 : 1);

        areaAmbushes = 0;
        areaAltPathsEncountered = 0;

        if (EAreaSet != null) EAreaSet(this, curArea);
    }

    private int GetCurAreaProgressPercent()
    {
        float fraction = (float)battleNumInArea / dungeonData.battlesPerArea;
        int percent = (int)(fraction * 100);
        return percent;
    }

    public CharacterTeam GetTeamForFaction(Faction inFaction)
    {
        if (inFaction == Faction.ENEMY) return enemyTeam;
        else return playerTeam;
    }

    public CharacterTeam GetTeamOppositeOfFaction(Faction inFaction)
    {
        if (inFaction == Faction.ENEMY) return playerTeam;
        else return enemyTeam;
    }

    public bool EndedDueToBattleLimitReached()
    {
        return (isComplete && battleLimitReached);
    }

    public void LogAdventureMessage(string inMessage, string inIcon)
    {
        //REMOVED: code that generates an adventure log message in a proprietary format.
    }

    public void LogAdventureDeath(CharacterData inCharacter, int inPosition, long inFloor, string inSource, bool inSourceWasStatusProc, string inSkillId, bool inWasBackAttack, bool inWasAmbush, List<StatusEffect> inStatusEffects = null)
    {
        //REMOVED: code that generates a detailed adventure log message with the given parameters. 
        //instead, observers can just listen for the ECharactersDied event to receive basic information
    }

    public void TerminateEarly()
    {
        //normally can only do this on the player's turn, when accepting manual input
        if (battleManager.activeCharacter.curFaction == Faction.PLAYER && isAcceptingManualInput && !battleManager.isProcessing)
        {
            playerTeam.KillAllMembers();
            playerSurrendered = true;

            cachedSkillCommand.Reset(Skill.GetSkill(SkillId.SURRENDER), battleManager.activeCharacter);
            //cachedSkillCommand.AddTarget(enemyTeam.members[0]);
            ReceiveCommand(cachedSkillCommand);
        }

        //special exception when using battle credits
        if(isUsingBattleCredits)
        {
            playerTeam.KillAllMembers();
            EndAdventure();
        }
    }

    public AltPathPortal ChoosePortalBasedOnKeySettings(AltPathPortal[] inPortals)
    {
        AltPathPortal portalChoice = null;
        AltPathPortal lockedPortal = null;

        for(int i=0; i<inPortals.Length; i++)
        {
            if(inPortals[i].isLocked)
            {
                lockedPortal = inPortals[i];
                break;
            }
        }
            
        int keysRequiredToOpen = 1; //the number of keys required for THIS portal
        int keysOwned = keys;

        if(lockedPortal != null)
        {
            AltPathResult portalDestination = lockedPortal.destination;
            if(pathSettings.IsAllowedPath(portalDestination) && keysOwned >= keysRequiredToOpen)
            {
                if(portalDestination == AltPathResult.HEAL)
                {
                    if(  playerTeam.GetTeamHPPercent() < pathSettings.teamHPHealPercent
                        || playerTeam.GetTeamMPPercent() < pathSettings.teamMPHealPercent  )
                    {
                        portalChoice = lockedPortal;
                    }
                }
                else
                {
                    portalChoice = lockedPortal;
                }
            }
        }

        //we made a choice and we have enough keys for it. but do we cancel in order to save keys for healing??
        if(portalChoice != null && portalChoice.destination != AltPathResult.HEAL && pathSettings.saveKeysForHealing)
        {
            int keysNeededForHealing = 1; //high dungeon levels may eventually introduce a higher key cost
            int keysLeftRemaining = keysOwned - keysRequiredToOpen;
            if(keysLeftRemaining < keysNeededForHealing)
            {
                portalChoice = null;
            }
        }
            
        //if we made it here and no path was chosen- should we choose a random one?
        if(portalChoice == null && Utils.PercentageChance(pathSettings.percentageChanceToPickUnknownPath))
        {
            //pick the first non-locked portal we see
            for (int i = 0; i < inPortals.Length; i++)
            {
                if (!inPortals[i].isLocked)
                {
                    portalChoice = inPortals[i];
                }
            }
        }

        return portalChoice;
    }

    public void Cleanup()
    {
        isProcessing = false;
        dungeonData = null;
        enemyTeam.Cleanup();
        playerTeam.Cleanup();
        enemyTeam = playerTeam = null;

        adventureLoot.Empty();
        PoolManager.Recycle(adventureLoot);
        adventureLoot = null;

        EAreaSet = null;
        EInputRequested = null;
        EAdventureStarted = null;
        EAltPathsEncountered = null;
        EAltPathConfirmed = null;
        EAdventureKeyUsed = null;
        EAltPathStatusInflicted = null;
        EAltPathTreasure = null;
        EBattleStarted = null;
        ECharacterActivated = null;
        ESkillUsed = null;
        ESkillResolved = null;
        EStatusProc = null;
        EStatusExpired = null;
        ERegenProc = null;
        EFinishCharacterProcs = null;
        ECharactersDied = null;
        EEnemyLootEarned = null;
        EBattleEnded = null;
        ETreasureRoomEncountered = null;
        EAdventureBattleLimitReached = null;
        EAdventureEnded = null;

        battleManager.Cleanup();
    }
}
