using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AINoisemaker : MonoBehaviour
{
    [SerializeField] AudioSource Audio;

    //[SerializeField] AISoundRelay Relay;
    [SerializeField] AIRelayChannel RelayChannel;

    public Noise[] Noises;

    // Start is called before the first frame update
    void Start()
    {
        if(!Audio)
        {
            Audio = GetComponent<AudioSource>();
        }
    }

    // private void Update()
    // {
    //     CreateOneShotNoise(0);
    // }

    //Creates a sphere around the source of the noise to see if an AI hears it
    public void CreateOneShotNoise(int NoiseIndex)
    {
        if(Noises[NoiseIndex].Clip != null)
        {
            Audio.PlayOneShot(Noises[NoiseIndex].Clip);
        }
        else
        {
            Debug.LogWarning("The Noise object at index " + NoiseIndex + " has an unassigned AudioClip!", gameObject);
        }
        

        //Broadcasts the noise if it is global
        if (Noises[NoiseIndex].IsGlobal)
        {
            //Relay.BroadcastNoise(transform.position, Noises[NoiseIndex].PriorityLevel);
            RelayChannel.BroadcastNoise(transform.position, Noises[NoiseIndex].PriorityLevel);
            return;
        }

        //Checks for anything in the sound range
        Collider[] cols = Physics.OverlapSphere(transform.position, Noises[NoiseIndex].Radius, Noises[NoiseIndex].AIMasks, QueryTriggerInteraction.Collide);

        for(int i = 0; i < cols.Length; i++)
        {
            //Checks if they have the AIHearing component
            if (cols[i].gameObject.TryGetComponent(out AIHearing Hearing))
            {

                Hearing.DetectNoise(transform.position, Noises[NoiseIndex].PriorityLevel);
            }
        }

        
    }



}
