using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyLocalization
{
    /// <summary>
    /// Static Manager 
    /// </summary>
    public static class LocalizationManager
    {
        //语言设置优先级，玩家选择语言>系统语言
        public static LanguageType Language { get; set; }
        public static LanguageType LastLanguage { get; set; }
        //通用多语言配置表
        public static string LOCALIZATION_CSV_NAME = "localization";


        //存储字体
        private static List<Font> fonts = new List<Font>();

        //多语言切换（如不需要运行中切换语言功能，请忽略）
        private static LocalizationAction _localizationAction;
        public static LocalizationAction Action
        {
            get
            {
                if (_localizationAction == null)
                    _localizationAction = new LocalizationAction();
                return _localizationAction;
            }
        }

        //初始化，请保证需要在多语言使用前生效
        public static void Init()
        {
            //如果是第一次进游戏,读取系统语言作为默认语言
            Language = LanguageType.en;
            //TODO 增加识别类型
            if (Application.systemLanguage == SystemLanguage.Chinese ||
               Application.systemLanguage == SystemLanguage.ChineseSimplified)
            {
                Language = LanguageType.zh;
            }

            IEnumerator tempIe = LocalizationDataHelper.ReadConfig();
            while (true)
            {
                //没有下一步的时候表示，表要么有->读到了数据，要么没有->初始化了数据
                if (!tempIe.MoveNext())
                {
                    ChangeLanguage(Language);
                    break;
                }
            }
        }

        //游戏内语言切换的时候调用，所有基于Localization && runtimeResponse == true会立即刷新
        public static void ChangeLanguage(LanguageType type)
        {
            LastLanguage = Language;
            Language = type;
            Action.DoChange();
        }

        public class LocalizationAction
        {
            public event Action ChangeLanguage;

            public void DoChange()
            {
                ChangeLanguage?.Invoke();
            }
        }
    }

}


