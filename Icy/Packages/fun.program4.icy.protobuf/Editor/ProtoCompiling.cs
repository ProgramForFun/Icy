using Icy.Base;
using Icy.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using pb = global::Google.Protobuf;


namespace Icy.Protobuf.Editor
{
	/// <summary>
	/// 编译Protobuf
	/// </summary>
	public static class ProtoCompiling
	{
		private const string GENERATING_PROTO_KEY = "_Icy_GeneratingProto";
		private const string GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES = "_Icy_GeneratingProtoAssemblyReloadTimes";

		private const string PROTO_MSG_ID_REGISTRY_TEMPLATE_PATH = "Packages/fun.program4.icy.protobuf/Editor/ProtoMsgIDRegistryTemplate.txt";
		private const string PROTO_RESET_TEMPLATE_PATH = "Packages/fun.program4.icy.protobuf/Editor/ProtoResetTemplate.txt";

		private static bool _IsProgressBarDisplaying;
		private static Process _Process;

		[InitializeOnLoadMethod]
		static void Init()
		{
			_IsProgressBarDisplaying = false;

			AssemblyReloadEvents.afterAssemblyReload -= OnAllAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAllAssemblyReload;
			CompilationPipeline.compilationFinished -= OnCompilationFinished;
			CompilationPipeline.compilationFinished += OnCompilationFinished;
		}

		/// <summary>
		/// 编译Protobuf
		/// </summary>
		[MenuItem("Icy/Compile Proto", false, 900)]
		[MenuItem("Icy/Proto/Compile Proto", false)]
		static void CompileProto()
		{
			BiProgress.Show("Compile Proto", "Compiling proto...", 0.5f);
			_IsProgressBarDisplaying = true;
			EditorLocalPrefs.SetInt(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES, 2);

			try
			{
				string batFilePath = null;
				ProtoSetting setting = GetSetting();
				if (setting != null)
					batFilePath = setting.CompileBatPath;
				if (string.IsNullOrEmpty(batFilePath))
				{
					EditorUtility.DisplayDialog("", $"编译未执行，请先去Icy/Proto/Setting菜单中，设置 编译Proto的Bat脚本路径", "OK");
					return;
				}

				ClearConsole.Clear();

				string outputDirFullPath = Path.GetFullPath(setting.ProtoOutputDir);
				ProcessStartInfo processInfo = new ProcessStartInfo()
				{
					FileName = batFilePath,					// 批处理文件名
					WorkingDirectory = "",					// 工作目录
					CreateNoWindow = true,					// 不创建新窗口（后台运行）
					UseShellExecute = false,				// 不使用系统Shell（用于重定向输出）
					Arguments = $"\"{outputDirFullPath}\"",	//Proto编译后的代码的输出目录，传入bat

					// 重定向输入/输出
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				};

				_Process = new Process();
				_Process.StartInfo = processInfo;
				_Process.EnableRaisingEvents = true;
				_Process.Exited += OnProcessExited;

				//注册输出/错误事件处理程序
				_Process.OutputDataReceived += OnCompileProtoLog;
				_Process.ErrorDataReceived += OnCompileProtoError;

				_Process.Start();

				// 如果重定向输出，需要开始异步读取
				_Process.BeginOutputReadLine();
				_Process.BeginErrorReadLine();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				Clear();
				_Process.Dispose();
			}
		}

		private static void OnProcessExited(object sender, EventArgs e)
		{
			int exitCode = _Process.ExitCode;
			UnityEngine.Debug.Log("Compile proto exit code = " + exitCode);

			EditorApplication.delayCall += () =>
			{
				_Process.Dispose();
			};

			if (exitCode == 0)
			{
				EditorLocalPrefs.SetBool(GENERATING_PROTO_KEY, true);
				EditorLocalPrefs.Save();

				//先写一个空的进去占位，避免proto中有删除操作时，旧的Reset扩展代码里还没删掉的对应字段导致报错
				string resetCodeTemplate = File.ReadAllText(PROTO_RESET_TEMPLATE_PATH);
				resetCodeTemplate = string.Format(resetCodeTemplate, "", "");
				WriteCodeFile(resetCodeTemplate, "ResetMethodExtension.cs");

				string msgIDRegistryCodeTemplate = File.ReadAllText(PROTO_MSG_ID_REGISTRY_TEMPLATE_PATH);
				msgIDRegistryCodeTemplate = string.Format(msgIDRegistryCodeTemplate, "", "");
				WriteCodeFile(msgIDRegistryCodeTemplate, "ProtoMsgIDRegistry.cs");

				EditorApplication.delayCall += () =>
				{
					AssetDatabase.Refresh();
				};
			}
			else
				Clear();
		}

