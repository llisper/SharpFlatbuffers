// automatically generated by the FlatBuffers compiler, do not modify

namespace Test
{

using System;
using FlatBuffers;

public sealed class TestMessage2 : Table {
  public static TestMessage2 GetRootAsTestMessage2(ByteBuffer _bb) { return GetRootAsTestMessage2(_bb, new TestMessage2()); }
  public static TestMessage2 GetRootAsTestMessage2(ByteBuffer _bb, TestMessage2 obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public TestMessage2 __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public string Name { get { int o = __offset(4); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetNameBytes() { return __vector_as_arraysegment(4); }
  public int Id { get { int o = __offset(6); return o != 0 ? bb.GetInt(o + bb_pos) : (int)0; } }

  public static Offset<TestMessage2> CreateTestMessage2(FlatBufferBuilder builder,
      StringOffset nameOffset = default(StringOffset),
      int id = 0) {
    builder.StartObject(2);
    TestMessage2.AddId(builder, id);
    TestMessage2.AddName(builder, nameOffset);
    return TestMessage2.EndTestMessage2(builder);
  }

  public static void StartTestMessage2(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(0, nameOffset.Value, 0); }
  public static void AddId(FlatBufferBuilder builder, int id) { builder.AddInt(1, id, 0); }
  public static Offset<TestMessage2> EndTestMessage2(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<TestMessage2>(o);
  }
  public static void FinishTestMessage2Buffer(FlatBufferBuilder builder, Offset<TestMessage2> offset) { builder.Finish(offset.Value); }
};


}
