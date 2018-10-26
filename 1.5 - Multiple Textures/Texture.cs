using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System.Drawing.Imaging;
using System.Drawing;

namespace LearnOpenGL_TK
{
    public class Texture : IDisposable
    {
        int Handle;

        public Texture(string path, RotateFlipType rotate = RotateFlipType.RotateNoneFlipNone)
        {
            Handle = GL.GenTexture();
            Use();

            var image = new Bitmap(path);

            //Flip the texture according to the rotation
            image.RotateFlip(rotate);

            OpenTK.Graphics.OpenGL4.PixelFormat format;
            if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                format = OpenTK.Graphics.OpenGL4.PixelFormat.Bgr;
            }
            else
            {
                format = OpenTK.Graphics.OpenGL4.PixelFormat.Bgra;
            }

            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, format, PixelType.UnsignedByte, bitmapData.Scan0);

            image.UnlockBits(bitmapData);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Texture()
        {
            GL.DeleteProgram(Handle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
