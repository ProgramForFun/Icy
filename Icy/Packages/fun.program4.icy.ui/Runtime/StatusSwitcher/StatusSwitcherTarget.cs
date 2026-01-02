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
	/// 受StatusSwitcher控制的目标，会序列化保存各种状态，由StatusSwitcher驱动切换
	/// </summary>
	[HideMonoScript]
	public class StatusSwitcherTarget : MonoBehaviour
	{
		/// <summary>
		/// StatusItem下拉列表的数据源，为拼接的StatusSwitcher + Status名字
		/// </summary>
		protected List<string> _StatusItems;

		/// <summary>
		/// 用下拉列表选择StatusItem
		/// </summary>
		[ValueDropdown(nameof(_StatusItems), IsUniqueList = true, DropdownWidth = 200)]
		[OnValueChanged(nameof(OnStatusItemDropdownChanged))]
		[ShowInInspector]
		protected string StatusItemName = NONE;

		[PropertySpace(10, 10)]
		[Title("此节点要记录哪些类型的状态")]
		[ShowIf(nameof(NeedShowRecordTypes))]
		[OnInspectorInit(nameof(OnInspectorInit))]
		[OnInspectorDispose(nameof(OnInspectorDispose))]
		public StatusSwitcherRecordType RecordTypes;

		[ShowIf(nameof(NeedShowGameObject))]
		[BoxGroup("GameObject")]
		public GameObjectStatus GameObjectStatus;

		[ShowIf(nameof(NeedShowTransform))]
		[BoxGroup("Transform")]
		public TransformStatus TransformStatus;

		/// <summary>
		/// 受这些StatusSwitcher的控制
		/// </summary>
		[Title("所属StatusSwitcher列表（双击跳转）")]
		[ListDrawerSettings(ShowItemCount = true, ShowFoldout = false, IsReadOnly = true)]
		[PropertySpace(10, 20)]
		[ShowInInspector]
		protected List<StatusSwitcher> StatusSwitchers;

		[SerializeField]
		[ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false, HideAddButton = true)]
		protected List<StatusSwitcherRecord> _Records;

		/// <summary>
		/// StatusItem下拉列表的空选项
		/// </summary>
		protected const string NONE = "None";

		protected void OnInspectorInit()
		{
			//RecordTypes = StatusSwitcherRecordType.None;
			//StatusItemName = null;


			StatusSwitchers = new List<StatusSwitcher>();
			Transform uiBaseTrans = CommonUtility.GetAncestor(transform, FindUIBase);
			if (uiBaseTrans != null)
			{
				StatusSwitcher[] switchers = uiBaseTrans.GetComponentsInChildren<StatusSwitcher>();
				for (int sw = 0; sw < switchers.Length; sw++)
				{
					List<StatusItem> list = switchers[sw].StatusList;
					for (int s = 0; s < list.Count; s++)
					{
						List<StatusSwitcherTarget> targets = list[s].Targets;
						for (int t = 0; t < targets.Count; t++)
						{
							if (targets[t] == this)
							{
								if (!StatusSwitchers.Contains(switchers[sw]))
									StatusSwitchers.Add(switchers[sw]);
								TryToAdd(list[s]);
							}
						}
					}
				}
			}


			_StatusItems = new List<string>();
			_StatusItems.Add(NONE); //以在下拉列表里显示一个可以置空的选项
			if (_Records != null)
			{
				for (int i = 0; i < _Records.Count; i++)
				{
					StatusItem item = _Records[i].StatusItem;
					string key = $"{item.StatusSwitcher.gameObject.name} - {item.Name}";
					_StatusItems.Add(key);
				}
			}
		}

		protected void OnInspectorDispose()
		{

		}

		protected void Clear()
		{
			GameObjectStatus?.Dispose();
			TransformStatus?.Dispose();
			GameObjectStatus = null;
			TransformStatus = null;
		}

		protected void OnStatusItemDropdownChanged()
		{
			if (StatusItemName == NONE)
				Clear();
			else
			{
				int idx = _StatusItems.IndexOf(StatusItemName);
				idx -= 1;//因为_StatusItems第一个元素是None，所以这里要减1
				StatusSwitcherRecord record = _Records[idx];
				RecordTypes = record.RecordTypes;

				GameObjectStatus = record.AllStatusSwitcherComponent.gameObject;
				GameObjectStatus.Init(this);
				GameObjectStatus.Apply();

				TransformStatus = record.AllStatusSwitcherComponent.transform;
				TransformStatus.Init(this);
				TransformStatus.Apply();
			}
		}

		[PropertySpace(10)]
		[ShowIf(nameof(NeedShowRecordTypes))]
		[Button("Save", Icon = SdfIconType.SdCardFill, ButtonHeight = (int)ButtonSizes.Medium), GUIColor(0, 1, 0)]
		protected void Save()
		{
#if UNITY_EDITOR
			int idx = _StatusItems.IndexOf(StatusItemName);
			idx -= 1;//因为_StatusItems第一个元素是None，所以这里要减1
			StatusSwitcherRecord record = _Records[idx];
			record.RecordTypes = RecordTypes;
			record.AllStatusSwitcherComponent.gameObject = GameObjectStatus;
			record.AllStatusSwitcherComponent.transform = TransformStatus;
#endif
		}

		protected void TryToAdd(StatusItem statusItem)
		{
			if (_Records == null)
				_Records = new List<StatusSwitcherRecord>();

			bool has = false;
			for (int i = 0; i < _Records.Count; i++)
			{
				if (_Records[i].StatusItem.StatusSwitcher.gameObject.name == statusItem.StatusSwitcher.gameObject.name
					&& _Records[i].StatusItem.Name == statusItem.Name)
				{
					has = true;
					break;
				}
			}

			if (!has)
			{
				StatusSwitcherRecord newRecord = new StatusSwitcherRecord();
				newRecord.StatusItem = statusItem;
				newRecord.RecordTypes = StatusSwitcherRecordType.None;
				newRecord.AllStatusSwitcherComponent = new AllStatusSwitcherComponent();
				_Records.Add(newRecord);
			}
		}

		protected bool FindUIBase(Transform trans)
		{
			if (trans.GetComponent<UIBase>() != null)
				return true;
			return false;
		}

		protected bool NeedShowRecordTypes()
		{
			return !string.IsNullOrEmpty(StatusItemName) && StatusItemName != NONE;
		}

		protected bool NeedShowGameObject()
		{
			return RecordTypes.HasFlag(StatusSwitcherRecordType.GameObject) && NeedShowRecordTypes();
		}

		protected bool NeedShowTransform()
		{
			return RecordTypes.HasFlag(StatusSwitcherRecordType.Transform) && NeedShowRecordTypes();
		}

		protected bool NeedShowRectTransform()
		{
			return RecordTypes.HasFlag(StatusSwitcherRecordType.RectTransform) && NeedShowRecordTypes();
		}
	}

	[System.Flags]
	[System.Serializable]
	public enum StatusSwitcherRecordType
	{
		None = 0,
		GameObject = 1 << 0,
		Transform = 1 << 1,
		RectTransform = 1 << 2,
		//ImageEx = 1 << 3,
		//TextEx = 1 << 4,
		//ButtonEx = 1 << 5,
		All = ~0,
	}

	[System.Serializable]
	public class StatusSwitcherRecord
	{
		/// <summary>
		/// 所属StatusItem
		/// </summary>
		public StatusItem StatusItem;
		/// <summary>
		/// 都存储了哪些类型的组件数据
		/// </summary>
		public StatusSwitcherRecordType RecordTypes;
		/// <summary>
		/// 所有组件数据的集合
		/// </summary>
		public AllStatusSwitcherComponent AllStatusSwitcherComponent;
	}

	[System.Serializable]
	public class AllStatusSwitcherComponent
	{
		public GameObjectStatus gameObject;
		public TransformStatus transform;
		//public ImageRecord imageRecord;
		//public RectTransformRecord rectRecord;
		//public TextRecord textRecord;
		//public ButtonRecord buttonRecord;
		//public AnimationRecord animationRecord;
	}
}
