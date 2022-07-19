using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JyModule
{
    public class PlacementManger : MonoBehaviour
    {
        [Header("아이템 레이어.")]
        public int ItemLayerId = 1;
        /// <summary>
        /// 아이템의 종류 분기
        /// </summary>
        public int ItemId = -1;

        [Header("아이템 크기.")]
        public Vector3 ObjSize = Vector3.one;
        [Header("배치점 자동 생성")]
        public bool AutoPosition = true;
        public Vector3 PosCorrect = Vector3.zero;

        [Header("아이템 생성시 사용.")]
        public RoomV1_Manager roomManager;
        /// <summary>
        /// 생성후 배치 아이템 종류 및 순서 파악
        /// </summary>
        public int PlacementID = -1;

        public Collider thisCollider;

        public bool putOk = false;

        public Vector3Int inPutPos = Vector3Int.zero;

        public Color ObjectColor = new Color();
        private Material Mat = null;

        private Vector3 subSize = Vector3.one;

        public Material outline;
        public Renderer renderers;
        public List<Material> materialList = new List<Material>();

        private void Awake()
        {
            outline = new Material(Shader.Find("Draw/OutlineShader"));

            outline.SetColor("_OutlineColor", new Color(1, 1, 1, 1f));
            initBoxSizeCheck();
        }

        private void FixedUpdate()
        {
            if (subSize != ObjSize)
            {
                initBoxSizeCheck();
            }
        }

        public void initBoxSizeCheck()
        {
            thisCollider = GetComponent<Collider>();

            transform.localScale = ObjSize;
            if (Mat == null)
            {
                Mat = Instantiate(gameObject.GetComponent<MeshRenderer>().material);
                gameObject.GetComponent<MeshRenderer>().material = Mat;
                ObjectColor = Mat.color;
            }
            renderers = this.GetComponent<Renderer>();

            AutoPositionSum();

            subSize = ObjSize;
        }

        public void AutoPositionSum()
        {
            if (!AutoPosition)
                return;

            if (roomManager != null)
            {
                if (roomManager.useType == MapType.Bottom)
                {
                    PosCorrect.x = (ObjSize.x - 1) * 0.5f;
                    PosCorrect.y = (ObjSize.y * 0.5f);
                    PosCorrect.z = (ObjSize.z - 1) * 0.5f;
                }
                else
                {
                    PosCorrect.x = (ObjSize.x - 1) * 0.5f;
                    PosCorrect.y = (ObjSize.y - 1) * 0.5f;
                    PosCorrect.z = (ObjSize.z * 0.5f) - ObjSize.z;
                }
            }
            else
            {
                PosCorrect.x = (ObjSize.x - 1) * 0.5f;
                PosCorrect.y = (ObjSize.y * 0.5f);
                PosCorrect.z = (ObjSize.z - 1) * 0.5f;

                Invoke("AutoPositionSum", 0.1f);
            }
        }

        public void ChangeColor(Color _color)
        {
            outline.SetColor("_OutlineColor", _color);
        }

        public void ChangeObjectColor(Color _color)
        {
            ObjectColor = _color;
            ObjectColor.a = 1;
            Mat.color = ObjectColor;
        }

        private void OnMouseDown()
        {
            roomManager.OnMouse_DownEvent_Bottom(PlacementID);
        }

        public void AddOutLine()
        {
            materialList.Clear();
            materialList.AddRange(renderers.sharedMaterials);
            materialList.Add(outline);

            renderers.materials = materialList.ToArray();
        }

        public void RemoveOutLine()
        {
            Renderer renderer = this.GetComponent<Renderer>();

            materialList.Clear();
            materialList.AddRange(renderer.sharedMaterials);
            materialList.Remove(outline);

            renderer.materials = materialList.ToArray();
        }
    }
}
