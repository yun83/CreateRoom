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

        //�� üũ�ϱ� ���� �迭
        public List<int[]> MapCheckList = new List<int[]>();

        //���� �迭 ������ŭ�� �ٴ� ����
        private List<BaseTileClass> baseClassList = new List<BaseTileClass>();

        //������ ��ġ�� ������Ʈ�� ����
        public List<PlacementManger> CreateItemList = new List<PlacementManger>();
        private int cpCount = 0;

        //���� ���õ� ������Ʈ�� �����͸� Ȯ���ϱ�����
        public PlacementManger InsPlacement = null;

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
            saveData.Bottom_Item.Clear();
            saveData.LeftObverse.Clear();
            saveData.RightObverse.Clear();
            saveData.LeftBack.Clear();
            saveData.RightBack.Clear();
            LoadingCheck = true;

            InitPage();
            LoadRoomData();
        }

        void InitPage()
        {
            if (MainCamera == null)
                MainCamera = Camera.main.transform;

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
            Quaternion v3Rotation = new Quaternion();
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
            //ī�޶� �ٶ󺸴� ���� ����
            MainCamera.position = CenterPos;
            v3Rotation.eulerAngles = new Vector3(90, 0, 0);
            MainCamera.rotation = v3Rotation;
        }

        void Map_ObverseLWall()
        {
            Vector3 CenterPos = Vector3.zero;
            Quaternion v3Rotation = new Quaternion();
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

            MainCamera.position = CenterPos;
            v3Rotation.eulerAngles = new Vector3(0, 0, 0);
            MainCamera.rotation = v3Rotation;
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

            ArrangementMode(1);
        }

#region ���콺 Ŭ�� �̺�Ʈ ����
        public void OnMouse_EnterEvent_Bottom(RoomTileEvnet mData)
        {
            if (!LoadingCheck)
                return;
            if (InsPlacement == null)
                return;

            switch (useType)
            {
                case MapType.Bottom:
                    if (MapCheckList[mData.MyPos.x][mData.MyPos.z] == 0)
                    {
                        InsPlacement.putOk = true;
                    }
                    break;
                case MapType.Obverse_Left_Wall:
                case MapType.Back_Left_Wall:
                    if (MapCheckList[mData.MyPos.x][mData.MyPos.y] == 0)
                    {
                        InsPlacement.putOk = true;
                    }
                    break;
                case MapType.Obverse_Right_Wall:
                case MapType.Back_Right_Wall:
                    if (MapCheckList[mData.MyPos.z][mData.MyPos.y] == 0)
                    {
                        InsPlacement.putOk = true;
                    }
                    break;
            }

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
                }

                HandUpObj = false;
                InsPlacement.RemoveOutLine();
                InsPlacement = null;

                PlacementPosSetting();
                ArrangementMode(0);
            }
        }

        //��ġ�� ������Ʈ Ŭ���� �߻�.
        public void OnMouse_DownEvent_Bottom(int pManagerId)
        {
            if (!LoadingCheck)
                return;
            if (InsPlacement != null)
                return;

            int idx = -1;
            int endIndex = -1;
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

            if (endIndex >= 0)
                InsPlacement = CreateItemList[endIndex];
            else
                InsPlacement = CreateItemList[idx];

            InsPlacement.roomManager = this;
            InsPlacement.AddOutLine();
            HandUpObj = true;

            CreateItemList.RemoveAt(idx);
            PlacementPosSetting();
            ArrangementMode(1);
        }

        public void OnClick_DeletObject()
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

            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
            else
            {
                Debug.Log("�̹� ������ �����Դϴ�.");
            }
        }

        public void Onclick_SaveRoom()
        {
            saveData.MapSize = RoomSize;
            saveData.BaseColor = BaseTileColor;
            saveData.CanColor = CanPutColor;
            saveData.NotColor = NotPutColor;

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
                                //Debug.Log("i[" + i + "] j[" + j + "] ==>" + (posIndex.x + i) + ":" +(posIndex.z + j));

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
                        if (RoomSize.z <= (posIndex.z + (InsPlacement.ObjSize.z - 1)))
                            return false;
                        if (RoomSize.y <= (posIndex.y + (InsPlacement.ObjSize.y - 1)))
                            return false;

                        for (int i = 0; i < InsPlacement.ObjSize.z; i++)
                        {
                            for (int j = 0; j < InsPlacement.ObjSize.y; j++)
                            {
                                //Debug.Log("i[" + i + "] j[" + j + "] ==>" + (posIndex.x + i) + ":" +(posIndex.z + j));

                                int nowMapLayer = MapCheckList[posIndex.z + i][posIndex.y + j];
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
                case MapType.Obverse_Right_Wall:
                case MapType.Back_Right_Wall:
                    for (int i = 0; i < CreateItemList.Count; i++)
                    {
                        for (int _z = 0; _z < CreateItemList[i].ObjSize.z; _z++)
                        {
                            for (int _y = 0; _y < CreateItemList[i].ObjSize.y; _y++)
                            {
                                if (MapCheckList[CreateItemList[i].inPutPos.z + _z][CreateItemList[i].inPutPos.y + _y] <= CreateItemList[i].ItemLayerId)
                                    MapCheckList[CreateItemList[i].inPutPos.z + _z][CreateItemList[i].inPutPos.y + _y] = CreateItemList[i].ItemLayerId;
                            }
                        }
                    }
                    break;
            }
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
