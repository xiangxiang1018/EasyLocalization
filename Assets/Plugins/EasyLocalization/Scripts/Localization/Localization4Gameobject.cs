using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EasyLocalization
{
    [Serializable]
    public struct LocalizationImgInfo
    {
        public LanguageType langType;
        [Header("Sprite-->多语言切换Image的图片")]
        [Tooltip("支持UI Image自身加载sprite的多语言响应")]
        public Sprite sprite;
        public bool autoNativeSize;
        [Header("子对象-->多语言切换显隐")]
        [Tooltip("支持父对象对子对象物体的多语言响应")]
        public GameObject go;
    }

    /// <summary>
    /// 图片多语言共支持3种
    /// 1.挂在到UI Image（或者Gameobject），只配置了myType，未配置infos，则响应方式为--->自身显隐
    /// 2.挂在UI Image上，配置了infos（忽略myType），则响应方式为--->切换图片
    /// 3.挂在Gameobject父对象上，infos里边需要设置了go为其子对象（忽略myType），则响应方式为--->子对象显隐
    /// </summary>
    public class Localization4Gameobject : MonoBehaviour
    {
        public LanguageType myType;

        public Image img;

        public LocalizationImgInfo[] infos;

        void Awake()
        {
            if (img == null)
                img = gameObject.GetComponent<Image>();
        }

        void Start()
        {
            //注册事件，切换语言的时候调用刷新
            LocalizationManager.Action.ChangeLanguage += DoChangeLanguage;
            DoChangeLanguage();
        }

        void OnDestroy()
        {
            LocalizationManager.Action.ChangeLanguage -= DoChangeLanguage;
        }

        /// <summary>
        /// 响应多语言重置
        /// </summary>
        public void DoChangeLanguage()
        {
            LanguageType type = LocalizationManager.Language;
            ChangeLanguage(type);
        }
        public void ChangeLanguage(LanguageType type)
        {
            int index = -1;
            if (infos != null)
            {
                for (int i = 0; i < infos.Length; i++)
                {
                    if (infos[i].langType == type)
                    {
                        if (img != null && infos[i].sprite != null)
                        {
                            img.sprite = infos[i].sprite;
                            if (infos[i].autoNativeSize)
                                img.SetNativeSize();
                            index = i;
                        }
                        if (infos[i].go != null)
                        {
                            infos[i].go.SetActive(true);
                            index = i;
                        }
                    }
                }
            }
            if (index != -1)
            {
                for (int j = 0; j < infos.Length; j++)
                {
                    if (j != index)
                    {
                        if (infos[j].go != null)
                        {
                            infos[j].go.SetActive(false);
                        }
                    }
                }
            }
            else
                gameObject.SetActive(myType == type);
        }
    }
}
