using UnityEngine;

public class EnableAndDisableGameObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject _gameobject;
    public void ActiveGameObject() => _gameobject.SetActive(true);
    public void DeactiveGameObject() => _gameobject.SetActive(false);

}
