using System.Collections.Generic;
using System.Linq;
using System;

public class BattleManager
{
    private Adventure adventure;
    private CharacterTeam playerTeam;
    private CharacterTeam enemyTeam;
    private List<CharacterData> turnQueue;
    private List<CharacterData> charactersInBattle;
    public CharacterData activeCharacter { get; private set; }
    private SkillCommand cachedSkillCommand; //this is a skill that is currently being decided on by a character
    public SkillCommand activeSkillCommand { get; private set; }//this is the skill that was decided on by a character
    private int turnsSinceRegenProc;
    private int turnsUntilRegenProc;
    private int turnsSinceStatusProc;
    private int turnsUntilStatusProc;
    public int curBattleTransmuteCount { get; private set; }
    public bool isProcessing { get; private set; }
    public int turnNum { get; private set; }
    private SaveFile curFile;
    private bool playerTeamHasRevivedOnceThisTrip;

    private SodaScript defaultEnemyScript;
    private SodaScript darkLadyScript;

    public event Action EBattleStarted;
    public event Action<CharacterData, CharacterData> ECharacterTransformed;
    public event Action<CharacterData> ECharacterActivated;
    public event Action<List<SkillResult>> ESkillResolved;
    public event Action<CharacterData, StatusEffect> EStatusProc;
    public event Action<CharacterData, StatusEffect> EStatusExpired;
    public event Action<CharacterData, long, long> ERegenProc;
    public event Action<CharacterData> EFinishCharacterProcs;
    public event Action<List<CharacterData>> ECharactersDied;
    public event Action<CharacterData, LootBundle, int> EEnemyLootEarned;
    public event Action EOfferPlayerTeamRevive;
    public event Action EPlayerTeamRevived;
    public event Action<CharacterTeam> EBattleEnded;

    private List<Action> stepsInATurn;
    private int stepIndex;
    private List<SkillResult> currentSkillResults;
    private List<CharacterData> currentDeadCharacters;
    private List<CharacterData[]> pendingCharacterTransformations; //this is NOT for transmuting. this is for when a character "dies" and turns into another

    public BattleManager(Adventure inAdventure)
    {
        adventure = inAdventure;

        curFile = SaveFile.GetSaveFile();
        
        charactersInBattle = new List<CharacterData>();
        turnQueue = new List<CharacterData>();
        currentSkillResults = new List<SkillResult>();
        currentDeadCharacters = new List<CharacterData>();
        pendingCharacterTransformations = new List<CharacterData[]>();

        cachedSkillCommand = new SkillCommand();

        stepsInATurn = new List<Action>();
            stepsInATurn.Add(EnactPendingCharacterTransformations);
            stepsInATurn.Add(BeginTurn);
            stepsInATurn.Add(CheckForRegenProc);
            stepsInATurn.Add(ActivateNextCharacter);
            stepsInATurn.Add(RequestActiveCharacterInput);
            stepsInATurn.Add(ResolveActiveSkillCommand);
            stepsInATurn.Add(CleanupActiveSkillCommand);
            stepsInATurn.Add(DeactivateActiveCharacter);
            stepsInATurn.Add(CheckForStatusProc);
            stepsInATurn.Add(CheckForDeaths);
            stepsInATurn.Add(EarnLootForDeadEnemies);
            stepsInATurn.Add(EndTurn);
            stepsInATurn.Add(CheckForPlayerTeamRevive);
            stepsInATurn.Add(CheckForBattleEnd);
            stepsInATurn.Add(Repeat);


        defaultEnemyScript = SodaScript.GetPremadeScript(SodaScript.SCRIPT_DEFAULT_ENEMY);
        darkLadyScript = SodaScript.GetPremadeScript(SodaScript.SCRIPT_DARK_LADY);

        playerTeamHasRevivedOnceThisTrip = false;
    }

    public void SetupNewBattle(CharacterTeam inPlayerTeam, CharacterTeam inEnemyTeam)
    {
        turnQueue.Clear();
        currentSkillResults.Clear();
        currentDeadCharacters.Clear();
        activeCharacter = null;
        activeSkillCommand = null;
        isProcessing = false;
        stepIndex = 0;
        turnNum = 0;
        curBattleTransmuteCount = 0;

        playerTeam = inPlayerTeam;
        enemyTeam = inEnemyTeam;
        RefreshCharactersInBattleList();

        playerTeam.PrepareForBattle();
        enemyTeam.PrepareForBattle();

        /*
        Debug.Log("battle order:");
        foreach (CharacterData c in charactersInBattle)
        {
            Debug.Log(c.name + " : " + c.orderInTeam);
        }
        */
    }

