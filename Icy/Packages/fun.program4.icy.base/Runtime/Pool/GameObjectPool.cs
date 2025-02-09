using Icy.Base;
using UnityEngine;

/// <summary>
/// GameObject专用池
/// </summary>
public class GameObjectPool : ObjectPool<GameObject>
{
	protected GameObject _Template;

	public GameObjectPool(GameObject template, int defaultSize = 16) : base(defaultSize)
	{
		_Template = template;
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
		return UnityEngine.Object.Instantiate(_Template);
	}

	public override void Dispose()
	{
		if (_OutPool.Count > 0)
			Log.LogWarning("Dispose GameObjectPool when there are GameObjects outside, first outside GameObject = " + _OutPool[0].name);

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
