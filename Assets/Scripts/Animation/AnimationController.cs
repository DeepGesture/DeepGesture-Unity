using UnityEngine;
using AI4Animation;

[RequireComponent(typeof(Actor))]
public abstract class AnimationController : MonoBehaviour {

	public Actor Actor;
	public float Framerate = 60f;

	protected abstract void Setup();
	protected abstract void Destroy();
	protected abstract void Control();
	protected abstract void OnGUIDerived();
	protected abstract void OnRenderObjectDerived();

	void Reset() {
		// Debug.Log("Reset");
		Actor = GetComponent<Actor>();
	}

    void Start() {
	    Debug.Log("Start" + Framerate);
		Time.fixedDeltaTime = 1f/Framerate;
		Utility.SetFPS(Mathf.RoundToInt(Framerate));
		Setup();
    }

	void OnDestroy() {
		// Debug.Log("OnDestroy");
		Destroy();
	}

	void FixedUpdate() {
		// Debug.Log("FixedUpdate");
		Control();
	}

    void OnGUI() {
		OnGUIDerived();
    }

	void OnRenderObject() {
		OnRenderObjectDerived();
	}
	
}