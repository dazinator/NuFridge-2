using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet;

namespace NuFridge.Shared.Server.NuGet.Symbols
{
    public class SymbolSourceMapper
    {
        public string FindSourceFile(string referencedSource, ISet<string> sourceFiles)
        {
            var parts = referencedSource.Split('/', '\\');
            var c = 0;
            var i = parts[0].Length + 1;

            while (++c < parts.Length)
            {
                var relPath = referencedSource.Substring(i);
                if (sourceFiles.Contains(relPath, StringComparer.InvariantCultureIgnoreCase))
                {
                    return relPath;
                }
                i += parts[c].Length + 1;
            }

            return string.Empty;
        }

        public string CreateSourceMappingIndex(IPackageName package, string symbolSourceUri, List<string> referencedSources, ISet<string> sourceFiles)
        {
            var version = package.Version;
            var packageId = package.Id;

            var sb = new StringBuilder();

            sb.AppendLine("SRCSRV: ini ------------------------------------------------");
            sb.AppendLine("VERSION=2");
            sb.AppendLine("INDEXVERSION=2");
            sb.AppendLine("VERCTRL=NuGet");
            sb.AppendFormat("DATETIME={0}" + Environment.NewLine, DateTime.UtcNow);
            sb.AppendLine("SRCSRV: variables ------------------------------------------");
            sb.AppendLine("SRCSRVVERCTRL=http");
            sb.AppendFormat("NUGET={0}" + Environment.NewLine, symbolSourceUri);
            sb.AppendLine("HTTP_EXTRACT_TARGET=%NUGET%/%var4%/%var2%/%var5%");
            sb.AppendLine("SRCSRVTRG=%HTTP_EXTRACT_TARGET%");
            sb.AppendLine("SRCSRVCMD=");
            sb.AppendLine("SRCSRV: source files ---------------------------------------");

            foreach (var source in referencedSources)
            {
                var relativePath = FindSourceFile(source, sourceFiles);
                if (string.IsNullOrWhiteSpace(relativePath)) continue;
                sb.AppendFormat("{0}*{1}*_*{2}*{3}" + Environment.NewLine, source, version, packageId, relativePath);
            }

            sb.AppendLine("SRCSRV: end ------------------------------------------------");

            return sb.ToString();
        }
    }
}
