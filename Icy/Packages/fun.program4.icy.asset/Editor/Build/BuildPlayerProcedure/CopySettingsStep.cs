using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;
using System.IO;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 把框架Setting文件copy到streamingAssetsPath
	/// </summary>
	public class CopySettingsStep : ProcedureStep
	{
		public override async UniTask Activate()
		{
			string dest = Path.Combine(Application.streamingAssetsPath, SettingsHelper.GetSettingDir());
			bool succeed = CommonUtility.CopyDir(SettingsHelper.GetSettingDir(), dest, false);
			if (!succeed)
			{
				Log.Assert(false, "Copy setting files failed", "CopySettingsStep");
				OwnerProcedure.Abort();
				return;
			}

			await UniTask.CompletedTask;
			Finish();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
