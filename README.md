# CreateRoom
This module is created for placing items in the room.

##Demo Scene
* location of the demo scene
	* JyCreatRoom/Scenes/DemoVer1

'''c#
#if UNITY_EDITOR
	SavePath = Path.Combine(Application.dataPath, "database.json");
#else
	SavePath = Path.Combine(Application.persistentDataPath , "database.json");
#endif
	string json = JsonUtility.ToJson(saveData, true);
	File.WriteAllText(SavePath, json);
'''

���� Json ���Ϸ� ���� �ϰ� �ִµ� ���� ������ �����ϴ� ���·� ��ȯ �ؾ� �Ѵ�.
