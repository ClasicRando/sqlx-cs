using System.Diagnostics.CodeAnalysis;

namespace Sqlx.Postgres.Type;

[SuppressMessage(
    "Naming",
    "CA1720:Identifier contains type name",
    Justification =
        "Type names within this as constant properties must be named that way to reflect the postgres ")]
public sealed class PgTypeInfo : IEquatable<PgTypeInfo>
{
    private const int BoolOid = 16;
    private const int BoolArrayOid = 1000;
    private const int ByteaOid = 17;
    private const int ByteaArrayOid = 1001;
    private const int CharOid = 18;
    private const int CharArrayOid = 1002;
    private const int NameOid = 19;
    private const int NameArrayOid = 1003;
    private const int Int2Oid = 21;
    private const int Int2ArrayOid = 1005;
    private const int Int4Oid = 23;
    private const int Int4ArrayOid = 1007;
    private const int Int8Oid = 20;
    private const int Int8ArrayOid = 1016;
    private const int TextOid = 25;
    private const int TextArrayOid = 1009;
    private const int OidOid = 26;
    private const int OidArrayOid = 1028;
    private const int JsonOid = 114;
    private const int JsonArrayOid = 199;
    private const int PointOid = 600;
    private const int PointArrayOid = 1017;
    private const int LsegOid = 601;
    private const int LsegArrayOid = 1018;
    private const int PathOid = 602;
    private const int PathArrayOid = 1019;
    private const int BoxOid = 603;
    private const int BoxArrayOid = 1020;
    private const int PolygonOid = 604;
    private const int PolygonArrayOid = 1027;
    private const int LineOid = 628;
    private const int LineArrayOid = 629;
    private const int CidrOid = 650;
    private const int CidrArrayOid = 651;
    private const int Float4Oid = 700;
    private const int Float4ArrayOid = 1021;
    private const int Float8Oid = 701;
    private const int Float8ArrayOid = 1022;
    private const int UnknownOid = 705;
    private const int CircleOid = 718;
    private const int CircleArrayOid = 719;
    private const int Macaddr8Oid = 774;
    private const int Macaddr8ArrayOid = 775;
    private const int MacaddrOid = 829;
    private const int MacaddrArrayOid = 1040;
    private const int InetOid = 869;
    private const int InetArrayOid = 1041;
    private const int BpcharOid = 1042;
    private const int BpcharArrayOid = 1014;
    private const int VarcharOid = 1043;
    private const int VarcharArrayOid = 1015;
    private const int DateOid = 1082;
    private const int DateArrayOid = 1182;
    private const int TimeOid = 1083;
    private const int TimeArrayOid = 1183;
    private const int TimestampOid = 1114;
    private const int TimestampArrayOid = 1115;
    private const int TimestamptzOid = 1184;
    private const int TimestamptzArrayOid = 1185;
    private const int IntervalOid = 1186;
    private const int IntervalArrayOid = 1187;
    private const int TimetzOid = 1266;
    private const int TimetzArrayOid = 1270;
    private const int BitOid = 1560;
    private const int BitArrayOid = 1561;
    private const int VarbitOid = 1562;
    private const int VarbitArrayOid = 1563;
    private const int NumericOid = 1700;
    private const int NumericArrayOid = 1231;
    private const int RecordOid = 2249;
    private const int RecordArrayOid = 2287;
    private const int UuidOid = 2950;
    private const int UuidArrayOid = 2951;
    private const int JsonbOid = 3802;
    private const int JsonbArrayOid = 3807;
    private const int Int4RangeOid = 3904;
    private const int Int4RangeArrayOid = 3905;
    private const int NumrangeOid = 3906;
    private const int NumrangeArrayOid = 3907;
    private const int TsrangeOid = 3908;
    private const int TsrangeArrayOid = 3909;
    private const int TstzrangeOid = 3910;
    private const int TstzrangeArrayOid = 3911;
    private const int DaterangeOid = 3912;
    private const int DaterangeArrayOid = 3913;
    private const int Int8RangeOid = 3926;
    private const int Int8RangeArrayOid = 3927;
    private const int JsonpathOid = 4072;
    private const int JsonpathArrayOid = 4073;
    private const int MoneyOid = 790;
    private const int MoneyArrayOid = 791;
    private const int XmlOid = 142;
    private const int XmlArrayOid = 143;
    private const int VoidOid = 2278;
    private const int UnspecifiedOid = 0;

