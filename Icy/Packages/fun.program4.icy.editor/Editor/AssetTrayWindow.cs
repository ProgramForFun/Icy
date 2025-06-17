using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Icy.Asset.Editor;
using UnityEngine;

namespace Icy.Editor
{
	/// <summary>
	/// Asset托盘窗口，便于暂存常用的文件
	/// </summary>
	public sealed class AssetTrayWindow : OdinEditorWindow
	{
		private const string ASSET_TRAY_KEY = "_Icy_AssetTray";
		private static AssetTrayWindow _AssetTrayWindow;

		[TableList(AlwaysExpanded = true, MinScrollViewHeight = 1000)]
		[ListDrawerSettings(DraggableItems = true)]
		[OnCollectionChanged("OnTableListChanged")]
		public List<AssetTrayWindowItem> _Assets;

		/// <summary>
		/// 资源路径List
		/// </summary>
		private List<string> _AssetPaths;


		[MenuItem("Icy/Asset/AssetTray", false)]
		public static void Open()
		{
			if (_AssetTrayWindow != null)
				_AssetTrayWindow.Close();
			_AssetTrayWindow = GetWindow<AssetTrayWindow>();

			Load();
		}

		private static void Load()
		{
			_AssetTrayWindow._Assets = new List<AssetTrayWindowItem>();
			_AssetTrayWindow._AssetPaths = new List<string>();

			string saved = EditorLocalPrefs.GetString(ASSET_TRAY_KEY, string.Empty);
			if (!string.IsNullOrEmpty(saved))
			{
				string[] split = saved.Split(";");
				_AssetTrayWindow._AssetPaths = new List<string>(split);
				for (int i = 0; i < split.Length; i++)
				{
					Object obj = AssetDatabase.LoadAssetAtPath<Object>(_AssetTrayWindow._AssetPaths[i]);
					_AssetTrayWindow.AddAsset(obj, false);
				}
			}
		}

		private void OnTableListChanged(CollectionChangeInfo info, object value)
		{
			if (info.ChangeType == CollectionChangeType.RemoveValue || info.ChangeType == CollectionChangeType.RemoveKey || info.ChangeType == CollectionChangeType.RemoveIndex)
			{
				_AssetPaths.RemoveAt(info.Index);
				Save();
			}
		}

		[Button("添加当前选中的资源进托盘", ButtonSizes.Medium)]
		public void AddSelection()
		{
			Object[] select = Selection.objects;
			for (int i = 0; i < select.Length; i++)
				AddAsset(select[i], true);
		}

		protected override void OnImGUI()
		{
			base.OnImGUI();

			//拖拽资源到窗口
			Event e = Event.current;
			if (e.type == EventType.DragUpdated)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				e.Use();
			}
			else if (e.type == EventType.DragPerform)
			{
				foreach (Object asset in DragAndDrop.objectReferences)
					AddAsset(asset, true);
				e.Use();
			}
		}

		private void AddAsset(Object asset, bool add2Data)
		{
			//判断是否重复
			for (int i = 0; i < _Assets.Count; i++)
			{
				if (_Assets[i].Asset == asset)
					return;
			}

			//判断是否是Project窗口里的独立资源
			if (!EditorUtility.IsPersistent(asset))
			{
				Debug.LogError("添加失败：添加的资源并不是一个Project窗口里的独立资源");
				return;
			}

			AssetTrayWindowItem item = new AssetTrayWindowItem();
			item.Asset = asset;
			item.Name = asset.name;
			_Assets.Add(item);

			string path = AssetDatabase.GetAssetPath(asset);

			if (add2Data)
			{
				_AssetPaths.Add(path);
				Save();
			}
		}

		private void Save()
		{
			List<string> all = new List<string>();
			for (int i = 0; i < _AssetPaths.Count; i++)
				all.Add(_AssetPaths[i]);

			EditorLocalPrefs.SetString(ASSET_TRAY_KEY, string.Join(";", all));
			EditorLocalPrefs.Save();
		}
	}
}
