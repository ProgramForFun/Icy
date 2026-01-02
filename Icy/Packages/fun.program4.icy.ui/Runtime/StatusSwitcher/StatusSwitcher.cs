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
	/// UX可以通过此组件配置多个节点的状态，代码侧就可以一键切换多个UI节点的状态；
	/// 避免程序员需要通过代码控制一堆节点状态，耗时且容易出错，
	/// 未来小范围的UI表现变动可能UX自己就能处理，解放程序员
	/// </summary>
	[HideMonoScript]
	public class StatusSwitcher : MonoBehaviour
	{
		/// <summary>
		/// 所有的Status
		/// </summary>
		[BoxGroup("状态列表")]
		[ListDrawerSettings(ShowItemCount = true, DraggableItems = true, ShowFoldout = false, HideAddButton = true)]
		[SerializeField]
		internal List<StatusItem> StatusList;

		/// <summary>
		/// 新Status的名字
		/// </summary>
		[BoxGroup("状态列表")]
		[HorizontalGroup("状态列表/Add")]
		[ShowInInspector]
		[HideLabel]
		[NonSerialized]
		protected string _InputName;

		/// <summary>
		/// 点击添加新Status的按钮
		/// </summary>
		[PropertySpace(0, 20)]
		[BoxGroup("状态列表")]
		[EnableIf(nameof(IsValidName))]
		[HorizontalGroup("状态列表/Add")]
		[Button("Add Status", ButtonSizes.Medium, Icon = SdfIconType.PlusCircleFill)]
		protected void AddNewStatus()
		{
			if (StatusList == null)
				StatusList = new List<StatusItem>();

			for (int i = 0; i < StatusList.Count; i++)
			{
				if (StatusList[i].Name == _InputName)
				{
					string msg = $"重复的Status名字：{_InputName}";

#if UNITY_EDITOR
					CommonUtility.SafeDisplayDialog("", msg, "OK", LogLevel.Error);
#endif
					return;
				}
			}

			StatusList.Add(new StatusItem(this, _InputName));
			_InputName = null;
		}

		//控制的所有节点
		[ShowIf(nameof(NeedShowTargetList))]
		[FoldoutGroup("控制的节点")]
		[ListDrawerSettings(ShowItemCount = true, DraggableItems = true, ShowFoldout = false, HideAddButton = true)]
		[SerializeField]
		internal List<StatusSwitcherTarget> SwitcherTargetList;

		[ShowIf(nameof(NeedShowTargetList))]
		[FoldoutGroup("控制的节点")]
		[Button("Add Target", ButtonSizes.Medium, Icon = SdfIconType.PlusCircleFill)]
		protected void AddNewTarget()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.update += OnEditorUpdate;
			UnityEditor.EditorGUIUtility.ShowObjectPicker<StatusSwitcherTarget>(null, true, "", 666);
#endif
		}

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void Init()
		{
			UnityEditor.SceneManagement.PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
			UnityEditor.SceneManagement.PrefabStage.prefabStageClosing += OnPrefabStageClosing;
		}

		private static void OnPrefabStageClosing(UnityEditor.SceneManagement.PrefabStage stage)
		{
			if (stage.prefabContentsRoot != null)
			{
				StatusSwitcher[] allStatuses = stage.prefabContentsRoot.GetComponentsInChildren<StatusSwitcher>();
				for (int i = 0; i < allStatuses.Length; i++)
				{
					if (allStatuses[i].CurrDirtyStatus != null)
					{
						string msg = $"有Status未保存：{allStatuses[i].CurrDirtyStatus.Name}";
						CommonUtility.SafeDisplayDialog("", msg, "OK", LogLevel.Error);
					}
				}
			}
		}

		/// <summary>
		/// 选择要添加的StatusSwitcherTarget
		/// </summary>
		private void OnEditorUpdate()
		{
			int pickerControlID = UnityEditor.EditorGUIUtility.GetObjectPickerControlID();

			if (pickerControlID == 666)
			{
				UnityEngine.Object obj = UnityEditor.EditorGUIUtility.GetObjectPickerObject();
				GameObject go = obj == null ? null : obj as GameObject;
				if (go == null)
					_CurrSelectTarget = null;
				else
					_CurrSelectTarget = go.GetComponent<StatusSwitcherTarget>();
			}

			if (pickerControlID == 0)
			{
				if (_CurrSelectTarget == null)
					SwitcherTargetList.Add(null);
				else
				{
					SwitcherTargetList.Add(_CurrSelectTarget);
				}

				UnityEditor.EditorApplication.update -= OnEditorUpdate;
			}
		}
#endif

		private bool NeedShowTargetList()
		{
			return CurrDirtyStatus != null;
		}

		/// <summary>
		/// 当前在Dirty状态的Status
		/// </summary>
		[NonSerialized]
		internal StatusItem CurrDirtyStatus;
		/// <summary>
		/// 当前ObjectPicker窗口里选中的Target
		/// </summary>
		[NonSerialized]
		protected StatusSwitcherTarget _CurrSelectTarget;


		/// <summary>
		/// 切换到指定名字的状态
		/// </summary>
		public bool SwitchTo(string statusName)
		{
			for (int i = 0; i < StatusList.Count; i++)
			{
				if (StatusList[i].Name == statusName)
				{

					return true;
				}
			}
			Log.Error($"Status switching failed, {gameObject.name} has NO status called {statusName}", nameof(StatusSwitcher));
			return false;
		}

		/// <summary>
		/// 是否有指定名字的状态
		/// </summary>
		public bool HasStatus(string statusName)
		{
			for (int i = 0; i < StatusList.Count; i++)
			{
				if (StatusList[i].Name == statusName)
					return true;
			}
			return false;
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

		public override string ToString()
		{
			return gameObject.name;
		}
	}

	[Serializable]
	public class StatusItem
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
		protected StatusSwitcher _StatusSwitcher;

		/// <summary>
		/// 状态是否在编辑中
		/// </summary>
		[NonSerialized]
		protected bool _IsDirty = false;

		protected string _DisplayName => _IsDirty? Name + "（Editing...）" : Name;

		public StatusItem(StatusSwitcher statusSwitcher, string name)
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

		}

		/// <summary>
		/// 编辑按钮
		/// </summary>
		[HideIf(nameof(_IsDirty))]
		[HorizontalGroup("H", 50)]
		[Button("Edit", ButtonSizes.Medium)]
		protected void Edit()
		{
			if (_StatusSwitcher.CurrDirtyStatus != null)
			{
				string msg = $"先保存{_StatusSwitcher.CurrDirtyStatus}后，再尝试编辑其他状态";
#if UNITY_EDITOR
				CommonUtility.SafeDisplayDialog("", msg, "OK", LogLevel.Error);
#endif
				return;
			}

			_StatusSwitcher.CurrDirtyStatus = this;
			if (Targets != null)
				_StatusSwitcher.SwitcherTargetList = Targets;
			else
				_StatusSwitcher.SwitcherTargetList = new List<StatusSwitcherTarget>();
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
			Targets = _StatusSwitcher.SwitcherTargetList;
			_StatusSwitcher.SwitcherTargetList = null;
			_StatusSwitcher.CurrDirtyStatus = null;
			_IsDirty = false;
		}
	}
}
