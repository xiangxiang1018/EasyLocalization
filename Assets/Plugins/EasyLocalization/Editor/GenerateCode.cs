using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System;
using UnityEngine;

public static class GenerateCode
{
    /// <summary>
    /// 生成.cs代码文件
    /// </summary>
    /// <param name="name"></param>文件名称
    /// <param name="path"></param>文件路径
    /// <param name="typeAttributes"></param>声明类型（Public Private Interface等）
    /// <param name="codeType"></param>类 枚举等
    /// <param name="fields"></param>字段 (public string name)
    /// <param name="properties"></param>属性 (public string name{get;set;})
    /// <param name="codeAttributeDeclaration"></param>标签 ( Serializable SerializableAttribute)
    /// <param name="nameSpace"></param>命名空间
    /// <param name="imports"></param>引入
    public static void Generate(string name, string path, TypeAttributes typeAttributes, CodeType codeType, 
        CodeMemberField[] fields = null, CodeMemberProperty[] properties = null,
        CodeAttributeDeclaration codeAttributeDeclaration = null, string nameSpace = null, string[] imports = null)
    {
        //准备一个代码编译器单元
        CodeCompileUnit unit = new CodeCompileUnit();
        //定义一堆东西
        CodeTypeDeclaration sampleClass = new CodeTypeDeclaration(name);
        CodeNamespace sampleNamespace = new CodeNamespace(nameSpace);
        if (imports != null)
        {
            for(int i = 0; i < imports.Length; i++)
            {
                sampleNamespace.Imports.Add(new CodeNamespaceImport(imports[i]));
            }
        }

        //放到定义的命名空间
        sampleNamespace.Types.Add(sampleClass);
        //命名空间加入到编译器单元的命名空间集合中
        unit.Namespaces.Add(sampleNamespace);

        //指定为公共类
        switch(codeType)
        {
            case CodeType.IsClass:
                sampleClass.IsClass = true;
                break;
            case CodeType.IsEnum:
                sampleClass.IsEnum = true;
                break;
            case CodeType.IsInterface:
                sampleClass.IsInterface = true;
                break;
            case CodeType.IsPartial:
                sampleClass.IsPartial = true;
                break;
            case CodeType.IsStruct:
                sampleClass.IsStruct = true;
                break;
        }
        sampleClass.TypeAttributes = typeAttributes;

        //输出路径
        string outputFile = path;
        if (path.LastIndexOf("/") == 0)
            outputFile += name + ".cs";
        else
            outputFile += "/" + name + ".cs";
        
        //添加字段
        if(fields != null)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                sampleClass.Members.Add(fields[i]);
            }
        }

        //添加属性
        if (properties != null)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                sampleClass.Members.Add(properties[i]);
            }
        }

        //添加标签
        if(codeAttributeDeclaration != null)
        {
            sampleClass.CustomAttributes.Add(codeAttributeDeclaration);
        }

        //生成代码
        CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
        CodeGeneratorOptions options = new CodeGeneratorOptions();
        options.BracingStyle = "C";
        options.BlankLinesBetweenMembers = true;

        //写入
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(outputFile))
        {
            provider.GenerateCodeFromCompileUnit(unit, sw, options);
        }
    }
}

public enum CodeType
{
    IsClass,
    IsEnum,
    IsPartial,
    IsInterface,
    IsStruct
}
