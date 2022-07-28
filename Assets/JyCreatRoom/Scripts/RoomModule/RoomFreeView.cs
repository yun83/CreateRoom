using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JyModule
{
    public class RoomFreeView : MonoBehaviour
    {
        public Transform MainCamera;
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public Transform Axis;

        private bool LoadingCheck = true;
        private string SavePath;
        private JySaveData saveData = new JySaveData();
        private bool ErrCheck = false;

        public VirtualJoystick vStick;

        public GameObject BaseTile;
        //현재 아이템분기가 없으니 그냥 배열에 넣고 추후에 데이터 베이스상의 경로에서 로딩
        public GameObject[] ItemObj;

        [SerializeField]
        private Vector3Int RoomSize = Vector3Int.one;
        private int CrateItemIndex = 0;
        private Vector3 PosCorrect = Vector3.zero;

        private Vector2 AxisRtt;
        public float RotationDisMin = -30;
        public float RotationDisMax = 70;
        public float rotateSpeed = 10;
        public float scrollSpeed = 1000;
        public float ZoomDis = -20;
        public float ZoomMin = -5;
        public float ZoomMax = -80;
        private Vector3 ZoomVecter = Vector3.zero;

        private void Awake()
        {
#if UNITY_EDITOR
            SavePath = Path.Combine(Application.dataPath, "database.json");
#else
            SavePath = Path.Combine(Application.persistentDataPath , "database.json");
#endif
            vStick = FindObjectOfType<VirtualJoystick>();
            StartCoroutine(LoadingMap());
        }

        private void FixedUpdate()
        {
            CameraMove();
        }

        void CameraMove()
        {
            if (LoadingCheck)
                return;
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
                AxisRtt.y -= vStick.JoyVec.x * (rotateSpeed*0.1f); // 마우스의 좌우 이동량을 xmove 에 누적합니다.
                AxisRtt.x += vStick.JoyVec.y * (rotateSpeed*0.1f); // 마우스의 상하 이동량을 ymove 에 누적합니다.
                //Debug.Log(moveH + ":::" + moveV);
             }

            ZoomDis = Mathf.Clamp(ZoomDis, ZoomMax, ZoomMin); ;
            ZoomVecter.z = ZoomDis;
            MainCamera.localPosition = ZoomVecter;

            //카메라의 회전
            Quaternion v3Rotation = new Quaternion();
            float minRttX = Mathf.Clamp(AxisRtt.x, RotationDisMin, RotationDisMax);
            AxisRtt.x = minRttX;
            v3Rotation.eulerAngles = new Vector3(minRttX, AxisRtt.y, 0.0f);
            Axis.rotation = v3Rotation;
        }

        IEnumerator LoadingMap()
        {
            var _yield = new WaitForEndOfFrame();
            Vector3 _posSetting = Vector3.zero;

            LoadingCheck = true;

            if (MainCamera == null)
                MainCamera = Camera.main.transform;

            if (Axis == null)
            {
                Axis = new GameObject("Axis").transform;
                MainCamera.parent = Axis;
                MainCamera.localPosition = new Vector3(0, 0, -10);
            }
            yield return _yield;

            saveData.Bottom_Item.Clear();
            saveData.LeftObverse.Clear();
            saveData.RightObverse.Clear();
            saveData.LeftBack.Clear();
            saveData.RightBack.Clear();

            yield return _yield;
            if (File.Exists(SavePath))
            {
                string loadJson = File.ReadAllText(SavePath);
                saveData = JsonUtility.FromJson<JySaveData>(loadJson);

                RoomSize = saveData.MapSize;
            }
            else
            {
                Debug.Log("Non File :: " + SavePath);
                ErrCheck = true;
            }

            yield return _yield;

            //카메라 위치 조절
            Vector3 CenterPos = Vector3.zero;
            CenterPos.x = RoomSize.x * 0.5f;
            CenterPos.y = RoomSize.z;
            CenterPos.z = RoomSize.z * 0.5f;

            Axis.position = new Vector3(CenterPos.x, 0, CenterPos.z);
            AxisRtt.x = 45;

            yield return _yield;

            Vector3 _Scale = new Vector3(RoomSize.x, 0.1f, RoomSize.z);
            Vector3 _wallCenter = new Vector3((RoomSize.x - 1) * 0.5f, -0.1f, (RoomSize.z - 1) * 0.5f);
            Transform _group = InstansGroupCreate("Bottom", _Scale, _wallCenter);

            yield return _yield;

            ItemGroupCreate(saveData.Bottom_Item, _group, 0);

            _posSetting = Vector3.zero;
            yield return _yield;

            _Scale = new Vector3(RoomSize.x, RoomSize.y, 0.1f);
            _wallCenter = new Vector3((RoomSize.x - 1) * 0.5f, (RoomSize.y) * 0.5f, 0.5f);
            _group = InstansGroupCreate("LeftObverse", _Scale, _wallCenter);

            yield return _yield;

            ItemGroupCreate(saveData.LeftObverse, _group, 1);

            _posSetting = Vector3.zero;
            _posSetting.z = RoomSize.z - 1;
            _group.position = _posSetting;

            yield return _yield;

            _Scale = new Vector3(RoomSize.z, RoomSize.y, 0.1f);
            _wallCenter = new Vector3((RoomSize.z - 1) * 0.5f, (RoomSize.y) * 0.5f, 0.5f);
            _group = InstansGroupCreate("RightObverse", _Scale, _wallCenter);

            yield return _yield;

            ItemGroupCreate(saveData.RightObverse, _group, 2);

            _posSetting = Vector3.zero;
            _posSetting.x = RoomSize.x - 1;
            _posSetting.z = RoomSize.z - 1;

            _group.position = _posSetting;
            _group.eulerAngles = new Vector3(0, 90, 0);

            yield return _yield;

            _Scale = new Vector3(RoomSize.x, RoomSize.y, 0.1f);
            _wallCenter = new Vector3((RoomSize.x - 1) * 0.5f, (RoomSize.y) * 0.5f, -0.5f);
            _group = InstansGroupCreate("LeftBack", _Scale, _wallCenter);

            yield return _yield;

            ItemGroupCreate(saveData.LeftBack, _group, 3);
            _posSetting = Vector3.zero;
            _group.position = _posSetting;

            yield return _yield;

            _Scale = new Vector3(RoomSize.z, RoomSize.y, 0.1f);
            _wallCenter = new Vector3((RoomSize.z - 1) * 0.5f, (RoomSize.y) * 0.5f, 0.5f);
            _group = InstansGroupCreate("RightBack", _Scale, _wallCenter);

            yield return _yield;

            ItemGroupCreate(saveData.RightBack, _group, 4);
            _group.position = _posSetting;
            _group.eulerAngles = new Vector3(0, -90, 0);

            yield return _yield;

            LoadingCheck = false;
        }

        void ItemSetting(GameObject _obj, ItemLsit _data, int _type )
        {
            PlacementManger temp;
            if (_obj.TryGetComponent(out PlacementManger rnd))
                temp = rnd;
            else
                temp = _obj.AddComponent<PlacementManger>();

            temp.ItemId = _data.itemId;
            temp.ItemLayerId = _data.layerId;
            temp.inPutPos = _data.inPutPos;
            temp.ObjSize = _data.Size;
            temp.ChangeObjectColor(_data.mColor);

            temp.roomManager = null;
            temp.initBoxSizeCheck();
            temp.PlacementID = CrateItemIndex;

            _obj.name = "ID : " + temp.ItemId + " / Layer : " + temp.ItemLayerId;
            CrateItemIndex++;

            Vector3 _Pos = temp.inPutPos;
            switch (_type)
            {
                case 0: //Bottom
                case 3: //Left Back
                    _Pos.x += temp.PosCorrect.x;
                    _Pos.y += temp.PosCorrect.y;
                    _Pos.z += temp.PosCorrect.z;
                    break;
                case 1: //Left Obverse
                case 2: //Right Obverse
                case 4: //Right back
                    _Pos.x += temp.PosCorrect.x;
                    _Pos.y += temp.PosCorrect.y;
                    _Pos.z -= temp.PosCorrect.z;
                    break;
            }
            _obj.transform.position = _Pos;
        }

        Transform InstansGroupCreate(string getName, Vector3 getScale, Vector3 getCorrect)
        {
            Transform _tempGroup = new GameObject(getName).transform;
            _tempGroup.parent = transform;

            Transform Wall_RB = Instantiate(BaseTile).transform;
            Wall_RB.name = getName + "Wall";
            Wall_RB.localScale = getScale;
            Wall_RB.parent = _tempGroup;
            Wall_RB.localPosition = getCorrect;

            return _tempGroup;
        }

        void ItemGroupCreate(List<ItemLsit> _data, Transform _group, int _idx)
        {
            for (int i = 0; i < _data.Count; i++)
            {
                GameObject _obj = Instantiate(ItemObj[_data[i].itemId]);
                _obj.transform.parent = _group;
                ItemSetting(_obj, _data[i], _idx);
            }
        }

        public void OnClick_RoomEdit()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DemoVer1");
        }
    }

}
