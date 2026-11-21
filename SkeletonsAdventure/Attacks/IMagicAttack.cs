namespace SkeletonsAdventure.Attacks
{
    internal enum MagicElement { Fire, Water, NonElemental }

    internal interface IMagicAttack
    {
        MagicElement Element { get; }
        string DisplayName { get; }
    }
}
