﻿// hide some meshes in first person. only display shadows.
// => this is better than using a transparent texture because this way we can
//    still display those parts in the equipment avatar
using UnityEngine;
using UnityEngine.Rendering;

public class FirstPersonClipping : MonoBehaviour
{
    [Header("Components")]
    public PlayerLook look;
    public PlayerEquipment equipment;

    [Header("Mesh Hiding")]
    public Transform[] hideRenderers; // transform because equipment slots won't have renderers initially

    [Header("Disable Depth Check (to avoid clipping)")]
    public string noDepthLayer = "NoDepthInFirstPerson";
    public Renderer[] disableArmsDepthCheck;
    Camera weaponCamera;

    void Start()
    {
        // find weapon camera
        foreach (Transform t in Camera.main.transform)
            if (t.CompareTag("WeaponCamera"))
                weaponCamera = t.GetComponent<Camera>();
    }

    void HideMeshes(bool firstPerson)
    {
        // need to hide some meshes in first person so we don't see ourselves
        // when looking downwards.
        // => destroying won't work because it doesn't work for the textmesh
        // => disabling the renderer won't work because IK stops working
        // => swapping out the layer with a layer hidden from camera works, but
        //    shows no more shadows either
        // ==> Unity Renderer has a ShadowCasting mode. if we set it to shadows-
        //     only then the mesh isn't shown, but shadow is still shown.
        //     (perfect solution for first person!)
        foreach (Transform tf in hideRenderers)
            foreach (Renderer rend in tf.GetComponentsInChildren<Renderer>())
                rend.shadowCastingMode = firstPerson ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
    }

    void DisableDepthCheck(bool firstPerson)
    {
        // enable weapon camera only in first person
        // (to draw arms and weapon without depth check to avoid clipping
        //  through walls)
        if (weaponCamera != null)
            weaponCamera.enabled = firstPerson;

        // convert name to layer only once
        int noDepth = LayerMask.NameToLayer(noDepthLayer);

        // set weapon layer to NoDepth (only for localplayer so we don't see
        // others without depth checks)
        // -> do for arms etc.
        foreach (Renderer renderer in disableArmsDepthCheck)
            renderer.gameObject.layer = noDepth;

        // -> do for weapons
        foreach (Renderer renderer in equipment.leftHandLocation.GetComponentsInChildren<Renderer>())
            renderer.gameObject.layer = noDepth;

        foreach (Renderer renderer in equipment.rightHandLocation.GetComponentsInChildren<Renderer>())
            renderer.gameObject.layer = noDepth;
    }

    void Update()
    {
        // only hide while in first person mode
        bool firstPerson = look.InFirstPerson();
        HideMeshes(firstPerson);
        DisableDepthCheck(firstPerson);
    }
}
