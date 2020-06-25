using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace CSharpNation
{
    class Texture
    {
        public enum ImageMode
        {
            notFound,
            full,
            split,
            invertSplit,
            fullSplit,
            invertFullSplit,

            splitSecondHalf,            
            invertSplitSecondHalf,            
        }

        private List<int> TexturesLoaded = new List<int>();
        private List<string> TexturesName = new List<string>();
        private List<string> backgroundConfig = new List<string>();
        private List<int> fullBackgroundIndex = new List<int>();
        private List<float> backgroundScale = new List<float>();
        public List<string> errors = new List<string>();

        public int BackgroundIndex { get; set; } = 0;

        public int LoadTexture(string file, ImageMode imageMode, bool handled = false)
        {
            if (!File.Exists("Resources/" + file))
            {
                throw new FileNotFoundException("Archivo no encontrado");
            }
           
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            
            Bitmap bmp = new Bitmap("Resources/" + file);

            if ((bmp.Width < bmp.Height && !handled) || imageMode == ImageMode.full)
            {
                imageMode = ImageMode.fullSplit;
            }            

            if(imageMode == ImageMode.notFound)
            {
                imageMode = ImageMode.fullSplit;
            }

            if(imageMode == ImageMode.split || imageMode == ImageMode.invertSplit || imageMode == ImageMode.splitSecondHalf || imageMode == ImageMode.invertSplitSecondHalf)
            {
                backgroundScale.Add((float)bmp.Height / (float)(bmp.Width / 2));
            }
            else
            {
                backgroundScale.Add((float)bmp.Height / (float)bmp.Width);
            }
            
            BitmapData data = new BitmapData();

            switch(imageMode)
            {                
                case ImageMode.split:

                    data = bmp.LockBits(new Rectangle(0, 0, bmp.Width / 2, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    break;

                case ImageMode.fullSplit:

                    data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    break;

                case ImageMode.invertSplit:

                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    data = bmp.LockBits(new Rectangle(bmp.Width / 2, 0, bmp.Width / 2, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    break;

                case ImageMode.invertFullSplit:

                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    break;

                case ImageMode.splitSecondHalf:

                    data = bmp.LockBits(new Rectangle(bmp.Width / 2, 0, bmp.Width / 2, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    break;

                case ImageMode.invertSplitSecondHalf:

                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    data = bmp.LockBits(new Rectangle(0, 0, bmp.Width / 2, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    break;
            }                     

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            bmp.Dispose();

            return id;
        }

        public float GetBackgroundScale(int index)
        {           
            return backgroundScale[index];
        }

        public bool ScaleBackground(int index)
        {
            for(int i = 0; i < backgroundConfig.Count; i++)
            {
                if(backgroundConfig[i].Contains(TexturesName[index]))
                {                    
                    string value = backgroundConfig[i].Split('|')[2];

                    if(value == true.ToString())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public int UpdateScaleMode(int index, bool enabled)
        {
            ImageMode imageMode = SearchConfig(TexturesName[index]);
            int configIndex = -1;

            for (int i = 0; i < backgroundConfig.Count; i++)
            {
                if (backgroundConfig[i].Contains(TexturesName[index]))
                {
                    configIndex = i;
                    break;
                }
            }

            if(configIndex < 0)
            {
                AddBackgroundConfig(imageMode);
                return UpdateScaleMode(index, enabled);
            }

            string actualConfig = backgroundConfig[configIndex];                        

            backgroundConfig[configIndex] = actualConfig.Split('|')[0] + "|" + actualConfig.Split('|')[1] + "|" + enabled.ToString();            

            TexturesLoaded[index] = LoadTexture("Backgrounds/" + TexturesName[index], imageMode, true);

            return TexturesLoaded[index];
        }

        private void LoadBackgroundConfig()
        {
            backgroundConfig.Clear();
            string[] lines = File.ReadAllLines("Resources\\BackgroundConfig.txt");

            for(int i = 0; i < lines.Length; i++)
            {
                backgroundConfig.Add(lines[i]);
            }
        }

        private void WriteBackgroundConfig()
        {
            string[] lines = backgroundConfig.ToArray();

            File.WriteAllLines("Resources\\BackgroundConfig.txt", lines);
        }

        private void AddBackgroundConfig(ImageMode imageMode)
        {
            for(int i = 0; i < backgroundConfig.Count; i++)
            {
                if(backgroundConfig[i].Contains(TexturesName[BackgroundIndex]))
                {
                    backgroundConfig.RemoveAt(i);
                }
            }
            backgroundConfig.Add(TexturesName[BackgroundIndex] + "|" + imageMode.ToString() + "|" + false.ToString());
        }

        private ImageMode SearchConfig(string fileName)
        {
            for(int i = 0; i < backgroundConfig.Count; i++)
            {
                if(backgroundConfig[i].Contains(fileName))
                {
                    string mode = backgroundConfig[i].Split('|')[1];

                    if(mode == ImageMode.full.ToString())
                    {
                        return ImageMode.full;
                    }
                    else if(mode == ImageMode.split.ToString())
                    {
                        return ImageMode.split;
                    }
                    else if(mode == ImageMode.invertSplit.ToString())
                    {
                        return ImageMode.invertSplit;
                    }
                    else if (mode == ImageMode.fullSplit.ToString())
                    {
                        return ImageMode.fullSplit;
                    }
                    else if (mode == ImageMode.invertFullSplit.ToString())
                    {
                        return ImageMode.invertFullSplit;
                    }
                    else if (mode == ImageMode.splitSecondHalf.ToString())
                    {
                        return ImageMode.splitSecondHalf;
                    }
                    else if (mode == ImageMode.invertSplitSecondHalf.ToString())
                    {
                        return ImageMode.invertSplitSecondHalf;
                    }
                }
            }

            return ImageMode.notFound;
        }

        public void LoadBackgrounds(string[] Paths)
        {
            LoadBackgroundConfig();

            for (int i = 0; i < Paths.Length; i++)
            {
                try
                {
                    ImageMode imageMode = SearchConfig(Paths[i]);

                    if (imageMode == ImageMode.notFound)
                    {
                        TexturesLoaded.Add(LoadTexture("Backgrounds/" + Paths[i], ImageMode.split));
                    }
                    else
                    {
                        if (imageMode == ImageMode.full)
                        {
                            fullBackgroundIndex.Add(TexturesLoaded.Count);
                        }

                        TexturesLoaded.Add(LoadTexture("Backgrounds/" + Paths[i], imageMode, true));
                    }

                    TexturesName.Add(Paths[i]);
                }
                catch
                {
                    errors.Add("An error occurred while trying to load the image (" + Paths[i] + ")");
                }
            }
        }

        public int NextBackground()
        {
            BackgroundIndex++;
            if (BackgroundIndex >= TexturesLoaded.Count)
            {
                BackgroundIndex = 0;
            }

            return TexturesLoaded[BackgroundIndex];
        }

        public int PreviousBackground()
        {
            BackgroundIndex--;
            if (BackgroundIndex < 0)
            {
                BackgroundIndex = TexturesLoaded.Count - 1;
            }

            return TexturesLoaded[BackgroundIndex];
        }

        public int ReloadBackground()
        {
            return LoadTexture("Backgrounds/" + TexturesName[BackgroundIndex], ImageMode.invertSplitSecondHalf);
        }
        
        public int GetBackgroundByIndex(int index)
        {
            BackgroundIndex = index;
            return TexturesLoaded[BackgroundIndex];
        } 

        public List<string> GetBackgroundsList()
        {
            return TexturesName;
        }

        public int ChangeBackgroundImageMode(ImageMode imageMode)
        {
            if(imageMode == ImageMode.full)
            {
                fullBackgroundIndex.Add(BackgroundIndex);
            }
            if (imageMode != ImageMode.notFound)
            {
                if(imageMode != ImageMode.full)
                {
                    for(int i = 0; i < fullBackgroundIndex.Count; i++)
                    {
                        if(fullBackgroundIndex[i] == BackgroundIndex)
                        {
                            fullBackgroundIndex.RemoveAt(i);
                        }
                    }
                }

                TexturesLoaded[BackgroundIndex] = LoadTexture("Backgrounds/" + TexturesName[BackgroundIndex], imageMode, true);
                AddBackgroundConfig(imageMode);                
                return TexturesLoaded[BackgroundIndex];
            }
            else
            {
                return TexturesLoaded[BackgroundIndex];
            }
        }

        public bool IsFullBackgroundIndex()
        {
            for(int i = 0; i < fullBackgroundIndex.Count; i++)
            {
                if(BackgroundIndex == fullBackgroundIndex[i])
                {
                    return true;
                }
            }

            return false;
        }

        public void SaveBackgroundConfig()
        {
            WriteBackgroundConfig();
        }

        public bool HasTexturesLoaded()
        {
            if (TexturesLoaded.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void WriteErrors()
        {            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            for(int i = 0; i < errors.Count; i++)
            {
                Console.WriteLine(errors[i]);
            }                       
        }
    }
}
