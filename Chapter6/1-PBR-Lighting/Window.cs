using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LearnOpenTK
{


    // In this tutorial we set up some basic lighting and look at how the phong model works
    // For more insight into how it all works look at the web version. If you are just here for the source,
    // most of the changes are in the shaders, specifically most of the changes are in the fragment shader as this is
    // where the lighting calculations happens.
    public class Window : GameWindow
    {
        // settings
        const uint SCR_WIDTH = 1280;
        const uint SCR_HEIGHT = 720;

        // camera
        private Camera _camera;

        float lastX = 800.0f / 2.0f;
        float lastY = 600.0f / 2.0f;
        bool firstMouse = true;

        // timing
        float deltaTime = 0.0f;
        float lastFrame = 0.0f;
        private Stopwatch _timer;

        int sphereVAO = 0;
        int indexCount;
        int nrRows = 7;
        int nrColumns = 7;
        float spacing = 2.5f;
        const int X_SEGMENTS = 64;
        const int Y_SEGMENTS = 64;

        Vector3 albedoVec = new Vector3(0.5f, 0.0f, 0.0f);

        Matrix4 lightSpaceMatrix;
        float prevMouseWheel;

        // renderQuad() renders a 1x1 XY quad in NDC
        // -----------------------------------------
        int quadVAO = 0;
        int quadVBO;

        // lighting info
        // -------------
        // lights
        // ------
        Vector3[] lightPositions = {
            new Vector3(-10.0f,  10.0f, 10.0f),
            new Vector3( 10.0f,  10.0f, 10.0f),
            new Vector3(-10.0f, -10.0f, 10.0f),
            new Vector3( 10.0f, -10.0f, 10.0f),
        };
        Vector3[] lightColors = {
            new Vector3(300.0f, 300.0f, 300.0f),
            new Vector3(300.0f, 300.0f, 300.0f),
            new Vector3(300.0f, 300.0f, 300.0f),
            new Vector3(300.0f, 300.0f, 300.0f)
        };





        private readonly Vector3 lightPos = new Vector3(-2.0f, 4.0f, -1.0f);

        private int _vertexBufferObject;

        private int _vaoModel;

        private int _vaoLamp;

        private Shader shader, simpleDepthShader, debugDepthQuad;
        private int planeVAO;
        private int planeVBO;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected void InitGL()
        {
            // configure GL.obal openGL. state
            // -----------------------------
            GL.Enable(EnableCap.DepthTest);

            // initialize static shader uniforms before rendering
            // --------------------------------------------------
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Fov), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);

            // build and compile shaders
            // -------------------------
            shader = new Shader("Shaders/pbr.vs", "Shaders/pbr.fs");
            shader.Use();
            shader.SetMatrix4("projection", projection);
        }
        protected override void OnLoad()
        {
            base.OnLoad();


            // We start the stopwatch here as this method is only called once.
            _timer = new Stopwatch();
            _timer.Start();

            _camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f), (float)Size.X / (float)Size.Y);
            InitGL();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            shader.Use();
            shader.SetVector3("albedo", albedoVec);
            shader.SetFloat("ao", 1.0f);

            // per-frame time logic
            // --------------------
            //float currentFrame = static_cast<float>(GL.fwGetTime());
            //deltaTime = currentFrame - lastFrame;
            //lastFrame = currentFrame;

            // render
            // ------
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            shader.Use();
            Matrix4 view = _camera.GetViewMatrix();
            shader.SetMatrix4("view", view);
            shader.SetVector3("camPos", _camera.Position);

            // render rows*column number of spheres with varying metallic/roughness values scaled by rows and columns respectively
            Matrix4 model = Matrix4.Identity;
            for (int row = 0; row < nrRows; ++row)
            {
                shader.SetFloat("metallic", (float)row / (float)nrRows);
                for (int col = 0; col < nrColumns; ++col)
                {
                    // we clamp the roughness to 0.05 - 1.0 as perfectly smooth surfaces (roughness of 0.0) tend to look a bit off
                    // on direct lighting.
                    shader.SetFloat("roughness", Math.Clamp((float)col / (float)nrColumns, 0.05f, 1.0f));

                    model = Matrix4.Identity;
                    model = model * Matrix4.CreateTranslation(new Vector3(
                        (col - (nrColumns / 2)) * spacing,
                        (row - (nrRows / 2)) * spacing,
                        0.0f
                    ));

                    shader.SetMatrix4("model", model);
                    //shader.SetMatrix3("normalMatrix", Matrix3.Transpose(Matrix3.Invert(new Matrix3(model))));
                    renderSphere();
                }
            }

            // render light source (simply re-render sphere at light positions)
            // this looks a bit off as we use the same shader, but it'll make their positions obvious and 
            // keeps the codeprint small.
            for (uint i = 0; i < lightPositions.Length; ++i)
            {
                Vector3 newPos = lightPositions[i] + new Vector3((float)Math.Sin(GLFW.GetTime() * 5.0) * 5.0f, 0.0f, 0.0f);
                newPos = lightPositions[i];
                shader.SetVector3("lightPositions[" + i + "]", newPos);
                shader.SetVector3("lightColors[" + i + "]", lightColors[i]);

                
                model = Matrix4.Identity;
                model = model * Matrix4.CreateScale(new Vector3(0.5f));
                model = model * Matrix4.CreateTranslation(newPos);

                shader.SetMatrix4("model", model);
                //shader.SetMatrix3("normalMatrix", Matrix3.Transpose(Matrix3.Invert(new Matrix3(model))));
                renderSphere();
            }

            // GL.fw: swap buffers and poll IO events (keys pressed/released, mouse moved etc.)
            // -------------------------------------------------------------------------------
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

            if (prevMouseWheel - e.OffsetY < 0)
                _camera.Fov --;
            else if (prevMouseWheel - e.OffsetY > 0)
                _camera.Fov ++;

            prevMouseWheel = e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            if (_camera != null)
                _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        // renders (and builds at first invocation) a sphere
        // -------------------------------------------------
        void renderSphere()
        {
            if (sphereVAO == 0)
            {
                sphereVAO = GL.GenVertexArray();

                int vbo, ebo;
                vbo = GL.GenBuffer();
                ebo = GL.GenBuffer();

                List<Vector3> positions = new();
                List<Vector2> uv = new();
                List<Vector3> normals = new();
                List< uint> indices = new();

                for (uint x = 0; x <= X_SEGMENTS; ++x)
                {
                    for (uint y = 0; y <= Y_SEGMENTS; ++y)
                    {
                        float xSegment = (float)x / (float)X_SEGMENTS;
                        float ySegment = (float)y / (float)Y_SEGMENTS;
                        float xPos = (float)(Math.Cos(xSegment * 2.0f * Math.PI) * Math.Sin(ySegment *Math.PI));
                        float yPos = (float)Math.Cos(ySegment *Math.PI);
                        float zPos = (float)(Math.Sin(xSegment * 2.0f *Math.PI) * Math.Sin(ySegment *Math.PI));

                        positions.Add(new Vector3(xPos, yPos, zPos));
                        uv.Add(new Vector2(xSegment, ySegment));
                        normals.Add(new Vector3(xPos, yPos, zPos));
                    }
                }

                bool oddRow = false;
                for (uint y = 0; y < Y_SEGMENTS; ++y)
                {
                    if (!oddRow) // even rows: y == 0, y == 2; and so on
                    {
                        for (int x = 0; x <= X_SEGMENTS; ++x)
                        {
                            indices.Add((uint)(y * (X_SEGMENTS + 1) + x));
                            indices.Add((uint)((y + 1) * (X_SEGMENTS + 1) + x));
                        }
                    }
                    else
                    {
                        for (int x = X_SEGMENTS; x >= 0; --x)
                        {
                            indices.Add((uint)((y + 1) * (X_SEGMENTS + 1) + x));
                            indices.Add((uint)(y * (X_SEGMENTS + 1) + x));
                        }
                    }
                    oddRow = !oddRow;
                }
                indexCount = indices.Count;

                List<float> data = new();
                for (int i = 0; i < positions.Count; ++i)
                {
                    data.Add(positions[i].X);
                    data.Add(positions[i].Y);
                    data.Add(positions[i].Z);
                    if (normals.Count > 0)
                    {
                        data.Add(normals[i].X);
                        data.Add(normals[i].Y);
                        data.Add(normals[i].Z);
                    }
                    if (uv.Count > 0)
                    {
                        data.Add(uv[i].X);
                        data.Add(uv[i].Y);
                    }
                }
                GL.BindVertexArray(sphereVAO);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, data.Count * sizeof(float), data.ToArray(), BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);
                int stride = (3 + 2 + 3) * sizeof(float);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            }

            GL.BindVertexArray(sphereVAO);
            GL.DrawElements(PrimitiveType.TriangleStrip, indexCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}
