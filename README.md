# EasyLocalization

# 准备

第一步：登陆百度翻译开放平台注册成为开发者。建议上传身份信息，申请高级版服务。地址：https://api.fanyi.baidu.com/

第二步：在你的百度开发者控制台总览页，找到APP ID和密钥，填写到Translate.cs中的appId和password中。

第三步：开始愉快使用吧。注意：高级版服务每月免费200万字，超出则付费。


# 使用方法

![image](https://github.com/xiangxiang1018/EasyLocalization/blob/master/Help/HowToUse.gif)

# 编辑器

- 菜单栏 EasyLocalization/1 - Language Setting

  设置当前使用的语言，会自动生成LanguageType.cs（当然手动更改也完全OK）
  
  此类决定是整个EasyLocalization的依赖
  
- 菜单栏 EasyLocalization/2 - Translate Json
  
  自动翻译Assets/StreamingAssets/Localization.json中未被翻译且定义于LanguageType.cs中的语言对应的内容（需要至少有一种语言已经配置内容）
  
* Assets右键 Assets/EasyLocalization/Check Prefabs

   可作用于文件夹或预设
   
   会自动检测预设中所有UGUI Text，处理以下两种情况：
  - 未添加Localization组件，添加Localization组件。查看配置表（Localization.json）是否含有当前Text上的文字内容，有则将其id设置到Localization。没有则扩展新的id添加到配置表中
  - 已添加Localization组件，检测此id内容是否匹配当前文本（若当前文本为Empty则跳过)
   
   以上两种情况下，都会依据LanguageType设置语言，翻译其中内容
   
 # 特性
 
 - 实时翻译
 
    配置表不存在，可配置Localization组件是否开启实时百度翻译（谷歌翻译被我注释了）
    
 - 完善的工具
 
    对于固定文本，一次输入动动手指即可生成多种语言文本（基于百度翻译最多支持20种语言）
    
 - 实时切换
 
    语言支持实时切换，不需要重新登录游戏即可看到效果
    
 - 易用友好
 
    哪怕无编程基础，依然可以Don't worry, be happy!

 # 特别说明
 
 - 动态文本
 
   添加Localization组件，设置正确的id，在设置文本时，不需要关心当前语言类型，只需要设置填充内容即可
 
   举例：初始化时调用Localization(.id = 1)的SetText(LocalizationDataHelper.GetTextById(2))方法
   
   配置表：id = 1 --> 我来自于{0}； id = 2 --> 中国。 
   
   当设置语言为中文时，内容为：我来自于中国。当语言切换为英文时，代码无非再次设置，内容会自动变成：I'm from China
