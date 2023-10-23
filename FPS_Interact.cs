using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum InteractState
{
    Ray,
    Viewer,
    Inventory,
    Null
}

public class FPS_Interact : MonoBehaviour
{
    [SerializeField] private InteractState myState;
    private InteractState previousState;

    //INTERACT
    private Object_Sim scrHit;
    [SerializeField] private LayerMask interLayer;
    [SerializeField] private float interactDistance, throwMaxForce;
    public float throwForce;

    [SerializeField] private Transform carryPos;
    [SerializeField] private Transform pickedObj;
    private Rigidbody pickedRig;
    private FixedJoint handJoint;

    //INSPECT
    [SerializeField] private Transform inspectPos;
    [SerializeField] private float inspectSpeed;

    //FLASH
    [SerializeField] private Transform flashPos;
    private Sim_Flashlight scrFlash = null;

    private void Start()
    {
        MainManager.Input.OnLMBPressed += LMBPress;
        MainManager.Input.OnRMBPressed += RMBPress;
        MainManager.Input.OnDrop += DropPress;
        MainManager.Input.OnDropHeld += DropHold;
        MainManager.Input.OnFlashlight += FlashPress;

        handJoint = carryPos.GetComponent<FixedJoint>();
        MainManager.UI.SetupSliderThrow(throwMaxForce);
    }

    private void LMBPress()
    {
        if (scrHit != null && myState == InteractState.Ray)
        {
            if(scrHit.isStimable)
                scrHit.Click();
        }
    }

    private void RMBPress()
    {
        if (pickedObj != null && myState == InteractState.Ray)
        {
            Sim_Pickable scrObj = pickedObj.GetComponent<Sim_Pickable>();
            if(scrObj.isInspectable)
            {
                previousState = myState;
                Inspect();
            }
        }
        else if (myState == InteractState.Viewer)
        {
            ReturnFromInspect();
        }
    }

    private void DropHold()
    {
        if (pickedObj != null && myState == InteractState.Ray)
        {
            throwForce += 700 * Time.deltaTime;
            MainManager.UI.ThrowUpdate(throwForce);
        }
        else
        {
            throwForce = 0;
            MainManager.UI.ThrowUpdate(throwForce);
        }
    }

    private void DropPress()
    {
        if (pickedObj != null && myState == InteractState.Ray)
            DropObj();
    }

    private void FlashPress()
    {
        if (scrFlash == null)
            return;

        scrFlash.ToggleLight();
    }

    void Update()
    {
        switch(myState)
        {
            case InteractState.Ray:
                RaycastWorld();
                break;
            case InteractState.Viewer:
                RotateModel();
                break;
            case InteractState.Inventory:
                //BrowseInventory();
                break;
            default:
                break;
        }
    }

    public void RotateCamera(Vector3 newRot, float time)
    {
        transform.DOLocalRotate(newRot, time);
    }

    private void ChangeState(InteractState newState)
    {
        myState = newState;
    }

    private void ReturnFromInspect()
    {
        if (previousState == InteractState.Inventory)
        {
            //MainManager.Inventory.ReturnFromInspect();
            //MainManager.UI.ToggleCursor(CursorType.Free);
        }
        else if (previousState == InteractState.Ray)
        {
            MoveObjectToCarry(false);
            MainManager.Player.PlayerControl(ControlType.Full);
            MainManager.UI.ToggleCursor(CursorType.Center);
        }

        ChangeState(previousState);
    }

    //INTERACT

    private void RaycastWorld()
    {
        RaycastHit Hit;
        if (Physics.Raycast(transform.position, transform.forward, out Hit, interactDistance, interLayer, QueryTriggerInteraction.Collide))
        {
            HoverRay(Hit.transform);
        }
        else
        {
            scrHit = null;
        }
    }

    private void HoverRay(Transform target)
    {
        scrHit = target.GetComponent<Object_Sim>();

        if (scrHit != null && scrHit.isStimable)
        {
            scrHit.Stim();
        }
    }

    public void Pickup(Transform obj)
    {
        if (pickedObj != null)
            DropObj();

        pickedObj = obj;
        pickedRig = obj.GetComponent<Rigidbody>();

        MoveObjectToCarry(true);
    }

    private void MoveObjectToCarry(bool firstTime)
    {
        Collider col = pickedObj.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        pickedRig.isKinematic = true;
        handJoint.connectedBody = null;

        pickedObj.parent = carryPos;
        pickedObj.DOLocalMove(Vector3.zero, 0.4f).OnComplete(FinalizeJoint);

        if(firstTime)
        {
            Sim_Pickable scrObj = pickedObj.GetComponent<Sim_Pickable>();
            pickedObj.DOLocalRotate(scrObj.defaultRot, 0.4f);
        }
    }

    private void FinalizeJoint()
    {
        pickedRig.isKinematic = false;
        handJoint.connectedBody = pickedRig;
    }

    private void DropObj()
    {
        float throwMinForce = 100f * pickedRig.mass;
        throwForce = Mathf.Clamp(throwForce * pickedRig.mass, throwMinForce, throwMaxForce);

        pickedObj.parent = MainManager.Player.rigidParent;
        pickedRig.isKinematic = false;
        handJoint.connectedBody = null;
        pickedRig.AddForce(transform.forward * throwForce);
        Debug.Log("Thrown with " + throwForce);

        pickedObj = null;
        throwForce = 0f;
        MainManager.UI.ThrowUpdate(throwForce);
    }

    //INSPECT

    private void Inspect()
    {
        MainManager.Player.PlayerControl(ControlType.Frozen);
        MainManager.UI.ToggleCursor(CursorType.Invisible);
        ChangeState(InteractState.Viewer);

        MoveObjectToInspect();
    }

    private void MoveObjectToInspect()
    {
        Collider col = pickedObj.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        pickedRig.isKinematic = true;
        handJoint.connectedBody = null;

        pickedObj.parent = inspectPos;
        pickedObj.DOLocalMove(Vector3.zero, 0.2f);
    }

    private void RotateModel()
    {
        float mouseX = Input.GetAxis("Mouse X") * inspectSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * inspectSpeed;
        Vector3 objRot = new Vector3(mouseY, -mouseX, 0);
        objRot *= Time.deltaTime;

        if (mouseX != 0 || mouseY != 0)
        {
            pickedObj.Rotate(transform.TransformDirection(objRot), Space.World);
        }
    }

    //FLASH
    public void AttachFlashlight(Transform obj)
    {
        obj.parent = flashPos;
        obj.DOLocalMove(Vector3.zero, 0.4f);
        obj.DOLocalRotate(Vector3.zero, 0.4f);

        scrFlash = obj.GetComponent<Sim_Flashlight>();
    }
}
