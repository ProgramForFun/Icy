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
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.UI
{
	[HideMonoScript]
	public class StatusSwitcher : MonoBehaviour
	{
		protected List<StatusSwitcherTarget> _SwitcherTargetList = new List<StatusSwitcherTarget>();

		/// <summary>
		/// 所有的Status
		/// </summary>
		[ListDrawerSettings(ShowItemCount = true, DraggableItems = true, ShowFoldout = false, HideAddButton = true)]
		[SerializeField]
		protected List<StatusName> _StatusList = new List<StatusName>();

		/// <summary>
		/// 当前在Dirty状态的Status
		/// </summary>
		internal string CurrDirtyStatus;


		/// <summary>
		/// 切换到指定名字的状态
		/// </summary>
		public void SwitchTo(string statusName)
		{
			for (int i = 0; i < _StatusList.Count; i++)
			{
				if (_StatusList[i].Name == statusName)
				{

					return;
				}
			}
			Log.Error($"Status switching failed, {gameObject.name} has NO status called {statusName}", nameof(StatusSwitcher));
		}

#if UNITY_EDITOR
		[HorizontalGroup("Add")]
		[ShowInInspector]
		[HideLabel]
		protected string _InputName;

		[EnableIf(nameof(IsValidName))]
		[HorizontalGroup("Add")]
		[Button("Add", ButtonSizes.Medium, Icon = SdfIconType.PlusCircleFill)]
		protected void AddNewStatus()
		{
			_StatusList.Add(new StatusName(this, _InputName));
		}

		protected bool IsValidName()
		{
			return !string.IsNullOrEmpty(_InputName);
		}

		internal void DeleteStatus(string statusName)
		{

		}

		internal void EditStatus(string statusName)
		{

		}
#endif
	}

	[Serializable]
	public class StatusName
	{
		/// <summary>
		/// 状态名字
		/// </summary>
		[HideInInspector]
		[SerializeField]
		public string Name;

		/// <summary>
		/// 所属StatusSwitcher
		/// </summary>
		[HideInInspector]
		[SerializeField]
		protected StatusSwitcher _StatusSwitcher;

		/// <summary>
		/// 状态是否在编辑中
		/// </summary>
		[NonSerialized]
		protected bool _IsDirty = false;

		protected string _DisplayName => _IsDirty? Name + "（Editing...）" : Name;

		public StatusName(StatusSwitcher statusSwitcher, string name)
		{
			_StatusSwitcher = statusSwitcher;
			Name = name;
		}

		/// <summary>
		/// 状态按钮本体
		/// </summary>
		[HorizontalGroup("H")]
		[Button("$_DisplayName", ButtonSizes.Medium)]
		protected void StatusList()
		{
			Debug.Log($"点击了Status: {Name}");
		}

		/// <summary>
		/// 编辑按钮
		/// </summary>
		[HideIf(nameof(_IsDirty))]
		[HorizontalGroup("H", 50)]
		[Button("Edit", ButtonSizes.Medium)]
		protected void Edit()
		{
			if (!string.IsNullOrEmpty(_StatusSwitcher.CurrDirtyStatus))
			{
				string msg = $"先保存{_StatusSwitcher.CurrDirtyStatus}后，再尝试编辑其他状态";
				CommonUtility.SafeDisplayDialog("", msg, "OK", LogLevel.Warning);
				return;
			}

			Debug.Log($"Edit: {Name}");
			_StatusSwitcher.CurrDirtyStatus = Name;
			_IsDirty = true;
		}

		/// <summary>
		/// 保存按钮
		/// </summary>
		[ShowIf(nameof(_IsDirty))]
		[HorizontalGroup("H", 50)]
		[Button("Save", ButtonSizes.Medium), GUIColor(0, 1, 0)]
		protected void Save()
		{
			Debug.Log($"Save: {Name}");
			_StatusSwitcher.CurrDirtyStatus = null;
			_IsDirty = false;
		}
	}
}