    public void RefreshCharactersInBattleList()
    {
        charactersInBattle.Clear();
        charactersInBattle.AddRange(playerTeam.members);
        charactersInBattle.AddRange(enemyTeam.members);
    }

    public void Start()
    {
        isProcessing = true;
        if (EBattleStarted != null) EBattleStarted();
        Process();
    }

    public void Process()
    {
        while(isProcessing)
        {
            StepThroughTurn();
        }
    }

    public void Pause()
    {
        isProcessing = false;
    }

    public void Resume()
    {
        //Main.Trace("battle is resuming");
        if(!isProcessing)
        {
            isProcessing = true;
            Process();
        }
    }

    private void Repeat()
    {
        stepIndex = 0;
    }

    private void StepThroughTurn()
    {
        if(adventure.showVerboseOutput)
            Console.WriteLine("calling battle step: " + stepsInATurn[stepIndex].Method.Name );
        stepsInATurn[stepIndex++]();
    }

    private void EnactPendingCharacterTransformations()
    {
        for(int i=0; i< pendingCharacterTransformations.Count; i++)
        {
            CharacterData c1, c2;
            c1 = pendingCharacterTransformations[i][0];
            c2 = pendingCharacterTransformations[i][1];

            CharacterTeam team = c1.IsInFaction(Faction.PLAYER) ? playerTeam : enemyTeam;
            turnQueue.Remove(c1);
            team.TurnMemberInto(c1, c2);
            RefreshCharactersInBattleList();

            if(GameContext.DEBUG_WEAK_ENEMIES && team == enemyTeam)
                c2.stats.Set(Stat.hp, 1);

            if (ECharacterTransformed != null)
                ECharacterTransformed(c1, c2);
        }

        pendingCharacterTransformations.Clear();
    }

    private void BeginTurn()
    {
        turnNum++;
    }

    private void CheckForRegenProc()
    {
        turnsSinceRegenProc++;
        turnsUntilRegenProc = playerTeam.GetNumLivingMembers() + enemyTeam.GetNumLivingMembers();

        if(turnsSinceRegenProc >= turnsUntilRegenProc)
        {
            turnsSinceRegenProc = 0;
            Stats stats;

            for(int i =0; i<charactersInBattle.Count; i++)
            {
                if(!charactersInBattle[i].dead)
                {
                    stats = charactersInBattle[i].stats;
                    long hpRegen = stats.Get(Stat.hp_regen);
                    long mpRegen = stats.Get(Stat.mp_regen);

                    //if character is magic inept, cancel any mp gain
                    if (charactersInBattle[i].HasSkill(SkillId.MAGIC_INEPT))
                        mpRegen = 0;

                    stats.Augment(Stat.hp, hpRegen);
                    stats.Augment(Stat.mp, mpRegen);

                    //enforcing limits here also takes care of any status effect damage
                    stats.EnforceLimits(); 

                    if(hpRegen >0 || mpRegen >0)
                    {
                        //broadcast this if at least one stat was regen'd
                        if (ERegenProc != null) ERegenProc(charactersInBattle[i], hpRegen, mpRegen);
                    }
                }
            }
        }
    }

    private void ActivateNextCharacter()
    {
        CharacterData nextCharacter = GetNextCharacterInTurnQueue();
        ActivateCharacter(nextCharacter);
    }

    private CharacterData GetNextCharacterInTurnQueue()
    {
        //prune any dead characters from the queue
        turnQueue.RemoveAll(character => character.dead );

        //prune any characters unable to take their turn
        turnQueue.RemoveAll(character => !character.IsAbleToTakeTurn());

        CharacterData nextCharacter = null;
        do
        {
            if(turnQueue.Count == 0)
            {
                BuildTurnQueue();
                SortTurnQueue();
            }

            if (turnQueue.Count > 0)
            {
                nextCharacter = turnQueue[0];
                turnQueue.RemoveAt(0);
            }
        }
        while (nextCharacter == null);

        return nextCharacter;
    }

    private void BuildTurnQueue()
    {
        //when the queue builds it stops as soon as it finds one character able to go. therefore it will add all characters who are able to go at that instant, but no more.
        CharacterData c;
        while(turnQueue.Count == 0)
        {
            for(int i=0; i< charactersInBattle.Count; i++)
            {
                c = charactersInBattle[i];

                if(!c.dead)
                {
                    c.TickSpeedUnits();

                    if (c.IsReadyForTurn())
                    {
                        c.ResetSpeedUnits();

                        //skip turn queue if unable to take turn (sleep, stone, exhausted, etc)
                        if(c.IsAbleToTakeTurn())
                            turnQueue.Add(c);

                        //no matter what, always proc certain things like exhaustion, marked, stunned, etc here
                        //even though they might not be taking a real 'turn' yet
                        c.ProcEffectsThatOnlyOccurOnOwnTurn();
                    }
                }
            }
        }

        RemoveExpiredStatusEffects();
    }

