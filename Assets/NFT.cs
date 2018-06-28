using System.Collections;
using System.Collections.Generic;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

using System.Numerics;

using UnityEngine;
using Bitski.Rpc;

public class NFT
{
    public static string ABI = @"[{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'getOwnerTokens','outputs':[{'name':'_tokenIds','type':'uint256[]'}],'payable':false,'stateMutability':'view','type':'function'}]";

    private static string contractAddress = "0x8c51dff8fcd48c292354ee751cceabeb25357df4";
    private Contract contract;
    private string accessToken;
    private string[] accounts;
    public TokensDTO tokens;

    public NFT(string accessToken)
    {
        this.accessToken = accessToken;
        this.contract = new Contract(null, ABI, contractAddress);
    }

    public IEnumerator GetAccountBalance()
    {
        yield return GetAccounts();
        yield return GetBalance(accounts[0]);
    }

    public IEnumerator GetAccounts()
    {
        var getDataCallUnityRequest = new EthAccountsBitskiUnityRequest(accessToken);
        yield return getDataCallUnityRequest.SendRequest();
        accounts = getDataCallUnityRequest.Result;
    }

    public IEnumerator GetBalance(string account)
    {
        var getDataCallUnityRequest = new EthCallBitskiUnityRequest(accessToken);
        var function = contract.GetFunction("getOwnerTokens");
        var callInput = function.CreateCallInput(account);
        callInput.From = account;
        callInput.To = contract.Address;

        Debug.Log("callInput data is " + callInput.Data);
        Debug.Log("callInput from is " + callInput.From);
        Debug.Log("callInput to is " + callInput.To);

        yield return getDataCallUnityRequest.SendRequest(callInput);
        var result = getDataCallUnityRequest.Result;

        tokens = new TokensDTO();
        function.DecodeDTOTypeOutput<TokensDTO>(tokens, result);
    }

    [FunctionOutput]
    public class TokensDTO
    {
        [Parameter("uint256[]", "_tokenIds", 1)]
        public List<BigInteger> tokenIDs { get; set; }
    }

}
