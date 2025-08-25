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
using System.Collections.Generic;

namespace Icy.UI
{
	/// <summary>
	/// 红点管理器
	/// </summary>
	public sealed class RedDotManager : Singleton<RedDotManager>
	{
		/// <summary>
		/// 红点ID → 红点
		/// </summary>
		private Dictionary<string, RedDot> _IDDotMap;
		/// <summary>
		/// 子红点 → 父红点
		/// </summary>
		private Dictionary<string, HashSet<string>> _ChildParentMap;
		/// <summary>
		/// 父红点 → 子红点
		/// </summary>
		private Dictionary<string, HashSet<string>> _ParentChildMap;


		protected override void OnInitialized()
		{
			base.OnInitialized();

			_IDDotMap = new Dictionary<string, RedDot>();
			_ChildParentMap = new Dictionary<string, HashSet<string>>();
			_ParentChildMap = new Dictionary<string, HashSet<string>>();
		}

		/// <summary>
		/// 尝试刷新指定ID的红点，如果当前这个红点不存在，则什么都不做
		/// </summary>
		public bool TryToRefresh(string id)
		{
			if (_IDDotMap.ContainsKey(id))
			{
				_IDDotMap[id]?.Refresh();
				return true;
			}
			return false;
		}

		/// <summary>
		/// 添加一个红点
		/// </summary>
		internal void Add(RedDot redDot, string parentID = null)
		{
			if (!_IDDotMap.ContainsKey(redDot.ID))
				_IDDotMap.Add(redDot.ID, redDot);

			if (parentID != null)
			{
				if (_ChildParentMap.ContainsKey(redDot.ID))
				{
					HashSet<string> parents = _ChildParentMap[redDot.ID];
					if (!parents.Contains(parentID))
						parents.Add(parentID);
				}
				else
					_ChildParentMap.Add(redDot.ID, new HashSet<string>() { parentID });

				if (_ParentChildMap.ContainsKey(parentID))
				{
					HashSet<string> children = _ParentChildMap[parentID];
					if (!children.Contains(redDot.ID))
						children.Add(redDot.ID);
				}
				else
					_ParentChildMap.Add(parentID, new HashSet<string>() { redDot.ID });
			}

			//如果已经存在以redDot为parent的子红点了，计算redDot的CountChildren
			if (_ParentChildMap.ContainsKey(redDot.ID))
			{
				int countChildren = 0;
				HashSet<string> allChildren = _ParentChildMap[redDot.ID];
				foreach (string item in allChildren)
					countChildren += _IDDotMap[item].Count;
				redDot.SetCountChildren(countChildren);
			}
		}

		/// <summary>
		/// 添加一个红点，并指定多个 parent
		/// </summary>
		internal void Add(RedDot redDot, params string[] parentIDs)
		{
			for (int i = 0; i < parentIDs.Length; i++)
				Add(redDot, parentIDs[i]);
		}

		/// <summary>
		/// 是否存在指定ID的红点
		/// </summary>
		internal bool ExistRedDot(string id)
		{
			return _IDDotMap.ContainsKey(id);
		}

		/// <summary>
		/// 移除一个红点
		/// </summary>
		internal void Remove(RedDot redDot)
		{
			if (_IDDotMap.ContainsKey(redDot.ID))
			{
				//在移除前，把这个红点数量当做0，刷新其parent
				Refresh(redDot.ID, redDot.Count, 0);

				_IDDotMap.Remove(redDot.ID);
			}

			if (_ChildParentMap.ContainsKey(redDot.ID))
			{
				HashSet<string> parents = _ChildParentMap[redDot.ID];
				foreach (string parent in parents)
				{
					if (_ParentChildMap.ContainsKey(parent))
						_ParentChildMap[parent].Remove(redDot.ID);
				}
				_ChildParentMap.Remove(redDot.ID);
			}
		}

		/// <summary>
		/// 刷新一个红点，及其parent
		/// </summary>
		internal void Refresh(string id, int prevCount, int newCount)
		{
			if (_ChildParentMap.ContainsKey(id))
			{
				HashSet<string> parents = _ChildParentMap[id];
				foreach (string parentID in parents)
				{
					if (_IDDotMap.ContainsKey(parentID))
					{
						RedDot parentRedDot = _IDDotMap[parentID];
						int count = parentRedDot._CountChildren - prevCount + newCount;
						Log.LogInfo($"{parentID} be set -{prevCount} + {newCount}");
						parentRedDot.SetCountChildren(count);
						parentRedDot.Refresh();
					}
				}
			}
		}

		/// <summary>
		/// 输出当前存在的红点父子关系及显隐情况
		/// </summary>
		public string Dump()
		{
			return string.Empty;
		}
	}

	/// <summary>
	/// 红点类型
	/// </summary>
	public enum RedDotType
	{
		/// <summary>
		/// 只有一个红点
		/// </summary>
		DotOnly,
		/// <summary>
		/// 红点 + 数量文本
		/// </summary>
		DotWithCount,
	}
}
