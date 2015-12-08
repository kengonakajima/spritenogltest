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
public class TileDeck {
    public uint id;
    public uint cell_width, cell_height;
    public uint tile_width, tile_height;
    Texture m_tex;
    public TileDeck() {
    }
    public void setTexture( Texture tex ) {
        m_tex = tex;
    }
    public void setSize( uint sprw, uint sprh, uint cellw, uint cellh ) {
        tile_width = sprw;
        tile_height = sprh;
        cell_width = cellw;
        cell_height = cellh;
    }
};
