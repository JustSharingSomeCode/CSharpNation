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
            Background_Change_Seconds = AppSettings.Default.Background_Change_Seconds;
            Auto_Change_Background = AppSettings.Default.Auto_Change_Background;
            Wave_Enhancements = AppSettings.Default.Wave_Enhancements;

            WriteShortcuts();
        }

        public int Background_Dim { get; set; }
        public int N_Particles { get; set; }
        public int Background_Change_Seconds { get; set; }
        public bool Auto_Change_Background { get; set; }
        public bool Wave_Enhancements { get; set; }

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
                    Console.WriteLine("Press 5 to switch wave enhancements");
                    Console.WriteLine("Press 6 to save settings");

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
                            Background_Change_Seconds = int.Parse(Console.ReadLine().ToString());
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
                            AppSettings.Default.Background_Change_Seconds = Background_Change_Seconds;
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

        public void WriteShortcuts()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("¡¡¡TO USE SHORTCUTS PRESS THE KEYS ON THE VISUALIZER WINDOW!!!");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-Shortcuts");
            Console.ResetColor();
            Console.WriteLine(" O = Settings");
            Console.WriteLine(" F = Fullscreen");
            Console.WriteLine(" N = Next background");
            Console.WriteLine(" B = Previous background");
        }

        public void WriteSettings()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-Settings");
            Console.ResetColor();

            Console.WriteLine(" 1) Background Dim = {0}%", Background_Dim);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     Makes The Background Brighter Or Darker");
            Console.ResetColor();

            Console.WriteLine(" 2) Particles On Screen = {0}", N_Particles);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     Number Of Particles Shown In Screen");
            Console.ResetColor();

            Console.WriteLine(" 3) Auto Change Background = {0}", Auto_Change_Background);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     If True, Backgrounds Are Going To Change After 'Background Duration' Has Elapsed");
            Console.ResetColor();

            Console.WriteLine(" 4) Background Duration = {0}", Background_Change_Seconds);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     Time In Seconds Before Show The Next Background");
            Console.WriteLine("     Only Works If 'Auto Change Background' Is True");
            Console.ResetColor();

            Console.WriteLine(" 5) Wave Enhancements = {0}", Wave_Enhancements);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     If True, Modify The Wave To Create Round Peeks And Reduce Wave Jumps");
            Console.ResetColor();

            Console.WriteLine("PRESS ESCAPE TO GO BACK");

            ConsoleKey[] keysToCheck = new ConsoleKey[6];
            keysToCheck[0] = ConsoleKey.D1;
            keysToCheck[1] = ConsoleKey.D2;
            keysToCheck[2] = ConsoleKey.D3;
            keysToCheck[3] = ConsoleKey.D4;
            keysToCheck[4] = ConsoleKey.D5;
            keysToCheck[5] = ConsoleKey.Escape;

            switch (KeyPressed(keysToCheck))
            {
                case ConsoleKey.D1:
                    ChangeDim();
                    break;

                case ConsoleKey.D2:
                    Change_N_Particles();
                    break;

                case ConsoleKey.D3:
                    Change_Auto_Change_Background();
                    break;

                case ConsoleKey.D4:
                    ChangeBackgroundDuration();
                    break;

                case ConsoleKey.D5:
                    Change_Wave_Enhancements();
                    break;

                case ConsoleKey.Escape:
                    WriteShortcuts();
                    break;
            }
        }

        private void ChangeDim()
        {            
            ShowChangeInstruction("-Background Dim", " Write A Number Between 0 And 100");

            try
            {
                int input = Convert.ToInt32(Console.ReadLine());

                if (input >= 0 && input <= 100)
                {
                    Background_Dim = input;
                    ActualBackground_Dim = Background_Dim;
                    SaveSettings();
                }
                else
                {
                    SettingsError("Please Write A Number Between 0 And 100");
                    ChangeDim();
                }
            }
            catch
            {
                SettingsError("Please Write Only Numbers");
                ChangeDim();
            }
        }

        private void Change_N_Particles()
        {            
            ShowChangeInstruction("-Particles On Screen", " Write The Number Of Particles You Want On Screen");

            try
            {
                int input = Convert.ToInt32(Console.ReadLine());

                N_Particles = input;
                SaveSettings();
            }
            catch
            {
                SettingsError("Please Write Only Numbers");
                Change_N_Particles();
            }
        }

        private void Change_Auto_Change_Background()
        {
            ShowChangeInstruction("-Auto Change Background", " 1) True    2) False");

            ConsoleKey[] keysToCheck = new ConsoleKey[2];
            keysToCheck[0] = ConsoleKey.D1;
            keysToCheck[1] = ConsoleKey.D2;

            if (KeyPressed(keysToCheck) == ConsoleKey.D1)
            {
                Auto_Change_Background = true;
            }
            else
            {
                Auto_Change_Background = false;
            }

            SaveSettings();
        }

        private void ChangeBackgroundDuration()
        {
            ShowChangeInstruction("-Background Duration", " Write The Time In Seconds");

            try
            {
                int input = Convert.ToInt32(Console.ReadLine());

                Background_Change_Seconds = input;
                SaveSettings();
            }
            catch
            {
                SettingsError("Please Write Only Numbers");
                ChangeBackgroundDuration();
            }
        }

        private void Change_Wave_Enhancements()
        {
            ShowChangeInstruction("-Wave Enhancements", " 1) True    2) False");

            ConsoleKey[] keysToCheck = new ConsoleKey[2];
            keysToCheck[0] = ConsoleKey.D1;
            keysToCheck[1] = ConsoleKey.D2;

            if (KeyPressed(keysToCheck) == ConsoleKey.D1)
            {
                Wave_Enhancements = true;
            }
            else
            {
                Wave_Enhancements = false;
            }

            SaveSettings();
        }

        private void ShowChangeInstruction(string Option, string Instruction)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Option);
            Console.ResetColor();
            Console.WriteLine(Instruction);
        }

        private void SettingsError(string errorMsg, bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMsg);
            Console.ResetColor();

            if (clearConsole)
            {
                Console.WriteLine("Press A Key");
                Console.ReadKey(true);
            }
        }

        private void SaveSettings()
        {
            AppSettings.Default.Background_Dim = Background_Dim;
            AppSettings.Default.N_Particles = N_Particles;
            AppSettings.Default.Background_Change_Seconds = Background_Change_Seconds;
            AppSettings.Default.Auto_Change_Background = Auto_Change_Background;
            AppSettings.Default.Wave_Enhancements = Wave_Enhancements;
            AppSettings.Default.Save();

            WriteSettings();
        }

        #region KeyInput

        private ConsoleKey KeyPressed()
        {
            ConsoleKeyInfo info = new ConsoleKeyInfo();
            info = Console.ReadKey();

            while (info.Key != ConsoleKey.Escape || info.Key != ConsoleKey.Enter)
            {
                info = Console.ReadKey();
            }

            return info.Key;
        }

        private ConsoleKey KeyPressed(ConsoleKey[] keysToCheck)
        {
            ConsoleKeyInfo info = new ConsoleKeyInfo();
            info = Console.ReadKey(true);

            for (int i = 0; i < keysToCheck.Length; i++)
            {
                if (info.Key == keysToCheck[i])
                {
                    return info.Key;
                }
            }

            SettingsError("Press A Valid Key", false);

            return KeyPressed(keysToCheck);
        }

        #endregion
    }
}
