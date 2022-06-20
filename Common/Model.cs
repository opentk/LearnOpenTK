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

namespace LearnOpenTK.Common
{
    public class Model
    {
        // model data
        private List<Mesh> meshes;

        // constructor, expects a filepath to a 3D model.
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

            //This is how we add a logging callback 
            LogStream logstream = new LogStream(delegate (String msg, String userData)
            {
                Console.WriteLine(msg);
            });
            logstream.Attach();

            //Import the model. All configs are set. The model
            //is imported, loaded into managed memory. Then the unmanaged memory is released, and everything is reset.
            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);

            // check for errors
            if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
            {
                Console.WriteLine("Unable to load model from: " + path);
                return;
            }
            
            //Reset the meshes and textures
            meshes = new List<Mesh>();

            //Set the scale of the model
            float scale = 1/200.0f;
            Matrix4x4 scalingMatrix = new Matrix4x4(scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, scale, 0, 0, 0, 0, 1);

            // process ASSIMP's root node recursively. We pass in the scaling matrix as the first transform
            ProcessNode(scene.RootNode, scene, scalingMatrix);

            importer.Dispose();
        }

        // processes a node in a recursive fashion. Processes each individual mesh located at the node and repeats this process on its children nodes (if any).
        private void ProcessNode(Node node, Scene scene, Matrix4x4 parentTransform)
        {
            //Multiply the transform of each node by the node of the parent, this will place the meshes in the correct relative location
            Matrix4x4 transform = node.Transform*parentTransform;

            // process each mesh located at the current node
            for (int i = 0; i < node.MeshCount; i++)
            {
                // the node object only contains indices to index the actual objects in the scene. 
                // the scene contains all the data, node is just to keep stuff organized (like relations between nodes).
                AssimpMesh mesh = scene.Meshes[node.MeshIndices[i]];
                meshes.Add(ProcessMesh(mesh, transform));
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene, transform);
            }
        }

        private Mesh ProcessMesh(AssimpMesh mesh, Matrix4x4 transform)
        {
            // data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();

            // walk through each of the mesh's vertices
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex();
                // we declare a placeholder vector since assimp uses its own vector class that doesn't directly convert to the Vector3 class so we transfer the data to this placeholder Vector3 first.

                // positions
                var transformedVertex = transform * mesh.Vertices[i];
                Vector3 vector;
                vector.X = transformedVertex.X;
                vector.Y = transformedVertex.Y;
                vector.Z = transformedVertex.Z;
                vertex.Position = vector;
                // normals
                if (mesh.HasNormals)
                {
                    var transformedNormal = transform * mesh.Normals[i];
                    vector.X = transformedNormal.X;
                    vector.Y = transformedNormal.Y;
                    vector.Z = transformedNormal.Z;
                    vertex.Normal = vector;
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