using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.IO;
using System.Collections.Generic;

namespace GridGame
{
    /// <summary>
    /// This is the main type for the grid game.
    /// Handles all resource management and updating/drawing.
    /// </summary>
    public class TheGridGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont theText;
        SpriteFont mainMenuFont;
        SoundEffect buttonSound;
        SoundEffectInstance buttonSoundInstance;
        
        Vector2 screenCenter;

        bool losingCondition;
        float elapsedTime;
        private float losingCountdown;
        private float losingCountdownFrom;
        private float countdownStartTime;
        bool countdownStarted;
        // This is the constant that the active battery ID in the power model is, if none is selected.
        const int NONE = 999;

        private bool paused = false;
        private bool pauseKeyDown = false;
        private bool powerEvent;
        private bool eventTriggered;
        private bool eventWasOnBeforeMenu;
        GameState gameState;
        HowToPlayState howToPlayState;

        enum GameState
        {  
            MainMenu,
            Playing,
            Lost,
            Won,
            LevelSelect,
            PauseMenu,
            HowToPlay,
            Options,
            Credits
        }

        enum HowToPlayState
        {
            FromPlaying,
            FromMenu
        }

        ButtonState previousClickState;
        bool lineTextOn;
        // Boolean to make sure only 1 keypress is processed each time.
        bool keyPressed;

        //These rectangles are the position of panels when playing.
        Rectangle barPosition;
        Rectangle busInfoPosition;
        Rectangle batteryInfoPosition;

        //This is the power model
        PowerModel powerModel;

        Sprite menuBackground;
        Texture2D selectedRectangle;

        private string selectedLevel;
        private string levelDescription;
        private string howToPlay;
        DirectoryInfo levelsDirectory;
        private int selectedLevelSet;
        private int maxLevelSets;
        private List<UIButton> levelSelectButtons;
        private List<UIButton> mainMenuButtons;
        private List<UIButton> pauseMenuButtons;
        private UIButton howToPlayBack;
        private UIButton menuButton;
        private UIButton playLevelButton;
        private UIButton goBackButton;
        private UIButton nextLevels;
        private UIButton previousLevels;
        private List<UIButton> batteryButtons;
        private UIButton disableSoundButton;
        private UIButton losingCountdownDifficulty;

        /// <summary>
        /// The constructor of the game class.
        /// </summary>
        public TheGridGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1024;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            gameState = GameState.MainMenu;
            howToPlayState = HowToPlayState.FromMenu;

            this.IsMouseVisible = true;
            menuBackground = new Sprite(Vector2.Zero, this.GraphicsDevice);

            powerModel = new PowerModel(this.GraphicsDevice, this.Content);

            elapsedTime = 0;

            // Initialize if a Click is ongoing and the Text is turned on.
            previousClickState = ButtonState.Released;
            lineTextOn = true;

            losingCondition = false;
            powerEvent = false;
            eventTriggered = false;
            eventWasOnBeforeMenu = false;

            losingCountdown = 0;
            losingCountdownFrom = 10f;
            countdownStartTime = 0;
            countdownStarted = false;




            selectedLevel = "";
            selectedLevelSet = 1;
            maxLevelSets = 1;
            howToPlay = "";

            StreamReader readerGameDescripion = new StreamReader("../../../../Content/Manual/GameDescription.txt");
            string lineGameDescription;
            while ((lineGameDescription = readerGameDescripion.ReadLine()) != null)
            {
                levelDescription += lineGameDescription;
                levelDescription += "\n";
            }

            StreamReader reader = new StreamReader("../../../../Content/Manual/HowToPlay.txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                howToPlay += line;
                howToPlay += "\n";
            }

            // Set up the rectangles for clicks on the level select screen
            levelsDirectory = new DirectoryInfo("../../../../Content/Level");
            int buttonY = 220;

            
            // Make new Lists for the different buttons.
            levelSelectButtons = new List<UIButton>();
            mainMenuButtons = new List<UIButton>();
            pauseMenuButtons = new List<UIButton>();
            batteryButtons = new List<UIButton>();

            // Make the level buttons by searching the level directory
            foreach (FileInfo fi in levelsDirectory.GetFiles("Level_*"))
            {
                char[] deletingChar = { '.', 't', 'x' };
                string nameOnButton = fi.Name.TrimEnd(deletingChar);
                levelSelectButtons.Add(new UIButton(new Rectangle(120, buttonY, 160, 50),nameOnButton, this.GraphicsDevice));
                buttonY += 60;
                // If there are more than 6 level buttons, reset the y position.
                if (buttonY >= 535)
                {
                    buttonY = 220;
                    maxLevelSets += 1;
                }
            }

