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


using Sirenix.OdinInspector;
using UnityEngine;

namespace Icy.UI
{
	/// <summary>
	/// GameObject状态，只记录是否Active
	/// </summary>
	[HideLabel]
	[System.Serializable]
	public class GameObjectStatus : StatusSwitcherStatusBase
	{
		[InlineProperty]
		[SerializeField]
		[OnValueChanged(nameof(Apply))]
		[InlineButton(nameof(Record))]
		public bool ActiveSelf;

		public override void Record()
		{
			ActiveSelf = Target.gameObject.activeSelf;
		}

		public override void Apply()
		{
			Target.gameObject.SetActive(ActiveSelf);
		}

		public override void CopyFrom(StatusSwitcherStatusBase other)
		{
			GameObjectStatus otherStatus = other as GameObjectStatus;
			ActiveSelf = otherStatus.ActiveSelf;
		}
	}
}
