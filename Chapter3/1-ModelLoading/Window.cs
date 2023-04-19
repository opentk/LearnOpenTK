﻿using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK
{
    // In this tutorial we focus on importing a model using Assimp and then drawing the model in our scene.
    public class Window : GameWindow
    {
        private Model _backPackModel;

        private Shader _backPackShader;

        private Texture _backPackTexture;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            // Backpack by Berk Gedik: https://sketchfab.com/3d-models/survival-guitar-backpack-low-poly-799f8c4511f84fab8c3f12887f7e6b36
            // Here we import the backpack model object, texture and define our Shader.
            // For now we are just using a simple shader which just samples the texture.
            _backPackModel = new Model("Resources/Backpack/Survival_BackPack_2.fbx");
            _backPackTexture = Texture.LoadFromFile("Resources/Backpack/1001_albedo.jpg");
            _backPackShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // First we setup the shader, including the texture uniform and then call the Draw() method on the imported model to draw all the contained meshes
            _backPackShader.Use();
            _backPackShader.SetMatrix4("model", Matrix4.Identity);
            _backPackShader.SetMatrix4("view", _camera.GetViewMatrix());
            _backPackShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            _backPackTexture.Use(TextureUnit.Texture0);
            _backPackShader.SetInt("texture0", 0);

            // For each mesh in the model, bind the VAO (that contains all of the vertices and indices) and draw the triangles
            foreach (Mesh mesh in _backPackModel.meshes)
            {
                GL.BindVertexArray(mesh.VAO);
                GL.DrawElements(PrimitiveType.Triangles, mesh.indicesCount, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }
            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}
