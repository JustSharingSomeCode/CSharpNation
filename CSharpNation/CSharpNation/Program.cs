using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

namespace CSharpNation
{
    class Program
    {
        private static int Width = 950, Height;

        static void Main(string[] args)
        {
            Console.WriteLine(Application.StartupPath);
            Console.WriteLine("Starting...");

            float AspectRatio = 9f / 16f;
            Height = (int)(AspectRatio * Width);

            GameWindow Window = new GameWindow(Width, Height,
            new GraphicsMode(new ColorFormat(8, 8, 8, 0),
            0, // Depth bits
            0,  // Stencil bits
            16   // FSAA samples
            ),
            "C#Nation Spectrum");

            Game Gm = new Game(Window);
        }
    }
}
