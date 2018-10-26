using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace LearnOpenGL_TK
{
    class Game : GameWindow
    {
        int ElementBufferObject;
        int VertexBufferObject;
        int VertexArrayObject;
        Shader shader;

        //For documentation on this, check Texture.cs
        Texture texture;


        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            //Because we're adding a texture, we modify the vertex array to include texture coordinates.
            //Texture coordinates range from 0.0 to 1.0, with (0.0, 0.0) representing the bottom left, and (1.0, 1.0) representing the top right
            //The new layout is three floats to create a vertex, then two floats to create the coordinates
            float[] vertices =
            {
                //Position          Texture coordinates
                 0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f,// bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,// bottom left
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f// top left 
            };

            uint[] indices =
            {
                0, 1, 3,
                1, 2, 3
            };


            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


            //The shaders have been modified to include the texture coordinates, check them out after finishing the OnLoad function.
            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();


            texture = new Texture("container.png");
            texture.Use();


            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);

            //Because there's now 5 floats between the start of the first vertex and the start of the second,
            //we modify this from 3 * sizeof(float) to 5 * sizeof(float).
            //This will now pass the new vertex array to the buffer.
            int vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            //Next, we also setup texture coordinates. It works in much the same way.
            //We add an offset of 3, since the first vertex coordinate comes after the first vertex
            //and change the amount of data to 2 because there's only 2 floats for vertex coordinates
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            base.OnLoad(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            while (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine("An error occurred!");

            GL.BindVertexArray(VertexArrayObject);

            texture.Use();
            shader.Use();

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();

            base.OnRenderFrame(e);
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
            //Don't forget to dispose of the texture too!
            texture.Dispose();
            base.OnUnload(e);
        }
    }
}
