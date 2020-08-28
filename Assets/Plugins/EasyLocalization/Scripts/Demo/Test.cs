using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyLocalization;
using System;

public class Test : MonoBehaviour
{
    public Dropdown dropdown;

    void Awake()
    {
        LocalizationManager.Init();
        if (dropdown == null)
            dropdown = gameObject.GetComponentInChildren<Dropdown>(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        //初始化下拉列表
        if(dropdown != null)
        {
            string[] values = Enum.GetNames(typeof(LanguageType));
            List<Dropdown.OptionData> dropList = new List<Dropdown.OptionData>();
            for (int i = 0; i < values.Length; i++)
            {
                Dropdown.OptionData op = new Dropdown.OptionData();
                op.text = values[i];
                dropList.Add(op);
            }
            dropdown.AddOptions(dropList);
            dropdown.onValueChanged.AddListener(OnDropDownChanged);
        }
    }

    private void OnBtnClick(LanguageType lang)
    {
        LocalizationManager.ChangeLanguage(lang);
    }

    private void OnDropDownChanged(int index)
    {
        Dropdown.OptionData option = dropdown.options[index];
        string value = option.text;
        LanguageType lang = (LanguageType)Enum.Parse(typeof(LanguageType), value);
        LocalizationManager.ChangeLanguage(lang);
    }
}
