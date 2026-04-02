using System.Collections;
using System.Collections.Generic;
using System.Security;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class Boss3SmokeArea : MonoBehaviour
{
    [SerializeField] ParticleSystem _smokeAreaParticle;

    [SerializeField] float _smokeLifeTime = 5f;

    int _smokeHiddenLayer ;
    
    private Camera _mainCamera;

    private List<GameObject> _hiddenEnemys = new List<GameObject>();


    void Awake()
    {
        _smokeHiddenLayer = LayerMask.NameToLayer("SmokeHidden");
        _mainCamera = Camera.main;
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

        if (other.CompareTag("Enemy") && !_hiddenEnemys.Contains(other.gameObject))
        {
            _hiddenEnemys.Add(other.gameObject);
            other.GetComponent<MeshRenderer>().enabled = false;
            
        }
        if (other.CompareTag("Player"))
        {
            _mainCamera.cullingMask &= ~(1<<_smokeHiddenLayer);
        }
    }
    private void OnTriggerExit(Collider other) {
        _mainCamera.cullingMask |= 1<<_smokeHiddenLayer;

        if (_hiddenEnemys.Contains(other.gameObject))
        {
            other.GetComponent<MeshRenderer>().enabled = true;
            _hiddenEnemys.Remove(other.gameObject);
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
            enemy.GetComponent<MeshRenderer>().enabled = true; 
        }

        _mainCamera.cullingMask |= 1<<_smokeHiddenLayer;
        yield return null;
        Destroy(gameObject);
    }
}
