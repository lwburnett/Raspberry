using System;
using Raspberry_Lib;

namespace Raspberry_Win
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new GameMaster();
            game.Run();
        }
    }
}
