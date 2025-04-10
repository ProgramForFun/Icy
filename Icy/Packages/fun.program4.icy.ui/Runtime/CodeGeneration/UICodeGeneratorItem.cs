#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using Icy.Base;
using Sirenix.OdinInspector;

namespace Icy.UI
{
	/// <summary>
	/// UI代码生成相关，编辑器里拖上去的item
	/// </summary>
	[Serializable]
	public sealed class UICodeGeneratorItem
	{
		/// <summary>
		/// 忽略掉，不显示在dropdown里的组件
		/// </summary>
		private static HashSet<string> IGNORE_COMPONENTS = new HashSet<string>() { "CanvasRenderer" };

		/// <summary>
		/// 置为true的话，Name显示红色
		/// </summary>
		[NonSerialized]
		public bool RedName = false;

		/// <summary>
		/// 拖上来的组件所属的物体
		/// </summary>
		//[TableColumnWidth(33, Resizable = false)]
		//[PreviewField(33, Alignment = ObjectFieldAlignment.Center)]
		[GUIColor("GetCompColor")]
		[OnValueChanged("OnObjectChanged")]
		public GameObject Object;

		/// <summary>
		/// 生成到C#代码的变量名
		/// </summary>
		[Delayed]
		[GUIColor("GetNameColor")]
		[OnValueChanged("OnNameChanged")]
		public string Name;

		/// <summary>
		/// 生成到C#代码的变量类型
		/// </summary>
		[ValueDropdown("_AllComponents", IsUniqueList = true)]
		public UnityEngine.Object Component;

		/// <summary>
		/// 拖上来的物体上面所有的组件，不包括忽略的
		/// </summary>
		[OnInspectorInit("OnInspectorInit")]
		private ValueDropdownList<UnityEngine.Object> _AllComponents;

		/// <summary>
		/// 更新Item的数据和显示
		/// </summary>
		private void UpdateComponents()
		{
			if (Object != null)
			{
				Component[] all = Object.GetComponents<Component>();
				_AllComponents = new ValueDropdownList<UnityEngine.Object>();
				for (int i = 0; i < all.Length; i++)
				{
					Component comp = all[i];
					string compName = comp.GetType().Name;
					if (comp is not Transform && comp is not RectTransform && !IGNORE_COMPONENTS.Contains(compName))
						_AllComponents.Add(compName, comp);
				}
				_AllComponents.Sort(SortComponents);

				//Transfrom/RectTransform和GameObject放在选项最后
				_AllComponents.Add(Object.transform is RectTransform ? "RectTransform" : "Transform", Object.transform);
				_AllComponents.Add("GameObject", Object);

				//自动选一个，目前默认选第一个
				if (Component == null)
					Component = _AllComponents[0].Value;
			}
			else
			{
				if (_AllComponents != null)
					_AllComponents.Clear();
				Component = null;
			}
		}

		private int SortComponents(ValueDropdownItem<UnityEngine.Object> x, ValueDropdownItem<UnityEngine.Object> y)
		{
			return x.Value.GetType().Name.CompareTo(y.Value.GetType().Name);
		}

		/// <summary>
		/// Comp没有引用时，显示红色
		/// </summary>
		private Color GetCompColor()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
			return Object == null ? Color.red : Color.white;
		}

		/// <summary>
		/// Name为空 或 主动设置时，显示红色
		/// </summary>
		private Color GetNameColor()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
			return string.IsNullOrEmpty(Name) || RedName ? Color.red : Color.white;
		}

		private void OnInspectorInit()
		{
			UpdateComponents();
		}

		private void OnObjectChanged()
		{
			if (Object != null)
			{
				Name = Object.name;
				OnNameChanged();
			}
			else
				Name = string.Empty;

			UpdateComponents();
		}

		private void OnNameChanged()
		{
			EventManager.Trigger(EventDefine.UICodeGeneratorNameChanged, new EventParam<UICodeGeneratorItem> { Value = this});
		}
	}
}
#endif
