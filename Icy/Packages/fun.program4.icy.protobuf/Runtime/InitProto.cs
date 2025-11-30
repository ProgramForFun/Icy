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


using Cysharp.Threading.Tasks;
using Icy.Base;
using Icy.Asset;
using System;
using System.Reflection;

namespace Icy.Protobuf
{
	/// <summary>
	/// 初始化Protobuf
	/// </summary>
	public static class InitProto
	{
		/// <summary>
		/// 反射调用注册所有proto id，牺牲一点点性能，换取用户不需要关心这个调用了
		/// </summary>
		public static async UniTaskVoid InitProtoMsgIDRegistry()
		{
			byte[] settingBytes = await SettingsHelper.LoadSetting(SettingsHelper.ProtoSetting);
			if (settingBytes != null)
			{
				ProtoSetting protoSetting = ProtoSetting.Parser.ParseFrom(settingBytes);
				if (protoSetting == null || string.IsNullOrEmpty(protoSetting.ProtoAssemblyName))
				{
					Log.Info("ProtoMsgIDRegistry not inited, if you do not use Protobuf, just ignore it.", nameof(InitProto), true);
					return;
				}

				bool isProtoAssemblyAOT = await IsProtoAssemblyAOT(protoSetting);
				Log.Info("ProtoMsgIDRegistry in " + (isProtoAssemblyAOT ? "AOT" : "Patch"), nameof(InitProto), true);

				DoInit(protoSetting);
			}
			else
				Log.Error($"Load ProtoSetting failed", nameof(InitProto));
		}

		/// <summary>
		/// 反射调用ProtoMsgIDRegistry
		/// </summary>
		private static void DoInit(ProtoSetting protoSetting)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.GetName().Name == protoSetting.ProtoAssemblyName)
				{
					Type type = assembly.GetType("ProtoMsgIDRegistry");
					if (type != null)
					{
						MethodInfo method = type.GetMethod("RegisterAll");
						method?.Invoke(null, null);
						Log.Info("ProtoMsgIDRegistry inited", nameof(InitProto), true);
					}
					else
						Log.Error($"Load ProtoMsgIDRegistry type failed", nameof(InitProto));
					return;
				}
			}
			Log.Error($"Load proto assembly {protoSetting.ProtoAssemblyName} failed", nameof(InitProto));
		}

		/// <summary>
		/// Proto程序集是不是AOT程序集
		/// </summary>
		private static async UniTask<bool> IsProtoAssemblyAOT(ProtoSetting protoSetting)
		{
			string assemblyName = protoSetting.ProtoAssemblyName;
			byte[] assetSettingBytes = await SettingsHelper.LoadSetting(SettingsHelper.AssetSetting);
			if (assetSettingBytes != null)
			{
				AssetSetting assetSetting = AssetSetting.Parser.ParseFrom(assetSettingBytes);
				bool isPatchAssembly = assetSetting.PatchDLLs.Contains(assemblyName + ".dll");
				return !isPatchAssembly;
			}

			return true;
		}
	}
}
