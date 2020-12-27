#region license

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing,
//  software distributed under the License is distributed on an
//  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//  KIND, either express or implied.  See the License for the
//  specific language governing permissions and limitations
//   under the License.
//  -------------------------------------------------------------

#endregion

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