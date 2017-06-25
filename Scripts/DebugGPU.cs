using UnityEngine;

public class DebugGPU : MonoBehaviour
{
	public enum RenderType
	{
		Normal,
		Mipmap,
		Overdraw
	}

	public Shader mipmapShader;
	public Shader overdrawShader;

	private Camera _cachedCamera;
	private Texture2D _mipColorsTexture;
	private RenderType _renderType = RenderType.Normal;

	void Awake ()
	{
		_cachedCamera = GetComponent<Camera> ();
	}

	void OnEable ()
	{
		UpdateRenderType ();
	}

	void OnDisable()
	{
		_cachedCamera.ResetReplacementShader ();
	}

	void OnGUI()
	{
		if (GUILayout.Button ("Normal")) {
			_renderType = RenderType.Normal;
			UpdateRenderType ();
		}
		if (GUILayout.Button ("Mipmap")) {
			_renderType = RenderType.Mipmap;
			UpdateRenderType ();
		}
		if (GUILayout.Button ("Overdraw")) {
			_renderType = RenderType.Overdraw;
			UpdateRenderType ();
		}
	}

	private void UpdateRenderType()
	{
		switch (_renderType) {
		case RenderType.Normal:
			_cachedCamera.ResetReplacementShader ();
			break;
		case RenderType.Mipmap:
			CreateMipColorsTexture ();
			_cachedCamera.SetReplacementShader (mipmapShader, "RenderType");
			break;
		case RenderType.Overdraw:
			_cachedCamera.SetReplacementShader (overdrawShader, "RenderType");
			break;
		}
	}

	private void CreateMipColorsTexture ()
	{
		if (_mipColorsTexture != null)
			return;

		_mipColorsTexture = new Texture2D (32, 32, TextureFormat.RGBA32, true);
		_mipColorsTexture.hideFlags = HideFlags.HideAndDontSave;
		Color[] colors = new Color[6];
		colors [0] = new Color (0.0f, 0.0f, 1.0f, 0.8f); // 1/4
		colors [1] = new Color (0.0f, 0.5f, 1.0f, 0.4f); // 1/2
		colors [2] = new Color (1.0f, 1.0f, 1.0f, 0.0f); // optimal level
		colors [3] = new Color (1.0f, 0.7f, 0.0f, 0.2f); // x2
		colors [4] = new Color (1.0f, 0.3f, 0.0f, 0.6f); // x4
		colors [5] = new Color (1.0f, 0.0f, 0.0f, 0.8f); // x8
		int mipCount = Mathf.Min (6, _mipColorsTexture.mipmapCount);
		for (int mip = 0; mip < mipCount; ++mip) {
			int width = Mathf.Max (_mipColorsTexture.width >> mip, 1);
			int height = Mathf.Max (_mipColorsTexture.height >> mip, 1);
			Color[] cols = new Color[width * height];
			for (int idx = 0; idx < cols.Length; ++idx) {
				cols [idx] = colors [mip];
			}
			_mipColorsTexture.SetPixels (cols, mip);
		}
		_mipColorsTexture.filterMode = FilterMode.Trilinear;
		_mipColorsTexture.Apply (false);
		Shader.SetGlobalTexture ("_SceneViewMipcolorsTexture", _mipColorsTexture);
	}
}
