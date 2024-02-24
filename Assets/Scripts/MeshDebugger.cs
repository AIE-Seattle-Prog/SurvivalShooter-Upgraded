using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshDebugger : MonoBehaviour
{
    [SerializeField] MeshFilter[] filters;

    [SerializeField] MeshFilter targetFilter;
    [SerializeField] Renderer targetRenderer;

    [SerializeField] Mesh savedMesh;

    int[] tris;
    Vector3[] verts;
    Vector3[] normals;

    [ContextMenu("Merge Meshes")]
    void MergeMeshes()
    {
        if (filters == null) return;

        CombineInstance[] combine = new CombineInstance[filters.Length];

        for (int i = 0; i < filters.Length; i++)
        {
            combine[i].mesh = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.localToWorldMatrix;
        }

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine);

        targetFilter = GetComponent<MeshFilter>();
        if (targetFilter == null)
        {
            targetFilter = gameObject.AddComponent<MeshFilter>();
        }
        targetFilter.sharedMesh = newMesh;
        SaveMesh();
    }

    [ContextMenu("Center Bounds")]
    void CenterBounds()
    {
        if (targetRenderer == null || targetFilter == null || targetFilter.sharedMesh == null) return;

        Bounds bounds = targetRenderer.bounds;

        Vector3[] vertices = targetFilter.sharedMesh.vertices;

        for (int i = 0; i < targetFilter.sharedMesh.vertexCount; i++)
        {
            vertices[i] -= bounds.center;
        }

        targetFilter.sharedMesh.SetVertices(vertices);

        targetRenderer.bounds = new Bounds(Vector3.zero, targetRenderer.bounds.size);
    }

    [ContextMenu("Save mesh")]
    void SaveMesh()
    {
        if (targetFilter != null)
        {
            savedMesh = targetFilter.sharedMesh;
        }
    }

    [ContextMenu("Load mesh")]
    void LoadMesh()
    {
        if (targetFilter != null)
        {
            targetFilter.sharedMesh = savedMesh;
        }
    }

    [ContextMenu("Create new mesh")]
    void TryCreateNewMesh()
    {
        if (targetFilter == null || targetFilter.sharedMesh == null) return;

        verts = targetFilter.sharedMesh.vertices;
        tris = targetFilter.sharedMesh.triangles;
        normals = targetFilter.sharedMesh.normals;

        Mesh newMesh = new Mesh();
        newMesh.SetVertices(verts);
        newMesh.SetTriangles(tris, 0);
        newMesh.SetNormals(normals);

        targetFilter.sharedMesh = newMesh;
    }

    [ContextMenu("Write Mesh Information")]
    void WriteMeshInformation()
    {
        if (targetFilter == null) return;

        Mesh mesh = targetFilter.sharedMesh;

        string triString = string.Empty;
        string vertString = string.Empty;
        string normalString = string.Empty;

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            triString += $"{mesh.triangles[i]}, ";
        }

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            vertString += $"new Vector3({mesh.vertices[i].x}f, {mesh.vertices[i].y}f, {mesh.vertices[i].z}f), ";
        }

        for (int i = 0; i < mesh.normals.Length; i++)
        {
            normalString += $"new Vector3({mesh.normals[i].x}f, {mesh.normals[i].y}f, {mesh.normals[i].z}f), ";
        }

        File.WriteAllText("Assets/tris.txt", triString);
        File.WriteAllText("Assets/verts.txt", vertString);
        File.WriteAllText("Assets/normals.txt", normalString);
    }

    void OnDrawGizmosSelected()
    {
        if (targetRenderer != null)
        {
            Gizmos.DrawWireCube(targetRenderer.bounds.center, targetRenderer.bounds.size);
        }
    }
}
