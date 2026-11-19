using System.ComponentModel;
using RpgLibrary.ItemClasses;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RpgEditor;

internal sealed class ShieldAuthoringFields(ShieldData shield)
{
    public string Name { get => shield.Name; set => shield.Name = value; }
    public string Description { get => shield.Description; set => shield.Description = value; }
    public string Type => string.IsNullOrWhiteSpace(shield.Type) ? ItemInput.DefaultEquipmentType(shield) : shield.Type;
    public string Slot => "Off hand";
    public int Price { get => shield.Price; set => shield.Price = value; }
    public float Weight { get => shield.Weight; set => shield.Weight = value; }
    [DisplayName("Defence Value")]
    public int DefenceValue { get => shield.DefenceValue; set => shield.DefenceValue = value; }
    public Vector2 Position { get => shield.Position; set => shield.Position = value; }
    [DisplayName("Texture Path")]
    public string TexturePath { get => shield.TexturePath; set => shield.TexturePath = value; }
    [DisplayName("Source Rectangle")]
    public Rectangle SourceRectangle { get => shield.SourceRectangle; set => shield.SourceRectangle = value; }
}