/*


int main()
{
    // GL.fw: initialize and configure
    // ------------------------------
    GL.fwInit();
    GL.fwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    GL.fwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    GL.fwWindowHint(GLFW_SAMPLES, 4);
    GL.fwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

    GL.fwSetFramebufferSizeCallback(window, framebuffer_size_callback);
    GL.fwSetCursorPosCallback(window, mouse_callback);
    GL.fwSetScrollCallback(window, scroll_callback);

    // tell GLFW to capture our mouse
    GL.fwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

    // build and compile shaders
    // -------------------------
    Shader shader("1.1.pbr.vs", "1.1.pbr.fs");



    // initialize static shader uniforms before rendering
    // --------------------------------------------------
    Matrix4 projection = GL.m::perspective(GL.m::radians(camera.Zoom), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
    shader.use();

    // render loop
    // -----------
    while (!GL.fwWindowShouldClose(window))
    {

    }

    // GL.fw: terminate, clearing all previously allocated GLFW resources.
    // ------------------------------------------------------------------
    GL.fwTerminate();
    return 0;
}


// GL.fw: whenever the window size changed (by OS or user resize) this callback function executes
// ---------------------------------------------------------------------------------------------
void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
    // make sure the viewport matches the new window dimensions; note that width and 
    // height will be significantly larger than specified on retina displays.
    GL.Viewport(0, 0, width, height);
}






*/