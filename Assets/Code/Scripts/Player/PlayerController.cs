using UnityEngine;
using UnityEngine.InputSystem;
using EnumType;
using System.Collections;
using NUnit.Framework.Constraints;       // GlobalEnum

public class PlayerController : MonoBehaviour, IDamageable
{
	// 플레이어 정보
	private Rigidbody2D rigid;
	private SpriteRenderer sprite;
	private PlayerState state;      // 플레이어 상태
	private bool isGrounded;        // 땅 여부
	private bool isUsedDash;        // 대쉬 사용 여부

	// 이동값
	private Vector2 inputVec;   // 입력된 플레이어 이동값 (-1, 0, 1)
	private float speed;        // 플레이어 이동 속도

	// 코루틴
	private Coroutine playerDashCoroutine;  // 플레이어 대쉬

	private void Awake()
	{
		rigid = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		isGrounded = true;
		isUsedDash = false;
		state = PlayerState.Idle;
		speed = GameManager.Instance.playerStatsRuntime.speed;
	}

	/* Update */
	private void Update()
	{
		rigid.linearVelocity = new Vector2(inputVec.x * speed, rigid.linearVelocityY);
	}

	// 플레이어 스프라이트 업데이트
	private void UpdateSprite()
	{
		// 좌우 플립
		if (inputVec.x > 0)
			sprite.flipX = false;
		else if (inputVec.x < 0)
			sprite.flipX = true;
	}

	/* Input System */
	void OnMove(InputValue val)     // 좌우 이동 (AD)
	{
		if (isUsedDash) return;     // 대쉬 사용 중일 경우 리턴
		inputVec = val.Get<Vector2>();
	}

	void OnJump(InputValue val)     // 점프 (W)
	{
		if (!isGrounded) return;    // 땅에 서있지 않을 경우 리턴

		rigid.AddForce(Vector2.up * GameManager.Instance.playerStatsRuntime.jumpForce, ForceMode2D.Impulse);
		isGrounded = false;
	}

	void OnCrouch(InputValue val)   // 구르기/대쉬/내려가기 (S)
	{
		if (isUsedDash) return;     // 대쉬 사용 중일 경우 리턴
		StartCoroutine(PlayerDash());   // 대쉬 코루틴 시작
	}

	/* Coroutine */
	IEnumerator PlayerDash()    // 플레이어 대쉬
	{
		float originalGravity = rigid.gravityScale;     // 원래 이동 속도 저장
		isUsedDash = true;
		rigid.linearVelocity = new Vector2(inputVec.x * GameManager.Instance.playerStatsRuntime.dashSpeed, 0f);
		// 정해진 무적 시간만큼 기다리기
		yield return new WaitForSeconds(GameManager.Instance.playerStatsRuntime.invincibilityDuration);
		isUsedDash = false;
	}

	/* Interface */
	public void TakeDamage(int attack)  // 데미지
	{
		if (isUsedDash) return;   // 무적일 경우 리턴

		GameManager.Instance.playerStatsRuntime.currentHP -= attack;    // 체력 감소

		if (GameManager.Instance.playerStatsRuntime.currentHP <= 0)     // 체력이 0 이하일 때
		{
			return;
		}
	}

	public void Die()   // 죽음 처리
	{

	}
}
