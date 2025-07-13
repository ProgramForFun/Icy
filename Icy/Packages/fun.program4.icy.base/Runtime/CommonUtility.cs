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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Icy.Base
{
	public static class CommonUtility
	{
		private static System.Random _randomEngine = new System.Random();

		/// <summary>
		/// 尝试一个概率事件是否发生
		/// </summary>
		/// <param name="probability">概率，取值1~100</param>
		/// <returns></returns>
		public static bool TryMyLuck(int probability)
		{
			return Random.Range(1, 100) <= probability;
		}

		/// <summary>
		/// 解一元二次方程ax^2 + bx + c = 0
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <param name="x1"></param>
		/// <param name="x2"></param>
		/// <returns></returns>
		public static bool SolveQuadraticEquation(float a, float b, float c, out float x1, out float x2)
		{
			float delta = b * b - 4.0f * a * c;

			//无解
			if (delta < 0)
			{
				x1 = 0;
				x2 = 0;
				return false;
			}

			float deltaSqrt = Mathf.Sqrt(delta);
			x1 = (-b + deltaSqrt) / (2.0f * a);
			x2 = (-b - deltaSqrt) / (2.0f * a);
			return true;
		}

		/// <summary>
		/// 把角度clamp在0~360区间内
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static float ClampAngleIn0To360(float angle)
		{
			while (angle >= 360.0f)
				angle -= 360.0f;
			while (angle < 0.0f)
				angle += 360.0f;
			return angle;
		}

		/// <summary>
		/// 判断一个Bounds是否在指定相机的视锥内
		/// </summary>
		/// <param name="camera">指定相机</param>
		/// <param name="bounds">要判断的bounds</param>
		/// <returns></returns>
		public static bool IsInFrustum(this Camera camera, Bounds bounds)
		{
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
			return GeometryUtility.TestPlanesAABB(planes, bounds);
		}

		/// <summary>
		/// RGB颜色应用HDR Intensity；
		/// https://forum.unity.com/threads/how-to-change-hdr-colors-intensity-via-shader.531861/
		/// </summary>
		/// <param name="color"></param>
		/// <param name="intensity">Unity HDR颜色调色盘上的Intensity</param>
		/// <returns></returns>
		public static Color ApplyHDR(this ref Color color, float intensity)
		{
			return color *= MathF.Pow(2.0f, intensity);
		}

		/// <summary>
		/// 把当前DateTime转换为从1970.1.1 零点以来所有的秒数
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static long TotalSeconds(this DateTime date)
		{
			TimeSpan diff = date.ToUniversalTime() - DateTime.UnixEpoch;
			return (long)diff.TotalSeconds;
		}

		/// <summary>
		/// 计算MD5，返回32位风格的MD5结果
		/// </summary>
		/// <param name="src"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static byte[] MD5(byte[] src, int length)
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			//C#返回16位byte
			byte[] hash = md5.ComputeHash(src, 0, length);

			int len = hash.Length;
			char[] cs = new char[len * 2];
			int index = 0;
			for (int i = 0; i < len; i++)
			{
				byte byte0 = hash[i];
				cs[index++] = MD5_HEX_DIGITS[byte0 >> 4 & 0xf];
				cs[index++] = MD5_HEX_DIGITS[byte0 & 0xf];
			}
			return Encoding.ASCII.GetBytes(cs);
		}
		private static char[] MD5_HEX_DIGITS = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

		/// <summary>
		/// List的扩展洗牌算法
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = _randomEngine.Next(n + 1);
				T tmp = list[k];
				list[k] = list[n];
				list[n] = tmp;
			}
		}

		/// <summary>
		/// 比较两个Dictionary的内容是否相同
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dict1"></param>
		/// <param name="dict2"></param>
		/// <returns></returns>
		public static bool CompareDict<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
		{
			if (dict1 == dict2)
				return true;
			if ((dict1 == null) || (dict2 == null))
				return false;
			if (dict1.Count != dict2.Count)
				return false;

			EqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;

			foreach (var kvp in dict1)
			{
				TValue value2;
				if (!dict2.TryGetValue(kvp.Key, out value2))
					return false;
				if (!valueComparer.Equals(kvp.Value, value2))
					return false;
			}
			return true;
		}

		/// <summary>
		/// 复制目录
		/// </summary>
		/// <param name="sourceDir">要复制的目录</param>
		/// <param name="destDir">复制到的目标目录</param>
		/// <param name="copySubDirs">是否复制子文件夹</param>
		public static bool CopyDir(string sourceDir, string destDir, bool copySubDirs = true)
		{
			DirectoryInfo source = new DirectoryInfo(sourceDir);

			if (!source.Exists)
			{
				Log.LogError(sourceDir + " does not exist", "CopyDir");
				return false;
			}

			Directory.CreateDirectory(destDir);

			foreach (FileInfo file in source.GetFiles())
			{
				string destFilePath = Path.Combine(destDir, file.Name);
				file.CopyTo(destFilePath, true); // 覆盖已存在文件
			}

			if (copySubDirs)
			{
				foreach (DirectoryInfo subDir in source.GetDirectories())
				{
					string destSubDirPath = Path.Combine(destDir, subDir.Name);
					CopyDir(subDir.FullName, destSubDirPath, copySubDirs);
				}
			}
			return true;
		}

		/// <summary>
		/// Copy指定文件名的文件，到指定目录
		/// </summary>
		/// <param name="sourceDir">要复制的目录</param>
		/// <param name="targetDir">复制到的目标目录</param>
		/// <param name="nameSet">指定的文件名</param>
		/// <param name="overwrite">是否允许覆盖同名文件</param>
		public static void CopyFilesByNames(string sourceDir, string targetDir, HashSet<string> nameSet, bool overwrite = true)
		{
			if (!Directory.Exists(sourceDir))
			{
				Log.LogError(sourceDir + " does not exist", "CopyFilesByNameList");
				return;
			}

			Directory.CreateDirectory(targetDir);

			string[] allFiles = Directory.GetFiles(sourceDir);
			for (int i = 0; i < allFiles.Length; i++)
			{
				string filePath = allFiles[i];
				string fileName = Path.GetFileName(filePath);
				if (nameSet.Contains(fileName))
				{
					string destPath = Path.Combine(targetDir, fileName);
					File.Copy(filePath, destPath, overwrite);
				}
			}
		}

		/// <summary>
		/// Copy指定扩展名的文件，到指定目录
		/// </summary>
		/// <param name="sourceDir">要复制的目录</param>
		/// <param name="targetDir">复制到的目标目录</param>
		/// <param name="extensionWithDotPrefix">带“点”的扩展名，比如".txt"</param>
		/// <param name="overwrite">是否允许覆盖同名文件</param>
		public static void CopyFilesByExtension(string sourceDir, string targetDir, string extensionWithDotPrefix, bool overwrite = true)
		{
			if (!Directory.Exists(sourceDir))
			{
				Log.LogError(sourceDir + " does not exist", "CopyFilesByNameList");
				return;
			}

			Directory.CreateDirectory(targetDir);

			string[] allFiles = Directory.GetFiles(sourceDir);
			for (int i = 0; i < allFiles.Length; i++)
			{
				string filePath = allFiles[i];
				string fileExtension = Path.GetExtension(filePath);
				if (fileExtension == extensionWithDotPrefix)
				{
					string destPath = Path.Combine(targetDir, Path.GetFileName(filePath));
					File.Copy(filePath, destPath, overwrite);
				}
			}
		}

		/// <summary>
		/// 逐byte异或一个byte数组
		/// </summary>
		/// <param name="array">要异或的byte数组</param>
		/// <param name="xorWith">用来做异或运算的数值</param>
		public static void xor(byte[] array, byte xorWith = 101)
		{
			for (int i = 0; i < array.Length; i++)
				array[i] ^= xorWith;
		}

		/// <summary>
		/// 检查当前类型是否派生自指定的泛型类型（忽略泛型参数）；
		/// 需要用到元数据，谨慎在运行时使用
		/// </summary>
		/// <param name="typeToCheck">要检查的类型</param>
		/// <param name="genericBaseType">泛型基类型（如 typeof(MyGenericClass<>)）</param>
		/// <returns>如果派生自指定的泛型类型则返回 true，否则返回 false</returns>
		public static bool IsSubclassOfRawGeneric(Type typeToCheck, Type genericBaseType)
		{
			if (!genericBaseType.IsGenericType || !genericBaseType.IsGenericTypeDefinition)
			{
				Log.LogError($"{nameof(genericBaseType)} must be a generic type without argument");
				return false;
			}

			Type objType = typeof(object);
			// 1. 检查当前类型及其所有基类（不包括接口）
			while (typeToCheck != null && typeToCheck != objType)
			{
				// 如果是泛型类型，先获取泛型定义
				Type current = typeToCheck.IsGenericType
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
				foreach (Type interfaceType in typeToCheck.GetInterfaces())
				{
					Type currentInterface = interfaceType.IsGenericType
						? interfaceType.GetGenericTypeDefinition()
						: interfaceType;

					if (currentInterface == genericBaseType)
						return true;
				}
			}

			return false;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Unity Editor是否有编译错误
		/// </summary>
		public static bool HasCompileErrors()
		{
			return UnityEditor.EditorUtility.scriptCompilationFailed;
		}
#endif

		#region Vector
		/// <summary>
		/// 在一个Rect范围内随机一个点
		/// </summary>
		/// <param name="rect"></param>
		/// <returns></returns>
		public static Vector2 RandomInRect(Rect rect)
		{
			float x = Random.Range(rect.xMin, rect.xMax);
			float y = Random.Range(rect.yMin, rect.yMax);
			return new Vector2(x, y);
		}

		/// <summary>
		/// 计算贝塞尔曲线上指定t的一点
		/// </summary>
		/// <param name="start">曲线的起始位置</param>
		/// <param name="control">决定曲线形状的控制点</param>
		/// <param name="end">曲线的终点</param>
		/// <param name="t">0到1的值，0获取曲线的起点，1获得曲线的终点</param>
		public static Vector3 CalculateBezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
		{
			return (1.0f - t) * (1.0f - t) * start + 2.0f * t * (1.0f - t) * control + t * t * end;
		}

		/// <summary>
		/// 计算两个向量的夹角，结果在-180~180，而Vector3.Angle在0~180
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="up">用于参考的上方向</param>
		/// <returns></returns>
		public static float AnglePlusOrMinus(Vector3 from, Vector3 to, Vector3 up)
		{
			float rtn = Vector3.Angle(from, to);
			Vector3 normal = Vector3.Cross(from, to);
			rtn *= Mathf.Sign(Vector3.Dot(normal, up)); //求法线向量与上方向向量点乘，结果为1或-1，修正旋转方向
			return rtn;
		}

		/// <summary>
		/// Vector3扩展，方便转换成xz平面的Vector2
		/// </summary>
		/// <param name="vec3"></param>
		/// <returns></returns>
		public static Vector2 xz(this Vector3 vec3)
		{
			return new Vector2(vec3.x, vec3.z);
		}

		/// <summary>
		/// Vector2扩展，方便把Vector2转换成(x, 0, y)的Vector3
		/// </summary>
		/// <param name="vec2"></param>
		/// <returns></returns>
		public static Vector3 x0y(this Vector2 vec2)
		{
			return new Vector3(vec2.x, 0.0f, vec2.y);
		}

		/// <summary>
		/// Vector3扩展，方便把y轴归零
		/// </summary>
		/// <param name="vec3"></param>
		/// <returns></returns>
		public static Vector3 x0z(this Vector3 vec3)
		{
			return new Vector3(vec3.x, 0.0f, vec3.z);
		}
		#endregion

		#region GameObject & Transform
		/// <summary>
		/// 获取或添加一个组件
		/// </summary>
		/// <typeparam name="T">任意Component</typeparam>
		/// <param name="go">所属GameObject</param>
		/// <returns>获取或添加的组件实例</returns>
		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			T rtn = go.GetComponent<T>();
			if (rtn == null)
				rtn = go.AddComponent<T>();
			return rtn;
		}

		/// <summary>
		/// 设置Transform的父子关系，位置、缩放、旋转都为默认值
		/// </summary>
		/// <param name="parent">父</param>
		/// <param name="child">子</param>
		/// <param name="fitLayer">是否和父节点统一Layer</param>
		public static void SetParent(Transform parent, Transform child, bool fitLayer = true)
		{
			child.SetParent(parent);
			child.localPosition = Vector3.zero;
			child.localScale = Vector3.one;
			child.localRotation = Quaternion.identity;
			if (fitLayer)
				child.gameObject.SetLayerRecursively(parent.gameObject.layer);
		}

		/// <summary>
		/// 设置GameObject的父子关系，位置、缩放、旋转都为默认值
		/// </summary>
		/// <param name="parent">父</param>
		/// <param name="child">子</param>
		/// <param name="fitLayer">是否和父节点统一Layer</param>
		public static void SetParent(GameObject parent, GameObject child, bool fitLayer = true)
		{
			SetParent(parent.transform, child.transform, fitLayer);
		}

		/// <summary>
		/// 递归设置所有Children的Layer
		/// </summary>
		/// <param name="layer"></param>
		public static void SetLayerRecursively(this GameObject go, int layer)
		{
			go.layer = layer;
			Transform trans = go.transform;
			for (int i = 0; i < trans.childCount; i++)
				SetLayerRecursively(trans.GetChild(i).gameObject, layer);
		}

		/// <summary>
		/// 递归设置所有Children的Layer
		/// </summary>
		/// <param name="layer"></param>
		public static void SetLayerRecursively(this GameObject go, string layer)
		{
			go.SetLayerRecursively(LayerMask.NameToLayer(layer));
		}

		/// <summary>
		/// 设置Transform的全局scale
		/// </summary>
		/// <param name="trans"></param>
		/// <param name="globalScale"></param>
		public static void SetGlobalScale(this Transform trans, Vector3 globalScale)
		{
			trans.localScale = Vector3.one;
			trans.localScale = new Vector3(globalScale.x / trans.lossyScale.x
										, globalScale.y / trans.lossyScale.y
										, globalScale.z / trans.lossyScale.z);
		}

		/// <summary>
		/// 获取Transform在Hierarchy中的路径
		/// </summary>
		/// <param name="trans"></param>
		/// <returns></returns>
		public static string GetHierarchyPath(Transform trans)
		{
			if (trans == null)
				return string.Empty;
			string path = trans.gameObject.name;
			Transform parent = trans.parent;
			while (parent != null)
			{
				path = parent.name + "/" + path;
				parent = parent.parent;
			}
			return path;
		}

		/// <summary>
		/// Transform递归查找指定名字的子节点，BFS
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Transform FindChildBFS(this Transform parent, string name)
		{
			queue4TransFindBFS.Enqueue(parent);
			while (queue4TransFindBFS.Count > 0)
			{
				Transform c = queue4TransFindBFS.Dequeue();
				if (c.name == name)
				{
					queue4TransFindBFS.Clear();
					return c;
				}
				for (int i = 0; i < c.childCount; i++)
					queue4TransFindBFS.Enqueue(c.GetChild(i));
			}
			queue4TransFindBFS.Clear();
			return null;
		}
		private static Queue<Transform> queue4TransFindBFS = new Queue<Transform>();

		/// <summary>
		/// Transform递归查找指定名字的子节点，DFS
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Transform FindChildDFS(this Transform parent, string name)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.name == name)
					return child;
				Transform result = child.FindChildDFS(name);
				if (result != null)
					return result;
			}
			return null;
		}

		/// <summary>
		/// Transform从子节点到某个父节点的路径
		/// </summary>
		/// <param name="parent">找寻到的父节点</param>
		/// <param name="child">开始逆向找寻子节点</param>
		/// <returns>路径</returns>
		public static string GetChildPath(Transform parent, Transform child)
		{
			// 最大支持20层深度查找
			int tryFindDepthTimes = 20;
			StringBuilder strBuilder = new StringBuilder();
			strBuilder.Append(child.name);
			for (int i = 0; i < tryFindDepthTimes; i++)
			{
				Transform p = child.parent;
				if (p == null || p == parent)
				{
					break;
				}
				else
				{
					strBuilder = strBuilder.Insert(0, $"{p.name}/");
					child = p;
				}
			}

			return strBuilder.ToString();
		}

		/// <summary>
		/// TransformBFS查找 所有 指定名字的子节点，并对其执行指定操作
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static void OperateChildrenBFS(this Transform parent, string name, Action<Transform> operation)
		{
			queue4OperateChildrenBFS.Enqueue(parent);
			while (queue4OperateChildrenBFS.Count > 0)
			{
				Transform c = queue4OperateChildrenBFS.Dequeue();
				if (c.name == name)
					operation(c);
				for (int i = 0; i < c.childCount; i++)
					queue4OperateChildrenBFS.Enqueue(c.GetChild(i));
			}
			queue4OperateChildrenBFS.Clear();
		}
		private static Queue<Transform> queue4OperateChildrenBFS = new Queue<Transform>();

		/// <summary>
		/// TransformDFS查找 所有 指定名字的子节点，并对其执行指定操作
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		public static void OperateChildrenDFS(this Transform parent, string name, Action<Transform> operation)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.name == name)
					operation(child);
				child.OperateChildrenDFS(name, operation);
			}
		}

		/// <summary>
		/// 复制Transform 世界 空间下的位置、旋转、缩放
		/// </summary>
		/// <param name="src">从这个复制</param>
		/// <param name="dest">复制到这个</param>
		public static void CopyTransformValue(Transform src, Transform dest)
		{
			dest.position = src.position;
			dest.rotation = src.rotation;
			dest.SetGlobalScale(src.lossyScale);
		}

		/// <summary>
		/// 复制Transform 本地 空间下的位置、旋转、缩放
		/// </summary>
		/// <param name="src">从这个复制</param>
		/// <param name="dest">复制到这个</param>
		public static void CopyTransformLocalValue(Transform src, Transform dest)
		{
			dest.localPosition = src.localPosition;
			dest.localScale = src.localScale;
			dest.localRotation = src.localRotation;
		}

		public static void SetPosX(this Transform trans, float x)
		{
			Vector3 curPos = trans.position;
			curPos.x = x;
			trans.position = curPos;
		}

		public static void SetPosY(this Transform trans, float y)
		{
			Vector3 curPos = trans.position;
			curPos.y = y;
			trans.position = curPos;
		}

		public static void SetPosZ(this Transform trans, float z)
		{
			Vector3 curPos = trans.position;
			curPos.z = z;
			trans.position = curPos;
		}

		public static void SetLocalPosX(this Transform trans, float x)
		{
			Vector3 curPos = trans.localPosition;
			curPos.x = x;
			trans.localPosition = curPos;
		}

		public static void SetLocalPosY(this Transform trans, float y)
		{
			Vector3 curPos = trans.localPosition;
			curPos.y = y;
			trans.localPosition = curPos;
		}

		public static void SetLocalPosZ(this Transform trans, float z)
		{
			Vector3 curPos = trans.localPosition;
			curPos.z = z;
			trans.localPosition = curPos;
		}

		#region String
		public static int ToInt(this string s)
		{
			return int.Parse(s);
		}

		public static long ToLong(this string s)
		{
			return long.Parse(s);
		}

		public static float ToFloat(this string s)
		{
			return float.Parse(s);
		}

		public static double ToDouble(this string s)
		{
			return double.Parse(s);
		}

		public static bool ToBool(this string s)
		{
			if (s == "1" || s.ToLower() == "true")
				return true;
			if (s == "0" || s.ToLower() == "false")
				return false;
			throw new Exception("string to bool failed, string = " + s);
		}

		/// <summary>
		/// 时间要为 yyyy-mm-dd hh:mm:ss的格式
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static long ToTimestampInMs(this string s)
		{
			DateTimeOffset dto = new DateTimeOffset(DateTime.Parse(s));
			return dto.ToUnixTimeMilliseconds();
		}

		/// <summary>
		/// 时间要为 yyyy-mm-dd hh:mm:ss的格式
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static long ToTimestamp(this string s)
		{
			return ToTimestampInMs(s) / 1000L;
		}
		#endregion

		#endregion

		#region Platform
		/// <summary>
		/// 使用Android Toast显示文本消息
		/// </summary>
		/// <param name="message"></param>
		public static void ShowAndroidToast(string message)
		{
#if UNITY_ANDROID
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

			if (unityActivity != null)
			{
				AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
				unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
				{
					AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
					toastObject.Call("show");
				}));
			}
