using System.Collections;
using UnityEngine;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
//using SimpleJSON;
using UnityEngine.Networking;

namespace EasyLocalization
{
    #region Translation Result
    [Serializable]
    public class TranslationSingleResult
    {
        public string src;
        public string dst;
    }

    //数组是因为百度支持 多组 分词的翻译 用换行符分割-->百度翻译网页也有说明
    [Serializable]
    public class TranslationResult
    {
        public string from;
        public string to;
        public TranslationSingleResult[] trans_result;
        public int error_code;
    }
    #endregion

    /// <summary>
    /// 翻译 （json解析上能用JsonUtility就用）
    /// </summary>
    public static class Translate
    {
        #region【推荐】百度翻译
        //百度翻译最大支持单次6000字节

        //去申请百度开发者，免费获取ID和密码（身份认证之后免费升级高级版 0.1秒/次 200万上限）
        public static string appId = "";
        public static string password = "";

        /// <summary>
        /// 协程回调方式（推荐）
        /// </summary>
        /// <param name="q"></param> 要翻译的文本内容
        /// <param name="sourceLanguage"></param> 源语言
        /// <param name="targetLanguage"></param> 目标语言
        /// <param name="callback"></param> 回调事件 错误的情况下返回null
        /// <returns></returns>
        public static IEnumerator TranslateByBaidu(string q, string sourceLanguage, string targetLanguage, Action<TranslationResult> callback)
        {
            //随机数
            System.Random rd = new System.Random();
            string salt = rd.Next(100000).ToString();
            //md5加密
            string md5Sign = EncryptString(appId + q + salt + password);
            //url
            string url = String.Format("http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}",
                UnityWebRequest.EscapeURL(q),
                sourceLanguage,
                targetLanguage,
                appId,
                salt,
                md5Sign
                );
            //使用UnityWebRequest配合协程
            using (UnityWebRequest webRequest = new UnityWebRequest(url))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();
                if (webRequest.isDone)
                {
                    if (webRequest.isHttpError || webRequest.isNetworkError)
                    {
                        callback.Invoke(null);
                    }
                    else
                    {
                        TranslationResult temp = JsonUtility.FromJson<TranslationResult>(webRequest.downloadHandler.text);
                        if (temp.error_code == -1 || temp.error_code == 0)
                            callback.Invoke(temp);
                    }
                }
            }
        }

        /// <summary>
        /// 直接参数返回方式
        /// </summary>
        /// <param name="q"></param> 要翻译的文本内容
        /// <param name="sourceLang"></param> 源语言
        /// <param name="targetLang"></param> 目标语言
        /// <returns></returns>
        public static TranslationResult TranslateByBaidu(string q, string sourceLanguage, string targetLanguage)
        {
            string jsonResult = String.Empty;
            //随机数
            System.Random rd = new System.Random();
            string salt = rd.Next(100000).ToString();
            //md5加密
            string md5Sign = EncryptString(appId + q + salt + password);
            //url
            //https addresss ： https://fanyi-api.baidu.com/api/trans/vip/translate
            string url = String.Format("http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}",
                UnityWebRequest.EscapeURL(q),
                sourceLanguage,
                targetLanguage,
                appId,
                salt,
                md5Sign
                );
            WebClient wc = new WebClient();
            try
            {
                jsonResult = wc.DownloadString(url);

            }
            catch
            {
                jsonResult = String.Empty;
            }
            if (!string.IsNullOrEmpty(jsonResult))
            {
                TranslationResult temp = JsonUtility.FromJson<TranslationResult>(jsonResult);
                if (temp.error_code == -1 || temp.error_code == 0)
                    return temp;
            }
            return null;
        }

        // 计算MD5值(百度官方API拷贝过来)
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
        #endregion

        #region 谷歌翻译
        ////use  translate.google.cn  so you don't need a vpn
        ////使用translate.google.cn这样不用VPN也没事，国外是不是友好就不知道了
        ////另外说明一下，这个接口不能过于频繁调用，有可能被封IP。。。所以推荐还是百度翻译
        ///// <summary>
        ///// 协程回调方式（推荐）
        ///// </summary>
        ///// <param name="q"></param> 要翻译的文本内容
        ///// <param name="sourceLang"></param> 源语言
        ///// <param name="targetLang"></param> 目标语言
        ///// <param name="callback"></param> 回调事件 错误的情况下返回null
        ///// <returns></returns>
        //public static IEnumerator TranslateByGoogle(string q, LanguageType sourceLang, LanguageType targetLang, Action<TranslationSingleResult> callback)
        //{
        //    //url
        //    string url = String.Format("https://translate.google.cn/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
        //           sourceLang.ToString(),
        //           targetLang.ToString(),
        //           UnityWebRequest.EscapeURL(q)
        //            );
        //    //使用UnityWebRequest配合协程
        //    UnityWebRequest request = new UnityWebRequest(url);
        //    yield return request;
        //    if (request.isDone)
        //    {
        //        if (request.isHttpError || request.isNetworkError)
        //        {
        //            callback.Invoke(null);
        //        }
        //        else
        //        {
        //            var result = JSONNode.Parse(request.downloadHandler.text);
        //            if (result[0] != null && result[0][0] != null)
        //            {
        //                TranslationSingleResult temp = new TranslationSingleResult();
        //                temp.src = result[0][0][1];
        //                temp.dst = result[0][0][0];
        //                callback.Invoke(temp);
        //            }
        //        }
        //    }
        //}

        //public static TranslationSingleResult TranslateByGoogle(string q, LanguageType sourceLang, LanguageType targetLang)
        //{
        //    string jsonResult = String.Empty;
        //    //url
        //    string url = String.Format("https://translate.google.cn/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
        //           sourceLang.ToString(),
        //           targetLang.ToString(),
        //           UnityWebRequest.EscapeURL(q)
        //           );
        //    //webclient
        //    WebClient wc = new WebClient();
        //    try
        //    {
        //        jsonResult = wc.DownloadString(url);

        //    }
        //    catch
        //    {
        //        jsonResult = String.Empty;
        //    }
        //    if (!string.IsNullOrEmpty(jsonResult))
        //    {
        //        var result = JSONNode.Parse(jsonResult);
        //        if (result[0] != null && result[0][0] != null)
        //        {
        //            TranslationSingleResult temp = new TranslationSingleResult();
        //            temp.src = result[0][0][1];
        //            temp.dst = result[0][0][0];
        //            return temp;
        //        }
        //    }
        //    return null;
        //}
        #endregion
    }
}