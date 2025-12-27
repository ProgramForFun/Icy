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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.UI
{
	public class StatusSwitcherTarget : MonoBehaviour
	{

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
