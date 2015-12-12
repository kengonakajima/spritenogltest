using UnityEngine;
using System.Collections;

public class SpriteCube : MonoBehaviour {
    void Start() {
    }
    void Update() {
    }
    public void setVisible(bool enable) {
        GetComponent<Renderer>().enabled = enable;        
    }
};
