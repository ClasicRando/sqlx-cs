using JetBrains.Annotations;

namespace Sqlx.Core.Types;

[TestSubject(typeof(Integers))]
public class IntegersTest
{
    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(1)]
    [Arguments(2561651)]
    [Arguments(int.MinValue)]
    [Arguments(int.MaxValue)]
    public async Task IsValidInt_Should_ReturnTrue_When_Int(long value)
    {
        var actual = Integers.IsValidInt(value);
        await Assert.That(actual).IsTrue();
    }
    
    [Test]
    [Arguments(int.MinValue - 1L)]
    [Arguments(int.MaxValue + 1L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    public async Task IsValidInt_Should_ReturnFalse_When_OutsideOfInt(long value)
    {
        var actual = Integers.IsValidInt(value);
        await Assert.That(actual).IsFalse();
    }
    
    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(1)]
    [Arguments(25616)]
    [Arguments(short.MinValue)]
    [Arguments(short.MaxValue)]
    public async Task IsValidShort_Should_ReturnTrue_When_Short(long value)
    {
        var actual = Integers.IsValidShort(value);
        await Assert.That(actual).IsTrue();
    }
    
    [Test]
    [Arguments(short.MinValue - 1L)]
    [Arguments(short.MaxValue + 1L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    public async Task IsValidShort_Should_ReturnFalse_When_OutsideOfShort(long value)
    {
        var actual = Integers.IsValidShort(value);
        await Assert.That(actual).IsFalse();
    }
    
    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(125)]
    [Arguments(byte.MinValue)]
    [Arguments(byte.MaxValue)]
    public async Task IsValidByte_Should_ReturnTrue_When_Byte(long value)
    {
        var actual = Integers.IsValidByte(value);
        await Assert.That(actual).IsTrue();
    }
    
    [Test]
    [Arguments(byte.MinValue - 1L)]
    [Arguments(byte.MaxValue + 1L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    public async Task IsValidByte_Should_ReturnFalse_when_OutsideOfByte(long value)
    {
        var actual = Integers.IsValidByte(value);
        await Assert.That(actual).IsFalse();
    }
}