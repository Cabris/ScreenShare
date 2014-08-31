
@script AddComponentMenu ("Forces/Global/Directional Force")


var Active: boolean = false; 
var OnMouseEvent: boolean = false;
var ObjectsTagName: String;
var Cloths: Cloth[] = null;
var ParticleSystems: ParticleEmitter[] = null;
var ForceSpeed: float = 1 ;
var ForceTurbulence: float = 0;

function Update () {

if (Active == true){

ActiveForce();
}
else{
DeActiveForce();
}
if (OnMouseEvent == true){

FunctionOnMouse();
}
}

//ACTIVE
function ActiveForce(){
     
var ForceRandomDirection : Vector3 = new Vector3(Random.Range(-1,2),0,0); 
this.transform.Rotate(ForceRandomDirection*ForceTurbulence); 
var externalAcceleration: Vector3;
var MyObjectsForce : Vector3  = this.transform.TransformDirection (0, 0, 1);    

//GameObjects
var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();
   
for (var i = 0; i < GameObjects.length; i++)
        {

	GameObjects[i].rigidbody.AddForce (MyObjectsForce * ForceSpeed);
	}
//Cloths	
if (Cloths != null) {


for (var i2 = 0; i2 < Cloths.length; i2++)
        {
	Cloths[i2].transform.GetComponent(Cloth).externalAcceleration = (MyObjectsForce*ForceSpeed) -Physics.gravity/10;
	
	}
	}

//Particlesystems
if (ParticleSystems != null){

for (var i3 = 0; i3 < ParticleSystems.length; i3++)
        {
	ParticleSystems[i3].worldVelocity = (MyObjectsForce * ForceSpeed);
}
}
}
function DeActiveForce (){
//Cloths	
for (var i5 = 0; i5 < Cloths.length; i5++)
        {
Cloths[i5].transform.GetComponent(Cloth).externalAcceleration = Vector3(0, 0, 0);
}
//Particlesystems
for (var i4 = 0; i4 < ParticleSystems.length; i4++)
        {
ParticleSystems[i4].worldVelocity = Vector3(0, 0, 0);
}
}



//ON MOUSE
function FunctionOnMouse(){


if(Input.GetMouseButton(1)){
     
var ForceRandomDirection : Vector3 = new Vector3(Random.Range(-1,2),0,0); 
this.transform.Rotate(ForceRandomDirection*ForceTurbulence); 
var externalAcceleration: Vector3;
var MyObjectsForce : Vector3  = this.transform.TransformDirection (0, 0, 1);    

//GameObjects
var GameObjects = GameObject.FindGameObjectsWithTag(ObjectsTagName).Select(function (go) {return go.transform;}).ToArray();
   
for (var i = 0; i < GameObjects.length; i++)
        {

	GameObjects[i].rigidbody.AddForce (MyObjectsForce * ForceSpeed);
	}
	}
//Cloths	
if(Input.GetMouseButton(1)){
if (Cloths != null) {


for (var i2 = 0; i2 < Cloths.length; i2++)
        {
	Cloths[i2].transform.GetComponent(Cloth).externalAcceleration = (MyObjectsForce*ForceSpeed) -Physics.gravity/10;
	
	}
	}
	}
	else{
for (var i5 = 0; i5 < Cloths.length; i5++)
        {
Cloths[i5].transform.GetComponent(Cloth).externalAcceleration = Vector3(0, 0, 0);
}
//Particlesystems

if(Input.GetMouseButton(1)){

if (ParticleSystems != null){

for (var i3 = 0; i3 < ParticleSystems.length; i3++)
        {
	ParticleSystems[i3].worldVelocity = (MyObjectsForce * ForceSpeed);
}
}
}
else{
for (var i4 = 0; i4 < ParticleSystems.length; i4++)
        {
ParticleSystems[i4].worldVelocity = Vector3(0, 0, 0);
}
}
}
}



function OnDrawGizmosSelected () {
		// Draws a 5 meter long red line in front of the object
		Gizmos.color = Color.red;
		var direction : Vector3 = transform.TransformDirection (Vector3.forward) * 5;
		Gizmos.DrawRay (transform.position, direction);
	}