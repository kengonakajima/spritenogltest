using System;
using System.Collections;
using System.IO;

public class Util {
    // ofs:1なら2バイト目[1]から開始
    public static byte[] slice( byte[] src, int ofs, int len ) {
        byte[] dest = new byte[len];
        Array.Copy( src, ofs, dest, 0, len );
        return dest;
    }
    public static string getASCIIString( byte[] src, int ofs, int len ) {
        string s = System.Text.Encoding.ASCII.GetString( slice(src,ofs,len) );
        return s;
    }
}


