using Icy.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Icy.UI.Editor
{
	/// <summary>
	/// UI代码生成器的逻辑部分，Editor UI部分见UICodeGenerator类
	/// </summary>
	[InitializeOnLoad]
	public class UICodeGeneratorEditor
	{
		private const string GENERATING_UI_NAME_KEY = "_Icy_GeneratingUIName";

		static UICodeGeneratorEditor()
		{
			AssemblyReloadEvents.afterAssemblyReload -= OnAllAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAllAssemblyReload;

			if (!EventManager.HasAlreadyListened(EventDefine.GenerateUICode, GenerateUICode))
				EventManager.AddListener(EventDefine.GenerateUICode, GenerateUICode);
			if (!EventManager.HasAlreadyListened(EventDefine.GenerateUILogicCode, GenerateUILogicCode))
				EventManager.AddListener(EventDefine.GenerateUILogicCode, GenerateUILogicCode);
			if (!EventManager.HasAlreadyListened(EventDefine.GenerateUICodeBoth, GenerateBoth))
				EventManager.AddListener(EventDefine.GenerateUICodeBoth, GenerateBoth);
		}

		private static void OnAllAssemblyReload()
		{
			string generatingUIName = LocalPrefs.GetString(GENERATING_UI_NAME_KEY, "");
			if (!string.IsNullOrEmpty(generatingUIName))
			{
				CopySerializeField("UI" + generatingUIName);

				LocalPrefs.RemoveKey(GENERATING_UI_NAME_KEY);
				LocalPrefs.Save();
			}
		}

		private static void GenerateUICode(int eventID, IEventParam param)
		{
			if (param is EventParam<UICodeGenerator> paramGenerator)
			{
				UICodeGenerator generator = paramGenerator.Value;
				bool isLogicFileExist = IsLogicFileExist(generator.UIName);
				DoGenerateUICode(generator, isLogicFileExist);
				AssetDatabase.Refresh();
				LocalPrefs.SetString(GENERATING_UI_NAME_KEY, generator.UIName);
				LocalPrefs.Save();
			}
		}
		private static void DoGenerateUICode(UICodeGenerator generator, bool withLogic)
		{
			string filePath = CheckGenerateCondition(generator.UIName, "", generator.Components);
			if (filePath != null)
			{
				StringBuilder builder = new StringBuilder();
				int count = generator.Components.Count;
				for (int i = 0; i < count; i++)
				{
					string className = generator.Components[i].Component.GetType().FullName;
					string line = string.Format("[SerializeField, ReadOnly] {0} _{1};", className, generator.Components[i].Name);
					if (i >= count - 1)
						builder.Append(line);
					else
						builder.AppendLine(line);
				}

				string logicTypeName = string.Format("UI{0}Logic", generator.UIName);
				string logicDecl = withLogic ? string.Format("\r\n	private {0} _Logic;\r\n", logicTypeName) : "";
				string logicAssign = withLogic ? "		_Logic = new();\r\n		_Logic.Init();\r\n" : "";
				string code = string.Format(UICodeTemplate.Code, generator.UIName, builder.ToString(), logicDecl, logicAssign);
				File.WriteAllText(filePath, code);
			}
		}

		private static void GenerateUILogicCode(int eventID, IEventParam param)
		{
			if (param is EventParam_String paramString)
			{
				DoGenerateUILogicCode(paramString.Value);
				AssetDatabase.Refresh();
			}
		}
		private static void DoGenerateUILogicCode(string uiName)
		{
			string filePath = CheckGenerateCondition(uiName, "Logic");
			if (filePath != null)
			{
				//EditorUtility.DisplayProgressBar("UI", "Generating UI Logic code...", 0.5f);
				string code = string.Format(UILogicCodeTemplate.Code, uiName);
				File.WriteAllText(filePath, code);
			}
		}

		private static void GenerateBoth(int eventID, IEventParam param)
		{
			if (param is EventParam<UICodeGenerator> paramGenerator)
			{
				DoGenerateUILogicCode(paramGenerator.Value.UIName);
				DoGenerateUICode(paramGenerator.Value, true);
				AssetDatabase.Refresh();
				LocalPrefs.SetString(GENERATING_UI_NAME_KEY, paramGenerator.Value.UIName);
				LocalPrefs.Save();
			}
		}

		/// <summary>
		/// 检查生成代码的前置条件
		/// </summary>
		private static string CheckGenerateCondition(string uiName, string typeName, List<UICodeGeneratorItem> components = null)
		{
			string uiRootPath = LocalPrefs.GetString("_Icy_UIRootPath", "");
			if (string.IsNullOrEmpty(uiRootPath))
			{
				Log.LogError($"Generate UI {typeName} code failed, please set UI root path first. Go to menu Icy/UI/Setting to set it");
				return null;
			}

			if (string.IsNullOrEmpty(uiName))
			{
				Log.LogError($"Generate UI {typeName} code failed, UI name is null or empty");
				return null;
			}

			string fileName = string.Format("UI{0}{1}.cs", uiName, typeName);
			string folderPath = Path.Combine(uiRootPath, uiName);
			string filePath = Path.Combine(uiRootPath, uiName, fileName);
			if (File.Exists(filePath))
			{
				Log.LogError($"Generate UI {typeName} code failed, {filePath} is alreay exist");
				return null;
			}

			if (components != null)
			{
				int count = components.Count;
				for (int i = 0; i < count; i++)
				{
					if (!CSharpVariableValidator.IsValidCSharpVariableName(components[i].Name))
					{
						Log.LogError($"Generate UI {typeName} code failed, {components[i].Name} is NOT a valid C# variable name");
						return null;
					}
				}
			}

			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);
			return filePath;
		}

		/// <summary>
		/// 把序列化引用，从UICodeGenerator，copy到生成的UI类上
		/// </summary>
		private static void CopySerializeField(string uiTypeName)
		{
			//string filter = $"\"{uiTypeName}\" t:prefab";
			//string[] guids = AssetDatabase.FindAssets(filter);
			//string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			//GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

			GameObject prefabInstance = Selection.activeGameObject;
			if (prefabInstance == null)
				return;
			UIBase ui = prefabInstance.GetComponent(uiTypeName) as UIBase;
			if (ui == null)
			{
				string typeName = string.Format("{0}, Assembly-CSharp", uiTypeName);
				Type type = Type.GetType(typeName);
				if (type == null)
				{
					Log.LogError("Get type just generated failed, type name = " + typeName, "UICodeGeneratorEditor");
					return;
				}
				//不能直接用prefabInstance.AddComponent，会Inspector刷新不及时，看不到新挂上的UI脚本
				ui = Undo.AddComponent(prefabInstance, type) as UIBase;
			}

			//复制序列化的引用
			UICodeGenerator generator = prefabInstance.GetComponent<UICodeGenerator>();
			SerializedObject target = new SerializedObject(ui);
			target.Update();

			for (int i = 0; i < generator.Components.Count; i++)
			{
				string fieldName = "_" + generator.Components[i].Name;
				SerializedProperty p = target.FindProperty(fieldName);
				if (p == null)
				{
					Log.LogError("Copy serialized field failed, filed name = " + fieldName, "UICodeGeneratorEditor");
					continue;
				}

				p.objectReferenceValue = generator.Components[i].Object;
			}

			target.ApplyModifiedPropertiesWithoutUndo();

			PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
		}

		private static bool IsLogicFileExist(string uiName)
		{
			string uiRootPath = LocalPrefs.GetString("_Icy_UIRootPath", "");
			if (string.IsNullOrEmpty(uiRootPath))
				return false;//其他地方有log，这里就不输出了

			string fileName = string.Format("UI{0}Logic.cs", uiName);
			string filePath = Path.Combine(uiRootPath, uiName, fileName);
			return File.Exists(filePath);
		}
	}
}
