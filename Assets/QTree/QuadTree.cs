using System;
using System.Collections.Generic;
using UnityEngine;

    public class QuadTree
    {
        public static bool CheckInternalCollision = false;
        private const int MAX_OBJECTS = 5;
        private const int MAX_LEVELS = 10;

        int level;
        public List<TreeNode> objects;
        public Rect bounds;
        public QuadTree[] subnodes;
        QuadTree parent;
        // private 
        public QuadTree(int level, Rect bounds, QuadTree parent=null)
			:this(level, bounds.x, bounds.y, bounds.width, bounds.height,parent){}

		public QuadTree(int level, float x, float y, float w, float h, QuadTree parent=null){
            Rect bounds = new Rect(x,y,w,h);
            this.level = level;
            this.objects = new List<TreeNode>();
            this.bounds = bounds;
            this.subnodes = new QuadTree[4];
            this.parent = parent;
        }

        public void clear(){
            objects.Clear();
            for(int i=0; i<subnodes.Length; i++){
                if(subnodes[i] != null){
                    subnodes[i].clear();
                    subnodes[i] = null;
                }
            }
        }

        private void split(){
            float subWidth = (bounds.width/2);
            float subHeight = (bounds.height/2);
            float x = bounds.x;
            float y = bounds.y;

            subnodes[0] = new QuadTree(level+1, new Rect(x+subWidth, y, subWidth,subHeight), this);
            subnodes[1] = new QuadTree(level+1, new Rect(x, y, subWidth,subHeight), this);
            subnodes[2] = new QuadTree(level+1, new Rect(x, y+subHeight, subWidth,subHeight), this);
            subnodes[3] = new QuadTree(level+1, new Rect(x+subWidth, y+subHeight, subWidth,subHeight), this);
        }

        private void onCheckCollision(TreeNode node){
            if(CheckInternalCollision){
                QuadTree root = this;
                while(root.parent != null){
                    root = root.parent;
                }
                var list = new List<TreeNode>();
                root.searchCircleArea(node.center, node.radius, list);

                for (int i = 0; i < node.colliders.Count; i++)
                {
                    TreeNode other = node.colliders[i];
                    if (!list.Contains(other))
                    {
                        node.colliders.Remove(other);
                        other.colliders.Remove(node);
                    }
                }
                for (int i = 0; i < list.Count; i++)
                {
                    TreeNode other = list[i];
                    if (other == node) continue;
                    if (!other.colliders.Contains(node))
                    {
                        other.colliders.Add(node);
                        node.colliders.Add(other);
                    }
                }



            }
           
        }

        public bool insert(TreeNode node, bool checkCollision=true, bool checkNodeBounds=false){
            if(checkNodeBounds){
                if(node.hasColliderBox){
                    if(!MathLib.isRectContainsOBBRect(bounds, node.colliderBox, node.boxAngle)){
                        return false;
                    }
                }else{
                    if(!MathLib.isRectContainsCircle(bounds, node.center, node.radius)){
                        return false;
                    }
                }
            }else{
                if(!bounds.Contains(node.center)){
                    return false;
                }         
            }
            
            if(objects.Count < MAX_OBJECTS || level > MAX_LEVELS){
                if(checkCollision){
                    onCheckCollision(node);
                }
                node.belongs = this;
                objects.Add(node);
                return true;
            }

            if(subnodes[0] == null){
                split();
            }
            for(int i=0;i<4;i++){
                if(subnodes[i].insert(node, checkCollision, checkNodeBounds)){
                    return true;
                }
            }
            objects.Add(node);
            return true;
        }

        public QuadTree find(TreeNode node){
            if(!bounds.Contains(node.center)){
                return null;
            }
            if(objects.Contains(node)){
                return this;
            }
            if(subnodes[0] != null){
                for(int i=0;i<4;i++){
                    QuadTree qt = subnodes[i].find(node);
                    if(qt != null){
                        return qt;
                    }
                }
            }
            return null;
        }

        public void update(TreeNode node, Vector2 newPos, bool checkCollision=true, bool checkNodeBounds=false){
            QuadTree root = (QuadTree)node.belongs;
            if(root != null){
                bool b = false;
                if(!checkNodeBounds){
                    b = root.bounds.Contains(newPos);
                }else{
                    b = MathLib.isRectContainsCircle(root.bounds, newPos, node.radius);
                }
                if(b){
                    node.center = newPos;

                    //Collision detect
                    if(checkCollision){
                        onCheckCollision(node);    
                    }
                }else{

                    root.remove(node, true);
                    
                    node.center = newPos;
                    while(root.parent != null){
                        root = root.parent;
                    }
                    root.insert(node, checkCollision, checkNodeBounds);
                }
            }
        }

        public bool remove(TreeNode node, bool isDirect=false){
            if(isDirect){
                for(int i=0; i<node.colliders.Count; i++){
                    TreeNode other = node.colliders[i];
                    other.colliders.Remove(node);
                }
                node.colliders.Clear();
				bool b = objects.Remove(node);                
				return b;
            }else{
                QuadTree root = find(node);
                return root.remove(node, true);
            }
            
        }

        public void rayCast(Vector2 rayStart, Vector2 rayEnd, List<TreeNode> ret)
        {
            if(!MathLib.isSegmentIntersectRect(rayStart, rayEnd, bounds)){
                return ;
            }
            for(int i=0; i<objects.Count; i++){
                if(objects[i].hasColliderBox){
                    if(MathLib.isSegmentIntersectObbRect(rayStart, rayEnd, objects[i].colliderBox, objects[i].boxAngle)){
                        ret.Add(objects[i]);
                    }
                }else{
                    if(MathLib.isSegmentIntersectCircle(rayStart, rayEnd, objects[i].center, objects[i].radius)){
                        ret.Add(objects[i]);
                    }
                }
            }

            if(subnodes[0] == null){
                return ;
            }

            for(int i=0; i<4; i++){
                subnodes[i].rayCast(rayStart, rayEnd, ret);
                
            }
            
        }
        public List<int> rayCastForLayer(float x1, float y1, float x2, float y2, int team, int type, bool checkObstacle){
            List<int> ret = null;
            Vector2 rayStart = new Vector2(x1, y1);
            Vector2 rayEnd = new Vector2(x2, y2);
            if(!MathLib.isSegmentIntersectRect(rayStart, rayEnd, bounds)){
                return ret;
            }
            for(int i=0; i<objects.Count; i++){
                if(objects[i].hasColliderBox){
                    if(MathLib.isSegmentIntersectObbRect(rayStart, rayEnd, objects[i].colliderBox, objects[i].boxAngle)
                         && checkTeam(objects[i], team, type, checkObstacle)){
                        if(ret == null) ret = new List<int>();
                        ret.Add(objects[i].id);
                    }
                }else{
                    if(MathLib.isSegmentIntersectCircle(rayStart, rayEnd, objects[i].center, objects[i].radius)
                         && checkTeam(objects[i], team, type, checkObstacle)){
                        if(ret == null) ret = new List<int>();
                        ret.Add(objects[i].id);
                    }
                }
            }

            if(subnodes[0] == null){
                return ret;
            }

            for(int i=0; i<4; i++){
                var list = subnodes[i].rayCastForLayer(x1, y2, x2, y2, team, type, checkObstacle);
                if(list != null){
                    if(ret == null) ret = new List<int>();
                    ret.AddRange(list);    
                }
                
            }

            return ret;
        }

        public void searchCircleArea(Vector2 center, float range, List<TreeNode> ret)
        {

            if(!MathLib.isRectIntersectCircle(bounds, center, range)){
                return ;
            }

            
            for(int i=0; i<objects.Count; i++){
                if(objects[i].hasColliderBox){
                    if(MathLib.isOBBRectIntersectCircle(objects[i].colliderBox, objects[i].boxAngle, center, range)){
                        if(ret == null) ret = new List<TreeNode>();
                        ret.Add(objects[i]);
                    }
                }else{
                    if((objects[i].center - center).sqrMagnitude < (range + objects[i].radius) * (range + objects[i].radius)){
                        if(ret == null) ret = new List<TreeNode>();
                        ret.Add(objects[i]);
                    }
                }
               
            }

            if(subnodes[0] == null){
                return ;
            }

            for(int i=0; i<4; i++){
                subnodes[i].searchCircleArea(center, range, ret);
                
            }

        }

        private bool checkTeam(TreeNode node, int team, int type, bool checkObstacle){
            if(node.layer == -1){
                return checkObstacle;
            }
            switch(type){
                case 0: return team == node.layer;
                case 1: return team != node.layer;
                case 2: return true;

            };
            return true;
        }

        public List<int> searchCircleAreaForLayer(float x, float y, float range, int team, int type, bool checkObstacle){
            List<int> ret = null;
            var center = new Vector2(x, y);
            if(!MathLib.isRectIntersectCircle(bounds, center, range)){
                return ret;
            }

            
            for(int i=0; i<objects.Count; i++){
                if(objects[i].hasColliderBox){
                    if(MathLib.isOBBRectIntersectCircle(objects[i].colliderBox, objects[i].boxAngle, center, range)
                        && checkTeam(objects[i], team, type, checkObstacle)){
                        if(ret == null) ret = new List<int>();
                        ret.Add(objects[i].id);
                    }
                }else{
                    if((objects[i].center - center).sqrMagnitude < (range + objects[i].radius) * (range + objects[i].radius)
                        && checkTeam(objects[i], team, type, checkObstacle)) {
                        if(ret == null) ret = new List<int>();
                        ret.Add(objects[i].id);
                    }
                }
               
            }

            if(subnodes[0] == null){
                return ret;
            }

            for(int i=0; i<4; i++){
                var list = subnodes[i].searchCircleAreaForLayer(x, y, range, team, type, checkObstacle);
                if(list != null){
                    if(ret == null) ret = new List<int>();
                    ret.AddRange(list);    
                }
                
            }

            return ret;
        }

        public void searchRectArea(Rect rect, float angle, List<TreeNode> ret)
        {
            if (!MathLib.isOBBRectIntersectObbRect(rect, angle, bounds, 0))
            {
                return;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                // if(rect.Contains(objects[i].center)){
                // if(MathLib.isOBBRectContainsPoint(rect, angle, objects[i].center)){
                if (MathLib.isOBBRectIntersectCircle(rect, angle, objects[i].center, objects[i].radius))
                {
                    ret.Add(objects[i]);
                }
            }

            if (subnodes[0] == null)
            {
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                subnodes[i].searchRectArea(rect, angle, ret);

            }
        }

        public List<int> searchRectAreaForLayer(float x, float y, float w, float h, float angle, int team, int type, bool checkObstacle){
            List<int> ret = null;
            Rect rect = new Rect(x, y, w, h);
            if(!MathLib.isOBBRectIntersectObbRect(rect, angle, bounds, 0)) {
                return ret;
            }

            for(int i=0; i<objects.Count; i++){
                // if(rect.Contains(objects[i].center)){
                // if(MathLib.isOBBRectContainsPoint(rect, angle, objects[i].center)){
                if(MathLib.isOBBRectIntersectCircle(rect, angle, objects[i].center, objects[i].radius)
                    && checkTeam(objects[i], team, type, checkObstacle)){
                    if(ret == null) ret = new List<int>();
                    ret.Add(objects[i].id);
                }
            }

            if(subnodes[0] == null){
                return ret;
            }

            for(int i=0; i<4; i++){
                var list = subnodes[i].searchRectAreaForLayer(x,y,w,h,angle, team, type, checkObstacle);
                if(list != null){
                    if(ret == null) ret = new List<int>();
                    ret.AddRange(list);    
                }
                
            }
            return ret;
        }


        public List<int> searchSectorAreaV2ForLayer(float x, float y, Vector2 direction, float theta, float radius, int team, int type, bool checkObstacle){
            List<int> ret = null;
            Vector2 center = new Vector2(x, y);
            if(!MathLib.isRectIntersectCircle(bounds, center, radius)){
                return ret;
            }

            for(int i=0; i<objects.Count; i++){
                if(MathLib.IsSectorDiskIntersect(center, direction, theta, radius, objects[i].center, objects[i].radius)
                    && checkTeam(objects[i], team, type, checkObstacle)) {
                    if(ret == null) ret = new List<int>();
                    ret.Add(objects[i].id);
                }
            }

            if(subnodes[0] == null){
                return ret;
            }

            for(int i=0; i<4; i++){
                var list = subnodes[i].searchSectorAreaV2ForLayer(x, y, direction, theta, radius, team, type, checkObstacle);
                if(list != null){
                    if(ret == null) ret = new List<int>();
                    ret.AddRange(list);    
                }
                
            }

            return ret;
        } 

        /// <summary>
        /// 搜索扇形区域内的所有子结点
        /// </summary>
        /// <param name="center">扇形的圆心</param>
        /// <param name="direction">方向</param>
        /// <param name="theta">弧长</param>
        /// <param name="radius">半径</param>
        /// <param name="ret">搜索到的结果将被插入到ret里</param>
        public void searchSectorArea(Vector2 center, Vector2 direction, float theta, float radius, List<TreeNode> ret)
        {
            if(!MathLib.isRectIntersectCircle(bounds, center, radius)){
                return ;
            }

            for(int i=0; i<objects.Count; i++){
                if(MathLib.IsSectorDiskIntersect(center, direction, theta, radius, objects[i].center, objects[i].radius)) {
                    ret.Add(objects[i]);
                }
            }

            if(subnodes[0] == null){
                return;
            }

            for(int i=0; i<4; i++){
                subnodes[i].searchSectorArea(center, direction, theta, radius, ret);
            }

        } 

        public Vector2 resolveCollision(TreeNode node, float speedX, float speedY){
            Vector2 velocity = Vector2.zero;

            Vector2 originVelocity = new Vector2(speedX, speedY);

            if(!CheckInternalCollision) return originVelocity;

            for(int a=0; a<node.colliders.Count; a++){
                TreeNode circle2 = node.colliders[a];
                if(!circle2.hasColliderBox){
                    Vector2 c = (node.center -  circle2.center) / 2;
                    velocity += c.normalized*originVelocity.magnitude;
                }else{
                    var wallRect = circle2.colliderBox;
                    var wallAngle = circle2.boxAngle;

					Vector2 center = MathLib.rotatePos (node.center, wallRect.center, Mathf.Cos (-wallAngle * Mathf.Deg2Rad), Mathf.Sin (-wallAngle * Mathf.Deg2Rad));
					Vector2 intersectionPoint = new Vector2 (Mathf.Max (wallRect.x, Mathf.Min (center.x, wallRect.x + wallRect.width)), Mathf.Max (wallRect.y, Mathf.Min (center.y, wallRect.y + wallRect.height)));
                    intersectionPoint = MathLib.rotatePos (intersectionPoint, wallRect.center, Mathf.Cos (wallAngle * Mathf.Deg2Rad), Mathf.Sin (wallAngle * Mathf.Deg2Rad));
					Vector2 c = node.center - intersectionPoint;
					velocity += c.normalized * originVelocity.magnitude;  
                }
            }
            return velocity;
        }



    }



