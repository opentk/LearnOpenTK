using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using System;
using System.IO;

namespace LearnOpenTK
{
    // In this tutorial we focus on how to set up a scene with multiple lights, both of different types but also
    // with several point lights
    public class Window : GameWindow
    {
        private readonly float[] _vertices =
        {
            // Positions          Normals              Texture coords
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f
        };

        private readonly Vector3[] _cubePositions =
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(2.0f, 5.0f, -15.0f),
            new Vector3(-1.5f, -2.2f, -2.5f),
            new Vector3(-3.8f, -2.0f, -12.3f),
            new Vector3(2.4f, -0.4f, -3.5f),
            new Vector3(-1.7f, 3.0f, -7.5f),
            new Vector3(1.3f, -2.0f, -2.5f),
            new Vector3(1.5f, 2.0f, -2.5f),
            new Vector3(1.5f, 0.2f, -1.5f),
            new Vector3(-1.3f, 1.0f, -1.5f)
        };

        // We need the point lights' positions to draw the lamps and to get light the materials properly
        private readonly Vector3[] _pointLightPositions =
        {
            new Vector3(0.7f, 0.2f, 2.0f),
            new Vector3(2.3f, -3.3f, -4.0f),
            new Vector3(-4.0f, 2.0f, -12.0f),
            new Vector3(0.0f, 0.0f, -3.0f)
        };

        private int _vertexBufferObject;

        private int _vaoModel;

        private int _vaoLamp;

        private int _lampShader;

        private int _lightingShader;

        private int _diffuseMap;

        private int _specularMap;

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

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _lightingShader = CompileProgram(File.ReadAllText("Shaders/shader.vert"), File.ReadAllText("Shaders/lighting.frag"));
            _lampShader = CompileProgram(File.ReadAllText("Shaders/shader.vert"), File.ReadAllText("Shaders/shader.frag"));

            {
                _vaoModel = GL.GenVertexArray();
                GL.BindVertexArray(_vaoModel);

                var positionLocation = 0; // aPos
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

                var normalLocation = 1; // aNormal
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

                var texCoordLocation = 2; // aTexCoord
                GL.EnableVertexAttribArray(texCoordLocation);
                GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            }

            {
                _vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(_vaoLamp);

                var positionLocation = 0; // aPos
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            }

            _diffuseMap = LoadTextureFromFile("Resources/container2.png");
            _specularMap = LoadTextureFromFile("Resources/container2_specular.png");

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vaoModel);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _diffuseMap);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _specularMap);
            GL.UseProgram(_lightingShader);

            Matrix4 projection = _camera.GetProjectionMatrix();
            Matrix4 view = _camera.GetViewMatrix();

            GL.UniformMatrix4(GL.GetUniformLocation(_lightingShader, "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(_lightingShader, "projection"), true, ref projection);

            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "viewPos"), _camera.Position);

            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "material.diffuse"), 0);
            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "material.specular"), 1);
            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "material.shininess"), 32.0f);

            /*
               Here we set all the uniforms for the 5/6 types of lights we have. We have to set them manually and index
               the proper PointLight struct in the array to set each uniform variable. This can be done more code-friendly
               by defining light types as classes and set their values in there, or by using a more efficient uniform approach
               by using 'Uniform buffer objects', but that is something we'll discuss in the 'Advanced GLSL' tutorial.
            */
            // Directional light
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "dirLight.direction"), new Vector3(-0.2f, -1.0f, -0.3f));
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "dirLight.ambient"), new Vector3(0.05f, 0.05f, 0.05f));
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "dirLight.diffuse"), new Vector3(0.4f, 0.4f, 0.4f));
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "dirLight.specular"), new Vector3(0.5f, 0.5f, 0.5f));

            // Point lights
            for (int i = 0; i < _pointLightPositions.Length; i++)
            {
                GL.Uniform3(GL.GetUniformLocation(_lightingShader, $"pointLights[{i}].position"), _pointLightPositions[i]);
                GL.Uniform3(GL.GetUniformLocation(_lightingShader, $"pointLights[{i}].ambient"), new Vector3(0.05f, 0.05f, 0.05f));
                GL.Uniform3(GL.GetUniformLocation(_lightingShader, $"pointLights[{i}].diffuse"), new Vector3(0.8f, 0.8f, 0.8f));
                GL.Uniform3(GL.GetUniformLocation(_lightingShader, $"pointLights[{i}].specular"), new Vector3(1.0f, 1.0f, 1.0f));
                GL.Uniform1(GL.GetUniformLocation(_lightingShader, $"pointLights[{i}].constant"), 1.0f);
                GL.Uniform1(GL.GetUniformLocation(_lightingShader, $"pointLights[{i}].linear"), 0.09f);
                GL.Uniform1(GL.GetUniformLocation(_lightingShader, $"pointLights[{i}].quadratic"), 0.032f);
            }

            // Spot light
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "spotLight.position"), _camera.Position);
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "spotLight.direction"), _camera.Front);
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "spotLight.ambient"), new Vector3(0.0f, 0.0f, 0.0f));
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "spotLight.diffuse"), new Vector3(1.0f, 1.0f, 1.0f));
            GL.Uniform3(GL.GetUniformLocation(_lightingShader, "spotLight.specular"), new Vector3(1.0f, 1.0f, 1.0f));
            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "spotLight.constant"), 1.0f);
            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "spotLight.linear"), 0.09f);
            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "spotLight.quadratic"), 0.032f);
            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "spotLight.cutOff"), MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            GL.Uniform1(GL.GetUniformLocation(_lightingShader, "spotLight.outerCutOff"), MathF.Cos(MathHelper.DegreesToRadians(17.5f)));

            for (int i = 0; i < _cubePositions.Length; i++)
            {
                Matrix4 model = Matrix4.CreateTranslation(_cubePositions[i]);
                float angle = 20.0f * i;
                model = model * Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), angle);
                GL.UniformMatrix4(GL.GetUniformLocation(_lightingShader, "model"), true, ref model);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }

            GL.BindVertexArray(_vaoLamp);

            GL.UseProgram(_lampShader);

            GL.UniformMatrix4(GL.GetUniformLocation(_lampShader, "view"), true, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(_lampShader, "projection"), true, ref projection);
            // We use a loop to draw all the lights at the proper position
            for (int i = 0; i < _pointLightPositions.Length; i++)
            {
                Matrix4 lampMatrix = Matrix4.CreateScale(0.2f) * Matrix4.CreateTranslation(_pointLightPositions[i]);

                GL.UniformMatrix4(GL.GetUniformLocation(_lampShader, "model"), true, ref lampMatrix);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
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

            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);

            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        private static int CompileProgram(string vertexSource, string fragmentSource)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

            int handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);

            GL.LinkProgram(handle);

            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                string infoLog = GL.GetProgramInfoLog(handle);
                throw new Exception($"Error occurred whilst linking Program({handle}):\n{infoLog}");
            }

            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            return handle;
        }

        private static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, source);

            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}):\n{infoLog}");
            }

            return shader;
        }

        public static int LoadTextureFromFile(string path)
        {
            int handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            StbImage.stbi_set_flip_vertically_on_load(1);

            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return handle;
        }
    }
}
