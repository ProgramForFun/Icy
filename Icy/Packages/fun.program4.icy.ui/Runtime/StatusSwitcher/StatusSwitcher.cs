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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.UI
{
	public class StatusSwitcher : MonoBehaviour
	{
		protected List<StatusSwitcherTarget> _SwitcherTargetList = new List<StatusSwitcherTarget>();

		[SerializeField]
		protected List<StatusName> _Statuses = new List<StatusName>();


		/// <summary>
		/// 切换到指定名字的状态
		/// </summary>
		public void SwitchTo(string statusName)
		{
			for (int i = 0; i < _Statuses.Count; i++)
			{
				if (_Statuses[i].Name == statusName)
				{

					return;
				}
			}
			Log.Error($"Status switching failed, {gameObject.name} has NO status called {statusName}", nameof(StatusSwitcher));
		}

#if UNITY_EDITOR
		internal void AddNewStatus(string statusName)
		{

		}

		internal void DeleteStatus(string statusName)
		{

		}

		internal void EditStatus(string statusName)
		{

		}

		internal void Save()
		{

		}
#endif
	}

	[System.Serializable]
	public class StatusName
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public bool IsDirty = true;
	}
}
