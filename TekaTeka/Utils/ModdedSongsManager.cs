using System.Text;
using TekaTeka.Plugins;
using Tommy;

namespace TekaTeka.Utils
{
    internal class ModdedSongsManager
    {
        private HashSet<int> currentSongs = new HashSet<int>();
        private Dictionary<int, string> uniqueIdToModName = new Dictionary<int, string>();
        private Dictionary<string, string> idToModName = new Dictionary<string, string>();
        private Dictionary<string, string> songFileToModName = new Dictionary<string, string>();
        private MusicDataInterface musicData => TaikoSingletonMonoBehaviour<DataManager>.Instance.MusicData;
        private InitialPossessionDataInterface initialPossessionData =>
            TaikoSingletonMonoBehaviour<DataManager>.Instance.InitialPossessionData;

        public class SongMod
        {
            public string modFolder;
            public string modName;
            public bool enabled;
            public List<MusicDataInterface.MusicInfo> songList = new List<MusicDataInterface.MusicInfo>();

            public SongMod(string modName)
            {
                this.modFolder = modName;
                string modPath = Path.Combine(CustomSongLoader.songsPath, this.modFolder);

                StreamReader configToml;

                if (!this.IsValidMod())
                {
                    this.enabled = false;
                    return;
                }

                TomlTable? table = null;

                try
                {
                    configToml = File.OpenText(Path.Combine(modPath, "config.toml"));

                    table = TOML.Parse(configToml);
                }
                catch (Exception e)
                {
                    if (e.GetType() != typeof(IOException))
                    {
                        Logger.Log($"'config.toml' not found in mod {modName}. Creating...", LogType.Warning);
                    }
                    else if (e.GetType() != typeof(TomlParseException))
                    {
                        Logger.Log($"'config.toml' from {modName} was not valid. Recreating...", LogType.Warning);
                    }
                }

                if (table == null)
                {
                    table = new TomlTable();
                    table.Add("enabled", true);
                    table.Add("name", modName);
                    table.Add("version", "0.1");
                    table.Add("description", "");

                    using (StreamWriter writer = File.CreateText(Path.Combine(modPath, "config.toml")))
                    {
                        table.WriteTo(writer);
                        writer.Flush();
                    }
                }

                this.enabled = table["enabled"].IsBoolean ? table["enabled"].AsBoolean.Value : true;
                this.modName = table["name"].IsString ? table["name"].AsString.Value : modName;

                if (!this.enabled)
                {
                    Logger.Log($"Mod {this.modName} explicitly disabled, skipping...", LogType.Debug);
                    return;
                }

                if (this.ReadMusicDb())
                {
                    this.enabled = true;
                };
            }

            public bool IsValidMod()
            {
                string modFolder = Path.Combine(CustomSongLoader.songsPath, this.modFolder);
                string musicDataPath = Path.Combine(modFolder, CustomSongLoader.ASSETS_FOLDER, "musicinfo");
                if (!Directory.Exists(modFolder))
                {
                    return false;
                }

                if (!File.Exists(musicDataPath + ".json") && !File.Exists(musicDataPath + ".bin"))
                {
                    return false;
                }

                return true;
            }

            public bool ReadMusicDb()
            {
                string musicDataPath = Path.Combine(CustomSongLoader.songsPath, this.modFolder,
                                                    CustomSongLoader.ASSETS_FOLDER, "musicinfo");
                string jsonString;
                if (File.Exists(musicDataPath + ".json"))
                {
                    musicDataPath += ".json";
                    jsonString = File.ReadAllText(musicDataPath);
                }
                else if (File.Exists(musicDataPath + ".bin"))
                {
                    musicDataPath += ".bin";
                    var bytes = Cryptgraphy.ReadAllAesAndGZipBytes(musicDataPath, Cryptgraphy.AesKeyType.Type2);
                    jsonString = Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    Logger.Log($"File \"musicinfo\" at {musicDataPath + "{.bin/.json}"} not present", LogType.Warning);
                    return false;
                }

                this.songList.AddRange(JsonSupports.ReadJson<MusicDataInterface.MusicInfo>(jsonString));

                return true;
            }
        }

        public ModdedSongsManager()
        {

            foreach (MusicDataInterface.MusicInfoAccesser accesser in musicData.MusicInfoAccesserList)
            {
                this.currentSongs.Add(accesser.UniqueId);
            }
            this.SetupMods();
        }

        public List<SongMod> GetMods()
        {
            List<SongMod> mods = new List<SongMod>();
            foreach (string path in Directory.GetDirectories(CustomSongLoader.songsPath))
            {
                string folder = Path.GetFileName(path) ?? "";
                if (folder != "")
                {
                    SongMod mod = new SongMod(folder);
                    if (mod.enabled)
                    {
                        Logger.Log($"Mod {mod.modFolder} Loaded", LogType.Info);
                        mods.Add(mod);
                    }
                }
            }
            return mods;
        }

        public void SetupMods()
        {
            List<SongMod> mods = this.GetMods();
            foreach (SongMod mod in mods)
            {
                if (mod.enabled)
                {
                    int songsAdded = this.AddMod(mod);
                    Logger.Log($"{songsAdded} out of {mod.songList.Count} songs were added from mod \"{mod.modName}\"");
                }
            }
        }

        public int AddMod(SongMod mod)
        {
            int songsAdded = 0;
            for (int i = 0; i < mod.songList.Count; i++)
            {
                MusicDataInterface.MusicInfo song = mod.songList[i];
                if (!this.currentSongs.Contains(song.UniqueId))
                {
                    songsAdded++;
                    this.currentSongs.Add(song.UniqueId);
                    songFileToModName.Add(song.SongFileName, mod.modFolder);
                    uniqueIdToModName.Add(song.UniqueId, mod.modFolder);
                    idToModName.Add(song.Id, mod.modFolder);

                    initialPossessionData.InitialPossessionInfoAccessers.Add(
                        new InitialPossessionDataInterface.InitialPossessionInfoAccessor(
                            (int)InitialPossessionDataInterface.RewardTypes.Song, song.UniqueId));
                    musicData.AddMusicInfo(ref song);
                }
                else
                {
#if DEBUG
                    Logger.Log($"{song.UniqueId} from {mod.modName} Skipped", LogType.Debug);
#endif
                }
            }
            return songsAdded;
        }

        public string GetModPath(int uniqueId)
        {
            if (this.uniqueIdToModName.ContainsKey(uniqueId))
            {
                return this.uniqueIdToModName[uniqueId];
            }
            else
            {
                return "";
            }
        }

        public string GetModPath(string songFileName)
        {
            if (this.songFileToModName.ContainsKey(songFileName))
            {
                return this.songFileToModName[songFileName];
            }
            else if (this.idToModName.ContainsKey(songFileName))
            {
                return this.idToModName[songFileName];
            }
            else
            {
                return "";
            }
        }
    }
}
