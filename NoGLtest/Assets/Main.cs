using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

    Texture2D m_basetex;
    Texture2D m_woodtex;
	// Use this for initialization
	void Start () {
        m_basetex = Resources.Load<Texture2D>("base") as Texture2D;
        Debug.Log(m_basetex);
        m_woodtex = Resources.Load<Texture2D>("wood256") as Texture2D;
        Debug.Log(m_woodtex);        
	}
	
	// Update is called once per frame
	void Update () {
        bool j = Input.GetButtonDown("Jump");
        if(j) {
            Debug.Log("jmpbutton");
            Vector2 at = new Vector2( Random.Range(-1f,1f), Random.Range(-1f,1f) );
            Sprite ns = Sprite.Create( m_woodtex,
                                       new Rect(0,0,48,48),
                                       at,
                                       128f );
            GameObject o = new GameObject();
            o.name = "hoge";
            o.AddComponent<SpriteRenderer>();
            SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
            sr.sprite = ns;
            o.transform.Translate(1,0,0);
            //            o.transform.Scale(2,1,1);            
            o.transform.localScale = new Vector3(2,1,1);
        }
	}
}
