using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;

public class Protocol : MonoBehaviour {
    TcpClient m_cli;
    NetworkStream m_stream;
    MemoryStream m_ms;
    byte[] m_readbuf;
    void Start () {
        m_readbuf = new byte[1024*16];
        m_ms = new MemoryStream();
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
                shiftMemoryStream(ms,record_len+2);
            }
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
