using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPattern : MonoBehaviour
{
    [SerializeField] private GameObject warning;
    [SerializeField] private GameObject gasi;
    [SerializeField] private GameObject bossMonster;

    [SerializeField] private ParticleSystem pattern1;
    [SerializeField] private ParticleSystem pattern3;

    [Space]

    [SerializeField] private GameObject bullet;
    [SerializeField] private float moveSpeed = 2f;

    private Transform player;
    private Coroutine attackCoroutine = null;

    private bool isHealUsed = false;


    private void Awake()
    {
        player = Player.Instance.transform;

        StartCoroutine(RandomPattern());
    }

    private void Update()
    {
        MoveToPlayer();
    }

    private void LateUpdate()
    {
        if (Boss.Instance.isBDead)
        {
            StopAllCoroutines();
        }
    }

    public void MoveToPlayer()
    {
        if (attackCoroutine != null || Boss.Instance.isBDead) return;

        float playerDistance = Vector2.Distance(player.position, transform.position);
        if (playerDistance <= 1f) return;

        Vector2 dir = player.position - transform.position;
        transform.Translate(dir.normalized * Time.deltaTime * moveSpeed);
    }

    private IEnumerator RandomPattern()
    {
        while(true)
        {
            if (attackCoroutine == null)
            {
                if (!isHealUsed && Boss.Instance.Base.Hp <= Boss.Instance.Base.MaxHp * 0.4f)
                {
                    attackCoroutine = StartCoroutine(Pattern_SummonMonster(10));
                }
                else
                {
                    switch (Random.Range(0, 3))
                    {
                        case 0:
                            attackCoroutine = StartCoroutine(Pattern_MakeThorn(Random.Range(3, 5)));
                            break;
                        case 1:
                            attackCoroutine = StartCoroutine(Pattern_ShootBullet(Random.Range(25, 30)));
                            break;
                        case 2:
                            attackCoroutine = StartCoroutine(Pattern_Teleport());
                            break;
                    }
                }
            }
            yield return new WaitUntil(() => attackCoroutine == null);
            yield return new WaitForSeconds(5f);
        }
    }

    private IEnumerator Pattern_MakeThorn(int attackCount)
    {
        for(int i = 0; i< attackCount; i++)
        {
            GameObject clone = Instantiate(warning, player.position, Quaternion.Euler(Vector3.zero));
            yield return new WaitForSeconds(1f);

            GameObject clone2 = Instantiate(gasi, clone.transform);
            pattern1.transform.position = clone.transform.position;
            pattern1.Play();
            clone2.transform.SetParent(null);

            Destroy(clone);

            yield return new WaitForSeconds(0.4f);
            Destroy(clone2);
        }

        yield return null;
        attackCoroutine = null;
    }

    private IEnumerator Pattern_ShootBullet(int attackCount)
    {
        float angle = 360 / (attackCount * 0.89f);
        
        for(int i = 0; i < attackCount; i++)
        {
            Instantiate(bullet, transform.position, Quaternion.Euler(Vector3.forward * angle * i));
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
        attackCoroutine = null;
    }

    private IEnumerator Pattern_Teleport()
    {
        moveSpeed *= 0.5f;
        float timer = 0f;
        
        while(timer <= 3f)
        {
            timer += Time.deltaTime;
            yield return null;

            Vector2 dir = player.position - transform.position;
            transform.Translate(dir.normalized * Time.deltaTime * moveSpeed);
        }

        transform.position = player.right + player.position;
        moveSpeed *= 2f;

        yield return null;
        attackCoroutine = null;
    }

    private IEnumerator Pattern_SummonMonster(int mobCount)
    {
        int finalCount = 0;
        List<GameObject> mobList = new List<GameObject>();
        List<GameObject> Patlist = new List<GameObject>();

        for (int i = 0; i < mobCount; i++)
        {
            GameObject clone = Instantiate(bossMonster, new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.Euler(Vector3.zero));
            GameObject pattern33 = Instantiate(pattern3.gameObject, new Vector2(0,1), Quaternion.Euler(Vector3.zero));
            pattern33.transform.position = clone.transform.position;
            ParticleSystem particle = pattern33.GetComponent<ParticleSystem>();
            particle.Play();
            
            
            Patlist.Add(pattern33);
            mobList.Add(clone);
        }

        yield return new WaitForSeconds(10f);

        foreach(var mob in mobList)
        {
            if(mob != null)
            {
                finalCount++;
                Destroy(mob);
            }

        }
        Boss.Instance.Base.Hp += finalCount * 10;
        mobList.Clear();

        yield return null;
        Debug.Log(Boss.Instance.Base.Hp);
        attackCoroutine = null;
    }
}
