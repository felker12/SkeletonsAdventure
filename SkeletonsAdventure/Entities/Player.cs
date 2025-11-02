using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
using SkeletonsAdventure.ItemClasses.ItemManagement;
using SkeletonsAdventure.Quests;

namespace SkeletonsAdventure.Entities
{
    internal class Player : Entity
    {
        public int bonusAttackFromLevel = 0, bonusDefenceFromLevel = 0,
            bonusHealthFromLevel = 0, bonusManaFromLevel = 0,
            bonusAttackFromAttributePoints = 0, bonusDefenceFromAttributePoints = 0, 
            bonusHealthFromAttributePoints = 0, bonusManaFromAttributePoints = 0;
        private bool justLeveled = false;

        public Backpack Backpack { get; set; }
        public EquippedItems EquippedItems { get; set; }
        public int TotalXP { get; set; } = 0;
        public int Mana { get; set; } = 0;
        public int BaseMana { get; set; } = 0;
        public int MaxMana { get; set; } = 0;
        public int AttributePoints { get; set; } = 0;
        public float XPModifier { get; set; } = 1.0f; //TODO
        private bool AimVisible { get; set; } = false;
        public List<Quest> ActiveQuests { get; set; } = [];
        public List<Quest> CompletedQuests { get; set; } = [];
        public string DisplayQuestName { get; private set; } = string.Empty;
        private BasicAttack AttackToAim { get; set; } = null;
        public KillCounter KillCounter { get; private set; } = new(); 
        public PlayerIndex PlayerIndex { get; set; } = PlayerIndex.One;
        public static Dictionary<Keys, BasicAttack> KeyBindings { get; private set; } = [];
        public int AttackExcludingEquipment { get; private set; } = 0;
        public int DefenceExcludingEquipment { get; private set; } = 0;

        public Player() : base()
        {
            BaseAttack = 300; //TODO correct the values
            BaseDefence = 6;
            BaseHealth = 3000;
            TotalXP = 0;

            Initialize(); 
        }

        private void Initialize()
        {
            RespawnTime = 0;
            Health = BaseHealth;
            MaxHealth = BaseHealth;
            Defence = BaseDefence;
            Attack = BaseAttack;
            Speed = 6; //TODO

            BaseMana = 1000; //TODO
            MaxMana = BaseMana;
            Mana = BaseMana;

            Backpack = new();
            EquippedItems = new(this);

            //TODO delete this line
            Info.TextColor = Color.Aqua;

            InitializeAttacks();

            HealthBarVisible = false;

            //GainXp(10000);
        }

        private void InitializeAttacks()
        {
            KeyBindings = new Dictionary<Keys, BasicAttack>
            {
                { Keys.D1, (FireBall)GameManager.EntityAttackClone["FireBall"]},
                { Keys.D2, (IcePillar)GameManager.EntityAttackClone["IcePillar"] },
                { Keys.D3, (IceBullet)GameManager.EntityAttackClone["IceBullet"] },
            };

            foreach (var attack in KeyBindings.Values)
                attack.Source = this;
        }

