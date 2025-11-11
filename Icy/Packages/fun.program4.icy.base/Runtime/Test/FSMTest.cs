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
		private class FSMTestState : FSMState
		{
			public override async UniTask Activate()
			{
				int rand = Random.Range(1000, 3000);
				string className = GetType().Name;
				Log.Info($"Activate {className} for {rand}", className);
				await UniTask.Delay(rand);
			}

			public override async UniTask Deactivate()
			{
				int rand = Random.Range(1000, 3000);
				string className = GetType().Name;
				Log.Info($"Deactivate {className} for {rand}", className);
				await UniTask.Delay(rand);
			}
		}

		private class Idle : FSMTestState { }
		private class Fight : FSMTestState { }
		private class Patrol : FSMTestState { }
		private class Escape : FSMTestState { }
		private class Pursue : FSMTestState { }

		private static FSM _FSM;
		private static Dictionary<int, FSMState> _FSMStatesMap;
		private static int _PrevRandomIdx = -1;

		public static void Test()
		{
			_FSMStatesMap = new Dictionary<int, FSMState>
			{
				{ 0, new Idle() },
				{ 1, new Patrol() },
				{ 2, new Fight() },
				{ 3, new Escape() },
				{ 4, new Pursue() },
			};

			_FSM = new FSM(nameof(FSMTest));
			for (int i = 0; i < 5; i++)
				_FSM.AddState(_FSMStatesMap[i]);
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
