using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JyModule
{
    public class RoomTileEvnet : MonoBehaviour
    {
        public Vector3Int MyPos = Vector3Int.zero;

        public RoomV1_Manager roomManager = null;
        public Transform thisTrans;
        public Collider thisCollider;
        public bool CheckMouseUp = false;

        private void Awake()
        {
            thisCollider = GetComponent<Collider>();
            thisTrans = transform;
        }

        private void OnMouseEnter()
        {
            CheckMouseUp = true;
            //Debug.Log("마우스가 오브젝트에 진입.");
            if (roomManager == null)
                return;
            roomManager.OnMouse_EnterEvent_Bottom(this);
        }

        private void OnMouseExit()
        {
            CheckMouseUp = false;
            //Debug.Log("마우스가 오브젝트를 벗어남.");
        }

        private void OnMouseUp()
        {
            if (roomManager == null)
                return;
            //print("마우스가 오브젝트를 놓았습니다.");
            roomManager.OnMouse_UpEvent_Bottom(this);
        }
    }
}
