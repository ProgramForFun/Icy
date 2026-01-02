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
		[PropertySpace(10, 10)]
		[Title("此节点要记录哪些类型的状态")]
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


		protected void OnInspectorInit()
		{
			GameObjectStatus = new GameObjectStatus();
			GameObjectStatus.Init(this);
			TransformStatus = new TransformStatus();
			TransformStatus.Init(this);

			//遍历所属UIBase下所有的StatusSwitcher，找到自己都受哪些StatusSwitcher控制
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
								StatusSwitchers.Add(switchers[sw]);
						}
					}
				}
			}
		}

		protected void OnInspectorDispose()
		{
			GameObjectStatus?.Dispose();
			TransformStatus?.Dispose();
		}

//		[PropertySpace(10)]
//		[Button("跳转到 StatusSwitcher", Icon = SdfIconType.ReplyFill, ButtonHeight = (int)ButtonSizes.Medium)]
//		protected void Goto()
//		{
//#if UNITY_EDITOR
//			if (StatusSwitcher != null)
//				UnityEditor.Selection.activeGameObject = StatusSwitcher.gameObject;
//#endif
//		}

		protected bool FindUIBase(Transform trans)
		{
			if (trans.GetComponent<UIBase>() != null)
				return true;
			return false;
		}

		protected bool NeedShowGameObject()
		{
			return RecordTypes.HasFlag(StatusSwitcherRecordType.GameObject);
		}

		protected bool NeedShowTransform()
		{
			return RecordTypes.HasFlag(StatusSwitcherRecordType.Transform);
		}

		protected bool NeedShowRectTransform()
		{
			return RecordTypes.HasFlag(StatusSwitcherRecordType.RectTransform);
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
