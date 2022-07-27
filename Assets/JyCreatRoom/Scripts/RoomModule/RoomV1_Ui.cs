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
        public ObjectData ObjData;

        private bool EnumeratorCheck = false;
        private Color checkColor;

        private void FixedUpdate()
        {
            if (ShowObjectData != roomManager.HandUpObj)
            {
                ObjectData_Setting();
            }

            if (ObjData == null)
            {
                ObjData = ObjectData.GetComponent<ObjectData>();
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
        public void OnClick_FreeView()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("freeView");
        }
        void ObjectData_Setting()
        {
            ShowObjectData = roomManager.HandUpObj;
            if (ShowObjectData)
            {
                ObjectData.SetActive(true);

                if (roomManager.InsPlacement == null)
                    return;

                ObjData.ObjName.text = roomManager.InsPlacement.name;
                ObjData.Description.text = "Layer Number : " + roomManager.InsPlacement.ItemLayerId.ToString() + "\n" + "Item ID : " + roomManager.InsPlacement.ItemId.ToString();

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
                ObjData.PutOk.text = "��ġ ������ �����Դϴ�.";
            }
            else
            {
                ObjData.PutOk.text = "�̰����� ������ �����ϴ�.";
            }
        }

        IEnumerator StartChangeColor()
        {
            EnumeratorCheck = true;
            //�ٷ� �÷��� ������ ���� �ϱ� ���ؼ� ���� �ð� ������ �� ���� ��Ű�� ���ؼ� ���� �Լ�.
            var _yield = new WaitForEndOfFrame(); ;

            checkColor = roomManager.InsPlacement.ObjectColor;

            yield return _yield;

            checkColor.a = 1;

            ObjData.ColorImage.color = checkColor;

            ObjData.Red.value = checkColor.r;
            ObjData.Green.value = checkColor.g;
            ObjData.Blue.value = checkColor.b;

            yield return _yield;

            EnumeratorCheck = false;
        }

        public void Change_SetColor()
        {
            if (EnumeratorCheck)
                return;

            Color _color;
            _color.r = ObjData.Red.value;
            _color.g = ObjData.Green.value;
            _color.b = ObjData.Blue.value;
            _color.a = 1;

            ObjData.ColorImage.color = _color;
            roomManager.InsPlacement.ChangeObjectColor(_color);
            checkColor = _color;
        }
    }
}
