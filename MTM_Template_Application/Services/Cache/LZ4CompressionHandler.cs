using System;
using System.Text;
using K4os.Compression.LZ4;

namespace MTM_Template_Application.Services.Cache;

/// <summary>
/// Compress/decompress cache entries using LZ4
/// </summary>
public class LZ4CompressionHandler
{
    /// <summary>
    /// Compress a string using LZ4
    /// </summary>
    public byte[] Compress(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var sourceBytes = Encoding.UTF8.GetBytes(data);
        var targetBytes = new byte[LZ4Codec.MaximumOutputSize(sourceBytes.Length)];
        
        var encodedLength = LZ4Codec.Encode(
            sourceBytes, 0, sourceBytes.Length,
            targetBytes, 0, targetBytes.Length,
            LZ4Level.L00_FAST);

        // Resize to actual compressed size
        Array.Resize(ref targetBytes, encodedLength);
        
        return targetBytes;
    }

    /// <summary>
    /// Decompress LZ4 data to string
    /// </summary>
    public string Decompress(byte[] compressedData)
    {
        ArgumentNullException.ThrowIfNull(compressedData);

        // Allocate buffer for decompression (assume max 10x compression ratio)
        var targetBytes = new byte[compressedData.Length * 10];
        
        var decodedLength = LZ4Codec.Decode(
            compressedData, 0, compressedData.Length,
            targetBytes, 0, targetBytes.Length);

        if (decodedLength < 0)
        {
            throw new InvalidOperationException("LZ4 decompression failed");
        }

        return Encoding.UTF8.GetString(targetBytes, 0, decodedLength);
    }

    /// <summary>
    /// Get compression ratio (compressed size / original size)
    /// </summary>
    public double GetCompressionRatio(string originalData, byte[] compressedData)
    {
        var originalSize = Encoding.UTF8.GetByteCount(originalData);
        return (double)compressedData.Length / originalSize;
    }
}
