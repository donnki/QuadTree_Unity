using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Ball : MonoBehaviour{
	public TreeNode node;
	private Renderer _renderer;
	
	void Awake(){
		_renderer = GetComponent<Renderer>();
	}

	private bool isHighlight = false;
	public void highlight(bool b){
        isHighlight = b;
        if (b)
        {
            _renderer.material.color = Color.red;
        }
        else {
            _renderer.material.color = Color.white;
        }
    }

	public void init(TreeNode node){
		this.node = node;
		transform.localScale = new Vector3(node.radius*2,node.radius*2 ,node.radius*2 );
		transform.position = new Vector3(node.center.x, 0, node.center.y);
		reset();
	}

	Vector3 targetPos;
	Vector3 originPos;
	float delta;
	public bool move = false;
	void reset(){
		originPos = transform.position;
		targetPos = new Vector3(Random.Range(0,600), 0, Random.Range(0,600));
		delta = 0;
	}
	void Update(){
		if(move){
			if(delta <= 1){
				transform.position = Vector3.Lerp(originPos, targetPos, delta);
				delta = delta + Time.deltaTime*0.1f;	
			}else{
				reset();
			}
				
		}
		
	}
#if UNITY_EDITOR
	void OnDrawGizmos(){
		UnityEditor.Handles.color = isHighlight ? Color.red : Color.white;
		UnityEditor.Handles.DrawWireDisc(transform.position,Vector3.up, node.radius);
	}
#endif
}

public class Demo : MonoBehaviour {
	public GameObject prefab;
	public int objectNum = 500;

	QuadTree quad;

	Ball[] balls;

	public Rect rect;
	public Vector2 center;
	public float radius;

	public Rect colliderRect;
	public float rectAngle;

	public float arcRadius;
	public float arcDegree;
	public Vector2 arcCenter;
	public float arcAngle;

	public Vector2 rayStart;
	public Vector2 rayEnd;
	void Start () {
		quad = new QuadTree(0, rect);
		balls = new Ball[objectNum];
		for(int i=0; i<objectNum; i++){
			GameObject go = Instantiate(prefab);
			balls[i] = go.AddComponent<Ball>();
			
			TreeNode node = new TreeNode(i, Random.Range(0,rect.width), Random.Range(0,rect.height), Random.Range(0.5f, 10f), 0, balls[i]);

			quad.insert(node);
			
			balls[i].init(node);
			
		}

		//System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
		//watch.Start ();
		//for (int i = 0; i < 1000000; i++) {
		//	//MathLib.isOBBRectIntersectCircle (colliderRect, rectAngle, center, radius);
		//	MathLib.isOBBRectIntersectObbRect(colliderRect, rectAngle,colliderRect, rectAngle);
		//}
		//watch.Start ();
		//print(watch.ElapsedMilliseconds);
	}

	void Update(){
        arcDegree += Time.deltaTime * 20;
		Vector3 v = transform.position;
		center.x = v.x;
		center.y = v.z;

		foreach(var ball in balls){
			ball.highlight(false);
			quad.update(ball.node, new Vector2(ball.transform.position.x, ball.transform.position.z));
		}

        List<TreeNode> list = new List<TreeNode>();
        quad.searchCircleArea(center, radius, list);
        
        quad.searchRectArea(colliderRect, rectAngle, list);
		

		// list = quad.searchSectorArea(arcCenter, arcRadius, arcAngle, arcDegree);
		Vector2 t = new Vector2(
			arcCenter.magnitude*Mathf.Cos(arcDegree * Mathf.Deg2Rad), 
			arcCenter.magnitude*Mathf.Sin(arcDegree * Mathf.Deg2Rad)
		);
		// print((arcAngle * Mathf.Deg2Rad)/2);
		quad.searchSectorArea(arcCenter, t.normalized, (arcAngle * Mathf.Deg2Rad)/2, arcRadius, list);

        
        quad.rayCast(rayStart, rayEnd, list);
        if(list != null)
            foreach (TreeNode node in list)
            {
                ((Ball)node.userdata).highlight(true);
            }

    }
	
	void OnGUI () {
		if(GUI.Button(new Rect(0,0,100,20), "test")){
			foreach(var ball in balls){
				ball.move = !ball.move;
			}
		}
		
	}
#if UNITY_EDITOR
	void drawRect(Rect rect){
		Gizmos.DrawLine(new Vector3(rect.min.x,0, rect.min.y), new Vector3(rect.max.x,0, rect.min.y));
        Gizmos.DrawLine(new Vector3(rect.max.x,0, rect.min.y), new Vector3(rect.max.x,0, rect.max.y));
        Gizmos.DrawLine(new Vector3(rect.max.x,0, rect.max.y), new Vector3(rect.min.x,0, rect.max.y));
        Gizmos.DrawLine(new Vector3(rect.min.x,0, rect.max.y), new Vector3(rect.min.x,0, rect.min.y));
	}

