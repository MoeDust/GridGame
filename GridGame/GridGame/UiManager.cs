using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace GridGame
{
    class UiManager
    {
        private UIbar lowerBar;

        string lowerBarFile = "Sprites/bar";

        public void Initialize(GraphicsDevice graphicDevice)
        {
            lowerBar = new UIbar(graphicDevice);
        }


        public void LoadContent(ContentManager cManager)
        {
            lowerBar.LoadContent(lowerBarFile, cManager);
        }

        public void Update(GameTime gameTime, Viewport viewport, MouseState mouseState, ButtonState previousClickState, ContentManager cManager)
        {
            lowerBar.Update(gameTime, viewport, mouseState, previousClickState, cManager);
            
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            lowerBar.Draw(spriteBatch);
        }
    }
}
