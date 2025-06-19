namespace Icy.Base
{
	public interface IUpdateable
	{
		void Update(float delta);
	}

	public interface IFixedUpdateable
	{
		void FixedUpdate(float delta);
	}

	public interface ILateUpdateable
	{
		void LateUpdate(float delta);
	}
}
