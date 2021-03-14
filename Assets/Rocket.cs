using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    Rigidbody rigidbody;
    AudioSource audioSource;
    bool collisionsDisabled = false;

    enum State
    {
        Alive, Dying, Transcending
    }
    State state = State.Alive;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        state = State.Alive;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            Thrust();
            Rotate();
        }
        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisabled = !(collisionsDisabled);
        }
    }

    private void Thrust()
    {
        float thrustThisFrame = mainThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))
        {
            rigidbody.AddRelativeForce(Vector3.up * thrustThisFrame);
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(mainEngine);
            }
            mainEngineParticles.Play();
        }
        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
        }
    }
    private void Rotate()
    {
        rigidbody.angularVelocity = Vector3.zero; //take manual control of rotation
        float rotationThisFrame = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidbody.freezeRotation = false;
    }

    void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1 < SceneManager.sceneCountInBuildSettings ? currentSceneIndex + 1 : 0;
        SceneManager.LoadScene(nextSceneIndex);
    }

    void LoadStartScene()
    {
        SceneManager.LoadScene(0);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || collisionsDisabled)
        {
            return;
        }
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;

            case "Finish":
                state = State.Transcending;
                audioSource.Stop();
                audioSource.PlayOneShot(success);
                successParticles.Play();
                Invoke("LoadNextScene", 1f);
                break;

            default:
                state = State.Dying;
                audioSource.Stop();
                audioSource.PlayOneShot(death);
                deathParticles.Play();
                Invoke("LoadStartScene", 1f);
                break;
        }
    }
}