    public PgOid TypeOid { get; }

    internal IPgTypeKind TypeKind { get; }

    internal PgTypeInfo(uint typeOid, IPgTypeKind typeKind) : this(new PgOid(typeOid), typeKind)
    {
    }

    private PgTypeInfo(PgOid typeOid, IPgTypeKind typeKind)
    {
        TypeOid = typeOid;
        TypeKind = typeKind;
    }

    public static readonly PgTypeInfo Bool = new(BoolOid, SimpleType.Instance);

    public static readonly PgTypeInfo BoolArray = new(
        BoolArrayOid,
        new ArrayType { ElementType = Bool });

    public static readonly PgTypeInfo Bytea = new(ByteaOid, SimpleType.Instance);

    public static readonly PgTypeInfo ByteaArray = new(
        ByteaArrayOid,
        new ArrayType { ElementType = Bytea });

    public static readonly PgTypeInfo Char = new(CharOid, SimpleType.Instance);

    public static readonly PgTypeInfo CharArray = new(
        CharArrayOid,
        new ArrayType { ElementType = Char });

    public static readonly PgTypeInfo Name = new(NameOid, SimpleType.Instance);

    public static readonly PgTypeInfo NameArray = new(
        NameArrayOid,
        new ArrayType { ElementType = Name });

    public static readonly PgTypeInfo Int2 = new(Int2Oid, SimpleType.Instance);

    public static readonly PgTypeInfo Int2Array = new(
        Int2ArrayOid,
        new ArrayType { ElementType = Int2 });

    public static readonly PgTypeInfo Int4 = new(Int4Oid, SimpleType.Instance);

    public static readonly PgTypeInfo Int4Array = new(
        Int4ArrayOid,
        new ArrayType { ElementType = Int4 });

    public static readonly PgTypeInfo Int8 = new(Int8Oid, SimpleType.Instance);

    public static readonly PgTypeInfo Int8Array = new(
        Int8ArrayOid,
        new ArrayType { ElementType = Int8 });

    public static readonly PgTypeInfo Text = new(TextOid, SimpleType.Instance);

    public static readonly PgTypeInfo TextArray = new(
        TextArrayOid,
        new ArrayType { ElementType = Text });

    public static readonly PgTypeInfo Oid = new(OidOid, SimpleType.Instance);

    public static readonly PgTypeInfo OidArray = new(
        OidArrayOid,
        new ArrayType { ElementType = Oid });

    public static readonly PgTypeInfo Json = new(JsonOid, SimpleType.Instance);

    public static readonly PgTypeInfo JsonArray = new(
        JsonArrayOid,
        new ArrayType { ElementType = Json });

    public static readonly PgTypeInfo Point = new(PointOid, SimpleType.Instance);

    public static readonly PgTypeInfo PointArray = new(
        PointArrayOid,
        new ArrayType { ElementType = Point });

    public static readonly PgTypeInfo Lseg = new(LsegOid, SimpleType.Instance);

    public static readonly PgTypeInfo LsegArray = new(
        LsegArrayOid,
        new ArrayType { ElementType = Lseg });

    public static readonly PgTypeInfo Path = new(PathOid, SimpleType.Instance);

    public static readonly PgTypeInfo PathArray = new(
        PathArrayOid,
        new ArrayType { ElementType = Path });

    public static readonly PgTypeInfo Box = new(BoxOid, SimpleType.Instance);

    public static readonly PgTypeInfo BoxArray = new(
        BoxArrayOid,
        new ArrayType { ElementType = Box });

    public static readonly PgTypeInfo Polygon = new(PolygonOid, SimpleType.Instance);

    public static readonly PgTypeInfo PolygonArray = new(
        PolygonArrayOid,
        new ArrayType { ElementType = Polygon });

