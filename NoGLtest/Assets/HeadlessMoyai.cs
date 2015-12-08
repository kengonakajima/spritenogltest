using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Image {
    public uint id;
    Texture2D m_tex; // moyaiのImageクラス相当の機能はTexture2Dが持っているので内部的に持って代用する
    public Image() {
        m_tex = new Texture2D(2,2);
    }
    public void loadPNGMem( byte[] pngbin ) {
        m_tex.LoadImage(pngbin);
    }
};
public class Texture {
    public uint id;
    Texture2D m_tex;
    Image m_img;
    public Texture() {
    }
    public void setImage(Image img) {
        m_img = img;
    }
};
