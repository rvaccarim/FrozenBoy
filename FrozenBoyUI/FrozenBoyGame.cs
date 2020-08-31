using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using FrozenBoyCore;

namespace FrozenBoyUI {

    public class FrozenBoyGame : Game {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GameBoy gameboy;
        private Texture2D gameboyBuffer;

        // the amount of clock cycles the gameboy can exectue every second is 4194304
        // 4194304 / 60
        private const int CYCLES_FOR_60FPS = 69905;

        public FrozenBoyGame() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

        }

        protected override void Initialize() {
            // string romFilename = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\games\alleyway.gb";
            string romFilename = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\games\drmario.gb";
            // string romFilename = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\ROMS\games\bubbleGhost.gb";
            gameboy = new GameBoy(romFilename);

            base.Initialize();
        }

        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            gameboyBuffer = new Texture2D(GraphicsDevice, 160, 144);

        }

        protected override void UnloadContent() {

        }

        protected override void Update(GameTime gameTime) {
            KeyboardState keyboardState = Keyboard.GetState();

            // inputs
            gameboy.joypad.JoypadKeys[0] = keyboardState.IsKeyDown(Keys.Right);
            gameboy.joypad.JoypadKeys[1] = keyboardState.IsKeyDown(Keys.Left);
            gameboy.joypad.JoypadKeys[2] = keyboardState.IsKeyDown(Keys.Up);
            gameboy.joypad.JoypadKeys[3] = keyboardState.IsKeyDown(Keys.Down);
            gameboy.joypad.JoypadKeys[4] = keyboardState.IsKeyDown(Keys.Z);
            gameboy.joypad.JoypadKeys[5] = keyboardState.IsKeyDown(Keys.X);
            gameboy.joypad.JoypadKeys[6] = keyboardState.IsKeyDown(Keys.Space);
            gameboy.joypad.JoypadKeys[7] = keyboardState.IsKeyDown(Keys.Enter);

            // upload backbuffer to texture
            byte[] backbuffer = gameboy.gpu.GetScreenBuffer();
            if (backbuffer != null)
                gameboyBuffer.SetData<byte>(backbuffer);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // compute bounds
            Rectangle bounds = GraphicsDevice.Viewport.Bounds;

            float aspectRatio = GraphicsDevice.Viewport.Bounds.Width / (float)GraphicsDevice.Viewport.Bounds.Height;
            float targetAspectRatio = 160.0f / 144.0f;

            if (aspectRatio > targetAspectRatio) {
                int targetWidth = (int)(bounds.Height * targetAspectRatio);
                bounds.X = (bounds.Width - targetWidth) / 2;
                bounds.Width = targetWidth;
            }
            else if (aspectRatio < targetAspectRatio) {
                int targetHeight = (int)(bounds.Width / targetAspectRatio);
                bounds.Y = (bounds.Height - targetHeight) / 2;
                bounds.Height = targetHeight;
            }

            // draw backbuffer
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw is called called 60 times per second
            UpdateWorld();

            spriteBatch.Draw(gameboyBuffer, bounds, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void UpdateWorld() {
            int cyclesThisUpdate = 0;

            while (cyclesThisUpdate < CYCLES_FOR_60FPS) {
                cyclesThisUpdate += gameboy.Step();
            }

        }
    }
}