    public static readonly PgTypeInfo Line = new(LineOid, SimpleType.Instance);

    public static readonly PgTypeInfo LineArray = new(
        LineArrayOid,
        new ArrayType { ElementType = Line });

    public static readonly PgTypeInfo Cidr = new(CidrOid, SimpleType.Instance);

    public static readonly PgTypeInfo CidrArray = new(
        CidrArrayOid,
        new ArrayType { ElementType = Cidr });

    public static readonly PgTypeInfo Float4 = new(Float4Oid, SimpleType.Instance);

    public static readonly PgTypeInfo Float4Array = new(
        Float4ArrayOid,
        new ArrayType { ElementType = Float4 });

    public static readonly PgTypeInfo Float8 = new(Float8Oid, SimpleType.Instance);

    public static readonly PgTypeInfo Float8Array = new(
        Float8ArrayOid,
        new ArrayType { ElementType = Float8 });

    public static readonly PgTypeInfo Unknown = new(UnknownOid, SimpleType.Instance);
    public static readonly PgTypeInfo Circle = new(CircleOid, SimpleType.Instance);

    public static readonly PgTypeInfo CircleArray = new(
        CircleArrayOid,
        new ArrayType { ElementType = Circle });

    public static readonly PgTypeInfo Macaddr8 = new(Macaddr8Oid, SimpleType.Instance);

    public static readonly PgTypeInfo Macaddr8Array = new(
        Macaddr8ArrayOid,
        new ArrayType { ElementType = Macaddr8 });

    public static readonly PgTypeInfo Macaddr = new(MacaddrOid, SimpleType.Instance);

    public static readonly PgTypeInfo MacaddrArray = new(
        MacaddrArrayOid,
        new ArrayType { ElementType = Macaddr });

    public static readonly PgTypeInfo Inet = new(InetOid, SimpleType.Instance);

    public static readonly PgTypeInfo InetArray = new(
        InetArrayOid,
        new ArrayType { ElementType = Inet });

    public static readonly PgTypeInfo Bpchar = new(BpcharOid, SimpleType.Instance);

    public static readonly PgTypeInfo BpcharArray = new(
        BpcharArrayOid,
        new ArrayType { ElementType = Bpchar });

    public static readonly PgTypeInfo Varchar = new(VarcharOid, SimpleType.Instance);

    public static readonly PgTypeInfo VarcharArray = new(
        VarcharArrayOid,
        new ArrayType { ElementType = Varchar });

    public static readonly PgTypeInfo Date = new(DateOid, SimpleType.Instance);

    public static readonly PgTypeInfo DateArray = new(
        DateArrayOid,
        new ArrayType { ElementType = Date });

    public static readonly PgTypeInfo Time = new(TimeOid, SimpleType.Instance);

    public static readonly PgTypeInfo TimeArray = new(
        TimeArrayOid,
        new ArrayType { ElementType = Time });

    public static readonly PgTypeInfo Timestamp = new(TimestampOid, SimpleType.Instance);

    public static readonly PgTypeInfo TimestampArray = new(
        TimestampArrayOid,
        new ArrayType { ElementType = Timestamp });

    public static readonly PgTypeInfo Timestamptz = new(TimestamptzOid, SimpleType.Instance);

    public static readonly PgTypeInfo TimestamptzArray = new(
        TimestamptzArrayOid,
        new ArrayType { ElementType = Timestamptz });

    public static readonly PgTypeInfo Interval = new(IntervalOid, SimpleType.Instance);

    public static readonly PgTypeInfo IntervalArray = new(
        IntervalArrayOid,
        new ArrayType { ElementType = Interval });

    public static readonly PgTypeInfo Timetz = new(TimetzOid, SimpleType.Instance);

    public static readonly PgTypeInfo TimetzArray = new(
        TimetzArrayOid,
        new ArrayType { ElementType = Timetz });

    public static readonly PgTypeInfo Bit = new(BitOid, SimpleType.Instance);

    public static readonly PgTypeInfo BitArray = new(
        BitArrayOid,
        new ArrayType { ElementType = Bit });

