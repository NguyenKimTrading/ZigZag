using UnityEngine;
using System.Collections;

public class ZigZagCreator : MonoBehaviour {

	private IDictionary logger = null;

	public Vector3 StartPoint = Vector3.zero;
	public PlayerController playerController;

	public GameObject CubeTemplate;
	
    Vector3 nextPosition;
    Vector3 nextDirection;

	private System.Collections.Generic.List<int> zigzag = new System.Collections.Generic.List<int>();

	public void SetLogger(IDictionary logger){
		this.logger = logger;
		logger ["zigzag"] = zigzag;
	}

	// Use this for initialization
	void Start () {
        nextPosition = new Vector3(5, 0, 3);
        nextDirection = Vector3.right;
	}

    // He so do do zigzag
	public float K = 0.1f;
	public float PaddingDistance = 10f;

	private System.Collections.Generic.LinkedList<GameObject> cubePool = new System.Collections.Generic.LinkedList<GameObject>();


    void createZigZag()
    {
		var rightPlusForward = Vector3.right + Vector3.forward;
        // Them 10 doan zigzag
        for (int i = 0; i < 10; i++)
        {
			int length = Mathf.RoundToInt ( K *  Mathf.Exp(Random.Range(0,4))) + 1;
			zigzag.Add(length);
            for (int j = 0; j < length; j++)
            {
				var newCube = Instantiate(CubeTemplate);
				newCube.transform.position = nextPosition;
				cubePool.AddLast(newCube);
                nextPosition = nextPosition + nextDirection * CubeTemplate.transform.localScale.x;
            }
			nextDirection = rightPlusForward - nextDirection;
        }
    }

	void DistroyZigZag() {
		// Xoa 20 cube dau tien
		//Debug.Log ("Xoa cube. So cube conlai: " + cubePool.Count);
		for (int i = 0; i < 10; i++) {
			if (cubePool.First != null) {
				GameObject.Destroy(cubePool.First.Value);
				cubePool.RemoveFirst();
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (playerController.IsRunning() || playerController.IsWating()) {
			if (Vector3.Distance( nextPosition, playerController.transform.position) < PaddingDistance * (CubeTemplate.transform.localScale.x + CubeTemplate.transform.localScale.z))
			{
				createZigZag();
			}
			if (playerController.IsRunning() && cubePool.First != null && Vector3.Distance( cubePool.First.Value.transform.position, playerController.transform.position) > PaddingDistance * (CubeTemplate.transform.localScale.x + CubeTemplate.transform.localScale.z))
			{
				DistroyZigZag();
			}
		}
	}
}
