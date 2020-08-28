using UnityEngine;
using UnityEditor;

namespace EasyLocalization
{
    [CustomEditor(typeof(Localization))]
    public class LocalizationInspector : Editor
    {
        LanguageType selectLang = LanguageType.auto;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            LanguageType temp = (LanguageType)EditorGUILayout.EnumPopup("切换显示语言：", selectLang);
            if(temp != selectLang)
            {
                Localization scr = (Localization)target;
                string value = LocalizationDataHelper.GetTextById(scr.id, temp);
                if(!string.IsNullOrEmpty(value))
                {
                    scr.ForceSetText(value);
                    selectLang = temp;
                }
            }
        }
    }
}
