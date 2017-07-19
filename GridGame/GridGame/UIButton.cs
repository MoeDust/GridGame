using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GridGame
{
    class UIButton
    {

        private string text;
        public String Text
        {
            set { text = value; }
            get { return text; }
        }

        private Rectangle rect;
        public Rectangle Rect
        {
            set { rect = value; }
            get { return rect; }
        }

        private Texture2D texture;

        public UIButton(Rectangle rect, string buttonText, GraphicsDevice gDevice)
        {
            this.Rect = rect;
            this.Text = buttonText;
            this.texture = new Texture2D(gDevice, 1, 1);

            texture.SetData(new[] { Color.Gray });
        }

        public void Update(MouseState mouseState)
        {
            if (Rect.Contains(mouseState.Position))
            {
                texture.SetData(new[] { Color.LightGray });
            }
            else
            {
                texture.SetData(new[] { Color.Gray });
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rect, null, Color.Gray, 0.0f, Vector2.Zero, SpriteEffects.None, 0.2f);
        }
    }
}
