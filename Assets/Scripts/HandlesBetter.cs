using UnityEngine;
using UnityEditor; // Handles
using Unity.VisualScripting; // IsRightMouseButton


public class HandlesBetter: MonoBehaviour
{
    // handle colors
    Color currentColor;
    readonly Color defaultColor = new Color(1, 0.8f, 0.4f, 1.0f);
    readonly Color hoveredColor = new Color(0, 1, 0, 1.0f);
    readonly Color focusedColor = new Color(1, 0, 1, 1.0f);
    
    bool isHovered = false;
    bool isFocused = false;
    bool isEditingPosition = true;
    public bool isFreeSpin = true;
    public Vector2 rotationDeltas; //TODO: why do these continuously cycle after rotation has ended?
    [Range(1, 256)] public float rotationSpeed;
    [Range(0,2.0f)] public float friction = 1f;
    
    public bool visibleAxes = true;
    public bool matchColliderRadius = true;
    [Range(0, 10f)] public float handleRadius = 5f; // slider which sets handlesize on all three axes
    Vector3 handleSizes; // per-axis handlesize; perfectly visualizes collider when 'matchColliderRadius' is enabled
    
    Camera mainCamera;
    new Renderer renderer;  // material colors can be accessed through this. 'new' keyword hides the (deprecated) inherited field
    Color originalMatColor; // storing original color of the mesh
    bool hasMaterialColors; // if the material/shader doesn't have a '_Color' property, it would crash;
    // "Material 'Material1 (Instance)' with Shader 'Unlit/Shader1' doesn't have a color property '_Color'"
    
    Vector3 GetColliderSize() { // collider is also affected by objects' scaling; so apply it for effective size
        return gameObject.transform.localScale * GetComponent<SphereCollider>().radius;
    } // TODO: handle arbitrary collider-types here
    // TODO: update collider position to match wavy-shader? Is that even possible?
    
