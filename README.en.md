# frameLogger

#### Description
FrameLogger is a tool framework for recording the execution records of each frame in a frame synchronization game.

#### Supported Data Types
FrameLogger supports 9 basic parameter types: Int32, UInt32, Int64, UInt64, Boolean, String, Int16, UInt16.

#### Data Type Extension
Other types can be adapted by modifying the initialization values of the LogUtilConfig.s_customArgTypesConfigs dictionary.

#### Usage
1. Call FrameLogger.EvolutionManager.Init to initialize the logging framework during game initialization.
2. Call FrameLogger.EvolutionManager.NextFrame to change the frame count when the frame count changes in battle.
3. Call FrameLogger.EvolutionManager.LogAllEvolution to save the log when the battle ends. The log file is saved in the Assets/…/evo folder and is a binary file.
4. Press CTRL+W in Unity to deserialize the log file into plain text. Right-click to open and view the program’s debug output.

### Installation Guide
1.  Download the code to your local machine.
2.  Copy all the contents of the Assets\Scripts\FrameLogger folder to your project.Copy all the contents of the Assets\Plugins folder to your project.
3.  Open the cs file LogUtilConfig and modify the value of the s_logPdbFilePath field as needed. This is the directory of the logPdb file.
4.  Open the cs file LogUtilConfig and modify the value of the s_searchPaths field as needed. This is the folder directory where automatic logging code insertion is controlled.
5.  Open the cs file LogUtilConfig and modify the value of the s_specSearchFilePaths field as needed. This is the file that needs to be added here when the directory is already ignored, but manual logging code is inserted in a file in this directory.
6.  Open the cs file LogUtilConfig and modify the value of the s_chooseFuncNames field as needed. This is the list of function names. If the CHOOSE_FUNC macro is enabled, it will use this list. Otherwise, it will use the ignore mode (not recommended).
7.  Open the cs file LogUtilConfig and modify the value of the s_ignoreFolders field as needed. These are the folder paths to be ignored. All files in this folder will not be automatically inserted with logging code.
8.  Open the cs file LogUtilConfig and modify the value of the s_ignoreFiles field as needed. These are the file paths to be ignored. These files will not be automatically inserted with logging code.
9.  Open the cs file LogUtilConfig and modify the value of the s_ignoreFuncNames field as needed. These functions will not be automatically inserted with logging code.
10.  Open the cs file LogUtilConfig and modify the value of the s_customArgTypesConfigs field as needed. From left to right, the first nameof(FrameDebuggerTest.TestEnum) is the enum name, the second typeof(FrameDebuggerTest.TestEnum) is the type, and the third $“(int){”#NAME#“}” means converting the enum value to int.
11.  Open the cs file LogUtilConfig and modify the value of the s_customArgTypesConfigs field as needed. From left to right, the first nameof(FrameDebuggerTest.SomeLogicObject) is the type name, the second typeof(FrameDebuggerTest.SomeLogicObject) is the type, and the third $“{”#NAME#“}?.mapObjectId ?? 0” means using the mapObjectId field of the SomeLogicObject type as the parameter.

### Notes
1. If a function is empty, no code will be inserted.
2. If a member does not have private/protected/public, logging code will not be automatically inserted.
3. When manually inserting logging code in a function, do not fill in the id yourself. After compiling, click the menu to generate the logging code and symbol table.
4. Manually inserting logging code in ignored folders will not generate an id. If you need to generate one, add the file path to s_specSearchFilePaths.
