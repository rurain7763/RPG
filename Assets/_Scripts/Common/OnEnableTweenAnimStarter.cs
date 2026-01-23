using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(DOTweenAnimation))]
public class OnEnableTweenAnimStarter : MonoBehaviour
{
    private DOTweenAnimation tweenAnimation;    

    private void Awake()
    {
        tweenAnimation = GetComponent<DOTweenAnimation>();
    }

    private void OnEnable()
    {
        tweenAnimation.DORestart();
    }

    private void OnDisable()
    {
        tweenAnimation.DOPause();
    }
}
