using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;


enum PACKETTYPE {
    R2C_PROP2D_CREATE_SNAPSHOT = 600,
    R2C_PROP2D_LOC = 601,
    R2C_PROP2D_GRID = 602,
    R2C_PROP2D_INDEX = 603,
    R2C_PROP2D_SCALE = 604,
    R2C_PROP2D_ROT = 605,
    R2C_PROP2D_XFLIP = 606,
    R2C_PROP2D_YFLIP = 607,
    R2C_PROP2D_COLOR = 608,
    R2C_PROP2D_DELETE = 610,

    R2C_LAYER_CREATE = 620,
    R2C_LAYER_VIEWPORT = 621,
    R2C_LAYER_CAMERA = 622,
    R2C_VIEWPORT_CREATE = 630,
    R2C_VIEWPORT_SIZE = 631,
    R2C_VIEWPORT_SCALE = 632,    
    R2C_CAMERA_CREATE = 640,
    R2C_CAMERA_LOC = 641,

    R2C_TEXTURE_CREATE = 650,
    R2C_TEXTURE_IMAGE = 651,
    R2C_IMAGE_CREATE = 660,
    R2C_IMAGE_LOAD_PNG = 661,
    
    R2C_TILEDECK_CREATE = 670,
    R2C_TILEDECK_TEXTURE = 671,
    R2C_TILEDECK_SIZE = 672,
    R2C_GRID_CREATE_SNAPSHOT = 680, // Gridの情報を一度に1種類送る
    R2C_GRID_TABLE_SNAPSHOT = 681, // Gridの水平移動各種テーブル
    R2C_GRID_INDEX = 682, // indexが変化した。
    R2C_FILE = 690, // ファイルを直接送信する step 1: ファイルを作成してIDを割りつける。
    
    
    ERROR = 2000, // 何らかのエラー。エラー番号を返す
};

public class PacketGridCreateSnapshot {
    public uint id;
    public ushort width;
    public ushort height;
    public uint tiledeck_id;
    public float enfat_epsilon;
    public void fromBytes( byte[] buf ) {
        id = BitConverter.ToUInt32(buf,0);
        width = BitConverter.ToUInt16(buf,4);
        height = BitConverter.ToUInt16(buf,6);
        tiledeck_id = BitConverter.ToUInt32(buf,8);
        enfat_epsilon = BitConverter.ToSingle(buf,12);
    }
};


public class Protocol : MonoBehaviour {
    TcpClient m_cli;
    NetworkStream m_stream;
    MemoryStream m_ms;
    byte[] m_readbuf;
    Storage m_storage;
    Pool<HM.Image> m_image_pool;
    Pool<HM.Texture> m_texture_pool;
    Pool<HM.TileDeck> m_tiledeck_pool;
    Pool<HM.Viewport> m_viewport_pool;
    Pool<HM.Camera> m_camera_pool;
    Pool<HM.Layer> m_layer_pool;
    Pool<HM.Grid> m_grid_pool;
    void Start () {
        m_storage = new Storage();        
        m_readbuf = new byte[1024*16];
        m_ms = new MemoryStream();
        m_image_pool = new Pool<HM.Image>();
        m_texture_pool = new Pool<HM.Texture>();
        m_tiledeck_pool = new Pool<HM.TileDeck>();
        m_viewport_pool = new Pool<HM.Viewport>();
        m_camera_pool = new Pool<HM.Camera>();
        m_layer_pool = new Pool<HM.Layer>();
        m_grid_pool = new Pool<HM.Grid>();
        setupClient();
    }
    
