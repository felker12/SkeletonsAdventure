using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using RpgLibrary.QuestClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.Quests;
using System.Linq;

namespace SkeletonsAdventure.Entities.PlayerClasses
{
    public class Player : Entity
    {
        public int bonusAttackFromLevel = 0, bonusDefenceFromLevel = 0,
            bonusHealthFromLevel = 0, bonusManaFromLevel = 0,
            bonusAttackFromAttributePoints = 0, bonusDefenceFromAttributePoints = 0,
            bonusHealthFromAttributePoints = 0, bonusManaFromAttributePoints = 0,
            bonusHealthFromEvolution = 0, bonusAttackFromEvolution = 0, 
            bonusDefenceFromEvolution = 0, bonusManaFromEvolution = 0;

        public PlayerEvolutionType EvolutionType { get; private set; } = PlayerEvolutionType.Skeleton;
        private readonly int _levelsPerEvolution = 25;

        public Backpack Backpack { get; set; }
        public EquippedItems EquippedItems { get; set; }
        public int TotalXP { get; private set; } = 0;
        public int Mana { get; set; } = 0;
        public int BaseMana { get; set; } = 0;
        public int MaxMana { get; set; } = 0;
        private static readonly TimeSpan ManaCombatWindow = TimeSpan.FromSeconds(5);
        private const long RecoveryTicksPerMana = TimeSpan.TicksPerSecond * 5;
        private long _manaRecoveryTicks;
        public int AttributePoints { get; set; } = 0;
        public float XPModifier
        {
            get;
            set => field = float.IsFinite(value) && value >= 0
                ? value : throw new ArgumentOutOfRangeException(nameof(value));
        } = 1.0f;
        private bool AimVisible { get; set; } = false;
        public List<Quest> ActiveQuests { get; set; } = [];
        public List<Quest> CompletedQuests { get; set; } = [];
        public string DisplayQuestName { get; private set; } = string.Empty;
        private BasicAttack AttackToAim { get; set; } = null;
        public KillCounter KillCounter { get; private set; } = new();
        public PlayerIndex PlayerIndex { get; set; } = PlayerIndex.One;
        public LearnedAttackManager LearnedAttackManager { get; private set; }
        public KeybindingsManager KeybindingsManager { get; private set; }
        public int AttackExcludingEquipment { get; private set; } = 0;
        public int DefenceExcludingEquipment { get; private set; } = 0;
        public int ManaExcludingEquipment { get; private set; } = 0;
        public int HealhExcludingEquipment { get; private set; } = 0;
        public bool CanEvolve { get; private set; } = false;

        private readonly Texture2D _armoredTexture;
        private readonly GameItem _coinTemplate;
        private readonly IReadOnlyDictionary<string, BasicAttack> _attackTemplates;

        public Player(EntityAssets assets, Texture2D armoredTexture, IReadOnlyDictionary<string, BasicAttack> attackTemplates,
            GameItem coinTemplate) : base(assets)
        {
            _armoredTexture = armoredTexture ?? throw new ArgumentNullException(nameof(armoredTexture));
            _attackTemplates = attackTemplates ?? throw new ArgumentNullException(nameof(attackTemplates));
            _coinTemplate = coinTemplate ?? throw new ArgumentNullException(nameof(coinTemplate));
            TotalXP = 0;
            LearnedAttackManager = new(this, _attackTemplates);
            KeybindingsManager = new(this);
            InitializeBaseStats();
            Initialize();
            SetBonussesFromEvolution();
            UpdateStatsWithBonusses();

            var fireBall = GetAttackByName(nameof(FireBall));
            LearnAttack(fireBall);
            SetKeyBinding(Keys.D1, fireBall);
        }

        private BasicAttack GetAttackByName(string name) =>
            _attackTemplates.TryGetValue(name, out var attack) ? attack.Clone() : null;

        private void InitializeBaseStats()
        {
            BaseAttack = 12;
            BaseDefence = 6;
            BaseHealth = 80;
            BaseMana = 20;
        }

