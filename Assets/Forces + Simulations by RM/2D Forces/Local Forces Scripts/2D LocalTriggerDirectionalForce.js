@script AddComponentMenu ("Forces 2D/Local/Trigger Directional Force")

var Force: Transform;
var ForcePower: float = 1;
var TriggerDrag: float = 0;
var SpaceDrag: float = 0;


function OnTriggerStay2D(other: Collider2D) {

var MyObjectsForce : Vector2  = Force.transform.TransformDirection (0, 1, 0); 

gameObject.rigidbody2D.AddForce (MyObjectsForce *ForcePower);

gameObject.rigidbody2D.drag = TriggerDrag;
}


function OnTriggerExit2D(other: Collider2D) {

//GameObjects
gameObject.rigidbody2D.drag = SpaceDrag;

}

//DRAW GIZMOS

function OnDrawGizmos () {
		// Draws a 5 meter long red line in front of the object
		Gizmos.color = Color.red;
		var direction : Vector3 = Force.transform.TransformDirection (Vector3.up) * 5;
		Gizmos.DrawRay (transform.position, direction);
	}