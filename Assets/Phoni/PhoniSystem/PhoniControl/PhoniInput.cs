using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using Phoni.PhoniCore;

/**
 * 
 * Phoni System
 * a network framework to make standalone device into game controllers.
 * 
 * Developed by Xun "Eric" Zhang (lxjk001@gmail.com)
 * 2013.3.14
 * 
 **/

namespace Phoni {
	
	
	public static class PhoniInput {
			
		private static PhoniDataBook _playerBook = new PhoniDataBook(true);
		public static PhoniDataBook Player {
			get {
				return _playerBook;
			}
		}
		
		private static PhoniDataBook _gameBook = new PhoniDataBook(false);
		public static PhoniDataBook Game {
			get {
				return _gameBook;
			}
		}
		
		public static bool RemovePhoniDataPort(PhoniDataPort port) {
			if(port.IsPlayerPort) {
				return Player.Remove(port.ID);
			}
			else {
				return Game.Remove(port.ID);
			}
		}
		
	}
	
	public delegate void PhoniCommandCallback(PhoniDataPort port, PhoniCommandInfo info);
	
	public class PhoniCommandInfo {
		public PhoniCommandCode command;
		public PhoniDataBase data;
		
		public PhoniCommandInfo(PhoniCommandCode command, PhoniDataBase data) {
			this.command = command;
			this.data = data;
		}
	}

} // namespace Phoni
	







