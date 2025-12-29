using System;
using UnityEngine;

public class PuzzleAnim : BaseViewAnimation
{
	public Animator animator;
	private Action callback;

	private void Awake()
	{
		animator.updateMode = AnimatorUpdateMode.UnscaledTime;
	}
	// Start is called before the first frame update
	public override void HideViewAnimation(Action callback)
	{
		this.callback = callback;
		animator.Play("PuzzleHide");
	}

	public override void ShowViewAnimation(Action callback)
	{
		this.callback = callback;
		animator.Play("PuzzleShow");
	}
	public void ShowDescription(Action callback)
	{
		this.callback = callback;
		animator.Play("ShowDescription");
	}
	public void HideDescription(Action callback)
	{
		this.callback = callback;
		animator.Play("HideDescription");
	}
	public void ShowAnim()
	{
		callback?.Invoke();
	}

	public void HideAnim()	
	{
		callback?.Invoke();
	}

	public void ShowDescAnim()
	{
		callback?.Invoke();
	}
	public void HideDescAnim()
	{
		callback?.Invoke();
	}
}
