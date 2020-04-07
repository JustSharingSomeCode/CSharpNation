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
        public List<int> TexturesLoaded = new List<int>();
        private int BackgroundIndex = 0;

        public int LoadTexture(string file, int DivideImg = 1)
        {
            if (!File.Exists("Resources/" + file))
            {
                throw new FileNotFoundException("Archivo no encontrado");
            }

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap("Resources/" + file);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width / DivideImg, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return id;
        }

        public void LoadBackgrounds(string[] Paths)
        {
            for (int i = 0; i < Paths.Length; i++)
            {
                TexturesLoaded.Add(LoadTexture("Backgrounds/" + Paths[i], 2));
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
    }
}
