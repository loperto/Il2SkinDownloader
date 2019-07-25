using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Il2.GreatBattles
{
    public class Il2Game
    {
        private readonly string _installationPath;

        public Il2Game(string installationPath)
        {
            _installationPath = installationPath;
        }

        public IEnumerable<DirectoryInfo> GetCustomSkinDirectories()
        {
            return Directory.EnumerateDirectories(Path.Combine(_installationPath, @"data\graphics\skins")).Select(x => new DirectoryInfo(x));
        }
    }
}
