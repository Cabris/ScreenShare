@script AddComponentMenu ("Forces 2D/Local/Spherical Force")
 
var Active: boolean = true;
var Force: Transform;
var ForcePower: float = 1 ;

function Update () {

if (Active == true){
var GameObjectsForce = Force.transform.position - gameObject.transform.position;

GameObjectsForce = GameObjectsForce.normalized;

gameObject.rigidbody2D.AddForce (GameObjectsForce *ForcePower);

}
}