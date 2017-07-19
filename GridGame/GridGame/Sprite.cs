using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GridGame
{
    class Sprite
    {
        // Sprite position
        public Vector2 position;
        // Sprite texture
        public Texture2D texture;
        // Sprite filename
        public string filename;
        // Sprite draw-rectangle
        public Rectangle size;
        // Sprite scaling
        public float scale = 1.0f;

        // Initializes a sprite at position 0 and scale 1.
        public Sprite()
        {
            position = Vector2.Zero;
            scale = 1.0f;
        }

        // Initializes a sprite at the position pos and scaling the sprite relative to standard resolution 1024x768
        public Sprite(Vector2 pos, GraphicsDevice gDevice)
        {
            position = pos;
            scale = gDevice.Viewport.Width / 1024;
        }

        public void LoadContent(string file, ContentManager cManager)
        {
            filename = file;
            texture = cManager.Load<Texture2D>(file);
            size = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, size, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.999f);
        }
    }
}
