using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AsImpL
{
    public static class Triangulator
    {

        // Triangulate a polygon
        public static void Triangulate(DataSet dataSet, DataSet.FaceIndices[] faces, bool fastAndInaccurate=false)
        {
            int numVerts = faces.Length;
            Debug.LogFormat("Triangulating a face with {0} vertices...", numVerts);
            if (fastAndInaccurate)
            {
                // (assuming it is a simple convex polygon)
                for (int q = 1; q < numVerts - 1; q++)
                {
                    dataSet.AddFaceIndices(faces[0]);
                    dataSet.AddFaceIndices(faces[q + 1]);
                    dataSet.AddFaceIndices(faces[q]);
                }
            }
            else
            {
                TriangulateGenericPolygon(dataSet, faces);
            }
        }

        public static void TriangulateGenericPolygon(DataSet dataSet, DataSet.FaceIndices[] faces)
        {
            // TODO: implement ear clipping triangulation (e.g. https://www.habrador.com/tutorials/math/10-triangulation/)
            throw new NotImplementedException();
        }
    }
}
