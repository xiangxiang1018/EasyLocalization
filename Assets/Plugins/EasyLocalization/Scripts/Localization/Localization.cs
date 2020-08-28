using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyLocalization
{
    public class Localization : MonoBehaviour
    {
        #region 公有参数
        [Header("需要显示的文本ID")]
        [Tooltip("同id可能对应已个中文但是对应多个英文\n例如Equip(vt.)和Equipment(n.)都可以对应到中文的装备")]
        public int id = 0;
        [Header("需要拼接的符号")]
        [Tooltip("鉴于各种语言的不连贯性，只支持于文本结尾添加字符")]
        public string sign = "";
        [Header("默认显示此ID对应内容")]
        [Tooltip("id大于0的情况下生效")]
        public bool defaultShow = true;
        [Header("自动翻译")]
        [Tooltip("数据表查找无效的情况下是否自动翻译并写入json")]
        public bool autoTranslate = true;

        [Header("即时响应多语言变换")]
        [Tooltip("不需要重启游戏即刻切换多语言")]
        public bool runtimeResponse = true;
        #endregion

        //原始文字
        private string _originalText = String.Empty;
        //可选参数
        private string arg0;
        private string arg1;
        private string arg2;
        
        void Start()
        {
            ShowDefault();
            //注册事件，切换语言的时候调用刷新
            if (runtimeResponse)
                LocalizationManager.Action.ChangeLanguage += DoChangeLanguage;
        }

        void OnDestroy()
        {
            if (runtimeResponse)
                LocalizationManager.Action.ChangeLanguage -= DoChangeLanguage;
        }
        
        //显示默认内容
        void ShowDefault()
        {
            if(defaultShow)
            {
                if(id > 0)
                {
                    if(!string.IsNullOrEmpty(originalText))
                        ForceSetText(originalText);
                }
            }
        }

        //如果需要，请自行扩展(个人原因。。。不喜欢参数string[])
        /// <summary>
        /// 程序调用，如需程序动态填充文字，调用
        /// </summary>
        /// <param name="arg0"></param>
        /// <returns></returns>
        public string SetText(string arg0)
        {
            this.arg0 = arg0;
            string newStr = string.Format(originalText, arg0);
            ForceSetText(newStr);
            return newStr;
        }

        public string SetText(string arg0, string arg1)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            string newStr = string.Format(originalText, arg0, arg1);
            ForceSetText(newStr);
            return newStr;
        }

        public string SetText(string arg0, string arg1, string arg2)
        {
            this.arg0 = arg0;
            this.arg1 = arg1;
            this.arg2 = arg2;
            string newStr = string.Format(originalText, arg0, arg1, arg2);
            ForceSetText(newStr);
            return newStr;
        }

        /// <summary>
        /// 显示value（可用于强行重置显示内容）
        /// 如无特殊情况，请尽量使用SetText(xxx,xxx,xxx)，这样程序可以专注于需要填充的内容，而不用关心其id和翻译情况如何
        /// </summary>
        /// <param name="value"></param> 需要显示的文本内容
        public void ForceSetText(string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        //清空显示内容
        public void ClearText()
        {
            ForceSetText(String.Empty);
        }

        //id对应的原始文字
        public string originalText
        {
            get
            {
                if (id > 0)
                {
                    //优先获取表中数据
                    if (string.IsNullOrEmpty(_originalText))
                    {
                        _originalText = LocalizationDataHelper.GetTextById(id);
                    }
                    //如果没有尝试获取Text上的显示数据
                    if (string.IsNullOrEmpty(_originalText))
                    {
                        Text uiText = gameObject.GetComponent<Text>();
                        if (uiText != null)
                            _originalText = uiText.text;
                    }
                }
                return _originalText;
            }
            set
            {
                _originalText = value;
            }
        }

        /// <summary>
        /// 响应多语言重置
        /// </summary>
        public void DoChangeLanguage()
        {
            //重新获取默认文字
            string value = LocalizationDataHelper.GetTextById(id);
            //表内无数据
            if (string.IsNullOrEmpty(value))
            {
                if(!string.IsNullOrEmpty(originalText))
                {
                    StartCoroutine(Translate.TranslateByBaidu(originalText, LanguageType.auto.ToString(), LocalizationManager.Language.ToString(), DoChangeAgain));
                }    
            }
            else
            {
                originalText = value;
                ChangeLanguage();
            }
            
        }

        //百度翻译结果
        public void DoChangeAgain(TranslationResult result)
        {
            string value = string.Empty;
            for(int i = 0; i < result.trans_result.Length; i++)
            {
                value += result.trans_result[i].dst;
            }
            originalText = value;
            ChangeLanguage();
        }

        //语言切换，重置文本
        public void ChangeLanguage()
        {
            if (!string.IsNullOrEmpty(arg2))
            {
                SetText(arg0, arg1, arg2);
            }
            else if (!string.IsNullOrEmpty(arg1))
            {
                SetText(arg0, arg1);
            }
            else if (!string.IsNullOrEmpty(arg0))
            {
                SetText(arg0);
            }
            else
            {
                ShowDefault();
            }
        }

        #region UGUI Text支持
        /*************************UGUI Text**************************/
        private Text _label;
        private Text label
        {
            get
            {
                if (_label == null)
                {
                    _label = this.transform.GetComponent<Text>();
                }
                return _label;
            }
        }
        
        #endregion
    }

}
