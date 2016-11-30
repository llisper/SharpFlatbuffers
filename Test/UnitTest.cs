using System;
using System.Diagnostics;
using System.Collections.Generic;
using FlatBuffers;
using Test;

public class UnitTest
{
    public static void Main()
    {
        InitializeTest();
        InstancePoolTest();
        OffsetArrayPoolTest();
        SerializeTest();
        ByteBufferPoolTest();
        Console.WriteLine("All Done");
    }

    static void InitializeTest()
    {
        FlatBuffersInitializer.Initialize(typeof(PhoneNumber).Assembly);

        var insts = InstancePool.sInstances;
        Debug.Assert(insts.Count == 5);
        Debug.Assert(insts.ContainsKey(typeof(PhoneNumber)));
        Debug.Assert(insts.ContainsKey(typeof(Person)));
        Debug.Assert(insts.ContainsKey(typeof(AddressBook)));

        var arrays = OffsetArrayPool.sArrays;
        Debug.Assert(arrays.Count == 2);
        Debug.Assert(arrays.ContainsKey(typeof(PhoneNumber)));
        Debug.Assert(arrays.ContainsKey(typeof(Person)));
        var pnarr = arrays[typeof(PhoneNumber)];
        Debug.Assert(pnarr.GetType() == typeof(List<OffsetArrayPool.Array<PhoneNumber>>));
        Debug.Assert(pnarr.Count == (OffsetArrayPool.maxSizePOT - OffsetArrayPool.minSizePOT + 1) * OffsetArrayPool.initialArraySize);
        var parr = arrays[typeof(Person)];
        Debug.Assert(parr.GetType() == typeof(List<OffsetArrayPool.Array<Person>>));
        Debug.Assert(parr.Count == (OffsetArrayPool.maxSizePOT - OffsetArrayPool.minSizePOT + 1) * OffsetArrayPool.initialArraySize);
        Console.WriteLine("Initialize Done");
    }

    static void InstancePoolTest()
    {
        Debug.Assert(InstancePool.Get<PhoneNumber>() != null);
        Debug.Assert(InstancePool.Get<Person>() != null);
        Debug.Assert(InstancePool.Get<AddressBook>() != null);
        Debug.Assert(InstancePool.Get<TestMessage>() != null);
        Debug.Assert(InstancePool.Get<TestMessage2>() != null);
        Debug.Assert(InstancePool.sInstances.Count == 5);
        Console.WriteLine("Instance Pool Checked");
    }

    static void OffsetArrayPoolTest()
    {
        var array = OffsetArrayPool.sArrays[typeof(PhoneNumber)];
        int size = (OffsetArrayPool.maxSizePOT - OffsetArrayPool.minSizePOT + 1) * OffsetArrayPool.initialArraySize;
        Debug.Assert(array.Count == size);

        List<OffsetArrayPool.Array<PhoneNumber>> oalist = new List<OffsetArrayPool.Array<PhoneNumber>>();

        OffsetArrayPool.Array<PhoneNumber> oa = OffsetArrayPool.Alloc<PhoneNumber>(1);
        oalist.Add(oa);
        Debug.Assert(oa.length == (1 << OffsetArrayPool.minSizePOT));
        Debug.Assert(array.Count == size - oalist.Count);

        oa = OffsetArrayPool.Alloc<PhoneNumber>(1 << OffsetArrayPool.minSizePOT);
        oalist.Add(oa);
        Debug.Assert(oa.length == (1 << OffsetArrayPool.minSizePOT));
        Debug.Assert(array.Count == size - oalist.Count);

        oa = OffsetArrayPool.Alloc<PhoneNumber>((1 << OffsetArrayPool.minSizePOT) + 1);
        oalist.Add(oa);
        Debug.Assert(oa.length == (1 << (OffsetArrayPool.minSizePOT + 1)));
        Debug.Assert(array.Count == size - oalist.Count);

        oa = OffsetArrayPool.Alloc<PhoneNumber>(1);
        oalist.Add(oa);
        Debug.Assert(oa.length == (1 << OffsetArrayPool.minSizePOT));
        Debug.Assert(array.Count == size - oalist.Count + OffsetArrayPool.initialArraySize);

        for (int i = 0; i < oalist.Count; ++i)
        {
            oa = oalist[i];
            OffsetArrayPool.Dealloc(ref oa);
            Debug.Assert(oa == null);
        }
        oalist.Clear();
        Debug.Assert(array.Count == size + OffsetArrayPool.initialArraySize);

        Console.WriteLine("Array Pool Checked");
    }

