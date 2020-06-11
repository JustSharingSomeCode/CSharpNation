using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using CSharpNation;

using OpenTK.Input;

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
            //Beat_Detection = AppSettings.Default.Beat_Detection;
            //Beat_Sensibility = AppSettings.Default.Beat_Sensibility;

            Logo_Top_Offset = AppSettings.Default.Logo_Top_Offset;
            Logo_Right_Offset = AppSettings.Default.Logo_Right_Offset;
            Logo_Bottom_Offset = AppSettings.Default.Logo_Bottom_Offset;
            Logo_Left_Offset = AppSettings.Default.Logo_Left_Offset;

            //WriteShortcuts();
        }

        public enum Axis
        {
            X_Left,
            X_Right,
            Y_Top,
            Y_Bottom
        }

        public Axis OffsetAxis { get; set; }

        public int Background_Dim { get; set; }
        public int N_Particles { get; set; }
        public int Background_Change_Seconds { get; set; }
        public bool Auto_Change_Background { get; set; }
        public bool Wave_Enhancements { get; set; }
        //public bool Beat_Detection { get; set; }
        //public double Beat_Sensibility { get; set; }

        public bool EnableShortcuts { get; set; } = true;

        public double Logo_Left_Offset { get; set; }
        public double Logo_Right_Offset { get; set; }
        public double Logo_Top_Offset { get; set; }
        public double Logo_Bottom_Offset { get; set; }

        public int ActualBackground_Dim;
        public int UpdateCount = 0;

        public int GetAlpha()
        {
            return ActualBackground_Dim * 255 / 100;
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
            Console.WriteLine(" L = Select Background By Index");
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
            Console.WriteLine("     If True, Modifies The Wave To Create Round Peeks And Reduce Wave Jumps");
            Console.ResetColor();
            /*
            Console.WriteLine(" 6) Beat Detection = {0}", Beat_Detection);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     If True, Modifies The Wave To Create An Special Effect When A Beat Is Detected");
            Console.ResetColor();

            Console.WriteLine(" 7) Beat Sensibility = {0}", Beat_Sensibility);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     Sensibility To Beats, Only Works If 'Beat Detection' Is True");
            Console.ResetColor();
            */
            Console.WriteLine(" 6) Logo Offset");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("     Move The Logo, Make It Bigger Or Smaller");
            Console.ResetColor();

            Console.WriteLine("PRESS ESCAPE TO GO BACK");

            ConsoleKey[] keysToCheck = new ConsoleKey[7];
            keysToCheck[0] = ConsoleKey.D1;
            keysToCheck[1] = ConsoleKey.D2;
            keysToCheck[2] = ConsoleKey.D3;
            keysToCheck[3] = ConsoleKey.D4;
            keysToCheck[4] = ConsoleKey.D5;            
            keysToCheck[5] = ConsoleKey.D6;
            keysToCheck[6] = ConsoleKey.Escape;

            switch (KeyPressed(keysToCheck))
            {
                case ConsoleKey.D1:
                    Change_Background_Dim();
                    break;

                case ConsoleKey.D2:
                    Change_N_Particles();
                    break;

                case ConsoleKey.D3:
                    Change_Auto_Change_Background();
                    break;

                case ConsoleKey.D4:
                    Change_Background_Duration();
                    break;

                case ConsoleKey.D5:
                    Change_Wave_Enhancements();
                    break;
               
                case ConsoleKey.D6:
                    AdjustLogoOffset();
                    break;

                case ConsoleKey.Escape:
                    WriteShortcuts();
                    break;
            }
        }

        public int SelectBackground(List<string> list, int bgIndex)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-Backgrounds list");
            Console.ResetColor();            

            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine(" {0}) {1}", i + 1, list[i]);
            }

            Console.WriteLine("Press 0 To Cancel");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("WRITE THE NUMBER OF THE BACKGROUND TO SELECT");
            Console.ResetColor();

            try
            {
                int input = Convert.ToInt32(Console.ReadLine()) - 1;

                if (input >= 0 && input < list.Count)
                {
                    return input;                    
                }
                else
                {
                    return bgIndex;
                }                
            }
            catch
            {
                SettingsError("Please Write Only Numbers");
                return SelectBackground(list, bgIndex);
            }
        }

        #region Set_Settings_Value

        private void Change_Background_Dim()
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
                    Change_Background_Dim();
                }
            }
            catch
            {
                SettingsError("Please Write Only Numbers");
                Change_Background_Dim();
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

        private void Change_Background_Duration()
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
                Change_Background_Duration();
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

        private void AdjustLogoOffset()
        {
            EnableShortcuts = false;
            ShowChangeInstruction("-Logo Offset","  Use The Arrow Keys To Select The Side You Want To Move, Then Use 'A' And 'D' Keys To Move Right,Left,Up Or Down.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("¡¡PRESS THE KEYS ON THE VISUALIZER WINDOW!! , PRESS ESCAPE TO GO BACK");
            Console.ResetColor();
        }               

        public void AdjustLogoOnRuntime(double value)
        {
            if (OffsetAxis == Axis.X_Left)
            {
                Logo_Left_Offset += value;
            }
            if (OffsetAxis == Axis.X_Right)
            {
                Logo_Right_Offset += value;
            }
            if (OffsetAxis == Axis.Y_Top)
            {
                Logo_Top_Offset += value;            
            }
            if (OffsetAxis == Axis.Y_Bottom)
            {
                Logo_Bottom_Offset += value;
            }

            WriteOffset();
        }

        public void WriteOffset()
        {
            Console.Clear();

            AdjustLogoOffset();
            Console.WriteLine();

            Console.WriteLine(" Selected Side = {0}", OffsetAxis);
            Console.WriteLine();

            Console.WriteLine(" Logo Left Offset = {0}",Logo_Left_Offset);
            Console.WriteLine(" Logo Right Offset = {0}", Logo_Right_Offset);
            Console.WriteLine(" Logo Top Offset = {0}", Logo_Top_Offset);
            Console.WriteLine(" Logo Bottom Offset = {0}", Logo_Bottom_Offset);
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

        public void SaveSettings()
        {
            AppSettings.Default.Background_Dim = Background_Dim;
            AppSettings.Default.N_Particles = N_Particles;
            AppSettings.Default.Background_Change_Seconds = Background_Change_Seconds;
            AppSettings.Default.Auto_Change_Background = Auto_Change_Background;
            AppSettings.Default.Wave_Enhancements = Wave_Enhancements;            

            AppSettings.Default.Logo_Top_Offset = Logo_Top_Offset;
            AppSettings.Default.Logo_Right_Offset = Logo_Right_Offset;
            AppSettings.Default.Logo_Bottom_Offset = Logo_Bottom_Offset;
            AppSettings.Default.Logo_Left_Offset = Logo_Left_Offset;

            AppSettings.Default.Save();

            WriteSettings();
        }

        #endregion

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
