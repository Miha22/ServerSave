using System;
using System.IO;
using Rocket.API;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Logger = Rocket.Core.Logging.Logger;

namespace ServerSave
{
    public class ServerSave : RocketPlugin<ServerSaveConfiguration>
    {
        private static ServerSave Instance;
        private static DateTime LastSave;
        protected override void Load()
        {
            ServerSave.Instance = this;
            Logger.Log($"ServerSave plugin loaded. Server will be save every [hh:mm:ss]: {Configuration.Instance.SaveEvery}");
        }
        protected override void Unload()
        {
            Logger.Log("ServerSave unloaded");
        }
        [RocketCommand("serversave", "", "", AllowedCaller.Both)]
        [RocketCommandAlias("ssave")]
        public void Execute(IRocketPlayer caller, params string[] command)
        {
            SaveServer();
        }
        private void SaveServer()
        {
            LastSave = DateTime.Now;
            DirectoryInfo source = new DirectoryInfo(SDG.Unturned.ServerSavedata.directory);// e.g "myServer" folder
            DirectoryInfo destination = new DirectoryInfo(Configuration.Instance.SaveWhere);// steam/common/Unturned/Saved_Servers/
            ushort count = (ushort)destination.GetDirectories().Length;// to set index to saved server
            CloneSourceDirectoryName(source, destination, count);//Save folder name e.g "steam/common/Unturned/Saved_Servers/myServer_0", "steam/common/Unturned/Saved_Servers/myServer_1", "steam/common/Unturned/Saved_Servers/myServer_2"   
            CloneDirectory(source, destination);//action
            //DirectoryInfo dirDynamicSource = new DirectoryInfo(SDG.Unturned.ServerSavedata.directory);
            //DirectoryInfo dirDynamicDestination = new DirectoryInfo(Configuration.Instance.SaveWhere);
           
        }
        private void CloneDirectory(DirectoryInfo dirDynamicSource, DirectoryInfo dirDynamicDestination)// (../steam/Unturned/Servers/myServer, ../steam/Unturned/Saved_Servers/myServer_[index])
        {
            foreach (var file in dirDynamicSource.GetFiles())
            {
                file.CopyTo($@"{dirDynamicDestination.FullName}/{file.Name}");
            }
            while (dirDynamicSource.GetDirectories() != null)
            {
                foreach (var dir in dirDynamicSource.GetDirectories())
                {
                    dirDynamicDestination = new DirectoryInfo($@"{dirDynamicDestination.FullName}/{dir.Name}");
                    CloneSourceDirectoryName(dirDynamicDestination.FullName);
                    foreach (var file in dir.GetFiles())
                    {
                        file.CopyTo($@"{dirDynamicDestination.FullName}/{file.Name}");
                    }
                    CloneDirectory(dir, dirDynamicDestination);
                }
            }
        }
        private void CloneSourceDirectoryName(string destination)
        {
            System.IO.Directory.CreateDirectory($@"{destination}");
        }
        private void CloneSourceDirectoryName(DirectoryInfo source, DirectoryInfo destination, ushort counter)
        {
            System.IO.Directory.CreateDirectory($@"{destination.FullName}/{source.Name + "_" + counter.ToString()}");
        }
    }
}
