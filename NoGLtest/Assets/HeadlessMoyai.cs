using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HM {
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
    public class Viewport {
        public uint id;
        public uint screen_width, screen_height;
        public Vector3 scl;
        public void setSize(uint w, uint h) { screen_width = w; screen_height = h; }
        public void setScale2D( float sx, float sy ) { scl.x = sx; scl.y=sy; scl.z=1; }

    };
    public class Camera {
        public uint id;
        public Vector3 loc;
        public void setLoc( float x, float y ) { loc.x = x; loc.y = y; }
    };
    public class Layer {
        public uint id;
        Viewport viewport;
        Camera camera;
        ArrayList props;

        public Layer() {
            props = new ArrayList();
        }
        public void setViewport(Viewport vp) { viewport = vp; }
        public void setCamera(Camera cam) { camera = cam; }
        public void insertProp( Prop2D prop ) {
            props.Add(prop);
            Debug.Log("insertProp: cnt:" + props.Count );
        }
    };
    public class Color {
        public float r,g,b,a;
    };
    public class Grid {
        public uint id;
        public uint width, height;
        public int[] index_table;
        public bool[] xflip_table;
        public bool[] yflip_table;
        public Vector2[] texofs_table;
        public bool[] rot_table;
        public Color[] color_table;
        public uint tiledeck_id;
        public TileDeck deck;
        public float enfat_epsilon;
        public uint getIndexTableSize() { return width*height*4; }
        public void setSize(uint w, uint h) {
            width = w;
            height = h;
            index_table = new int[w*h];
        }
        public void set(uint x, uint y, int ind ) {
            uint i = x+y*width;
            index_table[i] = ind;
        }
        public void bulkSetIndex( byte[] buf, uint ofs ) {
            uint ind=0;
            for(uint y=0;y<height;y++) {
                for(uint x=0;x<width;x++) {
                    set(x,y,BitConverter.ToInt32(buf,(int)(ofs+ind*4)) );
                    ind++;
                }
            }
        }
    };
    public class Prop2D {
        public uint id;
        public TileDeck deck;
        public int index;
        public Vector2 scl;
        public Vector2 loc;
        public float rot;
        public bool xflip;
        public bool yflip;
        public Color color;
        public Grid grid;
        public bool to_clean;
        
        public Prop2D() {
            color = new Color();
            to_clean = false;
        }
        public void setDeck(TileDeck dk) { deck = dk; }
        public void setIndex(int ind) { index = ind; }
        public void setScl( float xs, float ys ) { scl.x = xs; scl.y = ys; }
        public void setLoc( float x, float y ) { loc.x = x; loc.y = y; }
        public void setRot( float r ) { rot = r; }
        public void setXFlip( bool xf ) { xflip = xf; }
        public void setYFlip( bool yf ) { yflip = yf; }
        public void setColor( float r, float g, float b, float a ) {
            color.r = r; color.g = g; color.b = b; color.a = a;
        }
        public void setGrid( Grid g ) { grid = g; }
        public void setToClean(bool flag) { to_clean = flag; }
    };
};
