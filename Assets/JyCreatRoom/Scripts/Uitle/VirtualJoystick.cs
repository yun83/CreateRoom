using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JyModule
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public Transform Stick;         // 조이스틱.

        public bool MoveFlag = false;
        public Vector3 JoyVec;         // 조이스틱의 벡터(방향)
                                       // 비공개.
        private Vector3 StickFirstPos;  // 조이스틱의 처음 위치
        private float Radius;           // 조이스틱 배경의 반 지름.
        public bool Run = false;
        /// <summary>
        /// 0 - 정지
        /// 1 - 워킹
        /// 2 - 런
        /// </summary>
        public int MoveType = 0;
        public int TouchId = -1;

        void Start()
        {
            Radius = GetComponent<RectTransform>().sizeDelta.y * 0.5f;
            StickFirstPos = Stick.transform.position;

            // 캔버스 크기에대한 반지름 조절.
            float Can = transform.parent.GetComponent<RectTransform>().localScale.x;
            Radius *= Can;

            MoveType = 0;
            MoveFlag = false;
        }

        // 드래그
        public void Drag(BaseEventData _Data)
        {
            MoveType = 0;
            MoveFlag = true;

            PointerEventData Data = _Data as PointerEventData;
            Vector3 Pos = Data.position;

            TouchId = Data.pointerId;

            // 조이스틱을 이동시킬 방향을 구함.(오른쪽,왼쪽,위,아래)
            JoyVec = (Pos - StickFirstPos).normalized;

            // 조이스틱의 처음 위치와 현재 내가 터치하고있는 위치의 거리를 구한다.
            float Dis = Vector3.Distance(Pos, StickFirstPos);
            if (Dis < Radius)
            {// 거리가 반지름보다 작으면 조이스틱을 현재 터치하고 있는곳으로 이동. 
                Stick.position = StickFirstPos + JoyVec * Dis;
                Run = false;
            }
            else
            {// 거리가 반지름보다 커지면 조이스틱을 반지름의 크기만큼만 이동.
                Stick.position = StickFirstPos + JoyVec * Radius;
                Run = true;
            }
        }

        // 드래그 끝.
        public void DragEnd()
        {
            Stick.position = StickFirstPos; // 스틱을 원래의 위치로.
            JoyVec = Vector3.zero;          // 방향을 0으로.
            MoveFlag = false;
            TouchId = -1;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TouchId = eventData.pointerId;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Stick.position = StickFirstPos; // 스틱을 원래의 위치로.
            JoyVec = Vector3.zero;          // 방향을 0으로.
            MoveFlag = false;
            TouchId = -1;
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveType = 0;
            MoveFlag = true;

            PointerEventData Data = eventData as PointerEventData;
            Vector3 Pos = Data.position;

            TouchId = eventData.pointerId;

            // 조이스틱을 이동시킬 방향을 구함.(오른쪽,왼쪽,위,아래)
            JoyVec = (Pos - StickFirstPos).normalized;

            // 조이스틱의 처음 위치와 현재 내가 터치하고있는 위치의 거리를 구한다.
            float Dis = Vector3.Distance(Pos, StickFirstPos);
            if (Dis < Radius)
            {// 거리가 반지름보다 작으면 조이스틱을 현재 터치하고 있는곳으로 이동. 
                Stick.position = StickFirstPos + JoyVec * Dis;
                Run = false;
            }
            else
            {// 거리가 반지름보다 커지면 조이스틱을 반지름의 크기만큼만 이동.
                Stick.position = StickFirstPos + JoyVec * Radius;
                Run = true;
            }
        }
    }
}