using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Assimp;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;
using AssimpMesh = Assimp.Mesh;
using LearnOpenTK.Common;

namespace LearnOpenTK.Common
{
    public static class Extensions
    {
        public static Vector4 ConvertAssimpVector4(this Vector3D AssimpVector)
        {
            Vector4 OpenTkVector;
            OpenTkVector.X = AssimpVector.X;
            OpenTkVector.Y = AssimpVector.Y;
            OpenTkVector.Z = AssimpVector.Z;
            OpenTkVector.W = 1;
            return OpenTkVector;
        }
        public static Matrix4 ConvertAssimpMatrix4(this Matrix4x4 AssimpMatrix)
        {
            return new Matrix4(
                    AssimpMatrix.A1, AssimpMatrix.A2, AssimpMatrix.A3, AssimpMatrix.A4,
                    AssimpMatrix.B1, AssimpMatrix.B2, AssimpMatrix.B3, AssimpMatrix.B4,
                    AssimpMatrix.C1, AssimpMatrix.C2, AssimpMatrix.C3, AssimpMatrix.C4,
                    AssimpMatrix.D1, AssimpMatrix.D2, AssimpMatrix.D3, AssimpMatrix.D4
                    );
        }
    }

    public class Model
    {
        // model data
        private List<Mesh> meshes;

        // constructor, expects a filepath to a 3D model.
        // Assimp supports many common file formats including .fbx, .obj, .blend and many others
        // Check out http://assimp.sourceforge.net/main_features_formats.html for a complete list.
        public Model(string path)
        {
            LoadModel(path);
        }

        // draws the model, and thus all its meshes
        public void Draw()
        {
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Draw();
        }

        // loads a model with supported ASSIMP extensions from file and stores the resulting meshes in the meshes list.

        private void LoadModel(string path)
        {
            //Create a new importer
            AssimpContext importer = new AssimpContext();

            //This is how we add a logging callback to print messages from the importer to the console
            //These can be information about which step is happening in the import such as:
            //"Info,  T18696: Found a matching importer for this file format: Autodesk FBX Importer."
            //Or it can give you important error information such as:
            //"Error, T18696: FBX: no material assigned to mesh, setting default material"
            //Note that in order to see the messages, you must temporarily change your project to be a console application or create a log file
            LogStream logstream = new LogStream((String msg, String userData) =>
            {
                Console.WriteLine(msg);
            });
            logstream.Attach();

            //Import the model. All configs are set. The model is imported, loaded into managed memory.
            //Then the unmanaged memory is released, and everything is reset.
            //Because we only want to render triangles in OpenGL, we are using the PostProcessSteps.Triangulate enum
            //to tell Assimp to automatically convert quads or ngons into triangles.
            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);

            // check for errors
            if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
            {
                Console.WriteLine("Unable to load model from: " + path);
                return;
            }

            //Create an empty list to be filled with meshes in the ProcessNode method
            meshes = new List<Mesh>();

            //Set the scale of the model
            float scale = 1/200.0f;
            Matrix4 scalingMatrix = Matrix4.CreateScale(scale);
            //Matrix4x4 scalingMatrix = new Matrix4x4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, 1);
            // process ASSIMP's root node recursively. We pass in the scaling matrix as the first transform
            ProcessNode(scene.RootNode, scene, scalingMatrix);

            //Once we are done with the importer, we release the resources since all the data we need
            //is now contained within our list of processed meshes
            importer.Dispose();
        }

       

        // processes a node in a recursive fashion. Processes each individual mesh located at the node and repeats this process on its children nodes (if any).
        private void ProcessNode(Node node, Scene scene, Matrix4 parentTransform)
        {
            //Multiply the transform of each node by the node of the parent, this will place the meshes in the correct relative location
            Matrix4 transform = parentTransform * node.Transform.ConvertAssimpMatrix4();

            // process each mesh located at the current node
            for (int i = 0; i < node.MeshCount; i++)
            {
                // Nodes don't actually carry any of the mesh data, but rather give an index to the corresponding Mesh
                // within the scene.Meshes List. The Nodes form the hierarchy of the model so that we can establish 
                // parent-child relationships, which are important for passing along transformations.
                AssimpMesh mesh = scene.Meshes[node.MeshIndices[i]];
                meshes.Add(ProcessMesh(mesh, transform));
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene, transform);
            }
        }

        private Mesh ProcessMesh(AssimpMesh mesh, Matrix4 transform)
        {
            // data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();

            // walk through each of the mesh's vertices
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex();

                // positions
                Vector4 position = mesh.Vertices[i].ConvertAssimpVector4();
                Vector3 transformedPosition = new Vector3(transform * position);
                vertex.Position = transformedPosition;
                // normals
                if (mesh.HasNormals)
                {
                    Vector4 normal = mesh.Normals[i].ConvertAssimpVector4();
                    Vector3 transformedNormal = new Vector3(transform * normal);
                    vertex.Normal = transformedNormal;
                }
                // texture coordinates
                if (mesh.HasTextureCoords(0)) // does the mesh contain texture coordinates?
                {
                    Vector2 vec;
                    vec.X = mesh.TextureCoordinateChannels[0][i].X;
                    vec.Y = mesh.TextureCoordinateChannels[0][i].Y;
                    vertex.TexCoords = vec;
                }
                else vertex.TexCoords = new Vector2(0.0f, 0.0f);

                vertices.Add(vertex);
            }
            // now walk through each of the mesh's faces (a face is a mesh its triangle) and retrieve the corresponding vertex indices.
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                    indices.Add(face.Indices[j]);
            }

            //Convert to array for fast memory access after inital load
            return new Mesh(vertices.ToArray(), indices.ToArray());
        }
    }
}