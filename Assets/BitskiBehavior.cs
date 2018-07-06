using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Collections;
using AOT;

public class BitskiBehavior : MonoBehaviour
{
    private static BitskiBehavior callbackInstance;

    public Button signInButton;

    public SpriteRenderer[] characterSpriteRenderers;
    private Sprite[] characterSprites;

    void Start()
    {
        characterSprites = Resources.LoadAll<Sprite>("Characters");
        signInButton.GetComponent<Button>().onClick.AddListener(SignInButtonClicked);
    }

    void SignInButtonClicked()
    {
        SignIn();
    }

    public IEnumerator GetAccountBalance(string accessToken) {
        var nft = new NFT(accessToken);
        yield return nft.GetAccountBalance();
        showTokens(nft);
        signInButton.gameObject.SetActive(false);
    }

    void showTokens(NFT nft) {
        for (int i = 0; i < characterSpriteRenderers.Length; i++)
        {
            if (i >= nft.tokens.tokenIDs.Count - 1)
            {
                characterSpriteRenderers[i].enabled = false;
                break;
            }

            var tokenID = nft.tokens.tokenIDs[i];
            int spriteID = (int)(tokenID % 5);
            characterSpriteRenderers[i].sprite = characterSprites[spriteID];
            characterSpriteRenderers[i].enabled = true;
        }

    }

    private delegate void BitskiSignInCallback(string token, string error);

    #if UNITY_IPHONE
    [DllImport("__Internal")]
    #endif
    private static extern void BitskiSignIn(string ClientID, string CallbackUri, BitskiSignInCallback callback);

    void SignIn()
    {
        callbackInstance = this;
        BitskiSignIn("35a7e890-2f64-4332-b5bc-ee556bde5cf1", "bitskiexampledapp://application/callback", SignInCallback);
    }


    [MonoPInvokeCallback(typeof(BitskiSignInCallback))]
    static void SignInCallback(string accessToken, string error)
    {
        if (accessToken != null)
        {
            callbackInstance.didSignIn(accessToken);
        }

        if (error != null)
        {
            Debug.Log("Got an error" + error);
        }

        callbackInstance = null;
    }

    public void didSignIn(string accessToken)
    {
        StartCoroutine(GetAccountBalance(accessToken));
    }
}
