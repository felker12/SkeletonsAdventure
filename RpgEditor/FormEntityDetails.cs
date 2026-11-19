using System.ComponentModel;
using RpgLibrary.EntityClasses;
using RpgLibrary.ItemClasses;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace RpgEditor;

public partial class FormEntityDetails : Form
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public EntityData? Entity { get; private set; }

    private readonly ErrorProvider errors;
    private readonly TextBox dropTable = new();
    private List<ItemBaseData> guaranteed = [];

    public FormEntityDetails()
    {
        InitializeComponent();
        errors = new ErrorProvider(this) { BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Disposed += (_, _) => errors.Dispose();
        tbId.Visible = lblId.Visible = false;
        tbBaseHealth.Text = tbCurrentHealth.Text = "1";
        tbBaseDefence.Text = tbBaseAttack.Text = tbBaseXP.Text = "0";
        tbEntityLevel.Text = "1";
        btnOK.Click += BtnOK_Click;
        btnCancel.DialogResult = DialogResult.Cancel;
    }

    internal void ConfigureProject(EditorProject project)
    {
        if (Entity is not EnemyData)
            return;

        var label = new Label { Text = "Drop table", Location = lblId.Location, AutoSize = true };
        dropTable.Location = tbId.Location;
        dropTable.Width = tbId.Width;
        dropTable.AccessibleName = "Drop table";
        var loot = new Button { Text = "Guaranteed Drops…", AutoSize = true, Location = new Point(70, btnOK.Bottom + 4) };
        loot.Click += (_, _) =>
        {
            if (ReferenceDialogs.EditItems(this, project, "Guaranteed Enemy Drops", guaranteed) is { } items)
                guaranteed = items;
        };
        Controls.AddRange([label, dropTable, loot]);
        ClientSize = new Size(ClientSize.Width, loot.Bottom + 12);
    }

    private void BtnOK_Click(object? sender, EventArgs e)
    {
        errors.Clear();
        if (string.IsNullOrWhiteSpace(tbType.Text))
        {
            Invalid(tbType, "Enter an entity type.");
            return;
        }
        if (!ReadInt(tbBaseHealth, 1, out int baseHealth) || !ReadInt(tbBaseDefence, 0, out int baseDefence) ||
            !ReadInt(tbBaseAttack, 0, out int baseAttack) || !ReadInt(tbEntityLevel, 0, out int level) ||
            !ReadInt(tbBaseXP, 0, out int baseXp) || !ReadInt(tbCurrentHealth, 0, out int health) ||
            !ReadPosition(tbPositionX, tbPositionY, out var position) ||
            !ReadPosition(tbRespawnX, tbRespawnY, out var respawn))
            return;

        EntityData entity = Entity is EnemyData existingEnemy ? new EnemyData(existingEnemy) :
            Entity?.Clone() ?? new EntityData();
        entity.Type = tbType.Text.Trim();
        entity.BaseHealth = baseHealth;
        entity.BaseDefence = baseDefence;
        entity.BaseAttack = baseAttack;
        entity.EntityLevel = level;
        entity.BaseXP = baseXp;
        entity.CurrentHealth = health;
        entity.Position = position;
        entity.RespawnPosition = respawn;
        entity.IsDead = cbIsDead.Checked;
        if (entity is EnemyData enemy)
        {
            enemy.DropTableName = dropTable.Text.Trim();
            enemy.GuaranteedItems = guaranteed;
        }

        Entity = entity;
        DialogResult = DialogResult.OK;
    }

    public void LoadEntity(EntityData entity)
    {
        Entity = entity;
        tbType.Text = entity.Type;
        tbBaseHealth.Text = entity.BaseHealth.ToString();
        tbBaseDefence.Text = entity.BaseDefence.ToString();
        tbBaseAttack.Text = entity.BaseAttack.ToString();
        tbEntityLevel.Text = entity.EntityLevel.ToString();
        tbBaseXP.Text = entity.BaseXP.ToString();
        cbIsDead.Checked = entity.IsDead;
        tbCurrentHealth.Text = entity.CurrentHealth.ToString();
        tbPositionX.Text = entity.Position?.X.ToString() ?? "";
        tbPositionY.Text = entity.Position?.Y.ToString() ?? "";
        tbRespawnX.Text = entity.RespawnPosition?.X.ToString() ?? "";
        tbRespawnY.Text = entity.RespawnPosition?.Y.ToString() ?? "";
        if (entity is EnemyData enemy)
        {
            dropTable.Text = enemy.DropTableName;
            guaranteed = enemy.GuaranteedItems.Select(item => new ItemBaseData(item.Name, item.Quantity)).ToList();
        }
    }

    private bool ReadInt(TextBox field, int minimum, out int value)
    {
        if (int.TryParse(field.Text, out value) && value >= minimum)
            return true;
        Invalid(field, $"Enter a whole number of at least {minimum}.");
        return false;
    }

    private bool ReadPosition(TextBox x, TextBox y, out Vector2? position)
    {
        position = null;
        if (string.IsNullOrWhiteSpace(x.Text) && string.IsNullOrWhiteSpace(y.Text))
            return true;
        if (float.TryParse(x.Text, out float xValue) && float.TryParse(y.Text, out float yValue) &&
            float.IsFinite(xValue) && float.IsFinite(yValue))
        {
            position = new Vector2(xValue, yValue);
            return true;
        }
        Invalid(x, "Enter both coordinates as finite numbers, or leave both blank.");
        return false;
    }

    private void Invalid(Control control, string message)
    {
        errors.SetError(control, message);
        control.Focus();
    }
}