    private void SortTurnQueue()
    {
        if (turnQueue.Count > 1)
        {
            playerTeam.RefreshTeamOrder();
            //enemyTeam.RandomizeTeamOrder();
            turnQueue = ( turnQueue.OrderByDescending(c => c.stats.Get(Stat.spd)).ThenBy(c => c.orderInTeam).ThenBy(c => c.teamsId)).ToList<CharacterData>();
        }

        /*
        Debug.Log("turn queue is now:");
        foreach (CharacterData d in turnQueue)
        {
            Debug.Log(d.name + ", " + d.orderInTeam);
        }
        */
    }

    public void ActivateCharacter(CharacterData inCharacter)
    {
        activeCharacter = inCharacter;
        activeCharacter.Activate();
        if (ECharacterActivated != null) ECharacterActivated(activeCharacter);
    }

    public bool ActiveCharacterIsChefWithAtLeastOneTeammate()
    {
        return activeCharacter.id == CharId.CHEF && playerTeam.GetNumLivingMembers() > 1;
    }

    public void RequestActiveCharacterInput()
    {
        //run special autocombat here for chef but ONLY if he's with his team. otherwise we need to allow the game to pause here so players can exit the dungeon
        if(ActiveCharacterIsChefWithAtLeastOneTeammate())
        {
            //REMOVED: convoluted code that makes the chef choose the appropriate meal/skill to use
        }
        else if (activeCharacter.IsInFaction(Faction.ENEMY) || adventure.inputMode == AdventureInputMode.AUTO)
        {
            RunAutocombatForActiveCharacter();
        }
        else
        {
            adventure.WaitForManualInput();
        }
    }

    public void RunAutocombatForActiveCharacter()
    {
        CharacterTeam opposingTeam = adventure.GetTeamOppositeOfFaction(activeCharacter.curFaction);
        CharacterTeam ownTeam = adventure.GetTeamForFaction(activeCharacter.curFaction);

        SodaScript acScript = null;
        if (activeCharacter.curFaction == Faction.PLAYER)
        {
            acScript = activeCharacter.activeSodaScript;
        }
        else
        {
            if (activeCharacter.id == EnemyId.DARK_LADY_LARGE)
                acScript = darkLadyScript;
            else
                acScript = defaultEnemyScript;
        }

        acScript.Execute(activeCharacter, adventure, cachedSkillCommand);

        //keep track of how many times a team has healed itself in a row. this is used inside of soda script execution to break out of infinite heal loops
        if (cachedSkillCommand.skill.isHealing)
            ownTeam.consecutiveHeals++;
        else
            ownTeam.consecutiveHeals = 0;

        adventure.ReceiveCommand(cachedSkillCommand);
    }

    public void ReceiveSkillCommand(SkillCommand inCommand)
    {
        activeSkillCommand = inCommand;

        //deduct mp 
        if(!GameContext.DEBUG_INFINITE_MP)
            activeSkillCommand.source.SpendMp(activeSkillCommand.skill.mpCost);

        //if it's not a friendly skill always move to face the first target
        if(!activeSkillCommand.skill.isFriendly && activeSkillCommand.targets.Count > 0)
        {
            CharacterData source = activeSkillCommand.source;
            CharacterData target = activeSkillCommand.targets[0];

            float sourceX = source.spawnPosition.x;
            float targX = target.spawnPosition.x;
            if (targX > sourceX && source.direction == Direction.LEFT
            || targX < sourceX && source.direction == Direction.RIGHT) source.FlipDirection();
        }
    }

