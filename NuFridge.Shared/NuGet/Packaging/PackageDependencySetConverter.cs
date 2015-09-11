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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;

namespace NuFridge.Shared.NuGet.Packaging
{
    public static class PackageDependencySetConverter
    {
        public static IEnumerable<string> Flatten(PackageDependencySet set)
        {
            var shortFrameworkName = set.TargetFramework == null ? null : VersionUtility.GetShortFrameworkName(set.TargetFramework);

            if (shortFrameworkName != null && set.Dependencies.Count == 0)
            {
                return new[] { "::" + shortFrameworkName };
            }

            return set.Dependencies.Select(d => $"{d.Id}:{d.VersionSpec}:{shortFrameworkName}");
        }

        public static IEnumerable<PackageDependencySet> Parse(IEnumerable<string> dependencies)
        {
            var map = new Dictionary<string, List<PackageDependency>>();

            foreach (var str in dependencies)
            {
                var parts = str.Split(':');
                var key = parts.Length >= 3 ? parts[2] : string.Empty;

                List<PackageDependency> set;
                if (!map.TryGetValue(key, out set))
                {
                    set = new List<PackageDependency>();
                    map[key] = set;
                }

                var id = parts[0];
                var version = parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1])
                                  ? VersionUtility.ParseVersionSpec(parts[1])
                                  : null;

                if (string.IsNullOrWhiteSpace(id)) continue;

                set.Add(new PackageDependency(id, version));
            }

            return map.Select(kv => new PackageDependencySet(ParseFrameworkName(kv.Key), kv.Value));
        }

        private static FrameworkName ParseFrameworkName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            return VersionUtility.ParseFrameworkName(name);
        }
    }
}
