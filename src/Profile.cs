using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;

namespace HydraMenu
{
    internal class ToggleEntry
    {
        public string Key;
        public Func<bool> Get;
        public Action<bool> Set;
    }

    internal static class Profile
    {
        public static readonly List<ToggleEntry> Entries = new List<ToggleEntry>();
        private static readonly Dictionary<string, ToggleEntry> byKey = new Dictionary<string, ToggleEntry>();

        // 泛型值存储（float/int/enum）
        private static readonly Dictionary<string, Func<object>> valueGetters = new Dictionary<string, Func<object>>();
        private static readonly Dictionary<string, Action<object>> valueSetters = new Dictionary<string, Action<object>>();

        private static string FilePath => Path.Combine(Paths.ConfigPath, "HydraProfile.txt");

        public static void Register(string key, Func<bool> get, Action<bool> set)
        {
            if (byKey.ContainsKey(key)) return;
            var e = new ToggleEntry { Key = key, Get = get, Set = set };
            Entries.Add(e);
            byKey[key] = e;
        }

        public static void RegisterValue<T>(string key, Func<T> get, Action<T> set)
        {
            if (valueGetters.ContainsKey(key)) return;
            valueGetters[key] = () => get();
            valueSetters[key] = obj =>
            {
                try { set((T)Convert.ChangeType(obj, typeof(T))); }
                catch { }
            };
        }

        public static void RegisterAll()
        {
            HydraToggles.RegisterAll();
        }

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("# HydraProfile");
                sb.AppendLine("# 格式: 键名 = 值");
                sb.AppendLine();

                // bool 开关
                foreach (var e in Entries)
                    sb.AppendLine($"{e.Key} = {e.Get()}");

                // 泛型值（float/int/enum）
                foreach (var kvp in valueGetters)
                    sb.AppendLine($"{kvp.Key} = {kvp.Value()}");

                File.WriteAllText(FilePath, sb.ToString());
                Hydra.Log.LogInfo($"Profile 已保存: {FilePath}");
            }
            catch (Exception ex) { Hydra.Log.LogError($"保存 Profile 失败: {ex}"); }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) { Save(); return; }
                foreach (var raw in File.ReadAllLines(FilePath))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    var p = line.Split('=');
                    if (p.Length < 2) continue;
                    string key = p[0].Trim();
                    string value = p[1].Trim();

                    // bool 开关
                    if (byKey.TryGetValue(key, out var e) && bool.TryParse(value, out bool boolVal))
                    {
                        e.Set(boolVal);
                    }
                    // 泛型值
                    else if (valueSetters.TryGetValue(key, out var setter))
                    {
                        setter(value);
                    }
                }
                Hydra.Log.LogInfo("Profile 已载入");
            }
            catch (Exception ex) { Hydra.Log.LogError($"载入 Profile 失败: {ex}"); }
        }
    }
}
