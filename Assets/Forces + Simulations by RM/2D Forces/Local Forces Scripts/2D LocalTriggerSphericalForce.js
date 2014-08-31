@script AddComponentMenu ("Forces 2D/Local/Trigger Spherical Force")

var Force: Transform;
var ForcePower: float = 1;
var TriggerDrag: float = 0;
var SpaceDrag: float = 0;


function OnTriggerStay2D(other: Collider2D) {

var GameObjectsForce = Force.transform.position - gameObject.transform.position;

GameObjectsForce = GameObjectsForce.normalized;

gameObject.rigidbody2D.AddForce (GameObjectsForce *ForcePower);

gameObject.rigidbody2D.drag = TriggerDrag;
}


function OnTriggerExit2D(other: Collider2D) {

//GameObjects
gameObject.rigidbody2D.drag = SpaceDrag;

}