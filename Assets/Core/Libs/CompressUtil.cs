using System.IO;
using System.IO.Compression;
using System.Text;

/// <summary>  
/// 简单高效的压缩  （C#用原生DeflateStream，只支持桌面平台）
/// </summary>  
public static class Deflate  
{  
    public static byte[] CompressString(string str)  
    {  
        return CompressBytes(Encoding.UTF8.GetBytes(str));  
    }  

 
    public static byte[] CompressBytes(byte[] str)  
    {  
        var ms = new MemoryStream(str) {Position = 0};  
        var outms = new MemoryStream();  
        using (var deflateStream = new DeflateStream(outms, CompressionMode.Compress, true))  
        {  
            var buf = new byte[1024];  
            int len;  
            while ((len = ms.Read(buf, 0, buf.Length)) > 0)  
                deflateStream.Write(buf, 0, len);  
        }  
        return outms.ToArray();  
    }  
  
    public static string DecompressString(byte[] str)  
    {  
        return Encoding.UTF8.GetString(DecompressBytes(str));  
    }  
    

    public static byte[] DecompressBytes(byte[] str)  
    {  
        var ms = new MemoryStream(str) {Position = 0};  
        var outms = new MemoryStream();  
        using (var deflateStream = new DeflateStream(ms, CompressionMode.Decompress, true))  
        {  
            var buf = new byte[1024];  
            int len;  
            while ((len = deflateStream.Read(buf, 0, buf.Length)) > 0)  
                outms.Write(buf, 0, len);  
        }  
        return outms.ToArray();  
    }  
}


