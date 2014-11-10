using System;
using UnityEngine;

public abstract class ImageSource : MonoBehaviour 
{
	public ConcurrentStack<UnityEngine.Color32[]>  BufferStack { get; protected set;}

	public abstract int Width {
		get; protected set;
	}
	
	public abstract int Height {
		get;protected set;
	}

	public abstract void StartCapture ();
	
	public abstract void StopCapture ();

}


