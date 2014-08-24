using System;
//using System.Threading;
using System.Timers;

public class TimerTest
{
	public TimerTest ()
	{
		aTimer = new System.Timers.Timer(1000f / 60f);
		aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
		aTimer.AutoReset = true;
		//aTimer.Enabled = true;
	}

	//Timer t;
	System.Timers.Timer aTimer;
	public  void Start()
	{
//		t = new Timer(new TimerCallback(Send2));
//		t.Change(0, 1000 / 60);
		aTimer.Start();
	}
	
	public void Stop()
	{
		aTimer.Stop();
//		t.Change(Timeout.Infinite,Timeout.Infinite);
//		t.Dispose();
	}

	private void OnTimedEvent(object source, ElapsedEventArgs e) 
	{
		UnityEngine.Debug.Log("Hello World!");
	}

	void Send2(object s)
	{
		UnityEngine.Debug.Log("s");
	}


}


