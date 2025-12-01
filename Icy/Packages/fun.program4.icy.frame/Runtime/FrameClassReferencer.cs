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


using UnityEngine.Scripting;

namespace Icy.Frame
{
	/// <summary>
	/// 显示引用框架的类，每个程序集里引用一个（已在link.xml里整个保留所有Icy程序集），避免被裁剪掉；
	/// 以保证HybridCLR热更时使用了一个之前没用过的类时、都可以找得到
	/// </summary>
	[Preserve]
	public class FrameClassReferencer
	{
		/// <summary>
		/// 在link.xml里保留整个程序集后，还需要在代码里显示引用一下其中的任意东西；
		/// 因为即使在link.xml保留了程序集，如果代码中没有任何引用，仍可能被裁剪掉；
		/// </summary>
		[Preserve]
		public static void Preserve()
		{
			typeof(Icy.Base.Timer).ToString();
			typeof(Icy.Asset.AssetManager).ToString();
			typeof(Icy.UI.UIBase).ToString();
			typeof(Icy.Frame.IcyFrame).ToString();
			typeof(Icy.Network.TcpSession).ToString();
			typeof(Icy.Protobuf.InitProto).ToString();
			typeof(Icy.GM.GM).ToString();
		}
	}
}