#else
		Log.LogError("Call ShowAndroidToast from Non-Android platform");
#endif
		}

		/// <summary>
		/// 是否是低端设备；PC始终返回高端
		/// </summary>
		/// <returns></returns>
		public static bool IsLowEndDevice()
		{
			string operatorSystem = SystemInfo.operatingSystem;
			Log.LogInfo("[Device] OperatorSystem = " + operatorSystem);
			int ramMB = SystemInfo.systemMemorySize;
			Log.LogInfo("[Device] RAM in MB = " + ramMB);
#if UNITY_ANDROID
			//string operatorSystem = "Android OS 13 / API-33 (TQ2A.230305.008.C1/9619669)";
			bool succeed = int.TryParse(operatorSystem.Substring(11, 2), out int androidVersion);
			if (succeed && androidVersion <= 9)
				return true;

			if (ramMB <= 1024 * 6)
				return true;
#endif

#if UNITY_IOS
		bool succeed = int.TryParse(UnityEngine.iOS.Device.systemVersion, out int iOSVersion);
		if (succeed && iOSVersion <= 12)
			return true;
#endif
			return false;
		}


		/// <summary>
		/// 当前运行的Player，是否运行在PC平台下；
		/// PC泛指个人电脑，包括Windows、Mac OSX、Linux
		/// </summary>
		/// <param name="includeUnityEditor">如果为true的话，eidtor下也算是PC平台；默认为false</param>
		/// <returns></returns>
		public static bool IsPCPlatformPlayer(bool includeUnityEditor = false)
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer
				|| Application.platform == RuntimePlatform.OSXPlayer
				|| Application.platform == RuntimePlatform.LinuxPlayer)
			{
				return true;
			}

			if (includeUnityEditor && Application.isEditor)
				return true;

			return false;
		}

		/// <summary>
		/// 加载StreamingAssets目录下的资源（Coroutine + 回调版本）
		/// </summary>
		/// <param name="filePath">相对于SteamingAssets的目录</param>
		/// <param name="onSuccess">成功回调</param>
		/// <param name="onError">失败回调</param>
		public static IEnumerator LoadStreamingAsset(string filePath, Action<string, byte[]> onSuccess, Action<string, string> onError = null)
		{
			string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);

