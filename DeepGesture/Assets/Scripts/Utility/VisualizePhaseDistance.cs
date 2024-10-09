using UnityEngine;
using UnityEditor;
using OpenHuman;
using System;
using UnityEngine.AI;
using System.Collections.Generic;

#if UNITY_EDITOR
public class VisualizePhaseDistance : EditorWindow
{

	public static EditorWindow Window;
	public static Vector2 Scroll;

	//  ~~~~~~~~~~~~~~~~~~~~~~~~
	public bool Mirror = false;
	public float LineHeight = 100f;
	public float TargetFramerate = 60f;

	//  ~~~~~~~~~~~~~~~~~~~~~~~~

	private static MotionEditor editor = null;
	private static MotionAsset asset = null;
	private static DeepPhaseModule.DistanceSeries distances = null;
	private static DeepPhaseModule module = null;

	//  ~~~~~~~~~~~~~~~~~~~~~~~~
	public static float Timestamp = 0f;
	public static int FrameIndex = 1;
	public static float Zoom = 0.2f;
	public static float totalTime = 0f;
	public static float frameRate = 60f;
	public static string assetName = string.Empty;
	public static float height = 50f;

	public string Tag = "8Channels";

	[MenuItem("OpenHuman/Visualize/Visualize Phase Distance")]
	static void Init()
	{
		Window = EditorWindow.GetWindow(typeof(VisualizePhaseDistance));
		Scroll = Vector3.zero;
	}

	public void OnInspectorUpdate()
	{
		Repaint();
	}

	void DrawChannel(DeepPhaseModule.DistanceSeries.Map channel, Color color, Vector3Int view)
	{
		EditorGUILayout.BeginVertical(GUILayout.Height(height));
		Rect ctrl = EditorGUILayout.GetControlRect();
		Rect rect = new Rect(ctrl.x, ctrl.y, ctrl.width, height);
		EditorGUI.DrawRect(rect, UltiDraw.Black);
		UltiDraw.Begin();

		int pivot = editor.GetCurrentFrame().Index - 1;
		for (int j = 1; j < view.z; j++)
		{
			float prevx = rect.xMin + (float)(j - 1) / (view.z - 1) * rect.width;
			float prevy = rect.yMax - (float)channel.Values[pivot][view.x + j - 1 - 1].Normalize(channel.Min, channel.Max, 0f, 1f) * rect.height;
			float newx = rect.xMin + (float)(j) / (view.z - 1) * rect.width;
			float newy = rect.yMax - (float)channel.Values[pivot][view.x + j - 1].Normalize(channel.Min, channel.Max, 0f, 1f) * rect.height;
			UltiDraw.DrawLine(new Vector3(prevx, prevy), new Vector3(newx, newy), color);
		}

		UltiDraw.End();

		DrawPivotRect(rect);

		EditorGUILayout.EndVertical();
	}

	void OnGUI()
	{
		GUILayout.Space(20f);
		if (GUILayout.Button("Load Phase Distance"))
		{
			editor = GameObjectExtensions.Find<MotionEditor>(true);
			asset = editor.GetSession().Asset;
			totalTime = asset.GetTotalTime();
			assetName = asset.name;

			Actor actor = editor.GetSession().GetActor();
			TimeSeries timeSeries = editor.GetTimeSeries();
			module = asset.GetModule<DeepPhaseModule>(Tag);
			distances = new DeepPhaseModule.DistanceSeries(asset, actor, timeSeries, module);
		}
		GUILayout.FlexibleSpace();

		if (editor == null) { return; }
		if (asset == null) { return; }
		if (distances == null) { return; }

		Scroll = EditorGUILayout.BeginScrollView(Scroll);

		using (new GUILayout.VerticalScope("Box"))
		{
			Zoom = EditorGUILayout.Slider("Zoom", Zoom, 0f, 1f);

			FrameIndex = EditorGUILayout.IntSlider("FrameIndex", FrameIndex, 1, asset.Frames.Length - 1);
			Timestamp = FrameIndex / frameRate;
			EditorGUILayout.HelpBox("Timestamp: " + Timestamp, MessageType.None, true);

			GUILayout.Label("Visualize Bones", EditorStyles.boldLabel);

			assetName = EditorGUILayout.TextField("Asset name", assetName);
			Tag = EditorGUILayout.TextField("Tag", Tag);
			LineHeight = EditorGUILayout.Slider("Line Height", LineHeight, 10f, 200f);
			GUILayout.Space(10f);

			Vector3Int view = GetView();

			//  ~~~~~~~~~~~~~~~~~~~~~~~~~
			using (new GUILayout.VerticalScope("Box"))
			{
				GUILayout.Space(20f);
				DrawChannel(distances.GlobalChannel, Color.cyan, view);
				GUILayout.Space(10f);

				foreach (DeepPhaseModule.DistanceSeries.Map channel in distances.LocalChannels)
				{
					GUILayout.Space(10f);
					DrawChannel(channel, Color.white, view);
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