@script AddComponentMenu ("Forces 2D/Global/Directional Force")
 
var Active: boolean = true;
var OnMouse: boolean = false;
var OnCollision: boolean = false;
var OnTrigger: boolean = false;
var ObjectsTagName: String;
var ForcePower: float = 1 ;


function Update () {

if (Active == true){

ActiveForce();
}

if (OnMouse == true){

FunctionOnMouse();
}
}

function ActiveForce () {

if (Active == true){

var MyObjectsForce : Vector2  = this.transform.TransformDirection (0, 0, 1); 
var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

for (var i = 0; i < GameObjects.length; i++)
        {
GameObjects[i].rigidbody2D.AddForce (MyObjectsForce *ForcePower);
   
	}
}
}

function FunctionOnMouse(){


if(Input.GetMouseButton(1)){

if (OnMouse == true){

var MyObjectsForce : Vector2  = this.transform.TransformDirection (0, 0, 1); 
var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

for (var i = 0; i < GameObjects.length; i++)
        {
GameObjects[i].rigidbody2D.AddForce (MyObjectsForce *ForcePower);
   
	}
}
}
}
//On Collision
function OnCollisionEnter2D(collision : Collision2D) {	
		
if (OnCollision == true){


var MyObjectsForce : Vector2  = this.transform.TransformDirection (0, 0, 1); 
var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

for (var i = 0; i < GameObjects.length; i++)
        {
GameObjects[i].rigidbody2D.AddForce (MyObjectsForce *ForcePower);
   
	}
}
}

//On Collision
function OnTriggerStay2D(collision : Collider2D) {	
		
if (OnTrigger == true){


var MyObjectsForce : Vector2  = this.transform.TransformDirection (0, 0, 1); 
var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();

for (var i = 0; i < GameObjects.length; i++)
        {
GameObjects[i].rigidbody2D.AddForce (MyObjectsForce *ForcePower);
   
	}
}
}


//DRAW GIZMOS

function OnDrawGizmosSelected () {
		// Draws a 5 meter long red line in front of the object
		Gizmos.color = Color.red;
		var direction : Vector3 = transform.TransformDirection (Vector3.forward) * 10;
		Gizmos.DrawRay (transform.position, direction);
	}