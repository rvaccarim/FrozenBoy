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
        private Thread gameboyThread;
        private Texture2D gameboyBuffer;
        private bool cancelled = false;

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

            //// loading a rom and starting emulation
            //System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog {
            //    DefaultExt = ".gb",
            //    Filter = "ROM files (.gb)|*.gb",
            //    Multiselect = false
            //};

            //System.Windows.Forms.DialogResult result = ofd.ShowDialog();
            //if (result == System.Windows.Forms.DialogResult.OK) {
            //    string filename = ofd.FileName;

            //using (FileStream fs = new FileStream(filename, FileMode.Open)) {
            //    using BinaryReader br = new BinaryReader(fs);
            //    byte[] rom = new byte[fs.Length];
            //    for (int i = 0; i < fs.Length; i++)
            //        rom[i] = br.ReadByte();
            //    emulator.Load(rom);
            //}

            gameboyThread = new Thread(GameBoyWork);
            gameboyThread.Start();
            // }
        }

        protected override void UnloadContent() {
            if (gameboyThread != null && gameboyThread.IsAlive)
                cancelled = true;
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
            spriteBatch.Draw(gameboyBuffer, bounds, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void GameBoyWork() {
            double cpuSecondsElapsed = 0.0f;

            MicroStopwatch s = new MicroStopwatch();
            s.Start();

            while (!cancelled) {
                uint cycles = (uint)gameboy.Step();

                // timer handling
                // note: there's nothing quite reliable / precise enough in cross-platform .Net
                // so this is quite hack-ish / dirty
                cpuSecondsElapsed += cycles / GameBoy.ClockSpeed;

                double realSecondsElapsed = s.ElapsedMicroseconds * 1_000_000;

                if (realSecondsElapsed - cpuSecondsElapsed > 0.0) // dirty wait
                {
                    realSecondsElapsed = s.ElapsedMicroseconds * 1_000_000;
                }

                if (s.ElapsedMicroseconds > 1_000_000) // dirty restart every seconds to not loose too many precision
                {
                    s.Restart();
                    cpuSecondsElapsed -= 1.0;
                }
            }
        }
    }
}
