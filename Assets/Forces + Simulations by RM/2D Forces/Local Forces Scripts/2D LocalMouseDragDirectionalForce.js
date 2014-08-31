@script AddComponentMenu ("Forces 2D/Local/MouseDrag Directional Force")

var Force: Transform;
var ForcePower: float = 1;


function OnMouseDrag   (){
               
ActiveForce2D();

}



function ActiveForce2D(){


var MyObjectsForce : Vector2  = Force.transform.TransformDirection (0, 1, 0); 

gameObject.rigidbody2D.AddForce (MyObjectsForce *ForcePower);
}


function OnDrawGizmos () {
		// Draws a 5 meter long red line in front of the object
		Gizmos.color = Color.red;
		var direction : Vector3 = Force.transform.TransformDirection (Vector3.up) * 10;
		Gizmos.DrawRay (transform.position, direction);
	}