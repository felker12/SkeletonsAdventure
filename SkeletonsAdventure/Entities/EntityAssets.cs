using RpgLibrary.AttackData;

namespace SkeletonsAdventure.Entities
{
    public sealed record EntityAssets(
        Texture2D Texture,
        SpriteFont InfoFont,
        Texture2D StatusBarTexture,
        SpriteFont StatusBarFont,
        AttackData BasicAttackData,
        Texture2D BasicAttackTexture);
}
