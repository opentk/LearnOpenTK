using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System.IO;
using System;

namespace LearnOpenTK
{
    // In this tutorial we set up some basic lighting and look at how the phong model works
    // For more insight into how it all works look at the web version. If you are just here for the source,
    // most of the changes are in the shaders, specifically most of the changes are in the fragment shader as this is
    // where the lighting calculations happens.
    public class Window : GameWindow
    {
        const int SHADOW_WIDTH = 1024, SHADOW_HEIGHT = 1024;
        // settings
        float[] quadVertices = {
            // positions        // texture Coords
            -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        };


        // set up vertex data (and buffer(s)) and configure vertex attributes
        // ------------------------------------------------------------------
        float[] planeVertices = {
        // positions            // normals         // texcoords
         25.0f, -0.5f,  25.0f,  0.0f, 1.0f, 0.0f,  25.0f,  0.0f,
        -25.0f, -0.5f,  25.0f,  0.0f, 1.0f, 0.0f,   0.0f,  0.0f,
        -25.0f, -0.5f, -25.0f,  0.0f, 1.0f, 0.0f,   0.0f, 25.0f,

         25.0f, -0.5f,  25.0f,  0.0f, 1.0f, 0.0f,  25.0f,  0.0f,
        -25.0f, -0.5f, -25.0f,  0.0f, 1.0f, 0.0f,   0.0f, 25.0f,
         25.0f, -0.5f, -25.0f,  0.0f, 1.0f, 0.0f,  25.0f, 25.0f
    };
        // renderCube() renders a 1x1 3D cube in NDC.
        // -------------------------------------------------
        int cubeVAO = 0;
        int cubeVBO = 0;
        private Texture woodTexture;
        private int depthMapFBO;
        private int depthMap;
        // timing
        float deltaTime = 0.0f;
        float lastFrame = 0.0f;
        Matrix4 lightSpaceMatrix;
        float prevMouseWheel;

        // renderQuad() renders a 1x1 XY quad in NDC
        // -----------------------------------------
        int quadVAO = 0;
        int quadVBO;

        // lighting info
        // -------------
        private readonly Vector3 lightPos = new Vector3(-2.0f, 4.0f, -1.0f);

        private int _vertexBufferObject;

        private int _vaoModel;

        private int _vaoLamp;

