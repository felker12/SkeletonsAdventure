
namespace SkeletonsAdventure.GameUI
{
    internal class DamagePopUpManager()
    {
        public List<DamagePopUp> DamagePopUps { get; } = [];

        public void Update(GameTime gameTime)
        {
            //Remove timed out pop-ups
            List<DamagePopUp> timedOutPopUps = [];

            foreach (var popUp in DamagePopUps)
            {
                if (popUp.TimedOut)
                    timedOutPopUps.Add(popUp);
            }

            foreach(var popUp in timedOutPopUps)
            {
                Remove(popUp);
            }

            //Update remaining pop-ups
            foreach (var popUp in DamagePopUps)
            {
                popUp.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var popUp in DamagePopUps)
            {
                popUp.Draw(spriteBatch);
            }
        }

        public void Add(DamagePopUp popUp)
        {
            if (popUp != null)
            {
                DamagePopUps.Add(popUp);
            }
        }

        public void Remove(DamagePopUp popUp)
        {
            if (popUp != null && DamagePopUps.Contains(popUp))
            {
                DamagePopUps.Remove(popUp);
            }
        }

        public void Clear()
        {
            DamagePopUps.Clear();
        }
    }
}