    public static readonly PgTypeInfo Varbit = new(VarbitOid, SimpleType.Instance);

    public static readonly PgTypeInfo VarbitArray = new(
        VarbitArrayOid,
        new ArrayType { ElementType = Varbit });

    public static readonly PgTypeInfo Numeric = new(NumericOid, SimpleType.Instance);

    public static readonly PgTypeInfo NumericArray = new(
        NumericArrayOid,
        new ArrayType { ElementType = Numeric });

    public static readonly PgTypeInfo Record = new(RecordOid, SimpleType.Instance);

    public static readonly PgTypeInfo RecordArray = new(
        RecordArrayOid,
        new ArrayType { ElementType = Record });

    public static readonly PgTypeInfo Uuid = new(UuidOid, SimpleType.Instance);

    public static readonly PgTypeInfo UuidArray = new(
        UuidArrayOid,
        new ArrayType { ElementType = Uuid });

    public static readonly PgTypeInfo Jsonb = new(JsonbOid, SimpleType.Instance);

    public static readonly PgTypeInfo JsonbArray = new(
        JsonbArrayOid,
        new ArrayType { ElementType = Jsonb });

    public static readonly PgTypeInfo Int4Range = new(
        Int4RangeOid,
        new RangeType { RangeElement = Int4 });

    public static readonly PgTypeInfo Int4RangeArray = new(
        Int4RangeArrayOid,
        new ArrayType { ElementType = Int4Range });

    public static readonly PgTypeInfo Numrange = new(
        NumrangeOid,
        new RangeType { RangeElement = Numeric });

    public static readonly PgTypeInfo NumrangeArray = new(
        NumrangeArrayOid,
        new ArrayType { ElementType = Numrange });

    public static readonly PgTypeInfo Tsrange = new(
        TsrangeOid,
        new RangeType { RangeElement = Timestamp });

    public static readonly PgTypeInfo TsrangeArray = new(
        TsrangeArrayOid,
        new ArrayType { ElementType = Tsrange });

    public static readonly PgTypeInfo Tstzrange = new(
        TstzrangeOid,
        new RangeType { RangeElement = Timestamptz });

    public static readonly PgTypeInfo TstzrangeArray = new(
        TstzrangeArrayOid,
        new ArrayType { ElementType = Tstzrange });

    public static readonly PgTypeInfo Daterange = new(
        DaterangeOid,
        new RangeType { RangeElement = Date });

    public static readonly PgTypeInfo DaterangeArray = new(
        DaterangeArrayOid,
        new ArrayType { ElementType = Daterange });

    public static readonly PgTypeInfo Int8Range = new(
        Int8RangeOid,
        new RangeType { RangeElement = Int8 });

    public static readonly PgTypeInfo Int8RangeArray = new(
        Int8RangeArrayOid,
        new ArrayType { ElementType = Int8Range });

    public static readonly PgTypeInfo Jsonpath = new(JsonpathOid, SimpleType.Instance);

    public static readonly PgTypeInfo JsonpathArray = new(
        JsonpathArrayOid,
        new ArrayType { ElementType = Jsonpath });

    public static readonly PgTypeInfo Money = new(MoneyOid, SimpleType.Instance);

    public static readonly PgTypeInfo MoneyArray = new(
        MoneyArrayOid,
        new ArrayType { ElementType = Money });

    public static readonly PgTypeInfo Xml = new(XmlOid, SimpleType.Instance);

    public static readonly PgTypeInfo XmlArray = new(
        XmlArrayOid,
        new ArrayType { ElementType = Xml });

    public static readonly PgTypeInfo Void = new(VoidOid, new PseudoType());
    public static readonly PgTypeInfo Unspecified = new(UnspecifiedOid, SimpleType.Instance);

    public bool Equals(PgTypeInfo? other)
    {
        return other is not null && TypeOid == other.TypeOid;
    }

