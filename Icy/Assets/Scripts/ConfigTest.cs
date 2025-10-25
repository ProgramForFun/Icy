#if UNITY_EDITOR
using Icy.Base;
using Luban;
using System.IO;

public static class ConfigTest
{
	public static void Test()
	{
		cfg.Tables allCfg = new cfg.Tables(LoadByteBuf);
		cfg.RewardCfg rewardCfg = allCfg.Reward.Get(1001);
		Log.Info(rewardCfg.Name);
	}

	private static ByteBuf LoadByteBuf(string file)
	{
		//正式的改成通过YooAsset从Bundle中读
		return new ByteBuf(File.ReadAllBytes($"Assets/Example/Configs/bin/{file}.bytes"));
	}
}
#endif
