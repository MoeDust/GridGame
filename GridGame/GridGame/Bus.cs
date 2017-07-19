using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GridGame
{
    class Bus : Sprite
    {
        private Texture2D powerbar;
        private Texture2D blades;
        private Vector2 bladesPosition;
        private float bladesAngle;
        public Rectangle sizeFrame;
        public Rectangle sizeBar;
        private Vector2 barPos;

        private int id;
        public int Id
        {
            set
            {
                id = value;
            }
            get
            {
                return id;
            }
        }


        private double p;
        public double P
        {
            set
            {
                if(value > PMax)
                {
                    p = PMax;
                }
                else if(value < PMin)
                {
                    p = PMin;
                }
                else
                {
                    p = value;
                }
            }
            get
            {
                return p;
            }
        }

        private double pMax;
        public double PMax
        {
            set
            {
                pMax = value;
            }
            get
            {
                return pMax;
            }
        }

        private double pMin;
        public double PMin
        {
            set { pMin = value; }
            get { return pMin; }
        }

        private Rectangle mouseRectangle;
        public Rectangle MouseRectangle
        {
            set
            {
                mouseRectangle = value;
            }
            get
            {
                return mouseRectangle;
            }
        }

        private double u;
        public double U
        {
            set
            {
                u = value;
            }
            get
            {
                return u;
            }
        }


        private double theta;
        public double Theta
        {
            set
            {
                if (value > 360 || value < -360 )
                { theta = value % 360; }
                else
                { theta = value; }
            }
            get
            {
                return theta;
            }
        }

        private string type;
        public string Type
        {
            set
            {
                type = value;
            }
            get
            {
                return type;
            }
        }

        private double pForBattery;
        public double PForBattery
        {
            set
            {
                
                    pForBattery = value;
                
            }
            get
            {
                return pForBattery;
            }
        }
        /*
        private double q;
        public double Q
        {
            set
            {
                q = value;
            }
            get
            {
                return q;
            }
        }


        private double load;
        public double Load
        {
            set
            {
                load = value;
            }
            get
            {
                return load;
            }
        }


        private double generation;
        public double Generation
        {
            set
            {
                generation = value;
            }
            get
            {
                return generation;
            }
        }*/


        public Bus(Vector2 position, int Id, double P, double PMax, double PMin, double U, double Theta, string Type, GraphicsDevice gDevice) : base(position, gDevice)
        {
            this.PMax = PMax;
            this.PMin = PMin;
            this.Type = Type;
            this.Id = Id;
            this.P = P;
            this.U = U;
            this.Theta = Theta;
            this.PForBattery = P;
        }

        public void loadBar(ContentManager cManager)
        {
            powerbar = cManager.Load<Texture2D>("Sprites/powerbarTrans");
            sizeFrame = new Rectangle(0, 0, powerbar.Width, 10);

            sizeBar = new Rectangle(0, 10, powerbar.Width, 10);

            barPos = position - new Vector2(0, 10);
        }

        public void loadBlades(ContentManager cManager)
        {
            blades = cManager.Load<Texture2D>("Sprites/blades");
            bladesPosition = position; //- new Vector2(20, 20);
            bladesPosition += new Vector2(30, 3);
            bladesAngle = 0;
        }

        public void setMouseRectangle()
        {
            this.MouseRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        }

        public String toString()
        {
            String text = "";
            //text += id + " ";
            text += "P: " + Math.Truncate(PMin * 100) / 100 + " / " + Math.Truncate(P * 100) / 100 + " / " + Math.Truncate(PMax * 100) / 100 + " \n";
            text += "Theta: " + Math.Truncate(Theta * 100) / 100 + " rad ";

            return text;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont theText)
        {
            spriteBatch.Draw(texture, position, size, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.3f);
            if(Type == "W")
            {
                // If for some reason the program decides to go in here without it being wind, everything is null, so catch for that.
                try
                {
                    // This means the Angle at full speed will be += 0.1f every 1/60th of a second.
                    bladesAngle += (float)(this.P / this.PMin) * 0.1f; 

                    // To not go over 2*Pi
                    if (bladesAngle >= 2 * Math.PI)
                    {
                        bladesAngle = 0f;
                    }
                    spriteBatch.Draw(blades, bladesPosition, size, Color.White, bladesAngle, new Vector2(20, 12), scale, SpriteEffects.None, 0.3f);
                }
                catch (Exception e)
                { 
                    // Ignore and carry on.
                    String bla = e.ToString();
                }
            }
            Color barColor;

            // If the bus consumes energy, the bar will be will be orange.
            if(P > 0)
            {
                barColor = Color.Orange;
            }
            // If the bus produces energy, the bar will be green.
            else
            {
                barColor = Color.Green;
            }
            spriteBatch.Draw(powerbar, barPos, sizeFrame, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.28f);

            // The bar is 60 pixels wide. This function relates x-x_min/x_max-x_min, so if P = PMax the ratio will be 1 and the bar will be full.
            int barSize = (int)((Math.Abs((P - PMin))) / Math.Abs((PMax - PMin)) * 60d);
            
            // If the bus generates power, the above equation will subtract until the bar is empty at full production, hence 60px - barSize.
            if(P <= 0)
            {
                barSize = 60 - barSize;
            }

            spriteBatch.Draw(powerbar, new Rectangle((int)barPos.X, (int)barPos.Y, barSize, 10), sizeBar, barColor, 0.0f, Vector2.Zero, SpriteEffects.None, 0.29f);
            Vector2 textMid = theText.MeasureString(Id.ToString());
            Vector2 pos = position + (new Vector2(texture.Width / 2 - textMid.X / 2, 61));
            spriteBatch.DrawString(theText, Id.ToString(), pos, Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.4f);
        }
    }
}
