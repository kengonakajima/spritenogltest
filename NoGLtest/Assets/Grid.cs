using UnityEngine;
using System.Collections;

public class Grid : MonoBehaviour {

	// Use this for initialization
	void Start () {
        makeGrid();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    
    void makeGrid() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[3];


        vertices[0] = new Vector3(0,0,0);
        vertices[1] = new Vector3(0,1,0);
        vertices[2] = new Vector3(1,1,0);
        vertices[3] = new Vector3(1,0,0);
        uv[0] = new Vector2(0,0);
        uv[1] = new Vector2(0,1);
        uv[2] = new Vector2(1,1);
        uv[3] = new Vector2(1,0);
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    
}
