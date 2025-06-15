using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using pb = global::Google.Protobuf;


namespace Icy.Protobuf.Editor
{
	/// <summary>
	/// 编译Protobuf
	/// </summary>
	[InitializeOnLoad]
	public static class ProtoCompiling
	{
		private const string GENERATING_PROTO_KEY = "_Icy_GeneratingProto";

		static ProtoCompiling()
		{
			AssemblyReloadEvents.afterAssemblyReload -= OnAllAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAllAssemblyReload;
		}

		/// <summary>
		/// 编译Protobuf
		/// </summary>
		[MenuItem("Icy/Compile Proto", false, 900)]
		static void CompileProto()
		{
			//TODO : 从设置中读取
			string batFilePath = @"C:\Work\Z\Proto\_Compile.bat";
			string workingDirectory = @"";

			ProcessStartInfo processInfo = new ProcessStartInfo()
			{
				FileName = batFilePath,      // 批处理文件名
				WorkingDirectory = workingDirectory,  // 工作目录
				CreateNoWindow = true,       // 不创建新窗口（后台运行）
				UseShellExecute = false,     // 不使用系统Shell（用于重定向输出）

				// 重定向输入/输出
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};

			using (Process process = new Process())
			{
				process.StartInfo = processInfo;

				//注册输出/错误事件处理程序
				process.OutputDataReceived += OnCompileProtoLog;
				process.ErrorDataReceived += OnCompileProtoError;

				process.Start();

				// 如果重定向输出，需要开始异步读取
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				// 等待批处理执行完成
				process.WaitForExit();

				int exitCode = process.ExitCode;
				UnityEngine.Debug.Log("Compile proto exit code = " + exitCode);

				LocalPrefs.SetBool(GENERATING_PROTO_KEY, true);
				LocalPrefs.Save();

				AssetDatabase.Refresh();
			}
		}

		private static void OnAllAssemblyReload()
		{
			bool generatingProto = LocalPrefs.GetBool(GENERATING_PROTO_KEY, false);
			if (generatingProto)
			{
				LocalPrefs.RemoveKey(GENERATING_PROTO_KEY);
				LocalPrefs.Save();

				List<Type> allProtoTypes = GetAllProtoTypes();
				Dictionary<Type, List<FieldInfo>> allProtoFields = GetAllProtoFields(allProtoTypes);
				GenerateResetMethodExtension(allProtoFields);
			}
		}

		/// <summary>
		/// 获取所有的Proto类
		/// </summary>
		private static List<Type> GetAllProtoTypes()
		{
			Type targetInterface = typeof(pb::IMessage<>);

			// 扫描所有已加载程序集
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly =>
				{
					try { return assembly.GetTypes(); }
					catch { return Enumerable.Empty<Type>(); } // 忽略无法加载的程序集
				})
				.Where(type =>
					type.IsClass &&
					!type.IsAbstract &&
					type.GetInterfaces()
						.Any(i => i.IsGenericType &&
								  i.GetGenericTypeDefinition() == targetInterface)
						&& !type.Namespace.StartsWith("Google") //过滤掉Protobuf自带的
						&& !(type.Namespace.StartsWith("Icy") && type.Name.EndsWith("Setting"))//过滤掉框架里的Setting
				)
				.ToList();
		}

		/// <summary>
		/// 获取所有Proto类的所有字段
		/// </summary>
		private static Dictionary<Type, List<FieldInfo>> GetAllProtoFields(List<Type> allProtoTypes)
		{
			Dictionary<Type, List<FieldInfo>> rtn = new Dictionary<Type, List<FieldInfo>>();

			for (int i = 0; i < allProtoTypes.Count; i++)
			{
				// 获取所有实例字段（含私有/受保护字段）
				FieldInfo[] fields = allProtoTypes[i].GetFields(
					BindingFlags.Instance |
					BindingFlags.Public |
					BindingFlags.NonPublic
				);

				rtn.Add(allProtoTypes[i], fields
					.Where(field => field.Name != "_unknownFields") //过滤掉Protobuf自带的每个都有的 _unknownFields
					.ToList());
			}

			return rtn;
		}

