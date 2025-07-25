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


using UnityEngine;
using System.Collections.Generic;
using Icy.Base;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

namespace Icy.UI
{
#if UNITY_EDITOR
	/// <summary>
	/// 生成UI代码的挂载在prefab上的Editor UI部分；
	/// 因为要挂在prefab上，所以没有放到Editor目录下；
	/// 具体的生成逻辑见UICodeGeneratorEditor类
	/// </summary>
	[HideMonoScript]
	public sealed class UICodeGenerator : MonoBehaviour
	{
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		[OnInspectorInit("OnInspectorInit")]
		[OnInspectorDispose("OnInspectorDispose")]
		[OnCollectionChanged("OnTableListChanged")]
		public List<UICodeGeneratorItem> Components = new List<UICodeGeneratorItem>();

		private Dictionary<string, UICodeGeneratorItem> _ForDuplicateName = new Dictionary<string, UICodeGeneratorItem>();
		private bool _Inited = false;
		private bool _Disposed = false;


		private void OnInspectorInit()
		{
			if (!_Inited)
			{
				EventManager.AddListener(EventDefine.UICodeGeneratorNameChanged, ValidateName);
				_Inited = true;
				_Disposed = false;
			}

			if (Components.Count > 0)
				ValidateName(EventDefine.UICodeGeneratorNameChanged, new EventParam<UICodeGeneratorItem> { Value = Components[0] });

			//检查prefab前缀
			string goName = gameObject.name;
			if (goName.StartsWith("UI"))
			{
				UIName = goName.Substring(2);
			}
			else
			{
				EditorUtility.DisplayDialog("Error", "UI prefab的命名应该以UI作为前缀", "这就改");
				UIName = "UI prefab的命名应该以UI作为前缀";
			}

			//检查类名和prefab是否一致
			UIBase uiBase = gameObject.GetComponent<UIBase>();
			if (uiBase != null)
			{
				string className = uiBase.GetType().Name;
				if (goName != className)
					Log.Assert(false, $"UI Prefab and UI class must have the same name, prefab = {goName}, class = {className}");
			}
		}

		private void OnTableListChanged(CollectionChangeInfo info, object value)
		{
			if (info.ChangeType == CollectionChangeType.RemoveValue || info.ChangeType == CollectionChangeType.RemoveKey || info.ChangeType == CollectionChangeType.RemoveIndex)
			{
				if (Components.Count > 0)
					ValidateName(EventDefine.UICodeGeneratorNameChanged, new EventParam<UICodeGeneratorItem> { Value = Components[0] });
			}
		}

		[Space(10)]
		[InfoBox("UI name is just the UI prefab name without prefix 'UI'")]
		[LabelText("UI Name :")]
		[LabelWidth(80)]
		[ReadOnly]
		public string UIName;

		/// <summary>
		/// 有字段重名时，红色提示
		/// </summary>
		private void ValidateName(int eventID, IEventParam item)
		{
			if (item is EventParam<UICodeGeneratorItem> eventParam)
			{
				if (Components.Contains(eventParam.Value))
				{
					_ForDuplicateName.Clear();
					for (int i = 0; i < Components.Count; i++)
					{
						Components[i].RedName = false;
						string name = Components[i].Name;

						//检测命名合法性
						bool isValidName = CSharpVariableValidator.IsValidCSharpVariableName(name);
						if (!isValidName)
							Components[i].RedName = true;

						//检测重名
						if (_ForDuplicateName.ContainsKey(name))
						{
							Components[i].RedName = true;
							_ForDuplicateName[name].RedName = true;
						}
						else
							_ForDuplicateName.Add(name, Components[i]);
					}
				}
			}
		}

		[Button("Generate Logic Code", ButtonSizes.Medium), GUIColor(0, 1, 0)]
		public void GenerateLogicCode()
		{
			EventParam_String eventParam = EventManager.GetParam<EventParam_String>();
			eventParam.Value = UIName;
			EventManager.Trigger(EventDefine.GenerateUILogicCode, eventParam);
		}

		[Button("Generate UI Code", ButtonSizes.Medium), GUIColor(0, 1, 0)]
		public void GenerateUICode()
		{
			EventParam<UICodeGenerator> eventParam = EventManager.GetParam<EventParam<UICodeGenerator>>();
			eventParam.Value = this;
			EventManager.Trigger(EventDefine.GenerateUICode, eventParam);
		}

		[Button("Generate Both", ButtonSizes.Medium), GUIColor(0, 1, 0)]
		public void GenerateBoth()
		{
			EventParam<UICodeGenerator> eventParam = EventManager.GetParam<EventParam<UICodeGenerator>>();
			eventParam.Value = this;
			EventManager.Trigger(EventDefine.GenerateUICodeBoth, eventParam);
		}

		private void OnInspectorDispose()
		{
			if (!_Disposed)
			{
				EventManager.RemoveListener(EventDefine.UICodeGeneratorNameChanged, ValidateName);
				_Disposed = true;
				_Inited = false;
			}
		}

		#region 快捷添加
		/// <summary>
		/// 选中UI节点，右键快加添加生成器，支持多选；
		/// 或者选中后按回车
		/// </summary>
		[MenuItem("GameObject/Add To UI Code Generator _SPACE", false, -100)]
		private static void AddComponentByEditorSelection()
		{
			UICodeGenerator generator = null;
			GameObject[] allSelect = Selection.gameObjects;
			if (allSelect.Length == 0)
				return;
			for (int i = 0; i < allSelect.Length; i++)
			{
				Transform parent = allSelect[i].transform.parent;
				while(parent != null)
				{
					generator = parent.gameObject.GetComponent<UICodeGenerator>();
					if (generator != null)
						break;
					parent = parent.parent;
				}

				if (generator != null)
				{
					UICodeGeneratorItem item = new UICodeGeneratorItem();
					generator.Components.Add(item);
					item.Object = allSelect[i];
					item.Name = allSelect[i].name;
				}
			}

			EditorUtility.SetDirty(generator.gameObject);
			AssetDatabase.SaveAssets();

			if (generator.Components.Count > 0)
				generator.ValidateName(EventDefine.UICodeGeneratorNameChanged, new EventParam<UICodeGeneratorItem> { Value = generator.Components[0] });

			EditorApplication.delayCall += () =>
			{ 
				Selection.activeGameObject = generator.gameObject;
			};
		}
		#endregion
	}
#else
	public sealed class UICodeGenerator : MonoBehaviour
	{

	}
#endif
}
