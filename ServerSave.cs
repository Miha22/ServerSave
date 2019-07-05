using System;
using System.IO;
using System.Timers;
using Rocket.API;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Logger = Rocket.Core.Logging.Logger;

namespace ServerSave
{
    public class ServerSave : RocketPlugin<ServerSaveConfiguration>
    {
        private static ServerSave Instance;
        public Timer ServerTimer { get; set; }
        public DateTime ServerNextSave { get; set; }

        protected override void Load()
        {
            Instance = this;
            ServerTimer = new Timer(SaveEveryDouble());
            ServerTimer.Start();
            ServerNextSave = NextSave();
            ServerTimer.Elapsed += Time_Elapsed;
            Logger.Log($"Timer started. Next save: {NextSave().ToShortTimeString()}");
        }
        protected override void Unload()
        {
            Logger.Log("ServerSave unloaded");
        }
        [RocketCommand("serversave", "", "", AllowedCaller.Both)]
        [RocketCommandAlias("ssave")]
        public void Execute(IRocketPlayer caller, params string[] command)
        {
            //SaveServer();
            Console.WriteLine($@"SDG.Unturned.ServerSavedata.directory: {SDG.Unturned.ServerSavedata.directory}");
            Console.WriteLine($@"Configuration.Instance.SaveWhere: {Configuration.Instance.SaveWhere}");
        }

        private void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveServer();
            ServerNextSave = NextSave();
        }
        private DateTime NextSave()
        {
            return DateTime.Now.AddSeconds(TryGetDouble(Configuration.Instance.SaveEvery.Split(':')[2])).AddMinutes(TryGetDouble(Configuration.Instance.SaveEvery.Split(':')[1])).AddHours(TryGetDouble(Configuration.Instance.SaveEvery.Split(':')[0]));
        }
        private double SaveEveryDouble()
        {
            double number = 0;
            number += TryGetDouble(Configuration.Instance.SaveEvery.Split(':')[2]);
            number += 60 * TryGetDouble(Configuration.Instance.SaveEvery.Split(':')[1]);
            number += 3600 * TryGetDouble(Configuration.Instance.SaveEvery.Split(':')[0]);

            return number;
        }
        private double TryGetDouble(string str)
        {
            if (!Double.TryParse(str, out double number))
                Logger.LogException(new InvalidCastException(), "Error in ServerSave plugin configuration {SaveEvery}! Review the input!");

            return number;
        }
        public void SaveServer()
        {
            DirectoryInfo source = new DirectoryInfo(SDG.Unturned.ServerSavedata.directory);// e.g "myServer" folder
            DirectoryInfo destination = new DirectoryInfo(Configuration.Instance.SaveWhere);// steam/common/Unturned/Saved_Servers/
            ushort count = (ushort)destination.GetDirectories().Length;// to set index to saved server
            CloneSourceDirectoryName(source, destination, count);//Save folder name e.g "steam/common/Unturned/Saved_Servers/myServer_0", "steam/common/Unturned/Saved_Servers/myServer_1", "steam/common/Unturned/Saved_Servers/myServer_2"   
            CloneDirectory(source, destination);//action
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
