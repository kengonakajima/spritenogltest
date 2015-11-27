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

    // [0] : left bottom uv
    // [1] : right top uv
    Vector2[] getUVFromTileDeck(int ind) {
        // assume 32x32 cells in 256px x 256px
        // left bottom:0,0 right top:1,1
        int x = ind % 32;
        int y = ind / 32;
        float unit = 1.0f / 32.0f;
        float u = unit * x;
        float v = unit * y;
        Vector2[] o = { new Vector2(u,1.0f-v-unit), new Vector2(u+unit,1.0f-v) };
        return o;
    }
    
    void makeGrid() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        int w = 8;
        int h = 8;
        int ncells = w*h;
        
        Vector3[] vertices = new Vector3[4*ncells]; // UVを共有できないので頂点もセル数の4倍必要
        Vector2[] uv = new Vector2[4*ncells];
        int[] triangles = new int[6*ncells];

        Vector2 lb = new Vector2(0,0);
        float cellsz = 0.1f;

        // B--C
        // | /|
        // |/ |
        // A--D
        for(int y=0;y<h;y++) {
            for(int x=0;x<w;x++) {
                int triangle_ind = x+ y*w;
                int vi = triangle_ind * 4;
                int ti = triangle_ind * 6;
                float dx = x * cellsz, dy = y * cellsz;
                //                Debug.Log("i:"+triangle_ind+ "dx:"+dx+"dy:"+dy);
                vertices[vi+0] = new Vector3( lb.x + dx, lb.y + dy,0); // A
                vertices[vi+1] = new Vector3( lb.x + dx, lb.y + dy + cellsz, 0); // B
                vertices[vi+2] = new Vector3( lb.x + dx + cellsz, lb.y + dy + cellsz, 0); // C
                vertices[vi+3] = new Vector3( lb.x + dx + cellsz, lb.y + dy, 0); // D
                Vector2[] uvpair = getUVFromTileDeck( triangle_ind % 5 );
                uv[vi+0] = new Vector2( uvpair[0].x, uvpair[0].y ); // A
                uv[vi+1] = new Vector2( uvpair[0].x, uvpair[1].y ); // B
                uv[vi+2] = new Vector2( uvpair[1].x, uvpair[1].y ); // C
                uv[vi+3] = new Vector2( uvpair[1].x, uvpair[0].y ); // D
                triangles[ti+0] = vi+0; // A
                triangles[ti+1] = vi+1; // B
                triangles[ti+2] = vi+2; // C
                triangles[ti+3] = vi+0; // A
                triangles[ti+4] = vi+2; // C
                triangles[ti+5] = vi+3; // D           
            }
        }


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    
}
