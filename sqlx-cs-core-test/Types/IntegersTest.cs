using JetBrains.Annotations;
using NSubstitute;
using Sqlx.Core.Column;
using Sqlx.Core.Exceptions;

namespace Sqlx.Core.Types;

[TestSubject(typeof(Integers))]
public class IntegersTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(2561651)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void ValidateInt_Should_ReturnValue_WhenInt(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        Assert.Equal((int)value, Integers.ValidateInt(value, columnMetadata));
    }
    
    [Theory]
    [InlineData(int.MinValue - 1L)]
    [InlineData(int.MaxValue + 1L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void ValidateInt_Should_Thrown_WhenOutsideOfInt(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        var e = Assert.Throws<ColumnDecodeException>(() => Integers.ValidateInt(value, columnMetadata));
        Assert.Contains("Value is outside of valid int", e.Message);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(25616)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    public void ValidateShort_Should_ReturnValueWhenShort(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        Assert.Equal((short)value, Integers.ValidateShort(value, columnMetadata));
    }
    
    [Theory]
    [InlineData(short.MinValue - 1L)]
    [InlineData(short.MaxValue + 1L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void ValidateShort_Should_Thrown_WhenOutsideOfShort(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        var e = Assert.Throws<ColumnDecodeException>(() => Integers.ValidateShort(value, columnMetadata));
        Assert.Contains("Value is outside of valid short", e.Message);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(125)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    public void ValidateByte_Should_ReturnValueWhenByte(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        Assert.Equal((byte)value, Integers.ValidateByte(value, columnMetadata));
    }
    
    [Theory]
    [InlineData(byte.MinValue - 1L)]
    [InlineData(byte.MaxValue + 1L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    public void ValidateByte_Should_Thrown_WhenOutsideOfByte(long value)
    {
        var columnMetadata = Substitute.For<IColumnMetadata>();
        columnMetadata.DataType.Returns(0u);
        columnMetadata.FieldName.Returns("field");
        
        var e = Assert.Throws<ColumnDecodeException>(() => Integers.ValidateByte(value, columnMetadata));
        Assert.Contains("Value is outside of valid byte", e.Message);
    }
}
