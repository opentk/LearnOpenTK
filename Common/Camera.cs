using System;
using OpenTK;

namespace LearnOpenGL_TK.Common
{
    //The camera class can be static as most games will only need one camera
    public static class Camera
    {
        //We need a projection and view matrix for that defines the camera just like in the last tutorial
        //For now we split up the position and the rotation of the view matrix before we actually need the view matrix
        //This way we can easily modify each of the components before putting them together to form the view matrix
        static Vector3 position = Vector3.Zero;
        static Vector3 rotation = Vector3.Zero;
        //The projection matrix should only need to be modified very rarely
        //Therefor the matrix is set to being private write, but it is public read so we can use it for our shader
        public static Matrix4 projection { get; private set; }

        //The fov is the field of view, or the angle of the camera
        public static void Init(float fov, float aspectRatio)
        {
            //We only need to create the projection matrix once
            //So we can just create it in the constructor
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspectRatio, 0.1f, 100.0f);
        }
        
        public static void Move(Vector3 move)
        {
            //To move by a position just add the two positions together
            position += move;
        }

        public static void Rotate(Vector3 rotate)
        {
            //To rotate a quaternion we can just multiply it with another quaternion
            //We generate the quaternion from an axis angle which is an axis defined by the x, y and z components and
            //an angle defined by the w component of the vector
            rotation += rotate;
        }
        
        public static Matrix4 GetView()
        {
            //We generate the matrix from our position and rotation
            //After that we return the calculated view matrix
            return Matrix4.Identity * Matrix4.CreateTranslation(position) * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
        }
    }
}