    static void SerializeTest()
    {
        OffsetArrayPool.sArrays.Clear();
        OffsetArrayPool.Add(typeof(PhoneNumber));
        OffsetArrayPool.Add(typeof(Person));

        FlatBufferBuilder fbb = new FlatBufferBuilder(8192);
        int size = (OffsetArrayPool.maxSizePOT - OffsetArrayPool.minSizePOT + 1) * OffsetArrayPool.initialArraySize;
        FB_Serialize(fbb);
        Debug.Assert(OffsetArrayPool.sArrays[typeof(PhoneNumber)].Count == size);
        Debug.Assert(OffsetArrayPool.sArrays[typeof(Person)].Count == size);
        FB_Deserialize(fbb.DataBuffer);

        Console.WriteLine("Serialize Checked");
    }

    const string personName = "llisperzhang";

    static VectorOffset SetVector<T>(FlatBufferBuilder builder, OffsetArrayPool.Array<T> array) where T : class
    {
        for (int i = array.position - 1; i >= 0; --i)
            builder.AddOffset(array.offsets[i].Value);
        return builder.EndVector();
    }

    static void FB_Serialize(FlatBufferBuilder fbb)
    {
        fbb.Clear();
        var phoneArray = OffsetArrayPool.Alloc<PhoneNumber>(10);
        var personArray = OffsetArrayPool.Alloc<Person>(10);
        for (int p = 0; p < 10; ++p)
        {
            phoneArray.Clear();
            for (int n = 0; n < 10; ++n)
            {
                StringOffset phoneNumberOffset = fbb.CreateString((p * 100 + n).ToString());
                PhoneNumber.StartPhoneNumber(fbb);
                PhoneNumber.AddNumber(fbb, phoneNumberOffset);
                PhoneNumber.AddType(fbb, (PhoneType)(n % 3));
                phoneArray.offsets[phoneArray.position++] = PhoneNumber.EndPhoneNumber(fbb);
            }

            StringOffset nameOffset = fbb.CreateString(personName + p);
            StringOffset emailOffset = fbb.CreateString(string.Format("{0}{1}@gmail.com", personName, p));
            Person.StartPhonesVector(fbb, phoneArray.position);
            VectorOffset phoneArrayOffset = Helpers.SetVector(fbb, phoneArray);

            Person.StartPerson(fbb);
            Person.AddName(fbb, nameOffset);
            Person.AddId(fbb, p);
            Person.AddEmail(fbb, emailOffset);
            Person.AddPhones(fbb, phoneArrayOffset);
            personArray.offsets[personArray.position++] = Person.EndPerson(fbb);
        }

        AddressBook.StartPeopleVector(fbb, personArray.position);
        VectorOffset peopleArrayOffset = Helpers.SetVector(fbb, personArray);

        AddressBook.StartAddressBook(fbb);
        AddressBook.AddPeople(fbb, peopleArrayOffset);
        Offset<AddressBook> offset = AddressBook.EndAddressBook(fbb);
        fbb.Finish(offset.Value);

        OffsetArrayPool.Dealloc(ref phoneArray);
        OffsetArrayPool.Dealloc(ref personArray);
    }

