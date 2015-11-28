using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;

public class Protocol : MonoBehaviour {
    TcpClient m_cli;
    NetworkStream m_stream;
    
    void Start () {
        setupClient();
    }
    private void readCallback(IAsyncResult ar ) {
        int bytes = m_stream.EndRead(ar);
        Debug.Log("readCallback: bytes:" + bytes );
    }
    void Update () {
        byte[] readbuf = new byte[1024];
        IAsyncResult ar = m_stream.BeginRead( readbuf, 0, readbuf.Length, new AsyncCallback(readCallback), null );
    } 
    void setupClient() {
        try {
            m_cli = new TcpClient( "127.0.0.1", 23333 );
            m_stream = m_cli.GetStream();
            Debug.Log("Socket ok, stream created" );
        } catch (Exception e) {
            Debug.Log("Socket error: " + e);
        }
    }
}