    public void ResolveActiveSkillCommand()
    {
        SkillResult curResult;
        CharacterData source = activeSkillCommand.source;
        CharacterData curTarget;
        currentSkillResults.Clear();

        //tally up damage that was reflected back at the source
        float accumulatedDamageReflected = 0;
        
        //tally up other properties to be used for stats
        float accumulatedHpChange = 0;
        float accumulatedDamageAbsorbed = 0;
        uint totalCrits = 0;
        uint totalEvades = 0;
        uint totalBackAttacks = 0;
        uint totalDefends =0;

        //we take special actions for the defend and surrender skills
        if(activeSkillCommand.skill.id == SkillId.DEFEND)
        {
            source.isDefending = true;

            curResult = PoolManager.poolSkillResults.GetNext();
            curResult.Init(activeSkillCommand.skill , source, activeSkillCommand.targets[0]);
            currentSkillResults.Add(curResult);
        }
        else if(activeSkillCommand.skill.id == SkillId.SURRENDER)
        {
            //no action taken here; battle will be ended before anything meaningful can happen
        }
        else
        {
            for (int i = 0; i < activeSkillCommand.targets.Count; i++)
            {
                curTarget = activeSkillCommand.targets[i];
                curResult = activeSkillCommand.skill.GenerateResultToTarget(source, curTarget);
                Stats statChanges = curResult.statChanges;

                //make adjustments to the damage based on battle conditions AND the target's stats/conditions
                float hpChange = statChanges.Get(Stat.hp);

                //check this value here before any kind of absorb or defend is applied 
                if (hpChange < 0)
                    curResult.skillAttemptedToInflictDamage = true;

                //REMOVED: add extra damage for huntress kill count, also increase damage if target is "marked"

                //check for special case where pickaxe deals bonus damage to something turned to stone
                if(curTarget.HasStatusEffect(StatusEffectType.STONE) && curResult.skill.id == SkillId.PICKAXE)
                {
                    hpChange *= GameContext.PICKAXE_AGAINST_STONE_MULTIPLIER;
                }

                //back attack? can't back attack mineable ore
                if(curResult.skill.canBackAttack && !curTarget.isOre)
                {
                    if(curTarget.BackIsTurnedTo(source) && !curTarget.IsImmuneToBackAttackBonus() )
                    {
                        hpChange *= GameContext.BACK_ATTACK_MULTIPLIER;
                        curResult.wasBackAttack = true;
                        totalBackAttacks++;
                    }  
                }

                //boost magic attack and phys attacks
                if(curResult.skill.IsType(SkillType.MAGICAL))
                {
                    long magBoost = source.stats.Get(Stat.magic_boost);
                    if(magBoost != 0)
                    {
                        float boostAmt = 1f + (magBoost / 100f);
                        hpChange *= boostAmt;
                    }

                    //hard max for group heal: cannot exceed half of target's max hp
                    if(curResult.skill.id == SkillId.GROUP_HEAL)
                    {
                        long max = curTarget.stats.Get(Stat.max_hp) / 2;
                        if (hpChange > max) hpChange = max;
                    }
                }
                else if(curResult.skill.IsType(SkillType.PHYSICAL))
                {
                    long physBoost = source.stats.Get(Stat.phys_boost);
                    if(physBoost != 0)
                    {
                        float boostAmt = 1f + (physBoost / 100f);
                        hpChange *= boostAmt;
                    }
                }

                //crit. right now just manually check when using the strike skill. can't crit ore
                if(curResult.skill.id == SkillId.STRIKE && !curTarget.isOre)
                {
                    if(activeSkillCommand.source.RollPrimaryCritChance())
                    {
                        hpChange = activeSkillCommand.source.GetPrimaryCritBonusAppliedToDamage(hpChange);
                        curResult.wasCrit = true;
                    }
                }

                if (curResult.wasDoubleCrit) totalCrits += 2;
                else if (curResult.wasCrit) totalCrits++;

                //apply super bonus crit damage?
                if(curResult.wasCrit || curResult.wasDoubleCrit)
                {
                    if(adventure.nextCritDamageBoostPercent > 0)
                    {
                        float boostAmt = 1f + (adventure.nextCritDamageBoostPercent / 100f);
                        hpChange *= boostAmt;
                        adventure.nextCritDamageBoostPercent = 0;
                    }
                }

                bool sourceMissed = false;
                bool targetEvaded = false;

                //additional checks for non-friendly skills: (evade, reduce damage, defend, etc)
                if(!curResult.skill.isFriendly)
                {
                    //damage absorb/reduce?
                    long reduction = curTarget.stats.Get(Stat.dmg_reduction);
                    if (curTarget.HasStatusEffect(StatusEffectType.STONE) && curResult.skill.id != SkillId.PICKAXE)
                        reduction += 50;
                        
                    if(reduction != 0)
                    {
                        float prevHpChange = hpChange;
                        float reductionAmt = 1f - (reduction /100f);
                        hpChange *= reductionAmt;
                        accumulatedDamageAbsorbed += (Math.Abs(prevHpChange) - Math.Abs(hpChange));
                    }

                    //is target defending? cut damage in half
                    if(curTarget.isDefending)
                    {
                        hpChange = hpChange / 2f;
                        totalDefends++;
                    }

                    //if the skill attempted to inflict damage but damage was reduced below 1, make it 1
                    if (curResult.skillAttemptedToInflictDamage && Math.Abs(hpChange) < 1)
                        hpChange = -1;

                    //did the source miss because they were confused?
                    sourceMissed = activeSkillCommand.source.HasStatusEffect(StatusEffectType.CONFUSION) && Utils.PercentageChance(75);

                    //did the target evade successfully? note that they would never evade a friendly attack
                    CharacterTeam targetsTeam = adventure.GetTeamForFaction(curTarget.curFaction);
                    bool targetHasPartyProtection = curTarget.HasSkill(SkillId.PARTY_PROTECTION) && targetsTeam.GetNumLivingMembers() > 1;
                    if (curResult.source.curFaction != curResult.target.curFaction && 
                        (curTarget.PassesEvasionCheck() || targetHasPartyProtection) )
                    {
                        targetEvaded = true;
                        totalEvades++;
                    }
                }

                //is target an ore? they take damage from the ore bust stat instead (min 1)
                if(curTarget.isOre && curResult.skillAttemptedToInflictDamage)
                {
                    //ore bust is a mixture of our existing ore bust stat plus whatever the attack itself specified. it's always at least 1
                    long oreBust = activeSkillCommand.source.stats.Get(Stat.ore_bust);
                    oreBust += statChanges.Get(Stat.ore_bust);
                    oreBust = Math.Max(oreBust, 1);

                    //ore bust is stored as a positive number but must be converted to negative for damage
                    hpChange = -oreBust;
                }

                //finalize damage
                statChanges.Set(Stat.hp, (long)hpChange);

                //debug invincible characters
                if(    (GameContext.DEBUG_INVINCIBLE_CHARACTERS && curTarget.curFaction == Faction.PLAYER)
                    || (GameContext.DEBUG_INVINCIBLE_ENEMIES && curTarget.curFaction == Faction.ENEMY))
                {
                    if(statChanges.Get(Stat.hp) < 0)
                        statChanges.Set(Stat.hp, 0);
                }

                //this line will always knock the target down to 1hp
                //statChanges.Set(Stat.hp, -(curTarget.stats.Get(Stat.max_hp) - 1) );

                if ( sourceMissed || targetEvaded)
                {
                    curResult.SetAsEvaded();
                }
                else
                {
                    curResult.ResolveEffects(adventure);

                    //resolve team effects for the first result (we only do this once because technically all results contain copies of the same team effects
                    if (i == 0) curResult.ResolveTeamEffects(adventure);

                    //NOW THAT THE ATTACK CONNECTED AND WAS RESOLVED:
                    //tally it
                    accumulatedHpChange += hpChange;

                    //damage reflect
                    long damageReflect = curTarget.stats.Get(Stat.dmg_reflection);
                    if(damageReflect > 0 && hpChange < 0)
                    {
                        float reflectValue = (hpChange * (damageReflect/ 100f));
                        accumulatedDamageReflected += Math.Abs(reflectValue);
                    }

                    //dark savior
                    if(curTarget.stats.Get(Stat.hp) == 0)
                    {
                        CharacterData savior = curTarget.GetCurTeam().GetMemberWhoCanUseDarkSaviorThatIsnt(curTarget);
                        if(savior != null)
                        {
                            curTarget.stats.Set(Stat.hp, 1);
                            savior.IncrementDarkSaviorUseCount();
                            curResult.invokedDarkSavior = true;
                        }
                    }

                    //was this a transmute? max one per battle, and must be a common enemy
                    if(curResult.transmutesTarget)
                    {
                        bool isCommonEnemy = false;
                        if(curResult.target is EnemyCharacterData && !curResult.target.isOre)
                        {
                            if (((EnemyCharacterData)curResult.target).rank == EnemyRank.NORM)
                                isCommonEnemy = true;
                        }

                        //special exception for the grill which currently counts as a NORM enemy
                        if (curResult.target.id == EnemyId.GRILL)
                            isCommonEnemy = false;

                        if(isCommonEnemy && !CurBattleTransmuteLimitReached())
                        {
                            curBattleTransmuteCount++;

                            CharacterData newOre = CharacterData.Get(adventure.curOreTable.RollSpawn()).GetCopy();
                            newOre.UnsetFlags(CharacterFlags.DEATH_OPTIONAL); //unset the death optional flag, since the player transmuted them on purpose (it would be weird to instantly end the battle now)
                            curResult.transmuteResult = newOre;

                            curTarget.GetCurTeam().TurnMemberInto(curTarget, newOre);

                            turnQueue.Remove(curTarget);
                            RefreshCharactersInBattleList();
                        }
                        else
                        {
                            curResult.transmutesTarget = false;
                            curResult.failedATransmute = true;
                        }
                    }
                }

                currentSkillResults.Add(curResult);
            }
        }

        //last step is to see if we create an additional result to send reflected damage back to the source
        if(accumulatedDamageReflected > 0 && !source.HasSkill(SkillId.PARTY_PROTECTION))
        {
            curResult = PoolManager.poolSkillResults.GetNext();
            curResult.Init(Skill.GetSkill(SkillId.STRIKE), source, source);
            curResult.statChanges.Set(Stat.hp, -accumulatedDamageReflected);
            curResult.ResolveEffects(adventure);
            currentSkillResults.Add(curResult);
        }

        //REMOVED: some local stat reporting to the save file
        
        if (ESkillResolved != null) ESkillResolved(currentSkillResults);
    }