            // Main menu buttons
            buttonY = 220;
            mainMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 220, 240, 50),"Play", this.GraphicsDevice));
            mainMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 280, 240, 50), "How to play", this.GraphicsDevice));
            mainMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 340, 240, 50), "Credits", this.GraphicsDevice));
            mainMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 400, 240, 50), "Options", this.GraphicsDevice));
            mainMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 460, 240, 50), "Exit", this.GraphicsDevice));

            // Pause menu buttons
            buttonY = 220;
            pauseMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 220, 240, 50), "Resume", this.GraphicsDevice));
            pauseMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 280, 240, 50), "Main Menu", this.GraphicsDevice));
            pauseMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 340, 240, 50), "Select Level", this.GraphicsDevice));
            pauseMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 400, 240, 50), "How to play", this.GraphicsDevice));
            pauseMenuButtons.Add(new UIButton(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 120, 460, 240, 50), "Exit", this.GraphicsDevice));

            batteryButtons.Add(new UIButton(new Rectangle(400, graphics.PreferredBackBufferHeight - 140, 100, 25), "Charge", this.GraphicsDevice));
            batteryButtons.Add(new UIButton(new Rectangle(400, graphics.PreferredBackBufferHeight - 110, 100, 25), "Discharge", this.GraphicsDevice));
            batteryButtons.Add(new UIButton(new Rectangle(400, graphics.PreferredBackBufferHeight - 80, 100, 25), "Idle", this.GraphicsDevice));

            // "Back" button in how to play
            howToPlayBack = new UIButton(new Rectangle(120, this.graphics.PreferredBackBufferHeight - 170, 120, 50), "Back", this.GraphicsDevice);

            menuButton = new UIButton(new Rectangle(graphics.PreferredBackBufferWidth - 50, graphics.PreferredBackBufferHeight - 25, 40, 20), "MENU", this.GraphicsDevice);

            playLevelButton = new UIButton(new Rectangle(graphics.PreferredBackBufferWidth - 160 - 100 - 10, graphics.PreferredBackBufferHeight - 100 - 60, 160, 50), "Play", GraphicsDevice);
            previousLevels = new UIButton(new Rectangle(120, graphics.PreferredBackBufferHeight - 100 - 60, 75, 50), "<-", GraphicsDevice);
            nextLevels = new UIButton(new Rectangle(120 + 10 + 75 , graphics.PreferredBackBufferHeight - 100 - 60, 75, 50), "->", GraphicsDevice);
            goBackButton = new UIButton(new Rectangle(300 + 20, graphics.PreferredBackBufferHeight - 100 - 60, 160, 50), "Back", GraphicsDevice);


            // Positions for the UI elements and center of the screen.
            barPosition = new Rectangle(0, graphics.PreferredBackBufferHeight - 30, graphics.PreferredBackBufferWidth, 30);
            busInfoPosition = new Rectangle(20 , graphics.PreferredBackBufferHeight - 150, 170, 100);
            batteryInfoPosition = new Rectangle(200, graphics.PreferredBackBufferHeight - 150, 320, 100);
            screenCenter = new Vector2(this.graphics.PreferredBackBufferWidth / 2, this.graphics.PreferredBackBufferHeight / 2);

            disableSoundButton = new UIButton(new Rectangle((int) screenCenter.X- 80, 110, 160, 50), "Disable Sound", GraphicsDevice);
            losingCountdownDifficulty = new UIButton(new Rectangle((int)screenCenter.X - 80, 200, 160, 50), "10s (Hard)", GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the Spritefont
            theText = Content.Load<SpriteFont>("TextTest");
            mainMenuFont = Content.Load<SpriteFont>("MainMenuFont");

            menuBackground.LoadContent("BG/menuBackground", this.Content);
            selectedRectangle = Content.Load<Texture2D>("Sprites/selected");

            buttonSound = Content.Load<SoundEffect>("Sounds/clickButton");
            buttonSoundInstance = buttonSound.CreateInstance();
            buttonSoundInstance.Volume = 0.5f;
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void beginPause()
        {
            // TODO: Add pause logic for music or graphics if necessary.

            paused = true;
            gameState = GameState.PauseMenu;
            if (powerEvent)
            {
                eventWasOnBeforeMenu = true;
                powerEvent = false;
                paused = true;

            }
        }

        private void endPause()
        {
            // TODO: Resume audio if necessary

            paused = false;
            gameState = GameState.Playing;
            if (eventWasOnBeforeMenu)
            {
                powerEvent = true;
                eventWasOnBeforeMenu = false;
                paused = true;
                
            }
        }

        /// <summary>
        /// Check if pause key is down. If it is, set flag to pause the game to true.
        /// </summary>
        /// <param name="keyboardState">The current keyboard state.</param>
        private void checkPauseKey(KeyboardState keyboardState)
        {
            // Set the bool to check if the pause key is currently pressed at this moment.
            bool pauseKeyDownNow = keyboardState.IsKeyDown(Keys.Escape);

            // If it hasn't been pressed before and is being pressed right now.
            if (!pauseKeyDown && pauseKeyDownNow)
            {
                // If game is not paused yet, pause it.
                if (!paused || powerEvent)
                {
                    beginPause();
                }
                // Else stop the pause.
                else
                {
                    endPause();
                }
            }

            // Marks that the key is being held down for future updates to avoid the above if.
            pauseKeyDown = pauseKeyDownNow;
        }

        /// <summary>
        /// Handles the countdown to losing in case the player overloads a line.
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        public void handleLosingCondition(GameTime gameTime)
        {
            if (losingCondition && !countdownStarted)
            {
                countdownStarted = true;
                countdownStartTime = 0f;
                countdownStartTime = elapsedTime;
                losingCountdown = 0f;
            }

            if (countdownStarted)
            {
                losingCountdown = losingCountdownFrom - (elapsedTime - countdownStartTime);

                if( losingCountdown <= 0)
                {
                    gameState = GameState.Lost;
                }
            }

            if (!losingCondition)
            {
                countdownStarted = false;
                losingCountdown = 0f;
                countdownStartTime = 0f;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // Viewport to get current game window size
            Viewport viewport = GraphicsDevice.Viewport;
            // Current mouse state
            MouseState mouseState = Mouse.GetState();
            // Current keyboard state
            KeyboardState keyboardState = Keyboard.GetState();

            // If no keys are pressed "release" the flag and let keys register again.
            if (keyboardState.GetPressedKeys().Length == 0 && mouseState.LeftButton == ButtonState.Released)
            {
                keyPressed = false;
            }

            // Checks if mouse is released again.
            if (mouseState.LeftButton == ButtonState.Released)
            {
                previousClickState = ButtonState.Released;
            }

            if (gameState == GameState.MainMenu)
            {
                foreach(UIButton button in mainMenuButtons)
                {
                    button.Update(mouseState);
                }
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed)
                {
                    
                    keyPressed = true;
                    if (mainMenuButtons[0].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        gameState = GameState.LevelSelect;
                    }
                    if (mainMenuButtons[1].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        gameState = GameState.HowToPlay;
                    }
                    if (mainMenuButtons[2].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        gameState = GameState.Credits;
                    }
                    if (mainMenuButtons[3].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        gameState = GameState.Options;
                    }
                    if (mainMenuButtons[4].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        Exit();
                    }
                }   
            }

            if (gameState == GameState.Credits)
            {
                howToPlayBack.Update(mouseState);
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed && howToPlayBack.Rect.Contains(mouseState.Position))
                {
                    keyPressed = true;
                    buttonSoundInstance.Stop();
                    buttonSoundInstance.Play();
                    gameState = GameState.MainMenu;
                }
            }

            if (gameState == GameState.Options)
            {
                howToPlayBack.Update(mouseState);
                disableSoundButton.Update(mouseState);
                losingCountdownDifficulty.Update(mouseState);
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed && howToPlayBack.Rect.Contains(mouseState.Position))
                {
                    keyPressed = true;
                    buttonSoundInstance.Stop();
                    buttonSoundInstance.Play();
                    gameState = GameState.MainMenu;
                }
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed && disableSoundButton.Rect.Contains(mouseState.Position) && disableSoundButton.Text == "Disable Sound")
                {
                    keyPressed = true;
                    buttonSoundInstance.Volume = 0f;
                    disableSoundButton.Text = "Enable Sound";
                }
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed && disableSoundButton.Rect.Contains(mouseState.Position) && disableSoundButton.Text == "Enable Sound")
                {
                    keyPressed = true;
                    buttonSoundInstance.Volume = 0.5f;
                    disableSoundButton.Text = "Disable Sound";
                }
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed && losingCountdownDifficulty.Rect.Contains(mouseState.Position) && losingCountdownDifficulty.Text == "10s (Hard)")
                {
                    keyPressed = true;
                    losingCountdownDifficulty.Text = "14s (Medium)";
                    losingCountdownFrom = 14f;
                }
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed && losingCountdownDifficulty.Rect.Contains(mouseState.Position) && losingCountdownDifficulty.Text == "14s (Medium)")
                {
                    keyPressed = true;
                    losingCountdownDifficulty.Text = "18s (Easy)";
                    losingCountdownFrom = 18f;
                }
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed && losingCountdownDifficulty.Rect.Contains(mouseState.Position) && losingCountdownDifficulty.Text == "18s (Easy)")
                {
                    keyPressed = true;
                    losingCountdownDifficulty.Text = "10s (Hard)";
                    losingCountdownFrom = 10f;
                }
            }

            if (gameState == GameState.LevelSelect)
            {
                foreach (UIButton button in levelSelectButtons)
                {
                    button.Update(mouseState);
                }
                playLevelButton.Update(mouseState);
                goBackButton.Update(mouseState);
                nextLevels.Update(mouseState);
                previousLevels.Update(mouseState);

                if(mouseState.LeftButton == ButtonState.Pressed && !keyPressed)
                {
                    keyPressed = true;
                    if (nextLevels.Rect.Contains(mouseState.Position) && selectedLevelSet < maxLevelSets)
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        selectedLevelSet++;
                    }
                    if (previousLevels.Rect.Contains(mouseState.Position) && selectedLevelSet > 1)
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        selectedLevelSet--;
                    }
                    int amountOfLevels = levelSelectButtons.Count;
                    int buttonsChecked = 0;
                    int buttonsToSkip = 0;
                    buttonsToSkip = (selectedLevelSet - 1) * 6;
                        
                    foreach (UIButton button in levelSelectButtons)
                    {
                        if (buttonsChecked > 5)
                        {
                            continue;
                        }
                        if (buttonsToSkip >= 1)
                        {
                            buttonsToSkip--;
                            continue;
                        }
                        if (button.Rect.Contains(mouseState.Position))
                        {
                            buttonSoundInstance.Stop();
                            buttonSoundInstance.Play();
                            selectedLevel = button.Text;
                            levelDescription = "";

                            StreamReader reader = new StreamReader("../../../../Content/Level/" + selectedLevel+".txt");
                            string line;
                            while ((line = reader.ReadLine()) != "BUS")
                            {
                                levelDescription += line;
                                levelDescription += "\n";
                            }
                            
                        }
                        buttonsChecked += 1;
                    }

                        
                    if(playLevelButton.Rect.Contains(mouseState.Position) && selectedLevel.Length > 2)
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        powerModel = new PowerModel(this.GraphicsDevice, this.Content);
                        powerModel.loadLevel(selectedLevel, elapsedTime);
                                                
                        losingCondition = false;
                        losingCountdown = 0f;
                        countdownStarted = false;
                        gameState = GameState.Playing;
                        howToPlayState = HowToPlayState.FromPlaying;

                        // Need to unpause so simulation runs, in case it was paused and exited before.
                        paused = false;
                    }
                    if (goBackButton.Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        gameState = GameState.MainMenu;
                    }
                }
            }

            if (gameState == GameState.Lost)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    paused = false;
                    losingCondition = false;
                    gameState = GameState.LevelSelect;
                    howToPlayState = HowToPlayState.FromMenu;
                }
            }

            if (gameState == GameState.Won)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    gameState = GameState.LevelSelect;
                    howToPlayState = HowToPlayState.FromMenu;
                }
            }

            if (gameState == GameState.PauseMenu)
            {
                checkPauseKey(keyboardState);

                foreach (UIButton button in pauseMenuButtons)
                {
                    button.Update(mouseState);
                }
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed)
                {
                    keyPressed = true;
                    if(pauseMenuButtons[0].Rect.Contains(mouseState.Position)){
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        endPause();
                    }

                    if(pauseMenuButtons[1].Rect.Contains(mouseState.Position)){
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        eventTriggered = false;
                        powerModel.clearEventText();
                        eventWasOnBeforeMenu = false;
                        gameState = GameState.MainMenu;
                        howToPlayState = HowToPlayState.FromMenu;
                    }

                    if (pauseMenuButtons[2].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        eventTriggered = false;
                        powerModel.clearEventText();
                        eventWasOnBeforeMenu = false;
                        gameState = GameState.LevelSelect;
                        howToPlayState = HowToPlayState.FromMenu;
                    }

                    if (pauseMenuButtons[3].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        gameState = GameState.HowToPlay;
                    }

                    if (pauseMenuButtons[4].Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        Exit();
                    }
                }
            }

            if (gameState == GameState.HowToPlay)
            {
                howToPlayBack.Update(mouseState);
                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed)
                {
                    if (howToPlayBack.Rect.Contains(mouseState.Position))
                    {
                        buttonSoundInstance.Stop();
                        buttonSoundInstance.Play();
                        // Don't try to go back to GameState.Playing if you come from the menu, otherwise there might be no powermodel and it would crash.
                        gameState = (howToPlayState == HowToPlayState.FromMenu) ? GameState.MainMenu : GameState.PauseMenu;
                    }
                }
            }

            if (gameState == GameState.Playing)
            {
                // If there is a power Event show the text and pause the game.
                if (powerModel.eventOccurs() && !eventTriggered && (losingCountdown>=0.05 || losingCondition == false))
                {
                    powerEvent = true;
                    paused = true;
                    eventTriggered = true;
                }

                // If the player presses space the event text will close and the game resumes.
                if (powerEvent && (keyboardState.IsKeyDown(Keys.Space)))
                {
                    powerEvent = false;
                    paused = false;
                    eventTriggered = false;
                    powerModel.clearEventText();
                }
                checkPauseKey(keyboardState);
                menuButton.Update(mouseState);

                if (mouseState.LeftButton == ButtonState.Pressed && previousClickState == ButtonState.Released)
                {
                    previousClickState = ButtonState.Pressed;
                    if (menuButton.Rect.Contains(mouseState.Position))
                    {
                        beginPause();
                    }
                }
                
                if(keyboardState.IsKeyDown(Keys.P) && !keyPressed)
                {
                    keyPressed = true;
                    paused = (paused == false) ? true : false;
                }

                if(keyboardState.IsKeyDown(Keys.M) && keyboardState.IsKeyDown(Keys.I) && keyboardState.IsKeyDown(Keys.R) && keyboardState.IsKeyDown(Keys.A))
                {
                    gameState = GameState.Won;
                }

                if (keyboardState.IsKeyDown(Keys.M) && keyboardState.IsKeyDown(Keys.O) && keyboardState.IsKeyDown(Keys.E) && keyboardState.IsKeyDown(Keys.R) && keyboardState.IsKeyDown(Keys.U))
                {
                    powerModel.increaseLinePower();
                }

                if (mouseState.LeftButton == ButtonState.Pressed && !keyPressed)
                {
                    keyPressed = true;
                    powerModel.checkMouse(mouseState);

                    if (powerModel.containsBatteries())
                    {
                        if (powerModel.isActiveBatteryChangeable())
                        {
                            batteryInfoPosition = new Rectangle(200, graphics.PreferredBackBufferHeight - 150, 320, 100);
                        }
                        else
                        {
                            batteryInfoPosition = new Rectangle(200, graphics.PreferredBackBufferHeight - 150, 200, 100);
                        }

                        foreach (UIButton button in batteryButtons)
                        {
                            button.Update(mouseState);
                            if (mouseState.LeftButton == ButtonState.Pressed && button.Rect.Contains(mouseState.Position) && !paused)
                            {
                                if (powerModel.isActiveBatteryChangeable())
                                {
                                    powerModel.modifyBatteryState(powerModel.ActiveBattery, button.Text);
                                }

                            }
                        }
                    }  
                }

                // Checking for "paused" might be redundant with the gameState.PauseMenu now, but is still necessary for powerEvent.
                // Possibly rename it?
                if (!paused)
                {
                    elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    losingCondition = (powerModel.losingCondition() == true) ? true : false;
                    handleLosingCondition(gameTime);

                    if (powerModel.winCondition())
                    {
                        paused = true;
                        gameState = GameState.Won;
                    }

                    if (keyboardState.IsKeyDown(Keys.L) && !keyPressed){
                        keyPressed = true;
                        powerModel.removeLine(2);
                    }

                    if (keyboardState.IsKeyDown(Keys.R) && !keyPressed)
                    {
                        keyPressed = true;
                        powerModel.restoreLine(2);
                    }


                    // Update the Power Model
                    powerModel.Update(mouseState, elapsedTime);

                    // Changes power on clicked bus
                    if (keyboardState.IsKeyDown(Keys.Up))
                    {
                        powerModel.manualChangeBusPower(0.01);
                    }


                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        powerModel.manualChangeBusPower(-0.01);
                    }


                    if (keyboardState.IsKeyDown(Keys.T) && lineTextOn && !keyPressed)
                    {
                        keyPressed = true;
                        lineTextOn = false;
                        powerModel.toggleLineText();
                    }


                    if (keyboardState.IsKeyDown(Keys.T) && !lineTextOn && !keyPressed)
                    {
                        keyPressed = true;
                        lineTextOn = true;
                        powerModel.toggleLineText();
                    }

                }
            }

            base.Update(gameTime);
          
        }

        // Draws a rectangle at depth 0.2f.
        private void drawRectangle(Rectangle coordinates, Color color)
        {
            Texture2D rectangle = new Texture2D(this.GraphicsDevice, 1, 1);
            rectangle.SetData(new[] { color });

            spriteBatch.Draw(rectangle, coordinates, null, color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.2f);
        }

        // Draws a rectangle at depth 0.25f, behind regular rectangles.
        private void drawBackgroundRectangle(Rectangle coordinates, Color color)
        {
            Texture2D rectangle = new Texture2D(this.GraphicsDevice, 1, 1);
            rectangle.SetData(new[] { color });

            spriteBatch.Draw(rectangle, coordinates, null, color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.25f);
        }

        private void drawDeeperBackgroundRectangle(Rectangle coordinates, Color color)
        {
            Texture2D rectangle = new Texture2D(this.GraphicsDevice, 1, 1);
            rectangle.SetData(new[] { color });

            spriteBatch.Draw(rectangle, coordinates, null, color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.26f);
        }

        private void drawPauseMenu()
        {
            drawBackgroundRectangle(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 150, 150, 300, 500), Color.DarkGray);
            Vector2 textMid = theText.MeasureString("PAUSED");
            spriteBatch.DrawString(theText, "PAUSED", new Vector2(screenCenter.X - textMid.X / 2, 170), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            
            int textY = 220;
            foreach (UIButton button in pauseMenuButtons)
            {
                // Measure the text
                textMid = theText.MeasureString(button.Text);
                button.Draw(spriteBatch);
                // Draw text centered on button
                spriteBatch.DrawString(theText, button.Text, new Vector2(screenCenter.X - textMid.X / 2, textY + 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
                textY += 60;
            }
            
        }

        private void drawMainMenu()
        {
            drawBackgroundRectangle(new Rectangle(100, 100, this.graphics.PreferredBackBufferWidth - 200, 100), Color.DarkGray);
            drawBackgroundRectangle(new Rectangle(this.graphics.PreferredBackBufferWidth / 2 - 140, 210, 280, 340), Color.DarkGray);
            Vector2 textMid = mainMenuFont.MeasureString("THE GRID GAME");
            spriteBatch.DrawString(mainMenuFont, "THE GRID GAME", new Vector2(screenCenter.X - textMid.X / 2, 120), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            
            int buttonY = 220;

            foreach (UIButton button in mainMenuButtons)
            {
                textMid = theText.MeasureString(button.Text);
                button.Draw(spriteBatch);
                spriteBatch.DrawString(theText, button.Text, new Vector2(screenCenter.X - textMid.X / 2, buttonY + 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
                buttonY += 60;
            }
        }

        private void drawBatteryButtons()
        {
            Vector2 textMid = Vector2.Zero;
            foreach (UIButton button in batteryButtons)
            {
                textMid = theText.MeasureString(button.Text);
                button.Draw(spriteBatch);
                spriteBatch.DrawString(theText, button.Text, new Vector2(button.Rect.X + button.Rect.Width / 2f - textMid.X / 2f * 0.75f, button.Rect.Y + button.Rect.Height / 2f - textMid.Y / 2f * 0.75f), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0.1f);
            }
        }

        private void drawEndMessage(GameState endState)
        {
            string message = "";
            if (endState == GameState.Lost)
            {
                message = "You Lost \n \nPress Space to go \nto level selection.";
            }
            else if(endState == GameState.Won)
            {
                message = "You win! \nCongratulations! \nPress Space to go to level selection.";
            }
            Vector2 textSize = theText.MeasureString(message);
            drawBackgroundRectangle(new Rectangle((int)screenCenter.X - (int)textSize.X / 2 - 20,
                                    (int)screenCenter.Y - (int)textSize.Y / 2 - 20,
                                    (int) textSize.X + 40,
                                    (int) textSize.Y + 40),
                                    Color.DarkGray);
            
            spriteBatch.DrawString(theText, message , new Vector2(screenCenter.X - textSize.X / 2 , screenCenter.Y - textSize.Y / 2 ), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
        }

        private void drawLevelSelectMenu()
        {
            // Top rectangle for game title. Dimensions width - 200 x 100, pos: (100, 100)
            drawBackgroundRectangle(new Rectangle(100, 100, this.graphics.PreferredBackBufferWidth - 200, 100), Color.DarkGray);
            // Level button rectangle. Dim: 200 x height - 240 - 100 (because 100 from button and 210 from top.)
            drawBackgroundRectangle(new Rectangle(100, 210, 200, graphics.PreferredBackBufferHeight - 210 - 100), Color.DarkGray);
            String selectedSet = (selectedLevelSet + "/" + maxLevelSets);
            Vector2 levelMid = theText.MeasureString(selectedSet);
            spriteBatch.DrawString(theText, selectedSet, new Vector2(200 - levelMid.X / 2, graphics.PreferredBackBufferHeight - 100 - 90), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            // Level description. Dim: (width - 310 left - 100 right x height - 210 top - 100 bottom). 
            drawBackgroundRectangle(new Rectangle(310, 210, graphics.PreferredBackBufferWidth - 310 - 100, graphics.PreferredBackBufferHeight - 210 - 100), Color.DarkGray);
            Vector2 textMid = mainMenuFont.MeasureString("THE GRID GAME");
            spriteBatch.DrawString(mainMenuFont, "THE GRID GAME", new Vector2(screenCenter.X - textMid.X / 2, 120), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            textMid = mainMenuFont.MeasureString(selectedLevel);
            spriteBatch.DrawString(mainMenuFont, selectedLevel, new Vector2(330, 230), Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.1f);
            spriteBatch.DrawString(theText, levelDescription, new Vector2(330, 230 + textMid.Y / 2 + 20), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);

            int buttonY = 220;
            int amountOfLevels = levelSelectButtons.Count;
            int buttonsChecked = 0;
            int buttonsToSkip = 0;
            buttonsToSkip = (selectedLevelSet - 1) * 6;

            foreach(UIButton button in levelSelectButtons)
            {
                if (buttonsChecked > 5)
                {
                    continue;
                }
                if (buttonsToSkip >= 1)
                {
                    buttonsToSkip--;
                    continue;
                }
                textMid = theText.MeasureString(button.Text);
                button.Draw(spriteBatch);
                spriteBatch.DrawString(theText, button.Text, new Vector2(button.Rect.X + button.Rect.Width / 2 - textMid.X / 2, button.Rect.Y + button.Rect.Height / 2 - textMid.Y / 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
                //buttonY += 60;
                buttonsChecked += 1;
            }
            if(selectedLevel.Length > 2)
            {
                textMid = theText.MeasureString("Play");
                playLevelButton.Draw(spriteBatch);
                spriteBatch.DrawString(theText, playLevelButton.Text, new Vector2(playLevelButton.Rect.X + (playLevelButton.Rect.Width - textMid.X) / 2, playLevelButton.Rect.Y + (playLevelButton.Rect.Height - textMid.Y) / 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            }
            goBackButton.Draw(spriteBatch);
            textMid = theText.MeasureString(goBackButton.Text);
            spriteBatch.DrawString(theText, goBackButton.Text, new Vector2(goBackButton.Rect.X + (goBackButton.Rect.Width - textMid.X) / 2, goBackButton.Rect.Y + (goBackButton.Rect.Height - textMid.Y) / 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            textMid = theText.MeasureString(nextLevels.Text);
            nextLevels.Draw(spriteBatch);
            spriteBatch.DrawString(theText, nextLevels.Text, new Vector2(nextLevels.Rect.X + (nextLevels.Rect.Width - textMid.X) / 2, nextLevels.Rect.Y + (nextLevels.Rect.Height - textMid.Y) / 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            textMid = theText.MeasureString(previousLevels.Text);
            previousLevels.Draw(spriteBatch);
            spriteBatch.DrawString(theText, previousLevels.Text, new Vector2(previousLevels.Rect.X + (previousLevels.Rect.Width - textMid.X) / 2, previousLevels.Rect.Y + (previousLevels.Rect.Height - textMid.Y) / 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);

        }

        private void drawHowToPlay()
        {
            Texture2D rectangle = new Texture2D(this.GraphicsDevice, 1, 1);
            rectangle.SetData(new[] { Color.DarkGray });

            spriteBatch.Draw(rectangle, new Rectangle(100, 100, this.graphics.PreferredBackBufferWidth - 200, this.graphics.PreferredBackBufferHeight - 200), null, Color.DarkGray, 0.0f, Vector2.Zero, SpriteEffects.None, 0.24f);

            spriteBatch.DrawString(theText, howToPlay, new Vector2(110, 110), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);

            Vector2 textMid = theText.MeasureString(howToPlayBack.Text);
            howToPlayBack.Draw(spriteBatch);
            spriteBatch.DrawString(theText, howToPlayBack.Text, new Vector2(howToPlayBack.Rect.X + howToPlayBack.Rect.Width / 2 - textMid.X / 2, howToPlayBack.Rect.Y + 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);

        }

        private void drawOptions()
        {
            Rectangle rect = new Rectangle(100, 100, graphics.PreferredBackBufferWidth - 200, graphics.PreferredBackBufferHeight - 200);
            drawDeeperBackgroundRectangle(rect, Color.DarkGray);
            Vector2 textMid = theText.MeasureString(howToPlayBack.Text);
            howToPlayBack.Draw(spriteBatch);
            spriteBatch.DrawString(theText, howToPlayBack.Text, new Vector2(howToPlayBack.Rect.X + howToPlayBack.Rect.Width / 2 - textMid.X / 2, howToPlayBack.Rect.Y + 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            disableSoundButton.Draw(spriteBatch);
            textMid = theText.MeasureString(disableSoundButton.Text);
            spriteBatch.DrawString(theText, disableSoundButton.Text, new Vector2(disableSoundButton.Rect.X + disableSoundButton.Rect.Width / 2 - textMid.X / 2, disableSoundButton.Rect.Center.Y - textMid.Y/2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            textMid = theText.MeasureString(losingCountdownDifficulty.Text);
            losingCountdownDifficulty.Draw(spriteBatch);
            spriteBatch.DrawString(theText, losingCountdownDifficulty.Text, new Vector2(losingCountdownDifficulty.Rect.X + losingCountdownDifficulty.Rect.Width / 2 - textMid.X / 2, losingCountdownDifficulty.Rect.Center.Y - textMid.Y / 2), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            textMid = theText.MeasureString("Difficulty of Losing Countdown");
            spriteBatch.DrawString(theText, "Difficulty of Losing Countdown", new Vector2(losingCountdownDifficulty.Rect.Center.X - textMid.X / 2, losingCountdownDifficulty.Rect.Center.Y - textMid.Y / 2 - 40), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            
        }

        private void drawCredits()
        {
            Rectangle rect = new Rectangle(100, 100, graphics.PreferredBackBufferWidth - 200, graphics.PreferredBackBufferHeight - 200);
            Rectangle rect2 = new Rectangle(110, 110, graphics.PreferredBackBufferWidth - 220, graphics.PreferredBackBufferHeight - 220);
            drawDeeperBackgroundRectangle(rect , Color.LightGray);
            drawBackgroundRectangle(rect2, Color.DarkGray);
            Vector2 textMid = theText.MeasureString(howToPlayBack.Text);
            howToPlayBack.Draw(spriteBatch);
            spriteBatch.DrawString(theText, howToPlayBack.Text, new Vector2(howToPlayBack.Rect.X + howToPlayBack.Rect.Width / 2 - textMid.X / 2, howToPlayBack.Rect.Y + 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
            textMid = theText.MeasureString("Game programmed by: \nMoritz Staub \nMiriam Vonesch");
            string creditText = "Game programmed by:\nMoritz Staub\nMiriam Vonesch";
            String[] creditArray = creditText.Split('\n');
            Vector2 pos = new Vector2(rect.Center.X, rect.Center.Y - 30);
            foreach (String text in creditArray)
            {
                textMid = theText.MeasureString(text);
                spriteBatch.DrawString(theText, text, pos - textMid / 2, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.1f);
                pos += new Vector2(0, 30);
            }
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, null);
            if (gameState == GameState.MainMenu)
            {
                menuBackground.Draw(spriteBatch);
                drawMainMenu();
            }

            if (gameState == GameState.Options)
            {
                menuBackground.Draw(spriteBatch);
                drawOptions();
            }

            if (gameState == GameState.Credits)
            {
                menuBackground.Draw(spriteBatch);
                drawCredits();
            }

            if(gameState == GameState.Lost)
            {
                menuBackground.Draw(spriteBatch);
                drawEndMessage(GameState.Lost);
            }

            if(gameState == GameState.Won)
            {
                menuBackground.Draw(spriteBatch);
                drawEndMessage(GameState.Won);
            }

            if (gameState == GameState.LevelSelect)
            {
                menuBackground.Draw(spriteBatch);
                drawLevelSelectMenu();
            }

            if (gameState == GameState.PauseMenu)
            {
                drawPauseMenu();
            }

            if (gameState == GameState.HowToPlay)
            {
                menuBackground.Draw(spriteBatch);
                drawHowToPlay();
            }

            if (gameState == GameState.Playing || gameState == GameState.PauseMenu)
            {


                drawDeeperBackgroundRectangle(barPosition, Color.DarkGray);

                if (gameState != GameState.PauseMenu)
                {
                    drawDeeperBackgroundRectangle(busInfoPosition, Color.Gray);
                    spriteBatch.Draw(selectedRectangle, powerModel.getActiveBusPosition() - new Vector2(3, 3), selectedRectangle.Bounds, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.35f);
                    powerModel.drawBusInfo(spriteBatch, theText);
                }
                if (powerModel.containsBatteries() && gameState != GameState.PauseMenu && powerModel.ActiveBattery != NONE)
                {
                    drawDeeperBackgroundRectangle(batteryInfoPosition, Color.DarkGray);
                    powerModel.drawBatteryInfo(spriteBatch, theText);
                    spriteBatch.Draw(selectedRectangle, powerModel.getActiveBatteryPosition() - new Vector2(3,3), selectedRectangle.Bounds, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.35f);
                    if (powerModel.isActiveBatteryChangeable() && powerModel.ActiveBattery != NONE)
                    {
                        drawBatteryButtons();
                    }
                }
                
                if (losingCondition)
                {
                    Vector2 textSize = mainMenuFont.MeasureString("System breakdown in: ");
                    spriteBatch.DrawString(mainMenuFont, "System breakdown in: " + Math.Truncate(losingCountdown * 10) / 10, new Vector2(300, GraphicsDevice.Viewport.Height - barPosition.Height - textSize.Y - 10 ), Color.Red, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0.1f);

                }
                menuButton.Draw(spriteBatch);
                Vector2 textMid = theText.MeasureString(menuButton.Text);
                spriteBatch.DrawString(theText, menuButton.Text, new Vector2((menuButton.Rect.X + menuButton.Rect.Width / 2 - textMid.X / 2 + 16f), (menuButton.Rect.Y + textMid.Y / 2) - 8f), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0.1f);
                powerModel.Draw(spriteBatch, theText);

                if (powerEvent)
                {
                    string eventText = powerModel.getEventText();
                    eventText = eventText.Replace("\\n", "\n");
                    Vector2 textSize = theText.MeasureString(eventText);
                    Rectangle eventTextRectangle = new Rectangle((this.graphics.PreferredBackBufferWidth / 2 - (int)textSize.X / 2 - 20),
                                                this.graphics.PreferredBackBufferHeight / 2 - (int)textSize.Y / 2 - 20,
                                                (int)textSize.X + 40,
                                                (int)textSize.Y + 40);
                    drawBackgroundRectangle( eventTextRectangle, Color.Gray);
                    spriteBatch.DrawString(theText, eventText, new Vector2(eventTextRectangle.X + 20, eventTextRectangle.Y + 20) , Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.15f);
                }

            }
            spriteBatch.End();
            


            

            base.Draw(gameTime);
        }
    }
}