    void shiftMemoryStream(MemoryStream ms, int shiftsize ) {
        byte[] buf = ms.GetBuffer();
        //        Debug.Log( "shiftsize:" + shiftsize + " msl:" + ms.Length );
        Buffer.BlockCopy( buf, shiftsize, buf, 0, (int)ms.Length - shiftsize );
        ms.SetLength( ms.Length - shiftsize );
    }
    void parseStream( MemoryStream ms ) {
        byte[] b = ms.GetBuffer();
        int total_len = (int)ms.Length;
        if( b.Length >= 2+2 ) { // record_len + funcid_len
            ushort record_len = BitConverter.ToUInt16( b, 0 );
            if( record_len >= 2 && total_len >= record_len ) {
                ushort funcid = BitConverter.ToUInt16( b, 2 );
                //                Debug.Log( "record found! len:" + record_len + " fid:" + funcid );
                byte[] argbuf = new byte[record_len-2];
                Buffer.BlockCopy( b, 4, argbuf, 0, record_len-2);
                onRemoteFunction( funcid, argbuf );
                shiftMemoryStream(ms,record_len+2);
            }
        }
    }
    private void onRemoteFunction( ushort funcid, byte[] argbuf ) {
        switch( (PACKETTYPE)funcid) {
        case PACKETTYPE.R2C_PROP2D_CREATE_SNAPSHOT:
        case PACKETTYPE.R2C_PROP2D_LOC:
        case PACKETTYPE.R2C_PROP2D_GRID:
        case PACKETTYPE.R2C_PROP2D_INDEX:
        case PACKETTYPE.R2C_PROP2D_SCALE:
        case PACKETTYPE.R2C_PROP2D_ROT:
        case PACKETTYPE.R2C_PROP2D_XFLIP:
        case PACKETTYPE.R2C_PROP2D_YFLIP:
        case PACKETTYPE.R2C_PROP2D_COLOR:
        case PACKETTYPE.R2C_PROP2D_DELETE:

        case PACKETTYPE.R2C_LAYER_CREATE:
            {
                uint layer_id = BitConverter.ToUInt32(argbuf,0);
                m_layer_pool.ensure(layer_id);
                Debug.Log("received layer_create. " + layer_id );
                break;
            }            
        case PACKETTYPE.R2C_LAYER_VIEWPORT:
            {
                uint layer_id = BitConverter.ToUInt32(argbuf,0);
                uint vp_id = BitConverter.ToUInt32(argbuf,4);
                HM.Layer l = m_layer_pool.get(layer_id);
                HM.Viewport vp = m_viewport_pool.get(vp_id);
                if(l!=null && vp!=null) {
                    l.setViewport(vp);
                    Debug.Log("received layer_vp l:" + layer_id + " vp:" + vp_id );
                }
                break;
            }
        case PACKETTYPE.R2C_LAYER_CAMERA:
            {
                uint layer_id = BitConverter.ToUInt32(argbuf,0);
                uint camera_id = BitConverter.ToUInt32(argbuf,4);
                HM.Layer l = m_layer_pool.get(layer_id);
                HM.Camera cam = m_camera_pool.get(camera_id);
                if(l!=null && cam!=null) {
                    l.setCamera(cam);
                    Debug.Log("received layer_camera. l:" + layer_id + " cam:" + camera_id );
                }
                break;
            }
        case PACKETTYPE.R2C_VIEWPORT_CREATE:
            {
                uint vp_id = BitConverter.ToUInt32(argbuf,0);
                m_viewport_pool.ensure(vp_id);
                Debug.Log("received vp_create:" + vp_id );
                break;
            }
        case PACKETTYPE.R2C_VIEWPORT_SIZE:
            {
                uint vp_id = BitConverter.ToUInt32(argbuf,0);
                HM.Viewport vp = m_viewport_pool.get(vp_id);
                if(vp!=null) {
                    uint w = BitConverter.ToUInt32(argbuf,4);
                    uint h = BitConverter.ToUInt32(argbuf,8);
                    vp.setSize(w,h);
                    Debug.Log("received vp_size. id:" + vp_id + " " + w + "," + h );
                }
                break;
            }
        case PACKETTYPE.R2C_VIEWPORT_SCALE:
            {
                uint vp_id = BitConverter.ToUInt32(argbuf,0);
                HM.Viewport vp = m_viewport_pool.get(vp_id);
                if(vp!=null) {
                    float sclx = BitConverter.ToSingle(argbuf,4);
                    float scly = BitConverter.ToSingle(argbuf,8);
                    vp.setScale2D(sclx,scly);
                    Debug.Log("received vp_scale. id:" + vp_id + " " + sclx + "," + scly );
                }
                break;
            }            
        case PACKETTYPE.R2C_CAMERA_CREATE:
            {
                uint cam_id = BitConverter.ToUInt32(argbuf,0);
                m_camera_pool.ensure(cam_id);
                Debug.Log("received cam_create. " + cam_id );
                break;
            }
        case PACKETTYPE.R2C_CAMERA_LOC:
            {
                uint cam_id = BitConverter.ToUInt32(argbuf,0);
                HM.Camera cam = m_camera_pool.get(cam_id);
                if(cam!=null) {
                    float x = BitConverter.ToSingle(argbuf,4);
                    float y = BitConverter.ToSingle(argbuf,8);
                    cam.setLoc(x,y);
                    Debug.Log("received cam_loc: " + cam_id + " " + x + "," + y );
                }
                break;
            }            
        case PACKETTYPE.R2C_TEXTURE_CREATE:
            {
                uint tex_id = BitConverter.ToUInt32(argbuf,0);
                m_texture_pool.ensure(tex_id);
                Debug.Log( "received tex_create:" + tex_id );
                break;
            }
        case PACKETTYPE.R2C_TEXTURE_IMAGE:
            {
                uint tex_id = BitConverter.ToUInt32(argbuf,0);
                uint img_id = BitConverter.ToUInt32(argbuf,4);
                HM.Texture tex = m_texture_pool.get(tex_id);
                HM.Image img = m_image_pool.get(img_id);
                if( tex != null && img != null ) {
                    tex.setImage(img);
                    print("received tex_image. tex:" + tex_id + " img:" + img_id );
                }
                break;                
            }
        case PACKETTYPE.R2C_IMAGE_CREATE:
            {
                uint img_id = BitConverter.ToUInt32(argbuf,0);
                m_image_pool.ensure(img_id);
                Debug.Log( "received img_create:" + img_id);
                break;
            }
        case PACKETTYPE.R2C_IMAGE_LOAD_PNG:
            {
                uint img_id = BitConverter.ToUInt32(argbuf,0);
                int pathlen = (int)argbuf[4];
                string path = Util.getASCIIString( argbuf,4+1,pathlen);
                FileEntry fe = m_storage.findFileEntry(path);
                HM.Image img = m_image_pool.get(img_id);            
                if(fe != null && img != null ) {
                    img.loadPNGMem( fe.getBody() );
                    Debug.Log( "received imgloadpng:" + img_id + " path:" + path );
                }
                break;
            }
        case PACKETTYPE.R2C_TILEDECK_CREATE:
            {
                uint dk_id = BitConverter.ToUInt32(argbuf,0);
                m_tiledeck_pool.ensure(dk_id);
                Debug.Log( "received tdk_create. id:" + dk_id );
                break;
            }
        case PACKETTYPE.R2C_TILEDECK_TEXTURE:
            {
                uint dk_id = BitConverter.ToUInt32(argbuf,0);
                uint tex_id = BitConverter.ToUInt32(argbuf,4);
                HM.TileDeck dk = m_tiledeck_pool.get(dk_id);
                HM.Texture tex = m_texture_pool.get(tex_id);
                if( dk != null && tex != null ) {
                    dk.setTexture(tex);
                    Debug.Log( "received tdk_tex. dk:" + dk_id + " tex:" + tex_id );
                }
                break;
            }
        case PACKETTYPE.R2C_TILEDECK_SIZE:
            {
                uint dk_id = BitConverter.ToUInt32(argbuf,0);
                uint sprw = BitConverter.ToUInt32(argbuf,4);
                uint sprh = BitConverter.ToUInt32(argbuf,8);
                uint cellw = BitConverter.ToUInt32(argbuf,12);
                uint cellh = BitConverter.ToUInt32(argbuf,16);
                HM.TileDeck dk = m_tiledeck_pool.get(dk_id);
                if(dk != null) {
                    dk.setSize( sprw, sprh, cellw, cellh );
                    Debug.Log( "received tdk_size. id:" + dk_id + " spr:" + sprw + "," + sprh + " cell:" + cellw + "," + cellh );
                }
                break;
            }
            
        case PACKETTYPE.R2C_GRID_CREATE_SNAPSHOT:
            {
                PacketGridCreateSnapshot pkt = new PacketGridCreateSnapshot();
                pkt.fromBytes(argbuf);
                HM.TileDeck deck = m_tiledeck_pool.get(pkt.tiledeck_id);
                if(deck!=null) {
                    HM.Grid g = m_grid_pool.ensure(pkt.id);
                    g.setSize( pkt.width, pkt.height );
                    g.tiledeck_id = pkt.tiledeck_id;
                    g.deck = deck;
                    g.enfat_epsilon = pkt.enfat_epsilon;
                    Debug.Log("received gr-cr-ss id:" + pkt.id + " wh:" + pkt.width + "," + pkt.height );
                } else {
                    Debug.LogWarning("grid_create_ss: tiledeck not found:" + pkt.tiledeck_id );
                }
                break;
            }
        case PACKETTYPE.R2C_GRID_TABLE_SNAPSHOT:
            {
                uint grid_id = BitConverter.ToUInt32(argbuf,0);
                uint nbytes = BitConverter.ToUInt32(argbuf,4);
                HM.Grid g = m_grid_pool.get(grid_id);
                if(g!=null) {
                    if(nbytes == g.getIndexTableSize() ) {
                        g.bulkSetIndex(argbuf,8);
                    }
                }
                break;
            }
        case PACKETTYPE.R2C_GRID_INDEX:
            Debug.LogWarning( "funcid:" + funcid );                        
            break;
        case PACKETTYPE.R2C_FILE:
            {
                int pathlen = (int) argbuf[0];
                string path = Util.getASCIIString(argbuf,1,pathlen); 
                ushort datalen = BitConverter.ToUInt16(argbuf,1+pathlen);
                byte[] data = Util.slice(argbuf,1+pathlen+2,datalen);
                Debug.Log("received FILE. pathlen:" + pathlen + " path:" + path + "datalen:" + datalen );
                m_storage.ensureFileEntry(path,data);
                break;
            }
        case PACKETTYPE.ERROR:
            Debug.LogWarning( "func ERROR! funcid:" + funcid );                        
            break;
        }
    }
    private void readCallback(IAsyncResult ar ) {
        int bytes = m_stream.EndRead(ar);
        try {
            m_ms.Write( m_readbuf, 0, bytes );
            //            Debug.Log("readCallback: input bytes:" + bytes + " msl:" + m_ms.Length );
        } catch( Exception e ) {
            Debug.Log(e);            
        }
        m_stream.BeginRead( m_readbuf, 0, m_readbuf.Length, new AsyncCallback(readCallback), null );
    }
    void Update () {
        try {
            parseStream(m_ms);
        } catch( Exception e ) {
            Debug.Log(e);
        }
    } 
    void setupClient() {
        try {
            m_cli = new TcpClient( "127.0.0.1", 23333 );
            m_stream = m_cli.GetStream();
            Debug.Log("Socket ok, stream created" );
            m_stream.BeginRead( m_readbuf, 0, m_readbuf.Length, new AsyncCallback(readCallback), null );
        } catch (Exception e) {
            Debug.Log("Socket error: " + e);
        }
    }
}
