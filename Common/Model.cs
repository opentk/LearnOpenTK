using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using Assimp;
using AssimpMesh = Assimp.Mesh;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Matrix4x4 = Assimp.Matrix4x4;

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
        string directory;
        private List<Texture> textures_loaded;  // stores all the textures loaded so far, optimization to make sure textures aren't loaded more than once.


        // Constructor, expects a filepath to a 3D model.
        public Model(string path)
        {
            // Initialize loaded textures
            textures_loaded = new List<Texture>();

            loadModel(path);
        }

        // Assimp supports many common file formats including .fbx, .obj, .blend and many others
        // Check out http://assimp.sourceforge.net/main_features_formats.html for a complete list.
        public void loadModel(string path)
        {
            // Create a new importer
            AssimpContext importer = new AssimpContext();

            // We can define a logging callback function that receives messages during the ImportFile method and print them to the debug console.
            // These give information about which step is happening in the import such as:
            //      "Info, T18696: Found a matching importer for this file format: Autodesk FBX Importer."
            // or it can give you important error information such as:
            //      "Error, T18696: FBX: no material assigned to mesh, setting default material"
            LogStream logstream = new LogStream((string msg, string userData) =>
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


            // retrieve the directory path of the filepath
            directory = path.Substring(0, path.LastIndexOf('/'));
            
            // Process ASSIMP's root node recursively. We pass in the scaling matrix as the first transform
            ProcessNode(scene.RootNode, scene);

            // Once we are done with the importer, we release the resources since all the data we need
            // is now contained within our list of processed meshes
            importer.Dispose();
        }


        public void Draw(Shader shader)
        {
            foreach (Mesh mesh in meshes)
            {
                mesh.Draw(shader);
            }
        }

        // Processes a node in a recursive fashion. Processes each individual mesh located at the node and repeats this process on its children nodes (if any).
        private void ProcessNode(Node node, Scene scene)
        {
            // Process each mesh located at the current node
            for (int i = 0; i < node.MeshCount; i++)
            {
                // Nodes don't actually carry any of the mesh data, but rather give an index to the corresponding Mesh
                // within the scene.Meshes List. The Nodes form the hierarchy of the model so that we can establish 
                // parent-child relationships, which are important for passing along transformations.
                AssimpMesh mesh = scene.Meshes[node.MeshIndices[i]];
                meshes.Add(ProcessMesh(mesh, scene));
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                ProcessNode(node.Children[i], scene);
            }
        }

        private Mesh ProcessMesh(AssimpMesh mesh, Scene scene)
        {
            // Data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();
            List<Texture> textures = new List<Texture>();


            // Walk through each of the mesh's vertices
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex();

                // Positions
                vertex.Position = mesh.Vertices[i].ConvertAssimpVector3();  //transformedPosition;

                // Normals
                if (mesh.HasNormals)
                {
                    vertex.Normal = mesh.Normals[i].ConvertAssimpVector3(); //transformedNormal;
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

            // process materials
            Material material = scene.Materials[mesh.MaterialIndex];

            // we assume a convention for sampler names in the shaders. Each diffuse texture should be named
            // as 'texture_diffuseN' where N is a sequential number ranging from 1 to MAX_SAMPLER_NUMBER. 
            // Same applies to other texture as the following list summarizes:
            // diffuse: texture_diffuseN
            // specular: texture_specularN
            // normal: texture_normalN

            // 1. diffuse maps
            List<Texture> diffuseMaps = loadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse");
            textures.AddRange(diffuseMaps);
            // 2. specular maps
            List<Texture> specularMaps = loadMaterialTextures(material, TextureType.Specular, "texture_specular");
            textures.AddRange(specularMaps);
            // 3. normal maps
            List<Texture> normalMaps = loadMaterialTextures(material, TextureType.Height, "texture_normal");
            textures.AddRange(normalMaps);
            // 4. height maps
            List<Texture> heightMaps = loadMaterialTextures(material, TextureType.Ambient, "texture_height");
            textures.AddRange(heightMaps);

            // If we were targeting .net 5+ we could use
            //      return new Mesh(CollectionsMarshal.AsSpan(vertices), CollectionsMarshal.AsSpan(indices));
            // to avoid making a copy of all the vertex data.
            return new Mesh(vertices.ToArray(), indices.ToArray(), textures);
        }

        // checks all material textures of a given type and loads the textures if they're not loaded yet.
        // the required info is returned as a Texture struct.
        private List<Texture> loadMaterialTextures(Material mat, TextureType type, string typeName)
        {
            List<Texture> textures = new List<Texture>();

            for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
            {
                TextureSlot str;
                mat.GetMaterialTexture(type, i, out str);
                string filename = directory + "/" + str.FilePath;
                // check if texture was loaded before and if so, continue to next iteration: skip loading a new texture
                bool skip = false;
                for (int j = 0; j < textures_loaded.Count; j++)
                {
                    
                    if (textures_loaded[j].path.CompareTo(filename) ==0)
                    {
                        textures.Add(textures_loaded[j]);
                        skip = true; // a texture with the same filepath has already been loaded, continue to next one. (optimization)
                        break;
                    }
                }
                if (!skip)
                {   // if texture hasn't been loaded already, load it
                    Texture texture = Texture.LoadFromFile(filename, typeName);
                    textures.Add(texture);
                    textures_loaded.Add(texture);  // store it as texture loaded for entire model, to ensure we won't unnecessary load duplicate textures.
                }
            }
            return textures;
        }

    }
}
