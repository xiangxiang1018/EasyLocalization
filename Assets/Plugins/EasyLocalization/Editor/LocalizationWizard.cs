using UnityEditor;
using UnityEngine;
using EasyLocalization;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom;
using System;
using System.Collections;

namespace EasyLocalization
{
    public class LocalizationWizard : ScriptableWizard
    {
        [Header("配置语言")]
        public BaiduLanuageType[] selectLanguages;


        [MenuItem("EasyLocalization/1 - Language Setting")]
        public static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<LocalizationWizard>("EasyLocalization", "Apply");
        }

        [MenuItem("EasyLocalization/2 - Translate Json")]
        public static void TranslateJson()
        {
            //创建完毕 更新Localization.json的翻译情况
            EditorCoroutine.StartCoroutine(waitToReadJson());
        }

        [MenuItem("EasyLocalization/N - Clear Progressbar")]
        public static void ClearProgressbar()
        {
            EditorUtility.ClearProgressBar();
        }

        void Awake()
        {
            CheckCurrentSelected();
        }

        void OnWizardCreate()
        {
            if (selectLanguages == null || selectLanguages.Length <= 0)
            {
                EditorUtility.DisplayDialog("EasyLocalization Setting", "Please set your languages!", "I See");
                return;
            }
            //CreateDataClass();
            CreateEnum();
        }

        void CheckCurrentSelected()
        {
            //显示当前选择
            string[] values = Enum.GetNames(typeof(LanguageType));
            selectLanguages = new BaiduLanuageType[values.Length - 1];
            int index = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != "auto")
                    selectLanguages[index++] = (BaiduLanuageType)Enum.Parse(typeof(BaiduLanuageType), values[i]);
            }
        }

        #region 生成.cs
        //生成LocalizationData
        //private void CreateDataClass()
        //{
        //    string path = Application.dataPath + PathDefinition.Auto_Generate_Path;
        //    GenerateCode.Generate("LocalizationData", path, TypeAttributes.Public, CodeType.IsClass, GetFields(), null,
        //                            new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializableAttribute))), "EasyLocalization", GetImports());
        //}

        //生成LanguageType
        private void CreateEnum()
        {
            string path = Application.dataPath + PathDefinition.Auto_Generate_Path;
            GenerateCode.Generate("LanguageType", path, TypeAttributes.Public, CodeType.IsEnum, GetEnumFields(), null, null, "EasyLocalization", null);
            //刷新资源(LanguageType会很久才会更新成新设置的)
            AssetDatabase.Refresh();
        }

        //CodeMemberField[] GetFields()
        //{
        //    CodeMemberField[] fields = new CodeMemberField[selectLanguages.Length + 1];
        //    CodeMemberField field = new CodeMemberField(typeof(int), "id");
        //    field.Attributes = MemberAttributes.Public;
        //    fields[0] = field;
        //    for (int i = 0; i < selectLanguages.Length; i++)
        //    {
        //        field = new CodeMemberField(typeof(string), selectLanguages[i].ToString());
        //        field.Attributes = MemberAttributes.Public;
        //        fields[i + 1] = field;
        //    }
        //    return fields;
        //}

        CodeMemberField[] GetEnumFields()
        {
            CodeMemberField[] fields = new CodeMemberField[selectLanguages.Length + 1];
            CodeMemberField field = new CodeMemberField(typeof(Enum), "auto");
            fields[0] = field;
            for (int i = 0; i < selectLanguages.Length; i++)
            {
                field = new CodeMemberField(typeof(Enum), selectLanguages[i].ToString());
                fields[i + 1] = field;
            }
            return fields;
        }

        string[] GetImports()
        {
            string[] imports = new string[2];
            imports[1] = "UnityEngine";
            imports[0] = "System";
            return imports;
        }

        #endregion

        #region 翻译localization.json
        private static IEnumerator waitToReadJson()
        {
            EditorUtility.DisplayProgressBar("EasyLocalization", "Creating LanguageType.cs", 1f);
            yield return new WaitForSomeTime(1f);
            ReadJson();
        }

        private static void ReadJson()
        {
            EditorUtility.DisplayProgressBar("EasyLocalization", "Reading Lolization.json", 1f);
            //读取完毕再开始检测
            IEnumerator tempIe = LocalizationDataHelper.ReadConfig();
            EditorCoroutine.StartCoroutine(tempIe, () => {
                EditorCoroutine.StartCoroutine(CheckJson());
            });
        }

        private static IEnumerator CheckJson()
        {
            Dictionary<int, LocalizationData> dic = new Dictionary<int, LocalizationData>(LocalizationDataHelper.GetLocalizationDic());
            int progress = 0;
            foreach (int key in dic.Keys)
            {
                EditorUtility.DisplayProgressBar("EasyLocalization", "Checking Lolization.json. LocalizationData id :" + dic[key].id, progress++ / dic.Keys.Count);
                TranslateData(LocalizationDataHelper.GetLocalizationDic()[key]);
                yield return new WaitForSomeTime(0.1f);
            }
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("EasyLocalization Setting", "Done!", "OK");
        }

        //请注意：语种对应内容为空时，才进行翻译
        private static void TranslateData(LocalizationData data)
        {
            string sourceLang = "auto";
            string sourceText = string.Empty;
            //查看当前配置语言中，是否有存在的文本
            foreach (string lang in Enum.GetNames(typeof(LanguageType)))
            {
                sourceText = data.GetValue(lang);
                if (!string.IsNullOrEmpty(sourceText))
                {
                    sourceLang = lang;
                    break;
                }
            }

            if (sourceLang == "auto" || string.IsNullOrEmpty(sourceText))
            {
                Debug.LogError("There is no source text which id is " + data.id + " in Localization.json!");
                return;
            }

            foreach (string targetLang in Enum.GetNames(typeof(LanguageType)))
            {
                if (targetLang != "auto" && targetLang != sourceLang)
                {
                    //检测是否当前语种有内容，有则跳过
                    string cur = data.GetValue(targetLang);
                    if (!string.IsNullOrEmpty(cur)) continue;
                    //获取百度翻译结果
                    TranslationResult result = Translate.TranslateByBaidu(sourceText, sourceLang, targetLang);
                    if (result != null)
                    {
                        string resultText = string.Empty;
                        //字符串中如果有一些转义符，结果会是多个，直接用\n拼接了。。。一般也用不到别的转义符了
                        for (int j = 0; j < result.trans_result.Length; j++)
                        {
                            if (j != result.trans_result.Length - 1)
                                resultText += result.trans_result[j].dst + "\n";
                            else
                                resultText += result.trans_result[j].dst;
                        }
                        data.SetValue(targetLang, resultText);
                    }
                }
            }
            LocalizationDataHelper.WriteConfig(data.id, data);
        }
        #endregion
    }

}
