﻿//using BepInEx;
//using BepInEx.Logging;
using BepInEx.Configuration;
using BepInEx.Logging;
using COM3D2.i18nEx.Core;
using COM3D2.i18nEx.Core.Loaders;
using COM3D2.i18nEx.Core.Util;
using HarmonyLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i18nex.ZipLoader
{
    public class ZipLoader : ITranslationLoader
    {
        internal static ManualLogSource log = new ManualLogSource("ZipLoader");
        internal static ConfigFile config_;
        internal static ConfigEntry<bool> isLogLaod;
        internal static ConfigEntry<bool> isTranslation;

        public string CurrentLanguage { get; private set; }

        private static string langPath;

        internal readonly static Dictionary<string, byte[]> Scripts = new Dictionary<string, byte[]>();
        internal readonly static Dictionary<string, byte[]> Textures = new Dictionary<string, byte[]>();
        internal readonly static Dictionary<string, byte[]> UIs = new Dictionary<string, byte[]>();
        internal readonly static SortedDictionary<string, HashSet<string>> dictTmp = new SortedDictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);
        internal readonly static SortedDictionary<string, IEnumerable<string>> dict = new SortedDictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);

        static byte[] buffer = new byte[4096];


        public void SelectLanguage(string name, string path, global::ExIni.IniFile config)
        {
            Core.Logger.LogInfo($"Loading language \"{name}\"");

            CurrentLanguage = name;
            langPath = path;

            var type = AccessTools.TypeByName("COM3D2.i18nEx.Core.Paths");
            var property = AccessTools.Property(type, "TranslationsRoot");
            Core.Logger.LogInfo($"TranslationsRoot \"{(string)property.GetValue(null, null)}\"");

            config_ = new ConfigFile(Path.Combine((string)property.GetValue(null, null), "ZipLoader.cfg"), true);
            isLogLaod = config_.Bind("isLog", "isLogLaod", false);
            isTranslation = config_.Bind("isLog", "isTranslation", false);

            Scripts.Clear();
            Textures.Clear();
            UIs.Clear();
            dict.Clear();
            dictTmp.Clear();

            ZipLoad("Script", Scripts);
            ZipLoad("Textures", Textures);
            UIZipLoad();

            FileLoad("Script", Scripts, "*.txt");
            FileLoad("Textures", Textures, "*.png");
            UILoad();

            UIdictSet();
        }


        public void UnloadCurrentTranslation()
        {
            Core.Logger.LogInfo($"Unloading language \"{CurrentLanguage}\"");

            config_ = null;
            isLogLaod = null;
            CurrentLanguage = null;
            langPath = null;
        }

        public void ZipLoad(string type, Dictionary<string, byte[]> dic)
        {
            ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
            Core.Logger.LogInfo($"ZipLoad : {type} {ZipConstants.DefaultCodePage}");

            string path = Path.Combine(langPath, type);
            if (!Directory.Exists(path))
                return;

            foreach (string zipPath in Directory.GetFiles(path, "*.zip", SearchOption.AllDirectories))
            {
                using (ZipFile zip = new ZipFile(zipPath))
                {
                    Core.Logger.LogInfo($"zip : {zipPath} , {zip.Count} , {zip.ZipFileComment}");

                    foreach (ZipEntry zfile in zip)
                    {
                        if (!zfile.IsFile) continue;
                        if (!zfile.CanDecompress)
                        {
                            Core.Logger.LogInfo($"Can't Decompress {zfile.Name}");
                            continue;
                        }
                        DicAdd(dic, zip.GetInputStream(zfile), Path.GetFileName(zfile.Name));
                    }
                }
            }

            Core.Logger.LogInfo($"ZipLoad : {type} , {dic.Count}");
        }

        public void FileLoad(string type, Dictionary<string, byte[]> dic, string searchPattern)
        {
            Core.Logger.LogInfo($"FileLoad : {type}");

            string path = Path.Combine(langPath, type);
            if (!Directory.Exists(path))
                return;

            foreach (string file in Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories))
            {
                DicAdd(dic, File.OpenRead(file), Path.GetFileName(file));
            }
            Core.Logger.LogInfo($"FileLoad : {type} , {dic.Count}");
        }

        public void UIZipLoad()
        {

        }

        public void UILoad()
        {
            var uiPath = Path.Combine(langPath, "UI");
            if (!Directory.Exists(uiPath))
                return;

            foreach (var directory in Directory.GetDirectories(uiPath, "*", SearchOption.TopDirectoryOnly))
            {
                // Item
                //var dirName = directory.Splice(uiPath.Length, -1).Trim('\\', '/');
                var dirName = Path.GetFileName(directory);
                if (isLogLaod.Value)
                    Core.Logger.LogInfo($"GetUITranslationFileNames , {dirName} ");

                //IEnumerable<string> value = Directory.GetFiles(directory, "*.csv", SearchOption.AllDirectories).Select(s => s.Splice(directory.Length + 1, -1));
                var files = Directory.GetFiles(directory, "*.csv", SearchOption.AllDirectories);
                // H:\COM3D2_5-test\i18nEx\korean\UI\Item\accanl.csv  
                if (isLogLaod.Value)
                    if (files.Length > 0)
                    {
                        Core.Logger.LogInfo($"GetUITranslationFileNames , { files[0].Splice(directory.Length + 1, -1)}  , { files[0]}  ");
                    }

                if (!dictTmp.ContainsKey(dirName))
                {
                    dictTmp[dirName] = new HashSet<string>();
                }

                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    DicAdd(UIs, File.OpenRead(file), dirName + "\\" + name);
                    dictTmp[dirName].Add(name);
                }

                //UIs.Add(dirName, );
                //dict.Add(dirName, value.Select(s => s.Splice(directory.Length + 1, -1)).ToList());

                //Core.Logger.LogInfo($"UILoad : {directory} , {UIs[dirName].Count}");
            }
            Core.Logger.LogInfo($"UILoad : {UIs.Count}");
        }

        public void UIdictSet()
        {
            foreach (var item in dictTmp)
            {
                dict[item.Key] = item.Value;
            }
        }

        private static void DicAdd(Dictionary<string, byte[]> dic, Stream stream, string fileName)
        {
            using (stream)
            using (MemoryStream mstream = new MemoryStream())
            {
                StreamUtils.Copy(stream, mstream, buffer);
                dic[fileName] = mstream.ToArray();
                if (isLogLaod.Value)
                {
                    Core.Logger.LogInfo($"DicAdd : {fileName} , {dic[fileName].Length}");
                }
            }
        }

        public IEnumerable<string> GetScriptTranslationFileNames()
        {
            return Scripts.Keys;
        }

        public IEnumerable<string> GetTextureTranslationFileNames()
        {
            return Textures.Keys;
        }

        public SortedDictionary<string, IEnumerable<string>> GetUITranslationFileNames()
        {
            return dict;
        }

        public Stream GetStream(string path, Dictionary<string, byte[]> dic)
        {
            return new MemoryStream(dic[path]);
        }

        public Stream OpenScriptTranslation(string path)
        {
            if (isTranslation.Value)
                Core.Logger.LogInfo($"OpenScriptTranslation , {path} ");

            return GetStream(path, Scripts);
        }

        public Stream OpenTextureTranslation(string path)
        {
            if (isTranslation.Value)
                Core.Logger.LogInfo($"OpenTextureTranslation , {path} ");

            return GetStream(path, Textures);
        }

        // Dance\SceneDance_1OY_Release.csv 
        public Stream OpenUiTranslation(string path)
        {
            if (isTranslation.Value)
                Core.Logger.LogInfo($"OpenUiTranslation , {path} ");

            return GetStream(path, UIs);

            //path = Utility.CombinePaths(langPath, "UI", path);
            //return !File.Exists(path) ? null : File.OpenRead(path);
        }

    }
}