        private void Initialize()
        {
            RespawnTime = 0;
            Health = BaseHealth;
            MaxHealth = BaseHealth;
            Defence = BaseDefence;
            Attack = BaseAttack;
            Speed = 6; //TODO

            MaxMana = BaseMana;
            Mana = BaseMana;

            Backpack = new();
            EquippedItems = new(this);

            //TODO delete this line
            Info.TextColor = Color.Aqua;

            HealthBarVisible = false;
        }

        internal void EnableTestLoadout()
        {
            BaseAttack = 300;
            BaseHealth = 3000;
            BaseMana = 1000;
            UpdateStatsWithBonusses();
            Health = MaxHealth;
            Mana = MaxMana;

            Dictionary<Keys, string> bindings = new()
            {
                [Keys.D1] = nameof(FireBall),
                [Keys.D2] = nameof(IcePillar),
                [Keys.D3] = nameof(IceBullet),
                [Keys.D4] = nameof(WaterBall),
                [Keys.D5] = nameof(FireWave),
                [Keys.D6] = nameof(BlueFireWave),
                [Keys.D7] = nameof(TriangleAttack),
                [Keys.D9] = nameof(IceBullets),
                [Keys.D0] = nameof(SpinningTriangleAttack)
            };
            LearnedAttackManager.UpdateWithData(new() { LearnedAttackNames = [.. bindings.Values] });
            SetKeybinding(bindings);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //spriteBatch.DrawRectangle(Rectangle, SpriteColor, 1, 0); //TODO

            if (AimVisible)
            {
                switch (AttackToAim)
                {
                    case null:
                        break;
                    case ShootingAttack:
                        spriteBatch.DrawLine(GetMousePosition(), Center, Color.White, 1);
                        break;
                    case PopUpAttack attack:
                        Rectangle rect = new((int)(GetMousePosition().X - attack.DamageHitBox.Width / 2),
                            (int)(GetMousePosition().Y - attack.DamageHitBox.Height / 2),
                            attack.DamageHitBox.Width,
                            attack.DamageHitBox.Height);

                        spriteBatch.DrawRectangle(rect, Color.GhostWhite);
                        break;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (CanMove)
                UpdatePlayerMotion();

            CheckInput(gameTime);
            base.Update(gameTime); //keep the update call after updating motion

            Backpack.Update();

            CheckQuestCompleted();
            UpdateStatsWithBonusses();

            KeybindingsManager.Update(gameTime);

            //TODO delete this
            //Info.Text += $"\nXP = {TotalXP}";
            //Info.Text += $"\nAttack = {Attack}\nDefence = {Defence}";
            //Info.Text += $"\nMotion = {Motion}"; 
            //Info.Text += $"\nCurrent Animation = {CurrentAnimation}";

            //Info.Text += "\nFPS = " + (1 / gameTime.ElapsedGameTime.TotalSeconds);
            //Info.Text += "\nActive quests: " + ActiveQuests.Count;

            //if (ActiveQuests.Count > 0 && ActiveQuests[0].ActiveTask != null)
            //    Info.Text += "\nActive task: " + ActiveQuests[0].ActiveTask.ToString();
            //else
            //    Info.Text += "\nActive task: None";

            //Info.Text += $"{Position}";
            //Info.Text += $"\nbonusAttackFromLevel = {bonusAttackFromLevel}";

            //Info.Text += $"\nAttacks Hit by: {AttacksHitBy.Count}";

            //Info.Text += $"\nCan Evolve: {CanEvolve}";
            //Info.Text += $"\nCan Move: {CanMove}";

            Info.Text += $"\nAttacks hit by: {AttacksHitBy.Count}";
        }

        public void UpdatePlayerWithData(PlayerData playerData, Texture2D texture)
        {
            ArgumentNullException.ThrowIfNull(texture);
            EvolutionType = playerData.EvolutionType;
            TotalXP = Math.Clamp(playerData.totalXP, 0, GameManager.XPTable.MaximumXP);

            UpdateEntityWithData(playerData);
            Level = GameManager.XPTable.GetPlayerLevelAtXP(TotalXP);

            BaseMana = playerData.baseMana;
            Mana = playerData.mana;
            MaxMana = playerData.maxMana;
            AttributePoints = playerData.attributePoints;
            // Repair levels missed by older builds at exact XP thresholds, without replaying level-up events.
            int missedLevels = Math.Max(0, Level - Math.Clamp(playerData.EntityLevel, 0, GameManager.XPTable.MaximumLevel));
            AttributePoints = (int)Math.Clamp((long)AttributePoints + missedLevels * 5, 0, int.MaxValue);
            bonusAttackFromAttributePoints = playerData.bonusAttackFromAttributePoints;
            bonusDefenceFromAttributePoints = playerData.bonusDefenceFromAttributePoints;
            bonusHealthFromAttributePoints = playerData.bonusHealthFromAttributePoints;
            bonusManaFromAttributePoints = playerData.bonusManaFromAttributePoints;

            //TODO update active quests and completed quests
            CompletedQuests = playerData.completedQuests.GroupBy(q => q.Name)
                .Select(group => new Quest(group.First()) { Active = false, IsCompleted = true }).ToList();
            ActiveQuests = playerData.activeQuests.Where(q => GetCompletedQuestByName(q.Name) is null)
                .GroupBy(q => q.Name)
                .Select(group => new Quest(group.OrderByDescending(q => q.IsCompleted)
                    .ThenByDescending(q => q.BaseTasksData.Sum(task => task.CompletedAmount)).First())
                    { Active = !group.Any(q => q.IsCompleted) }).ToList();
            DisplayQuestName = playerData.displayQuestName;
            if (GetActiveQuestByName(DisplayQuestName) is null)
                SetDisplayQuestName();

            KillCounter = new KillCounter(playerData.killCounter);

            SetBonussesFromEvolution();
            PlayerStatAdjustmentForLevel();

            Texture = texture;
            UpdateEvolutionTexture();
            CanEvolve = CheckCanEvolve();
        }

        public PlayerData GetPlayerData()
        {
            return new PlayerData(ToData())
            {
                totalXP = TotalXP,
                baseMana = BaseMana,
                mana = Mana,
                maxMana = MaxMana,
                attributePoints = AttributePoints,
                bonusAttackFromAttributePoints = bonusAttackFromAttributePoints,
                bonusDefenceFromAttributePoints = bonusDefenceFromAttributePoints,
                bonusHealthFromAttributePoints = bonusHealthFromAttributePoints,
                bonusManaFromAttributePoints = bonusManaFromAttributePoints,
                backpack = Backpack.ToData(),
                activeQuests = ActiveQuests.ConvertAll(q => q.GetQuestData()),
                completedQuests = CompletedQuests.ConvertAll(q => q.GetQuestData()),
                displayQuestName = DisplayQuestName,
                killCounter = KillCounter.ToData(),
                EvolutionType = EvolutionType,
                TextureName = Texture.Name,
            };
        }

        public void LearnAttack(BasicAttack attack)
        {
            LearnedAttackManager.LearnAttack(attack.Name, attack);
        }

        public void SetKeyBinding(Keys key, BasicAttack attack)
        {
            if (LearnedAttackManager.Contains(attack?.Name) is false)
                return;

            KeybindingsManager.SetKeybinding(key, attack);
        }

        public void SetKeybinding(Dictionary<Keys, string> keybindings)
        {
            foreach (var key in KeybindingsManager.PossibleKeybindings)
                KeybindingsManager.RemoveKeybinding(key);
            foreach (var binding in keybindings)
            {
                var name = binding.Value;
                if (string.IsNullOrEmpty(name))
                    continue;
                if (!LearnedAttackManager.LearnedAttacks.TryGetValue(name, out var attack) ||
                    !KeybindingsManager.SetKeybinding(binding.Key, attack))
                    throw new System.IO.InvalidDataException($"Cannot restore skill binding: {name}");
            }
        }

        internal void RefreshEquipmentStats() => UpdateStatsWithBonusses();

        private protected void UpdateStatsWithBonusses()
        {
            AttackExcludingEquipment = BaseAttack + bonusAttackFromLevel + bonusAttackFromAttributePoints;
            DefenceExcludingEquipment = BaseDefence + bonusDefenceFromLevel + bonusDefenceFromAttributePoints;
            ManaExcludingEquipment = BaseMana + bonusManaFromLevel + bonusManaFromAttributePoints;
            HealhExcludingEquipment = BaseHealth + bonusHealthFromLevel + bonusHealthFromAttributePoints;

            Attack = AttackExcludingEquipment + EquippedItems.EquippedItemsAttackBonus();

            Defence = DefenceExcludingEquipment + EquippedItems.EquippedItemsDefenceBonus();

            MaxHealth = HealhExcludingEquipment; //TODO maybe allow gear to provide a health bonus

            MaxMana = ManaExcludingEquipment; //TODO maybe allow gear to provide a mana bonus

            //add bonusses from evolution
            Attack += bonusAttackFromEvolution;
            Defence += bonusDefenceFromEvolution;
            MaxHealth += bonusHealthFromEvolution;
            MaxMana += bonusManaFromEvolution;
        }

        public void CheckQuestCompleted()
        {
            List<Quest> completed = [];

            if (ActiveQuests != null && ActiveQuests.Count > 0)
            {
                foreach (Quest quest in ActiveQuests)
                {
                    quest.CheckTasksCompleted();
                    if (quest.IsCompleted)
                        completed.Add(quest);
                }
            }

            //if there are any completed quests, remove them from the active quests and
            //add them to the completed quests as well as give the player the rewards
            if (completed.Count > 0)
            {
                foreach (Quest quest in completed)
                {
                    ActiveQuests.Remove(quest);
                    if (GetCompletedQuestByName(quest.Name) is null)
                    {
                        CompletedQuests.Add(quest);
                        GiveQuestReward(quest.Reward);
                        World.AddMessage($"Rewards gained: {quest.Reward}");
                    }
                }
            }

            SetDisplayQuestName();
        }

        public void SetDisplayQuestName()
        {
            if (ActiveQuests.Count > 0)
                DisplayQuestName = ActiveQuests[0].Name;
            else
                DisplayQuestName = string.Empty;
        }

        public void GiveQuestReward(QuestReward reward)
        {
            GainXp(reward.XP);
            if (reward.Coins > 0)
            {
                var coins = _coinTemplate.Clone();
                coins.SetQuantity(reward.Coins);
                if (!Backpack.Add(coins))
                    World.CurrentLevel.EntityManager.DroppedLootManager.Add(coins, Position);
            }

            foreach (GameItem item in reward.Items)
            {
                var granted = item.Clone();
                if (granted is EquipableItem equipment)
                    equipment.SetEquipped(false);
                if (!Backpack.Add(granted))
                    World.CurrentLevel.EntityManager.DroppedLootManager.Add(granted, Position);
            }
        }

        public void AddActiveQuest(Quest quest)
        {
            if (quest is not null && quest.CanStart(this))
            {
                quest.StartQuest();
                ActiveQuests.Add(quest);
                SetDisplayQuestName();
                World.AddMessage($"Quest {quest.Name} started: {quest.Description}");
            }
        }

        internal bool HasLocationTask(QuestLocationAction action, string map, int objectId) =>
            !IsDead && Health > 0 && ActiveQuests.Any(quest => quest.ActiveTask is LocationTask task &&
                task.Action == action && task.MapName == map && task.ObjectId == objectId);

        internal bool ProgressLocationTasks(QuestLocationAction action, string map, Func<int, bool> matchesObject)
        {
            if (IsDead || Health <= 0)
                return false;

            bool progressed = false;
            foreach (var quest in ActiveQuests)
            {
                if (quest.ActiveTask is LocationTask task && task.Action == action && task.MapName == map && matchesObject(task.ObjectId))
                {
                    quest.ProgressTask();
                    progressed = true;
                }
            }
            return progressed;
        }

        public bool TryStartQuest(Quest template)
        {
            if (template is null || !template.CanStart(this))
                return false;
            var quest = template.Clone();
            AddActiveQuest(quest);
            return ActiveQuests.Contains(quest);
        }

        public Quest GetActiveQuestByName(string name)
        {
            foreach (Quest quest in ActiveQuests)
            {
                if (quest.Name == name)
                    return quest;
            }

            return null;
        }

        public Quest GetCompletedQuestByName(string name)
        {
            foreach (Quest quest in CompletedQuests)
            {
                if (quest.Name == name)
                    return quest;
            }

            return null;
        }

        public void GainXp(int XpGained)
        {
            if (XpGained < 0)
                throw new ArgumentOutOfRangeException(nameof(XpGained));
            // Scale before converting to int, and saturate at the final level's threshold.
            TotalXP = (int)Math.Min(GameManager.XPTable.MaximumXP,
                TotalXP + Math.Truncate((double)XpGained * XPModifier));
            int targetLevel = GameManager.XPTable.GetPlayerLevelAtXP(TotalXP);
            while (Level < targetLevel)
            {
                Level++;
                PlayerStatAdjustmentForLevel();
                ApplyLevelUpRewards();
            }
        }

        private void ApplyLevelUpRewards()
        {
            AttributePoints = (int)Math.Min(int.MaxValue, (long)AttributePoints + 5);

            World.AddMessage($"You leveled up! You are now level {Level}!");
            World.AddMessage($"Total Attribute Points: {AttributePoints}");

            //check if the player can evolve
            CanEvolve = CheckCanEvolve();
            Health = MaxHealth;
            Mana = MaxMana;
        }

        protected virtual bool CheckCanEvolve() //TODO add more evolution types
        {
            //check if the player has reached the required level for evolution
            return EvolutionType switch
            {
                PlayerEvolutionType.Skeleton => Level >= _levelsPerEvolution,
                //PlayerEvolutionType.ArmoredSkeleton => Level >= _levelsPerEvolution * 2, //currently only 1 evolution type
                _ => false,
            };
        }

        private void PlayerStatAdjustmentForLevel()
        {
            //TODO
            int levelModifier = Level * 2;

            bonusAttackFromLevel = levelModifier;
            bonusDefenceFromLevel = levelModifier;
            bonusHealthFromLevel = levelModifier * 10;
            bonusManaFromLevel = levelModifier * 10;

            UpdateStatsWithBonusses();
        }

        public void ApplyAttributePoints(int attackPoints, int defencePoints, int healthPoints, int manaPoints)
        {
            if (attackPoints < 0 || defencePoints < 0 || healthPoints < 0 || manaPoints < 0 ||
                (long)attackPoints + defencePoints + healthPoints + manaPoints > AttributePoints)
                throw new ArgumentOutOfRangeException(nameof(attackPoints), "Allocate only available attribute points.");
            bonusAttackFromAttributePoints += attackPoints;
            bonusDefenceFromAttributePoints += defencePoints;
            bonusHealthFromAttributePoints += healthPoints;
            bonusManaFromAttributePoints += manaPoints;
            AttributePoints -= (attackPoints + defencePoints + healthPoints + manaPoints);

            UpdateStatsWithBonusses();
        }

        public void ConsumeItem(GameItem item)
        {
            if (Backpack.ContainsItem(item))
            {
                //TODO
                if (item is Consumable consumable)
                {
                    switch (consumable.Effect)
                    {
                        case ConsumableEffect.Heal:

                            if (Health != MaxHealth)
                            {
                                if (Health + consumable.EffectBonus < MaxHealth)
                                    Health += consumable.EffectBonus;
                                else
                                    Health = MaxHealth;

                                item.RemoveQuantity(1);
                                if (item.Quantity == 0)
                                    Backpack.Remove(item);
                            }
                            break;
                        case ConsumableEffect.DefenceIncrease: //TODO
                            Debug.WriteLine("increase def");
                            break;
                        case ConsumableEffect.AttackIncrease: //TODO
                            Debug.WriteLine("increase attack");
                            break;
                    }
                }
            }
        }

        public void CheckInput(GameTime gameTime)
        {
            //TODO properly handle controller input

            //if the player is aiming an attack, check for mouse input
            if (AimVisible && InputHandler.CheckMouseReleased(MouseButton.Left))
                PerformAttack(gameTime, AttackToAim);

            KeybindingsManager.CheckInput(gameTime);

            if (InputHandler.KeyReleased(Keys.E) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex))
            {
                PerformAttack(gameTime, BasicAttack);
            }
        }

        private void UpdatePlayerMotion()
        {
            Vector2 motion = new();

            if (InputHandler.KeyDown(Keys.W) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickUp, PlayerIndex))
            {
                motion.Y = -1;
            }
            else if (InputHandler.KeyDown(Keys.S) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickDown, PlayerIndex))
            {
                motion.Y = 1;
            }
            if (InputHandler.KeyDown(Keys.A) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickLeft, PlayerIndex))
            {
                motion.X = -1;
            }
            else if (InputHandler.KeyDown(Keys.D) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickRight, PlayerIndex))
            {
                motion.X = 1;
            }

            if (motion != Vector2.Zero)
                motion.Normalize();

            Motion = motion; //(motion is normalized in collision detection of EntityManager)
        }

        internal void LeaveLevel()
        {
            CancelAim();
            AttackManager.ClearAttacks(preserveCooldown: true);
            AttacksHitBy.Clear();
            Motion = Vector2.Zero;
            CanMove = CanAttack = !IsDead;
        }

        private void CancelAim()
        {
            AimVisible = false;
            AttackToAim = null;
        }

        public override void Respawn()
        {
            CancelAim();
            base.Respawn();
            Mana = MaxMana;
            _manaRecoveryTicks = 0;
        }

        internal void RecoverMana(GameTime gameTime, bool enemyEngaged)
        {
            if (IsDead || Health <= 0 || Mana >= MaxMana)
            {
                _manaRecoveryTicks = 0;
                return;
            }

            TimeSpan cutoff = gameTime.TotalGameTime - ManaCombatWindow;
            bool inCombat = enemyEngaged || AttackManager.HasAttackedSince(cutoff) ||
                (LastTimeAttacked != TimeSpan.Zero && LastTimeAttacked >= cutoff);
            // One point every five combat seconds, with five times that rate outside combat.
            _manaRecoveryTicks += Math.Max(0, gameTime.ElapsedGameTime.Ticks) * (inCombat ? 1 : 5);
            Mana += (int)Math.Min((long)MaxMana - Mana, _manaRecoveryTicks / RecoveryTicksPerMana);
            _manaRecoveryTicks = Mana == MaxMana ? 0 : _manaRecoveryTicks % RecoveryTicksPerMana;
        }

        public override void PerformAttack(GameTime gameTime, BasicAttack entityAttack)
        {
            if (IsDead || Health <= 0 || !CanAttack)
                return;

            switch (entityAttack)
            {
                case null:
                    break;
                case ShootingAttack attack:
                    PerformAimedAttack(gameTime, attack, GetMousePosition());
                    break;
                case PopUpAttack attack:
                    PerformPopUpAttack(gameTime, attack, GetMousePosition());
                    break;
                default:
                    PerformBasicAttack(gameTime, entityAttack);
                    break;
            }
        }

        public virtual void PerformBasicAttack(GameTime gameTime, BasicAttack entityAttack)
        {
            if (entityAttack is null) return;

            //perform the attack
            base.PerformAttack(gameTime, entityAttack);
        }

        public virtual void PerformAimedAttack(GameTime gameTime, ShootingAttack entityAttack, Vector2 targetPosition)
        {
            if (IsDead || Health <= 0 || !CanAttack || entityAttack is null)
                return;
            if (AttackManager.AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if (AimVisible == true)
                {
                    AimVisible = false;
                    AttackToAim = null;

                    if (Mana < entityAttack.ManaCost)
                        return;
                    else
                        Mana -= entityAttack.ManaCost;

                    var fired = entityAttack.Clone();
                    fired.Source = this;
                    fired.SetUpAttack(gameTime, BasicAttackColor, Position);
                    fired.MoveInDirectionOfPosition(targetPosition);
                    if (!fired.AttackVisible)
                        fired.Motion = Vector2.Zero;
                    entityAttack.LastAttackTime = gameTime.TotalGameTime;
                    AttackManager.AddAttack(fired, gameTime);
                }
                else
                {
                    AimVisible = true;
                    AttackToAim = entityAttack;
                }
            }
        }

        public virtual void PerformPopUpAttack(GameTime gameTime, PopUpAttack entityAttack, Vector2 targetPosition)
        {
            if (IsDead || Health <= 0 || !CanAttack || entityAttack is null)
                return;
            if (AttackManager.AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if (AimVisible == true)
                {
                    AimVisible = false;
                    AttackToAim = null;

                    if (Mana < entityAttack.ManaCost)
                        return;
                    else
                        Mana -= entityAttack.ManaCost;

                    var fired = entityAttack.Clone();
                    fired.Source = this;
                    fired.SetUpAttack(gameTime, BasicAttackColor, targetPosition);
                    entityAttack.LastAttackTime = gameTime.TotalGameTime;
                    AttackManager.AddAttack(fired, gameTime);
                }
                else
                {
                    AimVisible = true;
                    AttackToAim = entityAttack;
                    entityAttack.ResetAttack();
                }
            }
        }

        public static Vector2 GetMousePosition()
        {
            MouseState _mouseState = Mouse.GetState();
            return Vector2.Transform(new(_mouseState.X, _mouseState.Y), Matrix.Invert(World.Camera.Transformation));
        }

        public void Evolve()
        {
            if (CanEvolve is false)
                return;

            EvolveTo(EvolutionType + 1); //advance to next evolution type
            UpdateEvolutionTexture();
        }

        private void EvolveTo(PlayerEvolutionType evolutionType)
        {
            EvolutionType = evolutionType;
            Debug.WriteLine($"Evolution type: {EvolutionType}");

            SetBonussesFromEvolution();
            UpdateStatsWithBonusses();

            CanEvolve = CheckCanEvolve();
        }

        private void UpdateEvolutionTexture()
        {
            //TODO set the player texture based on evolution type
            switch (EvolutionType)
            {
                case PlayerEvolutionType.Skeleton:
                    //dont do anything, already set to skeleton
                    break;
                case PlayerEvolutionType.ArmoredSkeleton:
                    Texture = _armoredTexture;
                    SetFrames(4, 64, 64, xOffset: 14, yOffset: 2, paddingX: 26, paddingY: 4,
                        order: [AnimationKey.Down, AnimationKey.Right, AnimationKey.Up, AnimationKey.Left]);
                    break;
                default:
                    break;
            }
        }

        private void SetBonussesFromEvolution()
        {
            //TODO set bonusses based on evolution type
            switch (EvolutionType)
            {
                case PlayerEvolutionType.Skeleton:
                    bonusAttackFromEvolution = 0;
                    bonusDefenceFromEvolution = 0;
                    bonusHealthFromEvolution = 0;
                    bonusManaFromEvolution = 0;
                    break;
                case PlayerEvolutionType.ArmoredSkeleton:
                    bonusAttackFromEvolution = 50;
                    bonusDefenceFromEvolution = 50;
                    bonusHealthFromEvolution = 500;
                    bonusManaFromEvolution = 200;
                    break;
                default:
                    bonusAttackFromEvolution = 0;
                    bonusDefenceFromEvolution = 0;
                    bonusHealthFromEvolution = 0;
                    bonusManaFromEvolution = 0;
                    break;
            }
        }
    }
}
