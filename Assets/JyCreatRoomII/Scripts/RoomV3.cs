using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JyModule
{
    public class RoomV3 : MonoBehaviour
    {
        [Header("배치 타입")]
        public MapType useType = MapType.Bottom;
        public Dropdown DropdownMapType;
        
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