    public override bool Equals(object? obj)
    {
        return obj is PgTypeInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TypeOid);
    }

    public static bool operator ==(PgTypeInfo left, PgTypeInfo right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Equals(right);
    }

    public static bool operator !=(PgTypeInfo left, PgTypeInfo right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return !left.Equals(right);
    }

    public static PgTypeInfo FromOid(PgOid oid)
    {
        return oid.Inner switch
        {
            BoolOid => Bool,
            BoolArrayOid => BoolArray,
            ByteaOid => Bytea,
            ByteaArrayOid => ByteaArray,
            CharOid => Char,
            CharArrayOid => CharArray,
            NameOid => Name,
            NameArrayOid => NameArray,
            Int2Oid => Int2,
            Int2ArrayOid => Int2Array,
            Int4Oid => Int4,
            Int4ArrayOid => Int4Array,
            Int8Oid => Int8,
            Int8ArrayOid => Int8Array,
            TextOid => Text,
            TextArrayOid => TextArray,
            OidOid => Oid,
            OidArrayOid => OidArray,
            JsonOid => Json,
            JsonArrayOid => JsonArray,
            PointOid => Point,
            PointArrayOid => PointArray,
            LsegOid => Lseg,
            LsegArrayOid => LsegArray,
            PathOid => Path,
            PathArrayOid => PathArray,
            BoxOid => Box,
            BoxArrayOid => BoxArray,
            PolygonOid => Polygon,
            PolygonArrayOid => PolygonArray,
            LineOid => Line,
            LineArrayOid => LineArray,
            CidrOid => Cidr,
            CidrArrayOid => CidrArray,
            Float4Oid => Float4,
            Float4ArrayOid => Float4Array,
            Float8Oid => Float8,
            Float8ArrayOid => Float8Array,
            UnknownOid => Unknown,
            CircleOid => Circle,
            CircleArrayOid => CircleArray,
            Macaddr8Oid => Macaddr8,
            Macaddr8ArrayOid => Macaddr8Array,
            MacaddrOid => Macaddr,
            MacaddrArrayOid => MacaddrArray,
            InetOid => Inet,
            InetArrayOid => InetArray,
            BpcharOid => Bpchar,
            BpcharArrayOid => BpcharArray,
            VarcharOid => Varchar,
            VarcharArrayOid => VarcharArray,
            DateOid => Date,
            DateArrayOid => DateArray,
            TimeOid => Time,
            TimeArrayOid => TimeArray,
            TimestampOid => Timestamp,
            TimestampArrayOid => TimestampArray,
            TimestamptzOid => Timestamptz,
            TimestamptzArrayOid => TimestamptzArray,
            IntervalOid => Interval,
            IntervalArrayOid => IntervalArray,
            TimetzOid => Timetz,
            TimetzArrayOid => TimetzArray,
            BitOid => Bit,
            BitArrayOid => BitArray,
            VarbitOid => Varbit,
            VarbitArrayOid => VarbitArray,
            NumericOid => Numeric,
            NumericArrayOid => NumericArray,
            RecordOid => Record,
            RecordArrayOid => RecordArray,
            UuidOid => Uuid,
            UuidArrayOid => UuidArray,
            JsonbOid => Jsonb,
            JsonbArrayOid => JsonbArray,
            Int4RangeOid => Int4Range,
            Int4RangeArrayOid => Int4RangeArray,
            NumrangeOid => Numrange,
            NumrangeArrayOid => NumrangeArray,
            TsrangeOid => Tsrange,
            TsrangeArrayOid => TsrangeArray,
            TstzrangeOid => Tstzrange,
            TstzrangeArrayOid => TstzrangeArray,
            DaterangeOid => Daterange,
            DaterangeArrayOid => DaterangeArray,
            Int8RangeOid => Int8Range,
            Int8RangeArrayOid => Int8RangeArray,
            JsonpathOid => Jsonpath,
            JsonpathArrayOid => JsonpathArray,
            MoneyOid => Money,
            MoneyArrayOid => MoneyArray,
            XmlOid => Xml,
            XmlArrayOid => XmlArray,
            VoidOid => Void,
            UnspecifiedOid => Unspecified,
            _ => new PgTypeInfo(oid, new UnknownType()),
        };
    }
}
