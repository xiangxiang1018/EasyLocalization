using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyLocalization
{
    public static class LocalizationDataHelper
    {
        //通用多语言配置表引用
        private static Dictionary<int, LocalizationData> locDatasDic = new Dictionary<int, LocalizationData>();

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="id"></param>id
        /// <param name="forceLan"></param>指定语言版本的文本
        /// <returns></returns>
        public static string GetTextById(int id, LanguageType? forceLang = null)
        {
            if (locDatasDic.Count == 0)
            {
                Debug.LogError("Localization Config Error : Before you use 'GetTextById' you must read the config!");
                return string.Empty;
            }
            LanguageType lang = forceLang ?? LocalizationManager.Language;
            LocalizationData data;
            if (!locDatasDic.TryGetValue(id, out data))
            {
                Debug.LogError("Localization Config Error : Can't find localization data which id is " + id);
                return string.Empty;
            }
            return data.GetValue(lang.ToString());
        }

        //匹配文字，返回id
        public static int GetIdByText(string text, LanguageType lang)
        {
            if (locDatasDic.Count == 0)
            {
                Debug.LogError("Localization Config Error : Before you use 'GetTextById' you must read the config!");
                return 0;
            }
            foreach(int key in locDatasDic.Keys)
            {
                if(locDatasDic[key].GetValue(lang.ToString()) == text)
                {
                    return key;
                }
            }
            return 0;
        }

        
        //读取通用多语言配置表
        public static IEnumerator ReadConfig()
        {
            string path = PathDefinition.Json_Path + PathDefinition.Json_Name;
            using (UnityWebRequest webRequest = new UnityWebRequest(path))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();
                if (webRequest.isHttpError || webRequest.isNetworkError)
                {
                    Debug.LogError("Read Localization.json error!");
                }
                else
                {
                    if(!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {
                        locDatasDic = new Dictionary<int, LocalizationData>(JsonUtility.FromJson<Serialization<int, LocalizationData>>(webRequest.downloadHandler.text).ToDictionary());
                    }
                    Debug.Log("EasyLocalization Read Config Complete!");
                }
            }
        }

        //缓存表数据 1
        public static void WriteConfig(int id, LocalizationData data, bool writeJson = true)
        {
            if (locDatasDic.ContainsKey(id))
                locDatasDic[id] = data;
            else
                locDatasDic.Add(id, data);
            if (writeJson)
                WriteJson();
        }
        public static void WriteConfig(int id, string text, LanguageType lang, string sourceText, LanguageType sourceLang, bool writeJson = true)
        {
            LocalizationData data;
            if(!locDatasDic.TryGetValue(id, out data))
            {
                data = new LocalizationData();
            }
            data.SetValue(lang.ToString(), text);
            data.SetValue(sourceLang.ToString(), sourceText);
            if (writeJson)
                WriteJson();
        }

        //写入数据生成Json文件
        public static void WriteJson()
        {
            string content = JsonUtility.ToJson(new Serialization<int, LocalizationData>(locDatasDic));
            string path = PathDefinition.Json_Data_Path + PathDefinition.Json_Name;
            //检测文件夹是否存在
            if(!Directory.Exists(PathDefinition.Json_Data_Path))
            {
                Directory.CreateDirectory(PathDefinition.Json_Data_Path);
            }
            //写入文件Localization.json
            File.WriteAllText(path, content);
            //刷新
            AssetDatabase.Refresh();
        }

        public static Dictionary<int, LocalizationData> GetLocalizationDic()
        {
            return locDatasDic;
        }
    }


    //序列化词典
    [Serializable]
    public class Serialization<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField]
        List<TKey> keys;
        [SerializeField]
        List<TValue> values;

        Dictionary<TKey, TValue> target;
        public Dictionary<TKey, TValue> ToDictionary() { return target; }

        public Serialization(Dictionary<TKey, TValue> target)
        {
            this.target = target;
        }

        public void OnBeforeSerialize()
        {
            keys = new List<TKey>(target.Keys);
            values = new List<TValue>(target.Values);
        }

        public void OnAfterDeserialize()
        {
            var count = Math.Min(keys.Count, values.Count);
            target = new Dictionary<TKey, TValue>(count);
            for (var i = 0; i < count; ++i)
            {
                target.Add(keys[i], values[i]);
            }
        }
    }
}

