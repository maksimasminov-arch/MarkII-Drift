using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    Material matRoad, matPaint, matBlue, matWhite, matBarrier, matLamp;
    void Start()
    {
        Application.targetFrameRate=60;
        QualitySettings.vSyncCount=0;
        RenderSettings.ambientLight=new Color(.28f,.32f,.42f);
        RenderSettings.fog=true; RenderSettings.fogColor=new Color(.035f,.045f,.075f); RenderSettings.fogDensity=.012f;

        matRoad=Mat(new Color(.075f,.085f,.10f));
        matPaint=Mat(new Color(.88f,.9f,.94f),.55f);
        matBlue=Mat(new Color(.04f,.22f,.95f),.65f);
        matWhite=Mat(new Color(.8f,.83f,.88f));
        matBarrier=Mat(new Color(.17f,.20f,.24f));
        matLamp=Mat(new Color(1f,.65f,.24f),0,2f);

        Cube("Road",new Vector3(0,-.35f,45),new Vector3(12,.5f,150),matRoad);
        for(int z=-20;z<130;z+=8) {
            Cube("Center dash",new Vector3(0,-.08f,z),new Vector3(.12f,.03f,3.2f),matWhite);
            Cube("Left barrier",new Vector3(-6.4f,.2f,z),new Vector3(.5f,1f,7.5f),matBarrier);
            Cube("Right barrier",new Vector3(6.4f,.2f,z),new Vector3(.5f,1f,7.5f),matBarrier);
            if(z%24==0) {
                var l=GameObject.CreatePrimitive(PrimitiveType.Sphere); l.name="Street lamp";
                l.transform.position=new Vector3(z%48==0?-7.5f:7.5f,5,z);
                l.transform.localScale=Vector3.one*.35f; l.GetComponent<Renderer>().material=matLamp;
                var light=l.AddComponent<Light>(); light.type=LightType.Point; light.color=new Color(1f,.62f,.32f); light.range=12; light.intensity=2.2f;
            }
        }
        for(int i=0;i<22;i++) {
            float x=(i%2==0?-1:1)*Random.Range(10f,24f);
            float z=Random.Range(-15f,140f);
            Cube("Night silhouette",new Vector3(x,Random.Range(2f,7f),z),new Vector3(Random.Range(2f,6f),Random.Range(5f,16f),Random.Range(2f,6f)),matBarrier);
        }

        var car=new GameObject("White Drift Sedan");
        car.transform.position=new Vector3(0,.65f,0);
        car.AddComponent<Rigidbody>();
        car.AddComponent<DriftCarController>();
        var body=GameObject.CreatePrimitive(PrimitiveType.Cube); body.name="Wide sedan body"; body.transform.SetParent(car.transform,false);
        body.transform.localPosition=new Vector3(0,.45f,0); body.transform.localScale=new Vector3(2.05f,.58f,4.25f); body.GetComponent<Renderer>().material=matPaint;
        var cabin=GameObject.CreatePrimitive(PrimitiveType.Cube); cabin.transform.SetParent(car.transform,false);
        cabin.transform.localPosition=new Vector3(0,.92f,-.15f); cabin.transform.localScale=new Vector3(1.55f,.62f,2.05f); cabin.GetComponent<Renderer>().material=matBarrier;
        for(int side=-1;side<=1;side+=2) for(int axle=-1;axle<=1;axle+=2) {
            var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel.name="Blue alloy";
            wheel.transform.SetParent(car.transform,false); wheel.transform.localPosition=new Vector3(side*1.08f,.25f,axle*1.38f);
            wheel.transform.localRotation=Quaternion.Euler(0,0,90); wheel.transform.localScale=new Vector3(.72f,.18f,.72f);
            wheel.GetComponent<Renderer>().material=matBlue;
        }
        var collider=car.AddComponent<BoxCollider>(); collider.center=new Vector3(0,.55f,0); collider.size=new Vector3(2.1f,1.2f,4.3f);
        var cam=Camera.main;
        if(!cam) { var c=new GameObject("Main Camera"); cam=c.AddComponent<Camera>(); c.tag="MainCamera"; }
        cam.transform.position=new Vector3(0,4,-8);
        var chase=cam.gameObject.AddComponent<ChaseCamera>(); chase.target=car.transform;
        cam.fieldOfView=65;
        var sun=new GameObject("Moonlight").AddComponent<Light>(); sun.type=LightType.Directional; sun.intensity=.28f; sun.transform.rotation=Quaternion.Euler(35,-25,0);
        gameObject.AddComponent<TouchDriveUI>();
    }

    Material Mat(Color c,float metallic=0,float emission=0) {
        var m=new Material(Shader.Find("Standard")); m.color=c; m.SetFloat("_Metallic",metallic); m.SetFloat("_Glossiness",.65f);
        if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*emission);}
        return m;
    }
    void Cube(string n,Vector3 p,Vector3 s,Material m) {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().material=m;
    }
}
