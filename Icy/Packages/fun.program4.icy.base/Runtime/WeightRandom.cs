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


using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 基于权重随机的封装
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class WeightRandom<T>
	{
		/// <summary>
		/// 由多少个权重分布构成的
		/// </summary>
		public int WeightCount { get { return _WeightDistribute.Count; } }
		/// <summary>
		/// 权重总和
		/// </summary>
		private float _TotalWeight;
		/// <summary>
		/// 权重分布和具体类型的映射
		/// </summary>
		private List<KeyValuePair<float, T>> _WeightDistribute;

		/// <summary>
		/// 注意两个参数的数量必须相同
		/// </summary>
		/// <param name="weights">权重分布数组</param>
		/// <param name="values">权重分布对应的具体类型的数组</param>
		public WeightRandom(float[] weights, T[] values)
		{
			if (weights.Length == 0 || values.Length == 0
				|| weights.Length != values.Length)
			{
				string log = string.Format("Construct RandomWeight failed, T = {0}, weights count = {1}, values count = {2}"
									, typeof(T).Name, weights.Length, values.Length);
				Log.LogError(log);
			}

			_TotalWeight = 0.0f;
			_WeightDistribute = new List<KeyValuePair<float, T>>();

			for (int i = 0; i < weights.Length; i++)
				_TotalWeight += weights[i];

			float sum = 0.0f;
			for (int i = 0; i < weights.Length; i++)
			{
				sum += weights[i] / _TotalWeight;
				_WeightDistribute.Add(new KeyValuePair<float, T>(sum, values[i]));
			}
		}

		/// <summary>
		/// 注意两个参数的数量必须相同
		/// </summary>
		/// <param name="weights">权重分布列表</param>
		/// <param name="values">权重分布对应的具体类型的列表</param>
		public WeightRandom(List<float> weights, List<T> values) : this(weights.ToArray(), values.ToArray()) { }

		/// <summary>
		/// 随机出一个结果，根据内部的权重
		/// </summary>
		/// <returns></returns>
		public T Random()
		{
			float rand = UnityEngine.Random.value;
			for (int i = 0; i < _WeightDistribute.Count; i++)
			{
				if (rand < _WeightDistribute[i].Key)
					return _WeightDistribute[i].Value;
			}

			Log.LogError("Invalid return path of RandomWeight.Random, T = " + typeof(T).Name);
			return _WeightDistribute[0].Value;
		}
	}
}
