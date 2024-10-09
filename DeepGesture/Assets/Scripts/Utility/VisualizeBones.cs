using UnityEngine;
using UnityEditor;
using OpenHuman;
using System;
using UnityEngine.AI;
using System.Collections.Generic;

#if UNITY_EDITOR
public class VisualizeBones : EditorWindow
{

	public static EditorWindow Window;
	public static Vector2 Scroll;

	//  ~~~~~~~~~~~~~~~~~~~~~~~~
	public bool Mirror = false;
	public bool DrawPivot = true;
	public float LineHeight = 100f;
	public float TargetFramerate = 60f;

	//  ~~~~~~~~~~~~~~~~~~~~~~~~

	private static MotionEditor editor = null;
	private static MotionAsset asset = null;
	private static DeepPhaseModule.CurveSeries curves = null;
	private static string[] boneNames = null;


	//  ~~~~~~~~~~~~~~~~~~~~~~~~
	public static float Timestamp = 0f;
	public static int FrameIndex = 1;
	public static float Zoom = 0.2f;
	public static float totalTime = 0f;
	public static float frameRate = 60f;
	public static string assetName = string.Empty;

	public float MinView = -2f;
	public float MaxView = 2f;

	[MenuItem("OpenHuman/Visualize/Visualize Bones")]
	static void Init()
	{
		Window = EditorWindow.GetWindow(typeof(VisualizeBones));
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
		GUILayout.Space(20f);
		if (GUILayout.Button("Load Bones"))
		{
			editor = GameObjectExtensions.Find<MotionEditor>(true);
			asset = editor.GetSession().Asset;
			totalTime = asset.GetTotalTime();
			assetName = asset.name;

			Actor actor = editor.GetSession().GetActor();
			boneNames = actor.GetBoneNames();
			TimeSeries timeSeries = editor.GetTimeSeries();
			curves = new DeepPhaseModule.CurveSeries(asset, actor, timeSeries);
		}
		GUILayout.FlexibleSpace();

		if (editor == null) { return; }
		if (asset == null) { return; }
		if (curves == null) { return; }
		if (boneNames == null) { return; }

		Scroll = EditorGUILayout.BeginScrollView(Scroll);
		float height = 100f;

		using (new GUILayout.VerticalScope("Box"))
		{
			Zoom = EditorGUILayout.Slider("Zoom", Zoom, 0f, 1f);

			FrameIndex = EditorGUILayout.IntSlider("FrameIndex", FrameIndex, 1, asset.Frames.Length - 1);
			Timestamp = FrameIndex / frameRate;
			EditorGUILayout.HelpBox("Timestamp: " + Timestamp, MessageType.None, true);

			GUILayout.Label("Visualize Bones", EditorStyles.boldLabel);

			assetName = EditorGUILayout.TextField("Asset name", assetName);
			LineHeight = EditorGUILayout.Slider("Line Height", LineHeight, 10f, 200f);
			GUILayout.Space(10f);

			Vector3Int view = GetView();

			//  ~~~~~~~~~~~~~~~~~~~~~~~~~
			using (new EditorGUILayout.VerticalScope("Box"))
			{
				curves.MinView = EditorGUILayout.FloatField("Min View", curves.MinView);
				curves.MaxView = EditorGUILayout.FloatField("Max View", curves.MaxView);

				EditorGUILayout.HelpBox("Curves " + curves.Curves.Length, MessageType.None);
				TimeSeries timeSeries = editor.GetTimeSeries();
				Actor actor = editor.GetSession().GetActor();

				for (int i = 0; i < curves.Curves.Length; i++)
				{
					EditorGUILayout.Space(5f);
					GUILayout.Label("Bones: " + boneNames[i], EditorStyles.miniLabel);

					EditorGUILayout.BeginVertical(GUILayout.Height(height));
					Rect ctrl = EditorGUILayout.GetControlRect();
					Rect rect = new Rect(ctrl.x, ctrl.y, ctrl.width, height);
					EditorGUI.DrawRect(rect, UltiDraw.Black);
					UltiDraw.Begin();

					// Zero
					{
						float prevx = rect.xMin;
						float prevy = rect.yMax - (0f).Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						float newx = rect.xMin + rect.width;
						float newy = rect.yMax - (0f).Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						UltiDraw.DrawLine(new Vector3(prevx, prevy), new Vector3(newx, newy), UltiDraw.Magenta.Opacity(0.5f));
					}

					// Values
					Vector3[] values = curves.Curves[i].GetValues(editor.Mirror);
					for (int j = 1; j < view.z; j++)
					{
						float prevx = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
						float prevy = rect.yMax - (float)values[view.x + j - 1 - 1].x.Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						float newx = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
						float newy = rect.yMax - (float)values[view.x + j - 1].x.Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						UltiDraw.DrawLine(new Vector3(prevx, prevy), new Vector3(newx, newy), UltiDraw.Red);
					}
					for (int j = 1; j < view.z; j++)
					{
						float prevx = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
						float prevy = rect.yMax - (float)values[view.x + j - 1 - 1].y.Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						float newx = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
						float newy = rect.yMax - (float)values[view.x + j - 1].y.Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						UltiDraw.DrawLine(new Vector3(prevx, prevy), new Vector3(newx, newy), UltiDraw.Green);
					}
					for (int j = 1; j < view.z; j++)
					{
						float prevx = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
						float prevy = rect.yMax - (float)values[view.x + j - 1 - 1].z.Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						float newx = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
						float newy = rect.yMax - (float)values[view.x + j - 1].z.Normalize(curves.MinView, curves.MaxView, 0f, 1f) * LineHeight;
						UltiDraw.DrawLine(new Vector3(prevx, prevy), new Vector3(newx, newy), UltiDraw.Blue);
					}
					UltiDraw.End();

					DrawPivotRect(rect);

					EditorGUILayout.EndVertical();
				}
			}
		}

		EditorGUILayout.EndScrollView();
	}


	public static void DrawPivotRect(Rect rect)
	{
		float PastWindow = 1f;
		float FutureWindow = 1f;

		Frame pastFrame = asset.GetFrame(Mathf.Clamp(Timestamp - PastWindow, 0f, totalTime));
		Frame futureFrame = asset.GetFrame(Mathf.Clamp(Timestamp + FutureWindow, 0f, totalTime));
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