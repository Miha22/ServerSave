using System.IO;
using Rocket.API;
namespace ServerSave
{
    public class ServerSaveConfiguration : IRocketPluginConfiguration
    {
        public string SaveEvery_Template= "hh:mm:ss";
        public string SaveEvery;
        public string SaveWhere;
        public void LoadDefaults()
        {
            string path = $@"../{SDG.Unturned.ServerSavedata.directory}/Saved_Servers";
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
                directory.Create();
            if (directory.Attributes == FileAttributes.ReadOnly)
                directory.Attributes &= ~FileAttributes.ReadOnly;
            SaveWhere = directory.FullName;
            SaveEvery = "00:05:00";
        }
    }
}