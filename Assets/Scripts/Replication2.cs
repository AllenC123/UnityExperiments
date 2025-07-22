using System.Collections.Generic; // List
using UnityEngine;


public class CloneT
{
    internal GameObject m_obj; // 'internal' members are accessible to other classes in the file
    internal Vector3 basePosition;
    internal Vector3 m_offset = new(0,0,0); // manual offset per-clone
    internal bool hasReplicator = false;
    
    public T GetComponent<T>() => m_obj.GetComponent<T>();
    public T AddComponent<T>() where T: UnityEngine.Component => m_obj.AddComponent<T>();
    public void Destroy() {
        GameObject.Destroy(m_obj);
        if (hasReplicator)
        foreach (CloneT clone in m_obj.GetComponent<Replication2>().clones)
            GameObject.Destroy(clone.m_obj);
    }
    
    public void UpdateTransform(Transform parent, Vector3 seq_offset, bool isRotationLinked)
    {
        basePosition = parent.position;
        m_obj.transform.position = basePosition + m_offset + seq_offset;
        //m_obj.transform.RotateAround(basePosition, Camera.main.transform.forward, Time.timeSinceLevelLoad * 10);
        if (isRotationLinked) m_obj.transform.rotation = parent.rotation;
        if (!hasReplicator) return;
        
        // propagating update to recursive clones if replicator exists
        Replication2 replicator = m_obj.GetComponent<Replication2>();
        replicator.Update();
        return;
    }
    
    public CloneT(GameObject parent, Vector3 offset) {
        m_obj = GameObject.Instantiate(parent);
        // the only way to make a child object in Unity is to share transform object (which breaks everything)
        // m_obj.transform.SetParent(parent.transform); // Unity is garbage
        basePosition = parent.transform.position;
        m_offset = offset;
    }
}


public class Replication2: MonoBehaviour
{
    [Range(0,128)] public int cloneCount = 0;
    public Vector3 seq_offset = new(0, 0, 0); // offset applied sequentially to clones
    internal List<CloneT> clones = new(){};
    public bool linkedRotation = false;
    private bool isClone = false;
    
    private float ZoffsetHack = 3;
    
    // these can't be static - unity can't display static variables
    public bool satellite_behavior = true; // clones orbit (rotate) around the parent
    public bool satellite_axis_cam = true; // use camera's orientation as axis of rotation (otherwise, relative to object)
    [Range(-16, 16)] public float satellite_orbit_speed = 0.0f;
    [Range(-1, 1)] public float orbit_speed_increment = 0.0f; // applied to orbit-speeds of each recursive-clone, sequentially
    public bool increment_as_multiplier = false;
    
    
    CloneT Clone(float x=0, float y=0, float z=0) // manual clone offset
    {
        CloneT clone = new CloneT(gameObject, new Vector3(x, y, z));
        DestroyImmediate(clone.GetComponent<Replication2>()); // avoid infinite duplication-loop (all clones call 'Start')
        // removing these from clones for better performance
        DestroyImmediate(clone.GetComponent<RotationControls>());
        DestroyImmediate(clone.GetComponent<HandlesBetter>());
        //DestroyImmediate(clone.GetComponent<SphereCollider>());
        
        //clone.GetComponent<HandlesBetter>().visibleAxes = false;
        clones.Add(clone); return clone;
    }
    
    // create clones from a list of offsets
    void Fill(List<Vector3> offsetList)
    {
        cloneCount += offsetList.Count;
        float scale = 0.75f;
        float scaleDelta = (scale/offsetList.Count) * 0.75f;
        foreach (Vector3 offset in offsetList) {
            CloneT newclone = Clone(offset.x, offset.y, offset.z);
            scale -= scaleDelta;
            newclone.m_obj.transform.localScale *= scale;
        }
    }
    
