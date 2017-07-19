using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace GridGame
{
    class UIbar : Sprite
    {
        private Rectangle barPosition;

        /// <summary>
        /// Initializes bar at bottom of the screen and scaled to 1024x768
        /// </summary>
        /// <param name="gDevice">The graphics device</param>
        public UIbar(GraphicsDevice gDevice)
        {
            scale = (float) gDevice.Viewport.Width / 1024;
            position = new Vector2(0, gDevice.Viewport.Height - scale*100);
            
        }

        public new void LoadContent(string file, ContentManager cManager)
        {
            filename = file;
            texture = cManager.Load<Texture2D>(file);
            size = new Rectangle(0, 0, texture.Width, texture.Height);
            barPosition = new Rectangle((int) position.X, (int) position.Y, texture.Width, texture.Height);
        }

        /// <summary>
        /// This function updates the UI Bar according to the current inputs and state of the game.
        /// To check if the mouse is on the UI bar, the mouse x and y are needed.
        /// </summary>
        /// <param name="gameTime"> The current game time</param>
        /// <param name="viewport"> The current viewport containing the dimensions</param> 
        /// <param name="currentMouse"> The current mouse state</param> 
        /// <param name="previousMouse"> The previous mouse state</param>
        /// <param name="cManager">The content manager of the game</param>
        public void Update(GameTime gameTime, Viewport viewport, MouseState currentMouse, ButtonState previousMouse, ContentManager cManager)
        {
            // This is to make sure that the UI stays in place even when the camera moves.
            position = new Vector2(viewport.X, viewport.Y + viewport.Height - scale * 100);

            // The position check Rectangle also needs to be resized.
            // barPosition = new Rectangle( (int) position.X, (int) position.Y, size.Width, size.Height);

            if( barPosition.Contains(currentMouse.X, currentMouse.Y) && currentMouse.LeftButton == ButtonState.Pressed)
            {
                previousMouse = ButtonState.Pressed;
                texture = cManager.Load<Texture2D>("Sprites/bar2");
            }
            else
            {
                texture = cManager.Load<Texture2D>("Sprites/bar");
            }
        }
    }
}
