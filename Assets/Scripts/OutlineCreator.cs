using System.Collections.Generic;
using UnityEngine;

public class OutlineCreator : MonoBehaviour
{
    public float lineWidth;
    public Material material;

    void Start()
    {
        // Stop if no mesh filter exists
        if (GetComponent<MeshFilter>() == null)
        {
            return;
        }

        // Get triangles and vertices from mesh
        int[] triangles = GetComponent<MeshFilter>().mesh.triangles;
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

        // Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
        Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int e = 0; e < 3; e++)
            {
                int vert1 = triangles[i + e];
                int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
                string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
                if (edges.ContainsKey(edge))
                {
                    edges.Remove(edge);
                }
                else
                {
                    edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
                }
            }
        }

        // Create edge lookup Dictionary
        Dictionary<int, int> lookup = new Dictionary<int, int>();
        foreach (KeyValuePair<int, int> edge in edges.Values)
        {
            if (lookup.ContainsKey(edge.Key) == false)
            {
                lookup.Add(edge.Key, edge.Value);
            }
        }

        // Create line prefab
        LineRenderer linePrefab = new GameObject().AddComponent<LineRenderer>();
        linePrefab.transform.name = "Line";
        linePrefab.positionCount = 0;
        linePrefab.material = material;
        linePrefab.startWidth = linePrefab.endWidth = lineWidth;

        // Create first line
        LineRenderer line = Instantiate(linePrefab.gameObject).GetComponent<LineRenderer>();
        line.transform.parent = transform;

        // This vector3 gets added to each line position, so it sits in front of the mesh
        // Change the -0.1f to a positive number and it will sit behind the mesh
        Vector3 bringFoward = new Vector3(0f, 0f, -0.1f);

        // Loop through edge vertices in order
        int startVert = 0;
        int nextVert = startVert;
        int highestVert = startVert;
        while (true)
        {

            // Add to line
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, vertices[nextVert] + bringFoward);

            // Get next vertex
            nextVert = lookup[nextVert];

            // Store highest vertex (to know what shape to move to next)
            if (nextVert > highestVert)
            {
                highestVert = nextVert;
            }

            // Shape complete
            if (nextVert == startVert)
            {

                // Finish this shape's line
                line.positionCount++;
                line.SetPosition(line.positionCount - 1, vertices[nextVert] + bringFoward);

                // Go to next shape if one exists
                if (lookup.ContainsKey(highestVert + 1))
                {

                    // Create new line
                    line = Instantiate(linePrefab).GetComponent<LineRenderer>();
                    line.transform.parent = transform;

                    // Set starting and next vertices
                    startVert = highestVert + 1;
                    nextVert = startVert;

                    // Continue to next loop
                    continue;
                }

                // No more verts
                break;
            }
        }
    }
}