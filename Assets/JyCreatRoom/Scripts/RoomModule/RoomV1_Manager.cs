using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JyModule
{
    public class RoomV1_Manager : MonoBehaviour
    {
        public Transform MainCamera;
        private string SavePath;
        private JySaveData saveData = new JySaveData();

        [Header("�� ũ��� �ٴ� ���� ����")]
        public Vector3Int RoomSize = Vector3Int.one;
        public Color BaseTileColor;
        public GameObject BaseTile;
        public GameObject BaseItemBox;

        [Header("��ġ�� ������ ��������")]
        public Color CanPutColor;
        public Color NotPutColor;

        [Header("��ġ Ÿ��")]
        public MapType useType = MapType.Bottom;
        private Transform GroundTrans;
        private Transform ItemGroup;

        private bool LoadingCheck = true;
        //�����Ϳ��� �ǽð� ������ Ȯ���ϱ� ���� üũ�� ����
        private Color subColor;
        private Vector3Int subSize = Vector3Int.one;
        /// <summary>
        /// true �϶� ������Ʈ ��� �������
        /// </summary>
        public bool HandUpObj = false;
        private bool ObjectDelete = false;

        //�� üũ�ϱ� ���� �迭
        public List<int[]> MapCheckList = new List<int[]>();

        //���� �迭 ������ŭ�� �ٴ� ����
        private List<BaseTileClass> baseClassList = new List<BaseTileClass>();

        //������ ��ġ�� ������Ʈ�� ����
        public List<PlacementManger> CreateItemList = new List<PlacementManger>();
        private int cpCount = 0;

        //ī�޶� ȸ�� ����
        public Transform LookTargetObj;
        public float rotateSpeed = 100;
        public float scrollSpeed = 1000;
        public float ZoomMin = -5;
        public float ZoomMax = -80;
        //���� ���õ� ������Ʈ�� �����͸� Ȯ���ϱ�����
        public PlacementManger InsPlacement = null;
        public VirtualJoystick vStick;

        private Vector2 AxisRtt;
        public float ZoomDis = -20;
        private Vector3 ZoomVecter = Vector3.zero;

        [Serializable]
        public class BaseTileClass
        {
            public RoomTileEvnet RTE;
            public Material Mat;
        }

        private void Awake()
        {
#if UNITY_EDITOR
            SavePath = Path.Combine(Application.dataPath, "database.json");
#else
            SavePath = Path.Combine(Application.persistentDataPath , "database.json");
#endif
            if (MainCamera == null)
                MainCamera = Camera.main.transform;
            if (LookTargetObj == null)
            {
                LookTargetObj = new GameObject("Axis").transform;
            }
            MainCamera.parent = LookTargetObj;
            MainCamera.localRotation = Quaternion.identity;

            vStick = FindObjectOfType<VirtualJoystick>();

            saveData.Bottom_Item.Clear();
            saveData.LeftObverse.Clear();
            saveData.RightObverse.Clear();
            saveData.LeftBack.Clear();
            saveData.RightBack.Clear();
            LoadingCheck = true;
            ObjectDelete = false;

            InitPage();
            LoadRoomData();
        }

        void InitPage()
        {
            CreateItemList.Clear();
        }

        public void LoadRoomData()
        {
            if (File.Exists(SavePath))
            {
                string loadJson = File.ReadAllText(SavePath);
                saveData = JsonUtility.FromJson<JySaveData>(loadJson);

                RoomSize = saveData.MapSize;
                BaseTileColor = saveData.BaseColor;
                CanPutColor = saveData.CanColor;
                NotPutColor = saveData.NotColor;
            }

            if(LoadingCheck)
                StartCoroutine(ListReSizeing());
        }

        void SaveDataIndexSetting(List<ItemLsit> _list)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                cpCount++;
                //���Ŀ��� ������ ���̵�� �ؼ� �ε� �Ҽ� �ְڲ� ����
                GameObject _obj = Instantiate(BaseItemBox);
                PlacementManger temp;
                if (_obj.TryGetComponent(out PlacementManger rnd))
                    temp = rnd;
                else
                    temp = _obj.AddComponent<PlacementManger>();

                _obj.transform.parent = ItemGroup;

                temp.ItemId = _list[i].itemId;
                temp.ItemLayerId = _list[i].layerId;
                temp.inPutPos = _list[i].inPutPos;
                temp.ObjSize = _list[i].Size;
                temp.ChangeObjectColor(_list[i].mColor);

                temp.roomManager = this;
                temp.initBoxSizeCheck();
                temp.PlacementID = cpCount;

                _obj.name = "ID : " + temp.ItemId + " / Layer : " + temp.ItemLayerId;

                Vector3 _Pos = temp.inPutPos;
                _Pos.x += temp.PosCorrect.x;
                _Pos.y += temp.PosCorrect.y;
                _Pos.z += temp.PosCorrect.z;
                _obj.transform.position = _Pos;

                CreateItemList.Add(temp);
            }
        }

        private void FixedUpdate()
        {
            //������ �󿡼� ������ ���� ���� �ٷ� ǥ�⸦ ���ؼ� ������Ʈ���� üũ.
            //���� ��ư Ŭ�� ���� Ÿ �̺�Ʈ �߻��÷� ����
            if (subSize != RoomSize)
            {
                if(LoadingCheck)
                    StartCoroutine(ListReSizeing());
            }

            //������ �󿡼� �ٴ� Ÿ���� ������ �ٷ� Ȯ�� ���ؼ� ������Ʈ �󿡼� üũ.
            //���� ���峪 �׽�Ʈ �Ϸ�� ����
            if (subColor != BaseTileColor)
                OnChangdeColor();

            CameraMouseMove();
        }

        void CameraMouseMove()
        {
            if (vStick == null)
                return;

            if (Input.GetMouseButton(1))
            {
                AxisRtt.y += Input.GetAxis("Mouse X") * rotateSpeed; // ���콺�� �¿� �̵����� xmove �� �����մϴ�.
                AxisRtt.x -= Input.GetAxis("Mouse Y") * rotateSpeed; // ���콺�� ���� �̵����� ymove �� �����մϴ�.
            }
            else
            {
                //ȭ�� ��
                ZoomDis += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            }

            if (vStick.MoveFlag == false && Input.touchCount >= 2)
            {
                Touch touchZero = Input.GetTouch(0); //ù��° �հ��� ��ġ�� ����
                Touch touchOne = Input.GetTouch(1); //�ι�° �հ��� ��ġ�� ����

                //��ġ�� ���� ���� ��ġ���� ���� ������
                //ó�� ��ġ�� ��ġ(touchZero.position)���� ���� �����ӿ����� ��ġ ��ġ�� �̹� �����ӿ��� ��ġ ��ġ�� ���̸� ��
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition; //deltaPosition�� �̵����� ������ �� ���
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // �� �����ӿ��� ��ġ ������ ���� �Ÿ� ����
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude; //magnitude�� �� ������ �Ÿ� ��(����)
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // �Ÿ� ���� ����(�Ÿ��� �������� ũ��(���̳ʽ��� ������)�հ����� ���� ����_���� ����)
                if (prevTouchDeltaMag - touchDeltaMag > 0)
                    ZoomDis = scrollSpeed;
                else
                    ZoomDis = -scrollSpeed;
            }

            if (vStick.MoveFlag)
            {
                AxisRtt.y -= vStick.JoyVec.x * (rotateSpeed * 0.1f); // ���콺�� �¿� �̵����� xmove �� �����մϴ�.
                AxisRtt.x += vStick.JoyVec.y * (rotateSpeed * 0.1f); // ���콺�� ���� �̵����� ymove �� �����մϴ�.
            }

            ZoomDis = Mathf.Clamp(ZoomDis, ZoomMax, ZoomMin); ;
            ZoomVecter.z = ZoomDis;
            MainCamera.localPosition = ZoomVecter;

            //ī�޶��� ȸ��
            Quaternion v3Rotation = new Quaternion();
            v3Rotation.eulerAngles = new Vector3(AxisRtt.x, AxisRtt.y, 0.0f);
            LookTargetObj.rotation = v3Rotation;
        }

        void ArrangementMode(int OnMode)
        {
            switch (OnMode)
            {
                case 0: //�ƹ� �͵� ���ϰ� �ִ� ����
                    for (int i = 0; i < baseClassList.Count; i++)
                    {//Ÿ���� ���콺 �̺�Ʈ ����
                        baseClassList[i].RTE.thisCollider.enabled = false;
                    }
                    for (int i = 0; i < CreateItemList.Count; i++)
                    {//������Ʈ Ŭ�� Ȱ��ȭ
                        CreateItemList[i].thisCollider.enabled = true;
                    }
                    break;
                case 1:
                    for (int i = 0; i < baseClassList.Count; i++)
                    {//Ÿ���� ���콺 �̺�Ʈ Ȱ��ȭ
                        baseClassList[i].RTE.thisCollider.enabled = true;
                    }
                    for (int i = 0; i < CreateItemList.Count; i++)
                    {//������Ʈ Ŭ�� Ȱ��ȭ
                        CreateItemList[i].thisCollider.enabled = false;
                    }
                    InsPlacement.thisCollider.enabled = false;
                    break;
            }
        }

        IEnumerator ListReSizeing()
        {
            var _yield = new WaitForEndOfFrame();
            LoadingCheck = false;

            if (ItemGroup != null)
                Destroy(ItemGroup.gameObject);

            yield return _yield;

            if (ItemGroup == null)
                ItemGroup = new GameObject("ItemGroup").transform;

            yield return _yield;

            if (GroundTrans != null)
            {
                Destroy(GroundTrans.gameObject);
                GroundTrans = null;
            }

            yield return _yield;
            if (GroundTrans == null)
            {
                GameObject temp = new GameObject("Ground");
                GroundTrans = temp.transform;
                GroundTrans.parent = transform;
            }

            yield return _yield;

            MapCheckList.Clear();
            baseClassList.Clear();
            CreateItemList.Clear();

            yield return _yield;

            switch (useType)
            {
                case MapType.Bottom:
                    for (int i = 0; i < RoomSize.x; i++)
                    {
                        MapCheckList.Add(new int[RoomSize.z]);
                        for (int j = 0; j < MapCheckList[i].Length; j++)
                        {
                            MapCheckList[i][j] = 0;
                        }
                    }
                    yield return _yield;
                    Map_BottomSetting();
                    break;
                case MapType.Obverse_Left_Wall:
                case MapType.Back_Left_Wall:
                    for (int i = 0; i < RoomSize.x; i++)
                    {
                        MapCheckList.Add(new int[RoomSize.y]);
                        for (int j = 0; j < MapCheckList[i].Length; j++)
                        {
                            MapCheckList[i][j] = 0;
                        }
                    }
                    yield return _yield;
                    Map_ObverseLWall();
                    break;
                case MapType.Obverse_Right_Wall:
                case MapType.Back_Right_Wall:
                    for (int i = 0; i < RoomSize.z; i++)
                    {
                        MapCheckList.Add(new int[RoomSize.y]);
                        for (int j = 0; j < MapCheckList[i].Length; j++)
                        {
                            MapCheckList[i][j] = 0;
                        }
                    }
                    yield return _yield;
                    Map_ObverseLWall();
                    break;
            }

            yield return _yield;

            switch (useType)
            {
                case MapType.Bottom:
                    SaveDataIndexSetting(saveData.Bottom_Item);
                    break;
                case MapType.Obverse_Left_Wall:
                    SaveDataIndexSetting(saveData.LeftObverse);
                    break;
                case MapType.Back_Left_Wall:
                    SaveDataIndexSetting(saveData.LeftBack);
                    break;
                case MapType.Obverse_Right_Wall:
                    SaveDataIndexSetting(saveData.RightObverse);
                    break;
                case MapType.Back_Right_Wall:
                    SaveDataIndexSetting(saveData.RightBack);
                    break;
            }

            yield return _yield;

            PlacementPosSetting();
            subSize = RoomSize;

            yield return _yield;

            LoadingCheck = true;
        }

        void Map_BottomSetting()
        {
            Vector3 CenterPos = Vector3.zero;
            //Quaternion v3Rotation = new Quaternion();
            BaseTile.transform.localScale = new Vector3(1, 0.1f, 1);

            for (int sx = 0; sx < MapCheckList.Count; sx++)
            {
                for (int sz = 0; sz < MapCheckList[sx].Length; sz++)
                {
                    Vector3 tempPos = Vector3.zero;
                    BaseTileClass tempBTC = new BaseTileClass();
                    GameObject tempObj = Instantiate(BaseTile);
                    tempObj.transform.parent = GroundTrans;

                    if (tempObj.TryGetComponent(out Renderer rnd))
                        tempBTC.Mat = rnd.sharedMaterial;
                    if (tempObj.TryGetComponent(out RoomTileEvnet rte))
                    {
                        tempBTC.RTE = rte;
                        tempBTC.RTE.roomManager = this;
                    }

                    tempBTC.RTE.MyPos.x = sx;
                    tempBTC.RTE.MyPos.z = sz;

                    tempPos.x = sx;
                    tempPos.z = sz;
                    tempPos.y = 0 - tempObj.transform.localScale.y;
                    tempObj.transform.position = tempPos;

                    baseClassList.Add(tempBTC);
                }
            }

            //ī�޶� ��ġ ����
            CenterPos.x = RoomSize.x / 2;
            CenterPos.y = RoomSize.z;
            CenterPos.z = RoomSize.z / 2;

            LookTargetObj.position = new Vector3(CenterPos.x, 0, CenterPos.z);
            AxisRtt.x = 45;
            AxisRtt.y = 0;
        }

        void Map_ObverseLWall()
        {
            Vector3 CenterPos = Vector3.zero;
            //Quaternion v3Rotation = new Quaternion();
            BaseTile.transform.localScale = new Vector3(1, 1f, 0.1f);

            for (int sx = 0; sx < MapCheckList.Count; sx++)
            {
                for (int sy = 0; sy < MapCheckList[sx].Length; sy++)
                {
                    Vector3 tempPos = Vector3.zero;
                    BaseTileClass tempBTC = new BaseTileClass();
                    GameObject tempObj = Instantiate(BaseTile);
                    tempObj.transform.parent = GroundTrans;

                    if (tempObj.TryGetComponent(out Renderer rnd))
                        tempBTC.Mat = rnd.sharedMaterial;
                    if (tempObj.TryGetComponent(out RoomTileEvnet rte))
                    {
                        tempBTC.RTE = rte;
                        tempBTC.RTE.roomManager = this;
                    }

                    tempBTC.RTE.MyPos.x = sx;
                    tempBTC.RTE.MyPos.y = sy;

                    tempPos.x = sx;
                    tempPos.y = sy;
                    tempPos.z = tempObj.transform.localScale.z;
                    tempObj.transform.position = tempPos;

                    baseClassList.Add(tempBTC);
                }
            }

            CenterPos.x = RoomSize.x / 2;
            CenterPos.y = RoomSize.y / 2;
            if (RoomSize.x < 10)
                CenterPos.z = -10;
            else
                CenterPos.z = -RoomSize.x;

            LookTargetObj.position = new Vector3(CenterPos.x, CenterPos.y, 0);
            AxisRtt.x = 20;
            AxisRtt.y = 0;
        }

        void OnChangdeColor()
        {
            if (!LoadingCheck)
                return;
            for (int i = 0; i < baseClassList.Count; i++)
            {
                baseClassList[i].Mat.color = BaseTileColor;
            }
            subColor = BaseTileColor;
        }

        public void OnClick_CreatePla(GameObject _obj)
        {
            ObjectDelete = false;

            if (!LoadingCheck)
                return;
            if (InsPlacement != null)
            {
                Destroy(InsPlacement.gameObject);
            }

            //��ġ ������ ����
            cpCount++;
            GameObject item = Instantiate(_obj);
            item.transform.parent = ItemGroup;
            InsPlacement = item.GetComponent<PlacementManger>();
            InsPlacement.roomManager = this;
            InsPlacement.PlacementID = cpCount;
            InsPlacement.AddOutLine();
            HandUpObj = true;

            //���׸��� �ν��Ͻ�ȭ ���� ������ ��� ������Ʈ�� ������ ���� �ȴ�.
            //InsPlacement.Mat = Instantiate(item.GetComponent<MeshRenderer>().sharedMaterial);
            //InsPlacement.GetComponent<MeshRenderer>().sharedMaterial = InsPlacement.Mat;

            PlacementPosSetting();
            ArrangementMode(1);
        }

