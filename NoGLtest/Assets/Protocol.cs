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




public class Protocol : MonoBehaviour {
    TcpClient m_cli;
    NetworkStream m_stream;
    MemoryStream m_ms;
    byte[] m_readbuf;
    Storage m_storage;
    Pool<Image> m_image_pool;
    void Start () {
        m_storage = new Storage();        
        m_readbuf = new byte[1024*16];
        m_ms = new MemoryStream();
        m_image_pool = new Pool<Image>();
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
                Debug.Log( "record found! len:" + record_len + " fid:" + funcid );
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
        case PACKETTYPE.R2C_LAYER_VIEWPORT:
        case PACKETTYPE.R2C_LAYER_CAMERA:
        case PACKETTYPE.R2C_VIEWPORT_CREATE:
        case PACKETTYPE.R2C_VIEWPORT_SIZE:
        case PACKETTYPE.R2C_VIEWPORT_SCALE:
        case PACKETTYPE.R2C_CAMERA_CREATE:
        case PACKETTYPE.R2C_CAMERA_LOC:

        case PACKETTYPE.R2C_TEXTURE_CREATE:
        case PACKETTYPE.R2C_TEXTURE_IMAGE:
        case PACKETTYPE.R2C_IMAGE_CREATE:
            uint img_id = BitConverter.ToUInt32(argbuf,0);
            m_image_pool.ensure(img_id);
            Debug.Log( "received img_create:" + img_id);
            break;
        case PACKETTYPE.R2C_IMAGE_LOAD_PNG:
    
        case PACKETTYPE.R2C_TILEDECK_CREATE:
        case PACKETTYPE.R2C_TILEDECK_TEXTURE:
        case PACKETTYPE.R2C_TILEDECK_SIZE:
        case PACKETTYPE.R2C_GRID_CREATE_SNAPSHOT:
        case PACKETTYPE.R2C_GRID_TABLE_SNAPSHOT: 
        case PACKETTYPE.R2C_GRID_INDEX:
            break;
        case PACKETTYPE.R2C_FILE:
            int pathlen = (int) argbuf[0];
            string path = System.Text.Encoding.ASCII.GetString(Util.slice(argbuf,1,pathlen));
            ushort datalen = BitConverter.ToUInt16(argbuf,1+pathlen);
            byte[] data = Util.slice(argbuf,1+pathlen+2,datalen);
            Debug.Log("received FILE. pathlen:" + pathlen + " path:" + path + "datalen:" + datalen );
            m_storage.ensureFileEntry(path,data);
            break;
        case PACKETTYPE.ERROR:
            break;
        }
    }
    private void readCallback(IAsyncResult ar ) {
        int bytes = m_stream.EndRead(ar);
        try {
            m_ms.Write( m_readbuf, 0, bytes );
            //            Debug.Log("readCallback: input bytes:" + bytes + " msl:" + m_ms.Length );
            parseStream(m_ms);
        } catch( Exception e ) {
            Debug.Log(e);            
        }
        m_stream.BeginRead( m_readbuf, 0, m_readbuf.Length, new AsyncCallback(readCallback), null );
    }
    void Update () {

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
