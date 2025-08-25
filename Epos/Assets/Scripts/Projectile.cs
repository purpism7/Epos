using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 targetPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.gameObject.activeSelf)
            return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 20f);

        var distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= 0.1f)
        {
            Extensions.SetActive(transform, false);
        }

    }
}
