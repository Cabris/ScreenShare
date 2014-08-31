@script AddComponentMenu ("Forces 2D/Local/OnTrigger Spherical Force")

var Force: Transform;
var ForcePower: float = 1;
var TriggerDrag: float = 0;
var SpaceDrag: float = 0;


function OnTriggerStay2D(other: Collider2D) {

var GameObjectsForce = Force.transform.position - other.transform.position;

GameObjectsForce = GameObjectsForce.normalized;

other.rigidbody2D.AddForce (GameObjectsForce *ForcePower);

other.rigidbody2D.drag = TriggerDrag;
}


function OnTriggerExit2D(other: Collider2D) {

//GameObjects
other.rigidbody2D.drag = SpaceDrag;

}