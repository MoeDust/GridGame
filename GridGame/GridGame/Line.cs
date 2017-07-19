using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GridGame
{
    class Line : Sprite
    {
        private bool linesToggled;
        private bool overCapacity;
        private float blinkCounter;
        private float transparencyAlpha;

        private Texture2D arrow;
        public Texture2D Arrow
        {
            get { return arrow; }
            set { arrow = value; }
        }

        // The ID of the "starting point" of the line.
        private int start;
        public int Start
        {
            get { return start; }
        }

        // THe ID of the "end point" of the line.
        private int end;
        public int End
        {
            get { return end; }
        }

        // Positions of start and end bus.
        private Vector2 startPos;

        private Vector2 endPos;

        // Flow from Start to End.
        private double p1;
        public double P1
        {
            set
            {
                p1 = value;
            }
            get
            {
                return p1;
            }
        }

        // Flow from End to Start.
        private double p2;
        public double P2
        {
            set
            {
                p2 = value;
            }
            get
            {
                return p2;
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

        // The p.u. value of R
        // Sets to 0 if the entered value is less than 0.
        private double r;
        public double R
        {
            get
            {
                return r;
            }
            set
            {
                if(value < 0)
                {
                    r = 0;
                }
                else
                {
                    r = value;
                }
            }
        }

        // The p.u. value of X
        // Sets to 0 if the entered value is less than 0.
        private double x;
        public double X
        {
            set
            {
                if (value < 0)
                    x = 0;
                else
                    x = value;
            }
            get { return x; }
        }

        private int id;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// The constructor of a line.
        /// </summary>
        /// <param name="start">Starting Bus ID</param>
        /// <param name="end">Ending Bus ID</param>
        /// <param name="pMax">Maximum P of Line</param>
        /// <param name="R">Serial R of the line</param>
        /// <param name="X">Serial X of the line</param>
        /// <param name="startPos">Position of start bus</param>
        /// <param name="endPos">Position of end bus</param>
        public Line(int id, int start, Vector2 startPos, int end, Vector2 endPos, double pMax, double R, double X)
        {
            this.start = start;
            this.startPos = startPos + new Vector2(30, 30);
            this.end = end;
            this.endPos = endPos  + new Vector2(30, 30);
            this.PMax = pMax;
            this.P1 = 0;
            this.P2 = 0;
            this.R = R;
            this.X = X;
            this.ID = id;
            this.overCapacity = false;
            this.transparencyAlpha = 0f;

            linesToggled = true;
            blinkCounter = 0;
        }

        /// <summary>
        /// Assembles a string describing the line.
        /// </summary>
        /// <returns>The assembled string.</returns>
        public String toString()
        {
            String text = String.Empty;
            //String text = "Line ";
            //text += Start;
            //text += " -> ";
            //text += End;
            text += "P: ";
            text += Math.Truncate(P1 * 100) / 100;
            text += " / ";
            text += Math.Truncate(PMax * 100) / 100;
            // text += " W ";
            // += ", P2: ";
            // text += -P1;
            // text += ", R: ";
            // text += R;
            //text += ", X: ";
            //text += Math.Truncate(X * 100) / 100;
            //text += " p.u.";

            return text;
        }

        /// <summary>
        /// Turns text on lines on and off
        /// </summary>
        public void toggleText()
        {
            linesToggled = (linesToggled == true) ? false : true;
        }

        public void Update()
        {
            if (Math.Abs(P1 / PMax) >= 1)
            {
                overCapacity = true;
            }
            else
            {
                overCapacity = false;
                blinkCounter = 0;
            }
            if (overCapacity)
            {
                blinkCounter += 1;
                if (blinkCounter >= 30)
                {
                    transparencyAlpha = (transparencyAlpha == 0f) ? 255f : 0f;
                    blinkCounter = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont theText)
        {
            Vector2 edge = endPos - startPos;

            // Simple trigonometric identity: Angle = Arctan(Y/X)
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            // Find middle of the line for arrow position
            Vector2 middle = startPos + ((endPos - startPos) / 2);

            Vector2 midNorm = 1.9f * Vector2.Normalize(middle);
            midNorm = new Vector2(-midNorm.Y, midNorm.X);

            // To check if arrow points A->B or B->A
            SpriteEffects flip = SpriteEffects.None;

            // Changes arrow to B->A if line flow reverse.
            if( P1 < 0)
            {
                flip = SpriteEffects.FlipVertically;
            }

            //Defines the size of the arrow depending of how big P1 is in relation to 1 p.u.
            scale = (float) (this.P1 / PMax);

            // Calculates how loaded the line is. If P1 >= PMax -> Line is overloaded.
            float usage = (float) Math.Abs(P1 / PMax);

            // Shortened expression for if (usage >= 1) usage = 1, else remain unchanged.
            // This is used because Color.Lerp requires a blending value between 0 and 1.
            usage = (usage >= 1) ? 1 : usage;

            Color usageColor = Color.Lerp(Color.Green, Color.Red, usage);

            if (overCapacity)
            {
                usageColor = new Color(255, 0, 0, transparencyAlpha);
            }

            // The rectangle is a line as long as the distance between Bus A and B.
            // Its width is 3.
            // Lerp blends between Green and Red depending on how much power flows through the line.
            // The angle rotates the line from origin BusA.position so it points to Bus B.
            // The layer depth is bigger than the one of the arrow because the line should draw underneath.
            spriteBatch.Draw(texture,
                new Rectangle(
                    (int)startPos.X,
                    (int)startPos.Y,
                    (int)edge.Length(),
                    3),
                null,
                usageColor,
                angle,
                new Vector2(0, 0),
                SpriteEffects.None,
                0.44f);

            // The arrow. Also blends between Green and Red
            // If P1 < 0 then the SpriteEffect "flip" flips the arrow around
            // Scale changes based on the magnitude of P1
            if (P1 != 0)
            {
                spriteBatch.Draw(arrow,
                    middle + midNorm,
                    null,
                    usageColor,
                    angle,
                    new Vector2(15, 15),
                    scale,
                    flip,
                    0.43f);
            }

            // Only draw the Text if the users wants to. Toggle Key : T
            if (linesToggled)
            {
                spriteBatch.DrawString(theText, toString(), middle, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.4f);
            }
        }
    }
}
