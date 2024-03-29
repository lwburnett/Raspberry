﻿using System;
using Raspberry_Lib;

namespace Raspberry_Win
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new GameMaster(false, false);
            PlatformUtils.SetVibrateCallback((_, _) => { });
            game.Run();
        }
    }
}
