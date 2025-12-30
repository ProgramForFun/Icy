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

using Cysharp.Text;
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
			if (_IDDotMap.TryGetValue(id, out RedDot redDot))
			{
				redDot.Refresh();
				return true;
			}
			return false;
		}

		/// <summary>
		/// 是否存在指定ID的红点，无关是否显示
		/// </summary>
		public bool Exist(string id)
		{
			return _IDDotMap.ContainsKey(id);
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
				if (_ChildParentMap.TryGetValue(redDot.ID, out HashSet<string> parents))
				{
					if (!parents.Contains(parentID))
						parents.Add(parentID);
				}
				else
					_ChildParentMap.Add(redDot.ID, new HashSet<string>() { parentID });

				if (_ParentChildMap.TryGetValue(parentID, out HashSet<string> children))
				{
					if (!children.Contains(redDot.ID))
						children.Add(redDot.ID);
				}
				else
					_ParentChildMap.Add(parentID, new HashSet<string>() { redDot.ID });
			}

			//如果已经存在以redDot为parent的子红点了，计算redDot的CountChildren
			if (_ParentChildMap.TryGetValue(redDot.ID, out HashSet<string> allChildren))
			{
				int countChildren = 0;
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

			if (_ChildParentMap.TryGetValue(redDot.ID, out HashSet<string> parents))
			{
				foreach (string parent in parents)
				{
					if (_ParentChildMap.TryGetValue(parent, out HashSet<string> hashSet))
						hashSet.Remove(redDot.ID);
				}
				_ChildParentMap.Remove(redDot.ID);
			}
		}

		/// <summary>
		/// 刷新一个红点，及其parent
		/// </summary>
		internal void Refresh(string id, int prevCount, int newCount)
		{
			if (_ChildParentMap.TryGetValue(id, out HashSet<string> parents))
			{
				foreach (string parentID in parents)
				{
					if (_IDDotMap.TryGetValue(parentID, out RedDot parentRedDot))
					{
						int count = parentRedDot._CountChildren - prevCount + newCount;
						//Log.Info($"{parentID} be set -{prevCount} + {newCount}");
						parentRedDot.SetCountChildren(count);
						parentRedDot.Refresh();
					}
				}
			}
		}

		/// <summary>
		/// 输出当前存在的红点父子关系及显隐情况，方便调试；
		/// 有大量文本操作，在性能敏感的场景慎用
		/// </summary>
		public string Dump()
		{
			Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();
			builder.Append("Total red dot count = ");
			builder.Append(_IDDotMap.Count);
			builder.AppendLine();

			foreach (KeyValuePair<string, RedDot> item in _IDDotMap)
			{
				if (_ChildParentMap.TryGetValue(item.Key, out HashSet<string> parents))
				{
					builder.Append(item.Key);
					builder.Append(" --parent--> ");
					foreach (string parent in parents)
					{
						builder.Append(" ");
						builder.Append(parent);
					}
					builder.Append(", count = ");
					builder.Append(_IDDotMap[item.Key].Count);
					builder.AppendLine();
				}
				else
				{
					builder.Append(item.Key);
					builder.Append(" --<TOP>-- ");
					builder.Append(", count = ");
					builder.Append(item.Value.Count);
					builder.AppendLine();
				}
			}

			return builder.ToString();
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
