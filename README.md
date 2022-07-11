# CreateRoom
This module is created for placing items in the room.

##Demo Scene
* location of the demo scene
	* JyCreatRoom/Scenes/DemoVer1

```c
#if UNITY_EDITOR
	SavePath = Path.Combine(Application.dataPath, "database.json");
#else
	SavePath = Path.Combine(Application.persistentDataPath , "database.json");
#endif
	string json = JsonUtility.ToJson(saveData, true);
	File.WriteAllText(SavePath, json);
```

현재 Json 파일로 저장 하고 있는데 추후 서버에 저장하는 형태로 변환 해야 한다.
Currently, it is saved as a Json file, but it must be converted to a be saved in the server later.
