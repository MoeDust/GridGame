using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace GridGame
{
    class PowerModel
    {
        private List<Bus> buses;
        private List<Line> lines;
        private List<Battery> batteries;
        private List<Line> removedLines;
        private List<Events> allEvents;
        private int currentID;
        private int currentLineID;
        private GraphicsDevice gDevice;
        private ContentManager cManager;

        Matrix<double> bSolved;
        bool modelSolved;
        
        int currentLines;
        int solvedLines;
        int idEvent;
        float timeInLevel;
        float timeInLevelHour;
        float startTime;
        bool showEventText;

        int activeBus;
        int activeBattery;
        const int NONE = 999;
        bool mouseClicked;
        float windEventStartingTime;
        float windEventEndingTime;
        float windScalingFactor;
        float solarScalingFactor;
        float solarEventEndingTime;

        private Texture2D arrow;
        Sprite gameBackground;

        public enum BatteryChargeState
        { 
            Charging,
            Discharging,
            Idle
        }

        public int ActiveBattery
        {
            set { }
            get { return activeBattery; }
        }

        /// <summary>
        /// Generates a new power model, buses and lines are empty at the beginning.
        /// </summary>
        public PowerModel(GraphicsDevice gDevice, ContentManager cManager)
        {
            currentID = 0;
            currentLineID = 0;
            buses = new List<Bus>();
            lines = new List<Line>();
            batteries = new List<Battery>();
            removedLines = new List<Line>();
            allEvents = new List<Events>();
            this.gDevice = gDevice;
            this.cManager = cManager;
            arrow = cManager.Load<Texture2D>("Sprites/arrow");
            gameBackground = new Sprite(Vector2.Zero, gDevice);
            gameBackground.LoadContent("BG/lucerne", cManager);
            solvedLines = 0;
            currentLines = 0;
            idEvent = 0;
            activeBus = 1;
            activeBattery = NONE;
            activeBattery = 0;
            modelSolved = false;
            windEventStartingTime = 24f;
            windEventEndingTime = 24f;
            windScalingFactor = 0f;
            solarScalingFactor = 1f;
            solarEventEndingTime = 24f;
            showEventText = false;
        }


        /// <summary>
        /// loadLevel reads the level from a text file and converts it into usable buses, lines and events. 
        /// file stored: '...\the-grid-game\GridGame\GridGame\Content\Level\Level_x.txt'
        /// <param name="level">The level number</param>
        /// <param name="elapsedTime">The elapsed game time to define the start of the level</param>
        /// </summary>
        public void loadLevel(string level, float elapsedTime)
        {

            buses.Clear();
            lines.Clear();
            activeBus = 1;
            activeBattery = NONE;
            startTime = elapsedTime;
            string line;
            string text = "";
            StreamReader reader = new StreamReader("../../../../Content/Level/" + level+".txt");
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "BUS")
                {
                    reader.ReadLine();
                    while ((line = reader.ReadLine()) != "BUS_END")
                    {
                        text = line;
                        String[] textBUS = text.Split('\t');
                        addBus(new Vector2(Convert.ToSingle(textBUS[0]), Convert.ToSingle(textBUS[1])), Convert.ToDouble(textBUS[3]), Convert.ToDouble(textBUS[6]), Convert.ToDouble(textBUS[7]), Convert.ToDouble(textBUS[4]), Convert.ToDouble(textBUS[5]), textBUS[8]);
                    }
                }

                if (line == "Battery")
                {
                    reader.ReadLine();
                    while ((line = reader.ReadLine()) != "Battery_END")
                    {
                        text = line;
                        String[] textBattery = text.Split('\t');
                        addBattery(Convert.ToInt32(textBattery[0]), Convert.ToDouble(textBattery[4]), Convert.ToDouble(textBattery[1]), Convert.ToDouble(textBattery[2]), Convert.ToDouble(textBattery[3]), Convert.ToChar(textBattery[5]));
                    }
                }

                if (line == "Line")
                {
                    reader.ReadLine();

                    while ((line = reader.ReadLine()) != "Line_END")
                    {
                        text = line;
                        String[] textLine = text.Split('\t');
                        addLine(Convert.ToInt32(textLine[0]), Convert.ToInt32(textLine[1]), Convert.ToDouble(textLine[2]), Convert.ToDouble(textLine[3]), Convert.ToDouble(textLine[4]));
                    }
                }

                if (line == "Events")
                {
                    reader.ReadLine();
                    while ((line = reader.ReadLine()) != "Events_END")
                    {
                        text = line;
                        String[] textEvent = text.Split('\t');

                        addEvent(Convert.ToInt32(textEvent[0]), Convert.ToInt32(textEvent[1]), Convert.ToSingle(textEvent[2]), Convert.ToSingle(textEvent[3]), Convert.ToDouble(textEvent[4]), textEvent[5]);
                    }
                }

                if (line == "Background")
                {
                    reader.ReadLine();
                    while ((line = reader.ReadLine()) != "Background_END")
                    {
                        int backGroundNumber = 1;
                        String[] textLine = line.Split('\t');
                        backGroundNumber = Convert.ToInt32(textLine[0]);

                        switch (backGroundNumber)
                        {
                            case 1:
                                gameBackground.LoadContent("BG/bern", cManager);
                                break;
                            case 2:
                                gameBackground.LoadContent("BG/zurich", cManager);
                                break;
                            case 3:
                                gameBackground.LoadContent("BG/lucerne", cManager);
                                break;
                            case 4:
                                gameBackground.LoadContent("BG/sg", cManager);
                                break;
                            default:
                                gameBackground.LoadContent("BG/zurich", cManager);
                                break;
                        }
                    }
                    break;
                }
                
            }
        }

        /// <summary>
        /// Solves the currently defined power model.
        /// </summary>
        public void solve()
        {
            if(currentLines == 0)
            {
                return;
            }
            if (modelSolved)
            {
                return;
            }
            //Define P
            double[] powersArray = getPowersArray();
            Vector<double> power = Vector<double>.Build.DenseOfArray(powersArray);

            //Define B
            if (solvedLines != currentLines)
            {
                double[,] bMatrixArray = getBMatrixArray();
                Matrix<double> bMatrix = Matrix<double>.Build.DenseOfArray(bMatrixArray);
                bSolved = bMatrix;
                solvedLines = currentLines;
            }
            
            

            //Define Theta = 0 for all Thetas
            int busSize = buses.Count - 1;
            double[] thetasArray = new double[busSize];
            for (int i = 0; i < busSize; i++)
            {
                thetasArray[i] = 0;
            }
            Vector<double> thetas = Vector<double>.Build.DenseOfArray(thetasArray);

            // Theta = B(^-1) * P
            thetas = bSolved.Inverse() * power;

            // Set Thetas of non-slack buses to proper ones.
            for (int i = 0; i < busSize; i++){
                foreach(Bus bus in buses){
                    if (bus.Id == i + 2)
                    {
                        bus.Theta = thetas[i];
                    }
                }
            }
            //Calculate the Line power flows.
            foreach (Line line in lines)
            {
                if (line.Start == 1)
                {
                    //Since theta_1 = 0:
                    //P1j = (1 / x_1j) * -thetha_j
                    line.P1 = (1 / line.X) * (-thetas[line.End - 2]);
                    line.P2 = -line.P1;
                }
                else
                {
                    //Pij = (1 / x_ij) * (theta_i - theta_j)
                    line.P1 = (1 / line.X) * (thetas[line.Start - 2] - thetas[line.End - 2]);
                    line.P2 = -line.P1;
                }
            }


            // Set the power of the Slackbus to compensate for changes in other buses.
            double slackP = 0;
            double otherP = 0;
            foreach (Bus bus in buses)
            {
                
                if(bus.Id == 1)
                {
                    slackP = bus.P;
                }
                else
                {
                    otherP += bus.P;
                }
            }
            if (slackP != -(otherP))
            {
                buses[0].P = -(otherP);
            }

            // Tells the class that the model is solved.
            modelSolved = true;
        }

        /// <summary>
        /// This function builds your double array of the bus powers without the slack bus.
        /// </summary>
        /// <returns>The double array with the powers that aren't the slack bus</returns>
        private double[] getPowersArray()
        {
            // Get Vector size of Buses minus slack bus.
            int vectorSize = buses.Count - 1;
            double[] powersArray = new double[vectorSize];
            int i = 0;

            foreach (Bus bus in buses)
            {
                if (bus.Id == 1)
                {
                    continue;
                }
                else
                {
                    powersArray[i] = bus.P;
                    i++;
                }
            }
            return powersArray;
        }


        /// <summary>
        /// Builds the B Matrix in 2D-Array form which is necessary for the DC power flow calculations
        /// </summary>
        /// <returns>The 2D-Array with all the B-values</returns>
        private double[,] getBMatrixArray()
        {
            int matrixSize = buses.Count - 1;
            double[,] bMatrixArray = new double[matrixSize, matrixSize];

            // Double for loop to fill the array.
            // Might have to replace this as for big systems the runtime can get huge.
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    // This is for B_ii, diagonal elements
                    if (i == j)
                    {
                        double value = 0;
                        // Go through each line
                        foreach (Line line in lines)
                        {
                            // If i+2 (which is the bus ID, also can't be slack bus since ID:1 there,
                            // then if the start or end of line is i+2, that means the line is connected.
                            // Since B_ii = sum of 1/x of the lines connected:
                            if (i + 2 == line.Start || i + 2 == line.End)
                                value += 1 / line.X;
                        }
                        //Assign the value once the individual 1/x have been added.
                        bMatrixArray[i,i] = value;
                    }
                    // For any elements that aren't diagonal.
                    else
                    {
                        double value = 0;
                        foreach (Line line in lines)
                        {
                            // Gets the value x_ij or xji, if either matches then assign B_ij = 1/x_ij
                            if ((i + 2 == line.Start && j + 2 == line.End) || (i + 2 == line.End && j + 2 == line.Start))
                            {
                                // B_ij = -1 / x_ij
                                value = -1 / line.X;
                            }
                        }
                        // If Line doesn't exist, B value stays 0.
                        bMatrixArray[i, j] = value;
                    }
                }
            }

            return bMatrixArray;
        }

        /// <summary>
        /// Adds an Event to the List of current events
        /// </summary>
        /// <param name="P"> Change in Active Power P</param>
        /// <param name="busID"> Bus number</param>
        /// <param name="eventID"> type of event</param>
        /// <param name="start"> starttime of event</param>
        /// <param name="end"> endtime of event</param>
        /// <param name="text"> Text that will be shown with the Event</param>
        public void addEvent(int eventID, int busID, float start, float end, double P, string text)
        {
            Events interruption = new Events(eventID, busID, P, start, end, text);
            allEvents.Add(interruption);
        }

        //loads event at starting time
        public void loadEvent()
        {
            if (allEvents.Count != 0 && idEvent < allEvents.Count)
            {
                int tempBus = activeBus;
                if ((int)allEvents[idEvent].T1 == (int)timeInLevelHour)
                {
                    if (allEvents[idEvent].ID == 1)
                    {
                        activeBus = allEvents[idEvent].BusID;
                        eventChangeBusPower(allEvents[idEvent].P);
                    }
                    else if (allEvents[idEvent].ID == 6)
                    {
                        removeLine(allEvents[idEvent].BusID);
                    }
                    else if (allEvents[idEvent].ID==7)
                    {
                        restoreLine(allEvents[idEvent].BusID);
                    }
                    else
                    {
                        foreach (Bus bus in buses)
                        {    
                            if (allEvents[idEvent].ID == 2 && bus.Type == "W")
                            {
                                windEventStartingTime = allEvents[idEvent].T1;
                                windEventEndingTime = allEvents[idEvent].T2;
                                windScalingFactor = (float)allEvents[idEvent].P;
                            }
                            else if (allEvents[idEvent].ID == 3 && bus.Type == "P")
                            {
                                solarScalingFactor = (float)allEvents[idEvent].P;
                                solarEventEndingTime = allEvents[idEvent].T2;
                            }
                            else if (allEvents[idEvent].ID == 4 && bus.Type == "G")
                            {
                                activeBus = bus.Id;
                                eventChangeBusPower(allEvents[idEvent].P);
                            }
                            else if (allEvents[idEvent].ID == 5 && bus.Type == "C")
                            {
                                activeBus = bus.Id;
                                eventChangeBusPower(allEvents[idEvent].P);
                            }
                            else if (allEvents[idEvent].ID == 8 && bus.Type == "H")
                            {
                                activeBus = bus.Id;
                                eventChangeBusPower(allEvents[idEvent].P);
                            }
                        }
                    }
                    //writes on screen what happend in the event
                    showEventText = true;
                    activeBus = tempBus;

                    idEvent++;
                }

            }
            
        }

        public bool eventOccurs()
        {
            return showEventText;
        }

        public string getEventText()
        {
            return allEvents[idEvent - 1].Text;
        }

        public void clearEventText()
        {
            showEventText = false;
        }

        /// <summary>
        /// Adds a Bus to the List of current buses
        /// </summary>
        /// <param name="P"> Active Power P</param>
        /// <param name="U"> Voltage U</param>
        /// <param name="Theta"> Angle Theta</param>
        /// <param name="position"> Position of the bus on the screen</param>
        /// <param name="PMax">The Maximum P of the bus.</param>
        /// <param name="Type">The type of the bus</param>
        public void addBus(Vector2 position, double P, double PMax, double PMin, double U, double Theta, string Type)
        {
            Bus bus = new Bus(position, currentID + 1, P, PMax, PMin, U, Theta, Type, gDevice);
            //Include the different pictures regarding the Bustype
            if (Type == "S")
            {
                //include picture of outside grid?!
                bus.LoadContent("Sprites/slack", cManager);
            }
            else if (Type == "P")
            {
                //include picture of PV-System
                bus.LoadContent("Sprites/pv", cManager);
            }
            else if (Type == "W")
            {
                //include picture of windturbine
                bus.LoadContent("Sprites/windTower", cManager);
                bus.loadBlades(cManager);
            }
            else if (Type == "G")
            {
                //include picture of power plant
                bus.LoadContent("Sprites/generation", cManager);
            }
            else if (Type == "C")
            {
                //include picture of consumer for example house
                bus.LoadContent("Sprites/consumer", cManager);
            }
            else if (Type == "H")
            {
                bus.LoadContent("Sprites/hydro", cManager);
            }
            else
            {
                //include other picture (Fixbus or in general wrong letter
                bus.LoadContent("Sprites/bus2", cManager);
            }
            bus.setMouseRectangle();
            bus.loadBar(cManager);
            currentID++;

            buses.Add(bus);

            modelSolved = false;
        }

        /// <summary>
        /// Add a line to your model. Start and End ID must exist.
        /// </summary>
        /// <param name="start">The Bus ID at the start</param>
        /// <param name="end">The Bus ID at the end</param>
        /// <param name="R">Serial R of the line</param>
        /// <param name="X">Serial X of the line</param>
        /// <param name="pMax">Maximum power flow of line</param>
        public void addLine(int start, int end, double pMax, double R, double X)
        {
            if(start > currentID || end > currentID || start < 1 || end < 1)
            {
                return;
            }
            Vector2 startPos = buses[start - 1].position;
            Vector2 endPos = buses[end - 1].position;
            
            Line line = new Line(currentLineID + 1, start, startPos, end, endPos, pMax, R, X);
            currentLineID++;
            line.Arrow = arrow;
            line.LoadContent("Sprites/line", cManager);

            currentLines++;
            lines.Add(line);

            modelSolved = false;
        }

        public void addBattery(int busID, double power, double eMax, double initialCharge, double chargeCapacity, char allowsChange)
        {
            if (busID >= currentID + 1)
            {
                return;
            }
            Bus associatedBus = buses[0];
            foreach (Bus bus in buses)
            {
                if(bus.Id == busID)
                {
                    associatedBus = bus;
                }
            }
            bool changeAllowed = false;
            switch (allowsChange)
            {
                case 'Y':
                    changeAllowed = true;
                    break;
                case 'N':
                    break;
                default:
                    break;
            }

            if (associatedBus.Type == "H" || associatedBus.Type == "G")
            {
                changeAllowed = true;
            }
            Vector2 batteryPosition = associatedBus.position;
            Battery battery = new Battery(busID, power, eMax, initialCharge, chargeCapacity, batteryPosition, gDevice, changeAllowed);

            Texture2D linePixel = new Texture2D(gDevice, 1, 1);
            linePixel.SetData(new[] { Color.White});

            battery.loadLineTexture(linePixel);

            battery.LoadContent("Sprites/battery", cManager);
            battery.setMouseRectangle();
            battery.loadBar(cManager);

            batteries.Add(battery);
        }

        public void removeLine(int id)
        {
            if(id > lines.Count)
                return;

            int x = 0;
            foreach( Line line in lines)
            {
                if(line.ID == id)
                {
                    Line removedLine = lines[x];
                    removedLines.Add(removedLine);
                    lines.RemoveAt(x);
                    currentLines--;
                    modelSolved = false;
                    break;
                }
                x++;
            }
        }

        public void restoreLine(int id)
        {
            if(removedLines.Count == 0)
            {
                return;
            }
            int x = 0;
            foreach (Line line in removedLines)
            {
                if (line.ID == id)
                {
                    lines.Add(removedLines[x]);
                    removedLines.RemoveAt(x);
                    currentLines++;
                    modelSolved = false;
                    break;
                }
                x++;
            }
        }

        /// <summary>
        /// Set the power at Bus #busNr to the power provided
        /// </summary>
        /// <param name="busID">The ID of the bus to be set</param>
        /// <param name="power">The new power at the bus</param>
        public void setBusPower(int busID, double power)
        {
            buses[busID - 1].P = power;
            buses[busID - 1].PForBattery = buses[busID - 1].P;
            modelSolved = false;
        }

        public void eventChangeBusPower(double amount)
        {
            buses[activeBus - 1].P += amount;
            buses[activeBus - 1].PForBattery += amount;
            modelSolved = false;
        }


        // dynamicWindChange changes the power of the windturbines dynamically if activated by a wind event.
        // It's a linear up and down function with its maximum at the middle between starttime and endtime of the event
        public void dynamicWindChange()
        {
                float middleHour = ((windEventEndingTime - windEventStartingTime) / 2);
                float middleTime = windEventStartingTime + middleHour;

                if(timeInLevelHour < windEventStartingTime || timeInLevelHour > windEventEndingTime)
                {
                    foreach (Bus bus in buses)
                    {
                        if (bus.Type == "W")
                        {
                            bus.P = 0f;
                            bus.PForBattery = 0f;
                            modelSolved = false;
                        }
                    }
                }

                if (timeInLevelHour >= windEventStartingTime && timeInLevelHour < middleTime)
                {
                    foreach (Bus bus in buses)
                    {
                        if (bus.Type == "W")
                        {
                            bus.P = windScalingFactor * ((timeInLevelHour - windEventStartingTime) / middleHour * bus.PMin);
                            bus.PForBattery = windScalingFactor * ((timeInLevelHour - windEventStartingTime) / middleHour * bus.PMin);

                            modelSolved = false;
                        }
                    }
                }

                if(timeInLevelHour >= middleTime && timeInLevelHour <= windEventEndingTime)
                {
                    foreach (Bus bus in buses)
                    {
                        if (bus.Type == "W")
                        {
                            bus.P = windScalingFactor * ((windEventEndingTime - timeInLevelHour) / middleHour * bus.PMin);
                            bus.PForBattery = windScalingFactor * ((windEventEndingTime - timeInLevelHour) / middleHour * bus.PMin);
                            modelSolved = false;
                        }
                    }
                }
            
        }

        public void dynamicPhotovoltaicChange()
        {
            if (timeInLevelHour > solarEventEndingTime && solarScalingFactor != 1f)
            {
                solarScalingFactor = 1f;
            }

            if (timeInLevelHour < 6 || timeInLevelHour > 18)
            {
                foreach (Bus bus in buses)
                {
                    if (bus.Type == "P")
                    {
                        bus.P = 0f;
                        bus.PForBattery = 0f;
                        modelSolved = false;
                    }
                }
            }
            if (timeInLevelHour >= 6 && timeInLevelHour < 12)
            {
                foreach (Bus bus in buses)
                {
                    if(bus.Type == "P")
                    {
                        bus.P = solarScalingFactor * ((timeInLevelHour - 6f) / 6f * bus.PMin);
                        bus.PForBattery = solarScalingFactor * ((timeInLevelHour - 6f) / 6f * bus.PMin);
                        modelSolved = false;
                    }
                }
            }
            if(timeInLevelHour >= 12 && timeInLevelHour <= 18)
            {
                foreach (Bus bus in buses)
                {
                    if (bus.Type == "P")
                    {
                        bus.P = solarScalingFactor*((18f - timeInLevelHour) / 6f * bus.PMin);
                        bus.PForBattery = solarScalingFactor * ((18f - timeInLevelHour) / 6f * bus.PMin);
                        modelSolved = false;
                    }
                }
            }
            
        }

        //updateBatteries decides the state of the Battery if it is not manually changeable
        public void updateBatteries()
        {
            foreach (Battery battery in batteries)
            {
                battery.Update(timeInLevel);
                int busID = battery.BusID;
                Bus bus = buses[busID - 1];

                // If the battery does not allow the user to change it, run this.
                if (!battery.AllowsChange)
                {

                    if (bus.Type == "P" || bus.Type == "W")
                    {
                        if (bus.PForBattery < 0 && battery.E < battery.EMax)
                        {
                            battery.changeChargeState(BatteryChargeState.Charging, timeInLevel);
                            buses[busID - 1].P = buses[busID - 1].PForBattery + battery.Power;
                        }
                        else if (bus.PForBattery == 0 && battery.E > 0)
                        {
                            battery.changeChargeState(BatteryChargeState.Discharging, timeInLevel);
                            buses[busID - 1].P = buses[busID - 1].PForBattery - battery.Power;
                        }
                        else if (battery.E >= battery.EMax || battery.E == 0)
                        {
                            battery.changeChargeState(BatteryChargeState.Idle, timeInLevel);
                            buses[busID - 1].P = buses[busID - 1].PForBattery;
                        }
                    }
                    else if (buses[busID - 1].Type == "C")
                    {
                        if (buses[0].P > 0.5 && battery.E < battery.EMax && battery.ChargeState != BatteryChargeState.Charging && (buses[0].P - battery.Power) > 0)
                        {
                            battery.changeChargeState(BatteryChargeState.Charging, timeInLevel);
                            buses[busID - 1].P = buses[busID - 1].PForBattery + battery.Power;
                        }
                        else if (buses[0].P < -0.5 && battery.E > 0 && battery.ChargeState != BatteryChargeState.Discharging && (buses[0].P + battery.Power) < 0)
                        {
                            battery.changeChargeState(BatteryChargeState.Discharging, timeInLevel);
                            buses[busID - 1].P = buses[busID - 1].PForBattery - battery.Power;
                        }
                        else if ((battery.E >= battery.EMax || battery.E == 0) && battery.ChargeState != BatteryChargeState.Idle)
                        {
                            battery.changeChargeState(BatteryChargeState.Idle, timeInLevel);
                            buses[busID - 1].P = buses[busID - 1].PForBattery;
                        }
                    }
                }

                // Else run this.
                else
                {
                    if ( battery.ChargeState == BatteryChargeState.Charging && buses[busID-1].PForBattery == 0)
                    {
                        battery.changeChargeState(BatteryChargeState.Idle, timeInLevel);
                    }
                    if (battery.E == 0)
                    {
                        battery.changeChargeState(BatteryChargeState.Idle, timeInLevel);
                        buses[busID - 1].P = buses[busID - 1].PForBattery;
                    }
                    if (battery.E >= battery.EMax)
                    {
                        battery.changeChargeState(BatteryChargeState.Idle, timeInLevel);
                        buses[busID - 1].P = buses[busID - 1].PForBattery;
                    }
                    if (battery.ChargeState == BatteryChargeState.Idle)
                    {
                        buses[busID - 1].P = buses[busID - 1].PForBattery;
                    }
                    if (battery.ChargeState == BatteryChargeState.Charging)
                    {
                        buses[busID - 1].P = buses[busID - 1].PForBattery + battery.Power;
                    }
                    if (battery.ChargeState == BatteryChargeState.Discharging)
                    {
                        buses[busID - 1].P = buses[busID - 1].PForBattery - battery.Power;
                    }
                }
            }
        }

        public void modifyBatteryState(int batteryID, String newState)
        {
            BatteryChargeState modifyState = BatteryChargeState.Idle;
            switch (newState)
            {
                case "Charge":
                    modifyState = BatteryChargeState.Charging;
                    break;
                case "Discharge":
                    modifyState = BatteryChargeState.Discharging;
                    break;
                case "Idle":
                    modifyState = BatteryChargeState.Idle;
                    break;
                default:
                    break;
            }
            foreach (Battery battery in batteries)
            {
                if (battery.BusID == batteryID)
                {
                    int busID = battery.BusID;
                    Bus bus = buses[busID-1];
                    // Do not allow the user to charge if the Power used to charge goes above PMax
                    // and similarly don't allow Discharging if it exceeds the minimum Use.
                    // If the battery should be changed to Idle, allow that all the time.
                    if ( ( (modifyState == BatteryChargeState.Charging && (bus.P + battery.Power) <= bus.PMax) || (modifyState == BatteryChargeState.Discharging && (bus.P - battery.Power) >= bus.PMin) ) || modifyState == BatteryChargeState.Idle )
                    {
                        battery.changeChargeState(modifyState, timeInLevel);
                    }
                }
            }
        }

        public bool isActiveBatteryChangeable()
        {
            foreach (Battery battery in batteries)
            {
                if (battery.BusID == activeBattery)
                {
                    return battery.AllowsChange;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds to the power at bus #busNr by the amount provided.
        /// Negative values subtract from the power
        /// </summary>
        /// <param name="amount">The change in power</param>
        public void manualChangeBusPower(double amount)
        {
            if(buses[activeBus - 1].Type == "W" || buses[activeBus - 1].Type == "P" || buses[activeBus - 1].Type == "S" || buses[activeBus - 1 ].Type == "C")
            {
                return;
            }
            if (buses[activeBus - 1].P <= 0)
            {
                amount = -amount;
            }
            buses[activeBus - 1].P += amount;
            buses[activeBus - 1].PForBattery += amount;

            modelSolved = false;
        }

        public void toggleLineText()
        {
            foreach (Line line in lines)
            {
                line.toggleText();
            }
        }

        public void increaseLinePower()
        {
            foreach (Line line in lines)
            {
                line.PMax = 30;
            }
        }

        public bool containsBatteries()
        {
            return (batteries.Count > 0);
        }

        public Boolean losingCondition()
        {
            foreach(Line line in lines)
            {
                if (Math.Abs(line.P1) >= line.PMax)
                {
                    return true;
                }
            }
            return false;
        }

        public Boolean winCondition()
        {
            return (timeInLevelHour >= 24) ? true : false;
        }

        public void Update(MouseState mouseState, float elapsedTime)
        {
            /*if (!mouseClicked)
            {
                checkMouse(mouseState);
            }*/
            if(mouseState.LeftButton == ButtonState.Released)
            {
                mouseClicked = false;
            }

            // timeInLevel equals the minutes that passed in "Leveltime" since Level startet. elapsedTime-startTime gives real time since level started. Times 6 is taken that a level will have 2 minutes and represents 24 hours -> 1s in game time equals 12 min in "Leveltime"
            timeInLevel = (elapsedTime - startTime) * 12;
            // gives the time in hours
            timeInLevelHour = timeInLevel / 60;

            loadEvent();

            dynamicPhotovoltaicChange();
            dynamicWindChange();
            if (batteries.Count > 0)
            {
                updateBatteries();
            }
            foreach (Line line in lines)
            {
                line.Update();
            }

            solve();
        }

        public void checkMouse(MouseState mouseState)
        {
            if(mouseState.LeftButton == ButtonState.Pressed && !mouseClicked)
            {
                mouseClicked = true;
                bool foundBus = false;
                foreach (Bus bus in buses)
                {
                    if (bus.MouseRectangle.Contains(mouseState.Position))
                    {
                        activeBus = bus.Id;
                        foundBus = true;
                    }
                }
                foreach (Battery battery in batteries)
                {
                    if (battery.MouseRectangle.Contains(mouseState.Position))
                    {
                        activeBattery = battery.BusID;
                        foundBus = true;
                    }
                }
                if(new Rectangle(20, gDevice.Viewport.Height - 150, 500, 100).Contains(mouseState.Position)){
                    // Ugly little solution to not make it deselect bus.
                    foundBus = true;
                }
                if (!foundBus)
                {
                    activeBus = 1;
                    activeBattery = NONE;
                }
            }
            
        }

        public Vector2 getActiveBatteryPosition()
        {
            foreach (Battery battery in batteries)
            {
                if (battery.BusID == activeBattery)
                {
                    return battery.position;
                }
            }
            return Vector2.Zero;
        }

        public void drawBatteryInfo(SpriteBatch spriteBatch, SpriteFont theText)
        {
            string batteryText = (isActiveBatteryChangeable() == true) ? "Active Battery:" : "Passive Battery:";
            
            if (activeBattery != NONE)
            {
                spriteBatch.DrawString(theText, batteryText, new Vector2(210, gDevice.Viewport.Height - 145), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.245f);
                foreach (Battery battery in batteries)
                {
                    if (battery.BusID == activeBattery)
                    {
                        spriteBatch.DrawString(theText, battery.toString(), new Vector2(210, gDevice.Viewport.Height - 127), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.259f);
                    }
                }
            }
        }

        public Vector2 getActiveBusPosition()
        {
            return buses[activeBus - 1].position;
        }

        public void drawBusInfo(SpriteBatch spriteBatch, SpriteFont theText)
        {
            spriteBatch.DrawString(theText, "Active Bus: " + activeBus, new Vector2(30, gDevice.Viewport.Height - 145), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.245f);

            spriteBatch.DrawString(theText, buses[activeBus - 1].toString(), new Vector2(30, gDevice.Viewport.Height - 127), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.245f);
        }


        public void Draw(SpriteBatch spriteBatch, SpriteFont theText)
        {
            gameBackground.Draw(spriteBatch);
            foreach (Bus bus in buses)
            {
                bus.Draw(spriteBatch, theText);
            }

            foreach (Line line in lines)
            {
                line.Draw(spriteBatch, theText);
            }

            if(batteries.Count > 0)
            {
                foreach(Battery battery in batteries){
                    battery.Draw(spriteBatch);
                }
            }            
            spriteBatch.DrawString(theText, "Time: " + Math.Truncate(timeInLevelHour)+ ":" + (int)Math.Truncate((timeInLevel % 60)) / 10 + "0" , new Vector2(20, gDevice.Viewport.Height - 23), Color.White, 0.0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.245f);
        }

        /// <summary>
        /// Assembles a string describing the whole model.
        /// If model is empty it will say so.
        /// </summary>
        /// <returns>The assembled string</returns>
        public String toString()
        {
            String text = "" ;
            if (currentID == 0)
            {
                return "Model is empty";
            }
            else
            {
                /*text += "Buses: \n";
                foreach(Bus bus in buses)
                {
                    text += bus.toString();
                    text += "\n";
                }*/
                text += "\nLines: \n";
                foreach (Line line in lines)
                {
                    text += line.toString();
                    text += "\n";
                }

            }


            return text;
        }
    }
}
