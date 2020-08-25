using FrozenBoyCore.Memory;
using FrozenBoyCore.Processor;
using Xunit;
using u8 = System.Byte;
using u16 = System.UInt16;
using FrozenBoyCore.Graphics;

namespace FrozenBoyTest {
    public class TimerTest {

        [Fact]
        public void TestDivIncrease() {
            InterruptManager intManager = new InterruptManager();
            Timer timer = new Timer(intManager);
            GPU gpu = new GPU(intManager);
            MMU mmu = new MMU(timer, intManager, gpu);

            for (int i = 0; i < 255; i++) {
                timer.Tick();
            }
            Assert.Equal(0, timer.DIV);

            timer.Tick();
            Assert.Equal(1, timer.DIV);

        }

        [Fact]
        public void TestClockDisabled() {
            InterruptManager intManager = new InterruptManager();
            Timer timer = new Timer(intManager);
            GPU gpu = new GPU(intManager);
            MMU mmu = new MMU(timer, intManager, gpu);

            for (int i = 0; i < 2056; i++) {
                timer.Tick();
            }
            // TIMA should not count
            Assert.Equal(0, timer.TIMA);
            // DIV doesn't care if the clock is enabled
            Assert.Equal(8, timer.DIV);
        }

        [Fact]
        public void TestTimaIncrease() {
            Assert.Equal(1024, AddTima(0b_0000_00100));
            Assert.Equal(16, AddTima(0b_0000_00101));
            Assert.Equal(64, AddTima(0b_0000_00110));
            Assert.Equal(256, AddTima(0b_0000_00111));
        }

        [Fact]
        public void TestTimaOverflow() {
            InterruptManager intManager = new InterruptManager();
            Timer timer = new Timer(intManager);
            GPU gpu = new GPU(intManager);
            MMU mmu = new MMU(timer, intManager, gpu);

            timer.TAC = 0b_0000_00101;  // frequency = 16

            int max = 16 * 256;
            for (int i = 0; i < max; i++) {
                timer.Tick();
            }

            // the interruption should not happen immediately
            Assert.True(((intManager.IF >> 2) & 1) == 0);

            timer.Tick();
            timer.Tick();
            timer.Tick();

            Assert.Equal(4, timer.ticksSinceOverflow);
            Assert.True(((intManager.IF >> 2) & 1) == 1);

            timer.Tick();
            Assert.Equal(5, timer.ticksSinceOverflow);
            timer.Tick();
            Assert.False(timer.overflow);

        }


        private int AddTima(u8 TAC) {
            InterruptManager intManager = new InterruptManager();
            Timer timer = new Timer(intManager);
            GPU gpu = new GPU(intManager);
            MMU mmu = new MMU(timer, intManager, gpu);

            timer.TAC = TAC;

            while (true) {
                timer.Tick();

                if (timer.TIMA == 1) {
                    break;
                }
            }

            return timer.timerCounter;
        }



    }
}
