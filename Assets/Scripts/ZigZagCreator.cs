using UnityEngine;
using System.Collections;

public class ZigZagCreator : MonoBehaviour {

	public Vector3 StartPoint = Vector3.zero;
    public GameObject Player;

	public GameObject CubeTemplate;

    GameObject lastCube;
    Vector3 lastPosition;
    Vector3 lastDirection;

	// Use this for initialization
	void Start () {
        lastPosition = new Vector3(23, 0, 15);
        lastDirection = Vector3.right;
        createZigZag();
	}

    
	public float K = 1 / 2;

    void createZigZag()
    {
        // Tao duong zigzag
        for (int i = 0; i < 10; i++)
        {
			int length = Mathf.RoundToInt ( K *  Mathf.Exp(Random.Range(0,4)));
            for (int j = 0; j < length; j++)
            {
                lastCube = Instantiate(CubeTemplate);
                lastPosition = lastPosition + lastDirection * lastCube.transform.localScale.x;
                lastCube.transform.position = lastPosition;
            }
            lastDirection = (lastDirection == Vector3.right) ? Vector3.forward : Vector3.right;
        }
    }

	// Update is called once per frame
	void Update () {
        if (lastCube != null)
        {
			if (Vector3.Distance( lastCube.transform.position, Player.transform.position) < 10f * (lastCube.transform.localScale.x + lastCube.transform.localScale.z))
            {
                createZigZag();
            }
        }
	}
}