#region ���콺 Ŭ�� �̺�Ʈ ����
        public void OnMouse_EnterEvent_Bottom(RoomTileEvnet mData)
        {
            if (!LoadingCheck)
                return;
            if (InsPlacement == null)
                return;

            InsPlacement.putOk = InsPutCheck(mData.MyPos, InsPlacement.ItemLayerId);
            Vector3 _Pos = mData.MyPos;
            _Pos.x += InsPlacement.PosCorrect.x;
            _Pos.y += InsPlacement.PosCorrect.y;
            _Pos.z += InsPlacement.PosCorrect.z;
            InsPlacement.transform.position = _Pos;

            if (InsPlacement.putOk)
            {
                InsPlacement.ChangeColor(CanPutColor);
            }
            else
                InsPlacement.ChangeColor(NotPutColor);
        }

        public void OnMouse_UpEvent_Bottom(RoomTileEvnet mData)
        {
            if (!LoadingCheck)
                return;
            if (InsPlacement == null)
                return;

            GameObject temp = null;
            Color subColor = Color.white;

            InsPlacement.putOk = InsPutCheck(mData.MyPos, InsPlacement.ItemLayerId);
            
            if (InsPlacement.putOk)
            {
                InsPlacement.inPutPos = mData.MyPos;
                bool saveCheck = true;

                for (int i = 0; i < CreateItemList.Count; i++)
                {
                    if (CreateItemList[i].PlacementID == InsPlacement.PlacementID)
                    {
                        saveCheck = false;
                        break;
                    }
                }

                if (saveCheck)
                {
                    CreateItemList.Add(InsPlacement);

                    temp = InsPlacement.gameObject;
                    subColor = InsPlacement.ObjectColor;
                }

                HandUpObj = false;
                InsPlacement.RemoveOutLine();
                InsPlacement = null;

                PlacementPosSetting();
                ArrangementMode(0);

                if (temp != null) 
                    CopyObjectSetting(temp, subColor);
            }

            //OnClick_CreatePla(InsPlacement.gameObject);
        }

        //��ġ�� ������Ʈ Ŭ���� �߻�.
        public void OnMouse_DownEvent_Bottom(int pManagerId)
        {
            if (!LoadingCheck)
                return;
            if (InsPlacement != null)
                return;

            int idx = -1;
            PlacementManger tempPM = null;
            for (int i = 0; i < CreateItemList.Count; i++)
            {
                if (CreateItemList[i].PlacementID == pManagerId)
                {
                    idx = i;
                    tempPM = CreateItemList[i];
                    break;
                }
            }

            if (tempPM == null)
                return;
            if (idx < 0)
                return;

            switch (useType)
            {
                case MapType.Bottom:
                    //�ش� ������Ʈ ���̾� ���� �ٸ� ������Ʈ�� ��ġ �Ǿ������ �Ǵ��Ͽ� ���� ����
                    for (int _x = 0; _x < tempPM.ObjSize.x; _x++)
                    {
                        for (int _z = 0; _z < tempPM.ObjSize.z; _z++)
                        {
                            if (MapCheckList[tempPM.inPutPos.x + _x][tempPM.inPutPos.z + _z] > tempPM.ItemLayerId)
                            {
                                Debug.Log((tempPM.inPutPos.x + _x) + ", " + (tempPM.inPutPos.z + _z) +
                                    " ��ǥ�� [" + MapCheckList[tempPM.inPutPos.x + _x][tempPM.inPutPos.z + _z] + "] ���̾��� ������Ʈ�� �ֽ��ϴ�");
                                return;
                            }
                        }
                    }
                    break;
            }

            if (ObjectDelete)
            {
                Destroy(CreateItemList[idx].gameObject);
                CreateItemList.RemoveAt(idx);
                PlacementPosSetting();
                ArrangementMode(0);
                return;
            }

            InsPlacement = CreateItemList[idx];

            InsPlacement.roomManager = this;
            InsPlacement.AddOutLine();
            HandUpObj = true;

            CreateItemList.RemoveAt(idx);

            PlacementPosSetting();
            ArrangementMode(1);
        }

        public void OnClick_HandOutObject()
        {
            if (!LoadingCheck)
                return;

            int idx = -1;
            for (int i = 0; i < CreateItemList.Count; i++)
            {
                if (CreateItemList[i].PlacementID == InsPlacement.PlacementID)
                {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
                CreateItemList.RemoveAt(idx);
            Destroy(InsPlacement.gameObject);

            HandUpObj = false;
            InsPlacement.RemoveOutLine();
            InsPlacement = null;

            PlacementPosSetting();
            ArrangementMode(0);
        }

        public void OnClick_rotationLeft()
        {
            var a = InsPlacement.ObjSize;
            InsPlacement.ObjSize = new Vector3(a.z, a.y, a.x);
        }

        public void OnClick_rotationRight()
        {
            var a = InsPlacement.ObjSize;
            InsPlacement.ObjSize = new Vector3(a.z, a.y, a.x);
        }

        public void Onclick_MapTypeSelect(int idx)
        {
            switch (idx)
            {
                case 0:
                    useType = MapType.Bottom;
                    break;
                case 1:
                    useType = MapType.Obverse_Left_Wall;
                    break;
                case 2:
                    useType = MapType.Obverse_Right_Wall;
                    break;
                case 3:
                    useType = MapType.Back_Left_Wall;
                    break;
                case 4:
                    useType = MapType.Back_Right_Wall;
                    break;
            }

            InitPage();
            LoadRoomData();
        }

        public void Onclick_DeleteSave()
        {
            for (int _x = 0; _x < MapCheckList.Count; _x++)
            {
                for (int _y = 0; _y < MapCheckList[_x].Length; _y++)
                {
                    MapCheckList[_x][_y] = 0;
                }
            }

            for (int i = CreateItemList.Count - 1; i >= 0; i--)
            {
                Destroy(CreateItemList[i].gameObject);
                CreateItemList.RemoveAt(i);
            }
        }

        public void OnClick_OneObjectDelete()
        {
            if (HandUpObj)
            {
                OnClick_HandOutObject();
            }

            ObjectDelete = true;
        }

        public void Onclick_SaveRoom()
        {
            saveData.MapSize = RoomSize;
            saveData.BaseColor = BaseTileColor;
            saveData.CanColor = CanPutColor;
            saveData.NotColor = NotPutColor;
            switch (useType)
            {
                case MapType.Bottom: saveData.Bottom_Item.Clear(); break;
                case MapType.Obverse_Left_Wall: saveData.LeftObverse.Clear(); break;
                case MapType.Obverse_Right_Wall: saveData.RightObverse.Clear(); break;
                case MapType.Back_Left_Wall: saveData.LeftBack.Clear(); break;
                case MapType.Back_Right_Wall: saveData.RightBack.Clear(); break;
            }

            for (int i = 0; i < CreateItemList.Count; i++)
            {
                ItemLsit temp = new ItemLsit();
                temp.itemId = CreateItemList[i].ItemId;
                temp.layerId = CreateItemList[i].ItemLayerId;
                temp.inPutPos = CreateItemList[i].inPutPos;
                temp.Size = CreateItemList[i].ObjSize;
                temp.mColor = CreateItemList[i].ObjectColor;
                switch (useType)
                {
                    case MapType.Bottom:
                        saveData.Bottom_Item.Add(temp);
                        break;
                    case MapType.Obverse_Left_Wall:
                        saveData.LeftObverse.Add(temp);
                        break;
                    case MapType.Obverse_Right_Wall:
                        saveData.RightObverse.Add(temp);
                        break;
                    case MapType.Back_Left_Wall:
                        saveData.LeftBack.Add(temp);
                        break;
                    case MapType.Back_Right_Wall:
                        saveData.RightBack.Add(temp);
                        break;
                }
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
        }
        #endregion

        //�ٴ� ���� �κ��̳� ���� ����ų� ��ø�Ǿ� ���ϼ� ���� ��� �Ǵ�.
        bool InsPutCheck(Vector3Int posIndex, int _LayerId)
        {
            //�ٷ� �Ʒ� �ܰ��� ���̾� ������ ��ġ �����ϴ�
            int oldLayer = _LayerId - 1;
            switch (useType)
            {
                case MapType.Bottom:
                    {
                        if (RoomSize.x <= (posIndex.x + (InsPlacement.ObjSize.x - 1)))
                            return false;
                        if (RoomSize.z <= (posIndex.z + (InsPlacement.ObjSize.z - 1)))
                            return false;

                        for (int i = 0; i < InsPlacement.ObjSize.x; i++)
                        {
                            for (int j = 0; j < InsPlacement.ObjSize.z; j++)
                            {
                                int nowMapLayer = MapCheckList[posIndex.x + i][posIndex.z + j];
                                //if (nowMapLayer != 0 &&
                                if(oldLayer != nowMapLayer)
                                {
                                    //0�� �ƴ� ��� �̹� ��ġ �Ǿ� �ִ� �����̰�
                                    //�ٷ� �Ʒ� �ܰ��� ���̾� ������ ��ġ �����ϴ�
                                   return false;
                                }
                            }
                        }
                    }
                    break;
                case MapType.Obverse_Left_Wall:
                case MapType.Back_Left_Wall:
                    {
                        if (RoomSize.x <= (posIndex.x + (InsPlacement.ObjSize.x - 1)))
                            return false;
                        if (RoomSize.y <= (posIndex.y + (InsPlacement.ObjSize.y - 1)))
                            return false;

                        for (int i = 0; i < InsPlacement.ObjSize.x; i++)
                        {
                            for (int j = 0; j < InsPlacement.ObjSize.y; j++)
                            {
                                int nowMapLayer = MapCheckList[posIndex.x + i][posIndex.y + j];
                                //if (nowMapLayer != 0 &&
                                if (oldLayer != nowMapLayer)
                                {
                                    //0�� �ƴ� ��� �̹� ��ġ �Ǿ� �ִ� �����̰�
                                    //�ٷ� �Ʒ� �ܰ��� ���̾� ������ ��ġ �����ϴ�
                                    return false;
                                }
                            }
                        }
                    }
                    break;
                case MapType.Obverse_Right_Wall:
                case MapType.Back_Right_Wall:
                    {
                        if (RoomSize.z <= (posIndex.x + (InsPlacement.ObjSize.x - 1)))
                            return false;
                        if (RoomSize.y <= (posIndex.y + (InsPlacement.ObjSize.y - 1)))
                            return false;

                        for (int i = 0; i < InsPlacement.ObjSize.x; i++)
                        {
                            for (int j = 0; j < InsPlacement.ObjSize.y; j++)
                            {
                                int nowMapLayer = MapCheckList[posIndex.x + i][posIndex.y + j];
                                //if (nowMapLayer != 0 &&
                                if (oldLayer != nowMapLayer)
                                {
                                    //0�� �ƴ� ��� �̹� ��ġ �Ǿ� �ִ� �����̰�
                                    //�ٷ� �Ʒ� �ܰ��� ���̾� ������ ��ġ �����ϴ�
                                    return false;
                                }
                            }
                        }
                    }
                    break;
            }
            return true;
        }

        void PlacementPosSetting()
        {
            if (!LoadingCheck)
                return;

            for (int _x = 0; _x < MapCheckList.Count; _x++)
            {
                for (int _y = 0; _y < MapCheckList[_x].Length; _y++)
                {
                    MapCheckList[_x][_y] = 0;
                }
            }

            switch (useType)
            {
                case MapType.Bottom:
                    for (int i = 0; i < CreateItemList.Count; i++)
                    {
                        for (int _x = 0; _x < CreateItemList[i].ObjSize.x; _x++)
                        {
                            for (int _z = 0; _z < CreateItemList[i].ObjSize.z; _z++)
                            {
                                if (MapCheckList[CreateItemList[i].inPutPos.x + _x][CreateItemList[i].inPutPos.z + _z] <= CreateItemList[i].ItemLayerId)
                                    MapCheckList[CreateItemList[i].inPutPos.x + _x][CreateItemList[i].inPutPos.z + _z] = CreateItemList[i].ItemLayerId;
                            }
                        }
                    }
                    break;
                case MapType.Obverse_Left_Wall:
                case MapType.Back_Left_Wall:
                case MapType.Obverse_Right_Wall:
                case MapType.Back_Right_Wall:
                    for (int i = 0; i < CreateItemList.Count; i++)
                    {
                        for (int _x = 0; _x < CreateItemList[i].ObjSize.x; _x++)
                        {
                            for (int _y = 0; _y < CreateItemList[i].ObjSize.y; _y++)
                            {
                                if (MapCheckList[CreateItemList[i].inPutPos.x + _x][CreateItemList[i].inPutPos.y + _y] <= CreateItemList[i].ItemLayerId)
                                    MapCheckList[CreateItemList[i].inPutPos.x + _x][CreateItemList[i].inPutPos.y + _y] = CreateItemList[i].ItemLayerId;
                            }
                        }
                    }
                    break;
                //case MapType.Obverse_Right_Wall:
                //case MapType.Back_Right_Wall:
                //    for (int i = 0; i < CreateItemList.Count; i++)
                //    {
                //        for (int _z = 0; _z < CreateItemList[i].ObjSize.x; _z++)
                //        {
                //            for (int _y = 0; _y < CreateItemList[i].ObjSize.y; _y++)
                //            {
                //                if (MapCheckList[CreateItemList[i].inPutPos.z + _z][CreateItemList[i].inPutPos.y + _y] <= CreateItemList[i].ItemLayerId)
                //                    MapCheckList[CreateItemList[i].inPutPos.z + _z][CreateItemList[i].inPutPos.y + _y] = CreateItemList[i].ItemLayerId;
                //            }
                //        }
                //    }
                //    break;
            }
        }

        void CopyObjectSetting(GameObject _temp,Color _color) {
            //Debug.Log(_temp);
            //��ġ ������ ����
            cpCount++;
            GameObject item = Instantiate(_temp);
            item.name = "CopyObject Number : " + cpCount;

            item.transform.parent = ItemGroup;
            InsPlacement = item.GetComponent<PlacementManger>();
            InsPlacement.ChangeColor(_color);
            InsPlacement.roomManager = this;
            InsPlacement.PlacementID = cpCount;
            InsPlacement.AddOutLine();
            HandUpObj = true;

            PlacementPosSetting();
            ArrangementMode(1);
        }
    }

    [Serializable]
    public enum MapType
    {
        Bottom,
        Obverse_Left_Wall,
        Obverse_Right_Wall,
        Back_Left_Wall,
        Back_Right_Wall,
    }

    [Serializable]
    public class ItemLsit
    {
        public int itemId;
        public int layerId;
        public Vector3Int inPutPos;
        public Vector3 Size;
        public Color mColor;
    }

    [Serializable]
    public class JySaveData
    {
        public Vector3Int MapSize = new Vector3Int();

        public Color BaseColor = new Color();
        public Color CanColor = new Color();
        public Color NotColor = new Color();

        public List<ItemLsit> Bottom_Item = new List<ItemLsit>();
        public List<ItemLsit> LeftObverse = new List<ItemLsit>();
        public List<ItemLsit> RightObverse = new List<ItemLsit>();
        public List<ItemLsit> LeftBack = new List<ItemLsit>();
        public List<ItemLsit> RightBack = new List<ItemLsit>();
    }
}
