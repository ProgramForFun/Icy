using Icy.Base;
using SRDebugger.Services;
using SRF.Service;

namespace Icy.GM
{
	/// <summary>
	/// GM 和 运行时Console
	/// </summary>
	public static class GM
	{
		/// <summary>
		/// 初始化GM
		/// </summary>
		public static void Init(IGMOptions gmOptions)
		{
			if (IsInited())
			{
				Log.LogError("Duplicate Init to GM is invalid", nameof(GM));
				return;
			}

			SRDebug.Init();
			//注册SRDebugger的Options
			IOptionsService srService = SRServiceManager.GetService<IOptionsService>();
			srService.AddContainer(gmOptions);
		}

		/// <summary>
		/// 是否已经初始化了
		/// </summary>
		public static bool IsInited()
		{
			return SRServiceManager.HasService<IConsoleService>();
		}
	}
}