    public bool CurBattleTransmuteLimitReached()
    {
        return curBattleTransmuteCount >= 1;
    }

    //this is a separate step so that outside listeners have time to process the results as they please before we recycle them
    public void CleanupActiveSkillCommand()
    {
        for(int i=0; i<currentSkillResults.Count; i++)
        {
            PoolManager.Recycle(currentSkillResults[i]);
        }
    }

    public void DeactivateActiveCharacter()
    {
        activeCharacter.Deactivate();
    }

    private void CheckForStatusProc()
    {
        //skip procs if the battle was surrendered
        if (activeSkillCommand.skill.id == SkillId.SURRENDER)
            return;

        turnsSinceStatusProc++;
        turnsUntilStatusProc = playerTeam.GetNumLivingMembers() + enemyTeam.GetNumLivingMembers();

        if(turnsSinceStatusProc >= turnsUntilStatusProc)
        {
            turnsSinceStatusProc = 0;
            List<StatusEffect> effects;
            CharacterData character;
            Stats stats;

            for(int i =0; i<charactersInBattle.Count; i++)
            {
                if(!charactersInBattle[i].dead)
                {
                    character = charactersInBattle[i];
                    stats = character.stats;
                    effects = character.statusEffects;

                    for(int j=0; j<effects.Count; j++)
                    {
                        if(!effects[j].hasExpired && !effects[j].onlyProcsOnOwnTurn)
                        {
                            effects[j].ProcTarget(character);
                            if (EStatusProc != null)
                                EStatusProc(character, effects[j]);
                        }
                        
                        //expired effects used to be removed right here, now it's a separate step
                    }

                    //enforcing limits here also takes care of any status effect damage
                    stats.EnforceLimits(); 

                    if (EFinishCharacterProcs != null)
                        EFinishCharacterProcs(character);
                }
            }

            RemoveExpiredStatusEffects();
        }
    }

