using System;
using System.Windows.Forms;
using LearnOpenGL_TK.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTK.GLControl
{
    public partial class MainForm : Form
    {
        private readonly Random _rand = new Random();
        
        private readonly float[] _data =
        {
            // Vertices          Colors
            -0.5f, -0.5f, -0.5f, 1f, 0f, 0f,
            +0.5f, -0.5f, -0.5f, 0f, 1f, 0f,
            +0.5f, -0.5f, +0.5f, 0f, 0f, 1f,
            -0.5f, -0.5f, +0.5f, 0f, 1f, 1f,
            +0.0f, +0.5f, +0.0f, 1f, 1f, 0f
        };

        private readonly uint[] _indices =
        {
            // Base
            0, 2, 1,
            0, 2, 3,
            
            // Sides
            4, 0, 1,
            4, 1, 2,
            4, 2, 3,
            4, 3, 0
        };

        private bool _loaded;
        
        private int _vertexBufferObject;
        private int _elementBufferObject;
        private int _vertexArrayObject;

        private Shader _shader;

        private float _rotationX;
        private float _rotationY;
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.94f, 0.94f, 0.94f, 1f);
            GL.Enable(EnableCap.DepthTest);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _data.Length * sizeof(float), _data, BufferUsageHint.DynamicDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            UpdateViewMatrix();
            _loaded = true;
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (!_loaded)
                return;
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _shader.Use();
            
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            
            glControl.SwapBuffers();
        }

        private void UpdateViewMatrix()
        {
            if (!_loaded)
                return;
            
            _shader.Use();

            var view = Matrix4.CreateRotationX(_rotationX);
            view *= Matrix4.CreateRotationY(_rotationY);
            _shader.SetMatrix4("view", view);
        }

        private void TrackBarX_Scroll(object sender, EventArgs e)
        {
            _rotationX = MathHelper.DegreesToRadians(trackBarX.Value);
            UpdateViewMatrix();
            glControl.Invalidate();
        }

        private void TrackBarY_Scroll(object sender, EventArgs e)
        {
            _rotationY = MathHelper.DegreesToRadians(trackBarY.Value);
            UpdateViewMatrix();
            glControl.Invalidate();
        }

        private void ButtonRandomize_Click(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            for (var i = 3; i < _data.Length; i += 6)
            {
                _data[i] = _rand.Next(255) / 255f;
                _data[i + 1] = _rand.Next(255) / 255f;
                _data[i + 2] = _rand.Next(255) / 255f;
            }
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _data.Length * sizeof(float), _data, BufferUsageHint.DynamicDraw);
            
            glControl.Invalidate();
        }

        private void ButtonChangeColor_Click(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            colorDialog.ShowDialog();
            GL.ClearColor(colorDialog.Color);
            glControl.Invalidate();
        }
    }
}
