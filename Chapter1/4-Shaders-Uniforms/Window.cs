using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using LearnOpenTK.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK
{
    // This project will explore how to use uniform variable type which allows you to assign values
    // to shaders at any point during the project.
    public class Window : GameWindow
    {

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        // So we're going make the triangle pulsate between a color range.
        // In order to do that, we'll need a constantly changing value.
        // The stopwatch is perfect for this as it is constantly going up.
        private Stopwatch _timer;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            // We start the stopwatch here as this method is only called once.
            _timer = new Stopwatch();
            _timer.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            // Here, we get the total seconds that have elapsed since the last time this method has reset
            // and we assign it to the timeValue variable so it can be used for the pulsating color.
            double timeValue = _timer.Elapsed.TotalSeconds;

            // We're increasing / decreasing the green value we're passing into
            // the shader based off of timeValue we created in the previous line,
            // as well as using some built in math functions to help the change be smoother.
            float greenValue = (float)Math.Sin(timeValue) / 2.0f + 0.5f;

            // This gets the uniform variable location from the frag shader so that we can 
            // assign the new green value to it.
            int vertexColorLocation = GL.GetUniformLocation(_shader.Handle, "ourColor");

            // Here we're assigning the ourColor variable in the frag shader 
            // via the OpenGL Uniform method which takes in the value as the individual vec values (which total 4 in this instance).
            GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

            // You can alternatively use this overload of the same function.
            // GL.Uniform4(vertexColorLocation, new OpenTK.Mathematics.Color4(0f, greenValue, 0f, 0f));

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }
}