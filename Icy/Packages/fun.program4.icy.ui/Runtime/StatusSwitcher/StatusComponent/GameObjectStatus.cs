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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.UI
{
	[HideLabel]
	[System.Serializable]
	public class GameObjectStatus : StatusSwitcherStatusBase
	{
		[InlineProperty]
		[SerializeField]
		[OnValueChanged(nameof(Apply))]
		public bool ActiveSelf;

		public override void Apply()
		{
			Target.gameObject.SetActive(ActiveSelf);
		}
	}
}