        public void UpdatePlayerWithData(PlayerData playerData)
        {
            UpdateEntityWithData(playerData);

            TotalXP = playerData.totalXP;
            BaseMana = playerData.baseMana;
            Mana = playerData.mana;
            MaxMana = playerData.maxMana;
            AttributePoints = playerData.attributePoints;
            bonusAttackFromAttributePoints = playerData.bonusAttackFromAttributePoints;
            bonusDefenceFromAttributePoints = playerData.bonusDefenceFromAttributePoints;
            bonusHealthFromAttributePoints = playerData.bonusHealthFromAttributePoints;
            bonusManaFromAttributePoints = playerData.bonusManaFromAttributePoints;

            //TODO update active quests and completed quests
            ActiveQuests = playerData.activeQuests.ConvertAll(q => new Quest(q));
            CompletedQuests = playerData.completedQuests.ConvertAll(q => new Quest(q));
            DisplayQuestName = playerData.displayQuestName;

            KillCounter = new KillCounter(playerData.killCounter);

            PlayerStatAdjustmentForLevel();
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
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //spriteBatch.DrawRectangle(GetRectangle, SpriteColor, 1, 0); //TODO

            if (AimVisible)
            {
                switch(AttackToAim)
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
            if(CanMove)
                UpdatePlayerMotion();

            CheckInput(gameTime);
            base.Update(gameTime); //keep the update call after updating motion
            
            Backpack.Update();

            CheckQuestCompleted();
            UpdateStatsWithBonusses();
            RefillStatsOnLevelUp();

            //update cooldowns of all attacks
            foreach (var attack in KeyBindings.Values)
                attack.UpdateCooldown(gameTime);

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
        }

        private protected void UpdateStatsWithBonusses()
        {
            AttackExcludingEquipment = BaseAttack + bonusAttackFromLevel + bonusAttackFromAttributePoints;
            DefenceExcludingEquipment = BaseDefence + bonusDefenceFromLevel + bonusDefenceFromAttributePoints;

            Attack = AttackExcludingEquipment + EquippedItems.EquippedItemsAttackBonus();

            Defence = DefenceExcludingEquipment + EquippedItems.EquippedItemsDefenceBonus();

            MaxHealth = BaseHealth + bonusHealthFromLevel + 
                bonusHealthFromAttributePoints; //TODO maybe allow gear to provide a health bonus

            MaxMana = BaseMana + bonusManaFromLevel + 
                bonusManaFromAttributePoints; //TODO maybe allow gear to provide a mana bonus
        }

        private void RefillStatsOnLevelUp()
        {
            if (justLeveled)
            {
                Health = MaxHealth;
                Mana = MaxMana;
                justLeveled = false;
            }
        }

        public void CheckQuestCompleted()
        {
            List<Quest> completed = [];

            if (ActiveQuests != null && ActiveQuests.Count > 0)
            {
                foreach (Quest quest in ActiveQuests)
                {
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
                    CompletedQuests.Add(quest);
                    GiveQuestReward(quest.Reward);
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
            //TODO give gold and items

            GameItem coins = new(GameManager.ItemsClone["Coins"]);
            coins.SetQuantity(reward.Coins);

            //if the items wont fit in the backpack, drop them on the ground
            if (Backpack.Add(coins) is false)
                World.CurrentLevel.EntityManager.DroppedLootManager.Add(coins, Position);
            
            foreach (GameItem item in reward.Items)
            {
                if (Backpack.Add(item.Clone()) is false)
                    World.CurrentLevel.EntityManager.DroppedLootManager.Add(item.Clone(), Position);
            }
        }

        public void AddActiveQuest(Quest quest)
        {
            if (quest != null && ActiveQuests.Contains(quest) is false)
            {
                ActiveQuests.Add(quest);
                World.AddMessage($"Quest {quest.Name} started: {quest.Description}");
            }
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

        public override void GetHitByAttack(BasicAttack attack, GameTime gameTime)
        {
            base.GetHitByAttack(attack, gameTime);
        }

        public void GainXp(int XpGained)
        {
            int currentLevel = GameManager.GetPlayerLevelAtXP(TotalXP);

            TotalXP += (int)(XpGained * XPModifier);

            Level = GameManager.GetPlayerLevelAtXP(TotalXP);

            if (Level > currentLevel)
            {
                PlayerStatAdjustmentForLevel();
                while (currentLevel < Level) //perform the levelUp event for every level gained
                {
                    currentLevel++;
                    LevelUP();
                }
            }
        }

        public void LevelUP() //TODO
        {
            AttributePoints += 5;
            justLeveled = true;

            World.AddMessage($"You leveled up! You are now level {Level}!");
            World.AddMessage($"Tottal Attribute Points: {AttributePoints}");
        }

        private void PlayerStatAdjustmentForLevel()
        {
            //TODO
            int levelModifier = Level * 2;

            bonusAttackFromLevel = levelModifier;
            bonusDefenceFromLevel = levelModifier;
            bonusHealthFromLevel = levelModifier * 10;
            bonusManaFromLevel = levelModifier * 10;
        }

        public void ApplyAttributePoints(int attackPoints, int defencePoints, int healthPoints, int manaPoints)
        {
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
                if(item is Consumable consumable)
                {
                    switch(consumable.Effect)
                    {
                        case ConsumableEffect.Heal:

                            if(Health != MaxHealth)
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
            //if the player is aiming an attack, check for mouse input
            if (AimVisible)
            {
                if(InputHandler.CheckMouseReleased(MouseButton.Left))
                {
                    PerformAttack(gameTime, AttackToAim);
                }
            }

            //TODO properly handle controller input
            //Keys 1 through 0 
            foreach (var keyBinding in KeyBindings)
            {
                if (InputHandler.KeyReleased(keyBinding.Key))
                {
                    PerformAttack(gameTime, keyBinding.Value);
                }
            }

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

        public override void Respawn()
        {
            base.Respawn();
        }

        public override void PerformAttack(GameTime gameTime, BasicAttack entityAttack)
        {
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
            if (AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if (AimVisible == true)
                { 
                    AimVisible = false;
                    AttackToAim = null;

                    if (Mana < entityAttack.ManaCost)
                        return;
                    else
                        Mana -= entityAttack.ManaCost;

                    entityAttack.SetUpAttack(gameTime, BasicAttackColor, Position);
                    entityAttack.MoveInDirectionOfPosition(targetPosition);

                    AttackManager.AddAttack(entityAttack.Clone(), gameTime); //TODO
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
            if (AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if (AimVisible == true)
                {
                    AimVisible = false;
                    AttackToAim = null;

                    if (Mana < entityAttack.ManaCost)
                        return;
                    else
                        Mana -= entityAttack.ManaCost;

                    entityAttack.SetUpAttack(gameTime, BasicAttackColor, targetPosition);

                    AttackManager.AddAttack(entityAttack.Clone(), gameTime);
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
    }
}
