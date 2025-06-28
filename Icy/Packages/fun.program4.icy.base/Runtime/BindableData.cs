using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
		/// <summary>
		/// 绑定上来的其他BindableData<T>
		/// </summary>
		private List<BindableData<T>> _Others = new List<BindableData<T>>();

		public BindableData()
		{
			_Data = default(T);
		}

		public BindableData(T data)
		{
			_Data = data;
		}

		/// <summary>
		/// 持有的数据；
		/// </summary>
		public T Data
		{
			get => _Data;
			set
			{
				_Data = value;

				for (int i = 0; i < _Listeners.Count; i++)
					_Listeners[i]?.Invoke(_Data);
				for (int i = 0; i < _Others.Count; i++)
					_Others[i].Data = value;
			}
		}

		/// <summary>
		/// 把一个事件绑定到BindableData，BindableData修改时调用这个事件
		/// </summary>
		public bool Bind(Action<T> listener)
		{
			if (!_Listeners.Contains(listener))
			{
				_Listeners.Add(listener);
				return true;
			}

			Log.LogError($"Duplicate binding, BindableData T = {typeof(T).Name}, listener = {listener.Target.GetType().Name}.{listener.Method.Name}", "BindableData");
			return false;
		}

		/// <summary>
		/// 把我 Bind 到other，other变化时会通知我
		/// </summary>
		public bool BindTo(BindableData<T> other)
		{
			//不能Bind自己
			if (other == this)
			{
				Log.LogError($"Invalid binding to this, BindableData T = {typeof(T).Name}", "BindableData");
				return false;
			}

			//避免死循环
			if (_Others.Contains(other))
			{
				Log.LogError($"Binding lead to endless loop, BindableData T = {typeof(T).Name}", "BindableData");
				return false;
			}

			if (other._Others.Contains(this))
			{
				Log.LogError($"Duplicate binding, BindableData T = {typeof(T).Name}", "BindableData");
				return false;
			}

			other._Others.Add(this);
			return true;
		}

		/// <summary>
		/// 解除Bind一个Listener
		/// </summary>
		public void Unbind(Action<T> listener)
		{
			_Listeners.Remove(listener);
		}

		/// <summary>
		/// 解除Bind一个其他BindableData<T>
		/// </summary>
		public void UnbindTo(BindableData<T> other)
		{
			if (other == null)
			{
				Log.LogError("UnbindTo a null", "BindableData");
				return;
			}
			other._Others.Remove(this);
		}

		/// <summary>
		/// 序列化输出当前所有Bind上来的监听，方便调试；
		/// 内部实现有反射，注意在性能敏感的场景使用
		/// </summary>
		public string Dump()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"BindableData<{typeof(T).Name}> HashCode = {GetHashCode()}");
			stringBuilder.AppendLine($"Listeners count = {_Listeners.Count}");
			foreach (Action<T> l in _Listeners)
			{
				MethodInfo methodInfo = l.Method;
				stringBuilder.AppendLine($"{l.Target.GetType().Name}.{l.Method.Name}");
			}

			stringBuilder.AppendLine($"Other Bindables count = {_Others.Count}");
			foreach (BindableData<T> other in _Others)
				stringBuilder.AppendLine($"Bindable<{typeof(T).Name}> = {other.Data}");

			return stringBuilder.ToString();
		}

		#region Override
		public static implicit operator T(BindableData<T> data)
		{
			return data._Data;
		}

		//不能提供T到BindableData的隐式转换，因为这样就允许了直接通过 = 给BindableData赋值
		//由于C#禁止重载operator=，导致此时无法copy _Listeners，进而导致无法触发监听
		//https://github.com/dotnet/csharplang/discussions/2773，这个也行不通
		//public static implicit operator BindableData<T>(T data)
		//{
		//	return new BindableData<T>(data);
		//}

		public override string ToString()
		{
			return _Data.ToString();
		}
		#endregion
	}
}
