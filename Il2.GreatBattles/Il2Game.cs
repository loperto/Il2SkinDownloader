using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Il2.GreatBattles
{
    public class Il2Game
    {
        private readonly string _installationPath;
        public string SkinDirectoryPath => Path.Combine(_installationPath, "data", "graphics", "skins");
        public static List<string> Il2FolderNames = new List<string> {
            "IL-2 Sturmovik Battle of Stalingrad",
            "IL-2 Sturmovik Battle of Moscow",
            "IL-2 Sturmovik Battle of Kuban",
            "IL-2 Sturmovik Battle of Bodenplatte",
        };

        public Il2Game(string installationPath)
        {
            _installationPath = installationPath;
        }
        public IEnumerable<DirectoryInfo> GetCustomSkinDirectories()
        {
            return Directory
                .EnumerateDirectories(SkinDirectoryPath)
                .Select(x => new DirectoryInfo(x))
                .Where(x => !x.Name.StartsWith("_"));
        }
    }
}
