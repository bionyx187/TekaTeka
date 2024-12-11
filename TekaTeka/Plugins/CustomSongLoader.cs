using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TekaTeka.Plugins
{
    internal class CustomSongLoader
    {
        const string CHARTS_FOLDER = "fumen";
        const string PRACTICE_DIVISIONS_FOLDER = "fumencsv";


        static readonly string songsPath = Path.Combine(BepInEx.Paths.GameRootPath, "TekaSongs");


        #region Append Custom Songs DB

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MusicDataInterface), "LoadDataFromFile")]
        static void LoadSongsDatabase_Postfix(MusicDataInterface __instance, ref string path)
        {
            
            var bytes = Cryptgraphy.ReadAllAesAndGZipBytes(path, Cryptgraphy.AesKeyType.Type2);
            string jsonString = Encoding.UTF8.GetString(bytes);
            var musicInfolist = JsonSupports.ReadJson<MusicDataInterface.MusicInfo>(jsonString);


            for (int i = 0; i < musicInfolist.Count; i++)
            {
                MusicDataInterface.MusicInfo song = musicInfolist[i];

                __instance.AddMusicInfo(ref song);
            }
        }

        #endregion

        #region Load Custom Chart

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FumenLoader.PlayerData), "ReadCoroutine")]
        static void ReadSongChart_Prefix(FumenLoader.PlayerData __instance, ref string filePath)
        {
            if (File.Exists(filePath))
            {
                return;
            };
            string fileName = Path.GetFileName(filePath);
            filePath = Path.Combine(songsPath, CHARTS_FOLDER, fileName);

        }

        #endregion

        #region Load Custom Practice Chart

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FumenDivisionManager), nameof(FumenDivisionManager.Load))]
        static bool ReadSongPracticeDivision_Prefix(FumenDivisionManager __instance, ref string musicuid)
        {

            if (File.Exists(Path.Combine(UnityEngine.Application.streamingAssetsPath, PRACTICE_DIVISIONS_FOLDER, musicuid + ".bin")))
            {
                return true;
            };
            string filePath = Path.Combine(songsPath, PRACTICE_DIVISIONS_FOLDER, musicuid + ".bin");


            var bytes = Cryptgraphy.ReadAllAesAndGZipBytes(filePath, Cryptgraphy.AesKeyType.Type2);
            string csvString = Encoding.UTF8.GetString(bytes);
            var datas = __instance.loader_.CreateDivisionData(csvString);
            __instance.datas_ = datas;
            __instance.IsLoadFinished = true;
            __instance.musicuId_ = musicuid;
            return false;
        }

        #endregion

        #region Load Custom Song file

        // TODO

        #endregion



    }
}
