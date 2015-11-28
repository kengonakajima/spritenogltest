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
    void parseStream( MemoryStream ms ) {
        byte[] b = ms.GetBuffer();
        Debug.Log( "blen:" + b.Length );
    }
    private void readCallback(IAsyncResult ar ) {
        int bytes = m_stream.EndRead(ar);
        Debug.Log("readCallback:"+bytes);
        try {
            m_ms.Write( m_readbuf, 0, bytes );
            Debug.Log( "ms:" + m_ms.Position );
            parseStream(m_ms);
            Debug.Log( "ms after read:" + m_ms.Position );            
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
