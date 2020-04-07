using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using CSharpNation;

namespace CSharpNation
{
    class Config
    {
        public Config()
        {
            Background_Dim = AppSettings.Default.Background_Dim;
            ActualBackground_Dim = Background_Dim;
            N_Particles = AppSettings.Default.N_Particles;
            BackgroundChangeSeconds = AppSettings.Default.Background_Change_Seconds;
            Auto_Change_Background = AppSettings.Default.Auto_Change_Background;

            WriteSettings();
        }

        public int Background_Dim;
        public int N_Particles;
        public int BackgroundChangeSeconds;
        public bool Auto_Change_Background;

        public int ActualBackground_Dim;
        public int UpdateCount = 0;

        public int GetAlpha()
        {
            return ActualBackground_Dim * 255 / 100;
        }

        public void ChangeSettings()
        {
            try
            {
                bool Repeat = true;
                int Option;
                while (Repeat)
                {
                    Console.Clear();
                    Console.WriteLine("Press 1 to change background dim");
                    Console.WriteLine("Press 2 to change number of particles in screen");
                    Console.WriteLine("Press 3 to change the background duration");
                    Console.WriteLine("Press 4 to switch auto background change");
                    Console.WriteLine("Press 5 to save settings");

                    Option = int.Parse(Console.ReadKey(true).KeyChar.ToString());

                    switch (Option)
                    {
                        case 1:
                            Console.WriteLine("Write a number between 0 and 100");
                            Background_Dim = int.Parse(Console.ReadLine().ToString());
                            ActualBackground_Dim = Background_Dim;
                            break;
                        case 2:
                            Console.WriteLine("Write number of particles");
                            N_Particles = int.Parse(Console.ReadLine().ToString());
                            break;
                        case 3:
                            Console.WriteLine("Write time in seconds");
                            BackgroundChangeSeconds = int.Parse(Console.ReadLine().ToString());
                            break;
                        case 4:
                            if (Auto_Change_Background)
                            {
                                Auto_Change_Background = false;
                            }
                            else
                            {
                                Auto_Change_Background = true;
                            }
                            break;
                        case 5:
                            AppSettings.Default.Background_Dim = Background_Dim;
                            AppSettings.Default.N_Particles = N_Particles;
                            AppSettings.Default.Background_Change_Seconds = BackgroundChangeSeconds;
                            AppSettings.Default.Auto_Change_Background = Auto_Change_Background;
                            AppSettings.Default.Save();

                            Repeat = false;
                            break;
                    }
                }
                Console.Clear();
                WriteSettings();
            }
            catch (Exception Ex)
            {
                Console.Clear();
                Console.WriteLine(Ex.Message);
                Console.WriteLine("Press a key to continue");
                Console.ReadKey(true);
                Console.Clear();
                WriteSettings();
            }
        }

        public void WriteSettings()
        {
            Console.WriteLine("Background dim = {0}", AppSettings.Default.Background_Dim);
            Console.WriteLine("Particles in screen = {0}", AppSettings.Default.N_Particles);
            Console.WriteLine("Background duration = {0}", AppSettings.Default.Background_Change_Seconds);
            Console.WriteLine("Auto change background = {0}", AppSettings.Default.Auto_Change_Background);
        }
    }
}
