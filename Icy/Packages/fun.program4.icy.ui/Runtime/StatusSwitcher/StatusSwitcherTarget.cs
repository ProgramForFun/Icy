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
#if UNITY_EDITOR
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
#endif

		[PropertySpace(10, 10)]
		[Title("此节点要记录哪些类型的状态")]
#if UNITY_EDITOR
		[ShowIf(nameof(NeedShowRecordTypes))]
		[OnInspectorInit(nameof(OnInspectorInit))]
		[OnInspectorDispose(nameof(OnInspectorDispose))]
#endif
		public StatusSwitcherRecordType RecordTypes;

		/// <summary>
		/// GameObject类型的状态
		/// </summary>
#if UNITY_EDITOR
		[ShowIf(nameof(NeedShowGameObject))]
#endif
		[BoxGroup("GameObject")]
		public GameObjectStatus GameObjectStatus;

		/// <summary>
		/// Transform类型的状态
		/// </summary>
#if UNITY_EDITOR
		[ShowIf(nameof(NeedShowTransform))]
#endif
		[BoxGroup("Transform")]
		public TransformStatus TransformStatus;

		//New Status stub

#if UNITY_EDITOR
		/// <summary>
		/// 受这些StatusSwitcher的控制
		/// </summary>
		[Title("所属StatusSwitcher列表（双击跳转）")]
		[ListDrawerSettings(ShowItemCount = true, ShowFoldout = false, IsReadOnly = true)]
		[PropertySpace(10, 20)]
		[ShowInInspector]
		protected List<StatusSwitcher> StatusSwitchers;
#endif

		/// <summary>
		/// 节点的记录数据
		/// </summary>
		[SerializeField]
		[ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false, HideAddButton = true)]
		internal List<StatusSwitcherRecord> Records;


		/// <summary>
		/// 切换到指定StatusItem
		/// </summary>
		internal bool SwitchTo(StatusSwitcherItem statusItem)
		{
			for (int i = 0; i < Records.Count; i++)
			{
				StatusSwitcherItem item = Records[i].StatusItem;
				if (item == statusItem)
				{
					AllStatusSwitcherComponent all = Records[i].AllStatusSwitcherComponent;
					all.gameObject.Init(this);
					all.gameObject.Apply();

					all.transform.Init(this);
					all.transform.Apply();

					//New Status stub
					return true;
				}
			}
			return false;
		}

