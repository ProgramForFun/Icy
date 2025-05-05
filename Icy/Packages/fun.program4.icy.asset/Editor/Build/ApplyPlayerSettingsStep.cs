using Icy.Base;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 把框架Setting文件PlayerSetting部分，应用到PlayerSetting
	/// </summary>
	public class ApplyPlayerSettingsStep : ProcedureStep
	{
		private BuildTarget _BuildTarget;
		private BuildSetting _BuildSetting;

		public override async UniTask Activate()
		{
			_BuildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget");
			_BuildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting") as BuildSetting;

			if (_BuildSetting != null)
			{
				if (!string.IsNullOrEmpty(_BuildSetting.ApplicationIdentifier))
					PlayerSettings.applicationIdentifier = _BuildSetting.ApplicationIdentifier;

				if (!string.IsNullOrEmpty(_BuildSetting.ProductName))
					PlayerSettings.productName = _BuildSetting.ProductName;

				if (!string.IsNullOrEmpty(_BuildSetting.CompanyName))
					PlayerSettings.companyName = _BuildSetting.CompanyName;

				if (!string.IsNullOrEmpty(_BuildSetting.Version))
					PlayerSettings.bundleVersion = _BuildSetting.Version;

				switch (_BuildTarget)
				{
					case BuildTarget.Android:
						//PlayerSettings.Android.keyaliasPass = "";
						//PlayerSettings.Android.keystorePass = "";
						break;
					case BuildTarget.iOS:
						break;
					case BuildTarget.StandaloneWindows64:
						break;
					default:
						break;
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			await UniTask.CompletedTask;
			Finish();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
