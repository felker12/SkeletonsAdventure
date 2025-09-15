using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using SkeletonsAdventure.Attacks;
using SkeletonsAdventure.Engines;
using SkeletonsAdventure.GameWorld;
using SkeletonsAdventure.ItemClasses;
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
        public FireBall FireBall { get; set; }
        //public FireBall FireBall2 { get; set; }
        public IcePillar IcePillar { get; set; }
        //public IcePillar IcePillar2 { get; set; }
        public IceBullet IceBullet { get; set; } 
        public bool AimVisible { get; set; } = false;
        public List<Quest> ActiveQuests { get; set; } = [];
        public List<Quest> CompletedQuests { get; set; } = [];
        public string DisplayQuestName { get; private set; } = string.Empty;

        public Player() : base()
        {
            baseAttack = 400; //TODO correct the values
            baseDefence = 6;
            baseHealth = 3000;
            TotalXP = 0;

            Initialize(); 
        }

        private void Initialize()
        {
            RespawnTime = 0;
            Health = baseHealth;
            MaxHealth = baseHealth;
            Defence = baseDefence;
            Attack = baseAttack;
            Speed = 6; //TODO

            BaseMana = 1000; //TODO
            MaxMana = BaseMana;
            Mana = BaseMana;

            Backpack = new();
            EquippedItems = new();

            //TODO delete this line
            Info.TextColor = Color.Aqua;

            InitializeAttacks();

            //TODO
            HealthBarVisible = false;
        }

        private void InitializeAttacks()
        {
            //FireBall ??= new(GameManager.FireBallData, GameManager.FireBallTexture, this);
            //FireBall2 ??= new(GameManager.FireBallData, GameManager.FireBallTexture2, this);
            //IcePillar ??= new(GameManager.IcePillarData, GameManager.IcePillarTexture, this);
            //IcePillar2 ??= new(GameManager.IcePillarData, GameManager.IcePillarSpriteSheetTexture, this, 62, 62);
            //IceBullet ??= new(GameManager.IceBulletData, GameManager.IceBulletTexture, this);

            FireBall = (FireBall)GameManager.EntityAttackClone["FireBall"];
            IcePillar = (IcePillar)GameManager.EntityAttackClone["IcePillar"];
            IceBullet = (IceBullet)GameManager.EntityAttackClone["IceBullet"];

            FireBall.AnimatedAttack = true;
            FireBall.Source = this;
            IceBullet.Source = this;
            IcePillar.Source = this;

            //TODO
            //IcePillar2.AnimatedAttack = true;
            //IcePillar2.SetFrames(4, 62, 62, 0, 62);
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

            PlayerStatAdjustmentForLevel();
        }

        public PlayerData GetPlayerData()
        {
            return new(GetEntityData())
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
                backpack = Backpack.GetItemListItemData(),
                activeQuests = ActiveQuests.ConvertAll(q => q.GetQuestData()),
                completedQuests = CompletedQuests.ConvertAll(q => q.GetQuestData()),
                displayQuestName = DisplayQuestName
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            //spriteBatch.DrawRectangle(GetRectangle, SpriteColor, 1, 0); //TODO

            if (AimVisible)
                spriteBatch.DrawLine(GetMousePosition(), Center, Color.White, 1);
        }

        public override void Update(GameTime gameTime)
        {
            if(CanMove)
                UpdatePlayerMotion();

            CheckInput(gameTime);
            base.Update(gameTime); //keep the update call after updating motion
            
            Backpack.Update();
            UpdateStatsWithBonusses();
            IfLeveledRefillStats();
            CheckQuestCompleted();

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
        }

        private protected void UpdateStatsWithBonusses()
        {
            Attack = baseAttack + EquippedItems.EquippedItemsAttackBonus() + bonusAttackFromLevel + bonusAttackFromAttributePoints;
            Defence = baseDefence + EquippedItems.EquippedItemsDefenceBonus() + bonusDefenceFromLevel + bonusDefenceFromAttributePoints;
            MaxHealth = baseHealth + bonusHealthFromLevel + bonusHealthFromAttributePoints; //TODO maybe allow gear to provide a health bonus
            MaxMana = BaseMana + bonusManaFromLevel + bonusManaFromAttributePoints; //TODO maybe allow gear to provide a mana bonus
        }

        private void IfLeveledRefillStats()
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

        public void SetDisplayQuestName(Quest quest)
        {
            DisplayQuestName = quest.Name;
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
                SetDisplayQuestName(quest); //Make this the current quest to display
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
            //TODO properly handle controller input
            if (InputHandler.KeyReleased(Keys.E) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {
                PerformAttack(gameTime, BasicAttack);
            }

            //Keys 1 through 0 
            //TODO change the Buttons to correct buttons to be used on a controller
            if (InputHandler.KeyReleased(Keys.D1) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {
                PerformAimedAttack(gameTime, FireBall, GetMousePosition());
            }
            if (InputHandler.KeyReleased(Keys.D2) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

                PerformPopUpAttack(gameTime, IcePillar, GetMousePosition());
            }
            if (InputHandler.KeyReleased(Keys.D3) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {
                PerformAimedAttack(gameTime, IceBullet, GetMousePosition());
            }
            if (InputHandler.KeyReleased(Keys.D4) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

            }
            if (InputHandler.KeyReleased(Keys.D5) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

            }
            if (InputHandler.KeyReleased(Keys.D6) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

            }
            if (InputHandler.KeyReleased(Keys.D7) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

            }
            if (InputHandler.KeyReleased(Keys.D8) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

            }
            if (InputHandler.KeyReleased(Keys.D9) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

            }
            if (InputHandler.KeyReleased(Keys.D0) ||
            InputHandler.ButtonDown(Buttons.RightTrigger, PlayerIndex.One))
            {

            }
        }

        private void UpdatePlayerMotion()
        {
            Vector2 motion = new();

            if (InputHandler.KeyDown(Keys.W) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickUp, PlayerIndex.One))
            {
                motion.Y = -1;
            }
            else if (InputHandler.KeyDown(Keys.S) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickDown, PlayerIndex.One))
            {
                motion.Y = 1;
            }
            if (InputHandler.KeyDown(Keys.A) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickLeft, PlayerIndex.One))
            {
                motion.X = -1;
            }
            else if (InputHandler.KeyDown(Keys.D) ||
            InputHandler.ButtonDown(Buttons.LeftThumbstickRight, PlayerIndex.One))
            {
                motion.X = 1;
            }

            if (motion != Vector2.Zero)
            {
                motion.Normalize();
            }

            Motion = motion;
        }

        public override void Respawn()
        {
            base.Respawn();
        }

        public virtual void PerformAimedAttack(GameTime gameTime, ShootingAttack entityAttack, Vector2 targetPosition)
        {
            if (AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if (AimVisible == true)
                { 
                    AimVisible = false; 
                    
                    if (Mana < entityAttack.ManaCost)
                        return;
                    else
                        Mana -= entityAttack.ManaCost;

                    entityAttack.SetUpAttack(gameTime, BasicAttackColor, Position);
                    entityAttack.MoveInPositionDirection(targetPosition);

                    AttackManager.AddAttack(entityAttack, gameTime); //TODO
                }
                else
                    AimVisible = true;
            }
        }

        public virtual void PerformPopUpAttack(GameTime gameTime, ShootingAttack entityAttack, Vector2 targetPosition)
        {
            if (AttackingIsOnCoolDown(gameTime) is false && entityAttack.IsOnCooldown(gameTime) is false)
            {
                if (AimVisible == true)
                {
                    AimVisible = false;

                    if (Mana < entityAttack.ManaCost)
                        return;
                    else
                        Mana -= entityAttack.ManaCost;

                    entityAttack.SetUpAttack(gameTime, BasicAttackColor, targetPosition);

                    AttackManager.AddAttack(entityAttack.Clone(), gameTime);
                }
                else
                    AimVisible = true;
            }
        }

        public static Vector2 GetMousePosition()
        {
            MouseState _mouseState = Mouse.GetState();
            return Vector2.Transform(new(_mouseState.X, _mouseState.Y), Matrix.Invert(World.Camera.Transformation));
        }
    }
}