		/// <summary>
		/// 为每个Proto类，生成Reset方法
		/// </summary>
		private static void GenerateResetMethodExtension(Dictionary<Type, List<FieldInfo>> protoDict)
		{
			List<Type> allProtoTypes = protoDict.Keys.ToList();
			//按名字排序，同命名空间的自然挨在一起了
			allProtoTypes.Sort(SortProtoTypes);

			//TODO : 从设置中读取
			string targetDir = @"C:\Work\Z\Icy\Assets\Example\Protos";

			StringBuilder stringBuilder = new StringBuilder(10240);
			stringBuilder.AppendLine(@"using pb = global::Google.Protobuf;");

			string curNamespace = string.Empty;
			Type iMsgType = typeof(pb::IMessage);
			Type repeatedType = typeof(pb.Collections.RepeatedField<>);
			for (int i = 0; i < allProtoTypes.Count; i++)
			{
				Type protoType = allProtoTypes[i];

				if (protoType.Namespace != curNamespace)
				{
					//上一个namespace的结束括号
					if (!string.IsNullOrEmpty(curNamespace))
						stringBuilder.AppendLine(@"}");

					stringBuilder.AppendLine(@$"namespace {protoType.Namespace}");
					stringBuilder.AppendLine(@"{");
					curNamespace = protoType.Namespace;
				}

				stringBuilder.AppendLine(@$"	public sealed partial class {protoType.Name} : pb::IMessage<{protoType.Name}>");
				stringBuilder.AppendLine(@"	{");
				stringBuilder.AppendLine(@"		public void Reset()");
				stringBuilder.AppendLine(@"		{");
				List<FieldInfo> fields = protoDict[protoType];

				for (int f = 0; f < fields.Count; f++)
				{
					Type fieldType = fields[f].FieldType;
					if (iMsgType.IsAssignableFrom(fieldType))
						stringBuilder.AppendLine(@$"			{fields[f].Name}?.Reset();");
					else if (IsSubclassOfRawGeneric(fieldType, repeatedType))
						stringBuilder.AppendLine(@$"			{fields[f].Name}?.Clear();");
					else
						stringBuilder.AppendLine(@$"			{fields[f].Name} = default;");
				}
				stringBuilder.AppendLine(@"		}");
				stringBuilder.AppendLine(@"	}");
			}

			stringBuilder.AppendLine(@"}");


			string targetFile = Path.Combine(targetDir, "ResetMethodExtension.cs");
			if (File.Exists(targetFile))
				File.Delete(targetFile);
			File.WriteAllText(targetFile, stringBuilder.ToString());

			AssetDatabase.Refresh();
		}

		private static int SortProtoTypes(Type a, Type b)
		{
			return a.FullName.CompareTo(b.FullName);
		}

		/// <summary>
		/// 检查当前类型是否派生自指定的泛型类型（忽略泛型参数）
		/// </summary>
		/// <param name="typeToCheck">要检查的类型</param>
		/// <param name="genericBaseType">泛型基类型（如 typeof(MyGenericClass<>)）</param>
		/// <returns>如果派生自指定的泛型类型则返回 true，否则返回 false</returns>
		private static bool IsSubclassOfRawGeneric(Type typeToCheck, Type genericBaseType)
		{
			if (!genericBaseType.IsGenericType)
				throw new ArgumentException("目标类型必须是泛型类型定义", nameof(genericBaseType));
			if (genericBaseType.IsGenericTypeDefinition == false)
				throw new ArgumentException("必须使用未绑定的泛型类型定义（如 MyGenericClass<>）", nameof(genericBaseType));

			// 1. 检查当前类型及其所有基类（不包括接口）
			while (typeToCheck != null && typeToCheck != typeof(object))
			{
				// 如果是泛型类型，先获取泛型定义
				var current = typeToCheck.IsGenericType
					? typeToCheck.GetGenericTypeDefinition()
					: typeToCheck;

				// 2. 直接匹配当前类型
				if (current == genericBaseType)
					return true;

				// 3. 移动到直接基类继续检查
				typeToCheck = typeToCheck.BaseType;
			}

			// 4. 检查所有实现的接口（如果需要包括接口）
			if (genericBaseType.IsInterface)
			{
				foreach (var interfaceType in typeToCheck.GetInterfaces())
				{
					var currentInterface = interfaceType.IsGenericType
						? interfaceType.GetGenericTypeDefinition()
						: interfaceType;

					if (currentInterface == genericBaseType)
						return true;
				}
			}

			return false;
		}

		private static void OnCompileProtoLog(object sender, DataReceivedEventArgs e)
		{
			if (e != null && e.Data != null)
				UnityEngine.Debug.Log(e.Data);
		}

		private static void OnCompileProtoError(object sender, DataReceivedEventArgs e)
		{
			if (e != null && e.Data != null)
				UnityEngine.Debug.LogError(e.Data);
		}
	}
}
