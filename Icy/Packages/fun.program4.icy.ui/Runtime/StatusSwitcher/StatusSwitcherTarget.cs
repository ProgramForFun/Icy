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
using System.Collections.Generic;
using UnityEngine;

namespace Icy.UI
{
	public class StatusSwitcherTarget : MonoBehaviour
	{
		/// <summary>
		/// 受这个StatusSwitcher的控制
		/// </summary>
		[Title("所属StatusSwitcher")]
		[OnInspectorInit(nameof(OnInspectorInit))]
		[ValueDropdown(nameof(_AllSwithers), IsUniqueList = true, DropdownWidth = 300)]
		public UnityEngine.Object StatusSwitcher;

		/// <summary>
		/// 此UI下面所有的StatusSwitcher
		/// </summary>
		[HideInInspector]
		protected List<UnityEngine.Object> _AllSwithers;

		protected void OnInspectorInit()
		{
			_AllSwithers = new List<UnityEngine.Object>();
			Transform uiBaseTrans = CommonUtility.GetAncestor(transform, FindUIBase);
			if (uiBaseTrans != null)
			{
				StatusSwitcher[] switchers = uiBaseTrans.GetComponentsInChildren<StatusSwitcher>();
				for (int i = 0; i < switchers.Length; i++)
					_AllSwithers.Add(switchers[i]);
			}
		}

		[PropertySpace(10)]
		[Button("跳转到 StatusSwitcher", Icon = SdfIconType.ReplyFill, ButtonHeight = (int)ButtonSizes.Medium)]
		protected void Goto()
		{

		}

		protected bool FindUIBase(Transform trans)
		{
			if (trans.GetComponent<UIBase>() != null)
				return true;
			return false;
		}
	}

	[System.Flags]
	public enum SwitcherComponent
	{
		None = 0,
		GameObject = 1 << 0,
		Transform = 1 << 1,
		RectTransform = 1 << 2,
		ImageEx = 1 << 3,
		TextEx = 1 << 4,
		ButtonEx = 1 << 5,
		Everything = ~0,
	}

	[System.Serializable]
	public class AllStatusSwitcherComponent
	{
		public GameObjectStatus gameObjectRecord;
		public TransformStatus transRecord;
		//public ImageRecord imageRecord;
		//public RectTransformRecord rectRecord;
		//public TextRecord textRecord;
		//public ButtonRecord buttonRecord;
		//public AnimationRecord animationRecord;
	}
}
