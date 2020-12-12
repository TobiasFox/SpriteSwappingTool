using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace SpriteSortingPlugin.Survey
{
    public class FileZipper
    {
        public bool GenerateZip(string folderPath, string outputPath)
        {
            try
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
                
                ZipFile.CreateFromDirectory(folderPath, outputPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
}