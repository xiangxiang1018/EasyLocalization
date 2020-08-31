using UnityEngine;


namespace EasyLocalization
{
    public static class PathDefinition
    {
        //.cs自动生成文件存储路径
        public static string Auto_Generate_Path = "/Plugins/EasyLocalization/Scripts/AutoGenerate/";

        //json配置表名称
        public static string Json_Name = "Localization.json";

        //json配置表路径
        private static string json_local_path;
        private static string json_data_path;
        public static string Json_Path
        {
            get
            {
                if (string.IsNullOrEmpty(json_local_path))
                    GetPath();
                return json_local_path;
            }
        }
        public static string Json_Data_Path
        {
            get
            {
                if (string.IsNullOrEmpty(json_data_path))
                    GetPath();
                return json_data_path;
            }
        }

        private static void GetPath()
        {
#if UNITY_ANDROID
            json_local_path = "jar:file://" + Application.dataPath + "!/assets/";
            json_data_path = Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
            json_local_path = Application.dataPath + "/Raw/";
            json_data_path = Application.dataPath + "/Raw/";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            json_local_path = "file://" + Application.dataPath + "/StreamingAssets/";
            json_data_path = Application.dataPath + "/StreamingAssets/";
#endif
        }

    }
}


