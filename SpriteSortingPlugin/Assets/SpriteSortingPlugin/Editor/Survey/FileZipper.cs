using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace SpriteSortingPlugin.Survey
{
    public class FileZipper
    {
        public bool GenerateZip(string folderPath, string outputPath, out string adjustedOutputPath)
        {
            if (File.Exists(outputPath))
            {
                outputPath = GenerateUniqueName(outputPath);
            }

            adjustedOutputPath = outputPath;

            try
            {
                ZipFile.CreateFromDirectory(folderPath, outputPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private string GenerateUniqueName(string outputPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(outputPath);
            var baseDir = Path.GetDirectoryName(outputPath);

            var uniqueName = fileName + "_" + Guid.NewGuid();
            var extension = Path.GetExtension(outputPath);

            var uniqueFileName = $"{baseDir}{Path.DirectorySeparatorChar}{uniqueName}{extension}";

            return uniqueFileName;
        }
    }
}