using UnityEngine;
using UnityEditor;
using OpenHuman;
using System;
using UnityEngine.AI;

#if UNITY_EDITOR
public class VisualizeChannel : EditorWindow
{

	public static EditorWindow Window;
	public static Vector2 Scroll;

	// ~~~~~~~~~~~~~~~~~~~~~~~~
	public string Tag = "8Channels";
	public bool Mirror = false;
	public bool UseOffsets = false;
	public bool ShowParameters = false;
	public bool ShowNormalized = false;
	public bool DrawWindowPoses = false;
	public bool DrawPhaseSpace = true;
	public bool DrawPivot = true;
	public float LineHeight = 50f;
	public float TargetFramerate = 60f;

	// ~~~~~~~~~~~~~~~~~~~~~~~~

	private static MotionEditor editor = null;
	private static MotionAsset asset = null;
	private static DeepPhaseModule module = null;
	// private bool isLoad = false;
	private DeepPhaseModule.Channel[] channels = null;


	// ~~~~~~~~~~~~~~~~~~~~~~~~
	public static float Timestamp = 0f;
	public static int FrameIndex = 1;
	public static float Zoom = 0.02f;
	public static float totalTime = 0f;
	public static float frameRate = 60f;
	public static string assetName = string.Empty;
	


	public string Type = string.Empty;

	[MenuItem("OpenHuman/Visualize/Visualize Channel")]
	static void Init()
	{
		Window = EditorWindow.GetWindow(typeof(VisualizeChannel));
		Scroll = Vector3.zero;

		editor = GameObjectExtensions.Find<MotionEditor>(true);
		asset = editor.GetSession().Asset;

		assetName = asset.name;
	}

