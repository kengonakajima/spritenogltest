using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class FileEntry {
    string m_path;
    byte[] m_body;
    public FileEntry( string path, byte[] body ) {
        m_path = String.Copy(path);
        m_body = new byte[body.Length];
        Array.Copy( body, 0, m_body, 0, body.Length );
    }
    public bool equalPath( string path ) {
        return ( m_path.Equals(path) );
    }
    public byte[] getBody() {
        return m_body;
    }
};

public class Storage {
    const int MAX_FILEENTRY = 32;
    FileEntry[] m_fents;
    public Storage() {
        m_fents = new FileEntry[MAX_FILEENTRY];
    }
    public FileEntry findFileEntry( string path ) {
        for(int i=0;i<MAX_FILEENTRY;i++) {
            if( m_fents[i] != null && m_fents[i].equalPath(path) ) {
                return m_fents[i];
            }
        }
        return null;
    }
    public FileEntry ensureFileEntry( string path, byte[] data ) {
        FileEntry fe = findFileEntry(path);
        if(fe!=null) {
            Debug.Log( "ensureFileEntry: found entry:" + path );
            return fe;
        }
        for(int i=0;i<m_fents.Length;i++) {
            if( m_fents[i] == null ) {
                Debug.Log("allocated new fileentry:" + path + " len:" + data.Length + " at:" + i );
                fe = new FileEntry(path, data);
                m_fents[i] = fe;
                return fe;
            }
        }
        Debug.Log( "ensureFileEntry: full!");
        return null;
    }
};
