using UnityEngine;

public class Projectile : Common.Component<Projectile.Param>
{
    public class Param : Common.Param
    {
        public Transform TargetTm { get; private set; } = null;

        public Param WithTargetTm(Transform targetTm)
        {
            TargetTm = targetTm;
            return this;
        }
    }

    public Vector3 startPos;
    public Vector3 targetPos;


    public override void Initialize(Param param)
    {
        base.Initialize(param);
    }

    public override void Activate(Param param)
    {
        base.Activate(param);
    }

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
        if (distance <= 0.01f)
        {
            Extensions.SetActive(transform, false);
        }

    }
}
