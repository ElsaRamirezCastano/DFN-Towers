using UnityEngine;

public static class Globals{
   public static Bounds worldBounds;

   public static Bounds WorldBounds{
        get{return worldBounds;}
        set{ worldBounds = value; }
   }

   public static void InitializeWorldBounds(float minX, float minY, float maxX, float maxY){
        Vector3 center = new Vector3((minX + maxX)/2f, (minY + maxY)/2f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);

        worldBounds = new Bounds(center, size);
   }
}
