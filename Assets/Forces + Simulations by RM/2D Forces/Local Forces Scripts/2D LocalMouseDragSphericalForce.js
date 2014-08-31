@script AddComponentMenu ("Forces 2D/Local/MouseDrag Spherical Force")

var Force: Transform;
var ForcePower: float = 1;


function OnMouseDrag   (){
               
ActiveForce2D();

}



function ActiveForce2D(){

var GameObjectsWindForce = Force.transform.position - gameObject.transform.position;

GameObjectsWindForce = GameObjectsWindForce.normalized;

gameObject.rigidbody2D.AddForce (GameObjectsWindForce *ForcePower);
}