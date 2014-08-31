@script AddComponentMenu ("Forces 2D/Local/Directional Force")
 
var Active: boolean = true;
var Force: Transform;
var ForcePower: float = 1 ;

function Update () {

if (Active == true){
var MyObjectsForce : Vector2  = Force.transform.TransformDirection (0, 0, 1); 

gameObject.rigidbody2D.AddForce (MyObjectsForce *ForcePower);

}
}

//DRAW GIZMOS

function OnDrawGizmos () {
		// Draws a 5 meter long red line in front of the object
		Gizmos.color = Color.red;
		var direction : Vector3 = Force.transform.TransformDirection (Vector3.forward) * 10;
		Gizmos.DrawRay (transform.position, direction);
	}