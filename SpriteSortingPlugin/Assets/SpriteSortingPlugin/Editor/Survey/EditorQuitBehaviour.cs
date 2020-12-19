using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpriteSortingPlugin.Survey
{
    [InitializeOnLoad]
    public class EditorQuitBehaviour
    {
        static EditorQuitBehaviour()
        {
            EditorApplication.quitting += Quit;
        }

        private static void Quit()
        {
            DeleteCache();
        }

        private static void DeleteCache()
        {
            var dirInfo = new DirectoryInfo(Application.temporaryCachePath);

            foreach (var file in dirInfo.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            foreach (var dir in dirInfo.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
    }
}