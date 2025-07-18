using System.Collections.Generic; // List
using UnityEngine;


public class Replication: MonoBehaviour
{
    [Range(0,128)] public int cloneCount = 0;
    public Vector3 clone_offset = new(5,0,0);
    private List<GameObject> clones = new(){};
    Vector3 basePosition;
    public bool linkedRotation = false;
    
    GameObject Clone()
    {
        GameObject clone = GameObject.Instantiate(gameObject);
        DestroyImmediate(clone.GetComponent<Replication>()); // avoid infinite duplication-loop (all clones call 'Start')
        clone.GetComponent<HandlesBetter>().visibleAxes = false;
        clones.Add(clone); return clone;
    }
    
    GameObject CloneWithOffset(float x, float y=0, float z=0)
    {
        GameObject clone = Clone();
        Vector3 position = clone.transform.position;
        position.x += x; position.y += y; position.z += z;
        clone.transform.position = position;
        return clone;
    }
    
    GameObject CloneWithOffset(Vector3 offset) { return CloneWithOffset(offset.x, offset.y, offset.z); }
    
    void Start()
    {
        basePosition = gameObject.transform.position;
        while (clones.Count < cloneCount) {
            Clone();
            /* CloneWithOffset(clone_offset);
            clone_offset.x += 5; */
        }
    }
    
    void Update()
    {
        // adding or removing clones
        if (!(cloneCount == clones.Count)) {
            if (cloneCount < clones.Count) {
                foreach (GameObject obj in clones.GetRange(cloneCount, clones.Count-cloneCount)) Destroy(obj);
                clones.RemoveRange(cloneCount, clones.Count-cloneCount);
            }
            
            float xOffset = 0;
            while (cloneCount > clones.Count) {
                CloneWithOffset(xOffset+5);
            }
        }
        
        // updating clone positions
        basePosition = gameObject.transform.position;
        int I = 1;
        foreach (GameObject obj in clones) {
            if (linkedRotation)
            obj.transform.rotation = gameObject.transform.rotation;
            obj.transform.position = basePosition + (clone_offset * I++);
        }
    
    }
}