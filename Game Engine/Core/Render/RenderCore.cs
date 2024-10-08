﻿using Game_Engine.Enums;
using OpenTK.Graphics.OpenGL4;

namespace Game_Engine.Core.Render
{
    internal class RenderCore : IDisposable
    {
        private readonly Shader _shader = new(@"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.vert",
                                              @"C:\Users\it_ge\source\repos\Game Engine\Game Engine\Shaders\Shader.frag");

        public void Dispose() => _shader.Dispose();

        public static void LoadGameObjectInGPU(GameObjectBase3D obj)
        {
            var vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, obj.Model.Vertices.Length * sizeof(float),
                                                    obj.Model.Vertices, BufferUsageHint.DynamicDraw);

            var vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            obj.VAO = vertexArrayObject;
            obj.VBO = vertexBufferObject;
        }

        public void DrawGameObject(GameObjectBase3D obj, Camera cam)
        {
            GL.BindVertexArray(obj.VAO);
            _shader.Use();

            _shader.SetMatrix4("model", obj.ModelMatrix);
            _shader.SetMatrix4("view", cam.ViewMatrix);
            _shader.SetMatrix4("projection", cam.ProjectionMatrix);
            _shader.SetVector3("aColor", obj.Model.GetData(AttribTypes.Color, 0));

            GL.DrawArrays(PrimitiveType.Triangles, 0, (int)obj.Model.TrianglesCount * 3);
        }
    }
}
