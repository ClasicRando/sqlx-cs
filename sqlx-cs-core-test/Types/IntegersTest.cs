using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

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
    public async Task ValidateInt_Should_ReturnValue_WhenInt(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");

        var actual = Integers.ValidateInt(value, columnMetadata);
        await Assert.That(actual).IsEqualTo((int)value);
    }
    
    [Test]
    [Arguments(int.MinValue - 1L)]
    [Arguments(int.MaxValue + 1L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    public async Task ValidateInt_Should_Thrown_WhenOutsideOfInt(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        var e = Assert.Throws<ColumnDecodeException>(() => Integers.ValidateInt(value, columnMetadata));
        await Assert.That(e.Message).Contains("Value is outside of valid int");
    }
    
    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(1)]
    [Arguments(25616)]
    [Arguments(short.MinValue)]
    [Arguments(short.MaxValue)]
    public async Task ValidateShort_Should_ReturnValueWhenShort(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");

        var actual = Integers.ValidateShort(value, columnMetadata);
        await Assert.That(actual).IsEqualTo((short)value);
    }
    
    [Test]
    [Arguments(short.MinValue - 1L)]
    [Arguments(short.MaxValue + 1L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    public async Task ValidateShort_Should_Thrown_WhenOutsideOfShort(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        var e = Assert.Throws<ColumnDecodeException>(() => Integers.ValidateShort(value, columnMetadata));
        await Assert.That(e.Message).Contains("Value is outside of valid short");
    }
    
    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(125)]
    [Arguments(byte.MinValue)]
    [Arguments(byte.MaxValue)]
    public async Task ValidateByte_Should_ReturnValueWhenByte(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");

        var actual = Integers.ValidateByte(value, columnMetadata);
        await Assert.That(actual).IsEqualTo((byte)value);
    }
    
    [Test]
    [Arguments(byte.MinValue - 1L)]
    [Arguments(byte.MaxValue + 1L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    public async Task ValidateByte_Should_Thrown_WhenOutsideOfByte(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        var e = Assert.Throws<ColumnDecodeException>(() => Integers.ValidateByte(value, columnMetadata));
        await Assert.That(e.Message).Contains("Value is outside of valid byte");
    }
}