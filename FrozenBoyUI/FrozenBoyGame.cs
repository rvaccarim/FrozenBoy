using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FrozenBoyCore;
using System.IO;
using System.Text;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Util;

namespace FrozenBoyUI {

    public class FrozenBoyGame : Game {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GameBoy gameboy;
        private Texture2D gameboyBuffer;
        private bool scheduledScreenshot;
        private byte[] backbuffer;
        private string romFilename;
        private string romPath;
        private int screenshotCount = 1;

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
                romPath = Path.GetDirectoryName(ofd.FileName) + @"\";

                GameOptions gbOptions = new GameOptions(romFilename, romPath, GetGreenPalette());
                gameboy = new GameBoy(gbOptions);

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
            gameboy.joypad.JoypadKeys[4] = keyboardState.IsKeyDown(Keys.S);
            gameboy.joypad.JoypadKeys[5] = keyboardState.IsKeyDown(Keys.A);
            gameboy.joypad.JoypadKeys[6] = keyboardState.IsKeyDown(Keys.Space);
            gameboy.joypad.JoypadKeys[7] = keyboardState.IsKeyDown(Keys.Enter);

            // upload backbuffer to texture
            backbuffer = gameboy.gpu.GetScreenBuffer();

            if (keyboardState.IsKeyDown(Keys.F10) && !scheduledScreenshot) {
                scheduledScreenshot = true;
                // F10 was being reported as pressed multiple times so multiple screenshots were being generated
                Thread.Sleep(1500);
            }

            if (backbuffer != null)
                gameboyBuffer.SetData<byte>(backbuffer);

            base.Update(gameTime);
        }

        private GPU_Palette GetGreenPalette() {
            GPU_Color white = new GPU_Color(224, 248, 208, 255);
            GPU_Color lightGray = new GPU_Color(136, 192, 112, 255);
            GPU_Color darkGray = new GPU_Color(52, 104, 86, 255);
            GPU_Color black = new GPU_Color(8, 24, 32, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }

        public GPU_Palette GetWhitePalette() {
            var white = new GPU_Color(255, 255, 255, 255);
            var lightGray = new GPU_Color(170, 170, 170, 255);
            var darkGray = new GPU_Color(85, 85, 85, 255);
            var black = new GPU_Color(0, 0, 0, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            // compute bounds
            Microsoft.Xna.Framework.Rectangle bounds = GraphicsDevice.Viewport.Bounds;

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

            spriteBatch.Draw(gameboyBuffer, bounds, Microsoft.Xna.Framework.Color.White);
            spriteBatch.End();

            base.Draw(gameTime);

        }

        private void UpdateWorld() {
            int cyclesThisUpdate = 0;

            while (cyclesThisUpdate < CYCLES_FOR_60FPS) {
                cyclesThisUpdate += gameboy.Step();
            }

            if (scheduledScreenshot) {
                TakeScreenshotAndHash();
            }

        }

        private void TakeScreenshotAndHash() {
            string outputFile = @"D:\Users\frozen\Documents\03_programming\online\emulation\FrozenBoy\FrozenBoyTest\hashes\" + romFilename + "_" + screenshotCount.ToString();

            int width = 160;
            int height = 144;
            int bytesPerPixel = 4;
            Bitmap bmp = BuildImage(backbuffer, width, height, width * bytesPerPixel, PixelFormat.Format32bppArgb);
            bmp.Save(outputFile + ".png", ImageFormat.Png);

            string result = Crypto.MD5(backbuffer);
            File.WriteAllText(outputFile + ".hash.txt", result);

            screenshotCount++;
            scheduledScreenshot = false;

        }

        private Bitmap BuildImage(Byte[] sourceData, Int32 width, Int32 height, Int32 stride, PixelFormat pixelFormat) {
            Bitmap newImage = new Bitmap(width, height, pixelFormat);
            BitmapData targetData = newImage.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
            Int32 newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;
            // Compensate for possible negative stride on BMP format.
            Boolean isFlipped = stride < 0;
            stride = Math.Abs(stride);
            // Cache these to avoid unnecessary getter calls.
            Int32 targetStride = targetData.Stride;
            Int64 scan0 = targetData.Scan0.ToInt64();
            for (Int32 y = 0; y < height; y++)
                Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
            newImage.UnlockBits(targetData);

            // Fix negative stride on BMP format.
            if (isFlipped)
                newImage.RotateFlip(RotateFlipType.Rotate180FlipX);

            return newImage;
        }
    }
}
