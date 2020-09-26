using System;
using FrozenBoyCore.Processor;
using FrozenBoyCore.Graphics;
using FrozenBoyCore.Util;
using FrozenBoyCore.Memory;
using FrozenBoyCore.Controls;
using System.Text;
using FrozenBoyCore.Serial;
using u8 = System.Byte;
using u16 = System.UInt16;

namespace FrozenBoyCore {

    public class GameBoy {
        public const int ClockSpeed = 4_194_304;
        public GameOptions gbOptions;
        public CPU cpu;
        public GPU gpu;
        public MMU mmu;
        public Dma dma;
        public InterruptManager intManager;
        public Timer timer;
        public SerialLink serial;
        public Joypad joypad;

        // constructor
        public GameBoy(GameOptions gbOptions) {
            this.gbOptions = gbOptions;

            intManager = new InterruptManager();
            timer = new Timer(intManager);
            gpu = new GPU(intManager, gbOptions.Palette);
            joypad = new Joypad(intManager);
            dma = new Dma();
            serial = new SerialLink(intManager);
            mmu = new MMU(timer, intManager, gpu, joypad, dma, serial);
            cpu = new CPU(mmu, timer, intManager, gpu);

            dma.SetMMU(mmu);

            mmu.LoadData(gbOptions.RomPath + gbOptions.RomFilename);
        }

        public int Step() {
            timer.Tick();
            cpu.ExecuteNext();
            dma.Tick();
            serial.Tick();
            gpu.Tick();
            return 1;
        }

    }
}