		private static void OnAllAssemblyReload()
		{
			try
			{
				int times = EditorLocalPrefs.GetInt(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES, int.MaxValue);
				EditorLocalPrefs.SetInt(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES, --times);
				if (times <= 0)
					Clear();

				bool generatingProto = EditorLocalPrefs.GetBool(GENERATING_PROTO_KEY, false);
				if (generatingProto)
				{
					EditorLocalPrefs.RemoveKey(GENERATING_PROTO_KEY);
					EditorLocalPrefs.Save();

					BiProgress.Show("Compile Proto", "Generate Reset Extension...", 0.8f);

					List<Type> allProtoTypes = GetAllProtoTypes();
					Dictionary<Type, List<FieldInfo>> allProtoFields = GetAllProtoFields(allProtoTypes);
					GenerateCode(allProtoFields);
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				Clear();
			}
		}

		private static void OnCompilationFinished(object _)
		{
			EditorApplication.delayCall += () =>
			{
				//有编译错误时，关闭ProgressBar，避免卡死edtior
				if (EditorUtility.scriptCompilationFailed && _IsProgressBarDisplaying)
					Clear();
			};
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
		/// 生成具体代码
		/// </summary>
		private static void GenerateCode(Dictionary<Type, List<FieldInfo>> protoDict)
		{
			List<Type> allProtoTypes = protoDict.Keys.ToList();
			//按名字排序，同命名空间的自然挨在一起了
			allProtoTypes.Sort(SortProtoTypes);

			GenerateResetMethodExtension(protoDict, allProtoTypes);
			GenerateMsgIDRegistry(allProtoTypes);

			//延迟一帧，否则无法触发代码重新编译
			EditorApplication.delayCall += () =>
			{
				AssetDatabase.Refresh();
			};
		}

		/// <summary>
		/// 为每个Proto类，生成Reset方法
		/// </summary>
		private static void GenerateResetMethodExtension(Dictionary<Type, List<FieldInfo>> protoDict, List<Type> allProtoTypes)
		{
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


			string codeTemplate = File.ReadAllText(PROTO_RESET_TEMPLATE_PATH);
			codeTemplate = string.Format(codeTemplate, iMessageBuilder.ToString(), resetPerProtoBuilder.ToString());
			WriteCodeFile(codeTemplate, "ResetMethodExtension.cs");
		}

		/// <summary>
		/// 生成MsgID的映射
		/// </summary>
		private static void GenerateMsgIDRegistry(List<Type> allProtoTypes)
		{
			StringBuilder stringBuilder = new StringBuilder(10240);
			for (int i = 0; i < allProtoTypes.Count; i++)
			{
				Type protoType = allProtoTypes[i];
				stringBuilder.AppendLine(@$"		Register<{protoType.FullName}>();");
			}

			string codeTemplate = File.ReadAllText(PROTO_MSG_ID_REGISTRY_TEMPLATE_PATH);
			codeTemplate = string.Format(codeTemplate, stringBuilder.ToString());
			WriteCodeFile(codeTemplate, "ProtoMsgIDRegistry.cs");
		}

		private static void WriteCodeFile(string code, string fileName)
		{
			string targetDir = null;
			ProtoSetting setting = GetSetting();
			if (setting != null)
				targetDir = setting.ProtoOutputDir;
			if (string.IsNullOrEmpty(targetDir))
			{
				EditorUtility.DisplayDialog("", $"输出失败，请先去Icy/Proto/Setting菜单中，设置 Proto编译后的代码的输出目录", "OK");
				return;
			}

			string targetFile = Path.Combine(targetDir, fileName);
			File.WriteAllText(targetFile, code);
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

		private static ProtoSetting GetSetting()
		{
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetEditorOnlySettingDir(), "ProtoSetting.json");
			if (bytes != null)
			{
				ProtoSetting setting = ProtoSetting.Parser.ParseFrom(bytes);
				return setting;
			}
			return null;
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

		private static void Clear()
		{
			BiProgress.Hide();
			_IsProgressBarDisplaying = false;
			EditorLocalPrefs.RemoveKey(GENERATING_PROTO_ASSEMBLY_RELOAD_TIMES);
			EditorLocalPrefs.Save();
		}
	}
}
