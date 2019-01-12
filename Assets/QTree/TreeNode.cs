using UnityEngine;
using System.Collections;
using System.Collections.Generic;

	public class TreeNode{
        public int id;
        public Vector2 center;
        public float radius;
        public object userdata;
        public List<TreeNode> colliders;

        public Rect colliderBox;
        public float boxAngle;
        public bool hasColliderBox;
        public object belongs;
		public int layer;
        public int agentId;


        public TreeNode(int id, float x, float y, float radius, int layer, object userdata){
            this.id = id;
            this.center = new Vector2(x,y);
            this.radius = radius;
			this.layer = layer;
            this.userdata = userdata;
            this.colliders = new List<TreeNode>();
        }

        public void setColliderBox(Rect rect, float angle){
            hasColliderBox = true;
            colliderBox = rect;
            boxAngle = angle;
        }

        public void setColliderBox(float x, float y, float w, float h, float angle){
            hasColliderBox = true;
            colliderBox = new Rect(x, y, w, h);
            boxAngle = angle;
        }

        public bool hasCollider(){
            return colliders.Count > 0;
        }

        // ~TreeNode(){
        //     Debug.Log("~~~destroy");
        // }
    }
