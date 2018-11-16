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
    public Button mintButton;
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
                Debug.Log(e.Message);  
            }
        } else {
            Debug.Log("Already initialized");
        }
    }

    void Start()
    {
        characterSprites = Resources.LoadAll<Sprite>("Characters");

        // For WebGL we need to hook in to to the PointerDown event, not the regular click event.
        // In the future we will have a custom Bitski sign in button that handles this automatically.
        EventTrigger trigger = signInButton.gameObject.AddComponent<EventTrigger>();
        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => SignInButtonClicked());
        trigger.triggers.Add(pointerDown);

        mintButton.onClick.AddListener(MintButtonClicked);
    }

    void SignInButtonClicked()
    {
        feedbackLabel.enabled = true;
        feedbackLabel.text = "Signing in";
        signInButton.gameObject.SetActive(false);

        BitskiSDK.SignIn(SignInCallback);
    }

    void MintButtonClicked() {
        StartCoroutine(Mint());
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
        var nft = new BitskiNFT(contractAddress, networkName);
        yield return nft.GetFirsAccountTokens();

        if (nft.Tokens != null) {
            showTokens(nft.Tokens);
            signInButton.gameObject.SetActive(false);
        }
        
        if (nft.Exception != null) {
            Debug.Log("Got an error: " + nft.Exception.Message);
            feedbackLabel.text = nft.Exception.Message;
        }
    }

    public BigInteger getRandom(int bits){
        var random = new System.Random();
        byte[] data = new byte[bits / 8];
        random.NextBytes(data);
        var result = new BigInteger(data);
        if (result < BigInteger.Zero) {
            result = result * BigInteger.MinusOne; 
        }
        return result;
    }
    public IEnumerator Mint() {
        hideTokens();
        feedbackLabel.enabled = true;

        feedbackLabel.text = "Waiting for approval";

#if UNITY_EDITOR
        var networkName = "ganache";
        var contractAddress = "0x345ca3e014aaf5dca488057592ee47305d9b3e10";
#else
        var networkName = "rinkeby";
        var contractAddress = "0x8f83aadb8098a1b4509aaba77ba9d2cb1ac970ba";
#endif
        var nft = new BitskiNFT(contractAddress, networkName);

        yield return nft.Mint(getRandom(256));
        
        if (nft.Exception != null) {
            Debug.Log("Got an error: " + nft.Exception.Message);
            feedbackLabel.text = nft.Exception.Message;
            yield break;
        }

        feedbackLabel.text = "Waiting for transaction receipt";

        var pollingRequest = new TransactionReceiptPollingRequest(networkName);
        yield return pollingRequest.PollForReceipt(nft.mintReceipt, 1);

        if (pollingRequest.Exception != null) {
            Debug.Log("Got an error: " + pollingRequest.Exception.Message);
            feedbackLabel.text = pollingRequest.Exception.Message;
            yield break;
        }

        yield return GetAccountTokens();
    }

    void showTokens(BigInteger[] tokens) {
        if (tokens.Length < 1) {
            feedbackLabel.enabled = true;
            feedbackLabel.text = "You don't have any tokens";
        } else {
            feedbackLabel.enabled = false;
        }

        if (tokens.Length < 5) {
            mintButton.gameObject.SetActive(true);
        } else {
            mintButton.gameObject.SetActive(false);
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

    void hideTokens() {
        for (int i = 0; i < characterSpriteRenderers.Length; i++)
        {
            characterSpriteRenderers[i].enabled = false;
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
