using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossPattern : MonoBehaviour
{
    [Header("���� �̵� ���� ����")]
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float minDistance;

    [Space]

    [Header("���� �ɼ�")]

    [Header("���ǽ� ���� ���� ����")]
    [SerializeField] protected bool isHaveSpecialPattern = false;
    [Header("���� ���� �ĵ�����")]
    [SerializeField] private float patternDelay;
    
    protected int[] patternCount = new int[5];


    protected Transform player;
    
    protected Animation attackAnim;
    protected List<string> animArray;

    protected Coroutine attackCoroutine = null;

    protected bool isUsedSpecialPattern = false;
    protected bool isCanUseSpecialPattern = false;

    private void Start()
    {
        player = Player.Instance.transform;
        attackAnim = GetComponent<Animation>();

        AnimationArray();
        StartCoroutine(RandomPattern());
    }

    //private void Update()
    //{
    //    MoveToPlayer();
    //}

    public void AnimationArray()
    {
        animArray = new List<string>();

        foreach (AnimationState states in attackAnim)
        {
            animArray.Add(states.name);
        }
    }

    public void MoveToPlayer()
    {
        if (attackCoroutine != null || Boss.Instance.isBDead) return;

        float playerDistance = Vector2.Distance(player.position, transform.position);
        if (playerDistance <= minDistance) return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.Translate(dir * Time.deltaTime * moveSpeed);
    }

    private IEnumerator RandomPattern()
    {
        while (true)
        {
            int patternChoice = isHaveSpecialPattern ? Random.Range(0, 3) : Random.Range(0, 4);
            patternCount[patternChoice] = GetRandomCount(patternChoice);

            if (attackCoroutine == null)
            {
                if (!isUsedSpecialPattern && isHaveSpecialPattern && isCanUseSpecialPattern)
                {
                    isUsedSpecialPattern = true;
                    attackCoroutine = StartCoroutine(Pattern4(patternCount[3]));
                }
                else
                {
                    switch (patternChoice)
                    {
                        case 0:
                            attackCoroutine = StartCoroutine(Pattern1(patternCount[0]));
                            break;
                        case 1:
                            attackCoroutine = StartCoroutine(Pattern2(patternCount[1]));
                            break;
                        case 2:
                            attackCoroutine = StartCoroutine(Pattern3(patternCount[2]));
                            break;
                        case 3:
                            attackCoroutine = StartCoroutine(Pattern4(patternCount[3]));
                            break;
                    }
                }
            }
            yield return new WaitUntil(() => attackCoroutine == null);
            yield return new WaitForSeconds(patternDelay);
        }
    }

    public virtual int GetRandomCount(int choisedPattern)
    {
        return 0;
    }

    public abstract IEnumerator Pattern1(int count = 0);
    public abstract IEnumerator Pattern2(int count = 0);
    public abstract IEnumerator Pattern3(int count = 0);

    public virtual IEnumerator Pattern4(int count = 0)
    {
        yield break;
    }
    public virtual IEnumerator PatternFinal(int count = 0)
    {
        yield break;
    }

}