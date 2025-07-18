using UnityEngine;
using UnityEditor; // Handles
using UnityEngine.InputSystem;


// attach this script to the scene's camera
public class CameraController: MonoBehaviour
{
    private Camera mainCamera;
    struct TransformVectors {
        public Vector3 up;
        public Vector3 right;
        public Vector3 forward;
    }
    TransformVectors originalTransform;
    public bool keybindsActive = false;
    public bool objSelectionMode = false;
    public GameObject trackObject = null;
    private GameObject hoveredObject = null;
    private HandlesBetter tempHandles = null;
    
    public Vector3 orbit_point;
    public bool drawOrbit = true;
    [Range(0.001f, 100.0f)] public float orbit_radius = 50.0f;
    [Range(-360.0f, 360f)] public float angle = 0.0f;
    [Range(-50.0f, 50.0f)] public float elevation = 0.0f;
    
    Vector3 originalPosition;
    Vector3 momentum;
    float angularMomentum;
    public bool applyMomentum = false;
    [Range(0.025f, 1.0f)] public float speed = 0.25f; // for keyboard inputs
    [Range(0, 2.0f)] public float friction = 1f;
    
    public bool autoOrbit = false;
    [Range(-10.0f, 10.0f)] public float auto_orbit_speed = 0.0f;
    
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // somehow copying the transform copies the entire camera. Thanks unity.
        //originalTransform = Transform.Instantiate(mainCamera.transform); // copy transform, avoiding reference
        originalTransform = new TransformVectors(){};
        originalTransform.up = mainCamera.transform.up;
        originalTransform.right = mainCamera.transform.right;
        originalTransform.forward = mainCamera.transform.forward;
        orbit_point = mainCamera.transform.position + new Vector3(0, 0, orbit_radius);
        
        momentum = new Vector3(0, 0, 0);
        angularMomentum = 0;
        
        // add callback to textinput event
        Keyboard.current.onTextInput += TextInputCallback;
        originalPosition = transform.position;
        