	Vector3 rotatePos(Vector2 p, Vector2 center, float cosTheta, float sinTheta){
		float x = cosTheta*(p.x-center.x) - sinTheta*(p.y-center.y) + center.x;
		float y = sinTheta*(p.x-center.x) + cosTheta*(p.y-center.y) + center.y;
		return new Vector3(x, 0, y);
	}

	void drawObbRect(Rect rect, float theta){
		var v = MathLib.getObbRectCorner(rect, theta);
		
		Vector3 lu = new Vector3(v[0].x, 0, v[0].y);
		Vector3 lb = new Vector3(v[1].x, 0, v[1].y);
		Vector3 rb = new Vector3(v[2].x, 0, v[2].y);
		Vector3 ru = new Vector3(v[3].x, 0, v[3].y);

		Gizmos.DrawLine(lu, lb);
        Gizmos.DrawLine(lb, rb);
        Gizmos.DrawLine(rb, ru);
        Gizmos.DrawLine(ru, lu);

	}

	void drawQuadTree(QuadTree q){
		if(q == null) return;

		drawRect(q.bounds);
		if(q.subnodes[0] != null){
			foreach(QuadTree node in q.subnodes){
				drawQuadTree(node);
			}
		}
	}

	void OnDrawGizmos() {

		Vector3 p = new Vector3(center.x,0,center.y);
		

        Gizmos.color = Color.white;
        Rect r = new Rect(300,50,100,200);
        drawObbRect(r, 30);

        drawQuadTree(quad);

        // Gizmos.DrawWireSphere(new Vector3(center.x,0,center.y), radius);
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(p,Vector3.up, radius);
        Gizmos.color = Color.red;

        var c = new Vector3(arcCenter.x,0,arcCenter.y);
        var from = new Vector3(arcCenter.x+arcRadius*Mathf.Cos((arcDegree+arcAngle/2)*Mathf.Deg2Rad), 0, arcCenter.y+arcRadius*Mathf.Sin((arcDegree+arcAngle/2)*Mathf.Deg2Rad));
        var to = new Vector3(arcCenter.x+arcRadius*Mathf.Cos((arcDegree-arcAngle/2)*Mathf.Deg2Rad), 0, arcCenter.y+arcRadius*Mathf.Sin((arcDegree-arcAngle/2)*Mathf.Deg2Rad));

        var t = new Vector2(arcCenter.x+arcRadius*Mathf.Cos(arcDegree*Mathf.Deg2Rad), arcCenter.y+arcRadius*Mathf.Sin(arcDegree*Mathf.Deg2Rad));
        var ct = new Vector3(t.x, 0, t.y);

        UnityEditor.Handles.DrawWireArc(c, Vector3.up, from-c, arcAngle, arcRadius);
        UnityEditor.Handles.DrawLine(c, from);
        UnityEditor.Handles.DrawLine(c, to);
        UnityEditor.Handles.DrawLine(c, ct);

        if(MathLib.isOBBRectIntersectObbRect(colliderRect, rectAngle, r, 30)){
        	Gizmos.color = Color.blue;
        }
        drawObbRect(colliderRect, rectAngle);
        
        if(MathLib.isSegmentIntersectObbRect(rayStart, rayEnd, colliderRect, rectAngle)){
        		Gizmos.color = Color.green;
        		Vector2 po = MathLib.RaycastObbRect(colliderRect, rectAngle, rayStart, rayEnd);
        		UnityEditor.Handles.DrawSolidDisc(new Vector3(po.x,0,po.y), Vector3.up, 2);
        }
        Gizmos.DrawLine(new Vector3(rayStart.x, 0, rayStart.y), new Vector3(rayEnd.x, 0, rayEnd.y));

      	
        // print(Vector2.Angle(rayStart, rayEnd));
        // print(rayStart.x*rayEnd.y - rayStart.y*rayEnd.x);

        // if(MathLib.isOBBRectIntersectCircle(colliderRect, rectAngle, center, radius)){
        	Vector2 newCenter = MathLib.rotatePos (center, colliderRect.center, Mathf.Cos (-rectAngle * Mathf.Deg2Rad), Mathf.Sin (-rectAngle * Mathf.Deg2Rad));
			Vector2 intersectionPoint = new Vector2 (Mathf.Max (colliderRect.x, Mathf.Min (newCenter.x, colliderRect.x + colliderRect.width)), Mathf.Max (colliderRect.y, Mathf.Min (newCenter.y, colliderRect.y + colliderRect.height)));
			intersectionPoint = MathLib.rotatePos (intersectionPoint, colliderRect.center, Mathf.Cos (rectAngle * Mathf.Deg2Rad), Mathf.Sin (rectAngle * Mathf.Deg2Rad));
        	UnityEditor.Handles.DrawSolidDisc(new Vector3(intersectionPoint.x,0,intersectionPoint.y), Vector3.up, 2);
        // }
    }
#endif
}
