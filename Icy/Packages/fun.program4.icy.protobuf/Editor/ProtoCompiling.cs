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
		private const string GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES = "_Icy_GeneratingProtoAssemblyReloadTimes";

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
			EditorUtility.DisplayProgressBar("Compile Proto", "Compiling proto...", 0.5f);
			EditorLocalPrefs.SetInt(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES, 2);

			try
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

					EditorLocalPrefs.SetBool(GENERATING_PROTO_KEY, true);
					EditorLocalPrefs.Save();

					AssetDatabase.Refresh();
				}
			}
			catch (Exception)
			{
				EditorUtility.ClearProgressBar();
			}
		}

		private static void OnAllAssemblyReload()
		{
			try
			{
				int times = EditorLocalPrefs.GetInt(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES, int.MaxValue);
				EditorLocalPrefs.SetInt(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES, --times);
				if (times <= 0)
				{
					EditorUtility.ClearProgressBar();
					EditorLocalPrefs.RemoveKey(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES);
					EditorLocalPrefs.Save();
				}


				bool generatingProto = EditorLocalPrefs.GetBool(GENERATING_PROTO_KEY, false);
				if (generatingProto)
				{
					EditorLocalPrefs.RemoveKey(GENERATING_PROTO_KEY);
					EditorLocalPrefs.Save();

					EditorUtility.DisplayProgressBar("Compile Proto", "Generate Reset Extension...", 0.8f);

					List<Type> allProtoTypes = GetAllProtoTypes();
					Dictionary<Type, List<FieldInfo>> allProtoFields = GetAllProtoFields(allProtoTypes);
					GenerateResetMethodExtension(allProtoFields);
				}
			}
			catch (Exception)
			{
				EditorUtility.ClearProgressBar();
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

			StringBuilder iMessageBuilder = new StringBuilder(10240);
			StringBuilder resetPerProtoBuilder = new StringBuilder(10240);
			string curNamespace = string.Empty;
			Type iMsgType = typeof(pb::IMessage);
			Type repeatedType = typeof(pb.Collections.RepeatedField<>);
			for (int i = 0; i < allProtoTypes.Count; i++)
			{
				Type protoType = allProtoTypes[i];

				//IMessage的Reset
				string varName = FirstLetterToLower(protoType.Name);
				iMessageBuilder.AppendLine(@$"			case {protoType.FullName} {varName}:");
				iMessageBuilder.AppendLine(@$"				{varName}.Reset();");
				iMessageBuilder.AppendLine(@$"				break;");


				//每个Proto msg的Reset
				if (protoType.Namespace != curNamespace)
				{
					//上一个namespace的结束括号
					if (!string.IsNullOrEmpty(curNamespace))
						resetPerProtoBuilder.AppendLine(@"}");

					resetPerProtoBuilder.AppendLine(@$"namespace {protoType.Namespace}");
					resetPerProtoBuilder.AppendLine(@"{");
					curNamespace = protoType.Namespace;
				}

				resetPerProtoBuilder.AppendLine(@$"	public sealed partial class {protoType.Name} : pb::IMessage<{protoType.Name}>");
				resetPerProtoBuilder.AppendLine(@"	{");
				resetPerProtoBuilder.AppendLine(@"		public void Reset()");
				resetPerProtoBuilder.AppendLine(@"		{");
				List<FieldInfo> fields = protoDict[protoType];

				for (int f = 0; f < fields.Count; f++)
				{
					Type fieldType = fields[f].FieldType;
					if (iMsgType.IsAssignableFrom(fieldType))
						resetPerProtoBuilder.AppendLine(@$"			{fields[f].Name}?.Reset();");
					else if (CommonUtility.IsSubclassOfRawGeneric(fieldType, repeatedType))
						resetPerProtoBuilder.AppendLine(@$"			{fields[f].Name}?.Clear();");
					else
						resetPerProtoBuilder.AppendLine(@$"			{fields[f].Name} = default;");
				}
				resetPerProtoBuilder.AppendLine(@"		}");
				resetPerProtoBuilder.AppendLine(@"	}");
			}

			resetPerProtoBuilder.AppendLine(@"}");


			string codeTemplate = ProtoResetTemplate.Code;
			string final = string.Format(codeTemplate, iMessageBuilder.ToString(), resetPerProtoBuilder.ToString());

			string targetFile = Path.Combine(targetDir, "ResetMethodExtension.cs");
			if (File.Exists(targetFile))
				File.Delete(targetFile);
			File.WriteAllText(targetFile, final);

			//延迟一帧，否则无法触发代码重新编译
			EditorApplication.delayCall += () =>
			{
				AssetDatabase.Refresh();
			};
		}

		private static int SortProtoTypes(Type a, Type b)
		{
			return a.FullName.CompareTo(b.FullName);
		}

		private static string FirstLetterToLower(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;

			return str.Length == 1
				? char.ToLower(str[0]).ToString()
				: $"{char.ToLower(str[0])}{str.Substring(1)}";
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