#if UNITY_ANDROID && !UNITY_EDITOR
		using (UnityWebRequest request = UnityWebRequest.Get(fullPath))
		{
			request.downloadHandler = new DownloadHandlerBuffer();
			yield return request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
				onSuccess?.Invoke(filePath, request.downloadHandler.data);
			else
				onError?.Invoke(filePath, request.error);
		}
#else
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				onSuccess?.Invoke(filePath, bytes);
			}
			else
				onError?.Invoke(filePath, $"File not found: {fullPath}");
			yield return null;
#endif
		}

		/// <summary>
		/// 加载StreamingAssets目录下的资源（UniTask版本）
		/// </summary>
		/// <param name="filePath">相对于SteamingAssets的目录</param>
		public static async UniTask<byte[]> LoadStreamingAsset(string filePath)
		{
			string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);

#if UNITY_ANDROID && !UNITY_EDITOR
		using (UnityWebRequest request = UnityWebRequest.Get(fullPath))
		{
			request.downloadHandler = new DownloadHandlerBuffer();
			await request.SendWebRequest();

			if (request.result == UnityWebRequest.Result.Success)
				return request.downloadHandler.data;
			else
				return null;
		}
#else
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				return bytes;
			}
			else
			{
				await UniTask.CompletedTask;
				return null;
			}
#endif
		}
		#endregion
	}
}
