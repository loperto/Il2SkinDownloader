using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IL2SkinDownloader.Core.IL2
{
    public static class IL2Helpers
    {
        public static string SkinDirectoryPath(string installPath) => Path.Combine(installPath, "data", "graphics", "skins");
        public static List<string> Il2FolderNames = new List<string> {
            "IL-2 Sturmovik Battle of Stalingrad",
            "IL-2 Sturmovik Battle of Moscow",
            "IL-2 Sturmovik Battle of Kuban",
            "IL-2 Sturmovik Battle of Bodenplatte",
            "IL-2 Sturmovik Great Battles",
        };
        public static IEnumerable<DirectoryInfo> GetCustomSkinDirectories(string il2InstallPath)
        {
            return Directory
                .EnumerateDirectories(SkinDirectoryPath(il2InstallPath))
                .Select(x => new DirectoryInfo(x))
                .Where(x => !x.Name.StartsWith("_"));
        }
    }
}
