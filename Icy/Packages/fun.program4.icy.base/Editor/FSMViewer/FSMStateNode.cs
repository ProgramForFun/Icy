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


using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Icy.Base.Editor
{
	/// <summary>
	/// 状态节点
	/// </summary>
	public class FSMStateNode : Node
	{
		public FSMStateNode(string title)
		{
			this.title = title;

			AddInput(Orientation.Horizontal);
			AddOutput(Orientation.Horizontal);
			//SetColor(Color.cyan);
		}

		public void AddInput(Orientation orientation)
		{
			Port input = Port.Create<Edge>(orientation, Direction.Input, Port.Capacity.Multi, typeof(Port));
			input.portName = string.Empty;
			inputContainer.Add(input);
		}

		public void AddOutput(Orientation orientation)
		{
			Port output = Port.Create<Edge>(orientation, Direction.Output, Port.Capacity.Multi, typeof(Port));
			output.portName = string.Empty;
			outputContainer.Add(output);
		}

		public void SetColor(Color color)
		{
			style.backgroundColor = new UnityEngine.UIElements.StyleColor(color);
		}
	}
}
