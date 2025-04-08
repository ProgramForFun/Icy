using Icy.Base;
using System.IO;
using System.Text;
using UnityEditor;

namespace Icy.UI.Editor
{
	[InitializeOnLoad]
	public static class UICodeGeneratorEditor
	{
		static UICodeGeneratorEditor()
		{
			if (!EventManager.HasAlreadyListened(EventDefine.GenerateUICode, GenerateUICode))
				EventManager.AddListener(EventDefine.GenerateUICode, GenerateUICode);
			if (!EventManager.HasAlreadyListened(EventDefine.GenerateUILogicCode, GenerateUILogicCode))
				EventManager.AddListener(EventDefine.GenerateUILogicCode, GenerateUILogicCode);
			if (!EventManager.HasAlreadyListened(EventDefine.GenerateUICodeAll, GenerateAll))
				EventManager.AddListener(EventDefine.GenerateUICodeAll, GenerateAll);
		}

		private static void GenerateUICode(int eventID, IEventParam param)
		{
			if (param is EventParam<UICodeGenerator> paramGenerator)
			{
				UICodeGenerator generator = paramGenerator.Value;
				DoGenerateUICode(generator);
				AssetDatabase.Refresh();
			}
		}
		private static void DoGenerateUICode(UICodeGenerator generator)
		{
			string filePath = CheckGenerateCondition(generator.UIName, "");
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

				string code = string.Format(UICodeTemplate.Code, generator.UIName, builder.ToString());
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

		private static void GenerateAll(int eventID, IEventParam param)
		{
			if (param is EventParam<UICodeGenerator> paramGenerator)
			{
				DoGenerateUICode(paramGenerator.Value);
				DoGenerateUILogicCode(paramGenerator.Value.UIName);
				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		///  检查生成代码的前置条件
		/// </summary>
		private static string CheckGenerateCondition(string uiName, string typeName)
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

			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);
			return filePath;
		}
	}

	//public class ClassA : MonoBehaviour
	//{
	//	[SerializeField] private int number = 10;
	//	[SerializeField] private string text = "Hello";
	//}

	//public class ClassB : MonoBehaviour
	//{
	//	[SerializeField] private int number = 10;
	//	[SerializeField] private string text = "Hello";
	//}

	//public class ClassGeneratorEditor__ : EditorWindow
	//{
	//	[MenuItem("Tools/Generate ClassB")]
	//	public static void GenerateClassB()
	//	{
	//		string scriptPath = Path.Combine(Application.dataPath, "Scripts/ClassB.cs");

	//		string code = @"
	//using UnityEngine;

	//public class ClassB : MonoBehaviour
	//{
	//    [SerializeField] private int number;
	//    [SerializeField] private string text;
	//}";

	//		File.WriteAllText(scriptPath, code);
	//		AssetDatabase.Refresh();

	//		// 记录需要迁移的实例
	//		var instances = FindObjectsOfType<ClassA>();
	//		foreach (var instance in instances)
	//		{
	//			// 标记对象为脏以保存状态
	//			EditorUtility.SetDirty(instance.gameObject);
	//		}

	//		// 监听编译完成事件
	//		EditorApplication.update += WaitForCompilation;
	//	}

	//	private static void WaitForCompilation()
	//	{
	//		if (EditorApplication.isCompiling) return;

	//		EditorApplication.update -= WaitForCompilation;
	//		MigrateData();
	//	}

	//	private static void MigrateData()
	//	{
	//		foreach (var oldComponent in FindObjectsOfType<ClassA>())
	//		{
	//			var go = oldComponent.gameObject;

	//			// 创建ClassB并复制数据
	//			ClassB newComponent = go.AddComponent<ClassB>();
	//			SerializedObject source = new SerializedObject(oldComponent);
	//			SerializedObject dest = new SerializedObject(newComponent);

	//			// 复制所有序列化属性
	//			SerializedProperty prop = source.GetIterator();
	//			while (prop.NextVisible(true))
	//			{
	//				if (prop.name == "m_Script") continue;
	//				dest.CopyFromSerializedProperty(prop);
	//			}

	//			dest.ApplyModifiedProperties();

	//			// 移除旧组件
	//			DestroyImmediate(oldComponent);
	//		}

	//		AssetDatabase.SaveAssets();
	//	}
	//}
}
