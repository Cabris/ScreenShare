@script AddComponentMenu ("Forces 2D/Local/OnTrigger Directional Force")


var ForcePower: float = 1;
var TriggerDrag: float = 0;
var SpaceDrag: float = 0;


function OnTriggerStay2D(other: Collider2D) {

var MyObjectsForce : Vector2  = gameObject.transform.TransformDirection (0, 1, 0); 

other.rigidbody2D.AddForce (MyObjectsForce *ForcePower);

other.rigidbody2D.drag = TriggerDrag;
}


function OnTriggerExit2D(other: Collider2D) {

//GameObjects
other.rigidbody2D.drag = SpaceDrag;

}

//DRAW GIZMOS

function OnDrawGizmosSelected () {
		// Draws a 5 meter long red line in front of the object
		Gizmos.color = Color.red;
		var direction : Vector3 = gameObject.transform.TransformDirection (Vector3.up) * 10;
		Gizmos.DrawRay (transform.position, direction);
	}