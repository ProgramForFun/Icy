/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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
	/// <summary>
	/// StatusSwitcher的一个状态
	/// </summary>
	[Serializable]
	public class StatusSwitcherItem
	{
		/// <summary>
		/// 状态名字
		/// </summary>
		[HideInInspector]
		[SerializeField]
		public string Name;

		/// <summary>
		/// 受此状态控制的Target列表
		/// </summary>
		[HideInInspector]
		[SerializeField]
		public List<StatusSwitcherTarget> Targets;

		/// <summary>
		/// 所属StatusSwitcher
		/// </summary>
		[HideInInspector]
		[SerializeField]
		internal StatusSwitcher StatusSwitcher;

		/// <summary>
		/// 状态是否在编辑中
		/// </summary>
		[NonSerialized]
		protected bool _IsDirty = false;

		protected string _DisplayName => _IsDirty? Name + "（Editing...）" : Name;

		public StatusSwitcherItem(StatusSwitcher statusSwitcher, string name)
		{
			StatusSwitcher = statusSwitcher;
			Name = name;
		}

		/// <summary>
		/// 状态按钮本体
		/// </summary>
		[HorizontalGroup("H")]
		[Button("$_DisplayName", ButtonSizes.Medium)]
		internal void StatusList()
		{
			bool succeed = false;
			if (Targets != null)
			{
				for (int i = 0; i < Targets.Count; i++)
				{
					bool result = Targets[i].SwitchTo(this);
					if (!succeed)
						succeed = true;
				}
			}

#if UNITY_EDITOR
			if (!succeed)
				Log.Error("未执行任何状态切换，Status = " + Name);
#endif
		}

		/// <summary>
		/// 编辑按钮
		/// </summary>
		[HideIf(nameof(_IsDirty))]
		[HorizontalGroup("H", 50)]
		[Button("Edit", ButtonSizes.Medium)]
		protected void Edit()
		{
			if (StatusSwitcher.CurrDirtyStatus != null)
			{
				string msg = $"先保存{StatusSwitcher.CurrDirtyStatus}后，再尝试编辑其他状态";
#if UNITY_EDITOR
				CommonUtility.SafeDisplayDialog("", msg, "OK", LogLevel.Error);
#endif
				return;
			}

			StatusSwitcher.CurrDirtyStatus = this;
			if (Targets != null)
				StatusSwitcher.SwitcherTargetList = Targets;
			else
				StatusSwitcher.SwitcherTargetList = new List<StatusSwitcherTarget>();
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
			Targets = StatusSwitcher.SwitcherTargetList;
			StatusSwitcher.SwitcherTargetList = null;
			StatusSwitcher.CurrDirtyStatus = null;
			_IsDirty = false;
		}
	}
}
