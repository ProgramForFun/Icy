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


#if UNITY_EDITOR

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.Base
{
	public static class FSMTest
	{
		private class Idle : FSMState
		{
			public override async UniTask Activate()
			{
				int rand = Random.Range(1000, 3000);
				Log.Info($"Activate {nameof(Idle)} for {rand}", nameof(Idle));
				await UniTask.Delay(rand);
			}

			public override async UniTask Deactivate()
			{
				int rand = Random.Range(1000, 3000);
				Log.Info($"Deactivate {nameof(Idle)} for {rand}", nameof(Idle));
				await UniTask.Delay(rand);
			}
		}

		private class Fight : FSMState
		{
			public override async UniTask Activate()
			{
				int rand = Random.Range(1000, 3000);
				Log.Info($"Activate {nameof(Fight)} for {rand}", nameof(Fight));
				await UniTask.Delay(rand);
			}

			public override async UniTask Deactivate()
			{
				int rand = Random.Range(1000, 3000);
				Log.Info($"Deactivate {nameof(Fight)} for {rand}", nameof(Fight));
				await UniTask.Delay(rand);
			}
		}

		private class Sleep : FSMState
		{
			public override async UniTask Activate()
			{
				int rand = Random.Range(1000, 3000);
				Log.Info($"Activate {nameof(Sleep)} for {rand}", nameof(Sleep));
				await UniTask.Delay(rand);
			}

			public override async UniTask Deactivate()
			{
				int rand = Random.Range(1000, 3000);
				Log.Info($"Deactivate {nameof(Sleep)} for {rand}", nameof(Sleep));
				await UniTask.Delay(rand);
			}
		}

		private static FSM _FSM;
		private static Dictionary<int, FSMState> _FSMStatesMap;
		private static int _PrevRandomIdx = -1;

		public static void Test()
		{
			_FSMStatesMap = new Dictionary<int, FSMState>
			{
				{ 0, new Idle() },
				{ 1, new Fight() },
				{ 2, new Sleep() }
			};

			_FSM = new FSM(nameof(FSMTest));
			_FSM.AddState(_FSMStatesMap[0]);
			_FSM.AddState(_FSMStatesMap[1]);
			_FSM.AddState(_FSMStatesMap[2]);
			_FSM.Start();

			Timer.RepeatByTime(RandomChangeState, 6.1f, 100);
		}

		private static void RandomChangeState()
		{
			int randIdx = Random.Range(0, _FSMStatesMap.Count);
			while(randIdx == _PrevRandomIdx)
				randIdx = Random.Range(0, _FSMStatesMap.Count);

			_FSM.ChangeState(_FSMStatesMap[randIdx]);

			_PrevRandomIdx = randIdx;
		}
	}
}
#endif
