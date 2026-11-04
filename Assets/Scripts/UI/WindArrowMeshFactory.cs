using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Gera por código a malha (mesh) de uma seta 3D: uma haste (cilindro) com uma ponta (cone).
    /// A seta aponta para +Z e tem 1 metro de comprimento, centrada na origem.
    /// Assim a seta do vento não depende de nenhum modelo externo (dá para trocar por um modelo depois).
    ///
    /// Classe estática: não vai em nenhum GameObject. Quem usa: WindArrowIndicator.
    /// </summary>
    public static class WindArrowMeshFactory
    {
        private const int Segments = 20;      // quantos "lados" tem o cilindro e o cone
        private const float ShaftRadius = 0.09f;
        private const float HeadRadius = 0.22f;
        private const float TailZ = -0.5f;    // onde a haste começa
        private const float NeckZ = 0.1f;     // onde a haste termina e a ponta começa
        private const float TipZ = 0.5f;      // a ponta da seta

        public static Mesh Build()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            Vector3 tailCenter = new Vector3(0f, 0f, TailZ);
            Vector3 tip = new Vector3(0f, 0f, TipZ);

            for (int i = 0; i < Segments; i++)
            {
                float a0 = 2f * Mathf.PI * i / Segments;
                float a1 = 2f * Mathf.PI * (i + 1) / Segments;
                Vector3 dir0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f);
                Vector3 dir1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f);

                // Pontos da haste (cilindro) na base (tail) e no pescoço (neck).
                Vector3 shaftTail0 = dir0 * ShaftRadius + Vector3.forward * TailZ;
                Vector3 shaftTail1 = dir1 * ShaftRadius + Vector3.forward * TailZ;
                Vector3 shaftNeck0 = dir0 * ShaftRadius + Vector3.forward * NeckZ;
                Vector3 shaftNeck1 = dir1 * ShaftRadius + Vector3.forward * NeckZ;

                // Pontos da base da ponta (cone), mais largos que a haste.
                Vector3 headBase0 = dir0 * HeadRadius + Vector3.forward * NeckZ;
                Vector3 headBase1 = dir1 * HeadRadius + Vector3.forward * NeckZ;

                // A ordem dos vértices define o lado "de fora" de cada triângulo.
                AddTriangle(vertices, triangles, shaftTail0, shaftTail1, shaftNeck0); // parede da haste
                AddTriangle(vertices, triangles, shaftTail1, shaftNeck1, shaftNeck0);
                AddTriangle(vertices, triangles, tailCenter, shaftTail1, shaftTail0); // tampa traseira
                AddTriangle(vertices, triangles, shaftNeck0, headBase1, headBase0);   // anel sob a ponta
                AddTriangle(vertices, triangles, shaftNeck0, shaftNeck1, headBase1);
                AddTriangle(vertices, triangles, headBase0, headBase1, tip);          // cone
            }

            var mesh = new Mesh { name = "WindArrow" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // Cada triângulo ganha vértices próprios (arestas "duras"), o que é simples e basta para um visual liso.
        private static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            int start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
        }
    }
}
