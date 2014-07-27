using System.Collections;
using System;

namespace Phoni {
namespace PhoniCore {

	[System.AttributeUsage(System.AttributeTargets.Property)]
	public class PhoniDataAttribute : System.Attribute {
		public SendingCase sendingCase;
		public PhoniDataCode code;
		
		public PhoniDataAttribute(PhoniDataCode code) : this(code, SendingCase.SendToGame) {
		}
			
		public PhoniDataAttribute(PhoniDataCode code, SendingCase sendingCase) {
			this.sendingCase = sendingCase;
			this.code = code;
		}
	}
		
	public enum SendingCase {
		SendToNone, //does not add to sendingCode
		SendToGame, // add to sendingCode if the PhoniDataPort is of a player and connected to a game
		SendToPlayer, // add to sendingCode if the PhoniDataPort is of a game and connected to a player
		SendToBoth, // add to sendingCode anyway
	}
	
} // namespace PhoniCore
} // namespace Phoni
