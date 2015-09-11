/* 
* @author themotleyfool https://github.com/themotleyfool/NuGet.Lucene
* Apache License
* Version 2.0, January 2004
* http://www.apache.org/licenses/
*
* Copyright 2008-2012 by themotleyfool
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;

namespace NuFridge.Shared.NuGet.Symbols
{
    public class SymbolTools
    {
        private readonly string _toolsPath;

        public bool Enabled => !string.IsNullOrWhiteSpace(_toolsPath) && Directory.Exists(_toolsPath);

        public SymbolTools(string toolsPath)
        {
            _toolsPath = toolsPath;
        }

        public IEnumerable<string> GetSources(string symbolFile)
        {
            var output = ExecuteTool(@"srctool", "-r \"" + symbolFile + "\"");

            return output.Where(line => line.Trim() != "" && !line.ToLowerInvariant().Contains(symbolFile.ToLowerInvariant()));
        }

        public void MapSources(string symbolFile, string sourceMappingIndexContent)
        {
            var indexFile = Path.ChangeExtension(symbolFile, ".index");

            try
            {
                using (var writer = File.CreateText(indexFile))
                {
                    writer.Write(sourceMappingIndexContent);
                }

                ExecuteTool("pdbstr", $" -w -p:\"{symbolFile}\" -i:\"{indexFile}\" -s:srcsrv");
            }
            finally
            {
                File.Delete(indexFile);
            }
        }

        public void IndexSymbolFile(IPackageName package, string symbolPath,  string symbolFile)
        {
            ExecuteTool("symstore", $" add /f \"{symbolFile}\" /s \"{symbolPath}\" /t {package.Id} /v {package.Version}");
        }

        private IEnumerable<string> ExecuteTool(string tool, string arguments)
        {
            if (string.IsNullOrWhiteSpace(_toolsPath))
            {
                throw new InvalidOperationException("Cannot process symbol packages without setting path to Debugging Tools for Windows.");
            }

            var exePath = GetToolPath(tool);

            if (!File.Exists(exePath))
            {
                throw new IOException("Cannot locate " + tool + " in " + _toolsPath);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(exePath, arguments)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            var tcs = new TaskCompletionSource<int>();

            using (process)
            {
                process.Exited += (s, e) => tcs.SetResult(process.ExitCode);
                process.EnableRaisingEvents = true;
                process.Start();

                var stdout = process.StandardOutput.ReadToEndAsync();

                Task.WhenAll(tcs.Task, stdout).Wait();

                return stdout.Result.Replace("\r\n", "\n").Split('\n');
            }
        }

        private string GetToolPath(string tool)
        {
            var exePath = Path.Combine(_toolsPath, tool + ".exe");

            if (!File.Exists(exePath))
            {
                exePath = Path.Combine(Path.Combine(_toolsPath, "srcsrv"), tool + ".exe");
            }
            return exePath;
        }
    }
}
