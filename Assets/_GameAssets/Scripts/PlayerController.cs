using UnityEngine;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _orientationTransform;
    private Rigidbody _playerRigidbody;
    private float _movementSpeed = 25f;
    private float _verticalInput, _horizontalInput;


    [Header("Movement Settings")]
    private Vector3 _movementDirection;
    [SerializeField] private KeyCode _movementKey;


    [Header("Jump Settings")]
    [SerializeField] private KeyCode _jumpKey;
    [SerializeField] private float _jumpForce;
    [SerializeField] private bool _canJump;
    [SerializeField] private float _jumpCooldown;


    [Header("Ground Check Settings")]
    [SerializeField] private float _playerHight;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundDrag;


    [Header("Sliding Settings")]
    [SerializeField] private KeyCode _slideKey;
    [SerializeField] private float _slideMultiplier;
    [SerializeField] private float _slideDrag;
    private bool _isSliding;
    

    void Awake()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerRigidbody.freezeRotation = true;
    }

    void Update()
    {
        SetInputs();
        SetPlayerDamping();
        LimitPlayerSpeed();
    }

    void FixedUpdate()
    {
        SetPlayerMovement();
    }

    private void SetInputs()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(_slideKey)){
            _isSliding = true;
            Debug.Log("Player, Sliding!");
        }
        else if(Input.GetKeyDown(_movementKey)){
            _isSliding = false;
            Debug.Log("Player, Moving!");
        }

        else if(Input.GetKey(_jumpKey) && _canJump && IsGrounded()){
            //Zıplama işlemi gerçekleşecek
            _canJump = false;
            SetPlayerJumping();
            Invoke(nameof(ResetJumping), _jumpCooldown); // fonksiyondan sonra jumpcooldown kadar bekliyor
        }
    }
    
    private void SetPlayerMovement()
    {
        _movementDirection = _orientationTransform.forward * _verticalInput + _orientationTransform.right * _horizontalInput;

        if(_isSliding){
        //.normalized mesela a ve w ya basınca birlikte sol yukarı da aynı hızda gitmesini sağlıyor
        _playerRigidbody.AddForce(_movementDirection.normalized * _movementSpeed* _slideMultiplier, ForceMode.Force); //forcemode.force hareket etme gibi sürekli yapılan hareketlerde kullanılıyor
        }
        else
        _playerRigidbody.AddForce(_movementDirection.normalized * _movementSpeed, ForceMode.Force);
    }

    private void SetPlayerDamping()
    { 
        //rigidbodydeki damping yani çevreden etkilenme olayını (yercekimi mesela) ayarlıyo (linearDamping = drag)
        if(_isSliding){
            _playerRigidbody.linearDamping = _slideDrag;
        }
        else{
            _playerRigidbody.linearDamping = _groundDrag;
        }
    }

    private void LimitPlayerSpeed()
    {
        Vector3 flatVelocity = new Vector3(_playerRigidbody.linearVelocity.x, 0f, _playerRigidbody.linearVelocity.z);

        if(flatVelocity.magnitude > _movementSpeed){//flatvelocity.magnitude = flatvelocitynin büyüklüğü demek
        Vector3 limitedVelocity = flatVelocity.normalized * _movementSpeed;
        _playerRigidbody.linearVelocity = new Vector3(limitedVelocity.x, _playerRigidbody.linearVelocity.y, limitedVelocity.z); //x ve z yi limitledik y yi limitlemeye gerek yok diye rigidbodydeki değerin aynısını çektik
        }
    }

    private void SetPlayerJumping()
    {
        _playerRigidbody.linearVelocity = new Vector3(_playerRigidbody.linearVelocity.x, 0f, _playerRigidbody.linearVelocity.z); //y pozisyonunu her zıplamadan önce sıfırlamak için kullanıyoruz (y eksenindeki hızımızı sıfırlıyoruz ki zıplama bozulmasın)
        _playerRigidbody.AddForce(transform.up * _jumpForce, ForceMode.Impulse); //addforce kuvvet uyguluyor oraya gidiyor Forcemode.impulse ise ani hareketler için kullanılıyor
    }

    private void ResetJumping()
    {
        _canJump = true;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, _playerHight * 0.5f + 0.2f, _groundLayer); // karakterin altından ışın atıp yerde olup olmadığının kontrolünü yapıyoruz 2. virgülden 2. virgüle kadar olan kısım kadar büyüklükte groundLayere kadar ışın gönderecek
    }
}
