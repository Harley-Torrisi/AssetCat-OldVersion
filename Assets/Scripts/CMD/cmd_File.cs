using System;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

public class cmd_File : MonoBehaviour {

    ///// <summary>
    ///// Replaces / and \ with current system format.
    ///// </summary>
    ///// <param name="dirPath"></param>
    ///// <returns></returns>
    //public static string ConvertDirectorySeperators(string systemPath)
    //{
    //    switch (cmd_Application.GetOperatingPlatform())
    //    {
    //        case cmd_Application.OperatingPlatforms.Windows:
    //            return systemPath.Replace('/', '\\');
    //        case cmd_Application.OperatingPlatforms.OSX:
    //            return systemPath.Replace('\\', '/');
    //        default:
    //            return systemPath.Replace('/', '\\');
    //    }
    //}

    /// <summary>
    /// Returns the file path and name combined.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetFullFilePath(string filePath, string fileName)
    {
        return filePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + fileName;
    }

    /// <summary>
    /// Checks if file already exists.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool FileExists(string filePath, string fileName)
    {
        return File.Exists(GetFullFilePath(filePath, fileName));
    }

    /// <summary>
    /// Checks if directory conains anf files or folders.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public static bool IsFolderEmpty(string folderPath)
    {
        return !(Directory.GetFiles(folderPath).Length > 0 || Directory.GetDirectories(folderPath).Length > 0);
    }

    /// <summary>
    /// Attempts to delete folder, returns TRUE if operation succeeded.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public static bool DeleteFolder(string folderPath, bool logError)
    {
        try
        {
            Directory.Delete(folderPath, true);
            return true;
        }
        catch (Exception e)
        {
            if (logError)
                Debug.Log("DeleteFolder() Failed: " + e);
            return false;
        }
    }

    /// <summary>
    /// Creates temp file in directory, returns TRUE if operation succeeded.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="throwIfFails"></param>
    /// <returns></returns>
    public static bool IsDirectoryWritable(string folderPath, bool throwIfFails = false)
    {
        try
        {
            using (FileStream fs = File.Create(Path.Combine(folderPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
            return true;
        }
        catch
        {
            if (throwIfFails)
                throw;
            else
                return false;
        }
    }

    /// <summary>
    /// Save object to hard disk.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="obj"></param>
    /// <param name="fileMode"></param>
    /// <param name="fileAccess"></param>
    public static void SerializeObject(string filePath, string fileName, object obj, FileMode fileMode = FileMode.Create, FileAccess fileAccess = FileAccess.ReadWrite)
    {
        string fullPath = GetFullFilePath(filePath, fileName);
        IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        Stream stream = new FileStream(fullPath, fileMode, fileAccess);
        formatter.Serialize(stream, obj);
        stream.Close();
    }

    /// <summary>
    /// Load object from hgard disk.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    /// <param name="fileMode"></param>
    /// <param name="fileAccess"></param>
    /// <returns></returns>
    public static object DeserializeObject(string filePath, string fileName, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read)
    {
        string fullPath = GetFullFilePath(filePath, fileName);
        IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        Stream stream = new FileStream(fullPath, fileMode, fileAccess);
        object returnOBJ = formatter.Deserialize(stream);
        stream.Close();
        return returnOBJ;
    }
}
