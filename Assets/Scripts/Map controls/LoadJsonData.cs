using UnityEngine;

public class LoadJsonData: MonoBehaviour{
    public TextAsset nodeDataJson;

    public RootNodeData ReadNodeData() {
        return JsonUtility.FromJson<RootNodeData>(FixJson(nodeDataJson.text));
    }

    string FixJson(string value) {
        value = "{\"particleDataRoot\":" + value + "}";
        return value;
    }

    [System.Serializable]
    public class NodeJsonData {
        public int cellTypeNumber;
        public string cellName;
        public int state;
        public float cellColorR;
        public float cellColorG;
        public float cellColorB;
        public float cellColorA;
    }

    [System.Serializable]
    public class RootNodeData {
        public NodeJsonData[] particleDataRoot;
    }
}