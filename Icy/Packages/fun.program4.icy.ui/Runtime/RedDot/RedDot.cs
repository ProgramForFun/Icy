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
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// 红点
	/// </summary>
	public class RedDot : MonoBehaviour, IDisposable
	{
		/// <summary>
		/// 红点类型
		/// </summary>
		public RedDotType Type;
		/// <summary>
		/// 红点物体
		/// </summary>
		public GameObject Dot;
		/// <summary>
		/// 数量Text
		/// TODO ：替换为自定义的UIText
		/// </summary>
		[ShowIf("Type", RedDotType.DotWithCount)]
		public Text CountText;

		/// <summary>
		/// 红点ID
		/// </summary>
		public string ID { get; protected set; }
		/// <summary>
		/// 受本RedDot影响的父RedDot ID
		/// </summary>
		public string ParentID { get; protected set; }
		/// <summary>
		/// 此红点的当前数量
		/// </summary>
		public int Count => _CountThis + _CountChildren;
		/// <summary>
		/// 自己的Count
		/// </summary>
		protected int _CountThis;
		/// <summary>
		/// 来自Children的Count
		/// </summary>
		internal int _CountChildren;
		/// <summary>
		/// 红点计算方法
		/// </summary>
		protected Func<int> _Calculator;
		/// <summary>
		/// 上一次Refresh时记录下来的Count
		/// </summary>
		protected int _PrevCount;
		/// <summary>
		/// 是否已经Dispose了
		/// </summary>
		protected bool _Disposed;


		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="id">string类型的ID / 名字</param>
		/// <param name="calculator">红点数量的计算方法，不显示返回0，返回>0会显示红点，如果有是DotWithCount类型，还会显示数量</param>
		/// <param name="parentID"></param>
		public void Init(string id, Func<int> calculator = null, string parentID = null)
		{
			if (RedDotManager.Instance.ExistRedDot(id))
			{
				Log.LogError($"Trying to init a RedDot more than once, id = {id}", nameof(RedDot));
				return;
			}

			ID = id;
			ParentID = parentID;
			_Calculator = calculator;
			_CountThis = 0;
			_CountChildren = 0;
			_PrevCount = 0;

			RedDotManager.Instance.Add(this, parentID);
			_Disposed = false;
		}

		/// <summary>
		/// 刷新红点
		/// </summary>
		public void Refresh()
		{
			if (_Disposed || gameObject == null)
			{
				string id = gameObject == null ? "unknown" : ID;
				Log.LogError($"Try to refresh a disposed RedDot, id = {id}", nameof(RedDot));
				return;
			}

			try
			{
				if (_Calculator != null)
					_CountThis = _Calculator();
			}
			catch (Exception e)
			{
				_CountThis = 0;
				Log.LogError($"Calculate RedDot count failed , id = {ID}, exception = {e}", nameof(RedDot));
			}
			finally
			{
				int prevCount = _PrevCount;
				RefreshImpl();
				RedDotManager.Instance.Refresh(ID, prevCount, Count);
			}
		}

		internal void RefreshImpl()
		{
			Dot.SetActive(Count > 0);

			if (Type == RedDotType.DotWithCount && Count > 0)
			{
				CountText.gameObject.SetActive(true);
				CountText.text = Count.ToString();
			}
			else
				CountText.gameObject.SetActive(false);

			_PrevCount = Count;
		}

		internal void ClearCountChildren()
		{
			_CountChildren = 0;
		}

		/// <summary>
		/// 设置来自Children的Count
		/// </summary>
		internal void SetCountChildren(int countFromChildren)
		{
			_CountChildren = countFromChildren;
		}

		/// <summary>
		/// 主动释放掉红点，不参与红点计算了，但并不会销毁GameObject；
		/// 可以通过再次Init重新启用红点；
		/// </summary>
		public void Dispose()
		{
			if (_Disposed)
			{
				string id = gameObject == null ? "unknown" : ID;
				Log.LogError($"Trying to dispose a disposed RedDot, id = {id}", nameof(RedDot));
				return;
			}

			gameObject.SetActive(false);
			RedDotManager.Instance.Remove(this);
			_Disposed = true;
		}

		protected void OnDestroy()
		{
			RedDotManager.Instance.Remove(this);
		}
	}
}
