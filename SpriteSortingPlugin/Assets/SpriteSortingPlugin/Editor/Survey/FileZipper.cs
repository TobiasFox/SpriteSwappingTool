using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;

namespace SpriteSortingPlugin.Survey
{
    public class FileZipper
    {
        private const float FileInUseTimeout = 3000;

        public bool GenerateZip(string folderPath, string outputPath)
        {
            var isTimeout = CheckIfFileIsAlreadyInUse(outputPath);
            if (isTimeout)
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

        private bool CheckIfFileIsAlreadyInUse(string outputPath)
        {
            var startTime = DateTime.Now;

            while (true)
            {
                try
                {
                    if ((DateTime.Now - startTime).TotalMilliseconds > FileInUseTimeout)
                    {
                        return true;
                    }

                    if (File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }

                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(200);
                }
            }

            return false;
        }
    }
}