    public void RemoveExpiredStatusEffects()
    {
        List<StatusEffect> effects;
        CharacterData character;

        for (int i = 0; i < charactersInBattle.Count; i++)
        {
            if (!charactersInBattle[i].dead)
            {
                character = charactersInBattle[i];
                effects = character.statusEffects;

                for (int j = 0; j < effects.Count; j++)
                {
                    if (effects[j].hasExpired)
                    {
                        effects[j].RemoveFromTarget(character);

                        if (EStatusExpired != null)
                            EStatusExpired(character, effects[j]);
                    }
                }

                character.RemoveExpiredStatusEffects();
            }
        }
    }

    private void CheckForDeaths()
    {
        currentDeadCharacters.Clear();
        CharacterData curCharacter;
        bool vacationLordDied = false;

        for(int i =0; i<charactersInBattle.Count; i++)
        {
            curCharacter = charactersInBattle[i];
            if(!curCharacter.dead && curCharacter.stats.Get(Stat.hp) <= 0)
            {
                bool markCharacterAsDead = true;

                //special logic might apply to certain dead characters
                if (curCharacter.id == EnemyId.VACATION_LORD_LARGE)
                    vacationLordDied = true;
                else if (curCharacter.id == EnemyId.DARKEST_LORD_LARGE)
                {
                    //darkest lord doesn't die but instead transforms into the darkest one
                    markCharacterAsDead = false;
                    CharacterData theDarkestOne = CharacterData.Get(EnemyId.THE_DARKEST_ONE).GetCopy();
                    pendingCharacterTransformations.Add(new CharacterData[] { curCharacter, theDarkestOne });

                    //REMOVED: +1 to enemy kill count in the bestiary
                }

                if(markCharacterAsDead)
                {
                    curCharacter.Die();
                    currentDeadCharacters.Add(curCharacter);
                }
            }
        }

        //TODO special hack: if vacation lord dies, automatically kill any member on their team (the grill)
        if(vacationLordDied)
        {
            for(int i=0; i<enemyTeam.members.Count; i++)
            {
                curCharacter = enemyTeam.members[i];
                if(!curCharacter.dead)
                {
                    curCharacter.Die();
                    currentDeadCharacters.Add(curCharacter);
                }
            }
        }

        //report dead characters
        if(currentDeadCharacters.Count > 0)
        {
            if (ECharactersDied != null) ECharactersDied(currentDeadCharacters);
        }
    }

