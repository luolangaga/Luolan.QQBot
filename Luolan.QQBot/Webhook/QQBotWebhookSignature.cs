using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace Luolan.QQBot.Webhook;

/// <summary>
/// Webhook Ed25519 签名校验工具
/// </summary>
public static class QQBotWebhookSignature
{
    /// <summary>
    /// 校验回调请求签名(X-Signature-Ed25519 + X-Signature-Timestamp)
    /// </summary>
    public static bool Verify(string clientSecret, string body, string? signature, string? timestamp)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp))
            return false;

        byte[] signatureBytes;
        try
        {
            signatureBytes = Convert.FromHexString(signature);
        }
        catch (FormatException)
        {
            return false;
        }

        var message = Encoding.UTF8.GetBytes(timestamp + body);
        var privateKey = new Ed25519PrivateKeyParameters(DeriveSeed(clientSecret), 0);
        var verifier = new Ed25519Signer();
        verifier.Init(false, privateKey.GeneratePublicKey());
        verifier.BlockUpdate(message, 0, message.Length);
        return verifier.VerifySignature(signatureBytes);
    }

    /// <summary>
    /// 计算回调地址验证(OpCode 13)所需的签名
    /// </summary>
    public static string SignValidation(string clientSecret, string plainToken, string eventTs)
    {
        var privateKey = new Ed25519PrivateKeyParameters(DeriveSeed(clientSecret), 0);
        var signer = new Ed25519Signer();
        signer.Init(true, privateKey);

        var message = Encoding.UTF8.GetBytes(eventTs + plainToken);
        signer.BlockUpdate(message, 0, message.Length);
        return Convert.ToHexString(signer.GenerateSignature()).ToLowerInvariant();
    }

    /// <summary>
    /// 将 ClientSecret 循环填充/截断为 32 字节 Ed25519 seed
    /// </summary>
    private static byte[] DeriveSeed(string clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret))
            throw new ArgumentException("ClientSecret 不能为空", nameof(clientSecret));

        var secretBytes = Encoding.UTF8.GetBytes(clientSecret);
        var seed = new byte[32];
        for (var i = 0; i < seed.Length; i++)
        {
            seed[i] = secretBytes[i % secretBytes.Length];
        }
        return seed;
    }
}
