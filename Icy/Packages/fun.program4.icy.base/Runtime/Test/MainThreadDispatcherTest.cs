#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Icy.Base
{
	public static class MainThreadDispatcherTest
	{
		public static void Test()
		{
			Debug.Log($"Main Thread ID = {IcyFrame.Instance.MainThreadID}");

			//使用MainThreadDispatcher
			UniTask.RunOnThreadPool(() =>
			{
				Debug.Log($"Worker Thread ID = {Thread.CurrentThread.ManagedThreadId}");
				MainThreadDispatcher.Instance.Enqueue(TestAction);
			}).Forget();

			//使用UniTask
			TestWithUniTask().Forget();
		}

		private static void TestAction()
		{
			decimal a = 1;
			decimal b = 2;
			decimal c = 3;
			decimal d = 4;
			for (int i = 0; i < 1000000; i++)
				d = a * b / c + a;
			Debug.Log($"TestAction run on thread {Thread.CurrentThread.ManagedThreadId}");
		}

		private static async UniTaskVoid TestWithUniTask()
		{
			Debug.Log($"Current Thread ID = {Thread.CurrentThread.ManagedThreadId}");
			await UniTask.SwitchToThreadPool();
			Debug.Log($"Current Thread ID = {Thread.CurrentThread.ManagedThreadId}");
			await UniTask.SwitchToMainThread();
			Debug.Log($"Current Thread ID = {Thread.CurrentThread.ManagedThreadId}");
		}
	}
}
#endif
