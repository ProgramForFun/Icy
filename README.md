# Icy
Icy Unity Framework，提供Unity商业项目必备的各种基础设施，集成业界常用高质量插件/工具；  
HybridCLR + YooAsset + UniTask + Luban + Obfuz + KCP + NativeWebSocket + Protobuf + Odin + LitMotion + SRDebugger + UIParticle + UIEffect + ZString；  
更具体的见[Features & Roadmap](#features--roadmap)

![](https://img.shields.io/badge/Unity%20Version-2022.3-blue.svg?style=flat)
![](https://img.shields.io/github/license/ProgramForFun/Icy.svg)
![](https://img.shields.io/github/last-commit/ProgramForFun/Icy)
&nbsp;

[![](https://github-readme-activity-graph.vercel.app/graph?username=ProgramForFun&repo=Icy&theme=github-light&area=true)](https://github.com/ProgramForFun/Icy/activity)

&nbsp;

## Installation

1. **安装Framework**：
	* 方式一(推荐)：直接Clone本仓库，以本仓库作为基础进行项目开发
	* 方式二：
		1. 拷贝Icy/Packages、Icy/Assets/ThirdParty、Icy/Assets/Plugins 这三个目录下的内容到你的项目目录的同名文件夹
		2. 拷贝/Config、/Proto两个目录到你的项目的Assets目录的**上一级**目录
		3. 拷贝Icy/IcySettings到你的项目的Assets目录的**同级**目录，并从Unity菜单栏的Icy分别打开Asset、UI、Proto、Config的Setting窗口，根据你项目的情况填入对应的设置
		4. 把PlayerSetting中的Api Compatibility Level设置为`.Net Framework`
2. **安装以下付费插件**：

|插件名称|描述|Optionality|版本号需求|插件安装后移动到此目录|
|---|---|---|---|---|
|[Odin](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041?srsltid=AfmBOoqnEoW-YXYsMYcC16eMnl5dRPUn6r5arsTQzbamf9GPOZV6fplR)|强大的Editor扩展、Serializer|必选|3.3.1.13+|Icy/Assets<br>/Plugins/Sirenix|
|[SRDebugger](https://assetstore.unity.com/packages/tools/gui/srdebugger-console-tools-on-device-27688?srsltid=AfmBOopomW8bzQFHohdFJUhKFtu_gtCoFwMtWsb19arVXiJVZAnFVzU_)|GM工具、运行时Console|可选，如不需要可移除PlayerSetting中的`ICY_USE_SRDEBUGGER`宏|1.11.0+|Icy/Packages<br>/fun.program4.icy.gm<br>/StompyRobot|

Icy依赖上述这些强大的付费插件，但为了避免侵权、违反许可协议不能直接提供，  
请您按照上面提供的官方链接和版本号自行购买并导入；  
标为可选的付费插件，如确定不需要，可以按提示移除；

&nbsp;

## 文档
[How-Tos](https://github.com/ProgramForFun/Icy/wiki/How%E2%80%90tos)  
（目前在集中推进Roadmap的实现，文档的完善会晚一些）

&nbsp;

## Features & Roadmap
|目标|描述|完成状态|
|---|---|:---:|
|集成[UniTask](https://github.com/Cysharp/UniTask)|GC Free async/await方案|✔️|
|集成[Protobuf](https://github.com/ProgramForFun/protobuf_unity)|高效二进制序列化|✔️|
|集成[LitMotion](https://github.com/annulusgames/LitMotion)|高性能、Zero GC Alloc缓动效果库|✔️|
|集成[Luban](https://github.com/focus-creative-games/luban)|打表工具天花板|✔️|
|集成[HybridCLR](https://github.com/focus-creative-games/hybridclr)|特性完整、高性能的全平台原生C#热更新方案|✔️|
|集成[Obfuz](https://github.com/focus-creative-games/obfuz)|和Unity深度集成的代码混淆方案|待开始|
|集成[SRDebugger](https://assetstore.unity.com/packages/tools/gui/srdebugger-console-tools-on-device-27688?srsltid=AfmBOopomW8bzQFHohdFJUhKFtu_gtCoFwMtWsb19arVXiJVZAnFVzU_)|GM工具、运行时Console|✔️|
|集成[ZString](https://github.com/Cysharp/ZString)|Zero GC Alloc StringBuilder/Formatter|✔️|
|集成[LocalPreferences](https://github.com/neon-age/LocalPreferences)|基于Json的本地存储，代替PlayerPrefs|✔️|
|Base-事件系统|基础事件系统|✔️|
|Base-FSM|有限状态机，附带一个状态切换可视化工具|✔️|
|Base-Procedure|基于FSM的顺序执行的流程|✔️|
|Base-Log|提供Log到文件、多线程Log|✔️|
|Base-PeriodicRecord|方便的设置指定时间后过期的标志|✔️|
|Asset-集成[YooAsset](https://github.com/tuyoogame/YooAsset)|非常好用的资源管理系统|✔️|
|Asset-可定制的打包流程|配置驱动的打包流程，可插入自定义打包步骤|✔️|
|Network-集成[NativeWebSocket](https://github.com/ProgramForFun/NativeWebSocket_OptimizedGC)|支持WebGL平台的开源WebSocket库|✔️|
|Network-HTTP|基于UnityWebRequest、HttpClient|✔️|
|Network-TCP|基于.Net TCP|✔️|
|Network-[KCP](https://github.com/passiony/kcp-unity)|以速度著称的Reliable UDP|✔️|
|Network-WebSocket|基于NativeWebSocket，支持WebGL平台|✔️|
|UI-基础结构||✔️|
|UI-资源管理|基于YooAsset的UI Prefab、图集、Sprite管理|✔️|
|UI-数据绑定|一个简单的数据绑定数据结构|✔️|
|UI-TextEx||待开始|
|UI-ButtonEx||待开始|
|UI-ImageEx||待开始|
|UI-本地化||待开始|
|UI-后退栈||✔️|
|UI-红点||✔️|
|UI-背景模糊||✔️|
|UI-状态记录组件|序列化进Prefab的UI状态记录，<br>支持一键切换多个UI节点的状态|待开始|
|UI-集成[UIEffect](https://github.com/mob-sakai/UIEffect)|各种常用UI效果合集|✔️|
|UI-集成[UIParticle](https://github.com/mob-sakai/ParticleEffectForUGUI)|在UGUI上渲染粒子的工具|✔️|
|UI-新手引导||待开始|
|Gameplay-基础数值系统||待开始|
|Gameplay-技能系统||待开始|
|Gameplay-换装系统||待开始|
|Editor-显示当前分支|在Editor左上角显示当前Git / SVN分支，<br>便于多开Unity项目时的识别|✔️|
|Editor-Quick Play|不 重新Reload Domain，快速进入Play，<br>整个Icy框架支持DisableDomainReload|✔️|
|Editor-集成[Odin](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041?srsltid=AfmBOoqnEoW-YXYsMYcC16eMnl5dRPUn6r5arsTQzbamf9GPOZV6fplR)|强大的Editor扩展、Serializer|✔️|
|Editor-集成[ReferenceFinder](https://github.com/blueberryzzz/ReferenceFinder)|资源引用和依赖查询工具|✔️|
|Editor-资源托盘|暂存任意工程内文件，便于快速定位|✔️|
|Editor-C#热重载||待开始|

&nbsp;

## 免责声明
本仓库中使用的各种插件仅做学习、示例目的使用，如要用在生产环境、商业项目下，请大家购买正版