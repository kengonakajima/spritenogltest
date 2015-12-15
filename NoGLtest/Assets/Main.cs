using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    Texture2D m_basetex;
    Texture2D m_woodtex;
    Texture2D m_memtex;
    
    int cur_frame;
    float show_fps_at;
    int cur_num=0;

    float m_z = 0;

    void Awake() {
        GetComponent<Camera>().orthographicSize = Screen.height / 2;
        Debug.Log( "Main Awake!" + Screen.height/2 );
    }
	// Use this for initialization
	void Start () {
        m_basetex = Resources.Load<Texture2D>("base") as Texture2D;
        Debug.Log(m_basetex);
        m_woodtex = Resources.Load<Texture2D>("wood256") as Texture2D;
        Debug.Log(m_woodtex);
        m_memtex = new Texture2D(256,256);
        Debug.Log(m_memtex);
        for(int y=0;y<256;y++) {
            for(int x=0;x<256;x++) {
                Color color = ((x & y) != 0 ? Color.white : Color.gray);
                m_memtex.SetPixel(x, y, color);
            }
        }
        m_memtex.Apply();
	}

    
	// Update is called once per frame
	void Update () {
        bool j = Input.GetMouseButtonDown(0);
        if(j) {
            for(int i=0;i<100;i++) {
                Vector2 at = new Vector2( Random.Range(-Screen.width/2,Screen.width/2), Random.Range(-Screen.height/2,Screen.height/2) );
                float r = Random.Range(0f,1f);
                if( r < 0.3 ) {
                    addSprite1(at);
                } else if( r < 0.6 ) {
                    addSprite2(at);
                } else {
                    addGrid(at);
                }
            }
            cur_num+=100;
        }

        if( Input.GetKeyDown("t") ) {
            Debug.Log("SSSS: " + Screen.width + "," + Screen.height );
            //            Vector2 at = new Vector2( 1,1 );
            //            addSprite(at);
        }
            
        float nt = Time.time;
        if( show_fps_at < nt - 1 ) {
            show_fps_at = nt;
            Text t = GameObject.FindWithTag("FPStext").GetComponent<Text>() as Text;
            t.text = "fps:" + cur_frame + " num:" + cur_num + " Touch/Click to add sprites";
            cur_frame = 0;
        }
        cur_frame ++;
    }

    // Spriteクラスを使うもの
    void addSprite1( Vector2 at ) {
        Sprite ns = Sprite.Create( m_memtex,
                                   new Rect(0,0,100,100),
                                   at,
                                   128f );
        GameObject o = new GameObject();
        o.name = "wood";
        o.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
        sr.sprite = ns;
        o.transform.Translate(0,0,m_z);
        o.transform.localScale = new Vector3(32,32,1);
        m_z-=0.01f;
    }
    // tiledeck
    void addSprite2( Vector2 at ) {
        Debug.Log( "at:" + at.x + "," + at.y );
        Sprite ns = Sprite.Create( m_basetex,
                                   new Rect(0,256-8,8,8),
                                   at,
                                   128f );
        GameObject o = new GameObject();
        o.name = "base";
        o.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
        sr.sprite = ns;
        o.transform.Translate(0,0,m_z);
        o.transform.localScale = new Vector3(32,32,1);
        m_z-=0.01f;        
	}
    // Grid.csでメッシュを入れ替えている。
    // そのときzはすべて0にしているので、z値を入れ替えることで、優先順位を制御できる。
    // 
    void addGrid(Vector2 at ) {
        GameObject prefab = Resources.Load<GameObject>( "GridCube" ) as GameObject;
        GameObject o = Instantiate( prefab, new Vector3(at.x,at.y,m_z), Quaternion.identity ) as GameObject;
        m_z-=0.01f;
        o.transform.Translate(0,0,m_z);
        o.transform.localScale = new Vector3(16*4,16*4,1);        
        Grid g = o.GetComponent<Grid>();
        g.Poo(2);
    }
    // Cubeをベースに通常のスプライトを表示.
    // 1. FileEntryからmaterialを生成する:texture2dがTextureにある。TileDeckにm_texがある。
    // 2. SpriteCubeをつくり、materialを設定する(setdeck)
    // 3. setindex
    void addSprite( Vector2 at ) {
        GameObject prefab = Resources.Load<GameObject>( "SpriteCube" ) as GameObject;
        GameObject o = Instantiate( prefab, new Vector3(at.x,at.y,m_z), Quaternion.identity ) as GameObject;
        m_z-=0.01f;
        SpriteCube sc = o.GetComponent<SpriteCube>();

    }
}
