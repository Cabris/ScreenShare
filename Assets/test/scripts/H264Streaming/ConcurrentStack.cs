using System;
using System.Collections.Generic;

public class ConcurrentStack<tvalue>{
	private readonly object syncLock;
	private Stack< tvalue> stk;
	
//	public tvalue this[int key] {
//		get { lock (syncLock) { return dict[key]; } }
//		set { lock (syncLock) { dict[key] = value; } }
//	}
	
	public int Count
	{
		get
		{
			lock(syncLock)
			{
				return stk.Count;
			}
		}
	}
	
	//public bool ContainsKey(tkey item) { lock(syncLock) { return dict.ContainsKey(item); } }
	
	public ConcurrentStack()
	{
		this.stk = new Stack< tvalue>();
		this.syncLock=this;
	}
	
	public void Push( tvalue val)
	{
		lock(syncLock)
		{
			stk.Push( val);
		}
	}
	
	public tvalue Pop()
	{
		lock(syncLock)
		{
			return stk.Pop();
		}
	}
	
	public void Clear()
	{
		lock(syncLock)
		{
			stk.Clear();
		}
	}
	
	//public tkey[] GetKeysArray() { lock(syncLock) { tkey[] result = new tkey[dict.Keys.Count]; dict.Keys.CopyTo(result, 0); return result; } }
}