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
using Nethereum.Contracts.Extensions;

public class NFT
{
    internal string contractAddress;
    internal string networkName;
    internal string defaultAccount;
    public Exception Exception;
    public BigInteger Balance;
    public BigInteger[] Tokens;
    public String mintReceipt;

    public NFT(string contractAddress, string networkName)
    {
        this.contractAddress = contractAddress;
        this.networkName = networkName;
    }

    internal IEnumerator GetAccounts() {
        var accounts = new EthAccountsUnityRequest(networkName);
        yield return accounts.SendRequest();

        if (accounts.Exception != null) {
            this.Exception = accounts.Exception;
            yield break;
        }

        if (accounts.Result.Length < 1) {
            yield break;
        }

        defaultAccount = accounts.Result.First();
    }

    public IEnumerator GetFirsAccountTokens() {
        this.Exception = null;

        if (defaultAccount == null) {
            yield return GetAccounts();

            if (this.Exception != null) {
                yield break;
            }
        }

        if (defaultAccount == null) {
            Tokens = new BigInteger[0];
            Balance = BigInteger.Zero;
            yield break;
        }

        yield return GetBalance(defaultAccount);

        if (Exception != null) {
            yield break;
        }

        Tokens = new BigInteger[(int)Balance];
        for (BigInteger i = BigInteger.Zero; i < Balance; i++)
        {
            yield return GetToken(defaultAccount, i);
        }
    }

    internal IEnumerator GetBalance(string account)
    {
        var functionMessage = new BalanceOfFunctionMessage
        {
            Owner = account,
        };

        var request = new QueryUnityRequest<BalanceOfFunctionMessage, BalanceOfDTO>(defaultAccount, networkName);
        yield return request.Query(functionMessage, contractAddress);

        if (request.Exception != null) {
            Exception = request.Exception;
            yield break;
        }

        Balance = request.Result.Balance;
    }

    internal IEnumerator GetToken(string account, BigInteger index) {
        var functionMessage = new TokenOfOwnerByIndexFunctionMessage
        {
            Owner = account,
            Index = index,
        };
        var request = new QueryUnityRequest<TokenOfOwnerByIndexFunctionMessage, TokenDTO>(defaultAccount, networkName);
        yield return request.Query(functionMessage, contractAddress);

        if (request.Exception == null) {
            Tokens[(int)index] = request.Result.Token;
        } else {
            this.Exception = request.Exception;
        }
    }

    public IEnumerator Mint(BigInteger TokenId) {
        Exception = null;

        if (defaultAccount == null) {
            yield return GetAccounts();

            if (Exception != null) {
                Debug.Log("Got an error: " + Exception.Message);
                yield break;
            }
        }
        
        Debug.Log("Minting");
        var functionMessage = new MintToFunctionMessage
        {
            To = defaultAccount,
            TokenId = TokenId,
        };
        var transaction = new TransactionSignedUnityRequest(networkName, defaultAccount);
        yield return transaction.SignAndSendTransaction(functionMessage, contractAddress);

        if (transaction.Exception != null) {
            this.Exception = transaction.Exception;
            yield break;
        }

        this.mintReceipt = transaction.Result;
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

[Function("mint", "string")]
public class MintFunctionMessage : FunctionMessage
{
    [Parameter("uint256", "_tokenId", 1)]
    public BigInteger TokenId { get; set; }
}

[Function("mintTo", "string")]
public class MintToFunctionMessage : FunctionMessage
{
    [Parameter("address", "_to", 1)]
    public String To { get; set; }

    [Parameter("uint256", "_tokenId", 2)]
    public BigInteger TokenId { get; set; }
}