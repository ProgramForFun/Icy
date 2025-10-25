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
using System;
using System.Reflection;

namespace Icy.Protobuf
{
	/// <summary>
	/// 初始化Protobuf
	/// </summary>
	public class InitProto
	{
		/// <summary>
		/// 反射调用注册所有proto id，牺牲一点点性能，换取用户不需要关心这个调用了
		/// TODO：接入HybridCLR后，这里的调用时机要改
		/// </summary>
		public async UniTaskVoid InitProtoMsgIDRegistry()
		{
			byte[] settingBytes = await SettingsHelper.LoadSetting(SettingsHelper.ProtoSetting);
			if (settingBytes != null)
			{
				ProtoSetting protoSetting = ProtoSetting.Parser.ParseFrom(settingBytes);
				if (protoSetting != null && !string.IsNullOrEmpty(protoSetting.ProtoAssemblyName))
				{
					Assembly assembly = Assembly.Load(protoSetting.ProtoAssemblyName);
					if (assembly != null)
					{
						Type type = assembly.GetType("ProtoMsgIDRegistry");
						if (type != null)
						{
							MethodInfo method = type.GetMethod("RegisterAll");
							method?.Invoke(null, null);
						}
					}
					else
						Log.Error($"Load proto assembly {protoSetting.ProtoAssemblyName} failed");
				}
				else
					Log.Info("ProtoMsgIDRegistry not inited, if you do not use Protobuf, just ignore it.");
			}
		}
	}
}
