using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

namespace EasyLocalization
{
    public class LocalizationMenu : Editor
    {
        [MenuItem("Assets/EasyLocalization/Check Prefabs")]
        public static void Check()
        {
            EditorCoroutine.StartCoroutine(LocalizationDataHelper.ReadConfig(), CheckSelected);
        }
        
        #region 预设的UI Text批量翻译和添加本地化组件
        private static void CheckSelected()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (path.IndexOf(".prefab") != -1)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (go != null)
                {
                    CheckGameObject(go, path);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Localization Check", "预设\n【" + go.name + "】\n检测完毕！!", "朕已阅");
                }
            }
            else
            {
                EditorCoroutine.StartCoroutine(CheckByPath(path));
            }
        }

        //解析路径下所有预设（每隔3帧处理一个，太多API可能会出问题）
        private static IEnumerator CheckByPath(string path)
        {
            List<string> prefabPaths = GetAllPrefabsPath(path);
            for (int i = 0; i < prefabPaths.Count; i++)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath(prefabPaths[i], typeof(UnityEngine.Object)) as GameObject;
                CheckGameObject(obj, prefabPaths[i]);
                //展示进度
                EditorUtility.DisplayProgressBar("Check Prefab", "Checking " + obj.name, (float)i / (float)prefabPaths.Count);
                yield return new WaitForSomeFrame(3);
            }
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Localization Check", "目录【" + path + "】\n检测完毕!\n请留意Localization.json!", "朕已阅");
        }

        private static List<string> GetAllPrefabsPath(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("错误的路径！");
            List<string> assetPaths = new List<string>();
            string[] guids2 = AssetDatabase.FindAssets("t:Prefab", new string[] { directory });
            for (int i = 0; i < guids2.Length; i++)
            {
                assetPaths.Add(AssetDatabase.GUIDToAssetPath(guids2[i]));
            }
            return assetPaths;
        }

        //如果你没有localization.json，那么会看到相关的报错若干，请忽视。
        //会自动生成1个localization.json
        private static void CheckGameObject(GameObject go, string path)
        {
            if (go != null)
            {
                Text[] uiTexts = go.transform.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < uiTexts.Length; i++)
                {
                    string text = uiTexts[i].text;
                    int id = LocalizationDataHelper.GetIdByText(text, LanguageType.zh);
                    //数据表找不到，添加
                    if (id == 0)
                    {
                        LocalizationData data = new LocalizationData();
                        data.id = GetAKey();
                        foreach (LanguageType item in Enum.GetValues(typeof(LanguageType)))
                        {
                            if(item != LanguageType.auto)
                            {
                                TranslationResult result = Translate.TranslateByBaidu(text, LanguageType.auto.ToString(), item.ToString());
                                if(result != null)
                                {
                                    string resultText = string.Empty;
                                    //字符串中如果有一些转义符，结果会是多个，直接用\n拼接了。。。一般也用不到别的转义符了
                                    for(int j = 0; j < result.trans_result.Length; j++)
                                    {
                                        if (j != result.trans_result.Length - 1)
                                            resultText += result.trans_result[j].dst + "\n";
                                        else
                                            resultText += result.trans_result[j].dst;
                                    }
                                    data.SetValue(item.ToString(), resultText);
                                }
                            }
                        }
                        LocalizationDataHelper.WriteConfig(data.id, data);
                        id = data.id;
                    }
                    //修改或者添加
                    Localization localization = uiTexts[i].gameObject.GetComponent<Localization>();
                    if (localization == null)
                    {
                        localization = uiTexts[i].gameObject.AddComponent<Localization>();
                        localization.id = id;
                        EditorUtility.SetDirty(go);
                    }
                    if(localization.id != id)
                    {
                        localization.id = id;
                        EditorUtility.SetDirty(go);
                    }  
                }
                LocalizationDataHelper.WriteJson();
            }
        }
        #endregion

        private static int GetAKey()
        {
            //因为字典的key不能保证连续，而且是编辑器模式，所以无视性能从1开始遍历
            int index = 1;
            while(true)
            {
                LocalizationData data = null;
                if(!LocalizationDataHelper.GetLocalizationDic().TryGetValue(index, out data))
                {
                    return index;
                }
                index++;
            }
        }
    }
}


