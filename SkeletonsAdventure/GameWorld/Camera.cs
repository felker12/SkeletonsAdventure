using MonoGame.Extended;

namespace SkeletonsAdventure.GameWorld
{
    internal sealed class Camera
    {
        private const float PanSpeed = 600f;
        private const float ZoomPerNotch = 1.25f;
        private readonly GraphicsDevice _graphics;
        private int _mapWidth, _mapHeight;
        private Vector2 _position;

        public Camera(GraphicsDevice graphics)
        {
            _graphics = graphics;
            View = new OrthographicCamera(graphics)
            {
                Origin = Vector2.Zero,
                MinimumZoom = 0.125f,
                MaximumZoom = 4f
            };
        }

        public OrthographicCamera View { get; }
        public Matrix Transformation => View.GetViewMatrix();
        public bool IsDetached { get; private set; }
        private Vector2 ViewSize => new Vector2(_graphics.Viewport.Width, _graphics.Viewport.Height) / View.Zoom;

        public void Update(Vector2 playerPosition)
        {
            if (!IsDetached)
                SetPosition(playerPosition - ViewSize / 2);
        }

        public void ToggleDetached(Vector2 playerPosition)
        {
            if (!Game1.IfDev)
                return;

            if (IsDetached)
                ResumeFollowing(playerPosition);
            else
                IsDetached = true;
        }

        public void MoveDetached(Vector2 direction, float wheelNotches, TimeSpan elapsed)
        {
            if (!IsDetached)
                return;

            var center = _position + ViewSize / 2;
            View.Zoom = Math.Clamp(View.Zoom * MathF.Pow(ZoomPerNotch, wheelNotches), View.MinimumZoom, View.MaximumZoom);
            if (direction.LengthSquared() > 1)
                direction.Normalize();
            center += direction * (PanSpeed * (float)elapsed.TotalSeconds / View.Zoom);
            SetPosition(center - ViewSize / 2);
        }

        public void ResumeFollowing(Vector2 playerPosition)
        {
            IsDetached = false;
            View.Zoom = 1f;
            Update(playerPosition);
        }

        public void SetBounds(int width, int height)
        {
            _mapWidth = width;
            _mapHeight = height;
        }

        private void SetPosition(Vector2 position)
        {
            var viewSize = ViewSize;
            _position = new(ClampAxis(position.X, _mapWidth - viewSize.X), ClampAxis(position.Y, _mapHeight - viewSize.Y));

            // Snap the translation to screen pixels so the map and sprites stay aligned.
            float zoom = View.Zoom;
            View.Position = new Vector2(MathF.Round(_position.X * zoom), MathF.Round(_position.Y * zoom)) / zoom;
        }

        private float ClampAxis(float position, float remainingSpace)
        {
            if (remainingSpace >= 0)
                return Math.Clamp(position, 0, remainingSpace);

            return IsDetached ? remainingSpace / 2 : 0;
        }
    }
}
