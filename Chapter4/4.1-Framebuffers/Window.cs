using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace LearnOpenTK
{
    public class Window : GameWindow
    {
        // set up vertex data (and buffer(s)) and configure vertex attributes
        // ------------------------------------------------------------------
        float[] cubeVertices = {
        // positions          // texture Coords
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
    };
        float[] planeVertices = {
        // positions          // texture Coords 
         5.0f, -0.5f,  5.0f,  2.0f, 0.0f,
        -5.0f, -0.5f,  5.0f,  0.0f, 0.0f,
        -5.0f, -0.5f, -5.0f,  0.0f, 2.0f,

         5.0f, -0.5f,  5.0f,  2.0f, 0.0f,
        -5.0f, -0.5f, -5.0f,  0.0f, 2.0f,
         5.0f, -0.5f, -5.0f,  2.0f, 2.0f
        };
        float[] quadVertices = { // vertex attributes for a quad that fills the entire screen in Normalized Device Coordinates. NOTE that this plane is now much smaller and at the top of the screen
        // positions   // texCoords
        -0.3f,  1.0f,  0.0f, 1.0f,
        -0.3f,  0.7f,  0.0f, 0.0f,
         0.3f,  0.7f,  1.0f, 0.0f,

        -0.3f,  1.0f,  0.0f, 1.0f,
         0.3f,  0.7f,  1.0f, 0.0f,
         0.3f,  1.0f,  1.0f, 1.0f
         };
     
        // renderCube() renders a 1x1 3D cube in NDC.
        // -------------------------------------------------
        private Texture cubeTexture, floorTexture;
        // timing
        float deltaTime = 0.0f;
        float lastFrame = 0.0f;
        float prevMouseWheel;

        private Shader shader, screenShader;
        private int cubeVAO, planeVAO, quadVAO;
        private int cubeVBO, planeVBO, quadVBO;
        private Camera _camera;

        private int framebuffer;
        private int textureColorBuffer;
        private int renderBuffer;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        private static void SetupBuffer(ref int vao, ref int vbo, int positionSize, float[] vertices)
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            // position coords
            GL.EnableVertexAttribArray(0); // aPos
            GL.VertexAttribPointer(0, positionSize, VertexAttribPointerType.Float, false, (2 + positionSize) * sizeof(float), 0);
            // texture coords
            GL.EnableVertexAttribArray(1); // aTexCoords
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (2 + positionSize) * sizeof(float), (positionSize * sizeof(float)));
        }

        protected void InitGL()
        {
            // cube VAO
            SetupBuffer(ref cubeVAO, ref cubeVBO, 3, cubeVertices);
            // plane VAO
            SetupBuffer(ref planeVAO, ref planeVBO, 3, planeVertices);
            // screen quad VAO
            SetupBuffer(ref quadVAO, ref quadVBO, 2, quadVertices);

            // load textures
            // -------------
            cubeTexture = Texture.LoadFromFile("Resources/container.jpg");
            floorTexture = Texture.LoadFromFile("Resources/metal.png");

            // build and compile shaders
            // -------------------------
            shader = new Shader("Shaders/framebuffers.vs", "Shaders/framebuffers.fs");
            screenShader = new Shader("Shaders/framebuffers_screen.vs", "Shaders/framebuffers_screen.fs");

            shader.Use();
            shader.SetInt("texture1", 0);

            screenShader.Use();
            screenShader.SetInt("screenTexture", 0);


            // framebuffer configuration
            // -------------------------
            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            // create depth texture
            textureColorBuffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 800, 600, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureColorBuffer, 0);

            // create a renderbuffer object for depth and stencil attachment (we won't be sampling these)

            renderBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, 800, 600); // use a single renderbuffer object for both a depth AND stencil buffer.
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderBuffer); // now actually attach it
                                                                                                          // now that we actually created the framebuffer and added all attachments we want to check if it is actually complete now
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Console.WriteLine("ERROR::FRAMEBUFFER:: Framebuffer is not complete!\n");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        protected override void OnLoad()
        {
            base.OnLoad();

            InitGL();

            _camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f), Size.X / (float)Size.Y);
            //WindowState = WindowState.Maximized;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // per-frame time logic
            // --------------------
            //deltaTime = currentFrame - lastFrame;
            //lastFrame = currentFrame;


            // first render pass: mirror texture.
            // bind to framebuffer and draw to color texture as we normally 
            // would, but with the view camera reversed.
            // bind to framebuffer and draw scene as we normally would to color texture 
            // ------------------------------------------------------------------------
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.Enable(EnableCap.DepthTest);

            // make sure we clear the framebuffer's content
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Use();

            _camera.Yaw += 180.0f; // rotate the camera's yaw 180 degrees around
            
            var view = _camera.GetViewMatrix();
            shader.SetMatrix4("view", view);

            //_camera.ProcessMouseMovement(0, 0, false); // call this to make sure it updates its camera vectors, note that we disable pitch constrains for this specific case (otherwise we can't reverse camera's pitch values)
            _camera.Yaw -= 180.0f; // reset it back to its original orientation
            //camera.ProcessMouseMovement(0, 0, true);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Fov), 800f/600f, 0.1f, 100.0f);

            shader.SetMatrix4("projection", projection);

            renderScene();

            // second render pass: draw as normal
            // ----------------------------------
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.SetMatrix4("view", _camera.GetViewMatrix());
            renderScene();
            
            // now draw the mirror quad with screen texture
            // --------------------------------------------
            GL.Disable(EnableCap.DepthTest); // disable depth test so screen-space quad isn't discarded due to depth test.

            screenShader.Use();
            GL.BindVertexArray(quadVAO);
            GL.BindTexture(TextureTarget.Texture2D, textureColorBuffer);   // use the color attachment texture as the texture of the quad plane
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // GL.fw: swap buffers and poll IO events (keys pressed/released, mouse moved etc.)
            // -------------------------------------------------------------------------------
            SwapBuffers();
        }

        // renders the 3D scene
        // --------------------
        void renderScene()
        {

            var model = Matrix4.Identity;
            // cubes
            GL.BindVertexArray(cubeVAO);
            cubeTexture.Use(TextureUnit.Texture0);
            model *= Matrix4.CreateTranslation(-1.0f, 0.0f, -1.0f);
            shader.SetMatrix4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            model = Matrix4.CreateTranslation(2.0f, 0.0f, 0.0f);
            shader.SetMatrix4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // floor
            GL.BindVertexArray(planeVAO);
            floorTexture.Use(TextureUnit.Texture0);
            shader.SetMatrix4("model", Matrix4.Identity);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

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
    }
}



