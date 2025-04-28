using System;
using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 可绑定的数据，数据变化时会调用绑定上来的事件；
	/// 也重载了必要的operator，在大部分场景可以像直接使用持有的T一样使用，比如和T做比较等等
	/// </summary>
	public class BindableData<T>
	{
		/// <summary>
		/// 持有的数据
		/// </summary>
		private T _Data;
		/// <summary>
		/// 绑定上来的事件
		/// </summary>
		private List<Action<T>> _Listeners = new List<Action<T>>();

		public BindableData()
		{
			_Data = default(T);
		}

		public BindableData(T data)
		{
			_Data = data;
		}

		/// <summary>
		/// 修改数据
		/// </summary>
		public void SetData(T data)
		{
			_Data = data;
			for (int i = 0; i < _Listeners.Count; i++)
				_Listeners[i]?.Invoke(_Data);
		}

		/// <summary>
		/// 把一个事件绑定到BindableData，BindableData修改时调用这个事件
		/// </summary>
		public void Bind(Action<T> listener)
		{
			if (!_Listeners.Contains(listener))
				_Listeners.Add(listener);
		}

		/// <summary>
		/// 解除Bind
		/// </summary>
		public void Unbind(Action<T> listener)
		{
			_Listeners.Remove(listener);
		}

		#region Override
		public static implicit operator T(BindableData<T> a)
		{
			return a._Data;
		}

		//不能提供T到BindableData的隐式转换，因为这样就允许了直接通过 = 给BindableData赋值
		//由于C#禁止重载operator=，导致此时无法copy _Listeners，进而导致无法触发监听
		//https://github.com/dotnet/csharplang/discussions/2773，这个也行不通
		//public static implicit operator BindableData<T>(T data)
		//{
		//	return new BindableData<T>(data);
		//}

		public static bool operator ==(BindableData<T> left, BindableData<T> right)
		{
			return left._Data.Equals(right._Data);
		}

		public static bool operator !=(BindableData<T> left, BindableData<T> right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return obj is BindableData<T> other && _Data.Equals(other._Data);
		}
		public override int GetHashCode()
		{
			return _Data.GetHashCode();
		}

		public override string ToString()
		{
			return _Data.ToString();
		}
		#endregion
	}
}
