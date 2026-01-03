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
	/// Transform状态，记录local位置旋转缩放
	/// </summary>
	[HideLabel]
	[System.Serializable]
	public class TransformStatus : StatusSwitcherStatusBase
	{
		[InlineProperty]
		[SerializeField]
		[OnValueChanged(nameof(Apply))]
		[InlineButton(nameof(RecordPos), "Record")]
		public Vector3 LocalPosition;

		[InlineProperty]
		[SerializeField]
		[OnValueChanged(nameof(Apply))]
		[InlineButton(nameof(RecordRotation), "Record")]
		public Quaternion LocalRotation;

		[InlineProperty]
		[SerializeField]
		[OnValueChanged(nameof(Apply))]
		[InlineButton(nameof(RecordScale), "Record")]
		public Vector3 LocalScale;

		private void RecordPos()
		{
			LocalPosition = Target.transform.localPosition;
		}

		private void RecordRotation()
		{
			LocalRotation = Target.transform.localRotation;
		}

		private void RecordScale()
		{
			LocalScale = Target.transform.localScale;
		}

		public override void Record()
		{
			RecordPos();
			RecordRotation();
			RecordScale();
		}

		public override void Apply()
		{
			Target.transform.localPosition = LocalPosition;
			Target.transform.localRotation = LocalRotation;
			Target.transform.localScale = LocalScale;
		}

		public override void CopyFrom(StatusSwitcherStatusBase other)
		{
			TransformStatus otherStatus = other as TransformStatus;
			LocalPosition = otherStatus.LocalPosition;
			LocalRotation = otherStatus.LocalRotation;
			LocalScale = otherStatus.LocalScale;
		}
	}
}
