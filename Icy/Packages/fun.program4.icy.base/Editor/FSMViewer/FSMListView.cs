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


using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Icy.Base.Editor
{
	/// <summary>
	/// FSM列表
	/// </summary>
	public class FSMListView : VisualElement
	{
		private ListView _ListView;
		private GraphView _Parent;
		private Action<string> _OnClickFSM;

		public FSMListView(GraphView parent, Action<string> onClickFSM)
		{
			_Parent = parent;
			_OnClickFSM = onClickFSM;
		}

		public void Update(List<string> fsms)
		{
			// 创建外边框容器
			name = "ListContainer";

			// 设置边框样式
			style.borderLeftWidth = 2f;
			style.borderRightWidth = 2f;
			style.borderTopWidth = 2f;
			style.borderBottomWidth = 2f;
			style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f);
			style.borderRightColor = new Color(0.4f, 0.4f, 0.4f);
			style.borderTopColor = new Color(0.4f, 0.4f, 0.4f);
			style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f);
			style.borderTopLeftRadius = 5f;
			style.borderTopRightRadius = 5f;
			style.borderBottomLeftRadius = 5f;
			style.borderBottomRightRadius = 5f;
			style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);

			// 设置容器尺寸和位置
			style.width = 220; // 比ListView宽一些，包含边框
			style.height = 520;
			style.position = Position.Absolute;
			style.left = 10;
			style.top = 10;
			style.paddingLeft = 5;
			style.paddingRight = 5;
			style.paddingTop = 5;
			style.paddingBottom = 5;

			// 创建标题栏
			Label header = new Label("FSM列表");
			header.style.unityTextAlign = TextAnchor.MiddleCenter;
			header.style.height = 25;
			header.style.marginBottom = 5;
			header.style.backgroundColor = new Color(0.3f, 0.3f, 0.5f);
			header.style.borderBottomWidth = 1f;
			header.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f);

			// 创建ListView
			_ListView = new ListView(fsms, 25, MakeItem, BindItem);
			_ListView.style.width = 200;
			_ListView.style.height = 250;
			_ListView.style.flexGrow = 1;

			// 设置ListView内部样式
			_ListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
			_ListView.selectionType = SelectionType.Single;

			// 注册选择事件
			_ListView.selectionChanged += OnSelectionChanged;

			// 将标题和列表添加到容器
			Add(header);
			Add(_ListView);

			// 将容器添加到GraphView
			_Parent.Add(this);
		}

		private VisualElement MakeItem()
		{
			VisualElement itemContainer = new VisualElement();
			itemContainer.style.flexDirection = FlexDirection.Row;
			itemContainer.style.alignItems = Align.Center;
			itemContainer.style.paddingLeft = 5;
			itemContainer.style.paddingRight = 5;
			itemContainer.style.height = 25;

			// 添加图标
			Label icon = new Label("●");
			icon.style.color = new Color(0.2f, 0.8f, 0.3f);
			icon.style.width = 20;
			icon.style.unityTextAlign = TextAnchor.MiddleCenter;

			var label = new Label();
			label.style.unityTextAlign = TextAnchor.MiddleLeft;
			label.style.flexGrow = 1;
			label.style.paddingLeft = 5;

			itemContainer.Add(icon);
			itemContainer.Add(label);

			return itemContainer;
		}

		private void BindItem(VisualElement element, int index)
		{
			if (element is VisualElement container &&
				container.childCount >= 2 &&
				container[1] is Label label &&
				_ListView.itemsSource is List<string> items)
			{
				label.text = items[index];

				// 可以根据索引设置不同的图标颜色
				if (container[0] is Label icon)
				{
					Color[] colors = new Color[]
					{
						new Color(0.9f, 0.2f, 0.2f),	// 红色
						new Color(0.2f, 0.7f, 0.9f),	// 蓝色
						new Color(0.9f, 0.7f, 0.2f),	// 黄色
						new Color(0.3f, 0.8f, 0.3f),	// 绿色
					};
					icon.style.color = colors[index % colors.Length];
				}
			}
		}

		private void OnSelectionChanged(System.Collections.IEnumerable selectedItems)
		{
			foreach (object item in selectedItems)
				_OnClickFSM(item.ToString());
		}

		// 添加方法来动态更新列表
		public void AddListItem(string newItem)
		{
			if (_ListView.itemsSource is List<string> items)
			{
				items.Add(newItem);
				_ListView.Rebuild();
			}
		}

		public void RemoveSelectedItem()
		{
			if (_ListView.selectedIndex >= 0 && _ListView.itemsSource is List<string> items)
			{
				items.RemoveAt(_ListView.selectedIndex);
				_ListView.Rebuild();
			}
		}
	}
}
