using UnityEngine;
using System.Collections.Generic;
using Icy.Base;
#if UNITY_EDITOR
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
				EventManager.AddListener(EventDefine.UICodeGeneratorNameChanged, ValidateDuplicateName);
				_Inited = true;
				_Disposed = false;
			}
		}

		private void OnTableListChanged(CollectionChangeInfo info, object value)
		{
			if (info.ChangeType == CollectionChangeType.RemoveValue || info.ChangeType == CollectionChangeType.RemoveKey || info.ChangeType == CollectionChangeType.RemoveIndex)
			{
				if (Components.Count > 0)
					ValidateDuplicateName(EventDefine.UICodeGeneratorNameChanged, new EventParam<UICodeGeneratorItem> { Value = Components[0] });
			}
		}

		[Space(10)]
		[InfoBox("For instance, if you want to generate codes for UISetting, input *Setting* here, WITHOUT prefix UI")]
		[LabelText("UI Name :")]
		[LabelWidth(80)]
		public string UIName;

		/// <summary>
		/// 有字段重名时，红色提示
		/// </summary>
		private void ValidateDuplicateName(int eventID, IEventParam item)
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

		[ButtonGroup]
		[Button("Generate UI Code", ButtonSizes.Medium), GUIColor(0, 1, 0)]
		public void GenerateUICode()
		{
			EventManager.Trigger(EventDefine.GenerateUICode, new EventParam<UICodeGenerator> { Value = this});
		}

		[ButtonGroup]
		[Button("Generate Logic Code", ButtonSizes.Medium), GUIColor(0, 1, 0)]
		public void GenerateLogicCode()
		{
			EventManager.Trigger(EventDefine.GenerateUILogicCode, new EventParam_String { Value = UIName });
		}

		[Button("Generate All", ButtonSizes.Medium), GUIColor(0, 1, 0)]
		public void GenerateAll()
		{
			EventManager.Trigger(EventDefine.GenerateUICodeAll, new EventParam<UICodeGenerator> { Value = this });
		}

		private void OnInspectorDispose()
		{
			if (!_Disposed)
			{
				EventManager.RemoveListener(EventDefine.UICodeGeneratorNameChanged, ValidateDuplicateName);
				_Disposed = true;
				_Inited = false;
			}
		}
#else
		public sealed class UICodeGenerator : MonoBehaviour
		{
			private void Awake()
			{
				Destroy(this);
			}
		}
#endif
	}
}
