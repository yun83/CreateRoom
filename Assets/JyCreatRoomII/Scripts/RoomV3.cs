using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JyModule
{
    public class RoomV3 : MonoBehaviour
    {
        private Camera MainCamera;
        public Transform BottomTrans;
        public Vector3 RoomSize = Vector3.zero;

        [Header("그룹")]
        public Transform ItemGroup;

        [Header("배치 타입")]
        public MapType useType = MapType.Bottom;
        public Dropdown DropdownMapType;

        public GameObject Sample;
        public Transform onTrans;
        private float sumPosition;

        private RaycastHit rayHit;
        private bool insItem = false;
        private Vector3 mousePos, transPos;

        [Header("배치 아이템의 아웃라인")]
        public Material TestMate;
        public Material outline;

        [Header("카메라의 이동 및 회전")]
        public VirtualJoystick vStick;
        public Transform Axis;
        private Vector2 AxisRtt = Vector2.zero;
        [Range(-50f, -10f)]
        public float RotationDisMin = -30;
        [Range(30f, 90f)]
        public float RotationDisMax = 70;
        public float rotateSpeed = 10;
        public float scrollSpeed = 50;
        public float ZoomDis = -20;
        [Range(-15f, 0f)]
        public float ZoomMin = -5;
        [Range(-70f, 90f)]
        public float ZoomMax = -80;
        private Vector3 ZoomVecter = Vector3.zero;

        private void Awake()
        {
            MainCamera = Camera.main;
            if (Axis == null)
            {
                Axis = new GameObject("Axis").transform;
                MainCamera.transform.parent = Axis;
                MainCamera.transform.localPosition = new Vector3(0, 0, -10);
            }
            vStick = FindObjectOfType<VirtualJoystick>();
            BottomTrans.localScale = RoomSize;
            AxisRtt.x = 45;

            outline = new Material(Shader.Find("Draw/OutlineShader"));
            outline.SetColor("_OutlineColor", new Color(1, 1, 1, 1f));

            if (ItemGroup != null)
                Destroy(ItemGroup.gameObject);
            ItemGroup = new GameObject("ItemGroup").transform;

            ItemGroup.parent = this.transform;
        }

        private void Update()
        {
            objectMove();
            CameraMove();
            OnChangeRoomSize();
        }

        void objectMove()
        {
            if (Input.GetMouseButton(0)) {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                Move_ItemObject();
            }

            if (insItem)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                        return;
                    UpMouse_ItemDrop();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    OnClick_ItemObject();
                }
            }
        }

        void CameraMove()
        {
            if (vStick == null)
                return;

            if (Input.GetMouseButton(1))
            {
                AxisRtt.y += Input.GetAxis("Mouse X") * rotateSpeed; // 마우스의 좌우 이동량을 xmove 에 누적합니다.
                AxisRtt.x -= Input.GetAxis("Mouse Y") * rotateSpeed; // 마우스의 상하 이동량을 ymove 에 누적합니다.
            }
            else
            {
                //화면 줌
                ZoomDis += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            }

            if (vStick.MoveFlag == false && Input.touchCount >= 2)
            {
                Touch touchZero = Input.GetTouch(0); //첫번째 손가락 터치를 저장
                Touch touchOne = Input.GetTouch(1); //두번째 손가락 터치를 저장

                //터치에 대한 이전 위치값을 각각 저장함
                //처음 터치한 위치(touchZero.position)에서 이전 프레임에서의 터치 위치와 이번 프로임에서 터치 위치의 차이를 뺌
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition; //deltaPosition는 이동방향 추적할 때 사용
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // 각 프레임에서 터치 사이의 벡터 거리 구함
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude; //magnitude는 두 점간의 거리 비교(벡터)
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // 거리 차이 구함(거리가 이전보다 크면(마이너스가 나오면)손가락을 벌린 상태_줌인 상태)
                if (prevTouchDeltaMag - touchDeltaMag > 0)
                    ZoomDis = scrollSpeed;
                else
                    ZoomDis = -scrollSpeed;
            }

            if (vStick.MoveFlag)
            {
                AxisRtt.y -= vStick.JoyVec.x * (rotateSpeed * 0.1f); // 마우스의 좌우 이동량을 xmove 에 누적합니다.
                AxisRtt.x += vStick.JoyVec.y * (rotateSpeed * 0.1f); // 마우스의 상하 이동량을 ymove 에 누적합니다.
                                                                     //Debug.Log(moveH + ":::" + moveV);
            }

            ZoomDis = Mathf.Clamp(ZoomDis, ZoomMax, ZoomMin); ;
            ZoomVecter.z = ZoomDis;
            MainCamera.transform.localPosition = ZoomVecter;

            //카메라의 회전
            Quaternion v3Rotation = new Quaternion();
            float minRttX = Mathf.Clamp(AxisRtt.x, RotationDisMin, RotationDisMax);
            AxisRtt.x = minRttX;
            v3Rotation.eulerAngles = new Vector3(minRttX, AxisRtt.y, 0.0f);
            Axis.rotation = v3Rotation;
        }

        void OnChangeRoomSize()
        {
            if(RoomSize != BottomTrans.localScale)
            {
                if (RoomSize.x <= 0)                    RoomSize.x = 0.01f;
                if (RoomSize.y <= 0)                    RoomSize.y = 0.01f;
                if (RoomSize.z <= 0)                    RoomSize.z = 0.01f;

                BottomTrans.localScale = RoomSize;
            }
        }

        void Move_ItemObject()
        {
            mousePos = Input.mousePosition;

            if (insItem)
            {
                Ray ray = MainCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out rayHit, 50))
                {
                    //Debug.Log(rayHit.point);
                    Debug.DrawLine(mousePos, rayHit.point);
                    if (rayHit.transform.gameObject != onTrans.gameObject)
                    {
                        transPos = rayHit.point;
                        transPos.y += sumPosition;

                        Vector3 changePos = Vector3Int.zero;

                        changePos.x = (int)transPos.x;
                        changePos.y = transPos.y;
                        changePos.z = (int)transPos.z;

                        onTrans.position = changePos;
                    }
                }
            }
        }

        void OnClick_ItemObject()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            mousePos = Input.mousePosition;

            Ray ray = MainCamera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out rayHit, 50))
            {
                if (rayHit.transform.TryGetComponent(out PiceData _pData)){
                    _pData.DownMouse();
                    onTrans = _pData.transform;
                    insItem = true;
                }
            }
        }

        void UpMouse_ItemDrop()
        {
            //오브젝트를 놓았을 경우
            if (onTrans.TryGetComponent(out PiceData _pData))
            {
                _pData.UpMouse();
            }

            onTrans = null;
            insItem = false;
        }

        public void OnClick_CreateItem(int _idx)
        {
            //오브젝트 생성
            Vector3 _objSize = Vector3.zero;
            GameObject _item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            PiceData _pData = _item.AddComponent<PiceData>();
            onTrans = _item.transform;

            if (_item.TryGetComponent(out BoxCollider _box))
            {
                _box.isTrigger = true;
            }

            /// 원래는 _idx 의 구분으로 실제 아이템을 받아 와야 하나 현재 아이템 관련된 정보가 없으므로 샘플로 대체
            _pData.DrawObject = Instantiate(Sample);
            if(_pData.DrawObject.TryGetComponent (out Collider _col))
            {
                _col.enabled = false;
            }
            /// 현재는 오브젝트의 사이즈를 가지고 오는데 추후 샘플의 스크립트에서 사이즈를 가져오자
            _objSize = _pData.DrawObject.transform.localScale;
            _pData.DrawObject.transform.parent = onTrans;

            _pData.Instantiate_Material(TestMate);

            _box.size = _objSize;
            sumPosition = (_objSize.y * 0.5f);

            _pData.ChangeColor(new Color(1, 1, 1, 0.2f));
            _pData.transform.parent = ItemGroup;
            insItem = true;
        }
        
        public void OnChangeMapType() {
            int _idx = DropdownMapType.value;
            switch (_idx)
            {
                case 0: useType = MapType.Bottom; break;
                case 1: useType = MapType.Obverse_Left_Wall; break;
                case 2: useType = MapType.Obverse_Right_Wall; break;
                case 3: useType = MapType.Back_Left_Wall; break;
                case 4: useType = MapType.Back_Right_Wall; break;
            }
        }
    }
}
