using System;
using OpenTK;

namespace LearnOpenGL_TK.Common
{
    public class Camera
    {
        //The projection matrix should only need to be modified very rarely
        //Therefor the matrix is set to being private write, but it is public read so we can use it for our shader
        public Matrix4 projection { get; private set; }
        //The view matrix represents the position of the camera
        //In reality it is a bit more complicated but i will go more in depth in the web version
        public Matrix4 view { get; private set; } = Matrix4.Identity;
        
        //The fov is the field of view, or the angle of the camera
        public void SetPerspective(float fov, float aspectRatio)
        {
            //We only need to create the projection matrix once
            //So we can just create it in the constructor
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspectRatio, 0.1f, 100.0f);
        }
        
        public void Move(Vector3 move)
        {
            //To move by a position just add the two positions together
            view *= Matrix4.CreateTranslation(move);
        }

        public void Rotate(Vector3 rotate)
        {
            //To rotate a quaternion we can just multiply it with another quaternion
            //We generate the quaternion from an axis angle which is an axis defined by the x, y and z components and
            //an angle defined by the w component of the vector
            view *= Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotate));
        }
    }
}