    private void EarnLootForDeadEnemies()
    {
        //update gold/item find
        playerTeam.CalculateTeamBonusStats();

        //handle loot
        CharacterData curCharacter;
        for (int i = 0; i < currentDeadCharacters.Count; i++)
        {
            curCharacter = currentDeadCharacters[i];
            if (curCharacter.curFaction == Faction.ENEMY)
            {
                EarnLootForCharacter(curCharacter);
            }
        }
    }

    private void EarnLootForCharacter(CharacterData inCharacter)
    {
        //REMOVED: local stat reporting

        if(inCharacter is EnemyCharacterData)
        {
            EnemyCharacterData enemy = (EnemyCharacterData)inCharacter;
            LootBundle enemyLoot = PoolManager.poolLootBundles.GetNext();

            //REMOVED: local stat reporting

            //only earn gold if the enemy isn't ore
            if (!enemy.isOre)
            {
                long goldDrop = enemy.GetGoldValue();
                goldDrop = adventure.BoostGoldByActiveMultipliers(goldDrop, true);
                enemyLoot.AddGold(goldDrop);
            }

            //were they holding essence?
            if (enemy.hasEssence)
            {
                long baseEssence = enemy.GetEssence();
                long extraEssence = 0;

                //if the last skill used was strike, and it was from a player character, and they have an essence reaver equipped, +1 essence
                if( (activeSkillCommand.skill.id == SkillId.STRIKE || activeSkillCommand.skill.id == SkillId.DUAL_STRIKE) && activeSkillCommand.source is PlayerCharacterData)
                {
                    PlayerCharacterData pcd = (PlayerCharacterData)activeSkillCommand.source;
                    int numEssenceReaversEquipped = pcd.NumItemOrVariantEquipped(ItemId.ESSENCE_REAVER);

                    //if the skill was a regular strike make sure we cap this back to 1: even if the character has two equipped they only get credit for both if dual strike was used
                    if (activeSkillCommand.skill.id == SkillId.STRIKE && numEssenceReaversEquipped > 1)
                        numEssenceReaversEquipped = 1;

                    if (numEssenceReaversEquipped > 0)
                        extraEssence += (long)(baseEssence * (numEssenceReaversEquipped * .15));
                }

                //check for essence boost from cook
                if(adventure.nextEnemyEssenceBoostPercent > 0)
                {
                    float extraFraction = (adventure.nextEnemyEssenceBoostPercent/100f);
                    extraEssence += (long)(baseEssence * extraFraction);
                    adventure.nextEnemyEssenceBoostPercent = 0;
                }

                enemyLoot.AddEssence(baseEssence + extraEssence);
            }
                
            int dropChance = enemy.dropRate;
              
            //the default drop rate for enemies is currently 1 in 9
            //100 item find = an extra 10 drop chance (max 10)
            int additionalDropChance = playerTeam.teamItemFindBonus / 10;
            if (additionalDropChance > 10) additionalDropChance = 10;
            dropChance -= additionalDropChance;
            if (dropChance < 1) dropChance = 1;

            if(dropChance != 0)
            {
                //int roll = UnityEngine.Random.Range(1, dropChance);
                if (Utils.OneIn(dropChance))
                {
                    string itemDropId = "";

                    //chance to drop an item from this area. otherwise drop the enemy's material
                    bool enemyDropsAreaItem = Utils.PercentageChance(33);
                    if (enemy.dropRate == 1 || enemy.isOre) enemyDropsAreaItem = false; //ore always drops its material, and a native drop rate of "1" means the enemy is always meant to drop their item (1 in 1 chance)
                    
                    if (enemy.HasItemDrops())
                    {
                        if (enemyDropsAreaItem) itemDropId = adventure.RollItemForArea();
                        else itemDropId = enemy.GetRandomItemDrop();
                    }
                    else
                    {
                        if(enemyDropsAreaItem)itemDropId = adventure.RollItemForArea();
                        else
                        {
                            //no materials and no chance of area item drop? they get NOTHING
                        }
                    }  
                        
                    if(itemDropId != "")
                    {
                        int dropQuantity = 1;
                        if(enemy.isOre)
                        {
                            dropQuantity += adventure.nextOreBonusDrops;
                            adventure.nextOreBonusDrops = 0;
                        }
                        enemyLoot.AddLoot(LootType.ITEM, itemDropId, dropQuantity);
                    } 
                }
            }

            int keysEarned = 0;
            if(curFile.FlagIsSet(Flag.SEEN_ADVENTURE_KEY))
            {
                if (enemy.rank == EnemyRank.MBOS || enemy.rank == EnemyRank.BOSS)
                {
                    int chance = 0;
                    if (adventure.battleNum < 100) chance = 15;
                    else if (adventure.battleNum < 200) chance = 10;
                    else if (adventure.battleNum < 250) chance = 5;

                    if (enemy.rank == EnemyRank.BOSS) chance += 10;

                    chance += adventure.playerTeam.teamKeyBonus;

                    //chance currently caps at 70%
                    if (chance > 70) chance = 70;

                    if (Utils.PercentageChance(chance))
                        keysEarned++;
                }
            }
            else
            {
                //is it time to give them their first key?
                if(adventure.battleNum >= 25 && enemy.rank == EnemyRank.MBOS && curFile.FlagIsSet(Flag.SEEN_ALT_PATH) && !adventure.isInteractiveCutscene)
                {
                    keysEarned = 1;
                }
            }

            //chance to earn an enigmatic fragment/remnant?
            if(enemy.tempered && Utils.OneIn(100))
            {
                enemyLoot.AddLoot(LootType.ITEM, ItemId.ENIGMATIC_SHARD, 1);
            }
            
            //report what loot was earned
            if (EEnemyLootEarned != null) EEnemyLootEarned(inCharacter, enemyLoot, keysEarned);
            
            PoolManager.Recycle(enemyLoot);
        }
    }

