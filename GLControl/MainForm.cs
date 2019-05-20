using System;
using System.Windows.Forms;
using LearnOpenTK.Common;
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

        // We store the loaded state in a boolean to prevent GL-specific instructions
        // being called before the GL context is created
        private bool _loaded;
        
        private int _vertexBufferObject;
        private int _elementBufferObject;
        private int _vertexArrayObject;

        private Shader _shader;

        // We use two TrackBars to rotate the object.
        // These two fields store the rotation in radians along the X and Y axis.
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
            
            // Because there's now 6 floats (vertex + color data) between the start of the first vertex
            // and the start of the second we specify a stride of 6 * sizeof(float)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            
            // Here we specify an offset of 3 * sizeof(float) as we want to point to the first color
            // which is after the first coordinate (x, y, z: 3 floats)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            UpdateViewMatrix();
            _loaded = true;
            
            // We have to force a draw or the window will stay blank until we invalidate the control somewhere else.
            glControl.Invalidate();
        }

        // Gets invoked when the Control changes size.
        private void GLControl_Resize(object sender, EventArgs e)
        {
            // As the object is firstly created with an empty constructor and all the properties are set afterwards
            // it's likely that this event gets fired once or twice before OpenGL initialization (Load event)
            // hence it's important to include the loaded check.
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
            
            // Clears the control using the background color
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _shader.Use();
            
            // Draws the object
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            
            // Swaps front frame with back frame
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
            // Gets the rotation around the X axis in radians
            _rotationX = MathHelper.DegreesToRadians(trackBarX.Value);
            
            // We send the updated view matrix to the graphics card
            UpdateViewMatrix();
            
            // and then we invalidate the form to redraw our object with our rotation
            glControl.Invalidate();
        }

        private void TrackBarY_Scroll(object sender, EventArgs e)
        {
            // Gets the rotation around the Y axis in radians
            _rotationY = MathHelper.DegreesToRadians(trackBarY.Value);
            
            // We send the updated view matrix matrix to the graphics card
            UpdateViewMatrix();
            
            // and then we invalidate the form to redraw our object with our rotation
            glControl.Invalidate();
        }

        // Invoked when the "Randomize Colors" button is clicked
        private void ButtonRandomize_Click(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            // Randomizes the colors of the object we are drawing
            for (var i = 3; i < _data.Length; i += 6)
            {
                _data[i] = Random.Next(256) / 255f;
                _data[i + 1] = Random.Next(256) / 255f;
                _data[i + 2] = Random.Next(256) / 255f;
            }
            
            // We update the data we sent to the graphics card with our new colors
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _data.Length * sizeof(float), _data, BufferUsageHint.DynamicDraw);
            
            // We once again invalidate the control.
            glControl.Invalidate();
        }

        // Invoked when the "Change Background" button is clicked
        private void ButtonChangeColor_Click(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            
            // Shows a Dialog in which you can choose a color.
            colorDialog.ShowDialog();
            
            // Changes the control background color
            GL.ClearColor(colorDialog.Color);
            
            // We invalidate the control to apply the background changes.
            glControl.Invalidate();
        }
    }
}