    void Awake() {
        // disabled scripts still run; only their 'update' method is disabled.
        // so we remove the script-component from the object manually on startup if it's disabled.
        if (!enabled) Destroy(gameObject.GetComponent<HandlesBetter>());
        // https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    }
    
    void Start()
    {
        currentColor = defaultColor;
        isHovered = false;
        isFocused = false;
        rotationSpeed = 64;
        rotationDeltas = new(0,0);
        friction = 1.0f;
        
        mainCamera = Camera.main;
        renderer = GetComponent<Renderer>();
        hasMaterialColors = renderer.sharedMaterial.HasProperty("_Color");
        if (hasMaterialColors) originalMatColor = renderer.sharedMaterial.color;
        
        //Physics.queriesHitTriggers = true; // not necessary here. also a project-setting.
        
        handleSizes = new Vector3(handleRadius, handleRadius, handleRadius);
        if (matchColliderRadius) {
            //print(string.Format("collider radius: {0}", GetComponent<SphereCollider>().radius));
            //print(string.Format("object localScale: {0}", gameObject.transform.localScale));
            handleSizes = GetColliderSize(); handleRadius = (handleSizes.x);
        }
    }
    
    // These overrides require the legacy input system. Also the object's collider needs to be marked as 'isTrigger'? <-- apparently not.
    void OnMouseEnter() { isHovered = true; if(!hasMaterialColors) return; renderer.sharedMaterial.color = Color.lightGreen; }
    void OnMouseExit() { isHovered = false; if(!hasMaterialColors) return; renderer.sharedMaterial.color = originalMatColor; }
    
    void OnRenderObject()
    {
        currentColor = defaultColor;
        if (isHovered) currentColor = hoveredColor;
        if (isFocused) currentColor = focusedColor;
        
        GUI.color = currentColor; // applies to label
        Handles.color = currentColor;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; // enables occlusion
        
        // builtin handles are not interactive in game-mode
        //transform.rotation = Handles.RotationHandle(transform.rotation, transform.position);
        
        if (!visibleAxes) return;
        if (visibleAxes) {
            if (matchColliderRadius) {
                handleSizes = GetColliderSize();
                handleRadius = (handleSizes.x);
            } else handleSizes = new Vector3(handleRadius, handleRadius, handleRadius);
            
            // draws a ring around the X-axis
            Handles.DrawWireDisc(transform.position, transform.right, handleSizes.x);
            Handles.DrawWireDisc(transform.position, transform.up, handleSizes.y);
            //Handles.DrawWireDisc(transform.position, transform.forward, handleSizes.z);
        }
        
        Vector3 labelPosition = transform.position + new Vector3(handleSizes.x*0.4f, handleSizes.y, 0);
        if (isEditingPosition)
             Handles.Label(labelPosition, transform.position.ToString());
        else Handles.Label(labelPosition, transform.rotation.ToString());
    }
    
    
    // called after all other rendering steps, except 'OnDrawGizmos'
    void OnGUI()
    {
        if (Event.current.isMouse)
        {
            // print(string.Format("MouseEvent: {0}", Event.current.ToString()));
            switch (Event.current.type)
            {
                // mouse interactions
                case EventType.MouseDown: isFocused = isHovered; break;
                case EventType.MouseDrag:
                if (isFocused) {
                    isEditingPosition = !Event.current.IsRightMouseButton();
                    if (isEditingPosition) { // modify position on left-click
                        if (!Event.current.shift) {
                            transform.position = mainCamera.ScreenToWorldPoint(new Vector3(
                                Event.current.mousePosition.x,
                                mainCamera.pixelHeight - Event.current.mousePosition.y, // y-axis is inverted, for some reason
                                mainCamera.WorldToScreenPoint(transform.position).z
                            ));
                        } else /* if (Event.current.shift) */ { // move along Z-axis instead of Y-axis when shift is held
                            int Zsign = -1;
                            if (mainCamera.transform.position.z >= 0) Zsign = 1;
                            
                            Vector3 currentPos = mainCamera.WorldToScreenPoint(transform.position);
                            //float mouseVertPos = (mainCamera.pixelHeight - Event.current.mousePosition.y) / mainCamera.pixelHeight;
                            transform.position = mainCamera.ScreenToWorldPoint(new Vector3(
                                Event.current.mousePosition.x,
                                currentPos.y,
                                //currentPos.y - mainCamera.pixelHeight/(mainCamera.pixelHeight - Event.current.mousePosition.y)/2,
                                Zsign * (mainCamera.transform.position.z + Zsign*(currentPos.z * (mainCamera.pixelHeight - Event.current.mousePosition.y * 2)/mainCamera.pixelHeight))
                                //currentPos.z + ((transform.position.z - mainCamera.transform.position.z) * (mainCamera.pixelHeight - Event.current.mousePosition.y * 2)/mainCamera.pixelHeight) * 0.1f
                                /* currentPos.z + ((currentPos.z - transform.position.z) * (mainCamera.pixelHeight - Event.current.mousePosition.y * 2)/mainCamera.pixelHeight) */
                                /* (transform.position.z - mainCamera.transform.position.z) + (50 * (mainCamera.pixelHeight - (Event.current.mousePosition.y * 2))/mainCamera.pixelHeight) */
                            ));
                        }
                    } else { // modify rotation on right-click
                        Vector2 newDeltas = new(
                            Event.current.delta.y * Time.deltaTime * rotationSpeed,
                            Event.current.delta.x * Time.deltaTime * rotationSpeed
                        );
                        if (isFreeSpin) {
                            rotationDeltas[0] = (rotationDeltas[0] + newDeltas[0]) * 0.5f;
                            rotationDeltas[1] = (rotationDeltas[1] + newDeltas[1]) * 0.5f;
                        } else { rotationDeltas = newDeltas; }
                    }
                }
                break;
                
                default:
                case EventType.MouseUp: isFocused = false; break;
            }
        } // end of mouseEvent
        
        if (isFreeSpin) rotationDeltas *= (1-(friction * Time.deltaTime));
        transform.Rotate(
            -rotationDeltas[0],
            -rotationDeltas[1],
            0, Space.World // consistent - not relative to current rotation
        );
        if (!isFreeSpin) rotationDeltas = new(0,0);
        
        // this implementation is affected by the object's current rotation
        //transform.rotation *= Quaternion.AngleAxis(Event.current.delta.x * Time.deltaTime * rotationSpeed, Vector3.right);
        //transform.rotation *= Quaternion.AngleAxis(Event.current.delta.y * Time.deltaTime * rotationSpeed, Vector3.forward);
        //print(string.Format("deltas: {0} {1} | deltaTime: {2}", Event.current.delta.x, Event.current.delta.y, Time.deltaTime));
        
        return;
    }
}
