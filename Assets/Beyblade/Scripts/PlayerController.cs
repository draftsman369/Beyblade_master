using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Unity.VisualScripting;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{

    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction ultimateAction;

    private Rigidbody playerRigidbody;
    [SerializeField] private Transform graphic;


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
    
    //Ground detection
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDetectionRadius;
    [SerializeField] private LayerMask groundLayer;


    [Header("Score values")]
    [SerializeField] private float currentScore;
    public float Score => currentScore;

    [Header("Ultimate")]
    [SerializeField] private float ultimateDetectionRadius = 8f;
    [SerializeField] private float ultimateMoveSpeed = 20f;
    [SerializeField] private float enemyPushForce = 12f;
    [SerializeField] private int maxUltimateTargets = 5;
    [SerializeField] private float targetReachDistance = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] private bool ultimatePressed;
    private bool isUsingUltimate;


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

    [Header("VFX")]
    public GameObject explosionVFX;

    public GameObject spikes;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        impulseSource = this.GetComponent<CinemachineImpulseSource>();

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
    }

    private void OnEnable()
    {
        jumpAction.Enable();
        ultimateAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.Disable();
        ultimateAction.Disable();
    }

    private void Update()
    {

        ReadInput();
        if(InputManager.Instance.UltimatePressed && !isUsingUltimate)
        {
    

            InputManager.Instance.ConsumeUltimate();

            //Add Camera Shake
            if(impulseSource != null)
                impulseSource.GenerateImpulse();
            else
                Debug.LogWarning("Impulse not set");

            //StartCoroutine(UltimateFlashRoutine());
            StartCoroutine(SlowMotionCoroutine());
            StartCoroutine(UltimatePostProcessRoutine());
            StartCoroutine(UltimateRoutine());
        }

    }

    private void FixedUpdate()
    {
        if(isUsingUltimate) return;

        HandleJump();
        Move();
        CheckGameOver();
        //ApplyGravity();
    }

    private void ReadInput()
    {
        moveInput = InputManager.Instance.GetMoveInput();

        if(jumpAction.WasPressedThisFrame())
        {
            jumpPressed = true;
        }
    }

    private IEnumerator UltimateRoutine()
    {
        isUsingUltimate = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, ultimateDetectionRadius, enemyLayer);

        if (hits.Length == 0)
        {
            isUsingUltimate = false;
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
        isUsingUltimate = false;
    }

    private IEnumerator UltimatePostProcessRoutine()
    {
        if (ultimateVolume == null)
            yield break;

        float t = 0f;

        // Fade in
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

        // Hold during the slow-mo moment
        yield return new WaitForSecondsRealtime(slowMotionDuration);

        t = 0f;

        // Fade out
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

/*
    private IEnumerator UltimateStartupTimeRoutine()
    {
        Time.timeScale = 0.03f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        yield return new WaitForSecondsRealtime(0.04f);

        Time.timeScale = 0.18f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        yield return new WaitForSecondsRealtime(0.12f);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
*/

/*For Impact Frame later
    private IEnumerator UltimateFlashRoutine()
    {
        if(ultimateFlashImage != null)
            yield break;
        
        Color color = ultimateFlashImage.color;
        color.a = flashMaxAlpha;
        ultimateFlashImage.color = color;

        float timer = 0f;

        while(timer < flashDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / flashDuration;

            color.a = Mathf.Lerp(flashMaxAlpha, 0f, t);
            ultimateFlashImage.color = color;

            yield return null;
        }

        color.a = 0f;
        ultimateFlashImage.color = color;
    }
*/

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
        Instantiate(explosionVFX, enemyObject.transform.position, explosionVFX.transform.rotation);
        Destroy(enemyObject);
    }

    Transform GetClosestEnemy(Collider[] hits, HashSet<Transform> alreadyHit)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach(Collider hit in hits)
        {
            if(hit == null)
                continue;
            
            Transform enemy = hit.transform;

            if(alreadyHit.Contains(enemy))
                continue;

            float distance = Vector3.Distance(transform.position, enemy.position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    private void Move()
    {
        moveDirection.Set(moveInput.x, 0f, moveInput.y);
        moveDirection.Normalize();

        Vector3 currentVelocity = playerRigidbody.linearVelocity;
        Vector3 horizontalVelocity = currentVelocity;
        horizontalVelocity.y = 0;

        Vector3 targertVelocity = moveDirection * moveSpeed;

        float rate = (targertVelocity.magnitude > 0.1f) ? acceleration : deceleration;

        Vector3 newHorizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targertVelocity, rate * Time.fixedDeltaTime);

        playerRigidbody.linearVelocity = new Vector3(newHorizontalVelocity.x, currentVelocity.y, newHorizontalVelocity.z);
        
        graphic.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if(!jumpPressed)
            return;

        if(IsGrounded())
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

    public void UpdgradeSpeed(float speedMultiplier)
    {
        if((moveSpeed + moveSpeed*speedMultiplier) >= maxSpeed + 0.5f)
            return;
        moveSpeed += moveSpeed*speedMultiplier;
        rotateSpeed += rotateSpeed*speedMultiplier;
    } 


    private void CheckGameOver()
    {
        if(this.transform.position.y < -10f)
        {
            Debug.Log("GameOver");
            StartCoroutine(RestartLevel());
        }
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }

    public void AddToScore(int value)
    {
        currentScore += value;
        Debug.Log(Score);
    }

}
