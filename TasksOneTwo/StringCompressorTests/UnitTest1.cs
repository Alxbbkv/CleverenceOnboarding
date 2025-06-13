using Cleverence.StringCompressor;
using NuGet.Frameworks;

namespace StringCompressorTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        _stringCompressor = new StringCompressor();
    }

    private StringCompressor _stringCompressor;
    
    [Test]
    public void TestCompressEmptyString()
    {
        Assert.That(_stringCompressor.Compress(""), Is.EqualTo(string.Empty));
    }
    
    [Test]
    public void TestCompressOneValidChar()
    {
        Assert.That(_stringCompressor.Compress("a"), Is.EqualTo("a"));
    }
    
    [Test]
    public void TestCompressTwoDifferentValidChars()
    {
        Assert.That(_stringCompressor.Compress("ab"), Is.EqualTo("ab"));
    }
    
    [Test]
    public void TestCompressTwoSameValidChars()
    {
        Assert.That(_stringCompressor.Compress("aa"), Is.EqualTo("a2"));
    }
    
    [Test]
    public void TestCompressTypicalValidString()
    {
        Assert.That(_stringCompressor.Compress("abbcccdddd"), Is.EqualTo("ab2c3d4"));
    }
    
    [Test]
    public void TestCompressInvalidString()
    {
        Assert.Throws<ArgumentException>(()=>_stringCompressor.Compress("a1b"));
    }

    [Test]
    public void TestDecompressInvalidString()
    {
        Assert.Throws<ArgumentException>(()=>_stringCompressor.Decompress("aSb"));
    }
    
    [Test]
    public void TestDecompressInvalidStringWithSameChars()
    {
        Assert.Throws<ArgumentException>(()=>_stringCompressor.Decompress("abb"));
    }
    
    [Test]
    public void TestDecompressEmptyString()
    {
        Assert.That(_stringCompressor.Decompress(""), Is.EqualTo(string.Empty));
    }
    
    [Test]
    public void TestDecompressTypicalString()
    {
        Assert.That(_stringCompressor.Decompress("ab2c3d4"), Is.EqualTo("abbcccdddd"));
    }
    
    [Test]
    public void TestDecompressChar()
    {
        Assert.That(_stringCompressor.Decompress("a"), Is.EqualTo("a"));
    }
}