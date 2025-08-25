﻿using RpgLibrary.EntityClasses;
using SkeletonsAdventure.Animations;
using SkeletonsAdventure.GameWorld;

namespace SkeletonsAdventure.Entities
{
    internal class SkeletalBruiser : Enemy
    {
        public SkeletalBruiser(EnemyData data) : base(data)
        {
            Initialize();
        }

        public SkeletalBruiser() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            Texture = GameManager.SkeletalBruiserTexture;
            SetFrames(4, 48, 48, paddingX: 20, order: [AnimationKey.Up, AnimationKey.Right, AnimationKey.Down, AnimationKey.Left]);
            BasicAttackColor = Color.Bisque;
            EnemyType = EnemyType.Skeleton;
        }

        public override SkeletalBruiser Clone()
        {
            SkeletalBruiser skeletalBruiser = new(GetEntityData())
            {
                Position = Position,
                GuaranteedDrops = GuaranteedDrops,
                SpriteColor = this.SpriteColor,
                DefaultColor = this.DefaultColor,
            };
            return skeletalBruiser;
        }
    }
}
