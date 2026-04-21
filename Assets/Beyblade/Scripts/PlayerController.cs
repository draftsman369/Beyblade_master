using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction jumpAction;

    private Rigidbody playerRigidbody;
    [SerializeField] private Transform graphic;
    public GameObject spikes;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    [SerializeField] private bool jumpPressed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float fallMultiplier;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDetectionRadius;
    [SerializeField] private LayerMask groundLayer;

    [Header("Score Values")]
    [SerializeField] private float currentScore;
    public float Score => currentScore;

    [Header("Ultimate")]
    [SerializeField] private float ultimateDetectionRadius = 8f;
    [SerializeField] private float ultimateMoveSpeed = 20f;
    [SerializeField] private float enemyPushForce = 12f;
    [SerializeField] private int maxUltimateTargets = 5;
    [SerializeField] private float targetReachDistance = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Ultimate Unlock")]
    [SerializeField] private bool unlockUltimateWithTime = true;
    [SerializeField] private float ultimateUnlockDelay = 20f;
    [SerializeField] private bool requireUltimateCharge = true;
    [SerializeField] private int currentUltimateCharge = 0;
    [SerializeField] private int requiredUltimateCharge = 5;

    private bool ultimateUnlocked;

    [Header("Ultimate Slow Motion")]
    [SerializeField] private float slowMotionScale = 0.2f;
    [SerializeField] private float slowMotionDuration = 0.1f;

    private float defaultFixedDeltaTime;

    [Header("Ultimate Post Process")]
    [SerializeField] private Volume ultimateVolume;
    [SerializeField] private float postEffectFadeInTime = 0.05f;
    [SerializeField] private float postEffectFadeOutTime = 0.12f;
    [SerializeField] private float maxChromaticIntensity = 0.9f;
    [SerializeField] private float maxVignetteIntensity = 0.5f;

    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    [Header("Ultimate Flash")]
    [SerializeField] private Image ultimateFlashImage;
    [SerializeField] private float flashMaxAlpha;
    [SerializeField] private float flashDuration = 0.12f;

    [Header("Camera Shake")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private PlayerState currentState;
    private bool isRestartingLevel = false;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        impulseSource = GetComponent<CinemachineImpulseSource>();

        defaultFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }

    private void Start()
    {
        if (ultimateVolume != null && ultimateVolume.profile != null)
        {
            ultimateVolume.profile.TryGet(out chromaticAberration);
            ultimateVolume.profile.TryGet(out vignette);

            ultimateVolume.weight = 0f;

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = 0f;

            if (vignette != null)
                vignette.intensity.value = 0f;
        }

        ultimateUnlocked = !unlockUltimateWithTime;

        if (unlockUltimateWithTime)
        {
            StartCoroutine(UnlockUltimateAfterDelay());
        }

        SwitchState(new PlayerMoveState(this));
    }

    private void OnEnable()
    {
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.Disable();
    }

    private void Update()
    {
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void SwitchState(PlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void ReadInput()
    {
        if (InputManager.Instance != null)
        {
            moveInput = InputManager.Instance.GetMoveInput();
        }

        if (jumpAction.WasPressedThisFrame())
        {
            jumpPressed = true;
        }
    }

    public bool TryConsumeUltimateInput()
    {
        if (InputManager.Instance == null || !InputManager.Instance.UltimatePressed)
            return false;

        InputManager.Instance.ConsumeUltimate();
        return true;
    }

    public bool CanStartUltimate()
    {
        if (!ultimateUnlocked)
            return false;

        if (requireUltimateCharge && currentUltimateCharge < requiredUltimateCharge)
            return false;

        return true;
    }

    public void BeginUltimateEffects()
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse();
        else
            Debug.LogWarning("Impulse not set");

        StartCoroutine(SlowMotionCoroutine());
        StartCoroutine(UltimatePostProcessRoutine());
    }

    public void StartUltimateStateRoutine()
    {
        StartCoroutine(UltimateStateRoutine());
    }

    private IEnumerator UltimateStateRoutine()
    {
        yield return StartCoroutine(UltimateRoutine());
        SwitchState(new PlayerMoveState(this));
    }

    private IEnumerator UnlockUltimateAfterDelay()
    {
        yield return new WaitForSeconds(ultimateUnlockDelay);
        ultimateUnlocked = true;
        Debug.Log("Ultimate unlocked!");
    }

    public void AddUltimateCharge(int amount)
    {
        currentUltimateCharge += amount;
        currentUltimateCharge = Mathf.Clamp(currentUltimateCharge, 0, requiredUltimateCharge);

        Debug.Log("Ultimate Charge: " + currentUltimateCharge + "/" + requiredUltimateCharge);
    }

    public void ConsumeUltimateCharge()
    {
        if (!requireUltimateCharge)
            return;

        currentUltimateCharge = 0;
    }

    public float GetUltimateChargeNormalized()
    {
        if (requiredUltimateCharge <= 0)
            return 0f;

        return (float)currentUltimateCharge / requiredUltimateCharge;
    }

    public bool IsUltimateUnlocked()
    {
        return ultimateUnlocked;
    }

    private IEnumerator UltimateRoutine()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ultimateDetectionRadius, enemyLayer);

        if (hits.Length == 0)
        {
            yield break;
        }

        var alreadyHit = new HashSet<Transform>();
        int hitCount = 0;

        while (hitCount < maxUltimateTargets)
        {
            hits = Physics.OverlapSphere(transform.position, ultimateDetectionRadius, enemyLayer);

            Transform target = GetClosestEnemy(hits, alreadyHit);

            if (target == null)
                break;

            alreadyHit.Add(target);

            float chaseTimer = 0f;
            float maxChaseTime = 1.5f;

            while (target != null)
            {
                Vector3 toTarget = target.position - transform.position;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude <= targetReachDistance * targetReachDistance)
                    break;

                Vector3 direction = toTarget.normalized;

                Vector3 currentVelocity = playerRigidbody.linearVelocity;
                Vector3 dashVelocity = direction * ultimateMoveSpeed;
                playerRigidbody.linearVelocity = new Vector3(dashVelocity.x, currentVelocity.y, dashVelocity.z);

                chaseTimer += Time.fixedDeltaTime;
                if (chaseTimer >= maxChaseTime)
                    break;

                yield return new WaitForFixedUpdate();
            }

            playerRigidbody.linearVelocity = new Vector3(0f, playerRigidbody.linearVelocity.y, 0f);

            if (target != null)
            {
                HitEnemy(target);
                hitCount++;
            }

            yield return new WaitForFixedUpdate();
        }

        playerRigidbody.linearVelocity = new Vector3(0f, playerRigidbody.linearVelocity.y, 0f);
    }

    private IEnumerator UltimatePostProcessRoutine()
    {
        if (ultimateVolume == null)
            yield break;

        float t = 0f;

        while (t < postEffectFadeInTime)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / postEffectFadeInTime);

            ultimateVolume.weight = normalized;

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(0f, maxChromaticIntensity, normalized);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, normalized);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        t = 0f;

        while (t < postEffectFadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float normalized = 1f - Mathf.Clamp01(t / postEffectFadeOutTime);

            ultimateVolume.weight = normalized;

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(0f, maxChromaticIntensity, normalized);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(0f, maxVignetteIntensity, normalized);

            yield return null;
        }

        ultimateVolume.weight = 0f;

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;

        if (vignette != null)
            vignette.intensity.value = 0f;
    }

    private IEnumerator SlowMotionCoroutine()
    {
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowMotionDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }

    private void HitEnemy(Transform target)
    {
        if (target == null)
            return;

        GameObject enemyObject = target.gameObject;
        Rigidbody targetRb = enemyObject.GetComponent<Rigidbody>();

        if (targetRb != null)
        {
            Vector3 pushDirection = (enemyObject.transform.position - transform.position).normalized;
            targetRb.AddForce(pushDirection * enemyPushForce, ForceMode.Impulse);
        }

        Destroy(enemyObject);
    }

    private Transform GetClosestEnemy(Collider[] hits, HashSet<Transform> alreadyHit)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            Transform enemy = hit.transform;

            if (alreadyHit.Contains(enemy))
                continue;

            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    public void Move()
    {
        moveDirection.Set(moveInput.x, 0f, moveInput.y);
        moveDirection.Normalize();

        Vector3 currentVelocity = playerRigidbody.linearVelocity;
        Vector3 horizontalVelocity = currentVelocity;
        horizontalVelocity.y = 0f;

        Vector3 targetVelocity = moveDirection * moveSpeed;

        float rate = (targetVelocity.magnitude > 0.1f) ? acceleration : deceleration;

        Vector3 newHorizontalVelocity =
            Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        playerRigidbody.linearVelocity =
            new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);

        if (graphic != null)
        {
            graphic.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }
    }

    public void HandleJump()
    {
        if (!jumpPressed)
            return;

        if (IsGrounded())
        {
            Vector3 currentVelocity = playerRigidbody.linearVelocity;
            currentVelocity.y = jumpForce;
            playerRigidbody.linearVelocity = currentVelocity;
            Debug.Log("Jumping");
        }

        jumpPressed = false;
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDetectionRadius, groundLayer);
    }

    public void UpgradeSpeed(float speedMultiplier)
    {
        if ((moveSpeed + moveSpeed * speedMultiplier) >= maxSpeed + 0.5f)
            return;

        moveSpeed += moveSpeed * speedMultiplier;
        rotateSpeed += rotateSpeed * speedMultiplier;
    }

    public void UpdgradeSpeed(float speedMultiplier)
    {
        UpgradeSpeed(speedMultiplier);
    }

    public void CheckGameOver()
    {
        if (transform.position.y < -10f && !isRestartingLevel)
        {
            Debug.Log("GameOver");
            isRestartingLevel = true;
            StartCoroutine(RestartLevel());
        }
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(0);
    }

    public void AddToScore(int value)
    {
        currentScore += value;
        Debug.Log(Score);
    }
}