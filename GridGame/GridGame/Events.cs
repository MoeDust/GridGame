using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace GridGame
{
    /// <summary>
    /// The class events contains the different events which can occure during a level.
    /// </summary>
    class Events : Sprite
    {
        public int ID
        {
            set
            {
                eventID = value;
            }
            get
            {
                return eventID;
            }
        }
        private double p;
        public double P
        {
            set
            {
                p = value;
            }
            get
            {
                return p;
            }
        }
        public float T1
        {
            set
            {
                start = value;
            }
            get
            {
                return start;
            }
        }
        public float T2
        {
            set
            {
                end = value;
            }
            get
            {
                return end;
            }
        }
        public int BusID
        {
            set
            {
                busID = value;
            }
            get
            {
                return busID;
            }
        }

        public string Text
        {
            set
            {
                text = value;
            }
            get
            {
                return text;
            }
        }

        private int busID;
        private int eventID;
        private float start;
        private float end;
        private string text;

        // If an Event is added, then the power generation of bus with the according id changes by P
        public Events(int eventID, int busID, double p, float start, float end, string text)
        {
            this.ID = eventID;
            this.BusID = busID;
            this.P = p;
            this.T1 = start;
            this.T2 = end;
            this.Text = text;
        }
    }
}