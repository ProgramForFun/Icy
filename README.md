# Icy
Icy Unity Framework，提供商业项目必备的各种基础设施，集成业界常用高质量插件/工具

![](https://img.shields.io/badge/Unity%20Ver-2022.3.60-blue.svg?style=flat-square)
![](https://img.shields.io/github/license/ProgramForFun/Icy.svg)
![](https://img.shields.io/github/last-commit/ProgramForFun/Icy)
&nbsp;

## 安装

#### 以下两种方式任选其一：
* 方式一(推荐)：直接Clone本仓库，以本仓库作为基础进行开发
* 方式二：拷贝Icy/Packages、Icy/Assets/ThirdParty、Icy/Assets/Plugins 这三个路径下的内容到你的项目目录的同名文件夹

&nbsp;

## 文档
[How-Tos](https://github.com/ProgramForFun/Icy/wiki/How%E2%80%90tos)

&nbsp;

## Roadmap
|目标|描述|状态|
|---|---|:---:|
|集成[YooAsset🔗](https://github.com/tuyoogame/YooAsset)|AssetBundle资源管理|✔️|
|集成[Luban🔗](https://github.com/focus-creative-games/luban)|打表工具天花板|✔️|
|集成SRDebugger||✔️|
|集成[Protobuf🔗](https://github.com/protocolbuffers/protobuf)|高效二进制序列化|✔️|
|集成[UniTask🔗](https://github.com/Cysharp/UniTask)|GC Free async/await方案|✔️|
|集成[Obfuz🔗](https://github.com/focus-creative-games/obfuz)|和Unity深度集成的代码混淆方案|待开始|
|集成[HybridCLR🔗](https://github.com/focus-creative-games/hybridclr)|特性完整、高性能的全平台原生C#热更新方案|待开始|
|Base-事件系统||✔️|
|Base-FSM||✔️|
|Base-基于FSM的Procedure||✔️|
|Base-Log||✔️|
|Base-Timer||✔️|
|Base-Pool||✔️|
|Base-集成[LocalPreferences🔗](https://github.com/neon-age/LocalPreferences)|基于Json的本地存储，代替PlayerPrefs|✔️|
|Base-PeriodicRecord|方便的设置指定时间后过期的标志|✔️|
|Asset-AssetManager|基于YooAsset的运行时资源管理器|✔️|
|Asset-可定制的打包流程|配置驱动的打包流程，可插入自定义打包步骤|✔️|
|Network-HTTP||✔️|
|Network-TCP||✔️|
|Network-KCP (ReliableUDP)||✔️|
|UI-基础结构||✔️|
|UI-资源管理|基于YooAsset的UI Prefab、图集、Sprite管理|✔️|
|UI-数据绑定||✔️|
|UI-UIText||待开始|
|UI-UIButton||待开始|
|UI-本地化||待开始|
|UI-后退栈||✔️|
|UI-红点||待开始|
|UI-模糊||待开始|
|UI-状态记录组件||待开始|
|UI-集成Super ScrollView||✔️|
|UI-集成[UIEffect](https://github.com/mob-sakai/UIEffect)|各种常用UI效果合集|✔️|
|UI-集成[UIParticle](https://github.com/mob-sakai/ParticleEffectForUGUI)|在UGUI上渲染粒子的工具|✔️|
|UI-集成3DObjectImageForUGUI||✔️|
|UI-易于使用的缓动动画系统|便于UI/UX/直接使用缓动动画制作效果、<br>并序列化进Prefab，以避免使用<br>Animation/Animator、解放程序员|待开始|
|UI-新手引导||待开始|
|Gameplay-基础数值系统||待开始|
|Gameplay-技能系统||待开始|
|Gameplay-换装系统||待开始|
|Editor-显示当前Git分支|在Editor左上角显示当前Git分支，<br>便于多工程管理|✔️|
|Editor-Quick Play|不重新Reload Domain，快速进入Play|✔️|
|Editor-集成Odin||✔️|
|Editor-集成[ReferenceFinder🔗](https://github.com/blueberryzzz/ReferenceFinder)|资源引用和依赖查询工具|✔️|
|Editor-资源托盘|暂存任意工程内文件，便于快速定位|✔️|
|Editor-C#热重载||待开始|

&nbsp;

## 免责声明
本仓库中使用的各种插件仅做学习、示例目的使用，如要用在生产环境、商业项目下，请大家购买正版