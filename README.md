# frameLogger

### 介绍
这是一个记录帧同步游戏，记录每一帧代码执行记录的工具框架。
### 支持的数据类型
支持的基本参数类型有9种，分别是： Int32、UInt32、 Int64、UInt64、Boolean、String、Int16、UInt16
### 数据类型拓展
其它类型，可以通过修改LogUtilConfig.s_customArgTypesConfigs字典初始化值来适配用户自定义的结构类型。

### 使用说明
   1. 在游戏初始化时，调用FrameLogger.EvolutionManager.Init来初始化Log框架
   2. 战斗帧数变化时，调用FrameLogger.EvolutionManager.NextFrame，改变框架帧数
   3. 战斗结束时，调用FrameLogger.EvolutionManager.LogAllEvolution来保存日志。日志文件保存在Assets/../evo文件夹下，是一个二进制文件
   4. 在unity中按CTRL+W反序列化日志文件成明文。右键打开可以查看程序打点内容

### 安装教程
1.  下载代码到本地
2.  Assets\Scripts\FrameLogger文件夹中的所有内容拷贝到自己的项目工程中；将Assets\Plugins文件夹中的所有内容拷贝到自己的项目工程中
3.  打开cs文件LogUtilConfig，字段s_logPdbFilePath值按需需改，这个是logPdb文件目录
4.  打开cs文件LogUtilConfig，字段s_searchPaths值按需需改，这个是控制自动插入日志代码的文件夹目录
5.  打开cs文件LogUtilConfig，字段s_specSearchFilePaths值按需需改，这个是当所在目录已经配忽略，又在这个目录的某个文件中手动插入了日志，则需要在这里添加这个文件
6.  打开cs文件LogUtilConfig，字段s_chooseFuncNames值按需需改，这个是选择方法名，需要开启宏CHOOSE_FUNC，否则走ignore模式（不建议使用）
7.  打开cs文件LogUtilConfig，字段s_ignoreFolders值按需需改，这个是要屏蔽的文件夹路径，该文件夹下的所有文件都不会被自动插入日志
8.  打开cs文件LogUtilConfig，字段s_ignoreFiles值按需需改，这个是要屏蔽的文件路径，该文件都不会被自动插入日志
9.  打开cs文件LogUtilConfig，字段s_ignoreFuncNames值按需需改，这些函数都不会被自动插入日志
10.  打开cs文件LogUtilConfig，字段s_customArgTypesConfigs值按需需改，从左到右，第一个 nameof(FrameDebuggerTest.TestEnum)是枚举名，第二个typeof(FrameDebuggerTest.TestEnum)是类型，第三个$"(int){"#NAME#"}"意思是将枚举值转换为int使用。
11.  打开cs文件LogUtilConfig，字段s_customArgTypesConfigs值按需需改，从左到右，第一个 nameof(FrameDebuggerTest.SomeLogicObject)是类型名，第二个typeof(FrameDebuggerTest.SomeLogicObject)是类型，第三个$"{"#NAME#"}?.mapObjectId ?? 0"意思是使用类型SomeLogicObject的mapObjectId字段作为参数使用。

### 注意事项
1. 如果函数是空的，不会插入代码
2. 如果成员还是没有private/protected/public是不会自动插入日志代码的
3. 在函数中手动插入函数代码是，不要自己填id，等编译完成后点击菜单生成日志代码于符号表
4. 在忽略的文件夹中，手动插入日志代码，不会生成id。需要生成的话，在s_specSearchFilePaths里加上文件路径