        return;
    }
    
    
    void LateUpdate()
    {
        if (trackObject && !objSelectionMode) {
            orbit_point = trackObject.transform.position;
        }
        
        if (applyMomentum) {
            momentum *= (1-(friction * Time.deltaTime));
            angularMomentum *= (1-(friction * Time.deltaTime));
            angle += angularMomentum;
            orbit_point += momentum;
        } else {
            orbit_point += momentum * 10;
            angle += angularMomentum * 10;
            momentum = new Vector3(0,0,0);
            angularMomentum = 0;
        }
        
        if (autoOrbit) angle += auto_orbit_speed;
        angle %= 360;
        
        float radians = (angle/360.0f) * (2 * 3.14f);
        mainCamera.transform.position = orbit_point + new Vector3(
            orbit_radius * (float)System.Math.Sin(radians),
            elevation, //originalPosition.y,
            -orbit_radius * (float)System.Math.Cos(radians)
        );
        
        mainCamera.transform.LookAt(orbit_point);
        return;
    }
    
    void OnRenderObject()
    {
        if (!drawOrbit) return;
        Handles.color = Color.skyBlue;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual; // enables occlusion
        
        float centerSize = 1f;
        Handles.DrawWireDisc(orbit_point, originalTransform.forward, centerSize);
        Handles.DrawWireDisc(orbit_point, originalTransform.right, centerSize);
        Handles.DrawWireDisc(orbit_point, originalTransform.up, centerSize);
        
        // visual center
        Handles.color = Color.blue;
        if (trackObject) Handles.color = Color.softGreen;
        Vector3 labelPosition = orbit_point + new Vector3(1, 1, 0);
        Handles.Label(labelPosition, orbit_point.ToString());
        
        // visualizing orbit radius
        Handles.DrawWireDisc(orbit_point, originalTransform.up, orbit_radius);
        return;
    }
    
    // if it lags whenever you hold a key, you need to change the 'Interaction Mode' setting; minimize throttling for both
    private void TextInputCallback(char ch)
    {
        if ((!keybindsActive) && (ch != ' ')) return;
        float speed_ = (Keyboard.current.shiftKey.isPressed? (speed*10) : speed);
        // TODO: pressing shift should not break key-repeat
        
        switch (char.ToLower(ch))
        {
            case ' ': keybindsActive = !keybindsActive; break;
            case 'm': applyMomentum = !applyMomentum; break;
            case 'f': {
                if (objSelectionMode) trackObject = null; // unset when toggled on/off without new selection
                objSelectionMode = !objSelectionMode;
                break;
            }
            
            case 'r': // reset
                angle = 0; elevation = 0; orbit_radius = 50;
                autoOrbit = false; auto_orbit_speed = 0;
                orbit_point = originalPosition;
                momentum = new Vector3(0,0,0);
                angularMomentum = 0.0f;
                trackObject = null;
            break;
            
            // movement
            case 'w': momentum +=  speed_ * mainCamera.transform.forward; break; // forward
            case 's': momentum += -speed_ * mainCamera.transform.forward; break; // back
            case 'a': momentum += -speed_ * mainCamera.transform.right; break; // left
            case 'd': momentum +=  speed_ * mainCamera.transform.right; break; // right
            
            // elevation
            case 'q': momentum += new Vector3(0,-speed_, 0); break; // down
            case 'e': momentum += new Vector3(0, speed_, 0); break; // up
            
            // rotation
            case 'j': angularMomentum -= speed_; break;
            case 'l': angularMomentum += speed_; break;
            
            // camera-elevation (seperate from orbit-point elevation)
            case 'i': elevation += speed_ * 10; break;
            case 'k': elevation -= speed_ * 10; break;
            
            // orbit-radius
            case 'u': orbit_radius -= speed_ * 10; break;
            case 'o': orbit_radius += speed_ * 10; break;
            
            // movespeed. note that the sliders' ranges don't actually prevent the values from going beyond their limits
            case '1': { speed -= 0.025f; if (speed < 0.025f) speed = 0.025f; } break;
            case '2': { speed += 0.025f; if (speed > 1.0f) speed = 1.0f; } break;
            
            // auto_orbit_speed
            case '3': { autoOrbit = true; auto_orbit_speed -= 0.1f; if (auto_orbit_speed < -10.0f) auto_orbit_speed = -10.0f; } break;
            case '4': { autoOrbit = true; auto_orbit_speed += 0.1f; if (auto_orbit_speed >  10.0f) auto_orbit_speed =  10.0f; } break;
            
            default: /* print(string.Format("unhandled keypress: {0}", ch)); */ break;
        }
    }
    
    
    void FindHoveredObject()
    {
        // note: objects must have an (enabled) collider to interact with raycasting
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit)) {
            if ((hoveredObject == null) || !hoveredObject.Equals(hit.collider.gameObject)) {
                hoveredObject = hit.collider.gameObject;
                if (tempHandles) Destroy(tempHandles); // when hovered object changes, need to destroy old temphandles before updating
                    tempHandles = hoveredObject.GetComponent<HandlesBetter>();
                if (tempHandles == null) tempHandles = hoveredObject.AddComponent<HandlesBetter>();
                else tempHandles = null; // if the object already has a 'HandlesBetter' script, discard the reference to avoid destroying it later
            }
            return;
        }
        
        // nothing hovered
        if (tempHandles) Destroy(tempHandles);
        hoveredObject = null;
        tempHandles = null;
        return;
    }
    
    
    // handling mouse events for objSelectionMode
    void OnGUI()
    {
        if (!objSelectionMode) return;
        
        FindHoveredObject();
        if (!hoveredObject) return;
        
        if (Event.current.isMouse)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (tempHandles) Destroy(tempHandles);
                    trackObject = hoveredObject;
                    objSelectionMode = false;
                break;
                
                // stay in objSelectionMode when nothing has been selected
                default: objSelectionMode = (trackObject == null); break;
            }
        }
        
    }
}
