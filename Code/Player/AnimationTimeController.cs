using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTimeController : MonoSingleton<AnimationTimeController>
{
	// 애니메이션 타입(Enum 변수)
	public enum AnimType { ALERT = 0, MOVE, JUMP, DASH, THROW, DIE , DIE2}

	// 애니메이터 변수
	public Animator anim;

	// 애니메이터 컨트롤러 
	private RuntimeAnimatorController animCtrl;

	// 애니메이션 시간
	private float[] animTimes;
	public float this[int index]
	{
		get { return animTimes[index];  }
	}

	// 애니메이션 중복여부
	private bool[] isAnimOverlaps;

	// 애니메이션 이름
	private readonly string[] animNames = { "Aert", "Move", "Jump", "Dash", "ATK1" , "Die1", "Die2" };

	private void Awake()
	{
		// 모노싱글톤 애니메이션 컨트롤러 할당
		//instance = this;

		// 애니메이션 컴포넌트 할당
		anim = GetComponent<Animator>();

		// 애니메이션 할당 여부 체크
		if(anim != null)
		{
			// 런타임 애니메이션 컨트롤러 할당 
			animCtrl = anim.runtimeAnimatorController;
			// 애니메이션 컨트롤러 할당 여부 체크
			if(animCtrl != null)
			{
				// 애니메이션 시간 저장
				SetAnimTime();
			}
		}
		
	}

	// 애니메이션 시간 저장 함수
	void SetAnimTime()
	{
		// 애니메이션 시간 배열 할당
		int animClipsLength = animCtrl.animationClips.Length;
		int animTypeSize = System.Enum.GetNames(typeof(AnimType)).Length;

		animTimes = new float[animTypeSize];
		isAnimOverlaps = new bool[animTypeSize];

		// 에니메이션 시간 타입별로 저장 및 중복 제거
		for (int iCnt = 0; iCnt < animClipsLength; iCnt++)
		{
			if (animCtrl.animationClips[iCnt].name == animNames[(int)AnimType.ALERT])
			{
				if(isAnimOverlaps[(int)AnimType.ALERT] == false)
				{
					animTimes[(int)AnimType.ALERT] = animCtrl.animationClips[iCnt].length;
					isAnimOverlaps[(int)AnimType.ALERT] = true;
				}
					
			}
			else if (animCtrl.animationClips[iCnt].name == animNames[(int)AnimType.MOVE])
			{
				if (isAnimOverlaps[(int)AnimType.MOVE] == false)
				{
					animTimes[(int)AnimType.MOVE] = animCtrl.animationClips[iCnt].length;
					isAnimOverlaps[(int)AnimType.MOVE] = true;
				}
			}
			else if (animCtrl.animationClips[iCnt].name == animNames[(int)AnimType.JUMP])
			{
				if (isAnimOverlaps[(int)AnimType.JUMP] == false)
				{
					animTimes[(int)AnimType.JUMP] = animCtrl.animationClips[iCnt].length;
					isAnimOverlaps[(int)AnimType.JUMP] = true;
				}
			}
			else if (animCtrl.animationClips[iCnt].name == animNames[(int)AnimType.DASH])
			{
				if (isAnimOverlaps[(int)AnimType.DASH] == false)
				{
					animTimes[(int)AnimType.DASH] = animCtrl.animationClips[iCnt].length;
					isAnimOverlaps[(int)AnimType.DASH] = true;
				}
			}
			else if (animCtrl.animationClips[iCnt].name == animNames[(int)AnimType.THROW])
			{
				if (isAnimOverlaps[(int)AnimType.THROW] == false)
				{
					animTimes[(int)AnimType.THROW] = animCtrl.animationClips[iCnt].length;
					isAnimOverlaps[(int)AnimType.THROW] = true;
				}
			}
            else if (animCtrl.animationClips[iCnt].name == animNames[(int)AnimType.DIE])
            {
                if (isAnimOverlaps[(int)AnimType.DIE] == false)
                {
                    animTimes[(int)AnimType.DIE] = animCtrl.animationClips[iCnt].length;
                    isAnimOverlaps[(int)AnimType.DIE] = true;
                }
            }
            else if (animCtrl.animationClips[iCnt].name == animNames[(int)AnimType.DIE2])
            {
                if (isAnimOverlaps[(int)AnimType.DIE2] == false)
                {
                    animTimes[(int)AnimType.DIE2] = animCtrl.animationClips[iCnt].length;
                    isAnimOverlaps[(int)AnimType.DIE2] = true;
                }
            }
        }
	}
}