    // creates a clone with it's own replication monobehavior
    CloneT RecursiveClone(Vector3 seqOffset, List<Vector3> offsetList)
    {
        CloneT clone = Clone();
        clone.hasReplicator = true;
        cloneCount += 1; // prevent new clone from being auto-deleted during Update()
        
        // Unity does not provide any mechanism for calling a constructor (passing in parameters is impossible)
        Replication2 replicator = clone.AddComponent<Replication2>();
        replicator.isClone = true;
        replicator.seq_offset = seqOffset;
        replicator.linkedRotation = linkedRotation;
        replicator.Fill(offsetList);
        return clone;
    }
    
    void Awake() {
        // disabled scripts still run; only their 'update' method is disabled.
        // so we remove the script-component from the object manually on startup if it's disabled.
        if (!enabled) Destroy(gameObject.GetComponent<Replication2>());
        // https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    }
    
    void MakeSatellites()
    {
        linkedRotation = true;
        float Xmax = 48;
        float spiralPeriod = 16;
        float spiralFrequency = ((2.0f * 3.14f) / spiralPeriod);
        
        //for (int X=1; X<16; ++X) { offsetList.Add(new(X, 0, 0)); offsetList.Add(new(0, X, 0)); offsetList.Add(new(-X, 0, 0)); offsetList.Add(new(0, -X, 0));}
        for (int Z=1; Z <= Xmax; ++Z) {
            List<Vector3> offsetList = new(){};
            for (int X=1; X <= Xmax; ++X) { offsetList.Add(new Vector3(-(float)System.Math.Sin(spiralFrequency*X) * ((X+Z)*0.625f), -(float)System.Math.Cos(spiralFrequency*X) * ((X+Z)*0.625f), (X+Z)*0.25f)); }
            CloneT baseclone = RecursiveClone(new Vector3((float)System.Math.Sin(Z)*0.025f, (float)System.Math.Cos(Z)*0.025f, 0), offsetList);
            baseclone.m_offset = new Vector3((float)System.Math.Sin(spiralFrequency*Z), (float)System.Math.Cos(spiralFrequency*Z), Z*ZoffsetHack);
        }
    }
    
    void Start()
    {
        if (isClone) return;
        // MakeSatellites();
    }
    
    public void Update()
    {
        if (!isClone) {
            // Squish the spiral until it's inside-out; +3 -> -3
            seq_offset.z = ZoffsetHack + (1.5f*ZoffsetHack * ((float)System.Math.Sin(Time.timeSinceLevelLoad*0.0628) - 1));
        }
        // adding or removing clones
        /* if (!(cloneCount == clones.Count)) {
            if (cloneCount < clones.Count) {
                foreach (CloneT clone in clones.GetRange(cloneCount, clones.Count-cloneCount)) clone.Destroy();
                clones.RemoveRange(cloneCount, clones.Count-cloneCount);
            }
            
            while (cloneCount > clones.Count) { Clone(); }
        } */
        
        float satellite_angle = Time.timeSinceLevelLoad * satellite_orbit_speed;
        Vector3 satellite_axis = (satellite_axis_cam? Camera.main.transform.forward : transform.forward);
        
        // updating clone positions
        int I = 1;
        foreach (CloneT clone in clones) {
            clone.UpdateTransform(gameObject.transform, (seq_offset * I++), linkedRotation);
            // propagating satellite behaviors/parameters to recursive clones
            if (!isClone && clone.hasReplicator) {
                Replication2 replicator = clone.m_obj.GetComponent<Replication2>();
                replicator.satellite_behavior = satellite_behavior;
                replicator.satellite_axis_cam = satellite_axis_cam;
                float increment = (increment_as_multiplier? (I*orbit_speed_increment) * ((0.5f*satellite_orbit_speed) + orbit_speed_increment) : (I*orbit_speed_increment));
                replicator.satellite_orbit_speed = satellite_orbit_speed + increment;
            }
            if (satellite_behavior) {
                clone.m_obj.transform.RotateAround(transform.position, satellite_axis, satellite_angle);
            }
        }
    }
    
    void OnGUI()
    {
        if (GUILayout.Button("Make Satellites")) { MakeSatellites(); }
    }
}