	public void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnGUI()
	{
		//
		// GUILayout.EndVertical();

		GUILayout.Space(20f);
		if (GUILayout.Button("Load Channel Asset"))
		{
			editor = GameObjectExtensions.Find<MotionEditor>(true);
			asset = editor.GetSession().Asset;
			totalTime = asset.GetTotalTime();
			module = asset.GetModule<DeepPhaseModule>(Tag);
			channels = module.Channels;
			assetName = asset.name;
		}
		GUILayout.FlexibleSpace();

		if (editor == null) { return; }
		if (asset == null) { return; }
		if (module == null) { return; }
		if (channels == null) { return; }

		Scroll = EditorGUILayout.BeginScrollView(Scroll);
		float height = 100f;

		using (new GUILayout.VerticalScope("Box"))
		{
			Zoom = EditorGUILayout.Slider("Zoom", Zoom, 0f, 1f);

			FrameIndex = EditorGUILayout.IntSlider("FrameIndex", FrameIndex, 1, asset.Frames.Length - 1);
			Timestamp = FrameIndex / frameRate;
			EditorGUILayout.HelpBox("Timestamp: " + Timestamp, MessageType.None, true);

			GUILayout.Label("Visualize Channel", EditorStyles.boldLabel);

			assetName = EditorGUILayout.TextField("Asset name", assetName);
			Tag = EditorGUILayout.TextField("Tag", Tag);
			UseOffsets = EditorGUILayout.Toggle("Use Offset", UseOffsets);
			ShowParameters = EditorGUILayout.Toggle("Show Parameters", ShowParameters);
			ShowNormalized = EditorGUILayout.Toggle("Show Normalized", ShowNormalized);
			DrawWindowPoses = EditorGUILayout.Toggle("Draw Window Poses", DrawWindowPoses);
			DrawPhaseSpace = EditorGUILayout.Toggle("Draw Phase Space", DrawPhaseSpace);
			LineHeight = EditorGUILayout.Slider("Line Height", LineHeight, 10f, 100f);
			GUILayout.Space(10f);

			Vector3Int view = GetView();

			float min = -1f;
			float max = 1f;
			float maxAmplitude = 1f;

			if (ShowNormalized)
			{
				maxAmplitude = 0f;
				foreach (DeepPhaseModule.Channel c in channels)
				{
					maxAmplitude = Mathf.Max(maxAmplitude, (Mirror ? c.MirroredAmplitudes : c.RegularAmplitudes).Max());
				}
			}

			for (int i = 0; i < channels.Length; i++)
			{
				DeepPhaseModule.Channel c = channels[i];
				EditorGUILayout.Space(50f);
				EditorGUILayout.LabelField("Channel: " + i, EditorStyles.miniLabel);
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.BeginVertical(GUILayout.Height(height));
				Rect ctrl = EditorGUILayout.GetControlRect();
				Rect rect = new Rect(ctrl.x, ctrl.y, ctrl.width, height);
				EditorGUI.DrawRect(rect, UltiDraw.Black);

				// Zero
				{
					float prevX = rect.xMin;
					float prevY = rect.yMax - (0f).Normalize(min, max, 0f, 1f) * LineHeight;
					float newX = rect.xMin + rect.width;
					float newY = rect.yMax - (0f).Normalize(min, max, 0f, 1f) * LineHeight;
					UltiDraw.Begin();
					UltiDraw.DrawLine(new Vector3(prevX, prevY), new Vector3(newX, newY), UltiDraw.White.Opacity(0.5f));
					UltiDraw.End();
				}


				// Phase 1D (Phase Values)
				for (int j = 0; j < view.z; j++)
				{
					float prevX = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
					float prevY = rect.yMax;
					float newX = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
					float newY = rect.yMax - c.GetPhaseValue(asset.GetFrame(view.x + j).Timestamp, Mirror) * LineHeight;
					float weight = c.GetAmplitude(asset.GetFrame(view.x + j).Timestamp, Mirror).Normalize(0f, maxAmplitude, 0f, 1f);
					UltiDraw.Begin();
					UltiDraw.DrawLine(new Vector3(prevX, prevY), new Vector3(newX, newY), UltiDraw.Cyan.Opacity(weight));
					UltiDraw.End();
				}

				// Phase 2D X (Sinusoidal)
				for (int j = 1; j < view.z; j++)
				{
					float prevX = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
					float prevY = rect.yMax - (float)c.GetManifoldVector(asset.GetFrame(view.x + j - 1).Timestamp, Mirror).x.Normalize(-1f, 1f, 0f, 1f) * LineHeight;
					float newX = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
					float newY = rect.yMax - (float)c.GetManifoldVector(asset.GetFrame(view.x + j).Timestamp, Mirror).x.Normalize(-1f, 1f, 0f, 1f) * LineHeight;
					float weight = c.GetAmplitude(asset.GetFrame(view.x + j).Timestamp, Mirror).Normalize(0f, maxAmplitude, 0f, 1f);
					UltiDraw.Begin();
					// UltiDraw.DrawLine(new Vector3(prevX, prevY), new Vector3(newX, newY), UltiDraw.Red.Opacity(weight));
					UltiDraw.DrawLine(new Vector3(prevX, prevY), new Vector3(newX, newY), UltiDraw.Orange.Opacity(weight));
					UltiDraw.End();
				}
				// Phase 2D Y (Cosine)
				for (int j = 1; j < view.z; j++)
				{
					float prevX = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
					float prevY = rect.yMax - (float)c.GetManifoldVector(asset.GetFrame(view.x + j - 1).Timestamp, Mirror).y.Normalize(-1f, 1f, 0f, 1f) * LineHeight;
					float newX = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
					float newY = rect.yMax - (float)c.GetManifoldVector(asset.GetFrame(view.x + j).Timestamp, Mirror).y.Normalize(-1f, 1f, 0f, 1f) * LineHeight;
					float weight = c.GetAmplitude(asset.GetFrame(view.x + j).Timestamp, Mirror).Normalize(0f, maxAmplitude, 0f, 1f);
					UltiDraw.Begin();
					UltiDraw.DrawLine(new Vector3(prevX, prevY), new Vector3(newX, newY), UltiDraw.Magenta.Opacity(weight));
					// UltiDraw.DrawLine(new Vector3(prevX, prevY), new Vector3(newX, newY), UltiDraw.Blue.Opacity(weight));
					UltiDraw.End();
				}

				DrawPivotRect(rect);
				DrawRect(
					asset.GetFrame(Timestamp - 1f / c.GetFrequency(Timestamp, Mirror) / 2f).Index,
					asset.GetFrame(Timestamp + 1f / c.GetFrequency(Timestamp, Mirror) / 2f).Index,
					1f,
					Color.green.Opacity(0.25f),
					rect
				);

				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			// EditorGUILayout.HelpBox(Channels[i].GetFrequency(Timestamp, Mirror).ToString(), MessageType.None, true);
		}

		using (new GUILayout.VerticalScope("Box"))
		{
			Vector3Int view = GetView();
			float maxOffset = 1f;
			float maxAmplitude = 1f;
			float maxFrequency = editor.GetTimeSeries().MaximumFrequency;
			if (ShowNormalized)
			{
				maxAmplitude = 0f;
				foreach (DeepPhaseModule.Channel c in channels)
				{
					maxAmplitude = Mathf.Max(maxAmplitude, (Mirror ? c.MirroredAmplitudes : c.RegularAmplitudes).Max());
				}
			}


			EditorGUILayout.LabelField("Channels", EditorStyles.boldLabel);

			for (int i = 0; i < channels.Length; i++)
			{
				DeepPhaseModule.Channel c = channels[i];
				EditorGUILayout.LabelField("Channels " + (i + 1), EditorStyles.miniLabel);
				EditorGUILayout.HelpBox("Frequency: " + c.GetFrequency(Timestamp, Mirror).ToString("F3") + " / " + "Delta: " + (TargetFramerate * c.GetDelta(Timestamp, Mirror, 1f / TargetFramerate)).ToString("F3"), MessageType.None);
			}

			EditorGUILayout.Space(50f);

			EditorGUILayout.LabelField("Amplitude (" + channels.Length + " channel)", EditorStyles.boldLabel);

			{
				EditorGUILayout.BeginHorizontal();



				EditorGUILayout.BeginVertical(GUILayout.Height(height));
				Rect ctrl = EditorGUILayout.GetControlRect();
				Rect rect = new Rect(ctrl.x, ctrl.y, ctrl.width, height);
				EditorGUI.DrawRect(rect, UltiDraw.Black);

				UltiDraw.Begin();

				Vector3 prevPos = Vector3.zero;
				Vector3 newPos = Vector3.zero;

				for (int i = 0; i < channels.Length; i++)
				{
					DeepPhaseModule.Channel c = channels[i];
					for (int j = 1; j < view.z; j++)
					{
						prevPos.x = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
						prevPos.y = rect.yMax - (float)c.GetAmplitude(asset.GetFrame(view.x + j - 1).Timestamp, Mirror).Normalize(0f, maxAmplitude, 0f, 1f) * rect.height;
						newPos.x = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
						newPos.y = rect.yMax - (float)c.GetAmplitude(asset.GetFrame(view.x + j).Timestamp, Mirror).Normalize(0f, maxAmplitude, 0f, 1f) * rect.height;
						UltiDraw.DrawLine(prevPos, newPos, UltiDraw.GetRainbowColor(i, channels.Length));
					}
				}

				UltiDraw.End();

				DrawPivotRect(rect);

				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space(50f);

			EditorGUILayout.LabelField("Frequency (" + channels.Length + " channel)", EditorStyles.boldLabel);

			{
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.BeginVertical(GUILayout.Height(height));
				Rect ctrl = EditorGUILayout.GetControlRect();
				Rect rect = new Rect(ctrl.x, ctrl.y, ctrl.width, height);
				EditorGUI.DrawRect(rect, UltiDraw.Black);

				UltiDraw.Begin();

				Vector3 prevPos = Vector3.zero;
				Vector3 newPos = Vector3.zero;

				for (int i = 0; i < channels.Length; i++)
				{
					DeepPhaseModule.Channel c = channels[i];
					for (int j = 1; j < view.z; j++)
					{
						prevPos.x = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
						prevPos.y = rect.yMax - (float)c.GetFrequency(asset.GetFrame(view.x + j - 1).Timestamp, Mirror).Normalize(0f, maxFrequency, 0f, 1f) * rect.height;
						newPos.x = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
						newPos.y = rect.yMax - (float)c.GetFrequency(asset.GetFrame(view.x + j).Timestamp, Mirror).Normalize(0f, maxFrequency, 0f, 1f) * rect.height;
						UltiDraw.DrawLine(prevPos, newPos, UltiDraw.GetRainbowColor(i, channels.Length));
					}
				}

				UltiDraw.End();

				DrawPivotRect(rect);

				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space(50f);

			EditorGUILayout.LabelField("Offset (" + channels.Length + " channel)", EditorStyles.boldLabel);

			{
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.BeginVertical(GUILayout.Height(height));
				Rect ctrl = EditorGUILayout.GetControlRect();
				Rect rect = new Rect(ctrl.x, ctrl.y, ctrl.width, height);
				EditorGUI.DrawRect(rect, UltiDraw.Black);

				UltiDraw.Begin();

				Vector3 prevPos = Vector3.zero;
				Vector3 newPos = Vector3.zero;

				for (int i = 0; i < channels.Length; i++)
				{
					DeepPhaseModule.Channel c = channels[i];
					for (int j = 1; j < view.z; j++)
					{
						prevPos.x = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
						prevPos.y = rect.yMax - (float)c.GetOffset(asset.GetFrame(view.x + j - 1).Timestamp, Mirror).Normalize(-maxOffset, maxOffset, 0f, 1f) * rect.height;
						newPos.x = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
						newPos.y = rect.yMax - (float)c.GetOffset(asset.GetFrame(view.x + j).Timestamp, Mirror).Normalize(-maxOffset, maxOffset, 0f, 1f) * rect.height;
						UltiDraw.DrawLine(prevPos, newPos, UltiDraw.GetRainbowColor(i, channels.Length));
					}
				}

				UltiDraw.End();

				DrawPivotRect(rect);

				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}
		}


		EditorGUILayout.EndScrollView();
	}


	public static void DrawPivotRect(Rect rect)
	{
		float PastWindow = 1f;
		float FutureWindow = 1f;

		Frame pastFrame = asset.GetFrame(Mathf.Clamp(Timestamp - PastWindow, 0f, asset.GetTotalTime()));
		Frame futureFrame = asset.GetFrame(Mathf.Clamp(Timestamp + FutureWindow, 0f, asset.GetTotalTime()));
		DrawRect(
			pastFrame.Index,
			futureFrame.Index,
			1f,
			UltiDraw.White.Opacity(0.1f),
			rect
		);
		Vector3 view = GetView();
		Vector3 top = new Vector3(rect.xMin + (float)(FrameIndex - view.x) / (view.z - 1) * rect.width, rect.yMax - rect.height, 0f);
		Vector3 bottom = new Vector3(rect.xMin + (float)(FrameIndex - view.x) / (view.z - 1) * rect.width, rect.yMax, 0f);
		UltiDraw.Begin();
		UltiDraw.DrawLine(top, bottom, UltiDraw.Yellow);
		UltiDraw.End();
	}

	public static void DrawRect(int start, int end, float thickness, Color color, Rect rect)
	{
		Vector3 view = GetView();
		float _start = (float)(Mathf.Clamp(start, view.x, view.y) - view.x) / (view.z - 1);
		float _end = (float)(Mathf.Clamp(end, view.x, view.y) - view.x) / (view.z - 1);
		float left = rect.x + _start * rect.width;
		float right = rect.x + _end * rect.width;
		Vector3 a = new Vector3(left, rect.y, 0f);
		Vector3 b = new Vector3(right, rect.y, 0f);
		Vector3 c = new Vector3(left, rect.y + rect.height, 0f);
		Vector3 d = new Vector3(right, rect.y + rect.height, 0f);
		UltiDraw.Begin();
		UltiDraw.DrawTriangle(a, c, b, color);
		UltiDraw.DrawTriangle(b, c, d, color);
		UltiDraw.End();
	}

	public static Vector3Int GetView()
	{
		float window = Zoom * totalTime;
		float startTime = Timestamp - window / 2f;
		float endTime = Timestamp + window / 2f;
		if (startTime < 0f)
		{
			endTime -= startTime;
			startTime = 0f;
		}
		if (endTime > totalTime)
		{
			startTime -= endTime - totalTime;
			endTime = totalTime;
		}
		int start = asset.GetFrame(Mathf.Max(0f, startTime)).Index;
		int end = asset.GetFrame(Mathf.Min(totalTime, endTime)).Index;
		int elements = end - start + 1;
		return new Vector3Int(start, end, elements);
	}



}
#endif