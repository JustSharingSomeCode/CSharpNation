using System;
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
        
        private double Radius;
        private int BackgroundTexture;
        private int LogoTexture;
        private int ParticleTexture;

        private bool restoreBackground_Dim = false;
        private bool startAnimation = false;
        private bool enhancements = true;

        private KeyboardState actualKeyboardState, oldKeyboardState;

        public Game(GameWindow _window)
        {
            window = _window;
            _analizer = new Analizer();
            _analizer.Enable = true;
            replay = new Replay();

            particles = new Particles(window.Width, window.Height, _config);

            if (Directory.Exists("Resources\\Backgrounds"))
            {
                string[] Backgrounds = Directory.GetFiles("Resources\\Backgrounds", "*.jpg");

                for (int i = 0; i < Backgrounds.Length; i++)
                {
                    Backgrounds[i] = Path.GetFileName(Backgrounds[i]);
                }

                _texture.LoadBackgrounds(Backgrounds);

                BackgroundTexture = _texture.TexturesLoaded[0];
            }

            if (!LoadTexture(ref LogoTexture, "Logo.jpg"))
            {
                LoadTexture(ref LogoTexture, "Logo.png");
            }

            if (!LoadTexture(ref ParticleTexture, "particle.jpg"))
            {
                LoadTexture(ref ParticleTexture, "particle.png");
            }

            Start();
        }

        private void Start()
        {
            window.Load += OnDisplay;
            window.Resize += OnResize;
            window.RenderFrame += OnRender;
            window.UpdateFrame += OnUpdate;

            Console.WriteLine("------------------------------------");
            Console.WriteLine("Press O to change settings");
            Console.WriteLine("Press F for fullscreen");
            Console.WriteLine("Press N for next background");

            window.Run(60, 60);
        }

        void OnDisplay(object sender, EventArgs e)
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

            particles.UpdateWindowSize(window.Width, window.Height);
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            KeyPressedActions();

            if (_config.Auto_Change_Background)
            {
                if (_config.UpdateCount > _config.BackgroundChangeSeconds * 60)
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
                BackgroundChangeAnimation();
            }

            tempSpectrumData = _analizer.GetSpectrum();

            for (int i = 0; i < 12; i++)
            {
                tempSpectrumData[i] = tempSpectrumData[i] / 2;
            }

            WaveEnhancements(enhancements);

            CalculateRadius(tempSpectrumData);

            CreateControlPoints();

            float aditionalVelocity = (float)((Radius - (window.Height / 4)) * 0.3);
            //Console.WriteLine(aditionalVelocity);

            particles.updateParticles(ParticleTexture, aditionalVelocity);

            particlesList = particles.GetParticlesList();

            replay.Push(catmullRomList);
        }

        private void OnRender(object sender, EventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            float AspectRatio = 9f / 16f;
            double increaseX = (Radius - (window.Height / 4)) / 1.5;
            double increaseY = AspectRatio * increaseX;                       

            DrawTexture(BackgroundTexture, 0 - increaseX, 0 - increaseY, window.Width / 2, window.Height + increaseY, _config.GetAlpha(), 255, 255, 255);//(Radius - (window.Height / 4))
            DrawTexture(BackgroundTexture, window.Width + increaseX, 0 - increaseY, window.Width / 2, window.Height + increaseY, _config.GetAlpha(), 255, 255, 255);

            for (int i = 0; i < particlesList.Count; i++)
            {
                double x = particlesList[i].position.X - (particlesList[i].radius / 2);
                double y = particlesList[i].position.Y - (particlesList[i].radius / 2);
                double xMax = particlesList[i].position.X + (particlesList[i].radius / 2);
                double yMax = particlesList[i].position.Y + (particlesList[i].radius / 2);

                DrawTexture(particlesList[i].texture, x, y, xMax, yMax, particlesList[i].opacity, 255, 255, 255);
            }

            DrawWave(replay.GetCatmullRomPoints(0), Color.FromArgb(0, 255, 0));
            DrawWave(replay.GetCatmullRomPoints(2), Color.FromArgb(51, 204, 255));
            DrawWave(replay.GetCatmullRomPoints(4), Color.Blue);
            DrawWave(replay.GetCatmullRomPoints(6), Color.FromArgb(51, 51, 153));
            DrawWave(replay.GetCatmullRomPoints(8), Color.FromArgb(255, 102, 255));
            DrawWave(replay.GetCatmullRomPoints(10), Color.Red);
            DrawWave(replay.GetCatmullRomPoints(12), Color.Yellow);
            DrawWave(catmullRomList, Color.White);

            /*
            for (int i = 0; i < controlPointsList.Count; i++)
            {
                DrawCircle(controlPointsList[i].X, controlPointsList[i].Y, 5, Color.Green);
            }
            */

            DrawPrincipalCircle();
            DrawTexture(LogoTexture, (window.Width / 2) - Radius + 10, (window.Height / 2) - Radius + 10, (window.Width / 2) + Radius - 10, (window.Height / 2) + Radius - 10, 255, 255, 255, 255);

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

            if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.M))
            {
                enhancements = !enhancements;
                tempSpectrumData = null;                
            }

            if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.N))
            {
                startAnimation = true;
            }

            if (KeyPressed(actualKeyboardState, oldKeyboardState, Key.O))
            {
                _config.ChangeSettings();
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
                Size = spectrumData[Rep] + Radius;
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

                for (int i = 0; i < spectrumData.Count; i++)
                {
                    if (Math.Abs(spectrumData[i] - tempSpectrumData[i]) > 10)
                    {
                        if (spectrumData[i] > tempSpectrumData[i])
                        {
                            spectrumData[i] -= 10;
                        }
                        else
                        {
                            spectrumData[i] += 10;
                        }
                    }
                    else
                    {
                        spectrumData[i] = tempSpectrumData[i];
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
                for (int i = 0; i < catmullRomList.Count - 1; i++)
                {
                    GL.Begin(PrimitiveType.Triangles);

                    GL.Color3(C);
                    GL.Vertex2(catmullRomList[i]);
                    GL.Color3(C);
                    GL.Vertex2(window.Width / 2, window.Height / 2);
                    GL.Color3(C);
                    GL.Vertex2(catmullRomList[i + 1]);

                    GL.End();
                }
            }
        }

        #endregion

        #region Circles

        private void DrawCircle(double X, double Y, double Radio, Color C)
        {
            GL.Color3(C);
            GL.Begin(PrimitiveType.TriangleFan);

            for (int i = 0; i <= 360; i++)
            {
                double PosX = X + (Math.Sin(i) * Radio);
                double PosY = Y + (Math.Cos(i) * Radio);

                GL.Vertex2(PosX, PosY);
            }

            GL.End();
        }

        private void DrawPrincipalCircle()
        {
            DrawCircle(window.Width / 2, window.Height / 2, Radius, Color.White);
            //DrawCircle(ventana.Width / 2, ventana.Height / 2, Radius - 10, Color.Black);
        }

        private void CalculateRadius(List<double> Data)
        {
            Radius = 0;

            for (int i = 0; i < Data.Count; i++)
            {
                Radius += Data[i];
            }

            Radius = ((Radius / Data.Count - 1) * 2) + (window.Height / 4) + 1;
        }

        #endregion

        #region Textures

        private bool LoadTexture(ref int texture, string fileName, int DivideImg = 1)
        {
            try
            {
                texture = _texture.LoadTexture(fileName, DivideImg);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void DrawTexture(int texture, double x, double y, double xMax, double yMax, int a, int r, int g, int b)
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
            if (_config.ActualBackground_Dim <= 0)
            {
                BackgroundTexture = _texture.NextBackground();
                restoreBackground_Dim = true;
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

        #endregion
    }
}
