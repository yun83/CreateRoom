using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JyModule
{
    public class RoomV1_Ui : MonoBehaviour
    {
        public RoomV1_Manager roomManager;
        public Dropdown RoomSeleftDromdown;
        public GameObject[] ItemBoxGroup;
        public GameObject[] ItemObject;

        public GameObject ObjectData;
        private bool ShowObjectData = true;
        public ObjectData od;

        private bool EnumeratorCheck = false;
        private Color checkColor;

        private void FixedUpdate()
        {
            if (ShowObjectData != roomManager.HandUpObj)
            {
                ObjectData_Setting();
            }
            showUsePutText();
        }

        public void OnClick_Event(int index)
        {
            int ObjListSize = ItemObject.Length;
            roomManager.OnClick_CreatePla(ItemObject[index]);
        }

        public void OnChangede_RoomType()
        {
            int idx = RoomSeleftDromdown.value;
            Debug.Log((MapType)idx);
            if (idx == 0)
            {
                ItemBoxGroup[0].SetActive(true);
                ItemBoxGroup[1].SetActive(false);
            }
            else
            {
                ItemBoxGroup[0].SetActive(false);
                ItemBoxGroup[1].SetActive(true);
            }
            roomManager.Onclick_MapTypeSelect(idx);
        }
        public void OnClick_RoomType(int index)
        {
            if(index == 0)
            {
                ItemBoxGroup[0].SetActive(true);
                ItemBoxGroup[1].SetActive(false);
            }
            else
            {
                ItemBoxGroup[0].SetActive(false);
                ItemBoxGroup[1].SetActive(true);
            }
            roomManager.Onclick_MapTypeSelect(index);
        }
        public void OnClick_Save()
        {
            roomManager.Onclick_SaveRoom();
        }
        public void OnClick_Delet()
        {
            roomManager.Onclick_DeleteSave();
        }
        public void OnClick_DeletObject()
        {
            roomManager.OnClick_OneObjectDelete();
        }

        void ObjectData_Setting()
        {
            ShowObjectData = roomManager.HandUpObj;
            if (ShowObjectData)
            {
                ObjectData.SetActive(true);

                if (roomManager.InsPlacement == null)
                    return;

                od.ObjName.text = roomManager.InsPlacement.name;
                od.Description.text = "Layer Number : " + roomManager.InsPlacement.ItemLayerId.ToString() + "\n" + "Item ID : " + roomManager.InsPlacement.ItemId.ToString();

                if(!EnumeratorCheck)
                    StartCoroutine(StartChangeColor());
            }
            else
            {
                ObjectData.SetActive(false);
            }
        }

        void showUsePutText()
        {
            if (!ShowObjectData)
                return;
            if (roomManager.InsPlacement == null)
                return;

            if (roomManager.InsPlacement.putOk)
            {
                od.PutOk.text = "배치 가능한 상태입니다.";
            }
            else
            {
                od.PutOk.text = "이곳에는 놓을수 없습니다.";
            }
        }

        IEnumerator StartChangeColor()
        {
            EnumeratorCheck = true;
            //바로 컬러값 변경을 방지 하기 위해서 일정 시간 딜레이 후 실행 시키기 위해서 만든 함수.
            var _yield = new WaitForEndOfFrame(); ;

            checkColor = roomManager.InsPlacement.ObjectColor;

            yield return _yield;

            checkColor.a = 1;

            od.ColorImage.color = checkColor;

            od.Red.value = checkColor.r;
            od.Green.value = checkColor.g;
            od.Blue.value = checkColor.b;

            yield return _yield;

            EnumeratorCheck = false;
        }

        public void Change_SetColor()
        {
            if (EnumeratorCheck)
                return;

            Color _color;
            _color.r = od.Red.value;
            _color.g = od.Green.value;
            _color.b = od.Blue.value;
            _color.a = 1;

            od.ColorImage.color = _color;
            roomManager.InsPlacement.ChangeObjectColor(_color);
            checkColor = _color;
        }
    }
}