        private Shader shader, simpleDepthShader, debugDepthQuad;
        private int planeVAO;
        private int planeVBO;
        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected void InitGL()
        {
            // configure global opengl state
            // -----------------------------
            GL.Enable(EnableCap.DepthTest);

            // build and compile shaders
            // -------------------------
            shader = new Shader("Shaders/3.1.3.shadow_mapping.vs", "Shaders/3.1.3.shadow_mapping.fs");

            simpleDepthShader = new Shader("Shaders/3.1.3.shadow_mapping_depth.vs", "Shaders/3.1.3.shadow_mapping_depth.fs");
            debugDepthQuad = new Shader("Shaders/3.1.3.debug_quad.vs", "Shaders/3.1.3.debug_quad_depth.fs");

            // plane VAO
            planeVAO = GL.GenVertexArray();
            planeVBO = GL.GenBuffer();
            GL.BindVertexArray(planeVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, planeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, planeVertices.Length * sizeof(float), planeVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (3 * sizeof(float)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (6 * sizeof(float)));
            GL.BindVertexArray(0);

            // load textures
            // -------------
            woodTexture = Texture.LoadFromFile("Resources/wood.png");
            woodTexture.Use(TextureUnit.Texture0);

            // configure depth map FBO
            // -----------------------
            depthMapFBO = GL.GenFramebuffer();
            // create depth texture
            depthMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_WIDTH, SHADOW_HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            var borderColor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            // attach depth texture as FBO's depth buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


            // shader configuration
            // --------------------
            shader.Use();
            shader.SetInt("diffuseTexture", 0);
            shader.SetInt("shadowMap", 1);
            debugDepthQuad.Use();
            debugDepthQuad.SetInt("depthMap", 0);

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


            // change light position over time
            //lightPos.x = sin(glfwGetTime()) * 3.0f;
            //lightPos.z = cos(glfwGetTime()) * 2.0f;
            //lightPos.y = 5.0 + cos(glfwGetTime()) * 1.0f;

            // render
            // ------
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // 1. render depth of scene to texture (from light's perspective)
            // --------------------------------------------------------------
            Matrix4 lightProjection, lightView;
            float near_plane = 1.0f, far_plane = 7.5f;
            //lightProjection = glm::perspective(glm::radians(45.0f), (GLfloat)SHADOW_WIDTH / (GLfloat)SHADOW_HEIGHT, near_plane, far_plane); // note that if you use a perspective projection matrix you'll have to change the light position as the current light position isn't enough to reflect the whole scene
            lightProjection = Matrix4.CreateOrthographicOffCenter(-10.0f, 10.0f, -10.0f, 10.0f, near_plane, far_plane);
            lightView = Matrix4.LookAt(lightPos, new Vector3(0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            //lightSpaceMatrix = lightProjection * lightView;
            lightSpaceMatrix = lightView * lightProjection;
            // render scene from light's point of view
            simpleDepthShader.Use();
            simpleDepthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);

            GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            //glActiveTexture(GL_TEXTURE0);
            //glBindTexture(GL_TEXTURE_2D, woodTexture);
            renderScene(simpleDepthShader);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // reset viewport
            GL.Viewport(0, 0, Size.X, Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // 2. render scene as normal using the generated depth/shadow map  
            // --------------------------------------------------------------
            shader.Use();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Fov), Size.X / (float)Size.Y, 0.1f, 100.0f);
            Matrix4 view = _camera.GetViewMatrix();
            shader.SetMatrix4("projection", projection);
            shader.SetMatrix4("view", view);
            // set light uniforms
            shader.SetVector3("viewPos", _camera.Position);
            shader.SetVector3("lightPos", lightPos);
            shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            woodTexture.Use(TextureUnit.Texture0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depthMap);
            renderScene(shader);

            // render Depth map to quad for visual debugging
            // ---------------------------------------------
            debugDepthQuad.Use();
            //debugDepthQuad.SetFloat("near_plane", near_plane);
            //debugDepthQuad.SetFloat("far_plane", far_plane);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, depthMap);
            //renderQuad();

            // GL.fw: swap buffers and poll IO events (keys pressed/released, mouse moved etc.)
            // -------------------------------------------------------------------------------
            SwapBuffers();
        }


        void renderCube()
        {
            // initialize (if necessary)
            if (cubeVAO == 0)
            {
                float[] vertices = {
            // back face
            -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f, // bottom-left
             1.0f,  1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f, // top-right
             1.0f, -1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f, // bottom-right         
             1.0f,  1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f, // top-right
            -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f, // bottom-left
            -1.0f,  1.0f, -1.0f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f, // top-left
            // front face
            -1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f, // bottom-left
             1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f, // bottom-right
             1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f, // top-right
             1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f, // top-right
            -1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f, // top-left
            -1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f, // bottom-left
            // left face
            -1.0f,  1.0f,  1.0f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-right
            -1.0f,  1.0f, -1.0f, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f, // top-left
            -1.0f, -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-left
            -1.0f, -1.0f, -1.0f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-left
            -1.0f, -1.0f,  1.0f, -1.0f,  0.0f,  0.0f, 0.0f, 0.0f, // bottom-right
            -1.0f,  1.0f,  1.0f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-right
            // right face
             1.0f,  1.0f,  1.0f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-left
             1.0f, -1.0f, -1.0f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-right
             1.0f,  1.0f, -1.0f,  1.0f,  0.0f,  0.0f, 1.0f, 1.0f, // top-right         
             1.0f, -1.0f, -1.0f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f, // bottom-right
             1.0f,  1.0f,  1.0f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f, // top-left
             1.0f, -1.0f,  1.0f,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f, // bottom-left     
            // bottom face
            -1.0f, -1.0f, -1.0f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f, // top-right
             1.0f, -1.0f, -1.0f,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f, // top-left
             1.0f, -1.0f,  1.0f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f, // bottom-left
             1.0f, -1.0f,  1.0f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f, // bottom-left
            -1.0f, -1.0f,  1.0f,  0.0f, -1.0f,  0.0f, 0.0f, 0.0f, // bottom-right
            -1.0f, -1.0f, -1.0f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f, // top-right
            // top face
            -1.0f,  1.0f, -1.0f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f, // top-left
             1.0f,  1.0f , 1.0f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f, // bottom-right
             1.0f,  1.0f, -1.0f,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f, // top-right     
             1.0f,  1.0f,  1.0f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f, // bottom-right
            -1.0f,  1.0f, -1.0f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f, // top-left
            -1.0f,  1.0f,  1.0f,  0.0f,  1.0f,  0.0f, 0.0f, 0.0f  // bottom-left        
        };
                cubeVAO = GL.GenVertexArray();
                cubeVBO = GL.GenBuffer();
                // fill buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
                // link vertex attributes
                GL.BindVertexArray(cubeVAO);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (3 * sizeof(float)));
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (6 * sizeof(float)));
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
            // render Cube
            GL.BindVertexArray(cubeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);
        }

        void renderQuad()
        {
            if (quadVAO == 0)
            {
                // setup plane VAO
                quadVAO = GL.GenVertexArray();
                quadVBO = GL.GenBuffer();
                GL.BindVertexArray(quadVAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (3 * sizeof(float)));
                GL.EnableVertexAttribArray(2);
            }
            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.BindVertexArray(0);
        }
        // renders the 3D scene
        // --------------------
        void renderScene(Shader shader)
        {
            // floor
            Matrix4 model = Matrix4.Identity;
            shader.SetMatrix4("model", model);
            GL.BindVertexArray(planeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            // cubes
            model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(new Vector3(0.5f));
            model = model * Matrix4.CreateTranslation(0.0f, 1.5f, 0.0f);
            shader.SetMatrix4("model", model);
            renderCube();

            model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(new Vector3(0.5f));
            model = model * Matrix4.CreateTranslation(2.0f, 0.0f, 1.0f);
            shader.SetMatrix4("model", model);
            renderCube();

            model = Matrix4.Identity;
            model = model * Matrix4.CreateFromAxisAngle(new Vector3(1f, 0, 1f), MathHelper.DegreesToRadians(60.0f)); 
            model = model * Matrix4.CreateScale(new Vector3(0.25f));
            model = model * Matrix4.CreateTranslation(-1.0f, 0.0f, 2.0f);
            shader.SetMatrix4("model", model);
            renderCube();
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

