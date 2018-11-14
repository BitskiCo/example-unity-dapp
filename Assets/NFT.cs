using System;
using System.Linq;
using System.Numerics;
using System.Collections;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

using Bitski;
using Bitski.Unity.Rpc;
using Bitski.Auth;
using UnityEngine;
using Nethereum.JsonRpc.Client;

public class NFT
{
    private string contractAddress;
    private string networkName;
    private AuthProvider authProvider;
    private string defaultAccount;
    public Exception Exeption;
    public BigInteger Balance;
    public BigInteger[] Tokens;

    public NFT(AuthProvider authProvider, string contractAddress, string networkName)
    {
        this.authProvider = authProvider;
        this.contractAddress = contractAddress;
        this.networkName = networkName;
    }

    public IEnumerator GetFirsAccountTokens() {
        var accounts = new EthAccountsUnityRequest(authProvider, networkName);
        yield return accounts.SendRequest();

        if (accounts.Exception != null) {
            Exeption = accounts.Exception;
            yield break;
        }

        if (accounts.Result.Length < 1) {
            Tokens = new BigInteger[0];
            Balance = BigInteger.Zero;
            yield break;
        }

        defaultAccount = accounts.Result.First();
        yield return GetBalance(defaultAccount);

        if (accounts.Exception != null) {
            Exeption = accounts.Exception;
            yield break;
        }

        Tokens = new BigInteger[(int)Balance];
        for (BigInteger i = BigInteger.Zero; i < Balance; i++)
        {
            yield return GetToken(defaultAccount, i);
        }
    }

    public IEnumerator GetBalance(string account)
    {
        var functionMessage = new BalanceOfFunctionMessage
        {
            Owner = account,
        };

        var request = new QueryUnityRequest<BalanceOfFunctionMessage, BalanceOfDTO>(defaultAccount, BitskiSDK.AuthProviderImpl, networkName);
        yield return request.Query(functionMessage, contractAddress);

        Balance = request.Result.Balance;
    }

    public IEnumerator GetToken(string account, BigInteger index) {
        var functionMessage = new TokenOfOwnerByIndexFunctionMessage
        {
            Owner = account,
            Index = index,
        };
        var request = new QueryUnityRequest<TokenOfOwnerByIndexFunctionMessage, TokenDTO>(defaultAccount, BitskiSDK.AuthProviderImpl, networkName);
        yield return request.Query(functionMessage, contractAddress);

        if (request.Exception == null) {
            Tokens[(int)index] = request.Result.Token;
        } else {
            Exeption = request.Exception;
        }
    }
}

[Function("balanceOf", typeof(BalanceOfDTO))]
public class BalanceOfFunctionMessage : FunctionMessage
{
    [Parameter("address", "owner", 1)]
    public string Owner { get; set; }
}

[FunctionOutput]
public class BalanceOfDTO: IFunctionOutputDTO {
    [Parameter("uint256", "", 1)]
    public BigInteger Balance { get; set; }
}

[Function("tokenOfOwnerByIndex", typeof(TokenDTO))]
public class TokenOfOwnerByIndexFunctionMessage : FunctionMessage
{
    [Parameter("address", "owner", 1)]
    public string Owner { get; set; }

    [Parameter("uint256", "index", 2)]
    public BigInteger Index { get; set; }
}

[FunctionOutput]
public class TokenDTO : IFunctionOutputDTO
{
    [Parameter("uint256", "", 1)]
    public BigInteger Token { get; set; }
}
