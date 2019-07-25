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
            var drive = new GoogleDrive("skinDownloader");
            drive.Connect(@"C:\Temp\auth.json");
        }
    }
}