    private void EndTurn()
    {
    }

    private void CheckForPlayerTeamRevive()
    {
        bool teamReviveAvailable = false;
        if (playerTeam.AllMembersDefeated() && !enemyTeam.AllMembersDefeated() && !adventure.isInteractiveCutscene && !adventure.dungeonData.IsAnArena() && adventure.battleNum > 10 && !adventure.playerSurrendered && !playerTeamHasRevivedOnceThisTrip)
        {
            //revive is not available when facing the darkest lord or darkest one (final boss fight)
            CharacterData enemy = enemyTeam.members[0];

            if(enemy.id != EnemyId.DARKEST_LORD_LARGE && enemy.id != EnemyId.THE_DARKEST_ONE)
            {
                //REMOVED: team revive is availalbe on platforms that display ads after enough time has passed. just force to false
                teamReviveAvailable = false;
            }
        }

        if(teamReviveAvailable && EOfferPlayerTeamRevive != null)
        {
            Pause();
            EOfferPlayerTeamRevive();
        }
    }

    public void ReceivePlayerTeamReviveResponse(bool inShouldRevive)
    {
        if(inShouldRevive)
        {
            //revive dead team members, then heal+restore both teams
            playerTeam.PrepareForAdventure();
            adventure.ApplyKitchenBoostToPlayerTeam();
            playerTeam.PrepareForBattle();

            enemyTeam.HealAndCureAllMembers();

            //curFile.teamLastRevivedAt = DateTime.Now; //commented out because this property doesn't exist in the mock save file
            playerTeamHasRevivedOnceThisTrip = true;

            if (EPlayerTeamRevived != null)
                EPlayerTeamRevived();
        }

        //no matter what, just resume. either the battle continues to flow or the next step will see the dead team and exit the battle
        Resume();
    }

    private void CheckForBattleEnd()
    {
        CharacterTeam winningTeam = null;
        if (playerTeam.AllMembersDefeated()) winningTeam = enemyTeam;
        else if (enemyTeam.AllMembersDefeated()) winningTeam = playerTeam;

        if(winningTeam != null)
        {
            EndBattle(winningTeam);
        }
    }

    private void EndBattle(CharacterTeam inWinningTeam)
    {
        Pause();

        if(!adventure.isInteractiveCutscene)
        {
            //REMOVED: awarding mastery xp to player team
        }
        
        //REMOVED: increase pet xp

        if (EBattleEnded != null) EBattleEnded(inWinningTeam);
    }

    public void Cleanup()
    {
        isProcessing = false;
        adventure = null;

        turnQueue.Clear();
        charactersInBattle.Clear();
        currentDeadCharacters.Clear();
        turnQueue = charactersInBattle = currentDeadCharacters = null;
        activeCharacter = null;

        currentSkillResults.Clear();
        currentSkillResults = null;
        activeSkillCommand = null;

        defaultEnemyScript = null;

        EBattleStarted = null;
        ECharacterActivated = null;
        ESkillResolved = null;
        EStatusProc = null;
        EStatusExpired = null;
        ERegenProc = null;
        EFinishCharacterProcs = null;
        ECharactersDied = null;
        EEnemyLootEarned = null;
        EBattleEnded = null;
    }
}
