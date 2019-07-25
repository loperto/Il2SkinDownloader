using System;
using System.Threading;
using System.Threading.Tasks;
using Il2.GreatBattles;
using Il2.RemoteDrive.GoogleDrive;
using NUnit.Framework;

namespace Il2.SkinDownloader.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var il2 = new Il2Game(@"Z:\Steam\steamapps\common\IL-2 Sturmovik Battle of Stalingrad");
            var planes = il2.GetCustomSkinDirectories();
            foreach (var plane in planes)
            {
                Console.WriteLine(plane.Name);
            }
        }

        [Test]
        public async Task Test2()
        {
            var drive = new GoogleDrive("skinDownloader");
            drive.Connect(@"C:\Temp\auth.json");
            var files = await drive.GetFiles(CancellationToken.None);
            foreach (var googleDriveItem in files)
            {
                Console.WriteLine(googleDriveItem);
            }

        }
    }
}