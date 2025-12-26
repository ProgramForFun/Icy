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


using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// GameObject专用池，接受GameObject作为模板
	/// </summary>
	public class GameObjectPool : ObjectPool<GameObject>
	{
		protected GameObject _Template;

		public GameObjectPool(GameObject template, int defaultSize = 16) : base(defaultSize)
		{
			_Template = template;
			if (_Template != null)
				_Template.SetActive(false);
		}

		public override GameObject Get()
		{
			//Get时不处理Active，由外部决定什么时候Active
			return base.Get();
		}

		public override void Put(GameObject obj)
		{
			obj.SetActive(false);
			base.Put(obj);
		}

		protected override GameObject InstantiateOne()
		{
			if (_Template == null)
			{
				Log.Error("GameObject template is null", nameof(GameObjectPool));
				return null;
			}
			return UnityEngine.Object.Instantiate(_Template);
		}

		public override void Dispose()
		{
			if (_OutPool.Count > 0)
				Log.Warn($"Dispose {nameof(GameObjectPool)} when there are GameObjects outside, first outside GameObject = " + _OutPool[0].name);

			for (int i = 0; i < _InPool.Count; i++)
				UnityEngine.Object.Destroy(_InPool[i]);
			_InPool.Clear();

			for (int i = 0; i < _OutPool.Count; i++)
				UnityEngine.Object.Destroy(_OutPool[i]);
			_OutPool.Clear();

			//确保是Instance，而不是Prefab Asset
			if (_Template.scene != null && _Template.scene.IsValid())
				UnityEngine.Object.Destroy(_Template);
		}
	}
}
