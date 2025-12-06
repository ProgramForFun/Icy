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
using System.Threading.Tasks;
using UnityEngine;

namespace Icy.Base
{
	public static class FSMTest
	{
		private class FSMTestState : FSMState
		{
			public override async UniTask Activate()
			{
				int rand = Random.Range(1000, 2000);
				string className = GetType().Name;
				Log.Info($"Activate {className} for {rand}", className);
				await UniTask.Delay(rand);
			}

			public override async UniTask Deactivate()
			{
				int rand = Random.Range(1000, 2000);
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

		private class FSMTestProcedureStep : ProcedureStep
		{
			public override async UniTask Activate()
			{
				int rand = Random.Range(1000, 2000);
				string className = GetType().Name;
				Log.Info($"Activate {className} for {rand}", className);
				await UniTask.Delay(rand);
				Finish();
			}

			public override async UniTask Deactivate()
			{
				int rand = Random.Range(1000, 2000);
				string className = GetType().Name;
				Log.Info($"Deactivate {className} for {rand}", className);
				await UniTask.Delay(rand);
			}
		}

		private class ProcedureStep_1 : FSMTestProcedureStep { }
		private class ProcedureStep_2 : FSMTestProcedureStep { }
		private class ProcedureStep_3 : FSMTestProcedureStep { }


		private static FSM _FSM1;
		private static FSM _FSM2;
		private static Dictionary<int, FSMState> _FSMStatesMap1;
		private static Dictionary<int, FSMState> _FSMStatesMap2;
		private static int _PrevRandomIdx1 = -1;
		private static int _PrevRandomIdx2 = -1;

		public static void Test()
		{
			//FSM 1
			_FSMStatesMap1 = new Dictionary<int, FSMState>
			{
				{ 0, new Idle() },
				{ 1, new Patrol() },
				{ 2, new Fight() },
				{ 3, new Escape() },
				{ 4, new Pursue() },
			};

			_FSM1 = new FSM(nameof(FSMTest) + "_1");
			for (int i = 0; i < 5; i++)
				_FSM1.AddState(_FSMStatesMap1[i]);
			_FSM1.Start();

			Timer.RepeatByTime(RandomChangeState1, 6.1f, 100);


			//FSM 2
			_FSMStatesMap2 = new Dictionary<int, FSMState>
			{
				{ 0, new Idle() },
				{ 1, new Patrol() },
				{ 2, new Fight() },
			};

			_FSM2 = new FSM(nameof(FSMTest) + "_2");
			for (int i = 0; i < 3; i++)
				_FSM2.AddState(_FSMStatesMap2[i]);
			_FSM2.Start();

			Timer.RepeatByTime(RandomChangeState2, 6.1f, 100);


			//Procedure
			Procedure procedure = new Procedure("FSMTest");
			procedure.AddStep(new ProcedureStep_1());
			procedure.AddStep(new ProcedureStep_2());
			procedure.AddStep(new ProcedureStep_3());
			procedure.Start();
		}

		private static async void RandomChangeState1()
		{
			if (_FSM1.IsChangingState)
				return;

			int randIdx = Random.Range(0, _FSMStatesMap1.Count);
			while(randIdx == _PrevRandomIdx1)
				randIdx = Random.Range(0, _FSMStatesMap1.Count);

			_FSM1.ChangeState(_FSMStatesMap1[randIdx]);
			await Task.Delay(1000);

			_PrevRandomIdx1 = randIdx;
		}

		private static async void RandomChangeState2()
		{
			if (_FSM2.IsChangingState)
				return;

			int randIdx = Random.Range(0, _FSMStatesMap2.Count);
			while (randIdx == _PrevRandomIdx2)
				randIdx = Random.Range(0, _FSMStatesMap2.Count);

			_FSM2.ChangeState(_FSMStatesMap2[randIdx]);
			await Task.Delay(1000);

			_PrevRandomIdx2 = randIdx;
		}
	}
}
#endif
