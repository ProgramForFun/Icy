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


using UnityEngine.Scripting;


/// <summary>
/// 显示引用Unity的各种类，避免被裁剪掉；
/// 以保证HybridCLR热更时使用了一个之前没用过的类时、都可以找得到
/// </summary>
[Preserve]
public class UnityClassReferencer
{
	[Preserve]
	public static void Preserve()
	{
		// 基本类型
		typeof(UnityEngine.Object).ToString();
		typeof(UnityEngine.GameObject).ToString();
		typeof(UnityEngine.Component).ToString();
		typeof(UnityEngine.Behaviour).ToString();
		typeof(UnityEngine.MonoBehaviour).ToString();
		typeof(UnityEngine.ScriptableObject).ToString();

		// 变换和层次结构
		typeof(UnityEngine.Transform).ToString();
		typeof(UnityEngine.RectTransform).ToString();

		// 渲染相关
		typeof(UnityEngine.Camera).ToString();
		typeof(UnityEngine.Renderer).ToString();
		typeof(UnityEngine.MeshRenderer).ToString();
		typeof(UnityEngine.SkinnedMeshRenderer).ToString();
		typeof(UnityEngine.SpriteRenderer).ToString();
		typeof(UnityEngine.ParticleSystemRenderer).ToString();
		typeof(UnityEngine.LineRenderer).ToString();
		typeof(UnityEngine.TrailRenderer).ToString();
		typeof(UnityEngine.BillboardRenderer).ToString();

		// 网格和过滤器
		typeof(UnityEngine.MeshFilter).ToString();
		typeof(UnityEngine.Mesh).ToString();

		// 物理系统
		typeof(UnityEngine.Rigidbody).ToString();
		typeof(UnityEngine.Rigidbody2D).ToString();
		typeof(UnityEngine.Collider).ToString();
		typeof(UnityEngine.BoxCollider).ToString();
		typeof(UnityEngine.SphereCollider).ToString();
		typeof(UnityEngine.CapsuleCollider).ToString();
		typeof(UnityEngine.MeshCollider).ToString();
		typeof(UnityEngine.WheelCollider).ToString();
		typeof(UnityEngine.TerrainCollider).ToString();
		typeof(UnityEngine.CharacterController).ToString();
		//typeof(UnityEngine.PhysicsMaterial).ToString();
		typeof(UnityEngine.PhysicsMaterial2D).ToString();
		typeof(UnityEngine.Physics).ToString();
		typeof(UnityEngine.Physics2D).ToString();
		typeof(UnityEngine.Joint).ToString();
		typeof(UnityEngine.HingeJoint).ToString();
		typeof(UnityEngine.SpringJoint).ToString();
		typeof(UnityEngine.FixedJoint).ToString();
		typeof(UnityEngine.CharacterJoint).ToString();
		typeof(UnityEngine.ConfigurableJoint).ToString();

		// 动画系统
		typeof(UnityEngine.Animator).ToString();
		typeof(UnityEngine.Animation).ToString();
		typeof(UnityEngine.AnimationClip).ToString();
		typeof(UnityEngine.AnimatorOverrideController).ToString();
		typeof(UnityEngine.Avatar).ToString();
		typeof(UnityEngine.HumanDescription).ToString();
		typeof(UnityEngine.RuntimeAnimatorController).ToString();

		// 音频系统
		typeof(UnityEngine.AudioSource).ToString();
		typeof(UnityEngine.AudioListener).ToString();
		typeof(UnityEngine.AudioClip).ToString();
		typeof(UnityEngine.AudioReverbZone).ToString();
		typeof(UnityEngine.AudioLowPassFilter).ToString();
		typeof(UnityEngine.AudioHighPassFilter).ToString();
		typeof(UnityEngine.AudioEchoFilter).ToString();
		typeof(UnityEngine.AudioDistortionFilter).ToString();
		typeof(UnityEngine.AudioReverbFilter).ToString();
		typeof(UnityEngine.AudioChorusFilter).ToString();

		// 光照系统
		typeof(UnityEngine.Light).ToString();
		typeof(UnityEngine.ReflectionProbe).ToString();
		typeof(UnityEngine.LightProbeGroup).ToString();
		typeof(UnityEngine.LightProbes).ToString();

		// 材质和着色器
		typeof(UnityEngine.Material).ToString();
		typeof(UnityEngine.Shader).ToString();
		typeof(UnityEngine.Texture).ToString();
		typeof(UnityEngine.Texture2D).ToString();
		typeof(UnityEngine.Texture3D).ToString();
		typeof(UnityEngine.Cubemap).ToString();
		typeof(UnityEngine.RenderTexture).ToString();
		typeof(UnityEngine.Sprite).ToString();

		// 粒子系统
		typeof(UnityEngine.ParticleSystem).ToString();

		// UI 系统
		typeof(UnityEngine.Canvas).ToString();
		typeof(UnityEngine.CanvasRenderer).ToString();
		typeof(UnityEngine.UI.Graphic).ToString();
		typeof(UnityEngine.UI.Image).ToString();
		typeof(UnityEngine.UI.Text).ToString();
		typeof(UnityEngine.UI.RawImage).ToString();
		typeof(UnityEngine.UI.Mask).ToString();
		typeof(UnityEngine.UI.RectMask2D).ToString();
		typeof(UnityEngine.UI.Button).ToString();
		typeof(UnityEngine.UI.Toggle).ToString();
		typeof(UnityEngine.UI.Slider).ToString();
		typeof(UnityEngine.UI.Scrollbar).ToString();
		typeof(UnityEngine.UI.Dropdown).ToString();
		typeof(UnityEngine.UI.InputField).ToString();
		typeof(UnityEngine.UI.ScrollRect).ToString();
		typeof(UnityEngine.UI.Selectable).ToString();
		typeof(UnityEngine.UI.CanvasScaler).ToString();
		typeof(UnityEngine.UI.ContentSizeFitter).ToString();
		typeof(UnityEngine.UI.AspectRatioFitter).ToString();
		typeof(UnityEngine.UI.HorizontalLayoutGroup).ToString();
		typeof(UnityEngine.UI.VerticalLayoutGroup).ToString();
		typeof(UnityEngine.UI.GridLayoutGroup).ToString();

		// 2D 相关
		typeof(UnityEngine.Tilemaps.Tilemap).ToString();
		typeof(UnityEngine.Tilemaps.TilemapRenderer).ToString();
		typeof(UnityEngine.Tilemaps.Tile).ToString();
		typeof(UnityEngine.Tilemaps.TileBase).ToString();
		typeof(UnityEngine.Collider2D).ToString();
		typeof(UnityEngine.BoxCollider2D).ToString();
		typeof(UnityEngine.CircleCollider2D).ToString();
		typeof(UnityEngine.CapsuleCollider2D).ToString();
		typeof(UnityEngine.PolygonCollider2D).ToString();
		typeof(UnityEngine.EdgeCollider2D).ToString();
		typeof(UnityEngine.CompositeCollider2D).ToString();
		typeof(UnityEngine.Joint2D).ToString();
		typeof(UnityEngine.SpringJoint2D).ToString();
		typeof(UnityEngine.DistanceJoint2D).ToString();
		typeof(UnityEngine.HingeJoint2D).ToString();
		typeof(UnityEngine.SliderJoint2D).ToString();
		typeof(UnityEngine.WheelJoint2D).ToString();
		typeof(UnityEngine.FixedJoint2D).ToString();
		typeof(UnityEngine.RelativeJoint2D).ToString();
		typeof(UnityEngine.FrictionJoint2D).ToString();
		typeof(UnityEngine.TargetJoint2D).ToString();
		typeof(UnityEngine.AreaEffector2D).ToString();
		typeof(UnityEngine.BuoyancyEffector2D).ToString();
		typeof(UnityEngine.PointEffector2D).ToString();
		typeof(UnityEngine.PlatformEffector2D).ToString();
		typeof(UnityEngine.SurfaceEffector2D).ToString();

		// 地形系统
		typeof(UnityEngine.Terrain).ToString();
		typeof(UnityEngine.TerrainData).ToString();
		typeof(UnityEngine.Tree).ToString();
		typeof(UnityEngine.DetailPrototype).ToString();

		// 视频和电影
		typeof(UnityEngine.Video.VideoPlayer).ToString();
		typeof(UnityEngine.Video.VideoClip).ToString();

		// 导航和寻路
		typeof(UnityEngine.AI.NavMeshAgent).ToString();
		typeof(UnityEngine.AI.NavMeshObstacle).ToString();
		typeof(UnityEngine.AI.NavMesh).ToString();
		typeof(UnityEngine.AI.NavMeshData).ToString();

		// VR/AR 相关
		//typeof(UnityEngine.XR.XRDevice).ToString();
		//typeof(UnityEngine.XR.InputTracking).ToString();
		//typeof(UnityEngine.XR.XRInputSubsystem).ToString();
		//typeof(UnityEngine.XR.WSA.WorldManager).ToString();

		// 输入系统
		typeof(UnityEngine.Input).ToString();
		typeof(UnityEngine.KeyCode).ToString();
		typeof(UnityEngine.Touch).ToString();
		typeof(UnityEngine.AccelerationEvent).ToString();
		typeof(UnityEngine.Gyroscope).ToString();

		// 数学和几何
		typeof(UnityEngine.Vector2).ToString();
		typeof(UnityEngine.Vector3).ToString();
		typeof(UnityEngine.Vector4).ToString();
		typeof(UnityEngine.Quaternion).ToString();
		typeof(UnityEngine.Matrix4x4).ToString();
		typeof(UnityEngine.Color).ToString();
		typeof(UnityEngine.Rect).ToString();
		typeof(UnityEngine.Ray).ToString();
		typeof(UnityEngine.RaycastHit).ToString();
		typeof(UnityEngine.RaycastHit2D).ToString();
		typeof(UnityEngine.Bounds).ToString();
		typeof(UnityEngine.Plane).ToString();
		typeof(UnityEngine.Mathf).ToString();
		typeof(UnityEngine.Random).ToString();

		// 系统和工具
		typeof(UnityEngine.Time).ToString();
		typeof(UnityEngine.SystemInfo).ToString();
		typeof(UnityEngine.Screen).ToString();
		typeof(UnityEngine.Application).ToString();
		typeof(UnityEngine.Resources).ToString();
		typeof(UnityEngine.SceneManagement.SceneManager).ToString();
		typeof(UnityEngine.Debug).ToString();
		typeof(UnityEngine.QualitySettings).ToString();
		typeof(UnityEngine.RenderSettings).ToString();
		typeof(UnityEngine.GL).ToString();
		typeof(UnityEngine.Graphics).ToString();
		typeof(UnityEngine.PlayerPrefs).ToString();

		// 网络
		typeof(UnityEngine.Networking.UnityWebRequest).ToString();
		typeof(UnityEngine.Networking.DownloadHandler).ToString();
		typeof(UnityEngine.Networking.UploadHandler).ToString();
	}
}
