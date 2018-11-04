using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace LearnOpenGL_TK
{
    //So you can setup OpenGL, you can draw basic shapes without wasting vertices, and you can texture them.
    //There's one big thing left, though: moving the shapes.
    //To do this, we use linear algebra to move the vertices in the vertex shader.

    //Just as a disclaimer: this tutorial will NOT explain linear algebra or matrices; those topics are wayyyyy too complex to do with comments.
    //If you want a more detailed understanding of what's going on here, look at the web version of this tutorial instead.
    //A deep understanding of linear algebra won't be necessary for this tutorial as OpenTK includes built-in matrix types that abstract over the actual math.

    //Head down to RenderFrame to see how we can apply transformations to our shape.
    class Game : GameWindow
    {
        float[] vertices =
        {
            //Position          Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left 
        };

        uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        int ElementBufferObject;
        int VertexBufferObject;
        int VertexArrayObject;

        Shader shader;
        Texture texture;
        Texture texture2;


        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


            //shader.frag has been modified yet again, take a look at it as well.
            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();

            texture = new Texture("container.png");
            texture.Use(TextureUnit.Texture0);

            texture2 = new Texture("awesomeface.png");
            texture2.Use(TextureUnit.Texture1);

            shader.SetInt("texture0", 0);
            shader.SetInt("texture1", 1);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);


            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);


            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            base.OnLoad(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(VertexArrayObject);

            //Note: The matrices we'll use for transformations are all 4x4.

            //We start with an identity matrix. This is just a simple matrix that doesn't move the vertices at all.
            Matrix4 transform = Matrix4.Identity;

            //The next few steps just show how to use OpenTK's matrix functions, and aren't necessary for the transform matrix to actually work.
            //If you want, you can just pass the identity matrix to the shader, though it won't affect the vertices at all.

            //To combine two matrices, you multiply them. Here, we combine the transform matrix with another one created by OpenTK to rotate it by 20 degrees.
            //Note that all Matrix4.CreateRotation functions take radians, not degrees. Use MathHelper.DegreesToRadians() to convert to radians, if you want to use degrees.
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(20));

            //Next, we scale the matrix. This will make the rectangle slightly larger.
            transform *= Matrix4.CreateScale(1.1f);

            //Then, we translate the matrix, which will move it slightly towards the top-right.
            //Note that we aren't using a full coordinate system yet, so the translation is in normalized device coordinates.
            //The next tutorial will be about how to set one up so we can use more human-readable numbers.
            transform *= Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);

            texture.Use(TextureUnit.Texture0);
            texture2.Use(TextureUnit.Texture1);
            shader.Use();

            //Now that the matrix is finished, pass it to the vertex shader.
            //Go over to shader.vert to see how we finally apply this to the vertices
            shader.SetMatrix4("transform", transform);

            //And that's it for now! In the next tutorial, I'll show you how to setup a full coordinates system.

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }


        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            shader.Dispose();
            texture.Dispose();
            texture2.Dispose();

            base.OnUnload(e);
        }
    }
}
