using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace LearnOpenGL_TK
{
    //So you've drawn the first triangle. But what about drawing multiple?
    //You may consider just adding more vertices to the array, and that would technically work, but say you're drawing a rectangle.
    //It only needs four vertices, but since OpenGL works in triangles, you'd need to define 6.
    //Not a huge deal, but it quickly adds up when you get to more complex models. For example, a cube only needs 8 vertices, but
    //doing it that way would need 36 vertices!

    //OpenGL provides a way to reuse vertices, which can heavily reduce memory usage on complex objects.
    //This is called an Element Buffer Object. This tutorial will be all about how to set one up.
    class Game : GameWindow
    {
        //We modify the vertex array to include four vertices for our rectangle.
        float[] vertices =
        {
             0.5f,  0.5f, 0.0f, // top right
             0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, // top left 
        };

        //Then, we create a new array: Indices.
        //This array controls how the EBO will use those vertices to create triangles
        uint[] indices =
        {
            //Note that indices start at 0!
            0, 1, 3, //The first triangle will be the bottom-right half of the triangle
            1, 2, 3  //Then the second will be the top-right half of the triangle
        };

        int VertexBufferObject;
        int VertexArrayObject;
        Shader shader;

        //Add a handle for the EBO
        int ElementBufferObject;


        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }
        
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            //We create/bind the EBO the same way as the VBO, just with a different BufferTarget.
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);

            //We also buffer data to the EBO the same way.
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            //The EBO has now been properly setup. Go to the Render function to see how we draw our rectangle now!

            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            //We bind the EBO here too, just like with the VBO in the previous tutorial.
            //Now, the EBO will be bound when we find the VAO.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            base.OnLoad(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Use();

            GL.BindVertexArray(VertexArrayObject);

            //Then replace your call to DrawTriangles with one to DrawElements
            //Arguments:
            //  Primitive type to draw. Triangles in this case.
            //  How many indices should be drawn. Six in this case.
            //  Data type of the indices. The indices are an unsigned int, so we want that here too.
            //  Offset in the EBO. Set this to 0 because we want to draw the whole thing.
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

            base.OnUnload(e);
        }
    }
}
