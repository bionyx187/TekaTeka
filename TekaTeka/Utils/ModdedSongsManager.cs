using System.Text;
using TekaTeka.Plugins;

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
            public string modName;
            public bool enabled;
            public List<MusicDataInterface.MusicInfo> songList = new List<MusicDataInterface.MusicInfo>();

            public SongMod(string modName)
            {
                this.modName = modName;
                if (!this.IsValidMod())
                {
                    this.enabled = false;
                    return;
                }

                if (this.ReadMusicDb())
                {
                    this.enabled = true;
                };
            }

            public bool IsValidMod()
            {
                string modFolder = Path.Combine(CustomSongLoader.songsPath, this.modName);
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
                string musicDataPath =
                    Path.Combine(CustomSongLoader.songsPath, this.modName, CustomSongLoader.ASSETS_FOLDER, "musicinfo");
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
                        Logger.Log($"Mod {mod.modName} Loaded", LogType.Info);
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
                this.AddMod(mod);
            }
        }

        public void AddMod(SongMod mod)
        {
            for (int i = 0; i < mod.songList.Count; i++)
            {
                MusicDataInterface.MusicInfo song = mod.songList[i];
                if (!this.currentSongs.Contains(song.UniqueId))
                {
                    this.currentSongs.Add(song.UniqueId);
                    songFileToModName.Add(song.SongFileName, mod.modName);
                    uniqueIdToModName.Add(song.UniqueId, mod.modName);
                    idToModName.Add(song.Id, mod.modName);

                    initialPossessionData.InitialPossessionInfoAccessers.Add(
                        new InitialPossessionDataInterface.InitialPossessionInfoAccessor(
                            (int)InitialPossessionDataInterface.RewardTypes.Song, song.UniqueId));
                    musicData.AddMusicInfo(ref song);
                }
            }
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
