﻿using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK
{
    // We can now move around objects. However, how can we move our "camera", or modify our perspective?
    // In this tutorial, I'll show you how to setup a full projection/view/model (PVM) matrix.
    // In addition, we'll make the rectangle rotate over time.
    public class Window : GameWindow
    {
        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private int _texture;

        private int _texture2;

        // We create a double to hold how long has passed since the program was opened.
        private double _time;

        // Then, we create two matrices to hold our view and projection. They're initialized at the bottom of OnLoad.
        // The view matrix is what you might consider the "camera". It represents the current viewport in the window.
        private Matrix4 _view;

        // This represents how the vertices will be projected. It's hard to explain through comments,
        // so check out the web version for a good demonstration of what this does.
        private Matrix4 _projection;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // We enable depth testing here. If you try to draw something more complex than one plane without this,
            // you'll notice that polygons further in the background will occasionally be drawn over the top of the ones in the foreground.
            // Obviously, we don't want this, so we enable depth testing. We also clear the depth buffer in GL.Clear over in OnRenderFrame.
            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // shader.vert has been modified. Take a look at it after the explanation in OnRenderFrame.
            _shader = Shader.FromFile("Shaders/shader.vert", "Shaders/shader.frag");
            GL.UseProgram(_shader.Handle);

            var vertexLocation = 0; // The location of aPos
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = 1; // The location of aTexCoord
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Resources/container.png");
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            _texture2 = Texture.LoadFromFile("Resources/awesomeface.png");
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _texture2);

            GL.Uniform1(_shader.UniformLocations["texture0"], 0);
            GL.Uniform1(_shader.UniformLocations["texture1"], 1);

            // For the view, we don't do too much here. Next tutorial will be all about a Camera class that will make it much easier to manipulate the view.
            // For now, we move it backwards three units on the Z axis.
            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);

            // For the matrix, we use a few parameters.
            //   Field of view. This determines how much the viewport can see at once. 45 is considered the most "realistic" setting, but most video games nowadays use 90
            //   Aspect ratio. This should be set to Width / Height.
            //   Near-clipping. Any vertices closer to the camera than this value will be clipped.
            //   Far-clipping. Any vertices farther away from the camera than this value will be clipped.
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float) Size.Y, 0.1f, 100.0f);

            // Now, head over to OnRenderFrame to see how we setup the model matrix.
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // We add the time elapsed since last frame, times 4.0 to speed up animation, to the total amount of time passed.
            _time += 4.0 * e.Time;

            // We clear the depth buffer in addition to the color buffer.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _texture2);

            GL.UseProgram(_shader.Handle);

            // Finally, we have the model matrix. This determines the position of the model.
            var model = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));

            // Then, we pass all of these matrices to the vertex shader.
            // You could also multiply them here and then pass, which is faster, but having the separate matrices available is used for some advanced effects.

            // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
            // They are then transposed properly when passed to the shader. 
            // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
            // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
            // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
            // and finally apply the viewToProjectedSpace (aka projection) matrix.
            GL.UniformMatrix4(_shader.UniformLocations["model"], true, ref model);
            GL.UniformMatrix4(_shader.UniformLocations["view"], true, ref _view);
            GL.UniformMatrix4(_shader.UniformLocations["projection"], true, ref _projection);

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

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