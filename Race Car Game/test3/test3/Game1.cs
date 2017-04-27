using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using test3;

namespace test3
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        Song song;

        #region CAMERAS

        Camera currentCamera;

        List<Camera> cameras = new List<Camera>();

        bool changeCamTarget = false;
        int currentCamTarget = 0;

        bool changingCamera = false;


        #endregion
        #region ENVIRONMENT

        GameObject ground, groundyellow, groundred, groundgreen;

        bool greenmap = false, snowmap = false, yellowmap = false, redmap = false;
        public bool Greenmap
        {
            get { return greenmap; }
            set { greenmap = value; }
        }

        public bool Snowmap
        {
            get { return snowmap; }
            set { snowmap = value; }
        }


        public bool Yellowmap
        {
            get { return yellowmap; }
            set { yellowmap = value; }
        }

        public bool Redmap
        {
            get { return redmap; }
            set { redmap = value; }
        }

        Track track;
        //Sky sky;
        BillboardCross trees1, tree1red, tree1green, tree1yellow;
        BillboardSystem trees2, tree2red, tree2green, tree2yellow;
        BillboardSystem startSign;
        BillboardSystem clouds;

        const int trackWidth = 300;
        Vector2 startingPoint = new Vector2(-4000, 0);

        const float groundSize = 10000.0f;

        #endregion
        #region CARS

        internal List<Vehicle> vehicles = new List<Vehicle>();

        internal Car MyCar
        {
            get { return vehicles[0] as Car; }
        }
        const float carScale = 0.2f;
        const float opponentScale = 0.4f;
        const int opponentsCount = 2;

        #endregion
        #region GAMESTATES


        public GameState GameState
        {
            get { return currentGameState; }
            set { currentGameState = value; }
        }
        GameState currentGameState;

        bool isPaused = false;

        public Texture2D pauseBtnTex, Exit_BtnTex, music_on_Btn_Tex, music_off_Btn_Tex;
        public Texture2D pauseBtnTexS, Exit_BtnTexS, music_on_Btn_TexS, music_off_Btn_TexS;

        List<MenuButton> pause_buttons = new List<MenuButton>();

        #endregion
        #region INPUT

        KeyboardState currentKeyboardState = new KeyboardState();
        #region mouse
        public MouseState currentMouse, previousMouse;
        public bool MouseMoved
        {
            get
            {
                return currentMouse.X != previousMouse.X || currentMouse.Y != previousMouse.Y;
            }
        } // return true if the mouse moved 
        public bool MouseLeftClick
        {
            get
            {
                return previousMouse.LeftButton == ButtonState.Pressed &&
                   currentMouse.LeftButton == ButtonState.Released;
            }
        } // return true if the mouse left clicked
        #endregion
        #endregion
        const int lapsCount = 1;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            fbDeprofiler.DeProfiler.Run();
            Components.Add(new ScreenManager(this));


            graphics.PreferredBackBufferWidth = 1250;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 770;   // set this value to the desired height of your window

            previousMouse = currentMouse = Mouse.GetState();
        }

        protected override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            PrepareTrack();

            PrepareVehicles();

            PrepareEnvironment();

            LoadCameras();
            currentCamera = cameras[0];
            song = Content.Load<Song>("Game_Music");
            #region load texture
            Exit_BtnTex = Content.Load<Texture2D>("exit");
            Exit_BtnTexS = Content.Load<Texture2D>("exit selected");
            pauseBtnTex = Content.Load<Texture2D>("resume");
            pauseBtnTexS = Content.Load<Texture2D>("resume selected");
            music_on_Btn_Tex = Content.Load<Texture2D>("cutmypic (4)");
            music_on_Btn_TexS = Content.Load<Texture2D>("cutmypic (4)");
            music_off_Btn_Tex = Content.Load<Texture2D>("cutmypic (2)");
            music_off_Btn_TexS = Content.Load<Texture2D>("cutmypic (2)");
            #endregion
            Add_Elements_To_The_List();
            currentGameState = new MenuGS(this);
        }
        private void Add_Elements_To_The_List()
        {
            pause_buttons.Add(new MenuButton(pauseBtnTex, pauseBtnTexS, "PS", new Vector2(30, 270)));
            pause_buttons.Add(new MenuButton(Exit_BtnTex, Exit_BtnTexS, "EX", new Vector2(40, 420)));
            pause_buttons.Add(new MenuButton(music_on_Btn_Tex, music_on_Btn_TexS, "ON", new Vector2(400, 600)));
            pause_buttons.Add(new MenuButton(music_off_Btn_Tex, music_off_Btn_TexS, "OFF", new Vector2(700, 600)));
        }
        private void PrepareVehicles()
        {
            vehicles.Add(new Car(Content.Load<Model>("L200-C4D/L200-FBX"),
    new Vector3(startingPoint, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, "ME", lapsCount));

            for (int i = 1; i < opponentsCount + 1; i++)
            {
                if (i % 2 == 0)
                {
                    vehicles.Add(new Opponent(Content.Load<Model>("L200-C4D-RED/L200-FBX"), new Vector3(startingPoint.X + (i - 1) * trackWidth / opponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, (i - 1) * trackWidth / opponentsCount, "red_" + i.ToString(), lapsCount, 0.85f));
                }
                else
                {
                    vehicles.Add(new Opponent(Content.Load<Model>("L200-C4D-GRE/L200-FBX"), new Vector3(startingPoint.X - i * trackWidth / opponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, -i * trackWidth / opponentsCount, "green_" + i.ToString(), lapsCount, 0.83f));
                }

            }
        }
        private void PrepareTrack()
        {
            List<Vector2> trackPoints = new List<Vector2>() {
                startingPoint,
                new Vector2(-4000,2000),
                new Vector2(-2000,2000),
                new Vector2(-2000,4000),
                new Vector2(0, 4000),
                new Vector2(1000,2000),
                new Vector2(-1000,-2000),
                new Vector2(4000, -2000),
                new Vector2(4000, -4000),
                new Vector2(2000,-4000),
                new Vector2(0,-6000),
                new Vector2(-2000,-6000),
                new Vector2(-4000, -4000),
                startingPoint
            };
            track = new Track(trackPoints, 25, trackWidth, 30, GraphicsDevice, Content);
            Vector2 directionOnTrack;
            startingPoint = track.TracePath(0, out directionOnTrack);
        }
        private void PrepareEnvironment()
        {

            #region ground
            ground = new GameObject(Content.Load<Model>("ground_snow"),
new Vector3(0, -10f, 0), Vector3.Zero, Vector3.One, GraphicsDevice);
            groundyellow = new GameObject(Content.Load<Model>("ground_sand"),
new Vector3(0, -10f, 0), Vector3.Zero, Vector3.One, GraphicsDevice);
            groundred = new GameObject(Content.Load<Model>("ground_red"),
new Vector3(0, -10f, 0), Vector3.Zero, Vector3.One, GraphicsDevice);
            groundgreen = new GameObject(Content.Load<Model>("ground_green"),
new Vector3(0, -10f, 0), Vector3.Zero, Vector3.One, GraphicsDevice);
            #endregion
            //sky = new Sky(Content, GraphicsDevice, Content.Load<TextureCube>("clouds"));

            List<Vector3> treePositions = new List<Vector3>();
            Random r = new Random();

            for (int i = 0; i < 250; i++)
            {
                Vector3 pos = new Vector3((float)r.NextDouble() * 20000 - groundSize, 0, (float)r.NextDouble() * 20000 - groundSize);
                if (!track.IsOnTrackOrRoadside(pos))
                    treePositions.Add(new Vector3(pos.X, 240, pos.Z));
                else
                    Console.WriteLine("is on track " + pos);
            }

            #region tree1
            trees1 = new BillboardCross(GraphicsDevice, Content,
                Content.Load<Texture2D>("whitetree"), new Vector2(500),
                treePositions.ToArray());
            trees1.EnsureOcclusion = true;
            tree1yellow = new BillboardCross(GraphicsDevice, Content,
                Content.Load<Texture2D>("stock_cactus"), new Vector2(500),
                treePositions.ToArray());
            tree1yellow.EnsureOcclusion = true;

            tree1red = new BillboardCross(GraphicsDevice, Content,
                Content.Load<Texture2D>("red_tree"), new Vector2(500),
                treePositions.ToArray());
            tree1red.EnsureOcclusion = true;

            tree1green = new BillboardCross(GraphicsDevice, Content,
                Content.Load<Texture2D>("short-tree"), new Vector2(500),
                treePositions.ToArray());
            tree1green.EnsureOcclusion = true;
            #endregion
            treePositions.Clear();
            for (int i = 0; i < 250; i++)
            {
                Vector3 pos = new Vector3((float)r.NextDouble() * 20000 - groundSize, 0, (float)r.NextDouble() * 20000 - groundSize);
                if (!track.IsOnTrackOrRoadside(pos))
                    treePositions.Add(new Vector3(pos.X, 240, pos.Z));
                //else
                //    Console.WriteLine("is on track " + pos);
            }
            #region tree2
            trees2 = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("snow_tree"), new Vector2(500), treePositions.ToArray());
            trees2.EnsureOcclusion = true;
            tree2yellow = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("cactus_short"), new Vector2(500), treePositions.ToArray());
            tree2yellow.EnsureOcclusion = true;

            tree2red = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("Yellow_Fall_Tree"), new Vector2(500), treePositions.ToArray());
            tree2red.EnsureOcclusion = true;

            tree2green = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("green-tree"), new Vector2(500), treePositions.ToArray());
            tree2green.EnsureOcclusion = true;
            #endregion
            #region checkerd
            Vector3[] position = new Vector3[1];
            position[0] = new Vector3(startingPoint, 0);
            startSign = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("checkerd"), new Vector2(800), position);
            #endregion
            #region cloud
            Vector3[] cloudPositions = new Vector3[350];

            for (int i = 0; i < cloudPositions.Length; i++)
            {
                cloudPositions[i] = new Vector3(
                    r.Next(-6000, 6000),
                    r.Next(1000, 3000),
                    r.Next(-6000, 6000));
            }

            clouds = new BillboardSystem(GraphicsDevice, Content,
                Content.Load<Texture2D>("cloud2"), new Vector2(1000),
                cloudPositions);

            clouds.EnsureOcclusion = false;
            #endregion
        }
        private void LoadCameras()
        {
            Car car = vehicles[0] as Car;
            //TODO poprawic wlasciwosci kamer
            cameras.Add(new ChaseCam(new Vector3(0, 150, 400), new Vector3(0, 100, 0),
                new Vector3(0, MathHelper.Pi, 0), GraphicsDevice, car));
        }


        private void ParseInput(KeyboardState state)
        {

            if (state.IsKeyDown(Keys.Escape))
            {
                isPaused = true;
                IsMouseVisible = true;
            }

        }
        public void UpdateGame(GameTime gameTime, List<Vehicle> activePlayers)
        {
            if (isPaused)
                return;

            foreach (Vehicle model in activePlayers)
                model.Update(gameTime);

            CheckIsOnGround();
        }
        private void CheckIsOnGround()
        {
            Car car = vehicles[0] as Car;
            float bound = groundSize - 1000.0f;
            if (car.Position.X > bound)
                car.Position = new Vector3(bound, car.Position.Y, car.Position.Z);
            else if (car.Position.X < -bound)
                car.Position = new Vector3(-bound, car.Position.Y, car.Position.Z);
            if (car.Position.Z > bound)
                car.Position = new Vector3(car.Position.X, car.Position.Y, bound);
            else if (car.Position.Z < -bound)
                car.Position = new Vector3(car.Position.X, car.Position.Y, -bound);
        }
        public void DrawGameInProgress(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            #region green_map
            if (Greenmap == true)
            {
                groundgreen.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
                tree1green.Draw(currentCamera.View, currentCamera.Projection);
                tree2green.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                clouds.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                startSign.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, Vector3.Right);
                track.Draw(currentCamera.View, currentCamera.Projection);
            }
            #endregion

            #region yellow_map
            else if (Yellowmap == true)
            {
                groundyellow.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
                tree1yellow.Draw(currentCamera.View, currentCamera.Projection);
                tree2yellow.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                clouds.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                startSign.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, Vector3.Right);
                track.Draw(currentCamera.View, currentCamera.Projection);
            }
            #endregion

            #region red_map
            else if (Redmap == true)
            {
                groundred.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
                tree1red.Draw(currentCamera.View, currentCamera.Projection);
                tree2red.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                clouds.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                startSign.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, Vector3.Right);
                track.Draw(currentCamera.View, currentCamera.Projection);
            }
            #endregion
            #region snow_map
            else
            {
                ground.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
                trees1.Draw(currentCamera.View, currentCamera.Projection);
                trees2.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                clouds.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
                startSign.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, Vector3.Right);
                track.Draw(currentCamera.View, currentCamera.Projection);
            }
            #endregion

            foreach (Vehicle model in vehicles)
                if (currentCamera.IsInView(model.BoundingSphere))
                    model.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);

            if (isPaused)
                DrawPaused();
        }
        private bool CheckForGameFinished()
        {
            foreach (Vehicle vehicle in vehicles)
                if (vehicle.FinishedRace())
                    return true;
            return false;
        }
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        protected override void Update(GameTime gameTime)
        {

            currentGameState.Update(gameTime);
            currentKeyboardState = Keyboard.GetState();
            ParseInput(currentKeyboardState);

            if (!isPaused)
            {
                foreach (Camera cam in cameras)
                    cam.Update(gameTime);
            }
            else
            {
                #region update mouse
                previousMouse = currentMouse;
                currentMouse = Mouse.GetState();
                if (MouseMoved)
                {
                    foreach (MenuButton btn in pause_buttons)
                        btn.Selected = btn.Bounds.Contains(currentMouse.X, currentMouse.Y);
                }
                if (MouseLeftClick)
                {
                    MenuButton clicked = null;
                    foreach (MenuButton b in pause_buttons)
                        if (b.Bounds.Contains(currentMouse.X, currentMouse.Y))
                        {
                            clicked = b;
                            break;
                        }
                    if (clicked != null)
                    {
                        switch (clicked.name)
                        {
                            case "PS":
                                isPaused = !isPaused;
                                IsMouseVisible = !IsMouseVisible;
                                break;
                            case "EX":
                                Exit();

                                MediaPlayer.Stop();
                                break;
                            case "ON":
                                MediaPlayer.Play(song);
                                break;
                            case "OFF":

                                MediaPlayer.Stop();
                                break;
                        }
                    }
                }

                #endregion
            }

            base.Update(gameTime);
        }
        public void UpdateGameInProgress(GameTime gameTime)
        {
            UpdateGame(gameTime, vehicles);
            if (CheckForGameFinished())
                GameState = new FinishedGS(this, vehicles);
        }
        private void DrawPaused()
        {
            spriteBatch.Begin();
            foreach (MenuButton btn in pause_buttons)
                if (btn.Selected)
                    spriteBatch.Draw(btn.selectedTexture, btn.position, Color.White);
                else
                    spriteBatch.Draw(btn.texture, btn.position, Color.White);

            spriteBatch.End();
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawGameInProgress(gameTime);
            currentGameState.Draw(gameTime);
            base.Draw(gameTime);

        }

    }
}
