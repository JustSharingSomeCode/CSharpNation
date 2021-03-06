﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace CSharpNation
{
    class Game
    {
        private GameWindow window;
        private Analizer _analizer;
        private Particles particles;
        private Replay replay;
        private Texture _texture = new Texture();
        private Config _config = new Config();        

        private List<Vector2> controlPointsList = new List<Vector2>(), catmullRomList = new List<Vector2>();
        private List<double> spectrumData = new List<double>();
        private List<double> tempSpectrumData;
        private List<Particles.Particle> particlesList = new List<Particles.Particle>();
        
        private float Radius;
        private int BackgroundTexture;
        private int LogoTexture;
        private int ParticleTexture;

        private bool restoreBackground_Dim = false;
        private bool startAnimation = false;
        private bool reverseAnimation = false;
        private bool IsFullBackground = false;
        private bool scaleBackground = false;

        private readonly float aspectRatio = 9f / 16f;
        private float rate = 10f;
        private float waveMultiplier = 0.5f;
        private float scaleFactor = 0;

        private float increaseY, increaseX, scale;

        private KeyboardState actualKeyboardState, oldKeyboardState;

        public Game(GameWindow _window)
        {
            window = _window;
            _analizer = new Analizer();
            _analizer.Enable = true;
            replay = new Replay();

            particles = new Particles(window.Width, window.Height, _config);
            Console.WriteLine("Loading resources...");

            if (!File.Exists("Resources\\BackgroundConfig.txt"))
            {
                using (StreamWriter sw = File.CreateText("Resources\\BackgroundConfig.txt"))
                {
                    sw.Close();
                }
            }

            if (Directory.Exists("Resources\\Backgrounds"))
            {
                string[] BackgroundsJpg = Directory.GetFiles("Resources\\Backgrounds", "*.jpg");
                string[] BackgroundsPng = Directory.GetFiles("Resources\\Backgrounds", "*.png");

                string[] Backgrounds = new string[BackgroundsJpg.Length + BackgroundsPng.Length];

                for (int i = 0; i < Backgrounds.Length; i++)
                {
                    if (i < BackgroundsJpg.Length)
                    {
                        Backgrounds[i] = BackgroundsJpg[i];
                    }
                    else
                    {
                        Backgrounds[i] = BackgroundsPng[i - BackgroundsJpg.Length];
                    }
                }

                for (int i = 0; i < Backgrounds.Length; i++)
                {
                    Backgrounds[i] = Path.GetFileName(Backgrounds[i]);
                }

                _texture.LoadBackgrounds(Backgrounds);                
            }

            if (!LoadTexture(ref LogoTexture, "Logo.jpg", Texture.ImageMode.fullSplit))
            {
                LoadTexture(ref LogoTexture, "Logo.png", Texture.ImageMode.fullSplit);
            }

            if (!LoadTexture(ref ParticleTexture, "particle.jpg", Texture.ImageMode.fullSplit))
            {
                LoadTexture(ref ParticleTexture, "particle.png", Texture.ImageMode.fullSplit);
            }                        

            if (_texture.HasTexturesLoaded())
            {
                _config.WriteShortcuts();

                _texture.WriteErrors();

                UpdateBackgroundScale();
                BackgroundTexture = _texture.GetBackgroundByIndex(0);

                Start();
            }
            else
            {
                _texture.WriteErrors();

                Console.WriteLine();
                Console.WriteLine("No background has been loaded... Press a key to close");
                Console.ReadKey(true);
            }
        }

        private void Start()
        {
            window.Load += OnDisplay;
            window.Resize += OnResize;
            window.RenderFrame += OnRender;
            window.UpdateFrame += OnUpdate;
            window.Closing += OnClosing;
                       
            window.VSync = VSyncMode.On;            
            window.Run();
        }

        private void OnClosing(object sender, EventArgs e)
        {
            _texture.SaveBackgroundConfig();
        }

        private void OnDisplay(object sender, EventArgs e)
        {
            GL.ClearColor(0, 0, 0, 255);
        }

        private void OnResize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, window.Width, 0.0, window.Height, -1.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);

            window.Height = (int)(window.Width * (9f / 16f));
            
            particles.UpdateWindowSize(window.Width, window.Height);
            _analizer.multiplier = window.Height / 2;
            rate = window.Width * 10 / 950;
        }

        private void OnUpdate(object sender, EventArgs e)
        {                    
            KeyPressedActions();
            
            if (_config.Auto_Change_Background)
            {
                if (_config.UpdateCount > _config.Background_Change_Seconds * 60)
                {
                    startAnimation = true;
                    _config.UpdateCount = 0;
                }
                else
                {
                    if (!startAnimation)
                    {
                        _config.UpdateCount++;
                    }
                }
            }

            if (startAnimation)
            {
                _config.UpdateCount = 0;
                BackgroundChangeAnimation();
            }           
            
            tempSpectrumData = _analizer.GetSpectrum();            

            if (!_config.EnableShortcuts)
            {
                for (int i = 0; i < tempSpectrumData.Count; i++)
                {
                    tempSpectrumData[i] = 0;
                }
            }            
            
            WaveEnhancements(_config.Wave_Enhancements);

            CalculateRadius(tempSpectrumData);

            CreateControlPoints();

            float aditionalVelocity = (float)((Radius - (window.Height / 4)) * 0.3);           

            particles.UpdateParticles(ParticleTexture, aditionalVelocity);

            particlesList = particles.GetParticlesList();

            replay.Push(catmullRomList);

            if(_config.Enable_Background_Movement)
            {
                increaseX = (Radius - (window.Height / 4f)) / 1.5f;
            }
            else
            {
                increaseX = 0;
            }
            
            increaseY = aspectRatio * increaseX;
            scale = 0;

            if (scaleBackground)
            {
                if (IsFullBackground)
                {
                    scale = Math.Abs((((window.Width + increaseY) * scaleFactor) - window.Height) / 2);
                }
                else
                {
                    scale = Math.Abs(((((window.Width / 2) + increaseY) * scaleFactor) - window.Height) / 2);
                }
            }

            //Console.WriteLine("{0} , {1}", window.RenderFrequency, window.RenderTime);
        }

        private void OnRender(object sender, EventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);           

            if (IsFullBackground)
            {
                DrawTexture(BackgroundTexture, 0 - increaseX, 0 - increaseY - scale, window.Width + increaseX, window.Height + increaseY + scale, _config.GetAlpha(), 255, 255, 255);
            }
            else
            {
                DrawTexture(BackgroundTexture, 0 - increaseX, 0 - increaseY - scale, window.Width / 2, window.Height + increaseY + scale, _config.GetAlpha(), 255, 255, 255);//(Radius - (window.Height / 4))
                DrawTexture(BackgroundTexture, window.Width + increaseX, 0 - increaseY - scale, window.Width / 2, window.Height + increaseY + scale, _config.GetAlpha(), 255, 255, 255);
            }

            float x, y, xMax, yMax;
            for (int i = 0; i < particlesList.Count; i++)
            {                
                x = particlesList[i].position.X - (particlesList[i].actualRadius / 2);
                y = particlesList[i].position.Y - (particlesList[i].actualRadius / 2);
                xMax = particlesList[i].position.X + (particlesList[i].actualRadius / 2);
                yMax = particlesList[i].position.Y + (particlesList[i].actualRadius / 2);

                DrawTexture(particlesList[i].texture, x, y, xMax, yMax, particlesList[i].opacity, 255, 255, 255);
            }
            
            DrawGlow(replay.GetCatmullRomPoints(3), Color.FromArgb(0, 255, 0));
            DrawGlow(replay.GetCatmullRomPoints(5), Color.FromArgb(50, 205, 255));
            DrawGlow(replay.GetCatmullRomPoints(7), Color.Blue);
            DrawGlow(replay.GetCatmullRomPoints(8), Color.FromArgb(50, 50, 155));
            DrawGlow(replay.GetCatmullRomPoints(9), Color.FromArgb(255, 100, 255));
            DrawGlow(replay.GetCatmullRomPoints(10), Color.Red);
            //DrawGlow(replay.GetCatmullRomPoints(12), Color.FromArgb(255, 150, 0));
            DrawGlow(replay.GetCatmullRomPoints(13), Color.Yellow);            

            DrawWave(replay.GetCatmullRomPoints(3), Color.FromArgb(0, 255, 0));
            DrawWave(replay.GetCatmullRomPoints(5), Color.FromArgb(50, 205, 255));
            DrawWave(replay.GetCatmullRomPoints(7), Color.Blue);
            DrawWave(replay.GetCatmullRomPoints(8), Color.FromArgb(50, 50, 155));
            DrawWave(replay.GetCatmullRomPoints(9), Color.FromArgb(255, 100, 255));
            DrawWave(replay.GetCatmullRomPoints(10), Color.Red);
            DrawWave(replay.GetCatmullRomPoints(12), Color.FromArgb(255, 150, 0));
            DrawWave(replay.GetCatmullRomPoints(13), Color.Yellow);
            DrawWave(catmullRomList, Color.White);

            /*
            for (int i = 0; i < controlPointsList.Count; i++)
            {
                DrawCircle(controlPointsList[i].X, controlPointsList[i].Y, 5, Color.Green);
            }   
            */

            DrawPrincipalCircle();            
            DrawTexture(LogoTexture, (window.Width * (float)_config.Logo_Left_Offset) - Radius, (window.Height * (float)_config.Logo_Bottom_Offset) - Radius, (window.Width * (float)_config.Logo_Right_Offset) + Radius, (window.Height * (float)_config.Logo_Top_Offset) + Radius, 255, 255, 255, 255);

            window.SwapBuffers();
        }                                  
    
        #region Keys

        private bool KeyPressed(KeyboardState actualState, KeyboardState oldState, Key k)
        {
            return actualState.IsKeyDown(k) && !oldState.IsKeyDown(k) && window.Focused;
        }

        private void KeyPressedActions()
        {
            oldKeyboardState = actualKeyboardState;
            actualKeyboardState = Keyboard.GetState();

            if(actualKeyboardState == oldKeyboardState)
            {
                return;
            }

            if (_config.EnableShortcuts)
            {
                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.F))
                {
                    replay.catmullRomPoints.Clear();
                    if (window.WindowState == WindowState.Fullscreen)
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        window.WindowState = WindowState.Fullscreen;
                    }
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.B))
                {
                    reverseAnimation = true;
                    startAnimation = true;                    
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.N))
                {
                    startAnimation = true;                    
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.O))
                {
                    _config.WriteSettings();
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.L))
                {
                    BackgroundTexture = _texture.GetBackgroundByIndex(_config.SelectBackground(_texture.GetBackgroundsList(), _texture.BackgroundIndex));
                    _config.WriteShortcuts();

                    UpdateBackgroundScale();
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.C))
                {
                    bool activateAcb = false;

                    if (_config.Auto_Change_Background)
                    {
                        _config.Auto_Change_Background = false;
                        activateAcb = true;
                    }
                    
                    BackgroundTexture = _texture.ChangeBackgroundImageMode(_config.SelectImageMode());
                    _config.WriteShortcuts();
                    IsFullBackground = _texture.IsFullBackgroundIndex();    
                    
                    if(activateAcb)
                    {
                        _config.Auto_Change_Background = true;
                    }
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.S))
                {
                    BackgroundTexture = _texture.UpdateScaleMode(_texture.BackgroundIndex, !scaleBackground);
                    scaleBackground = !scaleBackground;
                }                
            }
            else
            {
                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.Escape))
                {
                    _config.SaveSettings();
                    _config.EnableShortcuts = true;
                    _config.WriteSettings();
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.Up))
                {
                    _config.OffsetAxis = Config.Axis.Y_Top;
                    _config.WriteOffset();
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.Down))
                {
                    _config.OffsetAxis = Config.Axis.Y_Bottom;
                    _config.WriteOffset();
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.Left))
                {
                    _config.OffsetAxis = Config.Axis.X_Left;
                    _config.WriteOffset();
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.Right))
                {
                    _config.OffsetAxis = Config.Axis.X_Right;
                    _config.WriteOffset();
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.D))
                {
                    _config.AdjustLogoOnRuntime(0.005);
                }

                if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.A))
                {
                    _config.AdjustLogoOnRuntime(-0.005);
                }
            }
        }

        #endregion

        #region CatmullRom

        private void CreateControlPoints()
        {
            controlPointsList.Clear();

            double X = window.Width / 2;
            double Y = window.Height / 2;
            double Size = 0, PosX, PosY, Rads;
            int Rep = 0;

            for (double i = 90; i < 270; i += 180 / _analizer._lines)
            {
                Size = spectrumData[Rep] * waveMultiplier + Radius;
                Rads = Math.PI * i / 180;

                PosX = X + (Math.Cos(Rads) * Size);
                PosY = Y + (Math.Sin(Rads) * Size);

                controlPointsList.Add(new Vector2((float)PosX, (float)PosY));

                Rep++;
                if (Rep >= spectrumData.Count)
                {
                    break;
                }
            }

            for (int i = controlPointsList.Count - 1; i >= 0; i--)
            {
                controlPointsList.Add(new Vector2((float)(controlPointsList[i].X + ((controlPointsList[i].X - (window.Width / 2)) * -2)), controlPointsList[i].Y));
            }

            GetLine();
        }

        private void GetLine()
        {
            catmullRomList.Clear();

            catmullRomList.Add(controlPointsList[0]);
            for (int i = 0; i < controlPointsList.Count; i++)
            {
                if (i == 0)
                {
                    for (float a = 0; a < 1; a += 0.05f)
                    {
                        catmullRomList.Add(CatmullRom(a, controlPointsList[i], controlPointsList[i], controlPointsList[i + 1], controlPointsList[i + 2]));
                    }

                    continue;
                }

                if (i == controlPointsList.Count - 2)
                {
                    continue;
                }

                if (i == controlPointsList.Count - 1)
                {
                    for (float a = 0; a < 1; a += 0.05f)
                    {
                        catmullRomList.Add(CatmullRom(a, controlPointsList[i - 2], controlPointsList[i - 1], controlPointsList[i], controlPointsList[i]));
                    }

                    continue;
                }

                for (float a = 0; a < 1; a += 0.05f)
                {
                    catmullRomList.Add(CatmullRom(a, controlPointsList[i - 1], controlPointsList[i], controlPointsList[i + 1], controlPointsList[i + 2]));
                }
            }
            catmullRomList.Add(controlPointsList[controlPointsList.Count - 1]);
        }

        private Vector2 CatmullRom(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 a = 2f * p1;
            Vector2 b = p2 - p0;
            Vector2 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector2 d = -p0 + 3f * p1 - 3f * p2 + p3;

            Vector2 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }               

        private void WaveEnhancements(bool enabled)
        {
            if (spectrumData.Count != tempSpectrumData.Count || !enabled)
            {
                spectrumData.Clear();
                for (int i = 0; i < tempSpectrumData.Count; i++)
                {
                    spectrumData.Add(tempSpectrumData[i]);
                }
            }
            else
            {
                GenerateRoundPeeks();

                double dif;
                
                for(int i = 0; i < spectrumData.Count; i++)
                {
                    dif = Math.Abs(spectrumData[i] - tempSpectrumData[i]);

                    if (dif > rate)
                    {
                        if (spectrumData[i] > tempSpectrumData[i])
                        {                            
                            spectrumData[i] -= dif / 3;
                        }
                        else
                        {                            
                            spectrumData[i] += dif / 3;
                        }
                    }
                    else
                    {
                        spectrumData[i] = tempSpectrumData[i];
                    }
                }
                
                for (int i = 1; i < spectrumData.Count - 1; i++)
                {
                    if (spectrumData[i] > (spectrumData[i - 1] + rate * 2) && spectrumData[i] > (spectrumData[i + 1] + rate * 2))
                    {
                        if (spectrumData[i - 1] > spectrumData[i + 1])
                        {
                            spectrumData[i - 1] += rate;
                        }
                        else
                        {
                            spectrumData[i + 1] += rate;
                        }
                    }                                        
                }
            }
        }

        private void GenerateRoundPeeks()
        {            
            for (int i = 1; i < tempSpectrumData.Count - 2; i++)
            {
                if (tempSpectrumData[i] > tempSpectrumData[i + 1] && tempSpectrumData[i] > tempSpectrumData[i - 1])
                {
                    if (tempSpectrumData[i - 1] > tempSpectrumData[i + 1])
                    {
                        tempSpectrumData[i - 1] = tempSpectrumData[i];
                    }
                    else
                    {
                        tempSpectrumData[i + 1] = tempSpectrumData[i];
                    }
                }
            }
            
            for (int i = 1; i < tempSpectrumData.Count - 1; i++)
            {
                if (Math.Abs(tempSpectrumData[i] - tempSpectrumData[i - 1]) < 20 && tempSpectrumData[i] < tempSpectrumData[i + 1])
                {
                    tempSpectrumData[i] = (tempSpectrumData[i + 1] + tempSpectrumData[i - 1]) / 3.5;
                }
            }            
        }

        private void DrawWave(List<Vector2> catmullRomList, Color C)
        {
            if (catmullRomList != null)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                for (int i = 0; i < catmullRomList.Count - 1; i++)
                {                    
                    GL.Begin(PrimitiveType.Triangles);

                    GL.Color3(C);
                    GL.Vertex2(catmullRomList[i]);                    
                    GL.Vertex2(window.Width / 2, window.Height / 2);                    
                    GL.Vertex2(catmullRomList[i + 1]);                    
                    
                    GL.End();                   
                }
                GL.Disable(EnableCap.Blend);
            }
        }

        private void DrawGlow(List<Vector2> replayList, Color C)
        {
            if (replayList != null)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                double dif;
                int alpha = 0;

                for (int i = 0; i < replayList.Count - 1; i++)
                {
                    dif = VectorLenght(replayList[i]) - Radius;

                    alpha = (int)dif * 200 / 30;

                    if (alpha > 200)
                    {
                        alpha = 200;
                    }
                    else if (alpha < 40)
                    {
                        alpha = 40;
                    }

                    GL.Begin(PrimitiveType.Quads);

                    GL.Color4(Color.FromArgb(alpha, C));
                    GL.Vertex2(replayList[i]);

                    GL.Color4(Color.FromArgb(0, C));
                    GL.Vertex2(IncreaseVector(0.10f, replayList[i]));

                    GL.Color4(Color.FromArgb(0, C));
                    GL.Vertex2(IncreaseVector(0.10f, replayList[i + 1]));

                    GL.Color4(Color.FromArgb(alpha, C));
                    GL.Vertex2(replayList[i + 1]);

                    GL.End();
                }
                GL.Disable(EnableCap.Blend);
            }
        }
        
        private Vector2 IncreaseVector(float multiplier, Vector2 vectorToIncrease)
        {
            Vector2 vectorToOrigin = new Vector2((vectorToIncrease.X - window.Width / 2) * multiplier, (vectorToIncrease.Y - window.Height / 2) * multiplier);

            Vector2 increasedVector = new Vector2(vectorToIncrease.X + vectorToOrigin.X, vectorToIncrease.Y + vectorToOrigin.Y);
            
            return increasedVector;
        }
        
        private double VectorLenght(Vector2 vector)
        {
            return Math.Sqrt(Math.Pow(vector.X - window.Width / 2, 2) + Math.Pow(vector.Y - window.Height / 2, 2));
        }

        #endregion

        #region Circles

        private void DrawCircle(double X, double Y, double Radio, Color C)
        {            
            GL.Color3(C);
            GL.Begin(PrimitiveType.Polygon);

            double rads, PosX, PosY;

            for (int i = 0; i <= 360; i += 2)
            {
                rads = Math.PI * i / 180;
                PosX = X + (Math.Sin(rads) * Radio);
                PosY = Y + (Math.Cos(rads) * Radio);

                GL.Vertex2(PosX, PosY);
            }

            GL.End();
        }

        private void DrawPrincipalCircle()
        {
            DrawCircle(window.Width / 2, window.Height / 2, Radius, Color.White);
            //DrawCircle(window.Width / 2, window.Height / 2, Radius, Color.FromArgb(255, _beatDetection.waitTicks * 10, _beatDetection.waitTicks * 10, _beatDetection.waitTicks * 10));
            //DrawCircle(ventana.Width / 2, ventana.Height / 2, Radius - 10, Color.Black);
        }

        private void CalculateRadius(List<double> Data)
        {                
            Radius = 0;

            for (int i = 0; i < Data.Count; i++)
            {
                Radius += (float)Data[i] * waveMultiplier;
            }

            Radius = ((Radius / Data.Count - 1) * 2) + (window.Height / 4) + 1;            
        }

        #endregion

        #region Textures

        private bool LoadTexture(ref int texture, string fileName, Texture.ImageMode imageMode)
        {
            try
            {
                texture = _texture.LoadTexture(fileName, imageMode);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void DrawTexture(int texture, float x, float y, float xMax, float yMax, int a, int r, int g, int b)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Color4(Color.FromArgb(a, r, g, b));
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 1);
            GL.Vertex2(x, y);

            GL.TexCoord2(0, 0);
            GL.Vertex2(x, yMax);

            GL.TexCoord2(1, 0);
            GL.Vertex2(xMax, yMax);

            GL.TexCoord2(1, 1);
            GL.Vertex2(xMax, y);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }

        private void BackgroundChangeAnimation()
        {
            if (_config.ActualBackground_Dim <= 0 && !restoreBackground_Dim)
            {                
                if (reverseAnimation)
                {
                    BackgroundTexture = _texture.PreviousBackground();
                    reverseAnimation = false;
                }
                else
                {
                    BackgroundTexture = _texture.NextBackground();
                }                
                restoreBackground_Dim = true;

                UpdateBackgroundScale();
            }

            if (!restoreBackground_Dim)
            {
                _config.ActualBackground_Dim--;
            }
            else
            {
                if (_config.ActualBackground_Dim < _config.Background_Dim)
                {
                    _config.ActualBackground_Dim++;
                }
                else
                {
                    restoreBackground_Dim = false;
                    startAnimation = false;                    
                }
            }
        }

        private void UpdateBackgroundScale()
        {
            IsFullBackground = _texture.IsFullBackgroundIndex();

            scaleBackground = _texture.ScaleBackground(_texture.BackgroundIndex);

            scaleFactor = _texture.GetBackgroundScale(_texture.BackgroundIndex);
        }

        #endregion
    }
}
