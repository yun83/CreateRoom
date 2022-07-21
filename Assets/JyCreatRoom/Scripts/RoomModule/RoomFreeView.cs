using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JyModule
{
    public class RoomFreeView : MonoBehaviour
    {
        public Transform MainCamera;
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public Transform Axis;

        private bool LoadingCheck = false;
        private string SavePath;
        private JySaveData saveData = new JySaveData();
        private bool ErrCheck = false;

        public GameObject BaseTile;
        //���� �����ۺбⰡ ������ �׳� �迭�� �ְ� ���Ŀ� ������ ���̽����� ��ο��� �ε�
        public GameObject[] ItemObj;

        [SerializeField]
        private Vector3Int RoomSize = Vector3Int.one;
        private int CrateItemIndex = 0;
        private Vector3 PosCorrect = Vector3.zero;

        private GameObject BaseItemBox;
        private Vector2 AxisRtt;
        public float RotationDisMin = -30;
        public float RotationDisMax = 70;
        public float rotateSpeed = 10;
        public float scrollSpeed = 1000;

        private void Awake()
        {
#if UNITY_EDITOR
            SavePath = Path.Combine(Application.dataPath, "database.json");
#else
            SavePath = Path.Combine(Application.persistentDataPath , "database.json");
#endif

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

            if (Input.GetMouseButton(1))
            {
                AxisRtt.y += Input.GetAxis("Mouse X") * rotateSpeed; // ���콺�� �¿� �̵����� xmove �� �����մϴ�.
                AxisRtt.x -= Input.GetAxis("Mouse Y") * rotateSpeed; // ���콺�� ���� �̵����� ymove �� �����մϴ�.
            }

            float scroollWheel = Input.GetAxis("Mouse ScrollWheel");
            //Debug.Log(scroollWheel);
            if (scroollWheel != 0)
            {
                Vector3 ComPos = MainCamera.localPosition;
                ComPos.z += scroollWheel * Time.deltaTime * scrollSpeed;
                MainCamera.localPosition = ComPos;
            }

            //ī�޶��� ȸ��
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

            Vector3 CenterPos = Vector3.zero;
            //ī�޶� ��ġ ����
            CenterPos.x = RoomSize.x / 2;
            CenterPos.y = RoomSize.z;
            CenterPos.z = RoomSize.z / 2;

            Axis.position = new Vector3(CenterPos.x, 0, CenterPos.z);
            AxisRtt.x = 45;

            yield return _yield;

            //Destroy(BaseTile.GetComponent<RoomTileEvnet>());

            Transform _Bouttom = new GameObject("Bottom").transform;
            _Bouttom.parent = transform;
            _posSetting = Vector3.zero;

            Transform Wall_BT = Instantiate(BaseTile).transform;
            Wall_BT.name = "Wall_BT";
            Wall_BT.localScale = new Vector3(RoomSize.x, 0.1f, RoomSize.z);
            PosCorrect.x = (RoomSize.x - 1) * 0.5f;
            PosCorrect.y = -0.1f;
            PosCorrect.z = (RoomSize.z - 1) * 0.5f;
            Wall_BT.localPosition = PosCorrect;
            Wall_BT.parent = _Bouttom;

            yield return _yield;

            for ( int i = 0; i < saveData.Bottom_Item.Count; i++)
            {
                GameObject _obj = Instantiate(ItemObj[saveData.Bottom_Item[i].itemId]);
                _obj.transform.parent = _Bouttom;
                ItemSetting(_obj, saveData.Bottom_Item[i], 0);
            }
            yield return _yield;

            Transform _LeftObverse = new GameObject("LeftObverse").transform;
            _LeftObverse.parent = transform;
            _posSetting = Vector3.zero;

            Transform Wall_LO = Instantiate(BaseTile).transform;
            Wall_LO.name = "Wall_LO";
            Wall_LO.localScale = new Vector3(RoomSize.x, RoomSize.y, 0.1f);
            Wall_LO.parent = _LeftObverse;
            PosCorrect = Vector3.zero;
            PosCorrect.x = (RoomSize.x - 1) * 0.5f;
            PosCorrect.y = (RoomSize.y) * 0.5f;
            PosCorrect.z = 0.5f;
            Wall_LO.localPosition = PosCorrect;

            yield return _yield;

            for (int i = 0; i < saveData.LeftObverse.Count; i++)
            {
                GameObject _obj = Instantiate(ItemObj[saveData.LeftObverse[i].itemId]);
                _obj.transform.parent = _LeftObverse;
                ItemSetting(_obj, saveData.LeftObverse[i], 1);
            }
            _posSetting.z = RoomSize.z - 1;
            _LeftObverse.position = _posSetting;

            yield return _yield;

            Transform _RightObverse = new GameObject("RightObverse").transform;
            _RightObverse.parent = transform;
            _posSetting = Vector3.zero;

            Transform Wall_RO = Instantiate(BaseTile).transform;
            Wall_RO.name = "Wall_RO";
            Wall_RO.localScale = new Vector3(RoomSize.z, RoomSize.y, 0.1f);
            Wall_RO.parent = _RightObverse;
            PosCorrect = Vector3.zero;
            PosCorrect.x = (RoomSize.z - 1) * 0.5f;
            PosCorrect.y = RoomSize.y * 0.5f;
            PosCorrect.z = 0.5f;
            Wall_RO.localPosition = PosCorrect;

            yield return _yield;

            for (int i = 0; i < saveData.RightObverse.Count; i++)
            {
                GameObject _obj = Instantiate(ItemObj[saveData.RightObverse[i].itemId]);
                _obj.transform.parent = _RightObverse;
                ItemSetting(_obj, saveData.RightObverse[i], 2);
            }
            _posSetting.x = RoomSize.x - 1;
            _posSetting.z = RoomSize.z - 1;

            _RightObverse.position = _posSetting;
            _RightObverse.eulerAngles = new Vector3(0, 90, 0);

            yield return _yield;

            Transform _LeftBack = new GameObject("LeftBack").transform;
            _LeftBack.parent = transform;
            _posSetting = Vector3.zero;

            Transform Wall_LB = Instantiate(BaseTile).transform;
            Wall_LB.name = "Wall_LB";
            Wall_LB.localScale = new Vector3(RoomSize.x, RoomSize.y, 0.1f);
            Wall_LB.parent = _LeftBack;
            PosCorrect = Vector3.zero;
            PosCorrect.x = (RoomSize.x - 1) * 0.5f;
            PosCorrect.y = RoomSize.y * 0.5f;
            PosCorrect.z = -0.5f;
            Wall_LB.localPosition = PosCorrect;

            yield return _yield;

            for (int i = 0; i < saveData.LeftBack.Count; i++)
            {
                GameObject _obj = Instantiate(ItemObj[saveData.LeftBack[i].itemId]);
                _obj.transform.parent = _LeftBack;
                ItemSetting(_obj, saveData.LeftBack[i], 3);
            }
            _LeftBack.position = _posSetting;

            yield return _yield;

            Transform _RightBack = new GameObject("RightBack").transform;
            _RightBack.parent = transform;
            _posSetting = Vector3.zero;

            Transform Wall_RB = Instantiate(BaseTile).transform;
            Wall_RB.name = "Wall_RB";
            Wall_RB.localScale = new Vector3(RoomSize.z, RoomSize.y, 0.1f);
            Wall_RB.parent = _RightBack;
            PosCorrect = Vector3.zero;
            PosCorrect.x = (RoomSize.z - 1) * 0.5f;
            PosCorrect.y = RoomSize.y * 0.5f;
            PosCorrect.z = 0.5f;
            Wall_RB.localPosition = PosCorrect;

            yield return _yield;

            for (int i = 0; i < saveData.RightBack.Count; i++)
            {
                GameObject _obj = Instantiate(ItemObj[saveData.RightBack[i].itemId]);
                _obj.transform.parent = _RightBack;
                ItemSetting(_obj, saveData.RightBack[i], 4);
            }
            _RightBack.position = _posSetting;
            _RightBack.eulerAngles = new Vector3(0, -90, 0);

            yield return _yield;

            LoadingCheck = false;
        }

        void ItemSetting(GameObject _obj, ItemLsit _data, int _type)
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

        public void OnClick_RoomEdit()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DemoVer1");
        }
    }

}
