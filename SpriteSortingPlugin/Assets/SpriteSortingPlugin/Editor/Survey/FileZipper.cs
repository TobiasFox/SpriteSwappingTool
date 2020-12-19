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
            if (string.IsNullOrEmpty(outputPath))
            {
                return false;
            }

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
            if (string.IsNullOrEmpty(outputPath))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(outputPath);
            var baseDir = Path.GetDirectoryName(outputPath);

            if (string.IsNullOrEmpty(baseDir))
            {
                return null;
            }

            var uniqueName = fileName + "_" + Guid.NewGuid();
            var extension = Path.GetExtension(outputPath);

            var uniqueFileName = Path.Combine(baseDir, uniqueName + extension);

            return uniqueFileName;
        }
    }
}