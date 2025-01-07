using UnityEngine;
using System.Collections;

public class BlacksmithAnimationController : MonoBehaviour
{
    private Animator animator;
    private bool isWaiting = false;
    public float delayTime = 3f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("BlacksmithAnimation") && stateInfo.normalizedTime >= 1.0f && !isWaiting)
        {
            StartCoroutine(WaitAtEndOfLoop());
        }
    }

    IEnumerator WaitAtEndOfLoop()
    {
        isWaiting = true;
        animator.speed = 0f;
        yield return new WaitForSeconds(delayTime);
        animator.Play("BlacksmithAnimation", 0, 0f);
        animator.speed = 1f;

        isWaiting = false;
    }
}
