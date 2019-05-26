using System;
using OpenTK;

namespace LearnOpenTK.Common
{
    // This is the camera class as it could be set up after the tutorials on the website
    // It is important to note there are a few ways you could have set up this camera, for example
    // you could have also managed the player input inside the camera class, and a lot of the properties could have
    // been made into functions.
    
    // TL;DR: This is just one of many ways in which we could have set up the camera
    // Check out the web version if you don't know why we are doing a specific thing or want to know more about the code
    public class Camera
    {
        // We need quite the amount of vectors to define the camera
        // The position is simply the position of the camera
        // the other vectors are directions pointing outwards from the camera to define how it is rotated
        public Vector3 Position;
        private Vector3 _front = -Vector3.UnitZ;
        public Vector3 Front => _front;
        private Vector3 _up = Vector3.UnitY;
        public Vector3 Up => _up;
        private Vector3 _right = Vector3.UnitX;
        public Vector3 Right => _right;

        // Pitch is the rotation around the x axis, and it is explained more specifically in the tutorial how we can use this
        private float _pitch;
        public float Pitch
        {
            get => _pitch;
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation. If you want to read more about this you can try researching a topic called gimbal lock
                if (value > 89.0f)
                {
                    _pitch = 89.0f;
                }
                else if (value <= -89.0f)
                {
                    _pitch = -89.0f;
                }
                else
                {
                    _pitch = value;
                }
                UpdateVertices();
            }
        }
        // Yaw is the rotation around the y axis, and it is explained more specifically in the tutorial how we can use this
        private float _yaw;
        public float Yaw
        {
            get => _yaw;
            set
            {
                _yaw = value;
                UpdateVertices();
            }
        }

        // The speed and the sensitivity are the speeds of respectively,
        // the movement of the camera and the rotation of the camera (mouse sensitivity)
        public float Speed = 1.5f;
        public float Sensitivity = 0.2f;

        // The fov (field of view) is how wide the camera is viewing, this has been discussed more in depth in a
        // previous tutorial, but in this tutorial you have also learned how we can use this to simulate a zoom feature.
        private float _fov = 45.0f;
        public float Fov
        {
            get => _fov;
            set
            {
                if (value >= 45.0f)
                {
                    _fov = 45.0f;
                }
                else if (value <= 1.0f)
                {
                    _fov = 1.0f;
                }
                else
                {
                    _fov = value;
                }
            }
        }
        public float AspectRatio { get; set; } // This is simply the aspect ratio of the viewport, used for the projection matrix
        
        // In the instructor we take in a position
        // We also set the yaw to -90, the code would work without this, but you would be started rotated 90 degrees away from the rectangle
        public Camera(Vector3 position)
        {
            Position = position;
            _yaw = -90;
        }
        
        // Get the view matrix using the amazing LookAt function described more in depth on the web version of the tutorial
        public Matrix4 GetViewMatrix() => 
            Matrix4.LookAt(Position, Position + Front, Up);
        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix() => 
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, 0.01f, 100f);

        // This function is going to update the direction vertices using some of the math learned in the web tutorials
        private void UpdateVertices()
        {
            // First the front matrix is calculated using some basic trigonometry
            _front.X = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(Yaw));
            _front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
            _front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(Yaw));
            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results
            _front = Vector3.Normalize(_front);
            
            // Calculate both the right and the up vector using the cross product
            // Note that we are calculating the right from the global up, this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera
            _right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}