    static void FB_Deserialize(ByteBuffer byteBuffer)
    {
        AddressBook addressBookInst = InstancePool.Get<AddressBook>();
        Person personInst = InstancePool.Get<Person>();
        PhoneNumber phoneNumberInst = InstancePool.Get<PhoneNumber>();
        
        AddressBook addressBook = AddressBook.GetRootAsAddressBook(byteBuffer, addressBookInst);
        int plen = addressBook.PeopleLength;
        for (int p = 0; p < plen; ++p)
        {
            Person person = addressBook.GetPeople(personInst, p);
            Debug.Assert(0 == string.Compare(person.Name, personName + p));
            Debug.Assert(person.Id == p);
            Debug.Assert(0 == string.Compare(person.Email, string.Format("{0}{1}@gmail.com", personName, p)));
            int len = person.PhonesLength;
            for (int n = 0; n < len; ++n)
            {
                PhoneNumber pn = person.GetPhones(phoneNumberInst, n);
                Debug.Assert(0 == string.Compare(pn.Number, (p * 100 + n).ToString()));
                Debug.Assert(pn.Type == (PhoneType)(n % 3));
            }
        }
    }

    static void ByteBufferPoolTest()
    {
        for (int i = 0; i < ByteBufferPool.sPool.Length; ++i)
        {
            var p = ByteBufferPool.sPool[i];
            Debug.Assert(p.Count == ByteBufferPool.initialArraySize);
            for (int j = 0; j < p.Count; ++j)
                Debug.Assert(p[j].Data.Length == 1 << (i + ByteBufferPool.minSizePOT));
        }

        int min = 1 << ByteBufferPool.minSizePOT;
        List<ByteBuffer> buffers = new List<ByteBuffer>();
        buffers.Add(ByteBufferPool.Alloc(0));
        buffers.Add(ByteBufferPool.Alloc(1));
        buffers.Add(ByteBufferPool.Alloc(min - 1));
        buffers.Add(ByteBufferPool.Alloc(min));
        buffers.Add(ByteBufferPool.Alloc(min + 1));

        var pool_64 = ByteBufferPool.sPool[ByteBufferPool.sIndexLookup[1 << ByteBufferPool.minSizePOT]];
        Debug.Assert(ByteBufferPool.AvailableSlots(pool_64) == 0);
        var pool_128 = ByteBufferPool.sPool[ByteBufferPool.sIndexLookup[1 << (ByteBufferPool.minSizePOT + 1)]];
        Debug.Assert(ByteBufferPool.AvailableSlots(pool_128) == ByteBufferPool.initialArraySize - 1);

        buffers.Add(ByteBufferPool.Alloc(min));
        Debug.Assert(ByteBufferPool.AvailableSlots(pool_64) == ByteBufferPool.initialArraySize - 1);

        for (int i = 0; i < buffers.Count; ++i)
        {
            ByteBuffer bb = buffers[i];
            ByteBufferPool.Dealloc(ref bb);
            Debug.Assert(null == bb);
        }
        Debug.Assert(ByteBufferPool.AvailableSlots(pool_64) == ByteBufferPool.initialArraySize * 2);
        Debug.Assert(ByteBufferPool.AvailableSlots(pool_128) == ByteBufferPool.initialArraySize);

        try
        {
            ByteBufferPool.Alloc((1 << ByteBufferPool.maxSizePOT) + 1);
            Debug.Assert(false);
        } catch (ArgumentOutOfRangeException) { }

        ByteBuffer wrongsize = new ByteBuffer(new byte[123]);
        ByteBufferPool.Dealloc(ref wrongsize);
        Debug.Assert(null != wrongsize);

        ByteBuffer exceed = new ByteBuffer(new byte[1 << ByteBufferPool.minSizePOT]);
        ByteBufferPool.Dealloc(ref exceed);
        Debug.Assert(null != exceed);
        
        Console.WriteLine("ByteBuffer Pool Checked");
    }
}
