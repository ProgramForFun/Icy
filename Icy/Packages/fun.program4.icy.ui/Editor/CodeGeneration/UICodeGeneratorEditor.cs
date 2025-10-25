/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using Icy.Base;
using Icy.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Icy.UI.Editor
{
	/// <summary>
	/// UI代码生成器的逻辑部分，Editor UI部分见UICodeGenerator类；
	/// 注意，此类被UICodeGenerator反射依赖；
	/// </summary>
	public class UICodeGeneratorEditor
	{
		private const string GENERATING_UI_NAME_KEY = "_Icy_GeneratingUIName";
		private const string GENERATING_UI_LOGIC_NAME_KEY = "_Icy_GeneratingUILogicName";

		private const string UI_CODE_TEMPLATE_PATH = "Packages/fun.program4.icy.ui/Editor/CodeGeneration/UICodeTemplate.txt";
		private const string UI_LOGIC_CODE_TEMPLATE_PATH = "Packages/fun.program4.icy.ui/Editor/CodeGeneration/UILogicCodeTemplate.txt";

		private static string _UIRootDir;


		[InitializeOnLoadMethod]
		static void Init()
		{
			AssemblyReloadEvents.afterAssemblyReload -= OnAllAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAllAssemblyReload;
			CompilationPipeline.compilationFinished -= OnCompilationFinished;
			CompilationPipeline.compilationFinished += OnCompilationFinished;
		}

		private static void OnAllAssemblyReload()
		{
			string generatingUIName = EditorLocalPrefs.GetString(GENERATING_UI_NAME_KEY, "");
			if (!string.IsNullOrEmpty(generatingUIName))
			{
				//只有非play、编辑Prefab状态下，才执行生成代码
				PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
				if (!EditorApplication.isPlaying && stage != null)
				{
					CopySerializeField("UI" + generatingUIName);
					EditorUtility.DisplayDialog("Generate UI Code", "生成UI代码完成", "OK");
					BiProgress.Hide();
				}

				EditorApplication.delayCall += () =>
				{
					EditorLocalPrefs.RemoveKey(GENERATING_UI_NAME_KEY);
					EditorLocalPrefs.Save();
				};
			}

			string generatingLogicUIName = EditorLocalPrefs.GetString(GENERATING_UI_LOGIC_NAME_KEY, "");
			if (!string.IsNullOrEmpty(generatingLogicUIName))
			{
				EditorUtility.DisplayDialog("Generate UI Code", "生成UI Logic代码完成", "OK");
				BiProgress.Hide();

				EditorApplication.delayCall += () =>
				{
					EditorLocalPrefs.RemoveKey(GENERATING_UI_LOGIC_NAME_KEY);
					EditorLocalPrefs.Save();
				};
			}
		}

		private static void OnCompilationFinished(object _)
		{
			EditorApplication.delayCall += () =>
			{
				string generatingUIName = EditorLocalPrefs.GetString(GENERATING_UI_NAME_KEY, "");
				string generatingLogicUIName = EditorLocalPrefs.GetString(GENERATING_UI_LOGIC_NAME_KEY, "");
				//有编译错误时，关闭ProgressBar，避免卡死edtior
				if (EditorUtility.scriptCompilationFailed && (!string.IsNullOrEmpty(generatingUIName) || !string.IsNullOrEmpty(generatingLogicUIName)))
				{
					EditorUtility.DisplayDialog("", $"已生成代码，但有编译错误，如果是删除、改名、改类型了{nameof(UICodeGenerator)}中的字段，这个报错是正常的", "OK");
					BiProgress.Hide();
					EditorLocalPrefs.RemoveKey(GENERATING_UI_NAME_KEY);
					EditorLocalPrefs.RemoveKey(GENERATING_UI_LOGIC_NAME_KEY);
					EditorLocalPrefs.Save();
				}
			};
		}

		/// <summary>
		/// 注意，此方法被UICodeGenerator反射依赖
		/// </summary>
		private static void GenerateUICode(UICodeGenerator generator)
		{
			BiProgress.Show("Generate UI Code", "Generating UI Code", 0.5f);

			bool isLogicFileExist = IsLogicFileExist(generator.UIName);

			EditorLocalPrefs.SetString(GENERATING_UI_NAME_KEY, generator.UIName);
			EditorLocalPrefs.Save();

			DoGenerateUICode(generator, isLogicFileExist);
			AssetDatabase.Refresh();
		}
		private static bool DoGenerateUICode(UICodeGenerator generator, bool withLogic)
		{
			string filePath = CheckGenerateCondition(generator.UIName, "", generator.Components);
			if (filePath != null)
			{
				//生成所有组件的代码
				List<string> componentCode = new List<string>(8);
				int count = generator.Components.Count;
				for (int i = 0; i < count; i++)
				{
					string className = generator.Components[i].Component.GetType().FullName;
					string line = string.Format("	[SerializeField, ReadOnly] {0} _{1};", className, generator.Components[i].Name);
					componentCode.Add(line);
				}

				if (!File.Exists(filePath))
				{
					//不存在文件就是全新生成
					string logicTypeName = string.Format("UI{0}Logic", generator.UIName);
					string logicDecl = withLogic ? string.Format("\r\n	private {0} _Logic;\r\n", logicTypeName) : "";
					string logicAssign = withLogic ? "		_Logic = new();\r\n		_Logic.Init();\r\n" : "";
					string componentCodeToMultiLine = string.Join("\r\n", componentCode);
					string template = File.ReadAllText(UI_CODE_TEMPLATE_PATH);
					string finalCodes = string.Format(template, generator.UIName, componentCodeToMultiLine, logicDecl, logicAssign);
					File.WriteAllText(filePath, finalCodes);
				}
				else
				{
					//存在的话，替换组件部分代码
					string[] oldLines = File.ReadAllLines(filePath);
					List<string> newlines = new List<string>(128);

					//组件代码之前的内容，原样保留
					int lineIdx = 0;
					for (;lineIdx < oldLines.Length; lineIdx++)
					{
						newlines.Add(oldLines[lineIdx]);
						if (oldLines[lineIdx].StartsWith("//↓="))
							break;
					}

					//插入新的组件代码
					newlines.AddRange(componentCode);

					bool foundGeneratedAreaEnd = false;
					for (; lineIdx < oldLines.Length; lineIdx++)
					{
						if (oldLines[lineIdx].StartsWith("//↑=") && !foundGeneratedAreaEnd)
							foundGeneratedAreaEnd = true;

						if (foundGeneratedAreaEnd)
							newlines.Add(oldLines[lineIdx]);
					}

					//有变化才写入文件，否则给出提示
					if (oldLines.Length != newlines.Count)
						File.WriteAllLines(filePath, newlines);
					else
					{
						bool isSame = true;
						for (int i = 0; i < oldLines.Length; i++)
						{
							if (oldLines[i] != newlines[i])
							{
								isSame = false;
								break;
							}
						}

						if (isSame)
						{
							//代码虽然一样，但实际引用的UI节点可能变了，所以这里要手动调一下去CopySerializeField
							OnAllAssemblyReload();
						}
						else
							File.WriteAllLines(filePath, newlines);
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// 注意，此方法被UICodeGenerator反射依赖
		/// </summary>
		private static void GenerateUILogicCode(UICodeGenerator generator)
		{
			BiProgress.Show("Generate UI Code", "Generating UI Logic Code", 0.5f);

			bool generated = DoGenerateUILogicCode(generator.UIName);
			if (generated)
			{
				EditorLocalPrefs.SetString(GENERATING_UI_LOGIC_NAME_KEY, generator.UIName);
				EditorLocalPrefs.Save();
			}
			else
			{
				EditorUtility.DisplayDialog("Generate UI Code", "生成UI Logic代码失败，文件已存在", "OK");
				BiProgress.Hide();
			}

			AssetDatabase.Refresh();
		}
		private static bool DoGenerateUILogicCode(string uiName)
		{
			string filePath = CheckGenerateCondition(uiName, "Logic");
			if (filePath != null)
			{
				string template = File.ReadAllText(UI_LOGIC_CODE_TEMPLATE_PATH);
				string code = string.Format(template, uiName);
				File.WriteAllText(filePath, code);
				return true;
			}
			return false;
		}

		/// <summary>
		/// 注意，此方法被UICodeGenerator反射依赖
		/// </summary>
		private static void GenerateBoth(UICodeGenerator generator)
		{
			BiProgress.Show("Generate UI Code", "Generating UI and UI Logic Code", 0.5f);

			EditorLocalPrefs.SetString(GENERATING_UI_NAME_KEY, generator.UIName);
			EditorLocalPrefs.Save();

			bool generatedLogic = DoGenerateUILogicCode(generator.UIName);
			DoGenerateUICode(generator, true);

			if (generatedLogic)
			{
				EditorLocalPrefs.SetString(GENERATING_UI_LOGIC_NAME_KEY, generator.UIName);
				EditorLocalPrefs.Save();
			}
			else
				EditorUtility.DisplayDialog("Generate UI Code", "生成UI Logic代码失败，文件已存在", "OK");

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// 检查生成代码的前置条件
		/// </summary>
		private static string CheckGenerateCondition(string uiName, string typeName, List<UICodeGeneratorItem> components = null)
		{
			string uiRootDir = GetUIRootDir();
			if (string.IsNullOrEmpty(uiRootDir))
			{
				Log.Error($"Generate UI {typeName} code failed, please set UI root path first. Go to menu Icy/UI/Setting to set it");
				return null;
			}

			if (string.IsNullOrEmpty(uiName))
			{
				Log.Error($"Generate UI {typeName} code failed, UI name is null or empty");
				return null;
			}

			string fileName = string.Format("UI{0}{1}.cs", uiName, typeName);
			string folderPath = Path.Combine(uiRootDir, uiName);
			string filePath = Path.Combine(uiRootDir, uiName, fileName);
			if (File.Exists(filePath) && !string.IsNullOrEmpty(typeName))
			{
				Log.Error($"Generate UI {typeName} code failed, {filePath} is alreay exist");
				return null;
			}

			if (components != null)
			{
				int count = components.Count;
				for (int i = 0; i < count; i++)
				{
					if (!CSharpVariableValidator.IsValidCSharpVariableName(components[i].Name))
					{
						Log.Error($"Generate UI {typeName} code failed, {components[i].Name} is NOT a valid C# variable name");
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
					Log.Error("Get type just generated failed, type name = " + typeName, "UICodeGeneratorEditor");
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
					Log.Error("Copy serialized field failed, filed name = " + fieldName, "UICodeGeneratorEditor");
					continue;
				}

				p.objectReferenceValue = generator.Components[i].Object;
			}

			target.ApplyModifiedPropertiesWithoutUndo();

			PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
		}

		private static bool IsLogicFileExist(string uiName)
		{
			string uiRootDir = GetUIRootDir();
			if (string.IsNullOrEmpty(uiRootDir))
				return false;//其他地方有log，这里就不输出了

			string fileName = string.Format("UI{0}Logic.cs", uiName);
			string filePath = Path.Combine(uiRootDir, uiName, fileName);
			return File.Exists(filePath);
		}

		private static string GetUIRootDir()
		{
			if (string.IsNullOrEmpty(_UIRootDir))
			{
				byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetEditorOnlySettingDir(), SettingsHelper.UISetting);
				if (bytes != null)
				{
					UISetting uiSetting = UISetting.Parser.ParseFrom(bytes);
					return uiSetting.UIRootDir;
				}
				else
					return null;
			}
			else
				return _UIRootDir;
		}
	}
}
