using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    Texture2D m_basetex;
    Texture2D m_woodtex;

    int cur_frame;
    float show_fps_at;
    int cur_num=0;
    
	// Use this for initialization
	void Start () {
        m_basetex = Resources.Load<Texture2D>("base") as Texture2D;
        Debug.Log(m_basetex);
        m_woodtex = Resources.Load<Texture2D>("wood256") as Texture2D;
        Debug.Log(m_woodtex);        
	}
	
	// Update is called once per frame
	void Update () {
        bool j = Input.GetMouseButtonDown(0);
        if(j) {
            for(int i=0;i<100;i++) {
                Vector2 at = new Vector2( Random.Range(-3f,3f), Random.Range(-3f,3f) );
                if( Random.Range(0f,1f) < 0.5 ) {
                    addSprite1(at);
                } else {
                    addSprite2(at);
                }
            }
            cur_num+=100;
        }

        float nt = Time.time;
        if( show_fps_at < nt - 1 ) {
            show_fps_at = nt;
            Text t = GameObject.FindWithTag("FPStext").GetComponent<Text>() as Text;
            t.text = "fps:" + cur_frame + " num:" + cur_num;
            cur_frame = 0;
        }
        cur_frame ++;
    }
    
    void addSprite1( Vector2 at ) {
        Sprite ns = Sprite.Create( m_woodtex,
                                   new Rect(0,0,48,48),
                                   at,
                                   128f );
        GameObject o = new GameObject();
        o.name = "wood";
        o.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
        sr.sprite = ns;
        //        o.transform.Translate(1,0,0);
        o.transform.localScale = new Vector3(1,1,1);
    }
    void addSprite2( Vector2 at ) {
        Sprite ns = Sprite.Create( m_basetex,
                                   new Rect(0,256-8,8,8),
                                   at,
                                   128f );
        GameObject o = new GameObject();
        o.name = "base";
        o.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
        sr.sprite = ns;
        //        o.transform.Translate(0,-0.5f,0);
        o.transform.localScale = new Vector3(2,2,1);        
	}
}
