using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JyModule
{
    public class RoomFreeView : MonoBehaviour
    {
        public Transform MainCamera;

        private bool LoadingCheck = false;
        private string SavePath;
        private JySaveData saveData = new JySaveData();
        private bool ErrCheck = false;

        public GameObject BaseTile;
        //현재 아이템분기가 없으니 그냥 배열에 넣고 추후에 데이터 베이스상의 경로에서 로딩
        public GameObject[] ItemObj;

        [SerializeField]
        private Vector3Int RoomSize = Vector3Int.one;
        private Color BaseTileColor;
        private int CrateItemIndex = 0;

        private GameObject BaseItemBox;

        private void Awake()
        {
#if UNITY_EDITOR
            SavePath = Path.Combine(Application.dataPath, "database.json");
#else
            SavePath = Path.Combine(Application.persistentDataPath , "database.json");
#endif
            if (MainCamera == null)
                MainCamera = Camera.main.transform;

            StartCoroutine(LoadingMap());
        }

        private void FixedUpdate()
        {
            
        }

        IEnumerator LoadingMap()
        {
            var _yield = new WaitForEndOfFrame();
            Vector3 _posSetting = Vector3.zero;

            LoadingCheck = true;

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

            Transform _Bouttom = new GameObject("Bottom").transform;
            _Bouttom.parent = transform;
            _posSetting = Vector3.zero;

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
                    _Pos.x += temp.PosCorrect.x;
                    _Pos.y += temp.PosCorrect.y;
                    _Pos.z += temp.PosCorrect.z;
                    break;
                case 1: //Left Obverse
                    _Pos.x += temp.PosCorrect.x;
                    _Pos.y += temp.PosCorrect.y;
                    _Pos.z -= temp.PosCorrect.z;
                    break;
                case 2: //Right Obverse
                    _Pos.x += temp.PosCorrect.x;
                    _Pos.y += temp.PosCorrect.y;
                    _Pos.z -= temp.PosCorrect.z;
                    break;
                case 3: //Left Back
                    _Pos.x += temp.PosCorrect.x;
                    _Pos.y += temp.PosCorrect.y;
                    _Pos.z += temp.PosCorrect.z;
                    break;
                case 4: //Right back
                    _Pos.x += temp.PosCorrect.x;
                    _Pos.y += temp.PosCorrect.y;
                    _Pos.z -= temp.PosCorrect.z;
                    break;
            }
            _obj.transform.position = _Pos;
        }
    }

}
