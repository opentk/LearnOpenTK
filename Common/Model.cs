using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;
using AssimpMesh = Assimp.Mesh;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace LearnOpenTK.Common
{
    public static class Extensions
    {
        public static Vector3 ConvertAssimpVector3(this Vector3D AssimpVector)
        {
            // Reinterpret the assimp vector into an OpenTK vector.
            return Unsafe.As<Vector3D, Vector3>(ref AssimpVector);
        }

        public static Matrix4 ConvertAssimpMatrix4(this Matrix4x4 AssimpMatrix)
        {
            // Take the column-major assimp matrix and convert it to a row-major OpenTK matrix.
            return Matrix4.Transpose(Unsafe.As<Matrix4x4, Matrix4>(ref AssimpMatrix));
        }
    }

    public class Model
    {
        // Model data
        private List<Mesh> meshes;

        // Constructor, expects a filepath to a 3D model.
        // Assimp supports many common file formats including .fbx, .obj, .blend and many others
        // Check out http://assimp.sourceforge.net/main_features_formats.html for a complete list.
        public Model(string path)
        {
            // Create a new importer
            AssimpContext importer = new AssimpContext();

            // We can define a logging callback function that receives messages during the ImportFile method and print them to the debug console.
            // These give information about which step is happening in the import such as:
            //      "Info, T18696: Found a matching importer for this file format: Autodesk FBX Importer."
            // or it can give you important error information such as:
            //      "Error, T18696: FBX: no material assigned to mesh, setting default material"
            LogStream logstream = new LogStream((String msg, String userData) =>
            {
                Debug.WriteLine(msg);
            });
            logstream.Attach();

            // Import the model into managed memory with any PostProcessPreset or PostProcessSteps we desire.
            // Because we only want to render triangles in OpenGL, we are using the PostProcessSteps.Triangulate enum
            // to tell Assimp to automatically convert quads or ngons into triangles.
            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);

            // Check for errors
            if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
            {
                Console.WriteLine("Unable to load model from: " + path);
                return;
            }

            // Create an empty list to be filled with meshes in the ProcessNode method
            meshes = new List<Mesh>();

            // Set the scale of the model. Ideally, the creator of the model would set the initial scale
            // and then any further scaling of each instance would take place in the "model" matrix that is passed to the shader.
            float scale = 1/200.0f;
            Matrix4 scalingMatrix = Matrix4.CreateScale(scale);

            // Process ASSIMP's root node recursively. We pass in the scaling matrix as the first transform
            ProcessNode(scene.RootNode, scene, scalingMatrix);

            // Once we are done with the importer, we release the resources since all the data we need
            // is now contained within our list of processed meshes
            importer.Dispose();
        }

        // Draws the model, and thus all its meshes
        public void Draw()
        {
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Draw();
        }
        
        // Processes a node in a recursive fashion. Processes each individual mesh located at the node and repeats this process on its children nodes (if any).
        private void ProcessNode(Node node, Scene scene, Matrix4 parentTransform)
        {
            // Multiply the transform of each node by the node of the parent, this will place the meshes in the correct relative location
            Matrix4 transform = node.Transform.ConvertAssimpMatrix4() * parentTransform;

            // Process each mesh located at the current node
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
            // Data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();

            // Calculate the inverse matrix once, so we don't need to do it for every vertex.
            // This matrix in combination with Vector3.TransformNormalInverse is used to transform normal vectors.
            Matrix4 inverseTransform = Matrix4.Invert(transform);
            
            // Walk through each of the mesh's vertices
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex();

                // Positions
                Vector3 position = mesh.Vertices[i].ConvertAssimpVector3();
                Vector3 transformedPosition = Vector3.TransformPosition(position, transform);
                vertex.Position = transformedPosition;
                
                // Normals
                if (mesh.HasNormals)
                {
                    Vector3 normal = mesh.Normals[i].ConvertAssimpVector3();
                    Vector3 transformedNormal = Vector3.TransformNormalInverse(normal, inverseTransform);
                    vertex.Normal = transformedNormal;
                }

                // Texture coordinates
                if (mesh.HasTextureCoords(0)) // Does the mesh contain texture coordinates?
                {
                    Vector2 vec;
                    vec.X = mesh.TextureCoordinateChannels[0][i].X;
                    vec.Y = mesh.TextureCoordinateChannels[0][i].Y;
                    vertex.TexCoords = vec;
                }
                else vertex.TexCoords = new Vector2(0.0f, 0.0f);

                vertices.Add(vertex);
            }

            // Now walk through each of the mesh's faces (a face is a group of vertices that form a triangle, quadrilateral, or ngon) and retrieve the corresponding vertex indices.
            // All of the faces should be triangles since we used PostProcessSteps.Triangulate during the Assimp import
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                    indices.Add(face.Indices[j]);
            }

            // If we were targeting .net 5+ we could use
            //      return new Mesh(CollectionsMarshal.AsSpan(vertices), CollectionsMarshal.AsSpan(indices));
            // to avoid making a copy of all the vertex data.
            return new Mesh(vertices.ToArray(), indices.ToArray());
        }
    }
}