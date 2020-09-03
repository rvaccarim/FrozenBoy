using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FrozenBoyCore;
using System.IO;
using System.Text;
using System;

namespace FrozenBoyUI {

    public class FrozenBoyGame : Game {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GameBoy gameboy;
        private Texture2D gameboyBuffer;
        private bool scheduledScreenshot;
        private byte[] backbuffer;
        private string romFilename;

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
            // loading a rom and starting emulation
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog {
                DefaultExt = ".gb",
                Filter = "ROM files (.gb)|*.gb",
                Multiselect = false
            };

            System.Windows.Forms.DialogResult result = ofd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                romFilename = Path.GetFileName(ofd.FileName);

                gameboy = new GameBoy(ofd.FileName);

                base.Window.Title = "FrozenBoy - " + romFilename;

            }
            else {
                Environment.Exit(0);
            }

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
            backbuffer = gameboy.gpu.GetScreenBuffer();

            if (keyboardState.IsKeyDown(Keys.F10)) {
                scheduledScreenshot = true;
            }

            if (scheduledScreenshot) {
                TakeScreenshot();
                scheduledScreenshot = false;
            }

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

        private void TakeScreenshot() {
            byte[] hash;
            using var md5 = System.Security.Cryptography.MD5.Create();
            md5.TransformFinalBlock(backbuffer, 0, backbuffer.Length);
            hash = md5.Hash;

            StringBuilder result = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
                result.Append(hash[i].ToString("X2"));

            File.WriteAllText(@"D:\Users\frozen\Documents\99_temp\GB_Debug\" + romFilename + ".hash.txt", result.ToString());

        }
    }
}
