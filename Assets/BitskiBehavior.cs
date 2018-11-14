using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using Nethereum.ABI;
using Bitski;
using Bitski.Unity.Rpc;
using Bitski.Auth;
using System;
using System.Numerics;

public class BitskiBehavior : MonoBehaviour
{
    public Button signInButton;
    public Text feedbackLabel;

    public SpriteRenderer[] characterSpriteRenderers;
    private Sprite[] characterSprites;

    void Awake() {
        #if !UNITY_EDITOR && UNITY_WEBGL
        UnityEngine.WebGLInput.captureAllKeyboardInput = false;
        #endif

        if (!BitskiSDK.IsInitialized)
        {
            try
            {
                BitskiSDK.Init("7193f979-c677-4276-bb54-5bc9a64a5f84");
            }
            catch(Exception e)
            {
                Debug.Log(e);  
            }
        } else {
            Debug.Log("Already initialized");
        }
    }

    void Start()
    {
        characterSprites = Resources.LoadAll<Sprite>("Characters");

        // For WebGL we need to hook in to to the PointerDown evenr, not the regular click event.
        // In the future we will have a custom Bitski sign in button that handles this automatically.
        EventTrigger trigger = signInButton.gameObject.AddComponent<EventTrigger>();
        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => SignInButtonClicked());
        trigger.triggers.Add(pointerDown);
    }

    void SignInButtonClicked()
    {
        feedbackLabel.enabled = true;
        feedbackLabel.text = "Signing in";
        signInButton.gameObject.SetActive(false);

        BitskiSDK.SignIn(SignInCallback);
    }

    public IEnumerator GetAccountTokens() {
        feedbackLabel.enabled = true;
        feedbackLabel.text = "Querying ethereum for tokens";

#if UNITY_EDITOR
        var networkName = "ganache";
        var contractAddress = "0x345ca3e014aaf5dca488057592ee47305d9b3e10";
#else
        var networkName = "rinkeby";
        var contractAddress = "0x8f83aadb8098a1b4509aaba77ba9d2cb1ac970ba";
#endif
        var nft = new NFT(BitskiSDK.AuthProviderImpl, contractAddress, networkName);
        yield return nft.GetFirsAccountTokens();

        if (nft.Tokens != null) {
            showTokens(nft.Tokens);
            signInButton.gameObject.SetActive(false);
        }
        
        if (nft.Exeption != null) {
            Debug.Log("Got an error: " + nft.Exeption.Message);
        }
    }

    void showTokens(BigInteger[] tokens) {
        if (tokens.Length < 1) {
            feedbackLabel.enabled = true;
            feedbackLabel.text = "You don't have any tokens";
        } else {
            feedbackLabel.enabled = false;
        }

        for (int i = 0; i < characterSpriteRenderers.Length; i++)
        {
            if (i >= tokens.Length)
            {
                characterSpriteRenderers[i].enabled = false;
                break;
            }

            var tokenID = tokens[i];
            int spriteID = (int)(tokenID % 5);
            characterSpriteRenderers[i].sprite = characterSprites[spriteID];
            characterSpriteRenderers[i].enabled = true;
        }
    }

    void SignInCallback(User user)
    {
        if (user == null) {
            feedbackLabel.enabled = false;
            signInButton.gameObject.SetActive(true);
        } else {
            StartCoroutine(GetAccountTokens());
        }
    }
}
