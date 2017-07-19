using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GridGame
{
    class Battery : Sprite
    {
        private Texture2D powerbar;
        private Texture2D lineTexture;
        public Rectangle sizeFrame;
        public Rectangle sizeBar;
        private Vector2 barPos;
        private PowerModel.BatteryChargeState chargeState;
        private float chargeStartTime;
        private double startCharge;
        private double chargeRate;
        private double power;
        private Vector2 busPos;
        private bool allowsChange;

        public double CurrentCharge
        {
            set { }
            get { return startCharge; }
        }

        public double Power
        {
            set { }
            get { return power; }
        }

        public double ChargeRate
        {
            set { }
            get { return chargeRate; }
        }

        public bool AllowsChange
        {
            set { allowsChange = value; }
            get { return allowsChange; }
        }

        public PowerModel.BatteryChargeState ChargeState
        {
            get { return chargeState; }
        }

        public Battery(int busId, double power, double eMax, double initialCharge, double chargeRate, Vector2 position, GraphicsDevice gDevice, bool allowsChange) : base(position + new Vector2(70, 70), gDevice)
        {
            this.BusID = busId;
            this.EMax = eMax;
            this.chargeState = PowerModel.BatteryChargeState.Idle;
            this.chargeStartTime = 0f;
            this.E = initialCharge;
            this.chargeRate = chargeRate;
            this.busPos = position;
            this.power = power;
            this.allowsChange = allowsChange;
        }

        private int busId;
        public int BusID
        {
            set { this.busId = value; }
            get { return this.busId; }
        }

        private double e;
        public double E
        {
            set
            {
                if (value <= 0) {
                    this.e = 0;
                }
                else if (value >= this.eMax)
                {
                    this.e = eMax;
                }
                else
                {
                    this.e = value;
                }
            }
            get { return this.e; }
        }

        private double eMax;
        public double EMax
        {
            set
            {
                if (value <= 0)
                {
                    this.eMax = 0;
                }
                else
                {
                    this.eMax = value;
                }
            }
            get { return this.eMax; }
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

        public void setMouseRectangle()
        {
            this.MouseRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
        }

        public void loadBar(ContentManager cManager)
        {
            powerbar = cManager.Load<Texture2D>("Sprites/powerbarTrans");
            sizeFrame = new Rectangle(0, 0, powerbar.Width, 10);

            sizeBar = new Rectangle(0, 10, powerbar.Width, 10);

            barPos = position - new Vector2(0, 10);
        }

        public void Update(float elapsedTime)
        {
            if(chargeState == PowerModel.BatteryChargeState.Charging)
            {
                charge(elapsedTime);
            }
            if(chargeState == PowerModel.BatteryChargeState.Discharging)
            {
                discharge(elapsedTime);
            }
        }

        public void charge(float elapsedTime)
        {
            E = startCharge + (elapsedTime - chargeStartTime) * (chargeRate / 60);
        }

        public void discharge(float elapsedTime)
        {
            E = startCharge - (elapsedTime - chargeStartTime) * chargeRate / 60;
        }

        public void changeChargeState(PowerModel.BatteryChargeState newState, float startTime)
        {
            if(chargeState == newState)
            {
                return;
            }
            chargeStartTime = startTime;
            chargeState = newState;
            startCharge = this.E;
        }

        public void loadLineTexture(Texture2D texture)
        {
            lineTexture = texture;
        }

        public string toString()
        {
            string text = "";
            text += "Battery for Bus: ";
            text += this.BusID;
            text += "\n" + "Capacity: ";
            text += Math.Truncate(this.E * 100) / 100;
            text += "/";
            text += this.EMax;
            text += "\n" + "State: ";
            switch (chargeState)
            {
                case PowerModel.BatteryChargeState.Charging:
                    text += "Charging";
                    break;

                case PowerModel.BatteryChargeState.Discharging:
                    text += "Discharging";
                    break;

                case PowerModel.BatteryChargeState.Idle:
                    text += "Idle";
                    break;

                default:
                    text += "ErrorState";
                    break;
            }

                return text;
        }

        private void drawLineToBus(SpriteBatch spriteBatch)
        { 
            //This will draw the line to the bus
            Vector2 edge = (busPos - position);

            // Simple trigonometric identity: Angle = Arctan(Y/X)
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            // The rectangle is a line as long as the distance between Bus A and B.
            // Its width is 1.
            // Lerp blends between Green and Red depending on how much power flows through the line.
            // The angle rotates the line from origin BusA.position so it points to Bus B.
            // The layer depth is bigger than the one of the arrow because the line should draw underneath.
            spriteBatch.Draw(lineTexture,
                new Rectangle(
                    (int)position.X + 30,
                    (int)position.Y + 30,
                    (int)edge.Length(),
                    1),
                null,
                Color.Black,
                angle,
                new Vector2(0, 0),
                SpriteEffects.None,
                0.43f);
        }

        new public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, size, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.3f);

            Color barColor;
            switch (chargeState)
            {
                case PowerModel.BatteryChargeState.Charging:
                    barColor = Color.Green;
                    break;

                case PowerModel.BatteryChargeState.Discharging:
                    barColor = Color.Red;
                    break;

                case PowerModel.BatteryChargeState.Idle:
                    barColor = Color.Yellow;
                    break;
                
                default:
                    barColor = Color.White;
                    break;
            }

            spriteBatch.Draw(powerbar, barPos, sizeFrame, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.28f);
            int barSize = (int)((E) / EMax * 60d);
            spriteBatch.Draw(powerbar, new Rectangle((int)barPos.X, (int)barPos.Y, barSize, 10), sizeBar, barColor, 0.0f, Vector2.Zero, SpriteEffects.None, 0.29f);

            drawLineToBus(spriteBatch);
        }
    }
}
