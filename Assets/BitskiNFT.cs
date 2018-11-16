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

public class BitskiNFT : NFT {
	public BitskiNFT(string contractAddress, string networkName): base(contractAddress, networkName) {}

	new public IEnumerator Mint(BigInteger TokenId) {
			Exception = null;

			if (defaultAccount == null) {
					yield return GetAccounts();

					if (Exception != null) {
							Debug.Log("Got an error: " + Exception.Message);
							yield break;
					}
			}
			
			Debug.Log("Minting");

			var tokenIDString = TokenId.ToString();
			
			var functionMessage = new MintWithTokenURIFunctionMessage {
					To = defaultAccount,
					TokenId = TokenId,
					TokenURI = "https://example-dapp-1-api.bitski.com/tokens/" + tokenIDString,
			};

			Debug.Log("Minting token with URI: " + functionMessage.TokenURI);

			var transaction = new TransactionSignedUnityRequest(networkName, defaultAccount);
			yield return transaction.SignAndSendTransaction(functionMessage, contractAddress);

			if (transaction.Exception != null) {
					this.Exception = transaction.Exception;
					yield break;
			}

			this.mintReceipt = transaction.Result;
	}
}

[Function("mintWithTokenURI", "string")]
public class MintWithTokenURIFunctionMessage : FunctionMessage
{
    [Parameter("address", "_to", 1)]
    public String To { get; set; }

		[Parameter("uint256", "_tokenId", 2)]
    public BigInteger TokenId { get; set; }

		[Parameter("string", "_tokenURI", 3)]
    public String TokenURI { get; set; }
}