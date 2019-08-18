using System.Collections;
using UnityEngine;

public class rocketLauncher : MonoBehaviour
{
    Rigidbody myRigidbody;
    [SerializeField] private GameObject trailobj;
    [SerializeField] private GameObject target;
    [SerializeField] private string enginestate;
    [SerializeField] private string steerstate;
    [SerializeField] private float frocket;
    float h, h1, h2, x, tx, ty, g;
    bool spawnlock;

    void Start()
    {
        myRigidbody = gameObject.GetComponent<Rigidbody>();
        enginestate = "inactive";
        steerstate = "wrong";
        StartCoroutine(launch());
        StartCoroutine(steer());
        StartCoroutine(bend());
        StartCoroutine(engine());
        //StartCoroutine(trail());

    }
    IEnumerator steer()
    {
        yield return new WaitUntil(() => steerstate == "balancing");
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;

        while (Vector3.Angle(Vector3.up, transform.forward) > 1)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.up, -direction), 1);
            yield return new WaitForSeconds(0.01f);
        }
        steerstate = "balanced";
    }

    IEnumerator bend()
    {
        yield return new WaitUntil(() => steerstate == "bending");
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;

        while (Vector3.Angle(direction, transform.forward) > 1)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction, transform.up), 0.1f);
            yield return new WaitForSeconds(0.02f);
        }

        steerstate = "bended";
    }

    IEnumerator engine()
    {
        while (true)
        {
            yield return new WaitUntil(() => enginestate == "active");
            while (enginestate == "active")
            {
                forceMain();
                yield return null;
            }
            yield return null;
        }
    }
    IEnumerator launch()
    {
        yield return new WaitForSeconds(2f);
        g = Physics.gravity.magnitude;
        x = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(target.transform.position.x, target.transform.position.z));

        h2 = target.transform.position.y - transform.position.y;
        tx = Mathf.Sqrt((2 * x) / frocket);
        h1 = g / 2 * Mathf.Pow(tx, 2);
        h = h1 + h2;
        ty = Mathf.Sqrt((2 * h * g) / ((frocket - g) * (g + (frocket - g))));

        float y0 = transform.position.y;
        float timer = Time.time;
        steerstate = "balancing";
        enginestate = "active";
        float limit = 0;
        print(x);
        if (x < 1500)
        {
            limit = 0.68f;
        }else if(x < 5000)
        {
            limit = 0.71f;
        }else if(x<10000)
        {
            limit = 0.80f;
        }
        yield return new WaitUntil(() => Time.time - timer >= ty * limit);
        enginestate = "inactive";
        yield return new WaitUntil(() => steerstate == "balanced");
        steerstate = "bending";
        yield return new WaitUntil(() => steerstate == "bended");
        yield return new WaitUntil(() => myRigidbody.velocity.magnitude < 3f);
        enginestate = "active";
        yield return null;
    }

    IEnumerator spawner()
    {
        while(true)
        {
            yield return new WaitUntil(() => spawnlock == true);
            Instantiate(trailobj, transform.position, transform.rotation);
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        spawnlock = true;
    }

    public void forceMain()
    {
        myRigidbody.AddForce(transform.forward * frocket);
    }
}
