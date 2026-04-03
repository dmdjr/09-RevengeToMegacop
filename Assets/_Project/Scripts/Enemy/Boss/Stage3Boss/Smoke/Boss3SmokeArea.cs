using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss3SmokeArea : MonoBehaviour
{
    [Header("SmokeSetting")]
    [SerializeField] ParticleSystem _smokeAreaParticle;

    [SerializeField] float _smokeLifeTime = 5f;


    [Header("상시 감춰야 하는 레이어")]
    [Tooltip("Enemy가 연막 안에 들어왔을 때 바뀔 레이어 번호 (예: SmokeHide)")]
    [SerializeField] private int _hideLayer;

    [Header("Enemy")]
    [Tooltip("연막 안에 들어가면 가려지는 레이어")]
    [SerializeField] int _EnemyLayer;

    [Header("연막에 가려지는 레이어")]
    [Tooltip("플레이어가 연막 안에 들어왔을 때 카메라에서 숨길 레이어들")]

    [SerializeField] LayerMask _beHideLayers;

    
    
    private Camera _mainCamera;

    private List<GameObject> _hiddenEnemys = new List<GameObject>();


    void Awake()
    {
        _mainCamera = Camera.main;
        if(_mainCamera != null)
        _mainCamera.cullingMask &= ~(1<<_hideLayer);
        
    }
    

    void OnEnable()
    {
        CreateArea();
        
        
    }

    public void CreateArea()
    {
        StartCoroutine(PlaySmokeArea());

    }

    void OnTriggerEnter(Collider other) {
        Debug.Log(other.name + "연막 안에 들어옴");

        if (other.CompareTag("Enemy"))
        {
           GameObject enemyRoot = other.transform.root.gameObject;

            if (!_hiddenEnemys.Contains(enemyRoot))
            {
                _hiddenEnemys.Add(enemyRoot);
                SetTargetLayer(enemyRoot, _hideLayer);
            }
            
        }
        if (other.CompareTag("Player"))
        {
            MainCameraLayerSet(_beHideLayers,false);
            
        }
    }
    private void OnTriggerExit(Collider other) {
        
        if (other.CompareTag("Player"))
        {
            MainCameraLayerSet(_beHideLayers,true);
        }

        if (other.CompareTag("Enemy"))
        {
            GameObject enemyRoot = other.transform.root.gameObject;

            if (_hiddenEnemys.Contains(enemyRoot))
            {
                SetTargetLayer(enemyRoot, _EnemyLayer);
                _hiddenEnemys.Remove(enemyRoot);
            }
        }
    }

    IEnumerator PlaySmokeArea()
    {
        _smokeAreaParticle.Play();

        yield return new WaitForSeconds(_smokeLifeTime);

        _smokeAreaParticle.Stop();

        foreach(GameObject enemy in _hiddenEnemys)
        {
            if(enemy == null) continue;
            SetTargetLayer(enemy, _EnemyLayer); 
        }
        _hiddenEnemys.Clear();

        MainCameraLayerSet(_beHideLayers,true);

        yield return null;
        Destroy(gameObject);
    }


    private void SetTargetLayer(GameObject target, LayerMask currentLayer)
    {
        target.layer = currentLayer;

        foreach(Transform child in target.transform)
        {
            SetTargetLayer(child.gameObject,currentLayer);
        }
    }

    private void MainCameraLayerSet(LayerMask layers , bool isactiveLayers)
    {
        if(_mainCamera ==null)return;

        
            if(isactiveLayers == true)
            {
                _mainCamera.cullingMask |= layers.value;
            }else
            {
                _mainCamera.cullingMask &= ~layers.value;
            }
        
        

    }
}