#if UNITY_EDITOR
		/// <summary>
		/// StatusItem下拉列表的空选项
		/// </summary>
		protected const string NONE = "None";

		protected void OnInspectorInit()
		{
			StatusSwitchers = new List<StatusSwitcher>();
			Transform uiBaseTrans = CommonUtility.GetAncestor(transform, FindUIBase);
			if (uiBaseTrans != null)
			{
				StatusSwitcher[] switchers = uiBaseTrans.GetComponentsInChildren<StatusSwitcher>();
				for (int sw = 0; sw < switchers.Length; sw++)
				{
					List<StatusSwitcherItem> list = switchers[sw].StatusList;
					for (int s = 0; s < list.Count; s++)
					{
						List<StatusSwitcherTarget> targets = list[s].Targets;
						for (int t = 0; t < targets.Count; t++)
						{
							if (targets[t] == this)
							{
								//所属的StatusSwitcher
								if (!StatusSwitchers.Contains(switchers[sw]))
									StatusSwitchers.Add(switchers[sw]);

								//尝试添加一个Record
								TryToAddRecord(list[s]);
							}
						}
					}
				}
			}

			//初始化StatusItem下拉列表
			_StatusItems = new List<string>();
			_StatusItems.Add(NONE); //以在下拉列表里显示一个可以置空的选项
			if (Records != null)
			{
				for (int i = 0; i < Records.Count; i++)
				{
					StatusSwitcherItem item = Records[i].StatusItem;
					string key = $"{item.StatusSwitcher.gameObject.name} - {item.Name}";
					_StatusItems.Add(key);
				}
			}
		}

		protected void OnStatusItemDropdownChanged()
		{
			if (StatusItemName == NONE)
				Clear();
			else
			{
				int idx = _StatusItems.IndexOf(StatusItemName);
				idx -= 1;//因为_StatusItems第一个元素是None，所以这里要减1
				StatusSwitcherRecord record = Records[idx];
				RecordTypes = record.RecordTypes;

				GameObjectStatus = new GameObjectStatus();
				InitStatus(GameObjectStatus, record.AllStatusSwitcherComponent.gameObject, StatusSwitcherRecordType.GameObject);
				TransformStatus = new TransformStatus();
				InitStatus(TransformStatus, record.AllStatusSwitcherComponent.transform, StatusSwitcherRecordType.Transform);

				//New Status stub
			}
		}

		/// <summary>
		/// 初始化一个Status
		/// </summary>
		protected void InitStatus(StatusSwitcherStatusBase status, StatusSwitcherStatusBase serializedStatus
			, StatusSwitcherRecordType recordType)
		{
			if (RecordTypes.HasFlag(recordType))
			{
				status.CopyFrom(serializedStatus);
				status.Init(this);
				status.Apply();
			}
			else
			{
				status.Init(this);
				status.Record();
			}
		}

		/// <summary>
		/// 将当前编辑的状态数据序列化存储起来
		/// </summary>
		[PropertySpace(10)]
		[ShowIf(nameof(NeedShowRecordTypes))]
		[Button("Save", Icon = SdfIconType.SdCardFill, ButtonHeight = (int)ButtonSizes.Medium), GUIColor(0, 1, 0)]
		protected void Save()
		{
			int idx = _StatusItems.IndexOf(StatusItemName);
			idx -= 1;//因为_StatusItems第一个元素是None，所以这里要减1
			StatusSwitcherRecord record = Records[idx];
			record.RecordTypes = RecordTypes;
			if (RecordTypes.HasFlag(StatusSwitcherRecordType.GameObject))
				record.AllStatusSwitcherComponent.gameObject = GameObjectStatus;
			if (RecordTypes.HasFlag(StatusSwitcherRecordType.Transform))
				record.AllStatusSwitcherComponent.transform = TransformStatus;

			//New Status stub
		}

		/// <summary>
		/// 尝试添加一个Record
		/// </summary>
		protected void TryToAddRecord(StatusSwitcherItem statusItem)
		{
			if (Records == null)
				Records = new List<StatusSwitcherRecord>();

			bool has = false;
			for (int i = 0; i < Records.Count; i++)
			{
				if (Records[i].StatusItem == statusItem)
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
				Records.Add(newRecord);
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

		//New Status stub

		protected void OnInspectorDispose()
		{

		}
#endif

		protected void Clear()
		{
			GameObjectStatus?.Dispose();
			TransformStatus?.Dispose();
			GameObjectStatus = null;
			TransformStatus = null;
		}
	}

	/// <summary>
	/// 所有状态的Flag
	/// </summary>
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

		//New Status stub
		All = ~0,
	}

	/// <summary>
	/// 一条Record，对应一个StatusItem
	/// </summary>
	[System.Serializable]
	public class StatusSwitcherRecord
	{
		/// <summary>
		/// 所属StatusItem
		/// </summary>
		public StatusSwitcherItem StatusItem;
		/// <summary>
		/// 都存储了哪些类型的组件数据
		/// </summary>
		public StatusSwitcherRecordType RecordTypes;
		/// <summary>
		/// 所有组件数据的集合
		/// </summary>
		public AllStatusSwitcherComponent AllStatusSwitcherComponent;
	}

	/// <summary>
	/// 所有组件数据的集合
	/// </summary>
	[System.Serializable]
	public class AllStatusSwitcherComponent
	{
		public GameObjectStatus gameObject;
		public TransformStatus transform;
		//New Status stub
	}
}
