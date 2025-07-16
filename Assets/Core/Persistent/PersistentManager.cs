using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using UnityEngine;

 

/// <summary>
/// 持久化存档管理模块
/// </summary>
public class PersistentManager : Singleton<PersistentManager>
{
    const string SaveName =  "PlayerData_{0}.save";
    public string persistentDataPath => Application.persistentDataPath + "/../saves/";
    string playerSavePathName = string.Empty;

    public void Init()
    {
        playerSavePathName =  persistentDataPath + SaveName;
    }

    string GetCurrSavaPath(int keyIndex)
    {
        var path =  string.Format(playerSavePathName, keyIndex);
        return path; 
    }
    
    const string KeyPlayer = "KeyPlayerData";
    const string m_commonName = "mmjh2023";
    string GetPlayerDataSaveKey(int keyIndex)
    {
        return string.Format("{0}{1}{2}", KeyPlayer, m_commonName, keyIndex);

    }

    /// <summary>
    /// 删除持久化文件
    /// </summary>
    /// <param name="saveKey"></param>
    /// <returns></returns>
    public bool PersistentDelete(int keyIndex)
    {
        try
        {
            string path = GetCurrSavaPath(keyIndex);
              if (!File.Exists(path))
            {
                return false;
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                 using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    string saveKey = GetPlayerDataSaveKey(keyIndex);
                    if(saveKey != binaryReader.ReadString()) 
                    {
                        Debug.LogError($"PersistentDelete  error, saveKey :'{saveKey}'.");  
                        return false; 
                    }
                }
            }
            File.Delete(path);
            return true;
        }      
        catch (Exception exception)
        {
            Debug.LogError($"PersistentDelete with exception '{exception}'.");
        } 
        return false;
    }

    bool PersistentWirte(int keyIndex, string saveData)
    {

        try
        {
            string path = GetCurrSavaPath(keyIndex);
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    string saveKey = GetPlayerDataSaveKey(keyIndex);
                    if(saveKey != null) 
                    {
                        binaryWriter.Write(saveKey);
                        binaryWriter.Write(saveData);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Save PlayerData with exception '{exception}'.");
            return false;
        }
        return true;
    }

    bool PersistentWirteByte(int keyIndex, byte[] saveData)
    {
 
        try
        {
            string path = GetCurrSavaPath(keyIndex);
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    string saveKey = GetPlayerDataSaveKey(keyIndex);
                    if(saveKey != null) 
                    {
                        binaryWriter.Write(saveKey);
                        binaryWriter.Write7BitEncodedInt32(saveData.Length);
                        binaryWriter.Write(saveData);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Save PlayerData with exception '{exception}'.");
            return false;
        }
        return true;
    }

    bool PersistentWirteByte(string path, byte[] saveData, bool verify)
    {
        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    if(verify) binaryWriter.Write7BitEncodedInt32(saveData.Length);
                    binaryWriter.Write(saveData);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"PersistentWirteByte with exception '{exception}'.");
            return false;
        }
        return true;
    }

    string? PersistentRead(int keyIndex)
    {
        try
        {
            string path = GetCurrSavaPath(keyIndex);
            if (!File.Exists(path))
            {
                Debug.LogError($"Load PlayerData path error :'{path}'.");
                return null;
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                 using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    string saveKey = GetPlayerDataSaveKey(keyIndex);
                    var _key =  binaryReader.ReadString();
                    if(!saveKey.Equals(_key, StringComparison.Ordinal)) 
                    {
                        Debug.LogError($"Load PlayerData saveKey error :'{saveKey}/{_key}'.");
                    } 
                    return  binaryReader.ReadString();
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Load PlayerData with exception '{exception}'.");
        } 
        return null;
    }
    byte[]? PersistentReadByte(int keyIndex)
    {
        try
        {
            string path = GetCurrSavaPath(keyIndex);
            if (!File.Exists(path))
            {
                Debug.LogError($"Load PlayerData path error :'{path}'.");
                return null;
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                 using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    string saveKey = GetPlayerDataSaveKey(keyIndex);
                    var _key =  binaryReader.ReadString();
                    if(!saveKey.Equals(_key, StringComparison.Ordinal)) 
                    {
                        Debug.LogError($"Load PlayerData saveKey error :'{saveKey}/{_key}'.");
                        //return null; 允许读取不匹配的档案
                    }
                    var length = binaryReader.Read7BitEncodedInt32();
                    return  binaryReader.ReadBytes(length);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Load PlayerData with exception '{exception}'.");
        } 
        return null;
    }
    byte[]? PersistentReadByte(string path, bool verify)
    {
        try
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"PersistentReadByte path error :'{path}'.");
                return null;
            }

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                 using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    int length ;
                    if(verify)
                    {
                        length = binaryReader.Read7BitEncodedInt32();
                    }else
                    {
                        length =  (int)(fileStream.Length - fileStream.Position);
                    }
                    
                    return  binaryReader.ReadBytes(length);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"PersistentReadByte with exception '{exception}'.");
        } 
        return null;
    }

    // public T  GetObject<T>(string saveKey)
    // {
    //     var json = PersistentRead(saveKey);
    //     T obj = JsonConvert.DeserializeObject<T>(json);
    //     return obj;
    // }
    
    // public bool  SetObject<T>(string saveKey, T obj)
    // {
    //     var json = JsonConvert.SerializeObject(obj);
        
    //     return PersistentWirte(saveKey, json);
    // }

    /// <summary>
    /// 从持久化数据取出对象（需解压缩）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keyIndex"></param>
    /// <returns></returns>
    public T?  GetObject<T>(int keyIndex, Encoding encoding, out bool succ)
    {
        succ = false;
        byte[]? strBytesCompressed = PersistentReadByte(keyIndex);
        if(strBytesCompressed == null) return default;
        
        T? obj ;
        try{
            byte[] strBytes = Deflate.DecompressBytes(strBytesCompressed);
            string json = encoding.GetString(strBytes);
            obj = JsonSerializer.Deserialize<T>(json);
        }      
        catch (Exception exception)
        {
            Debug.LogError($"GetObject with exception '{exception.Message}'.");
            return default;
        } 

        succ = true;
        return obj;
    }
    
    /// <summary>
    /// 从持久化数据取出对象（需解压缩）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="encoding"></param>
    /// <param name="succ"></param>
    /// <param name="compress">压缩加密</param>
    /// <param name="verify">审核加密</param>
    /// <returns></returns>
    public T?  GetObject<T>(string path, Encoding encoding, out bool succ, bool compress = true, bool verify = true)
    {
        succ = false;

        byte[]? strBytes = PersistentReadByte(path, verify);
        if(strBytes == null) return default;
        
        T? obj ;
        try{
            byte[] strBytesUnCompressed;
            if(compress)
            {
                strBytesUnCompressed = Deflate.DecompressBytes(strBytes);
            }else
            {
                strBytesUnCompressed = strBytes;
            }
            
            string json = encoding.GetString(strBytesUnCompressed);
            obj = JsonSerializer.Deserialize<T>(json);
        }      
        catch (Exception exception)
        {
            Debug.LogError($"GetObject with exception '{exception.Message}'.");
            return default;
        } 

        succ = true;
        return obj;
    }
    

    /// <summary>
    /// 持久化对象 （并进行压缩）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keyIndex"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool  SetObject<T>(int keyIndex, T obj)
    {

        try{
           string json = JsonSerializer.Serialize(obj);
        
            byte[] strBytes = Encoding.UTF8.GetBytes(json);
            byte[] strBytesCompressed = Deflate.CompressBytes(strBytes);
            string path = GetCurrSavaPath(keyIndex);
            return PersistentWirteByte(keyIndex, strBytesCompressed);
        }      
        catch (Exception exception)
        {
            Debug.LogError($"SetObject with exception '{exception.InnerException}'：'{exception.Message}.");
            return false;
        } 

    }

    /// <summary>
    /// 持久化对象 （并进行压缩）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <param name="compress">压缩加密</param>
    /// <param name="verify">审核加密</param>
    /// <returns></returns>
    public bool  SetObject<T>(string path, T obj, bool compress = true, bool verify = true)
    {

        try{
           string json = JsonSerializer.Serialize(obj);
        
            byte[] strBytes = Encoding.UTF8.GetBytes(json);
            if(compress)
            {
                byte[] strBytesCompressed = Deflate.CompressBytes(strBytes);
                return PersistentWirteByte(path, strBytesCompressed, verify);
            }
            return PersistentWirteByte(path, strBytes, verify);
        }      
        catch (Exception exception)
        {
            Debug.LogError($"SetObject with exception '{exception.InnerException}'：'{exception.Message}.");
            return false;
        } 

    }

    /// <summary>
    /// 加密文件到目标位置
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public bool  EncryptFile(string source, string dest)
    {

        try{
            //加载未加密byte
            byte[]? strBytes = PersistentReadByte(source, false);
            if(strBytes == null) return false;

            //压缩
            byte[] strBytesCompressed = Deflate.CompressBytes(strBytes);

            //写入新文件
            return PersistentWirteByte(dest, strBytesCompressed, true);
 
        }      
        catch (Exception exception)
        {
            Debug.LogError($"EncryptFile with exception '{exception.InnerException}'：'{exception.Message}.");
            return false;
        } 

    }

    /// <summary>
    /// 解析加密文件为对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public T?  DeEncryptObject<T>(string source)
    {

        try{
            //加载加密byte
            byte[]? strBytesCompressed = PersistentReadByte(source, true);
            if(strBytesCompressed == null) return default;

            //解压缩
            byte[] strBytes = Deflate.DecompressBytes(strBytesCompressed);

            using (MemoryStream ms = new MemoryStream(strBytes))
            {
                IFormatter iFormatter = new BinaryFormatter();
                return (T)iFormatter.Deserialize(ms);
            }
 
        }      
        catch (Exception exception)
        {
            Debug.LogError($"DeEncryptObject with exception '{exception.InnerException}'：'{exception.Message}.");
            return default;
        } 

    }

    /// <summary>
    /// 解析加密文件为byte
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public byte[]?  DeEncryptFile(string source)
    {

        try{
            //加载加密byte
            byte[]? strBytesCompressed = PersistentReadByte(source, true);
            if(strBytesCompressed == null) return default;

            //解压缩
            byte[] strBytes = Deflate.DecompressBytes(strBytesCompressed);

            return strBytes;
 
        }      
        catch (Exception exception)
        {
            Debug.LogError($"DeEncryptObject with exception '{exception.InnerException}'：'{exception.Message}.");
            return default;
        } 

    }

    /// <summary>
    /// 序列化到 支持steam 云储存和同步的目标文件
    /// </summary>
    public void SaveToJson<T>(T obj, string fileName)
    {
        try
        {
            var savaPath = $"{persistentDataPath}{fileName}.save";
            string json = JsonSerializer.Serialize(obj);
            byte[] strBytes = Encoding.UTF8.GetBytes(json);
            PersistentWirteByte(savaPath, strBytes, false); 
        }      
        catch (Exception exception)
        {
            Debug.LogError($"SaveToJson with exception '{exception.InnerException}'：'{exception.Message}.");
            return;
        } 
        
    }
    
    /// <summary>
    /// 反序列化 从支持steam 云储存和同步的目标文件
    /// </summary>
    public T? LoadFromJson<T>(string fileName)
    {
        try
        {
            var savaPath = $"{persistentDataPath}{fileName}.save";
            if (!File.Exists(savaPath)) return default;
            byte[]? strBytes = PersistentReadByte(savaPath,  false); 
            string json = System.Text.Encoding.UTF8.GetString(strBytes);
            var obj = JsonSerializer.Deserialize<T>(json);
            return obj;
        }      
        catch (Exception exception)
        {
            Debug.LogError($"LoadFromJson with exception '{exception.InnerException}'：'{exception.Message}.");
            return default;
        } 
        
    }


}
