using System;

namespace FrozenBoyUI {
    public static class Program {
        [STAThread]
        static void Main() {
            using var game = new FrozenBoyGame();
            game.Run();
        }
    }
}
