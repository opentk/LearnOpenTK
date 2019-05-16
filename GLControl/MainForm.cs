using System;
using System.Windows.Forms;
using LearnOpenGL_TK.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTK.GLControl
{
    public partial class MainForm : Form
    {
        private static readonly Random Random = new Random();
        
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

        // We store the loaded state in a boolean to prevent GL specific instructions to be called by WinForms events.
        private bool _loaded;
        
        private int _vertexBufferObject;
        private int _elementBufferObject;
        private int _vertexArrayObject;

        private Shader _shader;

        // We use two TrackBars to rotate the object, those fields store the rotation values in radians
        private float _rotationX;
        private float _rotationY;
        
        public MainForm()
        {
            InitializeComponent();
        }

        // Same as GameWindow.Load
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
            
            // Because there's now 6 floats (vertex + color data) between the start of the first vertex and the start of the second
            // we specify a stride of 6 * sizeof(float) 
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            UpdateViewMatrix();
            _loaded = true;
        }

        // Calls when the Control changes size.
        // As the object is firstly created with an empty constructor and then all the properties are changed
        // it's likely that this event gets fired once or twice before the Load event gets fired.
        // Is important to include the loaded check. 
        private void GLControl_Resize(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            
            // We invalidate the control to apply the viewport changes.
            // Invalidating the control forces the Paint event to be fired.
            // Without invalidating no changes would be visible.
            glControl.Invalidate();
        }

        // Same as GameWindow.RenderFrame
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

            // We update the view matrix with the new rotation data
            var view = Matrix4.CreateRotationX(_rotationX);
            view *= Matrix4.CreateRotationY(_rotationY);
            _shader.SetMatrix4("view", view);
        }

        private void TrackBarX_Scroll(object sender, EventArgs e)
        {
            // Rotates around the X axis
            _rotationX = MathHelper.DegreesToRadians(trackBarX.Value);
            UpdateViewMatrix();
            
            // We invalidate the control to apply the rotation changes
            glControl.Invalidate();
        }

        private void TrackBarY_Scroll(object sender, EventArgs e)
        {
            // Rotates around the Y axis
            _rotationY = MathHelper.DegreesToRadians(trackBarY.Value);
            UpdateViewMatrix();
            
            // We invalidate the control to apply the rotation changes
            glControl.Invalidate();
        }

        // Randomizes the colors of the object we are drawing
        private void ButtonRandomize_Click(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            for (var i = 3; i < _data.Length; i += 6)
            {
                _data[i] = Random.Next(256) / 255f;
                _data[i + 1] = Random.Next(256) / 255f;
                _data[i + 2] = Random.Next(256) / 255f;
            }
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _data.Length * sizeof(float), _data, BufferUsageHint.DynamicDraw);
            
            // We once again invalidate the control.
            glControl.Invalidate();
        }

        // Changes the GL background color
        private void ButtonChangeColor_Click(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            // Shows a Dialog in which you can choose a color.
            colorDialog.ShowDialog();
            GL.ClearColor(colorDialog.Color);
            
            // We invalidate the control to apply the background changes.
            glControl.Invalidate();
        }
    }
}
