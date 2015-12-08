using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Image {
    public uint id;
    Texture2D tex; // moyaiのImageクラス相当の機能はTexture2Dが持っているので内部的に持って代用する
    public Image() {
        tex = new Texture2D(2,2);
    }
    public void loadPNGMem( byte[] pngbin ) {
        tex.LoadImage(pngbin);
    }
};
