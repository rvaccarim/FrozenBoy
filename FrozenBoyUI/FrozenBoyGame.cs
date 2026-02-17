using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FrozenBoyCore;
using System.IO;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Util;
using SkiaSharp;

namespace FrozenBoyUI
{
    public class FrozenBoyGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
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

        public FrozenBoyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

        }

        protected override void Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1 && File.Exists(args[1]))
            {
                string fullPath = args[1];

                romFilename = Path.GetFileName(fullPath);
                romPath = Path.GetDirectoryName(fullPath)! + Path.DirectorySeparatorChar;

                GameOptions gbOoptions = new(romFilename, romPath, GetGreenPalette());
                gameboy = new GameBoy(gbOoptions);
                base.Window.Title = "FrozenBoy - " + romFilename;
            }
            else
            {
                Console.WriteLine("Usage: FrozenBoy <romfile.gb>");
                Environment.Exit(0);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            gameboyBuffer = new Texture2D(GraphicsDevice, 160, 144);
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
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

            if (keyboardState.IsKeyDown(Keys.F10) && !scheduledScreenshot)
            {
                scheduledScreenshot = true;
                // F10 was being reported as pressed multiple times so multiple screenshots were being generated
                Thread.Sleep(1500);
            }

            if (backbuffer != null)
                gameboyBuffer.SetData<byte>(backbuffer);

            base.Update(gameTime);
        }

        private static GPU_Palette GetGreenPalette()
        {
            GPU_Color white = new(224, 248, 208, 255);
            GPU_Color lightGray = new(136, 192, 112, 255);
            GPU_Color darkGray = new(52, 104, 86, 255);
            GPU_Color black = new(8, 24, 32, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }

        public static GPU_Palette GetWhitePalette()
        {
            GPU_Color white = new(255, 255, 255, 255);
            GPU_Color lightGray = new(170, 170, 170, 255);
            GPU_Color darkGray = new(85, 85, 85, 255);
            GPU_Color black = new(0, 0, 0, 255);
            return new GPU_Palette(white, lightGray, darkGray, black);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            // compute bounds
            Microsoft.Xna.Framework.Rectangle bounds = GraphicsDevice.Viewport.Bounds;

            float aspectRatio = GraphicsDevice.Viewport.Bounds.Width / (float)GraphicsDevice.Viewport.Bounds.Height;
            float targetAspectRatio = 160.0f / 144.0f;

            if (aspectRatio > targetAspectRatio)
            {
                int targetWidth = (int)(bounds.Height * targetAspectRatio);
                bounds.X = (bounds.Width - targetWidth) / 2;
                bounds.Width = targetWidth;
            }
            else if (aspectRatio < targetAspectRatio)
            {
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

        private void UpdateWorld()
        {
            int cyclesThisUpdate = 0;

            while (cyclesThisUpdate < CYCLES_FOR_60FPS)
            {
                cyclesThisUpdate += gameboy.Step();
            }

            if (scheduledScreenshot)
            {
                TakeScreenshotAndHash();
            }

        }

        private void TakeScreenshotAndHash()
        {
            string screenshotPath = Path.Combine(AppContext.BaseDirectory, @"Screenshots");
            Directory.CreateDirectory(screenshotPath);
            string outputFile = Path.Combine(screenshotPath, romFilename + "_" + screenshotCount.ToString());

            // hash the backbuffer
            string result = Crypto.MD5(backbuffer);
            File.WriteAllText(outputFile + ".hash.txt", result);

            // the screenshot is just for reference, to know what was on the screen when the hash was calculated
            int width = 160;
            int height = 144;
            int bytesPerPixel = 4;
            int stride = width * bytesPerPixel;
            using var bitmap = BuildImageZeroCopy(backbuffer, width, height, stride);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite(outputFile + ".png");
            data.SaveTo(stream);

            screenshotCount++;
            scheduledScreenshot = false;
        }

        private static SKBitmap BuildImageZeroCopy(byte[] sourceData, int width, int height, int stride)
        {
            bool isFlipped = stride < 0;

            // SKColorType.Bgra8888 = Format32bppArgb equivalent
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var bitmap = new SKBitmap();

            GCHandle handle = GCHandle.Alloc(sourceData, GCHandleType.Pinned);
            IntPtr basePtr = handle.AddrOfPinnedObject();

            if (isFlipped)
            {
                stride = Math.Abs(stride);

                // Move pointer to last row
                basePtr = IntPtr.Add(basePtr, stride * (height - 1));

                // Use negative rowBytes so Skia walks upward
                stride = -stride;
            }

            bitmap.InstallPixels(info, basePtr, stride, (_, ctx) => ((GCHandle)ctx!).Free(), handle);

            return bitmap;
        }

    }
}
